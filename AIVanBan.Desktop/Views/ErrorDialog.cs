using System;
using System.Windows;
using System.Windows.Controls;

namespace AIVanBan.Desktop.Views;

public class ErrorDialog : Window
{
    private readonly string _errorMessage;
    private readonly string _stackTrace;
    private TextBox txtError = null!;

    public ErrorDialog(string title, string message, string? stackTrace = null)
    {
        Title = title;
        _errorMessage = message;
        _stackTrace = stackTrace ?? string.Empty;
        
        Width = 600;
        Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.CanResize;

        BuildUI();
    }

    private void BuildUI()
    {
        var grid = new Grid { Margin = new Thickness(15) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Icon + Message
        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        Grid.SetRow(headerPanel, 0);

        var icon = new TextBlock
        {
            Text = "⚠️",
            FontSize = 32,
            Margin = new Thickness(0, 0, 15, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        headerPanel.Children.Add(icon);

        var msgText = new TextBlock
        {
            Text = _errorMessage,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(msgText);

        // Error details
        var detailsPanel = new StackPanel();
        Grid.SetRow(detailsPanel, 1);

        if (!string.IsNullOrWhiteSpace(_stackTrace))
        {
            detailsPanel.Children.Add(new TextBlock
            {
                Text = "Chi tiết lỗi:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            });

            txtError = new TextBox
            {
                Text = _stackTrace,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 11,
                Padding = new Thickness(8),
                Background = System.Windows.Media.Brushes.WhiteSmoke
            };
            detailsPanel.Children.Add(txtError);
        }

        // Buttons
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };
        Grid.SetRow(btnPanel, 2);

        if (!string.IsNullOrWhiteSpace(_stackTrace))
        {
            var btnCopy = new Button
            {
                Content = "📋 Copy lỗi",
                Width = 120,
                Height = 35,
                Margin = new Thickness(0, 0, 10, 0),
                FontSize = 14,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80))
            };
            btnCopy.Click += Copy_Click;
            btnPanel.Children.Add(btnCopy);
        }

        var btnClose = new Button
        {
            Content = "Đóng",
            Width = 120,
            Height = 35,
            FontSize = 14
        };
        btnClose.Click += (s, e) => Close();
        btnPanel.Children.Add(btnClose);

        grid.Children.Add(headerPanel);
        grid.Children.Add(detailsPanel);
        grid.Children.Add(btnPanel);

        Content = grid;
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var fullError = $"{_errorMessage}\n\nChi tiết:\n{_stackTrace}";
            Clipboard.SetText(fullError);
            MessageBox.Show("Đã copy lỗi vào clipboard!", "Thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể copy: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static void Show(string title, string message, string? stackTrace = null)
    {
        var dialog = new ErrorDialog(title, message, stackTrace);
        dialog.ShowDialog();
    }
}
