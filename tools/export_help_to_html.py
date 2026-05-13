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

# Danh sách section thu thập được khi duyệt cây để dựng TOC
SECTIONS: list[dict] = []

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


def slugify(s: str) -> str:
    """Tạo id an toàn từ tiêu đề tiếng Việt."""
    import unicodedata
    s = unicodedata.normalize("NFD", s)
    s = "".join(c for c in s if unicodedata.category(c) != "Mn")
    s = s.replace("đ", "d").replace("Đ", "D")
    s = re.sub(r"[^a-zA-Z0-9\s-]", "", s).strip().lower()
    s = re.sub(r"[\s-]+", "-", s)
    return s or "section"


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
            # Strip emoji/leading symbols để slugify gọn hơn
            plain = re.sub(r"<[^>]+>", "", text)
            sid = slugify(plain)
            # Đảm bảo unique
            base = sid
            i = 2
            existing = {s["id"] for s in SECTIONS}
            while sid in existing:
                sid = f"{base}-{i}"
                i += 1
            SECTIONS.append({"id": sid, "title": plain.strip(), "level": 2})
            parts.append(f'<h2 id="{sid}" class="section-header">{text}</h2>')
        elif style == "SubSectionHeader":
            plain = re.sub(r"<[^>]+>", "", text)
            sid = slugify(plain)
            base = sid
            i = 2
            existing = {s["id"] for s in SECTIONS}
            while sid in existing:
                sid = f"{base}-{i}"
                i += 1
            SECTIONS.append({"id": sid, "title": plain.strip(), "level": 3})
            parts.append(f'<h3 id="{sid}" class="sub-section-header">{text}</h3>')
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


def build_toc_html() -> str:
    """Sinh HTML TOC từ SECTIONS."""
    if not SECTIONS:
        return ""
    items = []
    for s in SECTIONS:
        cls = "toc-h2" if s["level"] == 2 else "toc-h3"
        title = html_escape(s["title"])
        items.append(
            f'<a href="#{s["id"]}" class="{cls}" data-target="{s["id"]}">{title}</a>'
        )
    return "\n".join(items)


def build_html(body_html: str) -> str:
    today = datetime.now().strftime("%d/%m/%Y")
    toc_html = build_toc_html()
    return f"""<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>VanBanPlus — Tài liệu hướng dẫn sử dụng</title>
<style>
  /* ============ RESET & BASE ============ */
  * {{ box-sizing: border-box; margin: 0; padding: 0; }}
  html, body {{
    font-family: 'Segoe UI', 'Calibri', Arial, sans-serif;
    font-size: 14px;
    line-height: 1.6;
    color: #212121;
    background: #FAFAFA;
    height: 100%;
  }}

  /* ============ TOP BAR ============ */
  .topbar {{
    position: sticky; top: 0; z-index: 100;
    background: linear-gradient(135deg, #1976D2 0%, #0D47A1 100%);
    color: #fff;
    padding: 14px 24px;
    display: flex; align-items: center; gap: 16px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  }}
  .topbar-title {{ font-size: 18px; font-weight: 600; flex: 1; }}
  .topbar-meta {{ font-size: 11px; opacity: 0.85; }}
  .topbar-print {{
    background: rgba(255,255,255,0.15); border: 1px solid rgba(255,255,255,0.3);
    color: #fff; padding: 6px 14px; border-radius: 4px; cursor: pointer; font-size: 12px;
  }}
  .topbar-print:hover {{ background: rgba(255,255,255,0.25); }}

  /* ============ LAYOUT 2 CỘT ============ */
  .layout {{
    display: grid;
    grid-template-columns: 280px 1fr;
    gap: 0;
    height: calc(100vh - 56px);
  }}

  /* ============ SIDEBAR (TOC) ============ */
  .sidebar {{
    background: #fff;
    border-right: 1px solid #E0E0E0;
    overflow-y: auto;
    padding: 16px 0;
  }}
  .sidebar-search {{
    margin: 0 16px 12px; position: relative;
  }}
  .sidebar-search input {{
    width: 100%; padding: 8px 12px 8px 32px;
    border: 1px solid #E0E0E0; border-radius: 6px;
    font-size: 13px; outline: none;
  }}
  .sidebar-search input:focus {{ border-color: #1976D2; box-shadow: 0 0 0 2px rgba(25,118,210,0.15); }}
  .sidebar-search::before {{
    content: "🔍"; position: absolute; left: 10px; top: 50%; transform: translateY(-50%); font-size: 12px; opacity: 0.6;
  }}
  .toc-title {{
    font-size: 11px; font-weight: 700; text-transform: uppercase;
    color: #757575; letter-spacing: 0.5px;
    padding: 8px 20px 6px;
  }}
  .sidebar a {{
    display: block; padding: 8px 20px;
    color: #424242; text-decoration: none;
    font-size: 13px; line-height: 1.4;
    border-left: 3px solid transparent;
    transition: all 0.15s;
  }}
  .sidebar a:hover {{ background: #F5F5F5; color: #1976D2; }}
  .sidebar a.toc-h3 {{ padding-left: 36px; font-size: 12.5px; opacity: 0.85; }}
  .sidebar a.active {{
    background: #E3F2FD; color: #0D47A1;
    border-left-color: #1976D2; font-weight: 600;
  }}
  .sidebar a.hidden {{ display: none; }}

  /* ============ CONTENT ============ */
  .content {{
    overflow-y: auto;
    padding: 32px 48px 80px;
    background: #FAFAFA;
  }}
  .content-inner {{ max-width: 920px; margin: 0 auto; }}

  h2.section-header {{
    color: #1976D2; font-size: 24px; font-weight: 700;
    margin: 36px 0 14px; padding-bottom: 8px;
    border-bottom: 2px solid #BBDEFB;
    scroll-margin-top: 80px;
  }}
  h2.section-header:first-of-type {{ margin-top: 0; }}
  h3.sub-section-header {{
    color: #0D47A1; font-size: 18px; font-weight: 600;
    margin: 24px 0 10px;
    scroll-margin-top: 80px;
  }}
  p.body-text {{ margin: 6px 0 8px; }}
  p.bold {{ font-weight: 600; }}
  p.large {{ font-size: 16px; }}
  p {{ margin: 4px 0; }}

  .tip-card, .warning-card, .changelog-card, .step-card, .generic-card {{
    border-radius: 8px;
    padding: 14px 18px;
    margin: 12px 0;
  }}
  .tip-card     {{ background: #FFF8E1; border-left: 4px solid #FFC107; }}
  .warning-card {{ background: #FFF3E0; border-left: 4px solid #FF9800; }}
  .changelog-card {{ background: #E8F5E9; border-left: 4px solid #4CAF50; }}
  .step-card    {{ background: #fff; border: 1px solid #E0E0E0; box-shadow: 0 1px 2px rgba(0,0,0,0.04); }}

  strong {{ color: #0D47A1; }}
  em {{ color: #424242; font-style: italic; }}
  a {{ color: #1565C0; text-decoration: none; }}
  a:hover {{ text-decoration: underline; }}

  /* Scroll-to-top button */
  .scroll-top {{
    position: fixed; bottom: 24px; right: 32px;
    width: 44px; height: 44px; border-radius: 50%;
    background: #1976D2; color: #fff; border: none;
    font-size: 20px; cursor: pointer; opacity: 0;
    transition: opacity 0.2s; box-shadow: 0 4px 12px rgba(0,0,0,0.2);
  }}
  .scroll-top.visible {{ opacity: 0.9; }}
  .scroll-top:hover {{ opacity: 1; }}

  /* ============ RESPONSIVE ============ */
  @media (max-width: 768px) {{
    .layout {{ grid-template-columns: 1fr; height: auto; }}
    .sidebar {{ border-right: none; border-bottom: 1px solid #E0E0E0; max-height: 240px; }}
    .content {{ padding: 20px; }}
  }}

  /* ============ PRINT ============ */
  @media print {{
    .topbar, .sidebar, .scroll-top {{ display: none !important; }}
    .layout {{ display: block; height: auto; }}
    .content {{ padding: 0; overflow: visible; background: #fff; }}
    body {{ font-size: 11.5pt; background: #fff; }}
    h2.section-header {{ page-break-before: always; }}
    h2.section-header:first-of-type {{ page-break-before: auto; }}
    .tip-card, .warning-card, .changelog-card, .step-card {{ page-break-inside: avoid; }}
    @page {{ size: A4; margin: 18mm 16mm; }}
  }}
</style>
</head>
<body>

<header class="topbar">
  <div class="topbar-title">📘 VanBanPlus — Hướng dẫn sử dụng</div>
  <div class="topbar-meta">v1.0.16 · Cập nhật {today}</div>
  <button class="topbar-print" onclick="window.print()">🖨️ In / Lưu PDF</button>
</header>

<div class="layout">

  <aside class="sidebar">
    <div class="sidebar-search">
      <input type="text" id="tocSearch" placeholder="Tìm trong mục lục..." />
    </div>
    <div class="toc-title">Mục lục</div>
    <nav id="tocNav">
{toc_html}
    </nav>
  </aside>

  <main class="content">
    <div class="content-inner">
{body_html}
    </div>
  </main>

</div>

<button class="scroll-top" id="scrollTop" title="Lên đầu trang">↑</button>

<script>
  // 1. Sidebar TOC: highlight link đang xem (scroll spy)
  const links = document.querySelectorAll('.sidebar a');
  const headings = Array.from(document.querySelectorAll('h2.section-header, h3.sub-section-header'));
  const contentEl = document.querySelector('.content');

  function updateActive() {{
    let currentId = '';
    const scrollTop = contentEl.scrollTop + 120;
    for (const h of headings) {{
      if (h.offsetTop <= scrollTop) currentId = h.id;
      else break;
    }}
    links.forEach(l => l.classList.toggle('active', l.dataset.target === currentId));
  }}
  contentEl.addEventListener('scroll', () => {{
    updateActive();
    document.getElementById('scrollTop').classList.toggle('visible', contentEl.scrollTop > 300);
  }});
  updateActive();

  // 2. Smooth scroll khi click TOC (override default vì content có scroll riêng)
  links.forEach(a => {{
    a.addEventListener('click', (e) => {{
      e.preventDefault();
      const id = a.dataset.target;
      const target = document.getElementById(id);
      if (target) {{
        contentEl.scrollTo({{ top: target.offsetTop - 24, behavior: 'smooth' }});
        history.replaceState(null, '', '#' + id);
      }}
    }});
  }});

  // 3. Search filter trong TOC
  document.getElementById('tocSearch').addEventListener('input', (e) => {{
    const q = e.target.value.toLowerCase().trim();
    links.forEach(a => {{
      const match = !q || a.textContent.toLowerCase().includes(q);
      a.classList.toggle('hidden', !match);
    }});
  }});

  // 4. Scroll-to-top
  document.getElementById('scrollTop').addEventListener('click', () => {{
    contentEl.scrollTo({{ top: 0, behavior: 'smooth' }});
  }});

  // 5. Mở đúng anchor khi load có hash
  if (location.hash) {{
    const target = document.querySelector(location.hash);
    if (target) setTimeout(() => contentEl.scrollTo({{ top: target.offsetTop - 24 }}), 50);
  }}
</script>

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
