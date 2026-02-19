using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class DocumentAdvisoryDialog : Window
{
    private string _content;
    private string _documentType;
    private readonly string _title;
    private readonly string _issuer;
    private DocumentAdvisory? _result;
    private DispatcherTimer? _timer;
    private int _elapsedSeconds;

    public DocumentAdvisoryDialog(string content, string documentType = "", string title = "", string issuer = "")
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
            txtHeaderInfo.Text = $"📄 {title} — Xem lại nội dung rồi nhấn Tham mưu";
        else
            txtHeaderInfo.Text = "Dán hoặc chỉnh sửa nội dung rồi nhấn Tham mưu";

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

        await StartAdvisory();
    }

    private async Task StartAdvisory()
    {
        ShowLoading(true);
        StartTimer();

        try
        {
            var service = new DocumentAdvisoryService();
            _result = await service.AdviseAsync(_content, _documentType, _title, _issuer);
            DisplayResults(_result);
        }
        catch (Exception ex)
        {
            ShowError($"Lỗi khi phân tích: {ex.Message}");
        }
        finally
        {
            StopTimer();
        }
    }

    private void DisplayResults(DocumentAdvisory result)
    {
        ShowLoading(false);
        pnlResults.Visibility = Visibility.Visible;
        pnlError.Visibility = Visibility.Collapsed;
        btnCopy.Visibility = Visibility.Visible;

        // ═══ Priority badge (header) ═══
        badgePriority.Visibility = Visibility.Visible;
        var (prioIcon, prioLabel, prioBg, prioFg) = result.Priority?.ToLower() switch
        {
            "high" => ("🔴", "Khẩn", "#FFCDD2", "#C62828"),
            "low" => ("🟢", "Thấp", "#C8E6C9", "#2E7D32"),
            _ => ("🟡", "Vừa", "#FFF3E0", "#E65100")
        };
        txtPriorityIcon.Text = prioIcon;
        txtPriorityLabel.Text = prioLabel;
        badgePriority.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(prioBg));
        txtPriorityLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(prioFg));

        // ═══ Urgency badge ═══
        var (urgIcon, urgLabel, urgBg, urgFg) = result.UrgencyLevel?.ToLower() switch
        {
            "hoa_toc" => ("🔥", "HỎA TỐC", "#D50000", "#FFFFFF"),
            "thuong_khan" => ("⚡", "THƯỢNG KHẨN", "#FF6D00", "#FFFFFF"),
            "khan" => ("⏰", "KHẨN", "#FFAB00", "#333333"),
            _ => ("", "", "", "")
        };
        if (!string.IsNullOrEmpty(urgLabel))
        {
            badgeUrgency.Visibility = Visibility.Visible;
            badgeUrgency.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(urgBg));
            txtUrgencyBadge.Text = $"{urgIcon} {urgLabel}";
            txtUrgencyBadge.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(urgFg));
        }

        // ═══ Inline badges ═══
        txtPriorityBadge.Text = $"{prioIcon} Ưu tiên {prioLabel.ToLower()}";
        badgePriorityInline.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(prioBg));
        txtPriorityBadge.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(prioFg));

        txtIncomingType.Text = string.IsNullOrEmpty(result.IncomingType) ? "VB đến" : result.IncomingType;
        txtField.Text = string.IsNullOrEmpty(result.RelatedField) ? "Tổng hợp" : $"📂 {result.RelatedField}";

        // ═══ Summary ═══
        txtSummary.Text = string.IsNullOrWhiteSpace(result.Summary) ? "(Không có tóm tắt)" : result.Summary;

        // ═══ Handler + Field ═══
        txtHandler.Text = string.IsNullOrEmpty(result.SuggestedHandler) ? "Chưa xác định" : result.SuggestedHandler;
        txtFieldDetail.Text = string.IsNullOrEmpty(result.RelatedField) ? "Tổng hợp" : result.RelatedField;

        // ═══ Coordination ═══
        if (result.Coordination?.Count > 0)
        {
            pnlCoordination.Visibility = Visibility.Visible;
            txtCoordination.Text = string.Join("  •  ", result.Coordination);
        }

        // ═══ Signing authority ═══
        if (!string.IsNullOrWhiteSpace(result.SigningAuthority))
        {
            pnlSigning.Visibility = Visibility.Visible;
            txtSigningAuthority.Text = result.SigningAuthority;
        }

        // ═══ Action items ═══
        lstActionItems.ItemsSource = result.ActionItems?.Count > 0
            ? result.ActionItems
            : new List<string> { "Lưu hồ sơ, theo dõi" };

        // ═══ Deadlines ═══
        if (result.Deadlines?.Count > 0)
        {
            cardDeadlines.Visibility = Visibility.Visible;
            lstDeadlines.ItemsSource = result.Deadlines;
        }

        // ═══ Legal references ═══
        if (result.LegalReferences?.Count > 0)
        {
            cardLegalRefs.Visibility = Visibility.Visible;
            lstLegalRefs.ItemsSource = result.LegalReferences;
        }

        // ═══ Response section ═══
        if (result.ResponseNeeded)
        {
            cardResponse.Visibility = Visibility.Visible;
            txtResponseType.Text = string.IsNullOrEmpty(result.ResponseType) ? "Công văn" : result.ResponseType;
            txtDraftOutline.Text = string.IsNullOrWhiteSpace(result.DraftResponseOutline)
                ? "(AI không đề xuất dàn ý phản hồi)"
                : result.DraftResponseOutline;
        }

        // ═══ Risk warning ═══
        if (!string.IsNullOrWhiteSpace(result.RiskWarning) 
            && !result.RiskWarning.Contains("Không có rủi ro"))
        {
            cardRisk.Visibility = Visibility.Visible;
            txtRiskWarning.Text = result.RiskWarning;
        }

        // Footer
        var responseText = result.ResponseNeeded ? "⚡ Cần phản hồi" : "📁 Lưu hồ sơ";
        var urgencyText = !string.IsNullOrEmpty(urgLabel) ? $" | {urgIcon} {urgLabel}" : "";
        txtFooterInfo.Text = $"Ưu tiên: {prioLabel}{urgencyText} | {responseText} | Lĩnh vực: {txtFieldDetail.Text}";
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
                <= 15 => "📋 AI đang tóm tắt nội dung...",
                <= 30 => "👤 AI đang xác định người xử lý và deadline...",
                <= 60 => "📝 AI đang soạn dàn ý phản hồi...",
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
        sb.AppendLine($"📌 THAM MƯU XỬ LÝ VĂN BẢN");
        if (!string.IsNullOrWhiteSpace(_title))
            sb.AppendLine($"📄 {_title}");
        sb.AppendLine();
        sb.AppendLine($"📋 Tóm tắt: {_result.Summary}");
        sb.AppendLine($"🎯 Ưu tiên: {_result.Priority}");

        // Urgency
        if (!string.IsNullOrWhiteSpace(_result.UrgencyLevel) && _result.UrgencyLevel?.ToLower() != "thuong")
        {
            var urgText = _result.UrgencyLevel?.ToLower() switch
            {
                "hoa_toc" => "HỎA TỐC",
                "thuong_khan" => "THƯỢNG KHẨN",
                "khan" => "KHẨN",
                _ => _result.UrgencyLevel
            };
            sb.AppendLine($"🔥 Mức độ khẩn: {urgText}");
        }

        sb.AppendLine($"👤 Đề xuất xử lý: {_result.SuggestedHandler}");
        sb.AppendLine($"📂 Lĩnh vực: {_result.RelatedField}");

        // Coordination
        if (_result.Coordination?.Count > 0)
        {
            sb.AppendLine($"🤝 Phối hợp: {string.Join(", ", _result.Coordination)}");
        }

        // Signing authority
        if (!string.IsNullOrWhiteSpace(_result.SigningAuthority))
        {
            sb.AppendLine($"✍️ Thẩm quyền ký: {_result.SigningAuthority}");
        }

        if (_result.ActionItems?.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("✅ Việc cần làm:");
            foreach (var item in _result.ActionItems)
                sb.AppendLine($"  • {item}");
        }

        if (_result.Deadlines?.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("⏰ Deadline:");
            foreach (var d in _result.Deadlines)
                sb.AppendLine($"  📅 {d.Task}: {d.Date}");
        }

        // Legal references
        if (_result.LegalReferences?.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("📚 Căn cứ pháp lý:");
            foreach (var lr in _result.LegalReferences)
                sb.AppendLine($"  • {lr}");
        }

        if (_result.ResponseNeeded)
        {
            sb.AppendLine();
            sb.AppendLine($"📝 Cần phản hồi bằng: {_result.ResponseType}");
            if (!string.IsNullOrWhiteSpace(_result.DraftResponseOutline))
                sb.AppendLine($"   Dàn ý: {_result.DraftResponseOutline}");
        }

        // Risk warning
        if (!string.IsNullOrWhiteSpace(_result.RiskWarning) && !_result.RiskWarning.Contains("Không có rủi ro"))
        {
            sb.AppendLine();
            sb.AppendLine($"⚠️ Cảnh báo rủi ro: {_result.RiskWarning}");
        }

        Clipboard.SetText(sb.ToString());
        MessageBox.Show("📋 Đã copy kết quả tham mưu vào clipboard!", "Thành công",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void Retry_Click(object sender, RoutedEventArgs e)
    {
        await StartAdvisory();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
