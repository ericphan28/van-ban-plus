using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class UserProfileDialog : Window
{
    private int _currentPage = 1;
    private int _totalPages = 1;

    public UserProfileDialog()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadProfileAsync();
    }

    #region Helpers

    private HttpClient CreateHttpClient()
    {
        var settings = AppSettingsService.Load();
        var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(20);
        http.DefaultRequestHeaders.Add("X-API-Key", settings.VanBanPlusApiKey);
        if (!string.IsNullOrEmpty(settings.VercelBypassToken))
            http.DefaultRequestHeaders.Add("x-vercel-protection-bypass", settings.VercelBypassToken);
        return http;
    }

    private string GetBaseUrl() => AppSettingsService.Load().VanBanPlusApiUrl.TrimEnd('/');

    #endregion

    #region Load Profile

    private async Task LoadProfileAsync()
    {
        try
        {
            using var http = CreateHttpClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<UserProfileDto>>($"{GetBaseUrl()}/api/auth/me");

            if (resp?.Success == true && resp.Data != null)
            {
                var d = resp.Data;
                txtHeaderName.Text = d.FullName;
                txtHeaderEmail.Text = d.Email;
                txtHeaderPlan.Text = $"📦 {d.Plan}";

                txtFullName.Text = d.FullName;
                txtEmail.Text = d.Email;
                txtPhone.Text = d.Phone;
                txtCompany.Text = d.Company;
                txtApiKey.Text = d.ApiKey;
                txtPlanInfo.Text = $"Gói: {d.Plan} | Bắt đầu: {d.SubscriptionStartDate?[..10] ?? "--"}";
                txtCreatedDate.Text = $"Tạo tài khoản: {d.CreatedDate?[..10] ?? "--"}";

                // Update cached settings
                var settings = AppSettingsService.Load();
                settings.UserEmail = d.Email;
                settings.UserFullName = d.FullName;
                settings.UserPlan = d.Plan;
                AppSettingsService.Save(settings);
            }
        }
        catch (Exception ex)
        {
            txtProfileStatus.Text = $"❌ Lỗi tải profile: {ex.Message}";
            txtProfileStatus.Foreground = System.Windows.Media.Brushes.Red;
        }

        // Load usage & logs in background
        _ = LoadUsageSummaryAsync();
        _ = LoadDailyUsageAsync();
        _ = LoadLogsAsync();
    }

    #endregion

    #region Tab 1: Profile

    private async void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        txtProfileStatus.Text = "⏳ Đang lưu...";
        txtProfileStatus.Foreground = System.Windows.Media.Brushes.Gray;

        try
        {
            using var http = CreateHttpClient();
            var body = new { fullName = txtFullName.Text.Trim(), phone = txtPhone.Text.Trim(), company = txtCompany.Text.Trim() };
            var resp = await http.PutAsJsonAsync($"{GetBaseUrl()}/api/auth/profile", body);
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (resp.IsSuccessStatusCode && result?.Success == true)
            {
                txtProfileStatus.Text = "✅ Đã lưu thông tin thành công!";
                txtProfileStatus.Foreground = System.Windows.Media.Brushes.Green;
                txtHeaderName.Text = txtFullName.Text.Trim();

                var settings = AppSettingsService.Load();
                settings.UserFullName = txtFullName.Text.Trim();
                AppSettingsService.Save(settings);
            }
            else
            {
                txtProfileStatus.Text = $"❌ {result?.Message ?? "Lỗi cập nhật"}";
                txtProfileStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        catch (Exception ex)
        {
            txtProfileStatus.Text = $"❌ {ex.Message}";
            txtProfileStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    #endregion

    #region Tab 2: Change Password

    private async void ChangePassword_Click(object sender, RoutedEventArgs e)
    {
        var current = txtCurrentPwd.Password;
        var newPwd = txtNewPwd.Password;
        var confirm = txtConfirmPwd.Password;

        if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(newPwd))
        {
            SetPwdStatus("❌ Vui lòng nhập đầy đủ mật khẩu", false);
            return;
        }

        if (newPwd.Length < 6)
        {
            SetPwdStatus("❌ Mật khẩu mới phải ≥ 6 ký tự", false);
            return;
        }

        if (newPwd != confirm)
        {
            SetPwdStatus("❌ Mật khẩu xác nhận không khớp", false);
            return;
        }

        SetPwdStatus("⏳ Đang đổi mật khẩu...", true);

        try
        {
            using var http = CreateHttpClient();
            var body = new { currentPassword = current, newPassword = newPwd };
            var resp = await http.PutAsJsonAsync($"{GetBaseUrl()}/api/auth/profile?action=change-password", body);
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (resp.IsSuccessStatusCode && result?.Success == true)
            {
                SetPwdStatus("✅ Đổi mật khẩu thành công!", true);
                txtCurrentPwd.Clear();
                txtNewPwd.Clear();
                txtConfirmPwd.Clear();
            }
            else
            {
                SetPwdStatus($"❌ {result?.Message ?? "Đổi mật khẩu thất bại"}", false);
            }
        }
        catch (Exception ex)
        {
            SetPwdStatus($"❌ {ex.Message}", false);
        }
    }

    private void SetPwdStatus(string msg, bool ok)
    {
        txtPwdStatus.Text = msg;
        txtPwdStatus.Foreground = ok ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
    }

    #endregion

    #region Tab 3: API Key

    private void CopyApiKey_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(txtApiKey.Text))
        {
            Clipboard.SetText(txtApiKey.Text);
            txtKeyStatus.Text = "✅ Đã copy API Key!";
            txtKeyStatus.Foreground = System.Windows.Media.Brushes.Green;
        }
    }

    private async void ResetApiKey_Click(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            "⚠️ Tạo API Key mới sẽ khiến key cũ không còn hoạt động.\n\nBạn có chắc chắn?",
            "Xác nhận tạo key mới", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        txtKeyStatus.Text = "⏳ Đang tạo key mới...";
        txtKeyStatus.Foreground = System.Windows.Media.Brushes.Gray;

        try
        {
            using var http = CreateHttpClient();
            var resp = await http.PutAsJsonAsync($"{GetBaseUrl()}/api/auth/profile?action=reset-api-key", new { });
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<ResetKeyData>>();

            if (resp.IsSuccessStatusCode && result?.Success == true && result.Data != null)
            {
                txtApiKey.Text = result.Data.ApiKey;

                // Update settings with new key
                var settings = AppSettingsService.Load();
                settings.VanBanPlusApiKey = result.Data.ApiKey;
                AppSettingsService.Save(settings);

                txtKeyStatus.Text = "✅ API Key mới đã được tạo và lưu!";
                txtKeyStatus.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                txtKeyStatus.Text = $"❌ {result?.Message ?? "Lỗi tạo key"}";
                txtKeyStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        catch (Exception ex)
        {
            txtKeyStatus.Text = $"❌ {ex.Message}";
            txtKeyStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    #endregion

    #region Tab 4: Usage & Billing

    private async Task LoadUsageSummaryAsync()
    {
        try
        {
            using var http = CreateHttpClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<UsageSummaryDto>>($"{GetBaseUrl()}/api/usage");

            if (resp?.Success == true && resp.Data != null)
            {
                var d = resp.Data;
                txtUsageRequests.Text = $"{d.RequestsUsed} / {d.RequestsLimit}";
                pbRequests.Value = d.RequestsPercent;
                txtUsageTokens.Text = d.TokensUsed.ToString("N0");
                pbTokens.Value = d.TokensPercent;
                txtUsageCost.Text = $"{d.EstimatedCostThisMonth:N0} ₫";
                txtBillingPeriod.Text = $"Kỳ: {d.BillingPeriod}";
            }
        }
        catch { /* ignore */ }
    }

    private async Task LoadDailyUsageAsync()
    {
        try
        {
            using var http = CreateHttpClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<List<DailyUsageDto>>>($"{GetBaseUrl()}/api/usage?action=daily&days=30");

            if (resp?.Success == true && resp.Data != null)
            {
                dgDailyUsage.ItemsSource = resp.Data;
            }
        }
        catch { /* ignore */ }
    }

    private async void RefreshUsage_Click(object sender, RoutedEventArgs e)
    {
        await LoadUsageSummaryAsync();
        await LoadDailyUsageAsync();
    }

    #endregion

    #region Tab 5: API Logs

    private async Task LoadLogsAsync()
    {
        try
        {
            using var http = CreateHttpClient();
            var url = $"{GetBaseUrl()}/api/usage?action=logs&days=30&page={_currentPage}&limit=50";
            var resp = await http.GetFromJsonAsync<ApiResponse<LogsResponseDto>>(url);

            if (resp?.Success == true && resp.Data != null)
            {
                var logs = resp.Data.Logs?.Select(l => new LogRow
                {
                    RequestDate = l.RequestDate?[..19].Replace("T", " ") ?? "",
                    Endpoint = l.Endpoint,
                    RequestType = l.RequestType,
                    TotalTokens = l.TotalTokens,
                    ResponseTimeMs = l.ResponseTimeMs,
                    StatusIcon = l.IsSuccess ? "✅" : "❌",
                    ErrorMessage = l.ErrorMessage ?? ""
                }).ToList();

                dgLogs.ItemsSource = logs;

                var p = resp.Data.Pagination;
                if (p != null)
                {
                    _totalPages = p.TotalPages;
                    txtPageInfo.Text = $"Trang {p.Page} / {p.TotalPages} ({p.Total} kết quả)";
                    btnPrevPage.IsEnabled = p.Page > 1;
                    btnNextPage.IsEnabled = p.Page < p.TotalPages;
                }
            }
        }
        catch { /* ignore */ }
    }

    private async void RefreshLogs_Click(object sender, RoutedEventArgs e)
    {
        _currentPage = 1;
        await LoadLogsAsync();
    }

    private async void PrevPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1) { _currentPage--; await LoadLogsAsync(); }
    }

    private async void NextPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages) { _currentPage++; await LoadLogsAsync(); }
    }

    #endregion

    #region DTOs

    private class ApiResponse<T>
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("data")] public T? Data { get; set; }
    }

    private class UserProfileDto
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        [JsonPropertyName("fullName")] public string FullName { get; set; } = "";
        [JsonPropertyName("phone")] public string Phone { get; set; } = "";
        [JsonPropertyName("company")] public string Company { get; set; } = "";
        [JsonPropertyName("apiKey")] public string ApiKey { get; set; } = "";
        [JsonPropertyName("plan")] public string Plan { get; set; } = "";
        [JsonPropertyName("subscriptionStartDate")] public string? SubscriptionStartDate { get; set; }
        [JsonPropertyName("subscriptionEndDate")] public string? SubscriptionEndDate { get; set; }
        [JsonPropertyName("createdDate")] public string? CreatedDate { get; set; }
        [JsonPropertyName("lastLoginDate")] public string? LastLoginDate { get; set; }
    }

    private class ResetKeyData
    {
        [JsonPropertyName("apiKey")] public string ApiKey { get; set; } = "";
    }

    private class UsageSummaryDto
    {
        [JsonPropertyName("planName")] public string PlanName { get; set; } = "";
        [JsonPropertyName("requestsUsed")] public int RequestsUsed { get; set; }
        [JsonPropertyName("requestsLimit")] public int RequestsLimit { get; set; }
        [JsonPropertyName("tokensUsed")] public long TokensUsed { get; set; }
        [JsonPropertyName("tokensLimit")] public long TokensLimit { get; set; }
        [JsonPropertyName("requestsPercent")] public double RequestsPercent { get; set; }
        [JsonPropertyName("tokensPercent")] public double TokensPercent { get; set; }
        [JsonPropertyName("estimatedCostThisMonth")] public double EstimatedCostThisMonth { get; set; }
        [JsonPropertyName("billingPeriod")] public string BillingPeriod { get; set; } = "";
    }

    private class DailyUsageDto
    {
        [JsonPropertyName("date")] public string Date { get; set; } = "";
        [JsonPropertyName("requests")] public int Requests { get; set; }
        [JsonPropertyName("tokens")] public long Tokens { get; set; }
        [JsonPropertyName("cost")] public double Cost { get; set; }
    }

    private class LogsResponseDto
    {
        [JsonPropertyName("logs")] public List<LogEntryDto>? Logs { get; set; }
        [JsonPropertyName("pagination")] public PaginationDto? Pagination { get; set; }
    }

    private class LogEntryDto
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("endpoint")] public string Endpoint { get; set; } = "";
        [JsonPropertyName("request_type")] public string RequestType { get; set; } = "";
        [JsonPropertyName("request_date")] public string? RequestDate { get; set; }
        [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; }
        [JsonPropertyName("response_time_ms")] public int ResponseTimeMs { get; set; }
        [JsonPropertyName("is_success")] public bool IsSuccess { get; set; }
        [JsonPropertyName("error_message")] public string? ErrorMessage { get; set; }
    }

    private class PaginationDto
    {
        [JsonPropertyName("page")] public int Page { get; set; }
        [JsonPropertyName("limit")] public int Limit { get; set; }
        [JsonPropertyName("total")] public int Total { get; set; }
        [JsonPropertyName("totalPages")] public int TotalPages { get; set; }
    }

    private class LogRow
    {
        public string RequestDate { get; set; } = "";
        public string Endpoint { get; set; } = "";
        public string RequestType { get; set; } = "";
        public int TotalTokens { get; set; }
        public int ResponseTimeMs { get; set; }
        public string StatusIcon { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }

    #endregion
}
