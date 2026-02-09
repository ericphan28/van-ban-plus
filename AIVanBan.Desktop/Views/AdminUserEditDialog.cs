using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Dialog để Admin tạo mới hoặc chỉnh sửa user
/// </summary>
public class AdminUserEditDialog : Window
{
    private readonly string? _userId;
    private readonly System.Windows.Controls.TextBox _txtName;
    private readonly System.Windows.Controls.TextBox _txtEmail;
    private readonly System.Windows.Controls.PasswordBox _txtPassword;
    private readonly System.Windows.Controls.TextBox _txtPhone;
    private readonly System.Windows.Controls.TextBox _txtCompany;
    private readonly System.Windows.Controls.ComboBox _cbPlan;
    private readonly System.Windows.Controls.CheckBox _chkActive;
    private readonly System.Windows.Controls.TextBlock _txtStatus;

    public AdminUserEditDialog(string? userId, string fullName, string email,
        string phone, string company, string plan, bool isActive)
    {
        _userId = userId;
        Title = userId == null ? "➕ Thêm người dùng" : $"✏️ Sửa: {fullName}";
        Width = 440;
        Height = userId == null ? 500 : 460;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var stack = new System.Windows.Controls.StackPanel { Margin = new Thickness(24) };

        // Header
        var header = new System.Windows.Controls.TextBlock
        {
            Text = userId == null ? "Tạo tài khoản mới" : "Chỉnh sửa thông tin",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 16)
        };
        stack.Children.Add(header);

        // Name
        _txtName = CreateTextBox("Họ và tên *");
        _txtName.Text = fullName;
        stack.Children.Add(_txtName);

        // Email
        _txtEmail = CreateTextBox("Email *");
        _txtEmail.Text = email;
        if (userId != null) _txtEmail.IsReadOnly = true;
        stack.Children.Add(_txtEmail);

        // Password (only for new users)
        if (userId == null)
        {
            _txtPassword = new System.Windows.Controls.PasswordBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                Height = 36,
                FontSize = 13
            };
            MaterialDesignThemes.Wpf.HintAssist.SetHint(_txtPassword, "Mật khẩu (≥ 6 ký tự) *");
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(_txtPassword, true);
            stack.Children.Add(_txtPassword);
        }
        else
        {
            _txtPassword = new System.Windows.Controls.PasswordBox(); // dummy
        }

        // Phone
        _txtPhone = CreateTextBox("Số điện thoại");
        _txtPhone.Text = phone;
        stack.Children.Add(_txtPhone);

        // Company
        _txtCompany = CreateTextBox("Cơ quan / Đơn vị");
        _txtCompany.Text = company;
        stack.Children.Add(_txtCompany);

        // Plan
        _cbPlan = new System.Windows.Controls.ComboBox
        {
            Margin = new Thickness(0, 0, 0, 10),
            Height = 36,
            FontSize = 13
        };
        _cbPlan.Items.Add("free");
        _cbPlan.Items.Add("basic");
        _cbPlan.Items.Add("pro");
        _cbPlan.Items.Add("enterprise");
        _cbPlan.SelectedItem = plan;
        MaterialDesignThemes.Wpf.HintAssist.SetHint(_cbPlan, "Gói subscription");
        MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(_cbPlan, true);
        stack.Children.Add(_cbPlan);

        // Active
        _chkActive = new System.Windows.Controls.CheckBox
        {
            Content = "Tài khoản hoạt động",
            IsChecked = isActive,
            Margin = new Thickness(0, 4, 0, 16),
            FontSize = 13
        };
        stack.Children.Add(_chkActive);

        // Buttons
        var btnPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var btnSave = new System.Windows.Controls.Button
        {
            Content = userId == null ? "➕ Tạo" : "💾 Lưu",
            Height = 36,
            Padding = new Thickness(20, 0, 20, 0),
            FontSize = 13,
            Margin = new Thickness(0, 0, 8, 0)
        };
        btnSave.Click += async (s, e) => await SaveAsync();
        btnPanel.Children.Add(btnSave);

        var btnCancel = new System.Windows.Controls.Button
        {
            Content = "Hủy",
            Height = 36,
            Padding = new Thickness(20, 0, 20, 0),
            FontSize = 13
        };
        btnCancel.Click += (s, e) => { DialogResult = false; Close(); };
        btnPanel.Children.Add(btnCancel);

        stack.Children.Add(btnPanel);

        // Status
        _txtStatus = new System.Windows.Controls.TextBlock
        {
            FontSize = 12,
            Margin = new Thickness(0, 8, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
        stack.Children.Add(_txtStatus);

        Content = stack;
    }

    private System.Windows.Controls.TextBox CreateTextBox(string hint)
    {
        var tb = new System.Windows.Controls.TextBox
        {
            Margin = new Thickness(0, 0, 0, 10),
            Height = 36,
            FontSize = 13
        };
        MaterialDesignThemes.Wpf.HintAssist.SetHint(tb, hint);
        MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(tb, true);
        return tb;
    }

    private HttpClient CreateAdminClient()
    {
        var settings = AppSettingsService.Load();
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        http.DefaultRequestHeaders.Add("X-API-Key", settings.VanBanPlusApiKey);
        if (!string.IsNullOrEmpty(settings.VercelBypassToken))
            http.DefaultRequestHeaders.Add("x-vercel-protection-bypass", settings.VercelBypassToken);
        return http;
    }

    private string GetBaseUrl() => AppSettingsService.Load().VanBanPlusApiUrl.TrimEnd('/');

    private async Task SaveAsync()
    {
        var name = _txtName.Text.Trim();
        var email = _txtEmail.Text.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
        {
            _txtStatus.Text = "❌ Vui lòng điền họ tên và email";
            _txtStatus.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        _txtStatus.Text = "⏳ Đang lưu...";
        _txtStatus.Foreground = System.Windows.Media.Brushes.Gray;

        try
        {
            using var http = CreateAdminClient();
            var baseUrl = GetBaseUrl();

            if (_userId == null)
            {
                // CREATE
                var pwd = _txtPassword.Password;
                if (string.IsNullOrEmpty(pwd) || pwd.Length < 6)
                {
                    _txtStatus.Text = "❌ Mật khẩu phải ≥ 6 ký tự";
                    _txtStatus.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                var body = new
                {
                    email,
                    password = pwd,
                    fullName = name,
                    phone = _txtPhone.Text.Trim(),
                    company = _txtCompany.Text.Trim(),
                    planId = _cbPlan.SelectedItem?.ToString() ?? "free"
                };
                var resp = await http.PostAsJsonAsync($"{baseUrl}/api/admin/users", body);
                var result = await resp.Content.ReadFromJsonAsync<ApiResp>();

                if (resp.IsSuccessStatusCode && result?.Success == true)
                {
                    MessageBox.Show("✅ Tạo tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    _txtStatus.Text = $"❌ {result?.Message ?? "Lỗi tạo user"}";
                    _txtStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            else
            {
                // UPDATE
                var body = new
                {
                    userId = _userId,
                    fullName = name,
                    phone = _txtPhone.Text.Trim(),
                    company = _txtCompany.Text.Trim(),
                    planId = _cbPlan.SelectedItem?.ToString(),
                    isActive = _chkActive.IsChecked == true
                };
                var resp = await http.PutAsJsonAsync($"{baseUrl}/api/admin/users", body);
                var result = await resp.Content.ReadFromJsonAsync<ApiResp>();

                if (resp.IsSuccessStatusCode && result?.Success == true)
                {
                    MessageBox.Show("✅ Cập nhật thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    _txtStatus.Text = $"❌ {result?.Message ?? "Lỗi cập nhật"}";
                    _txtStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }
        catch (Exception ex)
        {
            _txtStatus.Text = $"❌ {ex.Message}";
            _txtStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private class ApiResp
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
    }
}
