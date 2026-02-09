using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class DocumentRegisterPage : Page
{
    private readonly DocumentService _documentService;
    private List<Document> _allDocuments = new();

    public DocumentRegisterPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        InitializeFilters();
        LoadData();
    }

    private void InitializeFilters()
    {
        // Năm: từ năm hiện tại trở về 5 năm
        var years = new List<string> { "Tất cả" };
        for (int y = DateTime.Now.Year; y >= DateTime.Now.Year - 5; y--)
            years.Add(y.ToString());
        cboYear.ItemsSource = years;
        cboYear.SelectedIndex = 0;

        // Loại văn bản
        var types = new List<string> { "Tất cả" };
        types.AddRange(Enum.GetNames(typeof(DocumentType)));
        cboType.ItemsSource = types;
        cboType.SelectedIndex = 0;
    }

    private void LoadData()
    {
        _allDocuments = _documentService.GetAllDocuments();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allDocuments.AsEnumerable();

        // Filter by year
        if (cboYear.SelectedItem is string yearStr && yearStr != "Tất cả" && int.TryParse(yearStr, out int year))
            filtered = filtered.Where(d => d.IssueDate.Year == year);

        // Filter by type
        if (cboType.SelectedItem is string typeStr && typeStr != "Tất cả" && Enum.TryParse<DocumentType>(typeStr, out var type))
            filtered = filtered.Where(d => d.Type == type);

        // Filter by search
        var keyword = txtSearch.Text?.Trim().ToLower() ?? "";
        if (!string.IsNullOrEmpty(keyword))
        {
            filtered = filtered.Where(d =>
                (d.Number?.ToLower().Contains(keyword) == true) ||
                (d.Title?.ToLower().Contains(keyword) == true) ||
                (d.Subject?.ToLower().Contains(keyword) == true) ||
                (d.Issuer?.ToLower().Contains(keyword) == true));
        }

        var list = filtered.OrderByDescending(d => d.IssueDate).ToList();

        // Split by direction
        var outgoing = list.Where(d => d.Direction == Direction.Di).ToList();
        var incoming = list.Where(d => d.Direction == Direction.Den).ToList();
        var internalDocs = list.Where(d => d.Direction == Direction.NoiBo).ToList();

        // Add row numbers
        dgOutgoing.ItemsSource = AddRowNumbers(outgoing);
        dgIncoming.ItemsSource = AddRowNumbers(incoming);
        dgInternal.ItemsSource = AddRowNumbers(internalDocs);

        // Update counts
        txtTotal.Text = list.Count.ToString();
        txtOutgoing.Text = outgoing.Count.ToString();
        txtIncoming.Text = incoming.Count.ToString();
        txtInternal.Text = internalDocs.Count.ToString();

        txtOutgoingCount.Text = outgoing.Count.ToString();
        txtIncomingCount.Text = incoming.Count.ToString();
        txtInternalCount.Text = internalDocs.Count.ToString();
    }

    private List<DocumentRow> AddRowNumbers(List<Document> docs)
    {
        return docs.Select((d, i) => new DocumentRow
        {
            RowNumber = i + 1,
            Id = d.Id,
            Number = d.Number,
            Title = d.Title,
            Subject = string.IsNullOrEmpty(d.Subject) ? d.Title : d.Subject,
            Type = GetTypeDisplayName(d.Type),
            IssueDate = d.IssueDate,
            Issuer = d.Issuer,
            SignedBy = d.SignedBy,
            RecipientsText = d.Recipients != null && d.Recipients.Length > 0
                ? string.Join(", ", d.Recipients) : "",
            Status = d.Status,
            CreatedBy = d.CreatedBy,
            Direction = d.Direction
        }).ToList();
    }

    private string GetTypeDisplayName(DocumentType type)
    {
        return type switch
        {
            DocumentType.QuyetDinh => "Quyết định",
            DocumentType.CongVan => "Công văn",
            DocumentType.BaoCao => "Báo cáo",
            DocumentType.ToTrinh => "Tờ trình",
            DocumentType.KeHoach => "Kế hoạch",
            DocumentType.ThongBao => "Thông báo",
            DocumentType.NghiQuyet => "Nghị quyết",
            DocumentType.ChiThi => "Chỉ thị",
            DocumentType.HuongDan => "Hướng dẫn",
            DocumentType.QuyDinh => "Quy định",
            DocumentType.Luat => "Luật",
            DocumentType.NghiDinh => "Nghị định",
            DocumentType.ThongTu => "Thông tư",
            _ => type.ToString()
        };
    }

    private void TxtSearch_KeyUp(object sender, KeyEventArgs e)
    {
        ApplyFilters();
    }

    private void CboFilter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyFilters();
    }

    private void TabDirections_Changed(object sender, SelectionChangedEventArgs e)
    {
        // Tab changed - no additional action needed
    }

    private void DataGrid_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid dg && dg.SelectedItem is DocumentRow row)
        {
            var doc = _documentService.GetDocument(row.Id);
            if (doc != null)
            {
                try
                {
                    var dialog = new DocumentViewDialog(doc)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    dialog.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi mở văn bản: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
    }
}

/// <summary>
/// ViewModel row cho DataGrid sổ văn bản.
/// </summary>
public class DocumentRow
{
    public int RowNumber { get; set; }
    public string Id { get; set; } = "";
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Type { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public string Issuer { get; set; } = "";
    public string SignedBy { get; set; } = "";
    public string RecipientsText { get; set; } = "";
    public string Status { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public Direction Direction { get; set; }
}
