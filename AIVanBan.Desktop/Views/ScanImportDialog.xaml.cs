using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using Microsoft.Win32;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// ViewModel cho mỗi file trong danh sách scan
/// </summary>
public class ScanFileItem : INotifyPropertyChanged
{
    private int _order;
    
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string FileSize { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = "";
    public BitmapImage? Thumbnail { get; set; }
    public Visibility PdfIconVisibility { get; set; } = Visibility.Collapsed;
    
    public int Order
    {
        get => _order;
        set { _order = value; OnPropertyChanged(nameof(Order)); }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class ScanImportDialog : Window
{
    private readonly DocumentService _documentService;
    private readonly GeminiAIService _aiService;
    private readonly ObservableCollection<ScanFileItem> _files = new();
    private GeminiAIService.ExtractedDocumentData? _extractedData;
    
    // Cho chế độ "Tách riêng" — mỗi file → 1 Document
    private List<(GeminiAIService.ExtractedDocumentData Data, string FilePath)> _separateResults = new();
    
    /// <summary>
    /// Văn bản đã được tạo từ scan — dùng cho chế độ "Ghép trang" (1 VB)
    /// </summary>
    public Document? CreatedDocument { get; private set; }
    
    /// <summary>
    /// Danh sách văn bản — dùng cho chế độ "Tách riêng" (nhiều VB)
    /// </summary>
    public List<Document> CreatedDocuments { get; private set; } = new();
    
    /// <summary>
    /// true = "Tách riêng", false = "Ghép trang"
    /// </summary>
    public bool IsSeparateMode => rbSeparate.IsChecked == true;

    private static readonly string[] SupportedExtensions = 
        { ".pdf", ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".webp", ".gif" };

    public ScanImportDialog(DocumentService documentService, string? geminiApiKey = null)
    {
        InitializeComponent();
        _documentService = documentService;
        _aiService = string.IsNullOrEmpty(geminiApiKey) ? new GeminiAIService() : new GeminiAIService(geminiApiKey);
        
        lstFiles.ItemsSource = _files;
        InitializeComboBoxes();
    }
    
    private void InitializeComboBoxes()
    {
        // Loại văn bản — 32 loại theo Điều 7, NĐ 30/2020 + VBQPPL
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
            new { Value = "BienBan", Display = "📋 Biên bản" },
            new { Value = "GiayMoi", Display = "💌 Giấy mời" },
            new { Value = "HopDong", Display = "🤝 Hợp đồng" },
            new { Value = "QuyChE", Display = "📘 Quy chế" },
            new { Value = "QuyDinh", Display = "📘 Quy định" },
            new { Value = "ChuongTrinh", Display = "📋 Chương trình" },
            new { Value = "PhuongAn", Display = "📐 Phương án" },
            new { Value = "DeAn", Display = "📑 Đề án" },
            new { Value = "DuAn", Display = "🏗️ Dự án" },
            new { Value = "CongDien", Display = "⚡ Công điện" },
            new { Value = "ThongCao", Display = "📢 Thông cáo" },
            new { Value = "BanGhiNho", Display = "📝 Bản ghi nhớ" },
            new { Value = "BanThoaThuan", Display = "🤝 Bản thỏa thuận" },
            new { Value = "GiayUyQuyen", Display = "📜 Giấy ủy quyền" },
            new { Value = "GiayGioiThieu", Display = "📨 Giấy giới thiệu" },
            new { Value = "GiayNghiPhep", Display = "🏖️ Giấy nghỉ phép" },
            new { Value = "PhieuGui", Display = "📨 Phiếu gửi" },
            new { Value = "PhieuChuyen", Display = "📨 Phiếu chuyển" },
            new { Value = "PhieuBao", Display = "📨 Phiếu báo" },
            new { Value = "ThuCong", Display = "✉️ Thư công" },
            new { Value = "Luat", Display = "⚖️ Luật" },
            new { Value = "NghiDinh", Display = "📕 Nghị định" },
            new { Value = "ThongTu", Display = "📗 Thông tư" },
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

        // Mức độ khẩn
        var urgencies = new[]
        {
            new { Value = "Thuong", Display = "⚪ Thường" },
            new { Value = "Khan", Display = "🟡 Khẩn" },
            new { Value = "ThuongKhan", Display = "🟠 Thượng khẩn" },
            new { Value = "HoaToc", Display = "🔴 Hỏa tốc" }
        };
        cboDoKhan.ItemsSource = urgencies;
        cboDoKhan.DisplayMemberPath = "Display";
        cboDoKhan.SelectedValuePath = "Value";
        cboDoKhan.SelectedIndex = 0;

        // Độ mật
        var securities = new[]
        {
            new { Value = "Thuong", Display = "⚪ Thường" },
            new { Value = "Mat", Display = "🟡 Mật" },
            new { Value = "ToiMat", Display = "🟠 Tối mật" },
            new { Value = "TuyetMat", Display = "🔴 Tuyệt mật" }
        };
        cboDoMat.ItemsSource = securities;
        cboDoMat.DisplayMemberPath = "Display";
        cboDoMat.SelectedValuePath = "Value";
        cboDoMat.SelectedIndex = 0;
    }

    #region File management — Add, Remove, Reorder

    private void ChooseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn file PDF hoặc ảnh scan (có thể chọn nhiều file)",
            Filter = "File hỗ trợ|*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.tif;*.webp;*.gif|" +
                     "PDF|*.pdf|" +
                     "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.tif;*.webp;*.gif|" +
                     "Tất cả|*.*",
            Multiselect = true
        };
        
        if (dialog.ShowDialog() == true)
        {
            AddFiles(dialog.FileNames);
        }
    }
    
    private void AddFiles(string[] filePaths)
    {
        foreach (var path in filePaths)
        {
            var ext = Path.GetExtension(path).ToLower();
            if (!SupportedExtensions.Contains(ext))
            {
                MessageBox.Show($"File không được hỗ trợ: {Path.GetFileName(path)}\n\n" +
                    "Hỗ trợ: PDF, JPG, PNG, BMP, TIFF, WebP, GIF",
                    "Bỏ qua file", MessageBoxButton.OK, MessageBoxImage.Warning);
                continue;
            }
            
            // Kiểm tra trùng lặp
            if (_files.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                continue;
            
            var fileInfo = new FileInfo(path);
            var sizeText = fileInfo.Length < 1024 * 1024
                ? $"{fileInfo.Length / 1024} KB"
                : $"{fileInfo.Length / (1024.0 * 1024):F1} MB";
            
            var mimeType = ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                _ => "application/octet-stream"
            };
            
            var item = new ScanFileItem
            {
                FilePath = path,
                FileName = Path.GetFileName(path),
                FileSize = sizeText,
                FileSizeBytes = fileInfo.Length,
                MimeType = mimeType,
                Order = _files.Count + 1,
                PdfIconVisibility = ext == ".pdf" ? Visibility.Visible : Visibility.Collapsed,
            };
            
            // Tạo thumbnail cho ảnh
            if (ext != ".pdf")
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 80;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    item.Thumbnail = bitmap;
                }
                catch { /* Không tạo được thumbnail — bỏ qua */ }
            }
            
            _files.Add(item);
        }
        
        UpdateFileListUI();
    }
    
    private void RemoveFile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string filePath)
        {
            var item = _files.FirstOrDefault(f => f.FilePath == filePath);
            if (item != null)
            {
                _files.Remove(item);
                RenumberFiles();
                UpdateFileListUI();
            }
        }
    }
    
    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
        if (_files.Count == 0) return;
        
        var result = MessageBox.Show($"Xóa tất cả {_files.Count} file khỏi danh sách?",
            "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _files.Clear();
            UpdateFileListUI();
        }
    }
    
    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        var idx = lstFiles.SelectedIndex;
        if (idx <= 0) return;
        
        _files.Move(idx, idx - 1);
        RenumberFiles();
        lstFiles.SelectedIndex = idx - 1;
    }
    
    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        var idx = lstFiles.SelectedIndex;
        if (idx < 0 || idx >= _files.Count - 1) return;
        
        _files.Move(idx, idx + 1);
        RenumberFiles();
        lstFiles.SelectedIndex = idx + 1;
    }
    
    private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Bật/tắt nút Move tùy vị trí chọn
        var idx = lstFiles.SelectedIndex;
        btnMoveUp.IsEnabled = idx > 0;
        btnMoveDown.IsEnabled = idx >= 0 && idx < _files.Count - 1;
    }
    
    private void RenumberFiles()
    {
        for (int i = 0; i < _files.Count; i++)
            _files[i].Order = i + 1;
    }
    
    private void UpdateFileListUI()
    {
        var hasFiles = _files.Count > 0;
        emptyState.Visibility = hasFiles ? Visibility.Collapsed : Visibility.Visible;
        lstFiles.Visibility = hasFiles ? Visibility.Visible : Visibility.Collapsed;
        pnlMoveButtons.Visibility = hasFiles ? Visibility.Visible : Visibility.Collapsed;
        btnAnalyze.IsEnabled = hasFiles;
        
        txtFileCount.Text = $"{_files.Count} file";
        
        var totalSize = _files.Sum(f => f.FileSizeBytes);
        var totalSizeText = totalSize < 1024 * 1024
            ? $"{totalSize / 1024} KB"
            : $"{totalSize / (1024.0 * 1024):F1} MB";
        txtFooterInfo.Text = hasFiles ? $"Tổng: {_files.Count} file, {totalSizeText}" : "";
        
        // Reset extraction state khi thay đổi file
        _extractedData = null;
        _separateResults.Clear();
        btnSave.IsEnabled = false;
        txtExtractionStatus.Text = hasFiles ? "Sẵn sàng phân tích" : "";
    }

    #endregion

    #region Drag and drop

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
    
    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                AddFiles(files);
            }
        }
    }

    #endregion

    #region Mode selector

    private void ScanMode_Changed(object sender, RoutedEventArgs e)
    {
        if (txtModeDescription == null) return; // InitializeComponent chưa xong
        
        if (rbMerge.IsChecked == true)
        {
            txtModeDescription.Text = "Ghép tất cả ảnh thành 1 văn bản (VD: scan VB nhiều trang)";
            txtSaveButton.Text = "Lưu văn bản vào hệ thống";
        }
        else
        {
            txtModeDescription.Text = "Mỗi ảnh/PDF = 1 văn bản riêng biệt (batch import)";
            txtSaveButton.Text = $"Lưu {_files.Count} văn bản vào hệ thống";
        }
        
        // Reset extraction khi đổi mode
        _extractedData = null;
        _separateResults.Clear();
        btnSave.IsEnabled = false;
    }

    #endregion

    #region AI Analysis

    private async void Analyze_Click(object sender, RoutedEventArgs e)
    {
        if (_files.Count == 0) return;
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;
        
        // Validate file sizes
        foreach (var file in _files)
        {
            var sizeMB = file.FileSizeBytes / (1024.0 * 1024.0);
            if (sizeMB > 20)
            {
                MessageBox.Show(
                    $"📁 File quá lớn: {file.FileName} ({sizeMB:F1} MB)\n\n" +
                    "AI hỗ trợ tối đa 20MB mỗi file.\n" +
                    "Hãy xóa file này khỏi danh sách hoặc giảm kích thước.",
                    "File quá lớn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        
        // Cảnh báo tổng dung lượng lớn cho chế độ ghép
        if (!IsSeparateMode)
        {
            var totalMB = _files.Sum(f => f.FileSizeBytes) / (1024.0 * 1024.0);
            if (totalMB > 15)
            {
                var result = MessageBox.Show(
                    $"📁 Tổng dung lượng khá lớn ({totalMB:F1} MB cho {_files.Count} file)\n\n" +
                    "Chế độ \"Ghép trang\" gửi tất cả file cùng lúc.\n" +
                    "File quá lớn có thể bị từ chối hoặc timeout.\n\n" +
                    "💡 Gợi ý: Chuyển sang chế độ \"Tách riêng\" để xử lý từng file.\n\n" +
                    "Bạn vẫn muốn tiếp tục?",
                    "Cảnh báo dung lượng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }
        }
        
        // Disable UI
        btnAnalyze.IsEnabled = false;
        btnChooseFile.IsEnabled = false;
        btnSave.IsEnabled = false;
        txtAnalyzeButton.Text = "⏳ Đang xử lý...";
        
        try
        {
            if (IsSeparateMode)
                await AnalyzeSeparateAsync();
            else
                await AnalyzeMergeAsync();
        }
        catch (Exception ex)
        {
            loadingPanel.Visibility = Visibility.Collapsed;
            batchPanel.Visibility = Visibility.Collapsed;
            txtExtractionStatus.Text = "❌ Lỗi phân tích";
            ShowAnalysisError(ex);
        }
        finally
        {
            btnAnalyze.IsEnabled = true;
            btnChooseFile.IsEnabled = true;
            txtAnalyzeButton.Text = "🤖 Phân tích bằng AI";
        }
    }

    /// <summary>
    /// Chế độ "Ghép trang" — gửi nhiều ảnh trong 1 request AI → 1 văn bản
    /// </summary>
    private async Task AnalyzeMergeAsync()
    {
        loadingPanel.Visibility = Visibility.Visible;
        batchPanel.Visibility = Visibility.Collapsed;
        txtExtractionStatus.Text = $"⏳ Đang phân tích {_files.Count} file (ghép trang)...";
        
        var elapsed = 0;
        var progressTimer = CreateProgressTimer(ref elapsed);
        
        try
        {
            progressTimer.Start();
            txtLoadingStatus.Text = $"🤖 Đang gửi {_files.Count} file lên AI...";
            txtLoadingDetail.Text = $"Chế độ ghép trang — {_files.Count} ảnh → 1 văn bản";
            
            // Đọc tất cả file → base64
            var fileDataList = new List<(string Base64, string MimeType)>();
            for (int i = 0; i < _files.Count; i++)
            {
                txtLoadingStatus.Text = $"📂 Đang đọc file {i + 1}/{_files.Count}: {_files[i].FileName}";
                var bytes = await File.ReadAllBytesAsync(_files[i].FilePath);
                var base64 = Convert.ToBase64String(bytes);
                fileDataList.Add((base64, _files[i].MimeType));
            }
            
            txtLoadingStatus.Text = $"🤖 Đang gửi {_files.Count} file lên AI...";
            
            // Gọi AI với nhiều ảnh cùng lúc
            _extractedData = await _aiService.ExtractDocumentFromMultipleFilesAsync(fileDataList);
            
            progressTimer.Stop();
            txtLoadingStatus.Text = $"✅ Phân tích hoàn tất sau {elapsed}s!";
            await System.Threading.Tasks.Task.Delay(500);
            
            PopulateForm(_extractedData);
            
            loadingPanel.Visibility = Visibility.Collapsed;
            btnSave.IsEnabled = true;
            txtExtractionStatus.Text = $"✅ Ghép {_files.Count} trang → 1 văn bản — Kiểm tra và chỉnh sửa";
            txtFooterInfo.Text = $"✅ Trích xuất thành công ({elapsed}s) | {_files.Count} file ghép";
        }
        finally
        {
            progressTimer.Stop();
        }
    }

    /// <summary>
    /// Chế độ "Tách riêng" — xử lý từng file → nhiều văn bản
    /// </summary>
    private async Task AnalyzeSeparateAsync()
    {
        loadingPanel.Visibility = Visibility.Collapsed;
        batchPanel.Visibility = Visibility.Visible;
        txtExtractionStatus.Text = $"⏳ Đang xử lý {_files.Count} file (tách riêng)...";
        
        _separateResults.Clear();
        var errors = new List<string>();
        
        for (int i = 0; i < _files.Count; i++)
        {
            var file = _files[i];
            var progress = (int)((i + 1.0) / _files.Count * 100);
            
            pbBatch.Value = (int)(i * 100.0 / _files.Count);
            txtBatchStatus.Text = $"📑 Đang xử lý file {i + 1}/{_files.Count}...";
            txtBatchDetail.Text = $"File: {file.FileName} ({file.FileSize})";
            
            try
            {
                var data = await _aiService.ExtractDocumentFromFileAsync(file.FilePath);
                _separateResults.Add((data, file.FilePath));
            }
            catch (Exception ex)
            {
                errors.Add($"❌ {file.FileName}: {ex.Message}");
            }
        }
        
        pbBatch.Value = 100;
        
        if (_separateResults.Count > 0)
        {
            // Hiển thị kết quả file đầu tiên trong form
            PopulateForm(_separateResults[0].Data);
            
            txtBatchStatus.Text = $"✅ Hoàn tất: {_separateResults.Count}/{_files.Count} file thành công";
            txtBatchDetail.Text = errors.Count > 0 
                ? $"⚠️ {errors.Count} file lỗi — Bấm Lưu để lưu {_separateResults.Count} VB thành công"
                : $"Bấm Lưu để lưu {_separateResults.Count} văn bản vào hệ thống";
            
            btnSave.IsEnabled = true;
            txtSaveButton.Text = $"Lưu {_separateResults.Count} văn bản vào hệ thống";
            txtExtractionStatus.Text = $"✅ {_separateResults.Count} VB — Form hiển thị VB đầu tiên (tham khảo)";
            
            if (errors.Count > 0)
            {
                MessageBox.Show(
                    $"⚠️ {errors.Count} file không xử lý được:\n\n" + string.Join("\n", errors),
                    "Có lỗi một số file", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            txtBatchStatus.Text = "❌ Không có file nào xử lý được";
            txtBatchDetail.Text = string.Join("\n", errors.Take(3));
            txtExtractionStatus.Text = "❌ Lỗi tất cả file";
        }
    }
    
    private System.Windows.Threading.DispatcherTimer CreateProgressTimer(ref int elapsed)
    {
        var elapsedRef = elapsed; // capture
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        var localElapsed = 0;
        timer.Tick += (s, args) =>
        {
            localElapsed++;
            var statusText = localElapsed switch
            {
                <= 10 => $"🤖 Đang gửi file lên máy chủ AI... ({localElapsed}s)",
                <= 30 => $"🔍 AI đang đọc và phân tích văn bản... ({localElapsed}s)",
                <= 60 => $"📝 AI đang trích xuất nội dung chi tiết... ({localElapsed}s)",
                <= 120 => $"⏳ File lớn — AI cần thêm thời gian... ({localElapsed}s)",
                <= 180 => $"🔄 Đang chờ phản hồi từ máy chủ AI... ({localElapsed}s)",
                _ => $"⏳ Vẫn đang xử lý, xin kiên nhẫn... ({localElapsed}s)"
            };
            txtLoadingStatus.Text = statusText;
        };
        return timer;
    }
    
    private void ShowAnalysisError(Exception ex)
    {
        var msg = ex.Message + (ex.InnerException?.Message ?? "");
        string errorTitle;
        string errorDetail;
        
        if (msg.Contains("413") || msg.Contains("Entity Too Large") || msg.Contains("Payload Too Large"))
        {
            errorTitle = "File quá lớn";
            errorDetail = "📁 File vượt quá giới hạn của máy chủ.\n\n" +
                "💡 Cách khắc phục:\n" +
                "  • Chuyển sang chế độ \"Tách riêng\" để gửi từng file\n" +
                "  • Giảm dung lượng file (nén PDF, giảm độ phân giải)\n" +
                "  • Bớt số file trong danh sách\n\n" +
                "📌 Khuyến nghị: Mỗi file dưới 3MB sẽ xử lý nhanh nhất.";
        }
        else if (msg.Contains("Timeout") || msg.Contains("timeout") || msg.Contains("Không thể trích xuất sau"))
        {
            errorTitle = "Quá thời gian chờ";
            errorDetail = "⏰ AI không phản hồi kịp thời.\n\n" +
                "💡 Gợi ý:\n" +
                "  • Thử lại sau ít phút\n" +
                "  • Dùng chế độ \"Tách riêng\" cho nhiều file\n" +
                "  • Giảm dung lượng hoặc số file";
        }
        else if (msg.Contains("401") || msg.Contains("Unauthorized") || msg.Contains("API key"))
        {
            errorTitle = "Lỗi xác thực";
            errorDetail = "🔑 Phiên đăng nhập đã hết hạn hoặc API key không hợp lệ.\n\nHãy đăng xuất và đăng nhập lại.";
        }
        else if (msg.Contains("429") || msg.Contains("quota") || msg.Contains("rate"))
        {
            errorTitle = "Hết credit AI";
            errorDetail = "📊 Đã hết credit AI của tháng này.\nNâng cấp gói tại vanbanplus.giakiemso.com/bang-gia để có thêm credit.";
        }
        else if (msg.Contains("No such host") || msg.Contains("network") || msg.Contains("SocketException"))
        {
            errorTitle = "Lỗi kết nối";
            errorDetail = "🌐 Không thể kết nối đến máy chủ.\nKiểm tra kết nối Internet và thử lại.";
        }
        else
        {
            errorTitle = "Lỗi phân tích";
            errorDetail = $"Không thể phân tích file.\n\nChi tiết: {ex.Message}\n\nHãy thử lại hoặc chọn file khác.";
        }
        
        MessageBox.Show(errorDetail, errorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    #endregion

    #region Form Population
    
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
        
        // Map độ khẩn
        if (!string.IsNullOrEmpty(data.DoKhan))
        {
            for (int i = 0; i < cboDoKhan.Items.Count; i++)
            {
                var item = cboDoKhan.Items[i];
                var value = item?.GetType().GetProperty("Value")?.GetValue(item)?.ToString();
                if (value != null && value.Equals(data.DoKhan, StringComparison.OrdinalIgnoreCase))
                {
                    cboDoKhan.SelectedIndex = i;
                    break;
                }
            }
        }
        
        // Map độ mật
        if (!string.IsNullOrEmpty(data.DoMat))
        {
            for (int i = 0; i < cboDoMat.Items.Count; i++)
            {
                var item = cboDoMat.Items[i];
                var value = item?.GetType().GetProperty("Value")?.GetValue(item)?.ToString();
                if (value != null && value.Equals(data.DoMat, StringComparison.OrdinalIgnoreCase))
                {
                    cboDoMat.SelectedIndex = i;
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

    #endregion

    #region Save

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (IsSeparateMode)
            SaveSeparate();
        else
            SaveMerge();
    }
    
    /// <summary>
    /// Lưu 1 văn bản (chế độ Ghép trang) — lấy dữ liệu từ form
    /// </summary>
    private void SaveMerge()
    {
        if (string.IsNullOrWhiteSpace(txtTrichYeu.Text) && string.IsNullOrWhiteSpace(txtNoiDung.Text))
        {
            MessageBox.Show("Cần ít nhất Trích yếu hoặc Nội dung để lưu văn bản.",
                "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var doc = BuildDocumentFromForm();
        // Gán tất cả file paths (lưu file đầu tiên vào FilePath chính)
        doc.FilePath = _files.FirstOrDefault()?.FilePath ?? "";
        
        CreatedDocument = doc;
        CreatedDocuments = new List<Document> { doc };
        DialogResult = true;
        Close();
    }
    
    /// <summary>
    /// Lưu nhiều văn bản (chế độ Tách riêng) — mỗi file → 1 Document từ AI
    /// </summary>
    private void SaveSeparate()
    {
        if (_separateResults.Count == 0)
        {
            MessageBox.Show("Chưa có dữ liệu trích xuất.\nHãy phân tích bằng AI trước.",
                "Chưa phân tích", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        CreatedDocuments = new List<Document>();
        
        foreach (var (data, filePath) in _separateResults)
        {
            var doc = BuildDocumentFromData(data);
            doc.FilePath = filePath;
            CreatedDocuments.Add(doc);
        }
        
        CreatedDocument = CreatedDocuments.FirstOrDefault();
        DialogResult = true;
        Close();
    }
    
    /// <summary>
    /// Tạo Document từ dữ liệu form (dùng cho chế độ Ghép)
    /// </summary>
    private Document BuildDocumentFromForm()
    {
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
        };
        
        ApplyComboBoxValues(doc);
        ParseRecipientsAndBasis(doc, txtNoiNhan.Text, txtCanCu.Text);
        return doc;
    }
    
    /// <summary>
    /// Tạo Document từ ExtractedDocumentData (dùng cho chế độ Tách riêng)
    /// </summary>
    private Document BuildDocumentFromData(GeminiAIService.ExtractedDocumentData data)
    {
        var doc = new Document
        {
            Number = data.SoVanBan,
            Title = !string.IsNullOrWhiteSpace(data.TrichYeu) ? data.TrichYeu : data.SoVanBan,
            Subject = data.TrichYeu,
            Issuer = data.CoQuanBanHanh,
            Content = data.NoiDung,
            Category = data.LinhVuc,
            SignedBy = data.NguoiKy,
            SigningTitle = data.ChucDanhKy,
            SigningAuthority = data.ThamQuyenKy,
            Location = data.DiaDanh,
        };
        
        // Parse date
        if (!string.IsNullOrEmpty(data.NgayBanHanh) &&
            DateTime.TryParseExact(data.NgayBanHanh, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            doc.IssueDate = date;
        }
        else
        {
            doc.IssueDate = DateTime.Now;
        }
        
        // Type
        if (!string.IsNullOrEmpty(data.LoaiVanBan) && Enum.TryParse<DocumentType>(data.LoaiVanBan, out var docType))
            doc.Type = docType;
        
        // Direction
        if (!string.IsNullOrEmpty(data.HuongVanBan) && Enum.TryParse<Direction>(data.HuongVanBan, out var dir))
            doc.Direction = dir;
        
        // Urgency
        var urgencyMap = new Dictionary<string, UrgencyLevel>(StringComparer.OrdinalIgnoreCase)
        {
            ["Thuong"] = UrgencyLevel.Thuong, ["Khan"] = UrgencyLevel.Khan,
            ["ThuongKhan"] = UrgencyLevel.ThuongKhan, ["HoaToc"] = UrgencyLevel.HoaToc
        };
        if (!string.IsNullOrEmpty(data.DoKhan) && urgencyMap.TryGetValue(data.DoKhan, out var urgency))
            doc.UrgencyLevel = urgency;
        
        // Security
        var securityMap = new Dictionary<string, SecurityLevel>(StringComparer.OrdinalIgnoreCase)
        {
            ["Thuong"] = SecurityLevel.Thuong, ["Mat"] = SecurityLevel.Mat,
            ["ToiMat"] = SecurityLevel.ToiMat, ["TuyetMat"] = SecurityLevel.TuyetMat
        };
        if (!string.IsNullOrEmpty(data.DoMat) && securityMap.TryGetValue(data.DoMat, out var security))
            doc.SecurityLevel = security;
        
        // Recipients & Basis
        if (data.NoiNhan.Length > 0)
            doc.Recipients = data.NoiNhan;
        if (data.CanCu.Length > 0)
            doc.BasedOn = data.CanCu;
        
        return doc;
    }
    
    private void ApplyComboBoxValues(Document doc)
    {
        var typeValue = cboLoaiVanBan.SelectedValue?.ToString() ?? "Khac";
        if (Enum.TryParse<DocumentType>(typeValue, out var docType))
            doc.Type = docType;
        
        var dirValue = cboHuongVanBan.SelectedValue?.ToString() ?? "Den";
        if (Enum.TryParse<Direction>(dirValue, out var dir))
            doc.Direction = dir;
        
        var urgencyMap = new Dictionary<string, UrgencyLevel>(StringComparer.OrdinalIgnoreCase)
        {
            ["Thuong"] = UrgencyLevel.Thuong, ["Khan"] = UrgencyLevel.Khan,
            ["ThuongKhan"] = UrgencyLevel.ThuongKhan, ["HoaToc"] = UrgencyLevel.HoaToc
        };
        var urgencyValue = cboDoKhan.SelectedValue?.ToString() ?? "Thuong";
        if (urgencyMap.TryGetValue(urgencyValue, out var urgency))
            doc.UrgencyLevel = urgency;
        
        var securityMap = new Dictionary<string, SecurityLevel>(StringComparer.OrdinalIgnoreCase)
        {
            ["Thuong"] = SecurityLevel.Thuong, ["Mat"] = SecurityLevel.Mat,
            ["ToiMat"] = SecurityLevel.ToiMat, ["TuyetMat"] = SecurityLevel.TuyetMat
        };
        var securityValue = cboDoMat.SelectedValue?.ToString() ?? "Thuong";
        if (securityMap.TryGetValue(securityValue, out var security))
            doc.SecurityLevel = security;
    }
    
    private void ParseRecipientsAndBasis(Document doc, string noiNhanText, string canCuText)
    {
        if (!string.IsNullOrWhiteSpace(noiNhanText))
            doc.Recipients = noiNhanText.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        
        if (!string.IsNullOrWhiteSpace(canCuText))
            doc.BasedOn = canCuText.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
    }

    #endregion

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
