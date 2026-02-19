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
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;
        
        // Check file size (Gemini inline limit ~20MB)
        var fileInfo = new FileInfo(_selectedFilePath);
        if (fileInfo.Length > 20 * 1024 * 1024)
        {
            MessageBox.Show(
                "File quá lớn (> 20MB). AI hỗ trợ tối đa 20MB mỗi file.\n\n" +
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
        
        // Timer đếm thời gian chờ
        var elapsed = 0;
        var progressTimer = new System.Windows.Threading.DispatcherTimer();
        progressTimer.Interval = TimeSpan.FromSeconds(1);
        progressTimer.Tick += (s, args) =>
        {
            elapsed++;
            var statusText = elapsed switch
            {
                <= 10 => $"🤖 Đang gửi file lên Gemini AI... ({elapsed}s)",
                <= 30 => $"🔍 AI đang đọc và phân tích văn bản... ({elapsed}s)",
                <= 60 => $"📝 AI đang trích xuất nội dung chi tiết... ({elapsed}s)",
                <= 120 => $"⏳ File lớn — AI cần thêm thời gian... ({elapsed}s)",
                <= 180 => $"🔄 Đang chờ phản hồi từ Gemini... ({elapsed}s)",
                _ => $"⏳ Vẫn đang xử lý, xin kiên nhẫn... ({elapsed}s)"
            };
            txtLoadingStatus.Text = statusText;
        };
        
        try
        {
            txtLoadingStatus.Text = "🤖 Đang gửi file lên Gemini AI Vision...";
            progressTimer.Start();
            
            _extractedData = await _aiService.ExtractDocumentFromFileAsync(_selectedFilePath);
            
            progressTimer.Stop();
            txtLoadingStatus.Text = $"✅ Phân tích hoàn tất sau {elapsed}s! Đang điền dữ liệu...";
            await System.Threading.Tasks.Task.Delay(500); // Brief visual feedback
            
            // Populate form
            PopulateForm(_extractedData);
            
            loadingPanel.Visibility = Visibility.Collapsed;
            btnSave.IsEnabled = true;
            txtExtractionStatus.Text = "✅ Đã trích xuất — Kiểm tra và chỉnh sửa nếu cần";
            txtFooterInfo.Text = $"✅ Trích xuất thành công ({elapsed}s) | File: {Path.GetFileName(_selectedFilePath)}";
        }
        catch (Exception ex)
        {
            progressTimer.Stop();
            loadingPanel.Visibility = Visibility.Collapsed;
            txtExtractionStatus.Text = "❌ Lỗi phân tích";
            
            // Phân biệt lỗi timeout vs lỗi khác
            var isTimeout = ex.Message.Contains("Timeout") || ex.Message.Contains("timeout") 
                || ex.Message.Contains("Không thể trích xuất sau");
            var errorDetail = isTimeout
                ? $"⏰ Quá thời gian chờ ({elapsed}s)\n\n" +
                  "Nguyên nhân: File quá lớn hoặc mạng chậm.\n" +
                  "Gợi ý: Thử lại hoặc dùng file nhỏ hơn."
                : $"Lỗi khi phân tích file:\n\n{ex.Message}\n\n" +
                  "Hãy thử lại hoặc chọn file khác.";
            
            MessageBox.Show(errorDetail, "Lỗi AI", MessageBoxButton.OK, MessageBoxImage.Error);
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
        
        // Parse urgency
        var urgencyValue = cboDoKhan.SelectedValue?.ToString() ?? "Thuong";
        var urgencyMap = new Dictionary<string, UrgencyLevel>(StringComparer.OrdinalIgnoreCase)
        {
            ["Thuong"] = UrgencyLevel.Thuong,
            ["Khan"] = UrgencyLevel.Khan,
            ["ThuongKhan"] = UrgencyLevel.ThuongKhan,
            ["HoaToc"] = UrgencyLevel.HoaToc
        };
        if (urgencyMap.TryGetValue(urgencyValue, out var urgency))
            doc.UrgencyLevel = urgency;
        
        // Parse security
        var securityValue = cboDoMat.SelectedValue?.ToString() ?? "Thuong";
        var securityMap = new Dictionary<string, SecurityLevel>(StringComparer.OrdinalIgnoreCase)
        {
            ["Thuong"] = SecurityLevel.Thuong,
            ["Mat"] = SecurityLevel.Mat,
            ["ToiMat"] = SecurityLevel.ToiMat,
            ["TuyetMat"] = SecurityLevel.TuyetMat
        };
        if (securityMap.TryGetValue(securityValue, out var security))
            doc.SecurityLevel = security;
        
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
