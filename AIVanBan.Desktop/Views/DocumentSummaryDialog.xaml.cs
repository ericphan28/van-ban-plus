using System.Windows;
using System.Windows.Threading;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class DocumentSummaryDialog : Window
{
    private string _content;
    private string _documentType;
    private readonly string _title;
    private readonly string _issuer;
    private DocumentSummary? _result;
    private DispatcherTimer? _timer;
    private int _elapsedSeconds;

    public DocumentSummaryDialog(string content, string documentType = "", string title = "", string issuer = "")
    {
        InitializeComponent();
        _content = content;
        _documentType = documentType;
        _title = title;
        _issuer = issuer;

        // Hiện input panel, ẩn loading
        pnlQuickInput.Visibility = Visibility.Visible;
        pnlLoading.Visibility = Visibility.Collapsed;

        // Header info
        if (!string.IsNullOrWhiteSpace(title))
            txtHeaderInfo.Text = $"📄 {title} — Xem lại nội dung rồi nhấn Tóm tắt";
        else
            txtHeaderInfo.Text = "Dán hoặc chỉnh sửa nội dung rồi nhấn Tóm tắt";

        // Pre-fill content nếu có
        if (!string.IsNullOrWhiteSpace(content))
            txtQuickInput.Text = content;

        // Populate document type ComboBox
        var docTypes = EnumDisplayHelper.GetDocumentTypeItems();
        cboQuickDocType.Items.Add(new { Display = "— Không chọn —", Value = "" });
        foreach (var kv in docTypes)
            cboQuickDocType.Items.Add(new { Display = kv.Value, Value = kv.Value });
        cboQuickDocType.DisplayMemberPath = "Display";
        cboQuickDocType.SelectedValuePath = "Value";
        cboQuickDocType.SelectedIndex = 0;
        if (!string.IsNullOrWhiteSpace(documentType))
        {
            for (int i = 1; i < cboQuickDocType.Items.Count; i++)
            {
                var item = cboQuickDocType.Items[i];
                var displayProp = item.GetType().GetProperty("Display");
                if (displayProp?.GetValue(item)?.ToString() == documentType)
                {
                    cboQuickDocType.SelectedIndex = i;
                    break;
                }
            }
        }

        // Track char count
        txtQuickInput.TextChanged += (s, e) =>
        {
            var len = txtQuickInput.Text.Length;
            txtCharCount.Text = $"{len:N0} ký tự";
            btnQuickAction.IsEnabled = len > 10;
        };
        var initLen = txtQuickInput.Text.Length;
        txtCharCount.Text = $"{initLen:N0} ký tự";
        btnQuickAction.IsEnabled = initLen > 10;

        this.Height = 780;
    }

    private async void QuickAction_Click(object sender, RoutedEventArgs e)
    {
        if (!AiPromoHelper.CheckOrShowPromo(this)) return;

        var inputText = txtQuickInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(inputText) || inputText.Length <= 10)
        {
            MessageBox.Show("Vui lòng nhập nội dung văn bản (tối thiểu 10 ký tự).",
                "Thiếu nội dung", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _content = inputText;
        if (cboQuickDocType.SelectedValue is string selectedType)
            _documentType = selectedType;

        // Ẩn input panel, hiện loading
        pnlQuickInput.Visibility = Visibility.Collapsed;

        await StartSummary();
    }

    private async Task StartSummary()
    {
        ShowLoading(true);
        StartTimer();

        try
        {
            var service = new DocumentSummaryService();
            _result = await service.SummarizeAsync(_content, _documentType, _title, _issuer);
            DisplayResults(_result);
        }
        catch (Exception ex)
        {
            ShowError($"Lỗi khi tóm tắt: {ex.Message}");
        }
        finally
        {
            StopTimer();
        }
    }

    private void DisplayResults(DocumentSummary result)
    {
        ShowLoading(false);
        pnlResults.Visibility = Visibility.Visible;
        pnlError.Visibility = Visibility.Collapsed;
        btnCopy.Visibility = Visibility.Visible;

        // ═══ Header badge: document type ═══
        if (!string.IsNullOrWhiteSpace(result.DocumentType))
        {
            badgeDocType.Visibility = Visibility.Visible;
            txtDocTypeBadge.Text = result.DocumentType;
        }

        // ═══ Card 1: Brief ═══
        txtBrief.Text = string.IsNullOrWhiteSpace(result.Brief) 
            ? "(Không thể tóm tắt)" 
            : result.Brief;

        // ═══ Card 2: Document info ═══
        txtDocType.Text = string.IsNullOrWhiteSpace(result.DocumentType) ? "Chưa xác định" : result.DocumentType;
        txtIssuingAuth.Text = string.IsNullOrWhiteSpace(result.IssuingAuthority) ? "Chưa xác định" : result.IssuingAuthority;
        txtTargetAudience.Text = string.IsNullOrWhiteSpace(result.TargetAudience) ? "Chưa xác định" : result.TargetAudience;

        // ═══ Card 3: Key Points ═══
        if (result.KeyPoints?.Count > 0)
        {
            lstKeyPoints.ItemsSource = result.KeyPoints;
        }
        else
        {
            lstKeyPoints.ItemsSource = new List<SummaryKeyPoint>
            {
                new() { Heading = "Nội dung", Content = result.Brief ?? "(Không có dữ liệu)" }
            };
        }

        // ═══ Card 4: Legal Bases ═══
        if (result.LegalBases?.Count > 0)
        {
            cardLegalBases.Visibility = Visibility.Visible;
            lstLegalBases.ItemsSource = result.LegalBases;
        }

        // ═══ Card 5: Effective Dates ═══
        if (result.EffectiveDates?.Count > 0)
        {
            cardDates.Visibility = Visibility.Visible;
            lstDates.ItemsSource = result.EffectiveDates;
        }

        // ═══ Card 6: Key Figures ═══
        if (result.KeyFigures?.Count > 0)
        {
            cardFigures.Visibility = Visibility.Visible;
            lstFigures.ItemsSource = result.KeyFigures;
        }

        // ═══ Card 7: Impact ═══
        if (!string.IsNullOrWhiteSpace(result.Impact))
        {
            cardImpact.Visibility = Visibility.Visible;
            txtImpact.Text = result.Impact;
        }

        // ═══ Card 8: Notes ═══
        if (!string.IsNullOrWhiteSpace(result.Notes))
        {
            cardNotes.Visibility = Visibility.Visible;
            txtNotes.Text = result.Notes;
        }

        // Footer info
        var keyPointCount = result.KeyPoints?.Count ?? 0;
        var legalCount = result.LegalBases?.Count ?? 0;
        txtFooterInfo.Text = $"Loại: {txtDocType.Text} | {keyPointCount} nội dung chính | {legalCount} căn cứ pháp lý";
    }

    private void ShowError(string message)
    {
        ShowLoading(false);
        pnlResults.Visibility = Visibility.Collapsed;
        pnlError.Visibility = Visibility.Visible;
        txtError.Text = message;
    }

    private void ShowLoading(bool show)
    {
        pnlLoading.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        if (show)
        {
            pnlResults.Visibility = Visibility.Collapsed;
            pnlError.Visibility = Visibility.Collapsed;
        }
    }

    private void StartTimer()
    {
        _elapsedSeconds = 0;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) =>
        {
            _elapsedSeconds++;
            txtLoadingTimer.Text = $"⏱️ {_elapsedSeconds} giây...";
            txtLoadingStatus.Text = _elapsedSeconds switch
            {
                <= 5 => "🤖 Đang gửi văn bản cho AI phân tích...",
                <= 15 => "📋 AI đang đọc và phân tích cấu trúc văn bản...",
                <= 30 => "📌 AI đang trích xuất nội dung chính và số liệu...",
                <= 60 => "📝 AI đang tổng hợp và tóm tắt...",
                _ => "⏳ Đang xử lý văn bản dài, vui lòng chờ..."
            };
        };
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }

    private void CopySummary_Click(object sender, RoutedEventArgs e)
    {
        if (_result == null) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("📝 TÓM TẮT VĂN BẢN");
        if (!string.IsNullOrWhiteSpace(_title))
            sb.AppendLine($"📄 {_title}");
        sb.AppendLine();

        sb.AppendLine($"📋 Tóm tắt: {_result.Brief}");
        sb.AppendLine($"📄 Loại VB: {_result.DocumentType}");
        sb.AppendLine($"🏛️ Cơ quan: {_result.IssuingAuthority}");
        sb.AppendLine($"👥 Đối tượng: {_result.TargetAudience}");

        if (_result.KeyPoints?.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("📌 Nội dung chính:");
            foreach (var kp in _result.KeyPoints)
                sb.AppendLine($"  ▸ {kp.Heading}: {kp.Content}");
        }

        if (_result.LegalBases?.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("⚖️ Căn cứ pháp lý:");
            foreach (var lb in _result.LegalBases)
                sb.AppendLine($"  • {lb}");
        }

        if (_result.EffectiveDates?.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("📅 Mốc thời gian:");
            foreach (var d in _result.EffectiveDates)
                sb.AppendLine($"  • {d}");
        }

        if (_result.KeyFigures?.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("📊 Số liệu quan trọng:");
            foreach (var f in _result.KeyFigures)
                sb.AppendLine($"  • {f}");
        }

        if (!string.IsNullOrWhiteSpace(_result.Impact))
        {
            sb.AppendLine();
            sb.AppendLine($"💡 Tác động: {_result.Impact}");
        }

        if (!string.IsNullOrWhiteSpace(_result.Notes))
        {
            sb.AppendLine();
            sb.AppendLine($"⚠️ Lưu ý: {_result.Notes}");
        }

        Clipboard.SetText(sb.ToString());
        MessageBox.Show("📋 Đã copy bản tóm tắt vào clipboard!", "Thành công",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void Retry_Click(object sender, RoutedEventArgs e)
    {
        btnCopy.Visibility = Visibility.Collapsed;
        await StartSummary();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
