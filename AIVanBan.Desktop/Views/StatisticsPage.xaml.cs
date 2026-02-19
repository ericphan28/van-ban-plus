using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Trang báo cáo thống kê chuyên sâu.
/// Khác với Dashboard (tổng quan nhanh): trang này tập trung vào
/// so sánh kỳ trước, hiệu suất xử lý, và bảng dữ liệu chi tiết xuất CSV.
/// </summary>
public partial class StatisticsPage : Page
{
    private readonly DocumentService _documentService;
    private List<Document> _periodDocs = new();
    private List<Document> _prevPeriodDocs = new();

    #region View Models — dạng bảng, không dùng bar chart

    public class TypeRow
    {
        public string TypeName { get; set; } = "";
        public int Den { get; set; }
        public int Di { get; set; }
        public int NoiBo { get; set; }
        public int Total => Den + Di + NoiBo;
    }

    public class CategoryRow
    {
        public string Label { get; set; } = "";
        public int Count { get; set; }
        public string Percent { get; set; } = "";
    }

    public class MonthRow
    {
        public string Month { get; set; } = "";
        public int Den { get; set; }
        public int Di { get; set; }
        public int NoiBo { get; set; }
        public int Total => Den + Di + NoiBo;
        public string DeltaText { get; set; } = "";
    }

    #endregion

    public StatisticsPage(DocumentService documentService)
    {
        InitializeComponent();
        _documentService = documentService;
        Loaded += (s, e) => LoadStatistics();
    }

    private void Period_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) LoadStatistics();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => LoadStatistics();

    /// <summary>
    /// Trả về khoảng thời gian cho kỳ hiện tại VÀ kỳ trước đó (để so sánh).
    /// </summary>
    private (DateTime start, DateTime end, DateTime prevStart, DateTime prevEnd) GetPeriodRangeWithPrev()
    {
        var now = DateTime.Now;
        var period = cboPeriod?.SelectedIndex ?? 1;

        return period switch
        {
            0 => // Tuần này
            (
                now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday), now,
                now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday - 7),
                now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday).AddTicks(-1)
            ),
            1 => // Tháng này
            (
                new DateTime(now.Year, now.Month, 1), now,
                new DateTime(now.Year, now.Month, 1).AddMonths(-1),
                new DateTime(now.Year, now.Month, 1).AddTicks(-1)
            ),
            2 => // Quý này
            (
                new DateTime(now.Year, (now.Month - 1) / 3 * 3 + 1, 1), now,
                new DateTime(now.Year, (now.Month - 1) / 3 * 3 + 1, 1).AddMonths(-3),
                new DateTime(now.Year, (now.Month - 1) / 3 * 3 + 1, 1).AddTicks(-1)
            ),
            3 => // Năm nay
            (
                new DateTime(now.Year, 1, 1), now,
                new DateTime(now.Year - 1, 1, 1),
                new DateTime(now.Year, 1, 1).AddTicks(-1)
            ),
            4 => // Tất cả — không có kỳ trước
            (
                DateTime.MinValue, now,
                DateTime.MinValue, DateTime.MinValue
            ),
            _ => (new DateTime(now.Year, now.Month, 1), now,
                  new DateTime(now.Year, now.Month, 1).AddMonths(-1),
                  new DateTime(now.Year, now.Month, 1).AddTicks(-1))
        };
    }

    private void LoadStatistics()
    {
        try
        {
            var allDocs = _documentService.GetAllDocuments();
            var (start, end, prevStart, prevEnd) = GetPeriodRangeWithPrev();

            _periodDocs = start == DateTime.MinValue
                ? allDocs
                : allDocs.Where(d => d.IssueDate >= start && d.IssueDate <= end).ToList();

            _prevPeriodDocs = prevStart == DateTime.MinValue && prevEnd == DateTime.MinValue
                ? new List<Document>()
                : allDocs.Where(d => d.IssueDate >= prevStart && d.IssueDate <= prevEnd).ToList();

            var periodLabels = new[] { "Tuần này", "Tháng này", "Quý này", "Năm nay", "Tất cả" };
            var idx = cboPeriod?.SelectedIndex ?? 1;
            txtPeriodInfo.Text = $"Kỳ: {periodLabels[idx]} — {_periodDocs.Count} văn bản"
                + (idx < 4 ? $" (kỳ trước: {_prevPeriodDocs.Count})" : "");

            LoadPeriodComparison();
            LoadPerformanceMetrics();
            LoadTypeTable();
            LoadUrgencyTable();
            LoadSecurityTable();
            LoadMonthlyTrend(allDocs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Statistics error: {ex.Message}");
        }
    }

    #region Phần 1: So sánh kỳ trước

    private void LoadPeriodComparison()
    {
        int curDen = _periodDocs.Count(d => d.Direction == Direction.Den);
        int curDi = _periodDocs.Count(d => d.Direction == Direction.Di);
        int curNB = _periodDocs.Count(d => d.Direction == Direction.NoiBo);
        int curTotal = _periodDocs.Count;

        int prevDen = _prevPeriodDocs.Count(d => d.Direction == Direction.Den);
        int prevDi = _prevPeriodDocs.Count(d => d.Direction == Direction.Di);
        int prevNB = _prevPeriodDocs.Count(d => d.Direction == Direction.NoiBo);
        int prevTotal = _prevPeriodDocs.Count;

        // Kỳ này
        txtCompDen.Text = curDen.ToString();
        txtCompDi.Text = curDi.ToString();
        txtCompNB.Text = curNB.ToString();
        txtCompTotal.Text = curTotal.ToString();

        // Kỳ trước
        txtCompDenPrev.Text = $"Kỳ trước: {prevDen}";
        txtCompDiPrev.Text = $"Kỳ trước: {prevDi}";
        txtCompNBPrev.Text = $"Kỳ trước: {prevNB}";
        txtCompTotalPrev.Text = $"Kỳ trước: {prevTotal}";

        // Delta
        txtCompDenDelta.Text = FormatDelta(curDen, prevDen);
        txtCompDiDelta.Text = FormatDelta(curDi, prevDi);
        txtCompNBDelta.Text = FormatDelta(curNB, prevNB);
        txtCompTotalDelta.Text = FormatDelta(curTotal, prevTotal);
    }

    private static string FormatDelta(int current, int previous)
    {
        if (previous == 0) return current > 0 ? $"(+{current})" : "";
        var diff = current - previous;
        var pct = (double)diff / previous * 100;
        var arrow = diff > 0 ? "▲" : diff < 0 ? "▼" : "—";
        return $"{arrow} {(diff > 0 ? "+" : "")}{diff} ({pct:+0;-0}%)";
    }

    #endregion

    #region Phần 2: Hiệu suất xử lý VB Đến

    private void LoadPerformanceMetrics()
    {
        // Chỉ VB đến có hạn xử lý
        var docsWithDue = _periodDocs.Where(d =>
            d.Direction == Direction.Den && d.DueDate.HasValue).ToList();

        txtPerfTotal.Text = docsWithDue.Count.ToString();

        if (docsWithDue.Count > 0)
        {
            var onTime = docsWithDue.Count(d =>
                d.WorkflowStatus == DocumentStatus.Archived ||
                d.WorkflowStatus == DocumentStatus.Published ||
                d.DueDate!.Value.Date >= DateTime.Today);
            var overdue = docsWithDue.Count - onTime;
            var onTimeRate = (double)onTime / docsWithDue.Count * 100;
            var overdueRate = (double)overdue / docsWithDue.Count * 100;

            txtPerfOnTime.Text = onTime.ToString();
            txtPerfOnTimeRate.Text = $"{onTimeRate:F0}%";
            txtPerfOverdue.Text = overdue.ToString();
            txtPerfOverdueRate.Text = $"{overdueRate:F0}%";
        }
        else
        {
            txtPerfOnTime.Text = "0";
            txtPerfOnTimeRate.Text = "- %";
            txtPerfOverdue.Text = "0";
            txtPerfOverdueRate.Text = "- %";
        }

        // Trung bình ngày xử lý (VB đã xử lý xong)
        var processedDocs = _periodDocs.Where(d =>
            d.Direction == Direction.Den && d.ArrivalDate.HasValue &&
            (d.WorkflowStatus == DocumentStatus.Archived || d.WorkflowStatus == DocumentStatus.Published)
        ).ToList();

        if (processedDocs.Count > 0)
        {
            var avgDays = processedDocs.Average(d =>
            {
                var endDate = d.ModifiedDate ?? d.CreatedDate;
                return Math.Max(0, (endDate - d.ArrivalDate!.Value).TotalDays);
            });
            txtPerfAvgDays.Text = $"{avgDays:F1}";
        }
        else
        {
            txtPerfAvgDays.Text = "-";
        }
    }

    #endregion

    #region Phần 3: Bảng phân loại

    private void LoadTypeTable()
    {
        var grouped = _periodDocs
            .GroupBy(d => d.Type)
            .OrderByDescending(g => g.Count())
            .ToList();

        var rows = grouped.Select(g => new TypeRow
        {
            TypeName = g.Key.GetDisplayName(),
            Den = g.Count(d => d.Direction == Direction.Den),
            Di = g.Count(d => d.Direction == Direction.Di),
            NoiBo = g.Count(d => d.Direction == Direction.NoiBo)
        }).ToList();

        // Thêm hàng tổng
        if (rows.Count > 0)
        {
            rows.Add(new TypeRow
            {
                TypeName = "TỔNG CỘNG",
                Den = rows.Sum(r => r.Den),
                Di = rows.Sum(r => r.Di),
                NoiBo = rows.Sum(r => r.NoiBo)
            });
        }

        dgByType.ItemsSource = rows;
    }

    private void LoadUrgencyTable()
    {
        var total = Math.Max(1, _periodDocs.Count);
        var items = new List<CategoryRow>();

        foreach (var level in Enum.GetValues<UrgencyLevel>())
        {
            var count = _periodDocs.Count(d => d.UrgencyLevel == level);
            items.Add(new CategoryRow
            {
                Label = level.GetDisplayName(),
                Count = count,
                Percent = $"{(double)count / total * 100:F0}%"
            });
        }

        dgByUrgency.ItemsSource = items;
    }

    private void LoadSecurityTable()
    {
        var total = Math.Max(1, _periodDocs.Count);
        var items = new List<CategoryRow>();

        foreach (var level in Enum.GetValues<SecurityLevel>())
        {
            var count = _periodDocs.Count(d => d.SecurityLevel == level);
            items.Add(new CategoryRow
            {
                Label = level.GetDisplayName(),
                Count = count,
                Percent = $"{(double)count / total * 100:F0}%"
            });
        }

        dgBySecurity.ItemsSource = items;
    }

    #endregion

    #region Phần 4: Xu hướng 12 tháng

    private void LoadMonthlyTrend(List<Document> allDocs)
    {
        var now = DateTime.Now;
        var rows = new List<MonthRow>();

        for (int i = 11; i >= 0; i--)
        {
            var target = now.AddMonths(-i);
            var mStart = new DateTime(target.Year, target.Month, 1);
            var mEnd = mStart.AddMonths(1).AddTicks(-1);

            var monthDocs = allDocs.Where(d => d.IssueDate >= mStart && d.IssueDate <= mEnd).ToList();
            var curTotal = monthDocs.Count;

            // Tháng trước đó (để so sánh)
            var prevTarget = target.AddMonths(-1);
            var prevStart = new DateTime(prevTarget.Year, prevTarget.Month, 1);
            var prevEnd = prevStart.AddMonths(1).AddTicks(-1);
            var prevTotal = allDocs.Count(d => d.IssueDate >= prevStart && d.IssueDate <= prevEnd);

            var delta = curTotal - prevTotal;
            var deltaText = prevTotal == 0
                ? (curTotal > 0 ? $"+{curTotal}" : "—")
                : $"{(delta > 0 ? "+" : "")}{delta}";

            rows.Add(new MonthRow
            {
                Month = $"T{target.Month:D2}/{target.Year}",
                Den = monthDocs.Count(d => d.Direction == Direction.Den),
                Di = monthDocs.Count(d => d.Direction == Direction.Di),
                NoiBo = monthDocs.Count(d => d.Direction == Direction.NoiBo),
                DeltaText = deltaText
            });
        }

        dgMonthlyTrend.ItemsSource = rows;
    }

    #endregion

    #region Xuất CSV

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"BaoCao_VanBan_{DateTime.Now:yyyyMMdd}.csv",
                Title = "Xuất báo cáo"
            };

            if (dialog.ShowDialog() == true)
            {
                var sb = new StringBuilder();
                sb.AppendLine("\"Số VB\",\"Tiêu đề\",\"Loại\",\"Hướng\",\"Ngày BH\",\"Cơ quan\",\"Mức khẩn\",\"Độ mật\",\"Hạn XL\",\"Trạng thái\"");

                foreach (var doc in _periodDocs.OrderBy(d => d.IssueDate))
                {
                    var esc = (string s) => $"\"{s?.Replace("\"", "\"\"") ?? ""}\"";
                    sb.AppendLine(string.Join(",",
                        esc(doc.Number),
                        esc(doc.Title),
                        esc(doc.Type.GetDisplayName()),
                        esc(doc.Direction.GetDisplayName()),
                        $"\"{doc.IssueDate:dd/MM/yyyy}\"",
                        esc(doc.Issuer),
                        esc(doc.UrgencyLevel.GetDisplayName()),
                        esc(doc.SecurityLevel.GetDisplayName()),
                        $"\"{doc.DueDate?.ToString("dd/MM/yyyy") ?? ""}\"",
                        esc(doc.WorkflowStatus.ToString())
                    ));
                }

                File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));

                MessageBox.Show($"✅ Đã xuất {_periodDocs.Count} văn bản ra:\n{dialog.FileName}",
                    "Xuất CSV thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi xuất CSV:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Xuất Excel

    private void ExportExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var periodLabels = new[] { "Tuần này", "Tháng này", "Quý này", "Năm nay", "Tất cả" };
            var idx = cboPeriod?.SelectedIndex ?? 1;
            var periodLabel = periodLabels[idx];

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_ThongKe_{DateTime.Now:yyyyMMdd}.xlsx",
                Title = "Xuất báo cáo thống kê Excel"
            };

            if (dialog.ShowDialog() == true)
            {
                var allDocs = _documentService.GetAllDocuments();
                var excelService = new ExcelExportService();

                excelService.ExportStatisticsReport(
                    _periodDocs,
                    _prevPeriodDocs,
                    allDocs,
                    periodLabel,
                    dialog.FileName);

                var result = MessageBox.Show(
                    $"✅ Đã xuất báo cáo thống kê ({_periodDocs.Count} VB) ra:\n{dialog.FileName}\n\nBạn có muốn mở file không?",
                    "Xuất Excel thành công",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi xuất Excel:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}
