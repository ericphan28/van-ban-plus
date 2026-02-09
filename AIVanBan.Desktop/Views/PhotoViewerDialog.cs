using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AIVanBan.Core.Models;

namespace AIVanBan.Desktop.Views;

public partial class PhotoViewerDialog : Window
{
    public PhotoViewerDialog(Photo photo)
    {
        Title = string.IsNullOrWhiteSpace(photo.Event) ? "Xem ảnh" : photo.Event;
        Width = 900;
        Height = 700;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowState = WindowState.Maximized;

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Image
        var image = new Image
        {
            Stretch = System.Windows.Media.Stretch.Uniform,
            Margin = new Thickness(20)
        };

        if (File.Exists(photo.FilePath))
        {
            try
            {
                var bitmap = new BitmapImage(new Uri(photo.FilePath, UriKind.Absolute));
                image.Source = bitmap;
            }
            catch { }
        }

        Grid.SetRow(image, 0);

        // Info panel
        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Background = System.Windows.Media.Brushes.WhiteSmoke,
            Margin = new Thickness(20)
        };

        infoPanel.Children.Add(new TextBlock 
        { 
            Text = photo.Event, 
            FontSize = 18, 
            FontWeight = FontWeights.Bold 
        });
        
        if (!string.IsNullOrWhiteSpace(photo.Description))
        {
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = photo.Description, 
                Margin = new Thickness(0, 10, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });
        }

        infoPanel.Children.Add(new TextBlock 
        { 
            Text = $"📅 {photo.DateTaken:dd/MM/yyyy HH:mm}", 
            Margin = new Thickness(0, 10, 0, 0),
            Foreground = System.Windows.Media.Brushes.Gray
        });

        if (!string.IsNullOrWhiteSpace(photo.Location))
        {
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = $"📍 {photo.Location}", 
                Margin = new Thickness(0, 5, 0, 0),
                Foreground = System.Windows.Media.Brushes.Gray
            });
        }

        if (photo.Tags.Length > 0)
        {
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = $"🏷️ {string.Join(", ", photo.Tags)}", 
                Margin = new Thickness(0, 5, 0, 0),
                Foreground = System.Windows.Media.Brushes.Gray
            });
        }

        var closeBtn = new Button
        {
            Content = "Đóng",
            Width = 100,
            Margin = new Thickness(0, 15, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        closeBtn.Click += (s, e) => Close();
        infoPanel.Children.Add(closeBtn);

        Grid.SetRow(infoPanel, 1);

        grid.Children.Add(image);
        grid.Children.Add(infoPanel);

        Content = grid;
    }
}
