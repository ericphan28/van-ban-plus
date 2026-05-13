"""
Convert AIVanBan.Desktop/Views/HelpPage.xaml -> docs/HelpPage_Training.html
Mục đích: Xuất nội dung trang Trợ giúp trong app thành file HTML standalone
         để dùng làm tài liệu training cho end user (in được, share được).

Cách dùng:
    python tools\export_help_to_html.py

Output: docs\HelpPage_Training.html
"""
import re
import xml.etree.ElementTree as ET
from pathlib import Path
from datetime import datetime

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "AIVanBan.Desktop" / "Views" / "HelpPage.xaml"
OUT = ROOT / "docs" / "HelpPage_Training.html"

NS = {
    "x": "http://schemas.microsoft.com/winfx/2006/xaml",
    "wpf": "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
    "md": "http://materialdesigninxaml.net/winfx/xaml/themes",
}
WPF = "{http://schemas.microsoft.com/winfx/2006/xaml/presentation}"
X = "{http://schemas.microsoft.com/winfx/2006/xaml}"


def html_escape(s: str) -> str:
    return (s.replace("&", "&amp;")
             .replace("<", "&lt;")
             .replace(">", "&gt;"))


def get_style(elem) -> str:
    style = elem.get("Style", "")
    m = re.search(r"StaticResource\s+(\w+)", style)
    return m.group(1) if m else ""


def render_inline(elem) -> str:
    """Render TextBlock với inline Run/Bold/Italic."""
    parts = []
    if elem.text:
        parts.append(html_escape(elem.text))
    for child in elem:
        tag = child.tag.replace(WPF, "")
        if tag == "Run":
            text = child.get("Text", child.text or "")
            parts.append(html_escape(text))
        elif tag == "Bold":
            inner = "".join([
                html_escape(child.text or ""),
                *[render_inline(c) for c in child],
            ])
            parts.append(f"<strong>{inner}</strong>")
        elif tag == "Italic":
            inner = "".join([
                html_escape(child.text or ""),
                *[render_inline(c) for c in child],
            ])
            parts.append(f"<em>{inner}</em>")
        elif tag == "LineBreak":
            parts.append("<br>")
        elif tag == "Hyperlink":
            text = "".join([html_escape(child.text or ""),
                            *[render_inline(c) for c in child]])
            href = child.get("NavigateUri", "#")
            parts.append(f'<a href="{href}" target="_blank">{text}</a>')
        else:
            # Recursive for nested
            inner = render_inline(child)
            parts.append(inner)
        if child.tail:
            parts.append(html_escape(child.tail))
    return "".join(parts).strip()


def walk(elem, parts: list):
    """Duyệt cây XAML, render các element nội dung thành HTML."""
    tag = elem.tag.replace(WPF, "")
    style = get_style(elem)

    # TextBlock
    if tag == "TextBlock":
        text = render_inline(elem)
        if not text:
            return
        if style == "SectionHeader":
            parts.append(f'<h2 class="section-header">{text}</h2>')
        elif style == "SubSectionHeader":
            parts.append(f'<h3 class="sub-section-header">{text}</h3>')
        elif style == "BodyText":
            parts.append(f'<p class="body-text">{text}</p>')
        else:
            # Plain inline text
            fw = elem.get("FontWeight", "")
            fs = elem.get("FontSize", "")
            cls = []
            if fw in ("Bold", "SemiBold"):
                cls.append("bold")
            if fs and fs.replace(".", "").isdigit() and float(fs) >= 18:
                cls.append("large")
            cls_attr = f' class="{" ".join(cls)}"' if cls else ""
            parts.append(f'<p{cls_attr}>{text}</p>')
        return

    # Border (TipCard / WarningCard / ChangelogCard / StepCard)
    if tag == "Border":
        cls_map = {
            "TipCard": "tip-card",
            "WarningCard": "warning-card",
            "ChangelogCard": "changelog-card",
            "StepCard": "step-card",
        }
        css_cls = cls_map.get(style, "generic-card") if style else ""
        if css_cls:
            parts.append(f'<div class="{css_cls}">')
            for child in elem:
                walk(child, parts)
            parts.append("</div>")
            return

    # Containers we recurse into
    if tag in ("StackPanel", "Grid", "ScrollViewer", "DockPanel",
               "WrapPanel", "Border", "Page", "Page.Resources",
               "Grid.ColumnDefinitions", "Grid.RowDefinitions"):
        # Skip resource-only children
        if tag.endswith("Definitions"):
            return
        for child in elem:
            walk(child, parts)
        return

    # Skip unhandled (Button, Image, etc.)
    # but still recurse for any nested content
    for child in elem:
        walk(child, parts)


def build_html(body_html: str) -> str:
    today = datetime.now().strftime("%d/%m/%Y")
    return f"""<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="UTF-8">
<title>VanBanPlus — Tài liệu hướng dẫn sử dụng</title>
<style>
  @page {{ size: A4; margin: 18mm 16mm; }}
  * {{ box-sizing: border-box; }}
  body {{
    font-family: 'Segoe UI', 'Calibri', Arial, sans-serif;
    font-size: 13px;
    line-height: 1.55;
    color: #212121;
    max-width: 900px;
    margin: 24px auto;
    padding: 0 24px;
    background: #fff;
  }}
  .doc-header {{
    text-align: center;
    border-bottom: 3px solid #1976D2;
    padding-bottom: 16px;
    margin-bottom: 28px;
  }}
  .doc-header h1 {{
    color: #0D47A1;
    margin: 0 0 4px 0;
    font-size: 26px;
  }}
  .doc-header .subtitle {{ color: #666; font-size: 14px; }}
  .doc-header .meta {{ color: #999; font-size: 11px; margin-top: 6px; }}

  h2.section-header {{
    color: #1976D2;
    font-size: 22px;
    margin: 32px 0 12px;
    padding-bottom: 6px;
    border-bottom: 2px solid #BBDEFB;
    page-break-after: avoid;
  }}
  h3.sub-section-header {{
    color: #0D47A1;
    font-size: 17px;
    margin: 22px 0 8px;
    page-break-after: avoid;
  }}
  p.body-text {{ margin: 4px 0 6px; }}
  p.bold {{ font-weight: 600; }}
  p.large {{ font-size: 16px; }}

  .tip-card, .warning-card, .changelog-card, .step-card, .generic-card {{
    border-radius: 8px;
    padding: 12px 16px;
    margin: 10px 0;
    page-break-inside: avoid;
  }}
  .tip-card     {{ background: #FFF8E1; border: 1px solid #FFE082; }}
  .warning-card {{ background: #FFF3E0; border: 1px solid #FFCC80; }}
  .changelog-card {{ background: #E8F5E9; border: 1px solid #A5D6A7; }}
  .step-card    {{ background: #FAFAFA; border: 1px solid #E0E0E0; }}

  .tip-card p, .warning-card p, .changelog-card p, .step-card p {{ margin: 3px 0; }}

  strong {{ color: #0D47A1; }}
  em {{ color: #424242; }}
  a {{ color: #1565C0; text-decoration: none; }}
  a:hover {{ text-decoration: underline; }}

  .toc {{
    background: #F5F5F5;
    border: 1px solid #E0E0E0;
    border-radius: 8px;
    padding: 16px 24px;
    margin: 24px 0;
    page-break-inside: avoid;
  }}
  .toc h3 {{ margin-top: 0; color: #1976D2; }}
  .toc ul {{ columns: 2; column-gap: 24px; padding-left: 18px; }}
  .toc li {{ margin: 4px 0; font-size: 12.5px; }}

  .doc-footer {{
    margin-top: 48px;
    padding-top: 16px;
    border-top: 1px solid #E0E0E0;
    color: #888;
    font-size: 11px;
    text-align: center;
  }}

  @media print {{
    body {{ font-size: 11.5pt; max-width: none; padding: 0; }}
    h2.section-header {{ font-size: 18pt; page-break-before: always; }}
    h2.section-header:first-of-type {{ page-break-before: auto; }}
    .doc-header {{ page-break-after: avoid; }}
  }}
</style>
</head>
<body>
<div class="doc-header">
  <h1>📘 VanBanPlus — Tài liệu hướng dẫn sử dụng</h1>
  <div class="subtitle">Phần mềm Quản lý Văn bản hành chính cho cán bộ, công chức</div>
  <div class="meta">Phiên bản tài liệu: v1.0.16 · Cập nhật: {today} · Tự động xuất từ HelpPage trong ứng dụng</div>
</div>

{body_html}

<div class="doc-footer">
  © Ericphan / VanBanPlus 2026 · Tài liệu này được sinh tự động từ <code>HelpPage.xaml</code> qua <code>tools/export_help_to_html.py</code>.
</div>
</body>
</html>
"""


def main():
    print(f"Đọc: {SRC}")
    raw = SRC.read_text(encoding="utf-8")
    # Remove default xmlns prefix issue: ET parses ok, but strip any odd BOM
    root = ET.fromstring(raw)
    parts: list[str] = []
    walk(root, parts)
    body = "\n".join(parts)
    html = build_html(body)
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(html, encoding="utf-8")
    print(f"✅ Đã ghi: {OUT}  ({len(html):,} bytes, {len(parts)} blocks)")


if __name__ == "__main__":
    main()
