using System.IO;
using System.Windows;
using AIVanBan.Core.Models;
using Microsoft.Win32;

namespace AIVanBan.Desktop.Views;

public partial class DocumentEditDialog : Window
{
    public Document? Document { get; private set; }

    public DocumentEditDialog(Document? document = null, string? folderId = null)
    {
        InitializeComponent();
        
        // Load loại văn bản (hiển thị tên tiếng Việt)
        cboType.DisplayMemberPath = "Value";
        cboType.SelectedValuePath = "Key";
        foreach (var item in EnumDisplayHelper.GetDocumentTypeItems())
        {
            cboType.Items.Add(item);
        }
        
        // Load hướng văn bản (hiển thị tên tiếng Việt)
        cboDirection.DisplayMemberPath = "Value";
        cboDirection.SelectedValuePath = "Key";
        foreach (var item in EnumDisplayHelper.GetDirectionItems())
        {
            cboDirection.Items.Add(item);
        }

        if (document != null)
        {
            Document = document;
            Title = "Sửa văn bản";
            LoadDocument();
        }
        else
        {
            Document = new Document();
            // Set FolderId nếu có (khi tạo mới từ thư mục cụ thể)
            if (!string.IsNullOrEmpty(folderId))
            {
                Document.FolderId = folderId;
            }
            cboType.SelectedIndex = 0;
            cboDirection.SelectedIndex = 0;
        }
    }

    private void LoadDocument()
    {
        if (Document == null) return;

        txtNumber.Text = Document.Number;
        txtTitle.Text = Document.Title;
        txtIssuer.Text = Document.Issuer;
        txtSubject.Text = Document.Subject;
        txtRecipients.Text = string.Join(Environment.NewLine, Document.Recipients);
        txtBasedOn.Text = string.Join(Environment.NewLine, Document.BasedOn); // Căn cứ pháp lý
        txtContent.Text = Document.Content;
        txtFilePath.Text = Document.FilePath;
        dpIssueDate.SelectedDate = Document.IssueDate;
        cboType.SelectedValue = Document.Type;
        cboDirection.SelectedValue = Document.Direction;
    }

    private void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "All Files|*.*|Word|*.docx;*.doc|PDF|*.pdf|Excel|*.xlsx;*.xls",
            Title = "Chọn file văn bản"
        };

        if (dialog.ShowDialog() == true)
        {
            txtFilePath.Text = dialog.FileName;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(txtTitle.Text))
        {
            MessageBox.Show("Vui lòng nhập tiêu đề!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Document == null) return;

        // Save data
        Document.Number = txtNumber.Text;
        Document.Title = txtTitle.Text;
        Document.Issuer = txtIssuer.Text;
        Document.Subject = txtSubject.Text;
        Document.Recipients = txtRecipients.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
        Document.BasedOn = txtBasedOn.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
        Document.Content = txtContent.Text;
        Document.FilePath = txtFilePath.Text;
        Document.IssueDate = dpIssueDate.SelectedDate ?? DateTime.Now;
        Document.Type = cboType.SelectedValue is DocumentType t ? t : DocumentType.CongVan;
        Document.Direction = cboDirection.SelectedValue is Direction d ? d : Direction.Den;

        if (!string.IsNullOrEmpty(Document.FilePath) && File.Exists(Document.FilePath))
        {
            var fileInfo = new FileInfo(Document.FilePath);
            Document.FileExtension = fileInfo.Extension;
            Document.FileSize = fileInfo.Length;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
