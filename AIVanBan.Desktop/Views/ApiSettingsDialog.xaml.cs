using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class ApiSettingsDialog : Window
{
    private int _headerClickCount;
    private DateTime _lastHeaderClick = DateTime.MinValue;
    private bool _devModeActive;
    private DispatcherTimer? _countdownTimer;
    private DocumentService? _documentService;

    public ApiSettingsDialog()
    {
        InitializeComponent();
        LoadSettings();
        LoadOrganizationTab();
    }

    public ApiSettingsDialog(DocumentService documentService) : this()
    {
        _documentService = documentService;
        LoadOrganizationTab();
    }

    /// <summary>
    /// Click header 5 lần liên tục (trong 3 giây) để mở chế độ bảo trì (dev mode).
    /// </summary>
    private void Header_Click(object sender, MouseButtonEventArgs e)
    {
        var now = DateTime.Now;
        if ((now - _lastHeaderClick).TotalSeconds > 3)
            _headerClickCount = 0;

        _lastHeaderClick = now;
        _headerClickCount++;

        if (_headerClickCount >= 5 && !_devModeActive)
        {
            _devModeActive = true;
            grpModeSelector.Visibility = Visibility.Visible;
            grpGeminiDirect.Visibility = Visibility.Visible;
            UpdateVisibility();

            // Kiểm tra nếu đã có timestamp (đang trong phiên bảo trì cũ)
            var remaining = DevModePolicy.GetRemainingTime();
            if (remaining.HasValue)
            {
                StartCountdownTimer();
                var h = (int)remaining.Value.TotalHours;
                var m = remaining.Value.Minutes;
                MessageBox.Show($"🔧 Đã mở chế độ bảo trì.\n\n⏰ Còn lại: {h} giờ {m} phút\n(Tự động tắt sau {DevModePolicy.MaxHours} giờ kể từ khi kích hoạt)",
                    "Chế độ nâng cao", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"🔧 Đã mở chế độ bảo trì.\n\nChỉ dành cho kỹ thuật viên.\n⏰ Sẽ tự động tắt sau {DevModePolicy.MaxHours} giờ khi lưu.",
                    "Chế độ nâng cao", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void CloseDevMode_Click(object sender, RoutedEventArgs e)
    {
        _devModeActive = false;
        _headerClickCount = 0;
        _countdownTimer?.Stop();

        // Ẩn các group dev
        grpModeSelector.Visibility = Visibility.Collapsed;
        grpModeSelector.Header = "Chế độ kết nối (Nâng cao)";
        grpGeminiDirect.Visibility = Visibility.Collapsed;

        // Reset về VanBanPlus
        rbVanBanPlus.IsChecked = true;
        UpdateVisibility();
    }

    private void LoadSettings()
    {
        // Tự revert nếu dev mode quá hạn
        if (DevModePolicy.AutoRevertIfExpired())
        {
            MessageBox.Show($"⏰ Chế độ bảo trì đã tự động tắt sau {DevModePolicy.MaxHours} giờ.\nĐã chuyển về VanBanPlus.",
                "Tự động tắt bảo trì", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        var settings = AppSettingsService.Load();

        // Toggle AI
        tglAiEnabled.IsChecked = settings.AiEnabled;
        UpdateAiToggleVisual(settings.AiEnabled);

        // Chế độ API — mặc định VanBanPlus
        rbVanBanPlus.IsChecked = settings.UseVanBanPlusApi;
        rbGeminiDirect.IsChecked = !settings.UseVanBanPlusApi;

        // VanBanPlus
        txtApiUrl.Text = settings.VanBanPlusApiUrl;
        txtApiKey.Text = settings.VanBanPlusApiKey;

        // Gemini (chỉ hiện khi dev mode)
        txtGeminiKey.Text = settings.GeminiApiKey;

        // Nếu đang dùng Gemini trực tiếp → tự động bật dev mode để user thấy
        if (!settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.GeminiApiKey))
        {
            _devModeActive = true;
            grpModeSelector.Visibility = Visibility.Visible;
            grpGeminiDirect.Visibility = Visibility.Visible;
            StartCountdownTimer();
        }

        UpdateVisibility();
        UpdateCurrentStatus(settings);
    }

    /// <summary>Bắt đầu đếm ngược thời gian dev mode còn lại</summary>
    private void StartCountdownTimer()
    {
        _countdownTimer?.Stop();
        _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _countdownTimer.Tick += (s, e) =>
        {
            var remaining = DevModePolicy.GetRemainingTime();
            if (remaining == null || remaining <= TimeSpan.Zero)
            {
                // Hết hạn ngay trong dialog
                _countdownTimer?.Stop();
                DevModePolicy.AutoRevertIfExpired();
                _devModeActive = false;
                grpModeSelector.Visibility = Visibility.Collapsed;
                grpGeminiDirect.Visibility = Visibility.Collapsed;
                rbVanBanPlus.IsChecked = true;
                UpdateVisibility();
                LoadSettings(); // refresh UI
                MessageBox.Show($"⏰ Chế độ bảo trì đã hết hạn ({DevModePolicy.MaxHours} giờ).\nĐã tự động chuyển về VanBanPlus.",
                    "Hết hạn bảo trì", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            UpdateDevModeCountdown(remaining.Value);
        };
        _countdownTimer.Start();

        // Cập nhật ngay lần đầu
        var r = DevModePolicy.GetRemainingTime();
        if (r.HasValue) UpdateDevModeCountdown(r.Value);
    }

    private void UpdateDevModeCountdown(TimeSpan remaining)
    {
        var totalMinutes = (int)remaining.TotalMinutes;
        var seconds = remaining.Seconds;
        grpModeSelector.Header = $"Chế độ kết nối (Nâng cao) — ⏰ Còn {totalMinutes}p{seconds:D2}s";
    }

    private void ApiMode_Changed(object sender, RoutedEventArgs e)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (grpVanBanPlus == null || grpGeminiDirect == null) return;

        var aiEnabled = tglAiEnabled.IsChecked == true;
        var useVanBanPlus = rbVanBanPlus.IsChecked == true;

        // Nếu AI bị tắt → disable tất cả, không cần xét chế độ
        if (!aiEnabled)
        {
            grpVanBanPlus.IsEnabled = false;
            grpVanBanPlus.Opacity = 0.4;
            if (_devModeActive)
            {
                grpModeSelector.IsEnabled = false;
                grpModeSelector.Opacity = 0.4;
                grpGeminiDirect.IsEnabled = false;
                grpGeminiDirect.Opacity = 0.4;
            }
            return;
        }

        // AI đang bật → enable/disable theo chế độ kết nối
        grpVanBanPlus.IsEnabled = useVanBanPlus;
        grpVanBanPlus.Opacity = useVanBanPlus ? 1.0 : 0.4;

        if (_devModeActive)
        {
            grpModeSelector.IsEnabled = true;
            grpModeSelector.Opacity = 1.0;
            grpGeminiDirect.IsEnabled = !useVanBanPlus;
            grpGeminiDirect.Opacity = !useVanBanPlus ? 1.0 : 0.4;
        }
    }

    private void UpdateCurrentStatus(AppSettings settings)
    {
        if (settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.VanBanPlusApiKey))
        {
            var maskedKey = settings.VanBanPlusApiKey.Length > 10
                ? settings.VanBanPlusApiKey[..10] + "..."
                : settings.VanBanPlusApiKey;
            txtCurrentStatus.Text = $"✅ Đã kích hoạt\n" +
                                    $"🌐 Server: {settings.VanBanPlusApiUrl}\n" +
                                    $"🔑 Mã: {maskedKey}\n" +
                                    $"👤 {(string.IsNullOrEmpty(settings.UserEmail) ? "(chưa xác thực)" : settings.UserEmail)}";
        }
        else if (!settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.GeminiApiKey))
        {
            var remaining = DevModePolicy.GetRemainingTime();
            var timeStr = remaining.HasValue 
                ? $"\n⏰ Tự tắt sau: {(int)remaining.Value.TotalMinutes}p{remaining.Value.Seconds:D2}s"
                : "";
            txtCurrentStatus.Text = $"🔧 Chế độ bảo trì (kết nối trực tiếp){timeStr}\n" +
                                    $"⚠️ Không dùng cho sản phẩm chính thức";
        }
        else
        {
            txtCurrentStatus.Text = "⚠️ Chưa kích hoạt AI\n\nVui lòng nhập mã kích hoạt để sử dụng các tính năng AI.\n📞 Liên hệ Zalo: Thắng Phan — 0907136029";
        }
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        var apiUrl = txtApiUrl.Text.Trim().TrimEnd('/');
        var apiKey = txtApiKey.Text.Trim();

        if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
        {
            txtConnectionStatus.Text = "❌ Vui lòng nhập địa chỉ server và mã kích hoạt";
            txtConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        txtConnectionStatus.Text = "⏳ Đang kiểm tra...";
        txtConnectionStatus.Foreground = System.Windows.Media.Brushes.Gray;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);

            // Vercel bypass header
            var settings = AppSettingsService.Load();
            if (!string.IsNullOrEmpty(settings.VercelBypassToken))
                httpClient.DefaultRequestHeaders.Add("x-vercel-protection-bypass", settings.VercelBypassToken);

            // Test health endpoint
            var healthResponse = await httpClient.GetAsync($"{apiUrl}/api/health");
            if (!healthResponse.IsSuccessStatusCode)
            {
                txtConnectionStatus.Text = $"❌ Server không phản hồi (HTTP {(int)healthResponse.StatusCode})";
                txtConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            // Test auth/me endpoint
            var meResponse = await httpClient.GetAsync($"{apiUrl}/api/auth/me");
            if (meResponse.IsSuccessStatusCode)
            {
                var meResult = await meResponse.Content.ReadFromJsonAsync<MeResponse>();
                if (meResult?.Success == true && meResult.Data != null)
                {
                    txtConnectionStatus.Text = $"✅ Kết nối thành công!\n" +
                                               $"👤 {meResult.Data.FullName} ({meResult.Data.Email})\n" +
                                               $"📦 Gói: {meResult.Data.Plan}";
                    txtConnectionStatus.Foreground = System.Windows.Media.Brushes.Green;
                    return;
                }
            }

            txtConnectionStatus.Text = "✅ Server OK nhưng mã kích hoạt không hợp lệ hoặc hết hạn";
            txtConnectionStatus.Foreground = System.Windows.Media.Brushes.Orange;
        }
        catch (TaskCanceledException)
        {
            txtConnectionStatus.Text = "❌ Hết thời gian chờ (timeout). Kiểm tra URL.";
            txtConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
        catch (Exception ex)
        {
            txtConnectionStatus.Text = $"❌ Lỗi: {ex.Message}";
            txtConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private async void TestDirectConnection_Click(object sender, RoutedEventArgs e)
    {
        var geminiKey = txtGeminiKey.Text.Trim();
        if (string.IsNullOrEmpty(geminiKey))
        {
            txtDirectConnectionStatus.Text = "❌ Vui lòng nhập mã kết nối";
            txtDirectConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        txtDirectConnectionStatus.Text = "⏳ Đang kiểm tra...";
        txtDirectConnectionStatus.Foreground = System.Windows.Media.Brushes.Gray;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15);

            // Gọi Gemini models.list để verify API key
            var url = $"https://generativelanguage.googleapis.com/v1beta/models?key={geminiKey}";
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                txtDirectConnectionStatus.Text = "✅ API key hợp lệ! Kết nối AI trực tiếp thành công.";
                txtDirectConnectionStatus.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                var errorText = statusCode switch
                {
                    400 => "Key không đúng định dạng",
                    401 or 403 => "Key không hợp lệ hoặc đã bị vô hiệu hóa",
                    429 => "Key bị rate limit — thử lại sau",
                    _ => $"HTTP {statusCode}"
                };
                txtDirectConnectionStatus.Text = $"❌ {errorText}";
                txtDirectConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        catch (TaskCanceledException)
        {
            txtDirectConnectionStatus.Text = "❌ Hết thời gian chờ (timeout)";
            txtDirectConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
        catch (Exception ex)
        {
            txtDirectConnectionStatus.Text = $"❌ Lỗi: {ex.Message}";
            txtDirectConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var settings = AppSettingsService.Load();

        // AI toggle
        settings.AiEnabled = tglAiEnabled.IsChecked == true;

        // Nếu dev mode đang bật → cho phép chọn chế độ, ngược lại luôn VanBanPlus
        settings.UseVanBanPlusApi = _devModeActive ? rbVanBanPlus.IsChecked == true : true;
        settings.VanBanPlusApiUrl = txtApiUrl.Text.Trim().TrimEnd('/');
        settings.VanBanPlusApiKey = txtApiKey.Text.Trim();
        settings.GeminiApiKey = txtGeminiKey.Text.Trim();

        // Ghi/xóa timestamp dev mode
        if (!settings.UseVanBanPlusApi)
        {
            // Bật chế độ trực tiếp → ghi timestamp (nếu chưa có thì ghi mới)
            settings.DevModeActivatedAt ??= DateTime.Now;
        }
        else
        {
            // Về VanBanPlus → xóa timestamp
            settings.DevModeActivatedAt = null;
        }

        // Validate
        if (settings.AiEnabled && settings.UseVanBanPlusApi)
        {
            if (string.IsNullOrEmpty(settings.VanBanPlusApiKey))
            {
                MessageBox.Show("Vui lòng nhập mã kích hoạt!\n\n📞 Liên hệ Zalo: Thắng Phan — 0907136029\n💰 Chỉ từ 79.000đ/tháng",
                    "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        else if (settings.AiEnabled && !settings.UseVanBanPlusApi)
        {
            // Dev mode + Gemini trực tiếp
            if (string.IsNullOrEmpty(settings.GeminiApiKey))
            {
                MessageBox.Show("Vui lòng nhập mã kết nối AI trực tiếp!",
                    "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        AppSettingsService.Save(settings);
        SaveOrganizationTab();
        MessageBox.Show("✅ Đã lưu cài đặt thành công!\n\nCần khởi động lại ứng dụng để áp dụng thay đổi.",
            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    // ════════════════════════════════════════════════════════════════════════
    // ORGANIZATION TAB — Cấu hình cơ quan (NĐ 30/2020 — Phụ lục III, VI)
    // ════════════════════════════════════════════════════════════════════════

    private const string KEY_DEFAULT_SIGNER = "Org.DefaultSigner";
    private const string KEY_DEFAULT_SIGNING_TITLE = "Org.DefaultSigningTitle";
    private const string KEY_DEFAULT_LOCATION = "Org.DefaultLocation";

    private void LoadOrganizationTab()
    {
        try
        {
            // Load danh sách loại cơ quan vào combobox
            if (cboOrgType.Items.Count == 0)
            {
                foreach (AIVanBan.Core.Models.OrganizationType t in Enum.GetValues(typeof(AIVanBan.Core.Models.OrganizationType)))
                {
                    cboOrgType.Items.Add(new ComboBoxItem
                    {
                        Content = OrgTypeDisplay(t),
                        Tag = t
                    });
                }
            }

            // Load org config từ DB nếu có service, nếu không thì tạo service tạm
            DocumentService docSvc = _documentService ?? new DocumentService();
            var org = docSvc.GetOrganizationConfig();
            txtOrgName.Text = org.Name ?? "";
            txtOrgAbbreviation.Text = org.Abbreviation ?? "";

            foreach (ComboBoxItem item in cboOrgType.Items)
            {
                if (item.Tag is AIVanBan.Core.Models.OrganizationType t && t == org.Type)
                {
                    cboOrgType.SelectedItem = item;
                    break;
                }
            }
            if (cboOrgType.SelectedItem == null && cboOrgType.Items.Count > 0)
                cboOrgType.SelectedIndex = 0;

            // Load defaults từ ExtraSettings
            txtOrgLocation.Text = AppSettingsService.GetRawSettingValue(KEY_DEFAULT_LOCATION) ?? "";
            txtDefaultSigner.Text = AppSettingsService.GetRawSettingValue(KEY_DEFAULT_SIGNER) ?? "";
            txtDefaultSigningTitle.Text = AppSettingsService.GetRawSettingValue(KEY_DEFAULT_SIGNING_TITLE) ?? "";

            // Wire preview update
            txtOrgAbbreviation.TextChanged += (_, _) => UpdateOrgPreview();
            UpdateOrgPreview();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ LoadOrganizationTab error: {ex.Message}");
        }
    }

    private void UpdateOrgPreview()
    {
        try
        {
            var abbr = string.IsNullOrWhiteSpace(txtOrgAbbreviation.Text) ? "UBND" : txtOrgAbbreviation.Text.Trim();
            txtOrgPreview.Text = $"VD: 15/CV-{abbr}, 8/QĐ-{abbr}, 3/BC-{abbr}, 12/KH-{abbr}";
        }
        catch { /* ignore */ }
    }

    private void SaveOrganizationTab()
    {
        try
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtOrgName.Text) && string.IsNullOrWhiteSpace(txtOrgAbbreviation.Text))
            {
                // Người dùng để trống cả 2 → coi như chưa đụng tab này, bỏ qua
                return;
            }

            DocumentService docSvc = _documentService ?? new DocumentService();
            var org = docSvc.GetOrganizationConfig();
            org.Name = txtOrgName.Text?.Trim() ?? "";
            org.Abbreviation = (txtOrgAbbreviation.Text?.Trim() ?? "").ToUpperInvariant();

            if (cboOrgType.SelectedItem is ComboBoxItem item && item.Tag is AIVanBan.Core.Models.OrganizationType t)
                org.Type = t;

            docSvc.SaveOrganizationConfig(org);

            // Defaults via ExtraSettings
            AppSettingsService.SetRawSettingValue(KEY_DEFAULT_LOCATION, txtOrgLocation.Text?.Trim() ?? "");
            AppSettingsService.SetRawSettingValue(KEY_DEFAULT_SIGNER, txtDefaultSigner.Text?.Trim() ?? "");
            AppSettingsService.SetRawSettingValue(KEY_DEFAULT_SIGNING_TITLE, txtDefaultSigningTitle.Text?.Trim() ?? "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ SaveOrganizationTab error: {ex.Message}");
            MessageBox.Show($"Lỗi khi lưu cấu hình cơ quan:\n{ex.Message}",
                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>Tên hiển thị tiếng Việt cho OrganizationType (rút gọn)</summary>
    private static string OrgTypeDisplay(AIVanBan.Core.Models.OrganizationType t) => t switch
    {
        AIVanBan.Core.Models.OrganizationType.UbndXa => "UBND Xã/Phường/Thị trấn",
        AIVanBan.Core.Models.OrganizationType.UbndTinh => "UBND Tỉnh/Thành phố",
        AIVanBan.Core.Models.OrganizationType.HdndXa => "HĐND Xã/Phường/Thị trấn",
        AIVanBan.Core.Models.OrganizationType.HdndTinh => "HĐND Tỉnh/Thành phố",
        AIVanBan.Core.Models.OrganizationType.VanPhong => "Văn phòng UBND/HĐND",
        AIVanBan.Core.Models.OrganizationType.DangUyXa => "Đảng ủy Xã/Phường/Thị trấn",
        AIVanBan.Core.Models.OrganizationType.DangUyTinh => "Tỉnh ủy/Thành ủy",
        AIVanBan.Core.Models.OrganizationType.ChiBoDang => "Chi bộ Đảng",
        AIVanBan.Core.Models.OrganizationType.MatTran => "Mặt trận Tổ quốc",
        AIVanBan.Core.Models.OrganizationType.HoiNongDan => "Hội Nông dân",
        AIVanBan.Core.Models.OrganizationType.HoiPhuNu => "Hội Liên hiệp Phụ nữ",
        AIVanBan.Core.Models.OrganizationType.DoanThanhNien => "Đoàn TNCS Hồ Chí Minh",
        AIVanBan.Core.Models.OrganizationType.HoiCuuChienBinh => "Hội Cựu chiến binh",
        AIVanBan.Core.Models.OrganizationType.SoNoiVu => "Sở Nội vụ",
        AIVanBan.Core.Models.OrganizationType.SoTaiChinh => "Sở Tài chính",
        AIVanBan.Core.Models.OrganizationType.SoGiaoDuc => "Sở Giáo dục & Đào tạo",
        AIVanBan.Core.Models.OrganizationType.SoYTe => "Sở Y tế",
        AIVanBan.Core.Models.OrganizationType.TruongMamNon => "Trường Mầm non",
        AIVanBan.Core.Models.OrganizationType.TruongTieuHoc => "Trường Tiểu học",
        AIVanBan.Core.Models.OrganizationType.TruongTHCS => "Trường THCS",
        AIVanBan.Core.Models.OrganizationType.TruongTHPT => "Trường THPT",
        AIVanBan.Core.Models.OrganizationType.CongAn => "Công an",
        AIVanBan.Core.Models.OrganizationType.TramYTe => "Trạm Y tế Xã",
        AIVanBan.Core.Models.OrganizationType.CoQuanTuyChon => "Cơ quan tùy chọn",
        _ => t.ToString()
    };

    private void AiToggle_Changed(object sender, RoutedEventArgs e)
    {
        var enabled = tglAiEnabled.IsChecked == true;
        UpdateAiToggleVisual(enabled);
    }

    private void UpdateAiToggleVisual(bool enabled)
    {
        if (grpVanBanPlus == null) return; // not yet loaded

        // Khi toggle OFF → disable TẤT CẢ các group bên dưới
        grpVanBanPlus.IsEnabled = enabled;
        grpVanBanPlus.Opacity = enabled ? 1.0 : 0.4;

        // Cũng disable dev mode groups nếu đang hiện
        if (_devModeActive)
        {
            grpModeSelector.IsEnabled = enabled;
            grpModeSelector.Opacity = enabled ? 1.0 : 0.4;
            grpGeminiDirect.IsEnabled = enabled && (rbGeminiDirect.IsChecked == true);
            grpGeminiDirect.Opacity = enabled && (rbGeminiDirect.IsChecked == true) ? 1.0 : 0.4;
        }

        if (enabled)
        {
            txtAiToggleHint.Text = "✅ Tính năng AI đã được bật";
            brdAiToggle.BorderBrush = System.Windows.Media.Brushes.Green;
            // Áp dụng lại visibility theo chế độ kết nối
            UpdateVisibility();
        }
        else
        {
            txtAiToggleHint.Text = "Bật để sử dụng các tính năng AI nâng cao";
            brdAiToggle.BorderBrush = (System.Windows.Media.Brush)FindResource("MaterialDesignDivider");
        }
    }

    #region DTOs

    private class MeResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public MeData? Data { get; set; }
    }

    private class MeData
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = "";

        [JsonPropertyName("plan")]
        public string Plan { get; set; } = "";
    }

    #endregion
}
