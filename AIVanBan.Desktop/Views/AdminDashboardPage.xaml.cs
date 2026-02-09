using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class AdminDashboardPage : Page
{
    private List<UserRow> _allUsers = new();

    public AdminDashboardPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAllAsync();
    }

    #region Helpers

    private HttpClient CreateAdminClient()
    {
        var settings = AppSettingsService.Load();
        var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(30);
        http.DefaultRequestHeaders.Add("X-API-Key", settings.VanBanPlusApiKey);
        if (!string.IsNullOrEmpty(settings.VercelBypassToken))
            http.DefaultRequestHeaders.Add("x-vercel-protection-bypass", settings.VercelBypassToken);
        return http;
    }

    private string GetBaseUrl() => AppSettingsService.Load().VanBanPlusApiUrl.TrimEnd('/');

    #endregion

    #region Load Data

    private async Task LoadAllAsync()
    {
        loadingOverlay.Visibility = Visibility.Visible;
        try
        {
            await Task.WhenAll(LoadStatsAsync(), LoadUsersAsync());
        }
        finally
        {
            loadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            using var http = CreateAdminClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<AdminStatsDto>>($"{GetBaseUrl()}/api/admin/stats");

            if (resp?.Success == true && resp.Data != null)
            {
                var d = resp.Data;
                txtTotalUsers.Text = d.TotalUsers.ToString();
                txtActiveUsers.Text = d.ActiveUsers.ToString();
                txtRequestsToday.Text = d.RequestsToday.ToString();
                txtRequestsMonth.Text = d.RequestsThisMonth.ToString();
                txtCostMonth.Text = $"{d.CostThisMonth:N0} ₫";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải stats: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            using var http = CreateAdminClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<List<AdminUserDto>>>($"{GetBaseUrl()}/api/admin/stats?action=users");

            if (resp?.Success == true && resp.Data != null)
            {
                _allUsers = resp.Data.Select(u => new UserRow
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone ?? "",
                    Company = u.Company ?? "",
                    Plan = u.SubscriptionPlanId,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    ActiveIcon = u.IsActive ? "✅" : "🚫",
                    RequestsUsed = u.Usage?.RequestsUsed ?? 0,
                    TokensUsed = u.Usage?.TokensUsed ?? 0,
                    CreatedDate = u.CreatedDate?[..10] ?? "",
                    LastLogin = u.LastLoginDate?[..10] ?? "—"
                }).ToList();

                ApplyFilter();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải users: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ApplyFilter()
    {
        var keyword = txtSearch.Text.Trim().ToLower();
        var filtered = string.IsNullOrEmpty(keyword)
            ? _allUsers
            : _allUsers.Where(u =>
                u.FullName.ToLower().Contains(keyword) ||
                u.Email.ToLower().Contains(keyword) ||
                u.Company.ToLower().Contains(keyword) ||
                u.Plan.ToLower().Contains(keyword)
            ).ToList();

        dgUsers.ItemsSource = filtered;
    }

    private void SearchChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAllAsync();

    #endregion

    #region User Actions

    private void UserRow_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (dgUsers.SelectedItem is UserRow user)
        {
            ShowUserDetail(user);
        }
    }

    private void EditUser_Click(object sender, RoutedEventArgs e)
    {
        var userId = (sender as Button)?.Tag as string;
        var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
        if (user != null) ShowUserDetail(user);
    }

    private void ShowUserDetail(UserRow user)
    {
        var dialog = new AdminUserEditDialog(user.UserId, user.FullName, user.Email, 
            user.Phone, user.Company, user.Plan, user.IsActive)
        {
            Owner = Window.GetWindow(this)
        };
        if (dialog.ShowDialog() == true)
        {
            _ = LoadAllAsync();
        }
    }

    private async void ResetKey_Click(object sender, RoutedEventArgs e)
    {
        var userId = (sender as Button)?.Tag as string;
        var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
        if (user == null) return;

        var confirm = MessageBox.Show(
            $"Tạo API Key mới cho {user.FullName}?\nKey cũ sẽ không còn hoạt động.",
            "Reset API Key", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            using var http = CreateAdminClient();
            var body = new { userId, resetApiKey = true };
            var resp = await http.PutAsJsonAsync($"{GetBaseUrl()}/api/admin/users", body);
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<AdminUserUpdateDto>>();

            if (resp.IsSuccessStatusCode && result?.Success == true)
            {
                MessageBox.Show($"✅ Đã tạo key mới cho {user.FullName}.\n\nKey mới: {result.Data?.ApiKey}",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"❌ {result?.Message ?? "Lỗi"}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ToggleActive_Click(object sender, RoutedEventArgs e)
    {
        var userId = (sender as Button)?.Tag as string;
        var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
        if (user == null) return;

        var newState = !user.IsActive;
        var action = newState ? "MỞ KHÓA" : "KHÓA";
        var confirm = MessageBox.Show(
            $"{action} tài khoản {user.FullName} ({user.Email})?",
            "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            using var http = CreateAdminClient();
            var body = new { userId, isActive = newState };
            var resp = await http.PutAsJsonAsync($"{GetBaseUrl()}/api/admin/users", body);
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (resp.IsSuccessStatusCode && result?.Success == true)
            {
                MessageBox.Show($"✅ Đã {action.ToLower()} tài khoản {user.FullName}.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadUsersAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DeleteUser_Click(object sender, RoutedEventArgs e)
    {
        var userId = (sender as Button)?.Tag as string;
        var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
        if (user == null) return;

        var confirm = MessageBox.Show(
            $"⚠️ XÓA VĨNH VIỄN tài khoản {user.FullName} ({user.Email})?\n\n" +
            "Tất cả dữ liệu usage sẽ bị xóa. Hành động này không thể hoàn tác!",
            "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            using var http = CreateAdminClient();
            var resp = await http.DeleteAsync($"{GetBaseUrl()}/api/admin/users?id={userId}");
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (resp.IsSuccessStatusCode && result?.Success == true)
            {
                MessageBox.Show($"✅ Đã xóa tài khoản {user.FullName}.",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAllAsync();
            }
            else
            {
                MessageBox.Show($"❌ {result?.Message ?? "Lỗi xóa"}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddUser_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AdminUserEditDialog(null, "", "", "", "", "free", true)
        {
            Owner = Window.GetWindow(this)
        };
        if (dialog.ShowDialog() == true)
        {
            _ = LoadAllAsync();
        }
    }

    #endregion

    #region DTOs

    private class ApiResponse<T>
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("data")] public T? Data { get; set; }
    }

    private class AdminStatsDto
    {
        [JsonPropertyName("totalUsers")] public int TotalUsers { get; set; }
        [JsonPropertyName("activeUsers")] public int ActiveUsers { get; set; }
        [JsonPropertyName("requestsToday")] public int RequestsToday { get; set; }
        [JsonPropertyName("requestsThisMonth")] public int RequestsThisMonth { get; set; }
        [JsonPropertyName("tokensThisMonth")] public long TokensThisMonth { get; set; }
        [JsonPropertyName("costThisMonth")] public double CostThisMonth { get; set; }
        [JsonPropertyName("errorsToday")] public int ErrorsToday { get; set; }
    }

    private class AdminUserDto
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        [JsonPropertyName("full_name")] public string FullName { get; set; } = "";
        [JsonPropertyName("phone")] public string? Phone { get; set; }
        [JsonPropertyName("company")] public string? Company { get; set; }
        [JsonPropertyName("role")] public string Role { get; set; } = "user";
        [JsonPropertyName("is_active")] public bool IsActive { get; set; }
        [JsonPropertyName("subscription_plan_id")] public string SubscriptionPlanId { get; set; } = "free";
        [JsonPropertyName("created_date")] public string? CreatedDate { get; set; }
        [JsonPropertyName("last_login_date")] public string? LastLoginDate { get; set; }
        [JsonPropertyName("usage")] public AdminUsageDto? Usage { get; set; }
    }

    private class AdminUsageDto
    {
        [JsonPropertyName("requestsUsed")] public int RequestsUsed { get; set; }
        [JsonPropertyName("tokensUsed")] public long TokensUsed { get; set; }
    }

    private class AdminUserUpdateDto
    {
        [JsonPropertyName("api_key")] public string? ApiKey { get; set; }
    }

    private class UserRow
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Company { get; set; } = "";
        public string Plan { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
        public string ActiveIcon { get; set; } = "";
        public int RequestsUsed { get; set; }
        public long TokensUsed { get; set; }
        public string CreatedDate { get; set; } = "";
        public string LastLogin { get; set; } = "";
    }

    #endregion
}
