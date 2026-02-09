using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class ApiSettingsDialog : Window
{
    public ApiSettingsDialog()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = AppSettingsService.Load();

        // Chế độ API
        rbVanBanPlus.IsChecked = settings.UseVanBanPlusApi;
        rbGeminiDirect.IsChecked = !settings.UseVanBanPlusApi;

        // VanBanPlus
        txtApiUrl.Text = settings.VanBanPlusApiUrl;
        txtApiKey.Text = settings.VanBanPlusApiKey;

        // Gemini
        txtGeminiKey.Text = settings.GeminiApiKey;

        UpdateVisibility();
        UpdateCurrentStatus(settings);
    }

    private void ApiMode_Changed(object sender, RoutedEventArgs e)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (grpVanBanPlus == null || grpGeminiDirect == null) return;

        var useVanBanPlus = rbVanBanPlus.IsChecked == true;
        grpVanBanPlus.IsEnabled = useVanBanPlus;
        grpVanBanPlus.Opacity = useVanBanPlus ? 1.0 : 0.4;
        grpGeminiDirect.IsEnabled = !useVanBanPlus;
        grpGeminiDirect.Opacity = !useVanBanPlus ? 1.0 : 0.4;
    }

    private void UpdateCurrentStatus(AppSettings settings)
    {
        if (settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.VanBanPlusApiKey))
        {
            var maskedKey = settings.VanBanPlusApiKey.Length > 10
                ? settings.VanBanPlusApiKey[..10] + "..."
                : settings.VanBanPlusApiKey;
            txtCurrentStatus.Text = $"☁️ Chế độ: VanBanPlus API\n" +
                                    $"🌐 URL: {settings.VanBanPlusApiUrl}\n" +
                                    $"🔑 Key: {maskedKey}\n" +
                                    $"👤 User: {(string.IsNullOrEmpty(settings.UserEmail) ? "(chưa xác thực)" : settings.UserEmail)}";
        }
        else if (!string.IsNullOrEmpty(settings.GeminiApiKey))
        {
            var maskedKey = settings.GeminiApiKey.Length > 10
                ? settings.GeminiApiKey[..10] + "..."
                : settings.GeminiApiKey;
            txtCurrentStatus.Text = $"🔑 Chế độ: Gemini trực tiếp\n" +
                                    $"🔑 Key: {maskedKey}";
        }
        else
        {
            txtCurrentStatus.Text = "⚠️ Chưa cấu hình API Key. Vui lòng nhập thông tin.";
        }
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        var apiUrl = txtApiUrl.Text.Trim().TrimEnd('/');
        var apiKey = txtApiKey.Text.Trim();

        if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
        {
            txtConnectionStatus.Text = "❌ Vui lòng nhập API URL và API Key";
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

            txtConnectionStatus.Text = "✅ Server OK nhưng API Key không hợp lệ hoặc hết hạn";
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

        settings.UseVanBanPlusApi = rbVanBanPlus.IsChecked == true;
        settings.VanBanPlusApiUrl = txtApiUrl.Text.Trim().TrimEnd('/');
        settings.VanBanPlusApiKey = txtApiKey.Text.Trim();
        settings.GeminiApiKey = txtGeminiKey.Text.Trim();

        // Validate
        if (settings.UseVanBanPlusApi)
        {
            if (string.IsNullOrEmpty(settings.VanBanPlusApiKey))
            {
                MessageBox.Show("Vui lòng nhập VanBanPlus API Key!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        else
        {
            if (string.IsNullOrEmpty(settings.GeminiApiKey))
            {
                MessageBox.Show("Vui lòng nhập Gemini API Key!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        AppSettingsService.Save(settings);
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
