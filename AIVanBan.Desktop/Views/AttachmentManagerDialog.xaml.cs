using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace AIVanBan.Desktop.Views
{
    public partial class AttachmentManagerDialog : Window
    {
        private readonly DocumentService _documentService;
        private readonly string _documentId;
        private readonly string _documentTitle;
        private List<AttachmentViewModel> _attachments = new();

        public AttachmentManagerDialog(DocumentService documentService, string documentId, string documentTitle)
        {
            InitializeComponent();
            _documentService = documentService;
            _documentId = documentId;
            _documentTitle = documentTitle;
            
            txtDocumentInfo.Text = $"Văn bản: {_documentTitle}";
            LoadAttachments();
        }

        private void LoadAttachments()
        {
            try
            {
                var attachments = _documentService.GetAttachmentsByDocument(_documentId);
                _attachments = attachments.Select(a => AttachmentViewModel.FromAttachment(a)).ToList();
                
                dgAttachments.ItemsSource = _attachments;
                
                // Show/hide empty state
                if (_attachments.Count == 0)
                {
                    emptyState.Visibility = Visibility.Visible;
                    dgAttachments.Visibility = Visibility.Collapsed;
                }
                else
                {
                    emptyState.Visibility = Visibility.Collapsed;
                    dgAttachments.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách file:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UploadFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Chọn file đính kèm",
                    Filter = "Tất cả file hỗ trợ|*.doc;*.docx;*.pdf;*.xls;*.xlsx;*.ppt;*.pptx;*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                            "Word Documents (*.doc, *.docx)|*.doc;*.docx|" +
                            "PDF Files (*.pdf)|*.pdf|" +
                            "Excel Files (*.xls, *.xlsx)|*.xls;*.xlsx|" +
                            "PowerPoint Files (*.ppt, *.pptx)|*.ppt;*.pptx|" +
                            "Image Files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                            "Tất cả file (*.*)|*.*",
                    Multiselect = true
                };

                if (dialog.ShowDialog() == true)
                {
                    int successCount = 0;
                    int failCount = 0;
                    List<string> errors = new();

                    foreach (var filePath in dialog.FileNames)
                    {
                        try
                        {
                            // Save file to attachments folder
                            var savedPath = _documentService.SaveAttachmentFile(filePath, _documentId);
                            
                            // Get file info
                            var fileInfo = new FileInfo(filePath);
                            var extension = fileInfo.Extension.ToLower();
                            
                            // Determine file type
                            AttachmentType fileType = extension switch
                            {
                                ".doc" or ".docx" => AttachmentType.Word,
                                ".pdf" => AttachmentType.PDF,
                                ".xls" or ".xlsx" => AttachmentType.Excel,
                                ".ppt" or ".pptx" => AttachmentType.PowerPoint,
                                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => AttachmentType.Image,
                                _ => AttachmentType.Other
                            };
                            
                            // Create attachment record
                            var attachment = new Attachment
                            {
                                Id = Guid.NewGuid().ToString(),
                                DocumentId = _documentId,
                                FileName = fileInfo.Name,
                                FilePath = savedPath,
                                FileExtension = extension,
                                FileSize = fileInfo.Length,
                                Type = fileType,
                                UploadedDate = DateTime.Now,
                                UploadedBy = Environment.UserName
                            };
                            
                            // Save to database
                            _documentService.AddAttachment(attachment);
                            
                            // Update document's AttachmentIds array
                            var document = _documentService.GetDocument(_documentId);
                            if (document != null)
                            {
                                var attachmentIds = document.AttachmentIds?.ToList() ?? new List<string>();
                                attachmentIds.Add(attachment.Id);
                                document.AttachmentIds = attachmentIds.ToArray();
                                _documentService.UpdateDocument(document);
                            }
                            
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            errors.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                        }
                    }
                    
                    // Reload attachments
                    LoadAttachments();
                    
                    // Show result
                    if (failCount == 0)
                    {
                        MessageBox.Show($"✅ Đã tải lên thành công {successCount} file!", 
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var message = $"✅ Thành công: {successCount} file\n❌ Thất bại: {failCount} file\n\nLỗi:\n";
                        message += string.Join("\n", errors.Take(5));
                        if (errors.Count > 5)
                            message += $"\n... và {errors.Count - 5} lỗi khác";
                        
                        MessageBox.Show(message, "Kết quả tải lên", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải lên file:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectFromAlbum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get photos folder path
                var photosPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AIVanBan", "Photos");
                
                if (!Directory.Exists(photosPath))
                {
                    Directory.CreateDirectory(photosPath);
                }

                var dialog = new OpenFileDialog
                {
                    Title = "Chọn ảnh từ Album",
                    Filter = "Image Files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                    InitialDirectory = photosPath,
                    Multiselect = true
                };

                if (dialog.ShowDialog() == true)
                {
                    int successCount = 0;
                    int failCount = 0;
                    List<string> errors = new();

                    foreach (var filePath in dialog.FileNames)
                    {
                        try
                        {
                            // Save file to attachments folder
                            var savedPath = _documentService.SaveAttachmentFile(filePath, _documentId);
                            
                            // Get file info
                            var fileInfo = new FileInfo(filePath);
                            
                            // Create attachment record
                            var attachment = new Attachment
                            {
                                Id = Guid.NewGuid().ToString(),
                                DocumentId = _documentId,
                                FileName = fileInfo.Name,
                                FilePath = savedPath,
                                FileExtension = fileInfo.Extension.ToLower(),
                                FileSize = fileInfo.Length,
                                Type = AttachmentType.Image,
                                Description = "Ảnh từ Album",
                                UploadedDate = DateTime.Now,
                                UploadedBy = Environment.UserName
                            };
                            
                            // Save to database
                            _documentService.AddAttachment(attachment);
                            
                            // Update document's AttachmentIds array
                            var document = _documentService.GetDocument(_documentId);
                            if (document != null)
                            {
                                var attachmentIds = document.AttachmentIds?.ToList() ?? new List<string>();
                                attachmentIds.Add(attachment.Id);
                                document.AttachmentIds = attachmentIds.ToArray();
                                _documentService.UpdateDocument(document);
                            }
                            
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            errors.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                        }
                    }
                    
                    // Reload attachments
                    LoadAttachments();
                    
                    // Show result
                    if (failCount == 0)
                    {
                        MessageBox.Show($"✅ Đã thêm {successCount} ảnh từ Album!", 
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var message = $"✅ Thành công: {successCount} ảnh\n❌ Thất bại: {failCount} ảnh\n\nLỗi:\n";
                        message += string.Join("\n", errors.Take(5));
                        if (errors.Count > 5)
                            message += $"\n... và {errors.Count - 5} lỗi khác";
                        
                        MessageBox.Show(message, "Kết quả", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn ảnh từ Album:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string attachmentId)
                {
                    var attachment = _documentService.GetAttachment(attachmentId);
                    if (attachment != null && File.Exists(attachment.FilePath))
                    {
                        // Open file with default application
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = attachment.FilePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show("File không tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở file:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string attachmentId)
                {
                    var attachment = _documentService.GetAttachment(attachmentId);
                    if (attachment != null && File.Exists(attachment.FilePath))
                    {
                        var saveDialog = new SaveFileDialog
                        {
                            FileName = attachment.FileName,
                            Filter = $"File gốc (*{attachment.FileExtension})|*{attachment.FileExtension}|Tất cả file (*.*)|*.*"
                        };

                        if (saveDialog.ShowDialog() == true)
                        {
                            File.Copy(attachment.FilePath, saveDialog.FileName, true);
                            MessageBox.Show("✅ Đã tải xuống file thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("File không tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải xuống file:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string attachmentId)
                {
                    var attachment = _documentService.GetAttachment(attachmentId);
                    if (attachment == null) return;

                    var result = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa file '{attachment.FileName}'?", 
                        "Xác nhận xóa", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Delete from database and file system
                        _documentService.DeleteAttachment(attachmentId);
                        
                        // Update document's AttachmentIds array
                        var document = _documentService.GetDocument(_documentId);
                        if (document != null && document.AttachmentIds != null)
                        {
                            var attachmentIds = document.AttachmentIds.Where(id => id != attachmentId).ToArray();
                            document.AttachmentIds = attachmentIds;
                            _documentService.UpdateDocument(document);
                        }
                        
                        // Reload
                        LoadAttachments();
                        
                        MessageBox.Show("✅ Đã xóa file thành công!", "Thành công", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa file:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }

    public class AttachmentViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = "Other";
        public long FileSize { get; set; }
        public string FileSizeText { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        
        public PackIconKind IconKind
        {
            get
            {
                return FileType switch
                {
                    "Word" => PackIconKind.FileWord,
                    "PDF" => PackIconKind.FilePdfBox,
                    "Excel" => PackIconKind.FileExcel,
                    "PowerPoint" => PackIconKind.FilePowerpoint,
                    "Image" => PackIconKind.FileImage,
                    _ => PackIconKind.FileDocument
                };
            }
        }
        
        public string IconColor
        {
            get
            {
                return FileType switch
                {
                    "Word" => "#2B579A",
                    "PDF" => "#F40F02",
                    "Excel" => "#217346",
                    "PowerPoint" => "#D24726",
                    "Image" => "#9C27B0",
                    _ => "#666666"
                };
            }
        }

        public static AttachmentViewModel FromAttachment(Attachment attachment)
        {
            return new AttachmentViewModel
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileType = attachment.Type.ToString(),
                FileSize = attachment.FileSize,
                FileSizeText = FormatFileSize(attachment.FileSize),
                UploadedDate = attachment.UploadedDate
            };
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
