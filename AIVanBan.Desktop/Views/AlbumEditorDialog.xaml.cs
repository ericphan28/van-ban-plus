using System.Windows;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class AlbumEditorDialog : Window
{
    private readonly SimpleAlbumService _albumService;
    private readonly SimpleAlbum? _editingAlbum;

    public SimpleAlbum? ResultAlbum { get; private set; }

    /// <summary>
    /// Constructor for creating new album
    /// </summary>
    public AlbumEditorDialog(SimpleAlbum? album, SimpleAlbumService albumService)
    {
        InitializeComponent();
        
        _albumService = albumService;
        _editingAlbum = album;

        if (_editingAlbum != null)
        {
            // Edit mode
            txtHeader.Text = "Chỉnh sửa Album";
            btnSave.Content = "Cập nhật";
            LoadAlbumData();
        }
        else
        {
            // Create mode
            txtHeader.Text = "Tạo Album Mới";
            btnSave.Content = "Tạo Album";
        }
    }

    private void LoadAlbumData()
    {
        if (_editingAlbum != null)
        {
            txtTitle.Text = _editingAlbum.Title;
            txtDescription.Text = _editingAlbum.Description;
            txtTags.Text = string.Join(", ", _editingAlbum.Tags);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(txtTitle.Text))
        {
            MessageBox.Show("Vui lòng nhập tiêu đề album!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtTitle.Focus();
            return;
        }

        try
        {
            // Parse tags
            var tags = txtTags.Text
                .Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            if (_editingAlbum != null)
            {
                // Update existing album
                _editingAlbum.Title = txtTitle.Text.Trim();
                _editingAlbum.Description = txtDescription.Text.Trim();
                _editingAlbum.Tags = tags;
                _albumService.UpdateAlbum(_editingAlbum);
                ResultAlbum = _editingAlbum;

                MessageBox.Show("✅ Đã cập nhật album thành công!", "Thành công", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Create new album
                var newAlbum = _albumService.CreateAlbum(
                    txtTitle.Text.Trim(),
                    txtDescription.Text.Trim(),
                    tags
                );
                ResultAlbum = newAlbum;

                MessageBox.Show($"✅ Đã tạo album thành công!\n\n" +
                    $"Tên: {newAlbum.Title}\n" +
                    $"Thư mục: {newAlbum.FolderPath}", 
                    "Thành công", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu album:\n{ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
