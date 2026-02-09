using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using Microsoft.Win32;

namespace AIVanBan.Desktop.Views;

public partial class ScanImportDialog : Window
{
    private readonly DocumentService _documentService;
    private readonly GeminiAIService _aiService;
    private string? _selectedFilePath;
    private GeminiAIService.ExtractedDocumentData? _extractedData;
    
    /// <summary>
    /// Văn bản đã được tạo từ scan (null nếu user hủy)
    /// </summary>
    public Document? CreatedDocument { get; private set; }

    public ScanImportDialog(DocumentService documentService, string? geminiApiKey = null)
    {
        InitializeComponent();
        _documentService = documentService;
        _aiService = string.IsNullOrEmpty(geminiApiKey) ? new GeminiAIService() : new GeminiAIService(geminiApiKey);
        
        InitializeComboBoxes();
    }
    
    private void InitializeComboBoxes()
    {
        // Loại văn bản
        var docTypes = new[]
        {
            new { Value = "CongVan", Display = "📨 Công văn" },
            new { Value = "QuyetDinh", Display = "📋 Quyết định" },
            new { Value = "BaoCao", Display = "📊 Báo cáo" },
            new { Value = "ToTrinh", Display = "📄 Tờ trình" },
            new { Value = "KeHoach", Display = "📅 Kế hoạch" },
            new { Value = "ThongBao", Display = "📌 Thông báo" },
            new { Value = "NghiQuyet", Display = "📜 Nghị quyết" },
            new { Value = "ChiThi", Display = "🔖 Chỉ thị" },
            new { Value = "HuongDan", Display = "📝 Hướng dẫn" },
            new { Value = "Luat", Display = "⚖️ Luật" },
            new { Value = "NghiDinh", Display = "📕 Nghị định" },
            new { Value = "ThongTu", Display = "📗 Thông tư" },
            new { Value = "QuyDinh", Display = "📘 Quy định" },
            new { Value = "Khac", Display = "📎 Khác" }
        };
        cboLoaiVanBan.ItemsSource = docTypes;
        cboLoaiVanBan.DisplayMemberPath = "Display";
        cboLoaiVanBan.SelectedValuePath = "Value";
        cboLoaiVanBan.SelectedIndex = 0;
        
        // Hướng văn bản
        var directions = new[]
        {
            new { Value = "Den", Display = "📥 Văn bản đến" },
            new { Value = "Di", Display = "📤 Văn bản đi" },
            new { Value = "NoiBo", Display = "🔄 Nội bộ" }
        };
        cboHuongVanBan.ItemsSource = directions;
        cboHuongVanBan.DisplayMemberPath = "Display";
        cboHuongVanBan.SelectedValuePath = "Value";
        cboHuongVanBan.SelectedIndex = 0;
    }

    private void ChooseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn file PDF hoặc ảnh scan",
            Filter = "File hỗ trợ|*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.tif;*.webp;*.gif|" +
                     "PDF|*.pdf|" +
                     "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.tif;*.webp;*.gif|" +
                     "Tất cả|*.*"
        };
        
        if (dialog.ShowDialog() == true)
        {
            _selectedFilePath = dialog.FileName;
            ShowFilePreview();
            btnAnalyze.IsEnabled = true;
            txtExtractionStatus.Text = "Sẵn sàng phân tích";
        }
    }
    
    private void ShowFilePreview()
    {
        if (string.IsNullOrEmpty(_selectedFilePath)) return;
        
        var ext = Path.GetExtension(_selectedFilePath).ToLower();
        emptyState.Visibility = Visibility.Collapsed;
        
        if (ext == ".pdf")
        {
            // Show PDF info
            previewScroll.Visibility = Visibility.Collapsed;
            pdfPreview.Visibility = Visibility.Visible;
            
            txtPdfFileName.Text = Path.GetFileName(_selectedFilePath);
            var fileInfo = new FileInfo(_selectedFilePath);
            var sizeText = fileInfo.Length < 1024 * 1024
                ? $"{fileInfo.Length / 1024} KB"
                : $"{fileInfo.Length / (1024.0 * 1024):F1} MB";
            txtPdfFileSize.Text = sizeText;
        }
        else
        {
            // Show image preview
            pdfPreview.Visibility = Visibility.Collapsed;
            previewScroll.Visibility = Visibility.Visible;
            
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_selectedFilePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 600; // Limit memory usage
                bitmap.EndInit();
                imgPreview.Source = bitmap;
            }
            catch
            {
                // Fallback to PDF-like display
                previewScroll.Visibility = Visibility.Collapsed;
                pdfPreview.Visibility = Visibility.Visible;
                txtPdfFileName.Text = Path.GetFileName(_selectedFilePath);
                txtPdfFileSize.Text = "Không thể xem trước";
            }
        }
        
        txtFooterInfo.Text = $"File: {_selectedFilePath}";
    }

    private async void Analyze_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedFilePath)) return;
        
        // Check file size (Gemini inline limit ~20MB)
        var fileInfo = new FileInfo(_selectedFilePath);
        if (fileInfo.Length > 20 * 1024 * 1024)
        {
            MessageBox.Show(
                "File quá lớn (> 20MB). Gemini Vision hỗ trợ tối đa 20MB cho inline upload.\n\n" +
                "Hãy giảm kích thước file hoặc chia thành nhiều file nhỏ hơn.",
                "File quá lớn", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        // Start analysis
        btnAnalyze.IsEnabled = false;
        btnChooseFile.IsEnabled = false;
        btnSave.IsEnabled = false;
        loadingPanel.Visibility = Visibility.Visible;
        txtExtractionStatus.Text = "⏳ Đang phân tích...";
        txtAnalyzeButton.Text = "⏳ Đang xử lý...";
        
        try
        {
            txtLoadingStatus.Text = "🤖 Đang gửi file lên Gemini AI Vision...";
            
            _extractedData = await _aiService.ExtractDocumentFromFileAsync(_selectedFilePath);
            
            txtLoadingStatus.Text = "✅ Phân tích hoàn tất! Đang điền dữ liệu...";
            await System.Threading.Tasks.Task.Delay(500); // Brief visual feedback
            
            // Populate form
            PopulateForm(_extractedData);
            
            loadingPanel.Visibility = Visibility.Collapsed;
            btnSave.IsEnabled = true;
            txtExtractionStatus.Text = "✅ Đã trích xuất — Kiểm tra và chỉnh sửa nếu cần";
            txtFooterInfo.Text = $"✅ Trích xuất thành công | File: {Path.GetFileName(_selectedFilePath)}";
        }
        catch (Exception ex)
        {
            loadingPanel.Visibility = Visibility.Collapsed;
            txtExtractionStatus.Text = "❌ Lỗi phân tích";
            
            MessageBox.Show(
                $"Lỗi khi phân tích file:\n\n{ex.Message}\n\n" +
                "Hãy thử lại hoặc chọn file khác.",
                "Lỗi AI", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnAnalyze.IsEnabled = true;
            btnChooseFile.IsEnabled = true;
            txtAnalyzeButton.Text = "🤖 Phân tích bằng AI";
        }
    }
    
    private void PopulateForm(GeminiAIService.ExtractedDocumentData data)
    {
        txtSoVanBan.Text = data.SoVanBan;
        txtTrichYeu.Text = data.TrichYeu;
        txtCoQuanBanHanh.Text = data.CoQuanBanHanh;
        txtNguoiKy.Text = data.NguoiKy;
        txtNoiDung.Text = data.NoiDung;
        txtLinhVuc.Text = data.LinhVuc;
        txtDiaDanh.Text = data.DiaDanh;
        txtChucDanhKy.Text = data.ChucDanhKy;
        txtThamQuyenKy.Text = data.ThamQuyenKy;
        
        // Parse date
        if (!string.IsNullOrEmpty(data.NgayBanHanh))
        {
            if (DateTime.TryParseExact(data.NgayBanHanh, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" }, 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                dpNgayBanHanh.SelectedDate = date;
            }
        }
        
        // Map loại văn bản
        if (!string.IsNullOrEmpty(data.LoaiVanBan))
        {
            for (int i = 0; i < cboLoaiVanBan.Items.Count; i++)
            {
                var item = cboLoaiVanBan.Items[i];
                var value = item?.GetType().GetProperty("Value")?.GetValue(item)?.ToString();
                if (value != null && value.Equals(data.LoaiVanBan, StringComparison.OrdinalIgnoreCase))
                {
                    cboLoaiVanBan.SelectedIndex = i;
                    break;
                }
            }
        }
        
        // Map hướng VB
        if (!string.IsNullOrEmpty(data.HuongVanBan))
        {
            for (int i = 0; i < cboHuongVanBan.Items.Count; i++)
            {
                var item = cboHuongVanBan.Items[i];
                var value = item?.GetType().GetProperty("Value")?.GetValue(item)?.ToString();
                if (value != null && value.Equals(data.HuongVanBan, StringComparison.OrdinalIgnoreCase))
                {
                    cboHuongVanBan.SelectedIndex = i;
                    break;
                }
            }
        }
        
        // Căn cứ + Nơi nhận
        if (data.CanCu.Length > 0)
            txtCanCu.Text = string.Join("\n", data.CanCu);
        
        if (data.NoiNhan.Length > 0)
            txtNoiNhan.Text = string.Join("\n", data.NoiNhan);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validate minimum data
        if (string.IsNullOrWhiteSpace(txtTrichYeu.Text) && string.IsNullOrWhiteSpace(txtNoiDung.Text))
        {
            MessageBox.Show("Cần ít nhất Trích yếu hoặc Nội dung để lưu văn bản.",
                "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        // Build Document
        var doc = new Document
        {
            Number = txtSoVanBan.Text.Trim(),
            Title = !string.IsNullOrWhiteSpace(txtTrichYeu.Text) 
                ? txtTrichYeu.Text.Trim() 
                : txtSoVanBan.Text.Trim(),
            Subject = txtTrichYeu.Text.Trim(),
            IssueDate = dpNgayBanHanh.SelectedDate ?? DateTime.Now,
            Issuer = txtCoQuanBanHanh.Text.Trim(),
            Content = txtNoiDung.Text.Trim(),
            Category = txtLinhVuc.Text.Trim(),
            SignedBy = txtNguoiKy.Text.Trim(),
            SigningTitle = txtChucDanhKy.Text.Trim(),
            SigningAuthority = txtThamQuyenKy.Text.Trim(),
            Location = txtDiaDanh.Text.Trim(),
            FilePath = _selectedFilePath ?? "",
        };
        
        // Parse type
        var typeValue = cboLoaiVanBan.SelectedValue?.ToString() ?? "Khac";
        if (Enum.TryParse<DocumentType>(typeValue, out var docType))
            doc.Type = docType;
        
        // Parse direction
        var dirValue = cboHuongVanBan.SelectedValue?.ToString() ?? "Den";
        if (Enum.TryParse<Direction>(dirValue, out var dir))
            doc.Direction = dir;
        
        // Parse recipients
        if (!string.IsNullOrWhiteSpace(txtNoiNhan.Text))
            doc.Recipients = txtNoiNhan.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        
        // Parse căn cứ
        if (!string.IsNullOrWhiteSpace(txtCanCu.Text))
            doc.BasedOn = txtCanCu.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        
        CreatedDocument = doc;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
