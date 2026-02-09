using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class LoginRegisterDialog : Window
{
    public LoginRegisterDialog()
    {
        InitializeComponent();
    }

    private HttpClient CreateHttpClient()
    {
        var settings = AppSettingsService.Load();
        var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(20);
        if (!string.IsNullOrEmpty(settings.VercelBypassToken))
            http.DefaultRequestHeaders.Add("x-vercel-protection-bypass", settings.VercelBypassToken);
        return http;
    }

    private string GetBaseUrl()
    {
        return AppSettingsService.Load().VanBanPlusApiUrl.TrimEnd('/');
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        var email = txtLoginEmail.Text.Trim();
        var password = txtLoginPassword.Password;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            txtLoginStatus.Text = "❌ Vui lòng nhập email và mật khẩu";
            txtLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        txtLoginStatus.Text = "⏳ Đang đăng nhập...";
        txtLoginStatus.Foreground = System.Windows.Media.Brushes.Gray;

        try
        {
            using var http = CreateHttpClient();
            var body = new { email, password };
            var resp = await http.PostAsJsonAsync($"{GetBaseUrl()}/api/auth/login", body);
            var result = await resp.Content.ReadFromJsonAsync<AuthResponse>();

            if (resp.IsSuccessStatusCode && result?.Success == true && result.Data != null)
            {
                // Save settings
                var settings = AppSettingsService.Load();
                settings.UseVanBanPlusApi = true;
                settings.VanBanPlusApiKey = result.Data.ApiKey;
                settings.UserEmail = result.Data.Email;
                settings.UserFullName = result.Data.FullName;
                settings.UserPlan = result.Data.SubscriptionPlan;
                AppSettingsService.Save(settings);

                MessageBox.Show(
                    $"✅ Đăng nhập thành công!\n\n" +
                    $"👤 {result.Data.FullName}\n" +
                    $"📧 {result.Data.Email}\n" +
                    $"📦 Gói: {result.Data.SubscriptionPlan}\n\n" +
                    $"API Key đã được tự động lưu.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            else
            {
                txtLoginStatus.Text = $"❌ {result?.Message ?? "Đăng nhập thất bại"}";
                txtLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        catch (TaskCanceledException)
        {
            txtLoginStatus.Text = "❌ Hết thời gian chờ. Kiểm tra kết nối mạng.";
            txtLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
        catch (Exception ex)
        {
            txtLoginStatus.Text = $"❌ Lỗi: {ex.Message}";
            txtLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        var fullName = txtRegFullName.Text.Trim();
        var email = txtRegEmail.Text.Trim();
        var password = txtRegPassword.Password;
        var phone = txtRegPhone.Text.Trim();
        var company = txtRegCompany.Text.Trim();

        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            txtRegisterStatus.Text = "❌ Vui lòng điền các trường bắt buộc (*)";
            txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        if (password.Length < 6)
        {
            txtRegisterStatus.Text = "❌ Mật khẩu phải có ít nhất 6 ký tự";
            txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        txtRegisterStatus.Text = "⏳ Đang tạo tài khoản...";
        txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Gray;

        try
        {
            using var http = CreateHttpClient();
            var body = new { email, password, fullName, phone, company };
            var resp = await http.PostAsJsonAsync($"{GetBaseUrl()}/api/auth/register", body);
            var result = await resp.Content.ReadFromJsonAsync<AuthResponse>();

            if (resp.IsSuccessStatusCode && result?.Success == true && result.Data != null)
            {
                // Save settings
                var settings = AppSettingsService.Load();
                settings.UseVanBanPlusApi = true;
                settings.VanBanPlusApiKey = result.Data.ApiKey;
                settings.UserEmail = result.Data.Email;
                settings.UserFullName = result.Data.FullName;
                settings.UserPlan = result.Data.SubscriptionPlan;
                AppSettingsService.Save(settings);

                MessageBox.Show(
                    $"✅ Đăng ký thành công!\n\n" +
                    $"👤 {result.Data.FullName}\n" +
                    $"📧 {result.Data.Email}\n" +
                    $"📦 Gói: {result.Data.SubscriptionPlan}\n" +
                    $"🔑 API Key: {result.Data.ApiKey}\n\n" +
                    $"Key đã được tự động lưu vào ứng dụng.",
                    "Đăng ký thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            else
            {
                txtRegisterStatus.Text = $"❌ {result?.Message ?? "Đăng ký thất bại"}";
                txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        catch (TaskCanceledException)
        {
            txtRegisterStatus.Text = "❌ Hết thời gian chờ. Kiểm tra kết nối mạng.";
            txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
        catch (Exception ex)
        {
            txtRegisterStatus.Text = $"❌ Lỗi: {ex.Message}";
            txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    #region DTOs
    private class AuthResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("data")] public AuthData? Data { get; set; }
    }
    private class AuthData
    {
        [JsonPropertyName("userId")] public string UserId { get; set; } = "";
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        [JsonPropertyName("fullName")] public string FullName { get; set; } = "";
        [JsonPropertyName("apiKey")] public string ApiKey { get; set; } = "";
        [JsonPropertyName("subscriptionPlan")] public string SubscriptionPlan { get; set; } = "";
    }
    #endregion
}
