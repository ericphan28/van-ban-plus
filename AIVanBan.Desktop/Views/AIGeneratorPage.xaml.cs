using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class AIGeneratorPage : Page
{
    private readonly DocumentService _documentService;
    private const string GEMINI_API_KEY = "AIzaSyAhQRYO6lSjG8m0sTP-Y8Gk262QKJyLrUg";

    public AIGeneratorPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        InitializeData();
    }
    
    private void NewDocument_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new AIComposeDialog(_documentService, GEMINI_API_KEY);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true && dialog.GeneratedDocument != null)
            {
                // Lưu document
                _documentService.AddDocument(dialog.GeneratedDocument);
                
                MessageBox.Show(
                    $"✅ Đã tạo và lưu văn bản:\n\n{dialog.GeneratedDocument.Title}",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                
                // Refresh would go here if we had the UI elements
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Lỗi khi mở AI Composer:\n\n{ex.Message}\n\nChi tiết:\n{ex.ToString()}";
            
            // Create custom error window with copyable text
            var errorWindow = new Window
            {
                Title = "❌ Lỗi",
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = Window.GetWindow(this)
            };
            
            var grid = new Grid { Margin = new Thickness(15) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Scrollable error text (selectable)
            var scrollViewer = new ScrollViewer 
            { 
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var errorTextBox = new TextBox
            {
                Text = errorMessage,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            scrollViewer.Content = errorTextBox;
            Grid.SetRow(scrollViewer, 0);
            grid.Children.Add(scrollViewer);
            
            // Buttons
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right 
            };
            
            var copyButton = new Button
            {
                Content = "📋 Copy Lỗi",
                Width = 100,
                Height = 35,
                Margin = new Thickness(0, 0, 10, 0)
            };
            copyButton.Click += (s, args) =>
            {
                try
                {
                    Clipboard.SetText(errorMessage);
                    MessageBox.Show("✅ Đã copy lỗi vào clipboard!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch { }
            };
            
            var closeButton = new Button
            {
                Content = "Đóng",
                Width = 100,
                Height = 35,
                IsCancel = true
            };
            closeButton.Click += (s, args) => errorWindow.Close();
            
            buttonPanel.Children.Add(copyButton);
            buttonPanel.Children.Add(closeButton);
            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);
            
            errorWindow.Content = grid;
            errorWindow.ShowDialog();
        }
    }

    private void InitializeData()
    {
        // Load Document Types
        foreach (DocumentType type in Enum.GetValues(typeof(DocumentType)))
        {
            cboDocType.Items.Add(type);
        }
        cboDocType.SelectedIndex = 0;

        // Load Templates
        cboTemplate.Items.Add("Mặc định");
        var templates = _documentService.GetAllTemplates();
        foreach (var template in templates)
        {
            cboTemplate.Items.Add(template.Name);
        }
        cboTemplate.SelectedIndex = 0;

        // Set default values
        dpIssueDate.SelectedDate = DateTime.Now;
    }

    private void DocumentType_Changed(object sender, SelectionChangedEventArgs e)
    {
        // Update hints based on document type
        if (cboDocType.SelectedItem is DocumentType type)
        {
            txtStatus.Text = $"Đã chọn: {type}";
        }
    }

    private void Generate_Click(object sender, RoutedEventArgs e)
    {
        // Validate
        if (cboDocType.SelectedItem == null)
        {
            MessageBox.Show("Vui lòng chọn loại văn bản!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtSubject.Text))
        {
            MessageBox.Show("Vui lòng nhập trích yếu/về việc!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        btnGenerate.IsEnabled = false;
        txtStatus.Text = "⏳ Đang tạo văn bản...";

        try
        {
            var docType = (DocumentType)cboDocType.SelectedItem;
            var generatedText = GenerateDocument(docType);
            
            txtPreview.Text = generatedText;
            txtStatus.Text = "✅ Tạo thành công! Bạn có thể chỉnh sửa, copy hoặc lưu.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            txtStatus.Text = "❌ Có lỗi xảy ra";
        }
        finally
        {
            btnGenerate.IsEnabled = true;
        }
    }

    private string GenerateDocument(DocumentType type)
    {
        // Template-based generation (later: integrate real AI)
        var number = string.IsNullOrWhiteSpace(txtNumber.Text) ? "[Số văn bản]" : txtNumber.Text;
        var date = dpIssueDate.SelectedDate ?? DateTime.Now;
        var issuer = string.IsNullOrWhiteSpace(txtIssuer.Text) ? "[Cơ quan ban hành]" : txtIssuer.Text;
        var recipient = string.IsNullOrWhiteSpace(txtRecipient.Text) ? "[Người nhận]" : txtRecipient.Text;
        var subject = txtSubject.Text.Trim();
        var content = string.IsNullOrWhiteSpace(txtMainContent.Text) 
            ? "[Nội dung chi tiết...]" 
            : txtMainContent.Text.Trim();
        var signer = string.IsNullOrWhiteSpace(txtSigner.Text) ? "[Người ký]" : txtSigner.Text;

        return type switch
        {
            DocumentType.CongVan => GenerateCongVan(number, date, issuer, recipient, subject, content, signer),
            DocumentType.BaoCao => GenerateBaoCao(number, date, issuer, recipient, subject, content, signer),
            DocumentType.ToTrinh => GenerateToTrinh(number, date, issuer, recipient, subject, content, signer),
            DocumentType.QuyetDinh => GenerateQuyetDinh(number, date, issuer, recipient, subject, content, signer),
            DocumentType.ThongBao => GenerateThongBao(number, date, issuer, recipient, subject, content, signer),
            _ => GenerateCongVan(number, date, issuer, recipient, subject, content, signer)
        };
    }

    private string GenerateCongVan(string number, DateTime date, string issuer, 
        string recipient, string subject, string content, string signer)
    {
        return $@"{issuer.ToUpper()}
---------

Số: {number}
V/v: {subject}

                                                        {issuer}, ngày {date:dd} tháng {date:MM} năm {date:yyyy}

Kính gửi: {recipient}

    {content}

    {issuer} trân trọng thông báo và đề nghị {recipient} thực hiện.


                                                        {signer.ToUpper()}
                                                        (Ký và đóng dấu)



                                                        [{signer}]";
    }

    private string GenerateBaoCao(string number, DateTime date, string issuer, 
        string recipient, string subject, string content, string signer)
    {
        return $@"{issuer.ToUpper()}
---------

BÁO CÁO
{subject}

Số: {number}

Kính gửi: {recipient}

    Căn cứ yêu cầu của {recipient};
    Căn cứ kết quả thực hiện công việc;
    
    {issuer} báo cáo như sau:

I. TÌNH HÌNH THỰC HIỆN

    {content}

II. ĐÁNH GIÁ VÀ ĐỀ XUẤT

    [Nội dung đánh giá, kiến nghị...]

    Trên đây là báo cáo của {issuer}, kính trình {recipient} xem xét.


                                                        {issuer}, ngày {date:dd} tháng {date:MM} năm {date:yyyy}
                                                        {signer.ToUpper()}
                                                        (Ký và đóng dấu)



                                                        [{signer}]";
    }

    private string GenerateToTrinh(string number, DateTime date, string issuer, 
        string recipient, string subject, string content, string signer)
    {
        return $@"{issuer.ToUpper()}
---------

TỜ TRÌNH
{subject}

Số: {number}

Kính gửi: {recipient}

    Căn cứ Luật [Tên luật];
    Căn cứ [Các văn bản pháp lý liên quan];
    Căn cứ thực tế tình hình công việc;
    
    {issuer} kính trình {recipient} như sau:

I. SỰ CẦN THIẾT

    {content}

II. NỘI DUNG ĐỀ XUẤT

    [Nội dung cụ thể đề xuất...]

III. DỰ KIẾN KINH PHÍ VÀ NGUỒN KINH PHÍ

    [Nội dung kinh phí...]

    {issuer} kính trình {recipient} xem xét, quyết định.


                                                        {issuer}, ngày {date:dd} tháng {date:MM} năm {date:yyyy}
                                                        {signer.ToUpper()}
                                                        (Ký và đóng dấu)



                                                        [{signer}]";
    }

    private string GenerateQuyetDinh(string number, DateTime date, string issuer, 
        string recipient, string subject, string content, string signer)
    {
        return $@"{issuer.ToUpper()}
---------

QUYẾT ĐỊNH
{subject}

Số: {number}

                                                        {signer.ToUpper()}

    Căn cứ Luật [Tên luật];
    Căn cứ [Các văn bản pháp lý liên quan];
    Xét đề nghị của [Đơn vị/Cá nhân];

QUYẾT ĐỊNH:

Điều 1. {content}

Điều 2. Quyết định này có hiệu lực kể từ ngày ký.

Điều 3. [Các cơ quan, đơn vị, cá nhân có liên quan] chịu trách nhiệm thi hành Quyết định này.


                                                        {issuer}, ngày {date:dd} tháng {date:MM} năm {date:yyyy}
                                                        {signer.ToUpper()}
                                                        (Ký và đóng dấu)



                                                        [{signer}]";
    }

    private string GenerateThongBao(string number, DateTime date, string issuer, 
        string recipient, string subject, string content, string signer)
    {
        return $@"{issuer.ToUpper()}
---------

THÔNG BÁO
{subject}

Số: {number}

    {issuer} thông báo đến {recipient}:

    {content}

    Đề nghị các đơn vị, cá nhân có liên quan thực hiện đúng nội dung thông báo này.


                                                        {issuer}, ngày {date:dd} tháng {date:MM} năm {date:yyyy}
                                                        {signer.ToUpper()}
                                                        (Ký và đóng dấu)



                                                        [{signer}]";
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtPreview.Text))
        {
            Clipboard.SetText(txtPreview.Text);
            txtStatus.Text = "📋 Đã copy vào clipboard!";
            MessageBox.Show("Đã copy văn bản vào clipboard!", "Thành công", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void SaveToDatabase_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtPreview.Text) || cboDocType.SelectedItem == null)
        {
            MessageBox.Show("Vui lòng tạo văn bản trước!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var document = new Document
        {
            Number = txtNumber.Text.Trim(),
            Title = txtSubject.Text.Trim(),
            Type = (DocumentType)cboDocType.SelectedItem,
            IssueDate = dpIssueDate.SelectedDate ?? DateTime.Now,
            Issuer = txtIssuer.Text.Trim(),
            Subject = txtSubject.Text.Trim(),
            Content = txtPreview.Text,
            Direction = Direction.Di,
            CreatedDate = DateTime.Now
        };

        _documentService.AddDocument(document);
        
        txtStatus.Text = "💾 Đã lưu vào cơ sở dữ liệu!";
        MessageBox.Show("Đã lưu văn bản vào cơ sở dữ liệu!", "Thành công", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportWord_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtPreview.Text))
        {
            MessageBox.Show("Vui lòng tạo văn bản trước!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|Word files (*.docx)|*.docx|All files (*.*)|*.*",
            DefaultExt = ".txt",
            FileName = $"VanBan_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                System.IO.File.WriteAllText(saveDialog.FileName, txtPreview.Text);
                txtStatus.Text = $"📁 Đã xuất file: {System.IO.Path.GetFileName(saveDialog.FileName)}";
                MessageBox.Show($"Đã xuất văn bản ra file:\n{saveDialog.FileName}", "Thành công", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
