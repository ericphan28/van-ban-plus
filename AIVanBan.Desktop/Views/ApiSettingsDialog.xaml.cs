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

    private void LoadSettings()
    {
        var settings = AppSettingsService.Load();

        tglAiEnabled.IsChecked = settings.AiEnabled;
        UpdateAiToggleVisual(settings.AiEnabled);

        txtApiUrl.Text = settings.VanBanPlusApiUrl;
        txtApiKey.Text = settings.VanBanPlusApiKey;

        UpdateCurrentStatus(settings);
    }

    private void UpdateCurrentStatus(AppSettings settings)
    {
        if (!string.IsNullOrEmpty(settings.VanBanPlusApiKey))
        {
            var maskedKey = settings.VanBanPlusApiKey.Length > 10
                ? settings.VanBanPlusApiKey[..10] + "..."
                : settings.VanBanPlusApiKey;
            txtCurrentStatus.Text = $"✅ Đã kích hoạt\n" +
                                    $"🌐 Server: {settings.VanBanPlusApiUrl}\n" +
                                    $"🔑 Mã: {maskedKey}\n" +
                                    $"👤 {(string.IsNullOrEmpty(settings.UserEmail) ? "(chưa xác thực)" : settings.UserEmail)}";
        }
        else
        {
            txtCurrentStatus.Text = "⚠️ Chưa kích hoạt AI\n\nCần nhập mã kích hoạt để sử dụng các tính năng AI.\n📞 Liên hệ Zalo: Thắng Phan — 0907136029";
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

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var settings = AppSettingsService.Load();

        settings.AiEnabled = tglAiEnabled.IsChecked == true;
        settings.VanBanPlusApiUrl = txtApiUrl.Text.Trim().TrimEnd('/');
        settings.VanBanPlusApiKey = txtApiKey.Text.Trim();

        if (settings.AiEnabled && string.IsNullOrEmpty(settings.VanBanPlusApiKey))
        {
            MessageBox.Show("Cần nhập mã kích hoạt.\n\n📞 Liên hệ Zalo: Thắng Phan — 0907136029\n💰 Chỉ từ 99.000đ/tháng",
                "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        AppSettingsService.Save(settings);
        SaveOrganizationTab();
        // Ngoài SaveOrganizationTab vì hàm đó return sớm khi tên và viết tắt cơ quan đều trống.
        // Cấp trên luôn để trống: chính quyền hai cấp (ô đã ẩn, xóa giá trị cũ như «thành phố Thống Nhất»).
        ThietLapDiaPhuong.Luu(txtDpXa.Text, "", txtDpTinh.Text, txtDpDacDiem.Text);
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

            var diaPhuong = ThietLapDiaPhuong.Doc();
            txtDpXa.Text = diaPhuong?.Xa ?? "";
            txtDpCapTren.Text = diaPhuong?.CoQuanCapTren ?? "";
            txtDpTinh.Text = diaPhuong?.Tinh ?? "";
            txtDpDacDiem.Text = string.Join(Environment.NewLine, diaPhuong?.DacDiem ?? Array.Empty<string>());

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

        grpVanBanPlus.IsEnabled = enabled;
        grpVanBanPlus.Opacity = enabled ? 1.0 : 0.4;

        if (enabled)
        {
            txtAiToggleHint.Text = "✅ Tính năng AI đã được bật";
            brdAiToggle.BorderBrush = System.Windows.Media.Brushes.Green;
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
