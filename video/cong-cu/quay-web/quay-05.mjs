// Clip viec-that/05 — «Tìm lại văn bản năm ngoái: Ctrl+K, gõ không dấu». Quay THẬT bản web prod, KHUNG MÁY TÍNH
// (Ctrl+K là phím tắt bàn phím; trên điện thoại hộp này chỉ mở qua menu tài khoản).
//   node video/cong-cu/quay-web/quay-05.mjs xem    → in danh sách văn bản đang có trong kho tài khoản demo (không ghi gì)
//   node video/cong-cu/quay-web/quay-05.mjs giay   → ảnh «bản gốc» Quyết định hư cấu → video/anh/viec-that/web-05-giay-qd*.{png,jpg}
//   node video/cong-cu/quay-web/quay-05.mjs tao    → tạo văn bản demo năm 2025 QUA GIAO DIỆN (có rồi thì bỏ qua) + đính kèm bản gốc
//   node video/cong-cu/quay-web/quay-05.mjs quay   → quay: Ctrl+K → gõ «to cong nghe so» → mở quyết định → xem bản gốc
// Ra: video/anh/viec-that/web-05.mp4 (30 fps, 2360×1760) + web-05.moc.json (giây + hộp % khung 1180×880) + web-05.ket-qua.json.
// Tài khoản demo-web-clip (tài khoản 1) dùng chung với agent khác: KHÔNG đăng xuất, không xoá gì. Không gọi AI.
import { createRequire } from "node:module";
import { existsSync, mkdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { spawnSync } from "node:child_process";

const { chromium } = createRequire("D:/AIVanBanCaNhan/vanbanplus-web/package.json")("playwright");
const WEB = "https://app.vanbanplus.giakiemso.com";
const RA = "D:/AIVanBanCaNhan/video/anh/viec-that";
const TEP_TK = "C:/Users/PC/.gikidi/vanbanplus-demo-web-clip.txt";
const W = 1180, H = 880, HS = 2;
const che = process.argv[2];
mkdirSync(RA, { recursive: true });
const cho = (ms) => new Promise((r) => setTimeout(r, ms));

// ---- dữ liệu demo HƯ CẤU (xã A / tỉnh B), năm 2025 = «năm ngoái» ----
const QD = {
  huong: "Đi", loai: "QuyetDinh", so: "86", ky_hieu: "QĐ-UBND", ngay: "2025-08-12", co_quan: "UBND xã A",
  trich_yeu: "Về việc thành lập Tổ công nghệ số cộng đồng xã A",
  nguoi_ky: "Nguyễn Văn B", chuc_danh: "Chủ tịch",
  noi_nhan: "Như Điều 3;\nUBND tỉnh B (để b/c);\nLưu: VT, VP.",
  can_cu: "Luật Tổ chức chính quyền địa phương năm 2025",
  noi_dung: "Điều 1. Thành lập Tổ công nghệ số cộng đồng xã A gồm 12 thành viên (danh sách kèm theo).\n" +
    "Điều 2. Tổ có nhiệm vụ hướng dẫn người dân sử dụng dịch vụ công trực tuyến, tài khoản định danh điện tử, thanh toán không dùng tiền mặt.\n" +
    "Điều 3. Quyết định có hiệu lực từ ngày ký. Văn phòng HĐND và UBND xã, các thành viên Tổ chịu trách nhiệm thi hành Quyết định này.",
  tep: "web-05-giay-qd-chup.jpg",
};
const KHAC = [
  { huong: "Đến", loai: "CongVan", so: "512", ky_hieu: "SKHCN-CĐS", ngay: "2025-09-03", co_quan: "Sở Khoa học và Công nghệ tỉnh B",
    trich_yeu: "V/v hướng dẫn hoạt động Tổ công nghệ số cộng đồng năm 2025", nguoi_ky: "Phạm Văn E", chuc_danh: "Phó Giám đốc",
    noi_dung: "Sở Khoa học và Công nghệ hướng dẫn UBND các xã, phường kiện toàn, duy trì hoạt động Tổ công nghệ số cộng đồng; báo cáo kết quả trước ngày 15/12/2025." },
  { huong: "Đi", loai: "KeHoach", so: "45", ky_hieu: "KH-UBND", ngay: "2025-03-20", co_quan: "UBND xã A",
    trich_yeu: "Kế hoạch chuyển đổi số xã A năm 2025", nguoi_ky: "Nguyễn Văn B", chuc_danh: "Chủ tịch",
    noi_dung: "Mục tiêu: 80% hồ sơ thủ tục hành chính được tiếp nhận trực tuyến; 100% cán bộ, công chức xã sử dụng chữ ký số." },
  { huong: "Đi", loai: "BaoCao", so: "212", ky_hieu: "BC-UBND", ngay: "2025-12-15", co_quan: "UBND xã A",
    trich_yeu: "Báo cáo kết quả thực hiện nhiệm vụ phát triển kinh tế - xã hội năm 2025", nguoi_ky: "Nguyễn Văn B", chuc_danh: "Chủ tịch",
    noi_dung: "Tổng thu ngân sách trên địa bàn đạt 105% dự toán; giải quyết việc làm mới cho 320 lao động." },
  { huong: "Đến", loai: "QuyetDinh", so: "1520", ky_hieu: "QĐ-UBND", ngay: "2025-07-01", co_quan: "UBND tỉnh B",
    trich_yeu: "Về việc giao chỉ tiêu tuyển quân năm 2026", nguoi_ky: "Lê Văn G", chuc_danh: "Phó Chủ tịch",
    noi_dung: "Giao chỉ tiêu tuyển chọn, gọi công dân nhập ngũ năm 2026 cho các xã, phường." },
];

function taiKhoan() {
  const t = readFileSync(TEP_TK, "utf8");
  const g = (k) => (t.match(new RegExp(`^${k}=\\s*(.*)$`, "m")) || [])[1]?.trim();
  return { email: g("email"), matKhau: g("matkhau") };
}

async function moTrang() {
  const tr = await chromium.launch({ channel: "msedge" });
  const ctx = await tr.newContext({ viewport: { width: W, height: H }, deviceScaleFactor: HS,
    colorScheme: "light", locale: "vi-VN", timezoneId: "Asia/Ho_Chi_Minh", acceptDownloads: true });
  const p = await ctx.newPage();
  p.setDefaultTimeout(30_000);
  return { tr, ctx, p };
}

async function dangNhap(p) {
  const { email, matKhau } = taiKhoan();
  await p.goto(`${WEB}/dang-nhap`, { waitUntil: "networkidle" });
  await p.getByLabel("Email").first().fill(email);
  await p.getByLabel("Mật khẩu", { exact: true }).fill(matKhau);
  await p.getByRole("button", { name: "Đăng nhập", exact: true }).click();
  await p.waitForURL((u) => !u.pathname.startsWith("/dang-nhap"), { timeout: 30_000 });
  await cho(2000);
}

/** đọc danh sách văn bản (trang /van-ban, ô tìm của danh sách) */
async function dsVanBan(p, q = "") {
  await p.goto(`${WEB}/van-ban`, { waitUntil: "networkidle" }); await cho(1500);
  if (q) { await p.getByRole("searchbox", { name: "Tìm văn bản" }).fill(q); await cho(2500); }
  return (await p.locator(".vo-noi-dung").innerText()).trim();
}

async function xem() {
  const { tr, p } = await moTrang();
  try {
    await dangNhap(p);
    console.log(await dsVanBan(p));
  } finally { await tr.close(); }
}

// ---------------------------------------------------------------- ẢNH BẢN GỐC ----
async function giay() {
  const CSS = `
body{margin:0;background:#fff}
.trang{width:794px;height:1123px;box-sizing:border-box;padding:76px 72px 76px 113px;font:17.4px/1.42 "Times New Roman",serif;color:#111;position:relative}
.dau{width:100%;border-collapse:collapse}.dau td{vertical-align:top;text-align:center;padding:0}
.trai{width:40%}.phai{width:60%}.dam{font-weight:700}
.gach{width:90px;border-top:1px solid #111;margin:2px auto 8px}.gach.dai{width:200px}
.ngay{font-style:italic;margin-top:6px}
.ten{text-align:center;font-weight:700;margin:26px 0 0}.vv{text-align:center;font-weight:700;margin:0}
.gach2{width:140px;border-top:1px solid #111;margin:4px auto 16px}
.tq{text-align:center;font-weight:700;margin:0 0 12px}
p{margin:0 0 8px;text-align:justify;text-indent:38px}.cc{font-style:italic}
.ky{width:100%;margin-top:18px;border-collapse:collapse}.ky td{vertical-align:top;padding:0}
.nn{font-size:14.6px;line-height:1.3;width:50%}.cv{text-align:center}.da-ky{color:#444}`;
  const HTML = `<div class="trang">
<table class="dau"><tr>
  <td class="trai"><div class="dam">ỦY BAN NHÂN DÂN<br>XÃ A</div><div class="gach"></div><div>Số: 86/QĐ-UBND</div></td>
  <td class="phai"><div class="dam">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div><div class="dam">Độc lập - Tự do - Hạnh phúc</div><div class="gach dai"></div><div class="ngay">Xã A, ngày 12 tháng 8 năm 2025</div></td>
</tr></table>
<div class="ten">QUYẾT ĐỊNH</div>
<div class="vv">Về việc thành lập Tổ công nghệ số cộng đồng xã A</div><div class="gach2"></div>
<div class="tq">CHỦ TỊCH ỦY BAN NHÂN DÂN XÃ A</div>
<p class="cc">Căn cứ Luật Tổ chức chính quyền địa phương năm 2025;</p>
<p class="cc">Theo đề nghị của Văn phòng Hội đồng nhân dân và Ủy ban nhân dân xã.</p>
<div class="tq" style="margin-top:12px">QUYẾT ĐỊNH:</div>
<p><b>Điều 1.</b> Thành lập Tổ công nghệ số cộng đồng xã A gồm 12 thành viên (có danh sách kèm theo).</p>
<p><b>Điều 2.</b> Tổ công nghệ số cộng đồng có nhiệm vụ hướng dẫn người dân sử dụng dịch vụ công trực tuyến, tài khoản định danh điện tử, thanh toán không dùng tiền mặt.</p>
<p><b>Điều 3.</b> Quyết định này có hiệu lực từ ngày ký. Văn phòng Hội đồng nhân dân và Ủy ban nhân dân xã, các thành viên Tổ chịu trách nhiệm thi hành Quyết định này./.</p>
<table class="ky"><tr>
  <td class="nn"><b><i>Nơi nhận:</i></b><br>- Như Điều 3;<br>- UBND tỉnh B (để b/c);<br>- Lưu: VT, VP.</td>
  <td class="cv"><b>CHỦ TỊCH</b><br><br><i class="da-ky">(Đã ký)</i><br><br><b>Nguyễn Văn B</b></td>
</tr></table></div>`;
  const tr = await chromium.launch({ channel: "msedge" });
  const p = await tr.newPage({ viewport: { width: 794, height: 1123 }, deviceScaleFactor: 2 });
  await p.setContent(`<html><head><style>${CSS}</style></head><body>${HTML}</body></html>`);
  await p.screenshot({ path: `${RA}/web-05-giay-qd.png` });
  const b64 = readFileSync(`${RA}/web-05-giay-qd.png`).toString("base64");
  const q = await tr.newPage({ viewport: { width: 1200, height: 1600 } });
  await q.setContent(`<html><body style="margin:0;width:1200px;height:1600px;overflow:hidden;
    background:radial-gradient(ellipse at 40% 35%,#9c7b58 0%,#7a5c3e 55%,#4f3a27 100%)">
    <img src="data:image/png;base64,${b64}" style="position:absolute;left:96px;top:110px;width:1010px;transform:rotate(-1.1deg);
      box-shadow:0 18px 40px rgba(0,0,0,.45),0 2px 6px rgba(0,0,0,.3);filter:brightness(.97) contrast(1.04)">
    <div style="position:absolute;inset:0;background:radial-gradient(ellipse at 30% 25%,rgba(255,250,235,.18),rgba(0,0,0,.16) 80%)"></div>
  </body></html>`);
  await q.screenshot({ path: `${RA}/${QD.tep}`, type: "jpeg", quality: 88 });
  await tr.close();
  console.log(`📄 web-05-giay-qd.png · 📷 ${QD.tep}`);
}

// ---------------------------------------------------------------- TẠO DỮ LIỆU ----
async function taoMot(p, vb) {
  const soKh = `${vb.so}/${vb.ky_hieu}`;
  const co = await dsVanBan(p, soKh);
  if (co.includes(vb.trich_yeu)) { console.log(`· đã có ${soKh} — bỏ qua`); return false; }
  await p.goto(`${WEB}/van-ban/moi`, { waitUntil: "networkidle" }); await cho(1200);
  const f = p.locator("form.bm");
  await f.getByRole("button", { name: vb.huong, exact: true }).click();
  await f.getByLabel("Loại", { exact: true }).selectOption(vb.loai);
  await f.getByLabel("Số", { exact: true }).fill(vb.so);
  await f.getByLabel("Ký hiệu", { exact: true }).fill(vb.ky_hieu);
  await f.getByLabel("Ngày ban hành", { exact: true }).fill(vb.ngay);
  await f.getByLabel("Cơ quan ban hành", { exact: true }).fill(vb.co_quan);
  await f.getByLabel(/^Trích yếu/).fill(vb.trich_yeu);
  await f.getByLabel("Người ký", { exact: true }).fill(vb.nguoi_ky);
  await f.getByLabel("Chức danh", { exact: true }).fill(vb.chuc_danh);
  if (vb.noi_nhan) await f.getByLabel("Nơi nhận", { exact: true }).fill(vb.noi_nhan);
  if (vb.can_cu) await f.getByLabel("Căn cứ", { exact: true }).fill(vb.can_cu);
  await f.getByLabel("Nội dung", { exact: true }).fill(vb.noi_dung);
  await f.getByRole("button", { name: "Lưu", exact: true }).click();
  await p.waitForURL(/\/van-ban\/[0-9a-f-]{36}$/, { timeout: 60_000 }); await cho(1500);
  console.log(`✓ đã tạo ${soKh} · ${p.url().split("/").pop()}`);
  if (vb.tep) {
    await p.locator('input[type="file"][aria-label="Thêm tệp"]').setInputFiles(`${RA}/${vb.tep}`);
    await p.getByText("Xong", { exact: true }).waitFor({ timeout: 60_000 }); await cho(1200);
    console.log(`  ✓ đính kèm ${vb.tep}`);
  }
  return true;
}

async function tao() {
  if (!existsSync(`${RA}/${QD.tep}`)) await giay();
  const { tr, p } = await moTrang();
  try {
    await dangNhap(p);
    for (const vb of [...KHAC, QD]) await taoMot(p, vb);   // quyết định tạo SAU CÙNG
  } finally { await tr.close(); }
}

// ---------------------------------------------------------------- MÁY QUAY ----
async function mayQuay(ctx, p, ten) {
  const thu = `${RA}/.khung-${ten}`;
  rmSync(thu, { recursive: true, force: true }); mkdirSync(thu, { recursive: true });
  const cdp = await ctx.newCDPSession(p);
  const khung = [];
  cdp.on("Page.screencastFrame", async (f) => {
    const tep = `${thu}/${String(khung.length).padStart(5, "0")}.jpg`;
    writeFileSync(tep, Buffer.from(f.data, "base64"));
    khung.push({ tep, ts: f.metadata.timestamp });
    try { await cdp.send("Page.screencastFrameAck", { sessionId: f.sessionId }); } catch { /* đã dừng */ }
  });
  await cdp.send("Page.startScreencast", { format: "jpeg", quality: 92, maxWidth: W * HS, maxHeight: H * HS, everyNthFrame: 1 });
  const goc = Date.now() / 1000;
  const moc = [];
  const ghi = (nhan, them = {}) => { const s = +(Date.now() / 1000 - goc).toFixed(2); moc.push({ nhan, s, ...them }); console.log(`⏱ ${s.toFixed(2)} ${nhan}`); };
  const hop = async (loc) => { const b = await loc.boundingBox(); return b ? [b.x / W, b.y / H, (b.x + b.width) / W, (b.y + b.height) / H].map((v) => +(v * 100).toFixed(1)) : null; };
  return {
    moc: ghi, hop,
    async cham(loc, nhan) { await loc.scrollIntoViewIfNeeded(); await cho(300); ghi(nhan, { hop: await hop(loc) }); await loc.click(); },
    async dung() {
      await cho(600); ghi("het");
      await cdp.send("Page.stopScreencast"); await cho(300);
      const dong = [];
      for (let i = 0; i < khung.length; i++) {
        const sau = khung[i + 1]?.ts ?? khung[i].ts + 0.5;
        const tu = i === 0 ? goc : khung[i].ts;
        dong.push(`file '${khung[i].tep}'`, `duration ${Math.max(0.001, sau - tu).toFixed(4)}`);
      }
      dong.push(`file '${khung.at(-1).tep}'`);
      writeFileSync(`${thu}/ds.txt`, dong.join("\n"));
      const mp4 = `${RA}/web-${ten}.mp4`;
      const r = spawnSync("ffmpeg", ["-y", "-v", "error", "-f", "concat", "-safe", "0", "-i", `${thu}/ds.txt`,
        "-vf", `fps=30,scale=${W * HS}:${H * HS}:flags=lanczos,format=yuv420p`, "-c:v", "libx264", "-crf", "17", "-preset", "medium",
        "-g", "15", "-keyint_min", "15", "-sc_threshold", "0", "-movflags", "+faststart", mp4], { stdio: "inherit" });
      if (r.status !== 0) throw new Error("ffmpeg ghép hỏng");
      writeFileSync(`${RA}/web-${ten}.moc.json`, JSON.stringify({ _ghi_chu: `s = giây trong MP4; hop = [x0,y0,x1,y1] % khung ${W}×${H}`, moc }, null, 1));
      rmSync(thu, { recursive: true, force: true });
      console.log(`✓ ${mp4} (${khung.length} khung)`);
    },
  };
}

async function cuonToi(p, loc, tren = 90, ms = 900) {
  await loc.evaluate(async (el, [tren, ms]) => {
    const cuon = (() => { for (let e = el.parentElement; e; e = e.parentElement) { const s = getComputedStyle(e).overflowY; if ((s === "auto" || s === "scroll") && e.scrollHeight > e.clientHeight) return e; } return document.scrollingElement; })();
    const tu = cuon.scrollTop, den = tu + el.getBoundingClientRect().top - tren - (cuon === document.scrollingElement ? 0 : cuon.getBoundingClientRect().top);
    const t0 = performance.now();
    await new Promise((xong) => { const b = (t) => { const k = Math.min(1, (t - t0) / ms), e = k < .5 ? 2 * k * k : 1 - (-2 * k + 2) ** 2 / 2; cuon.scrollTop = tu + (den - tu) * e; k < 1 ? requestAnimationFrame(b) : xong(); }; requestAnimationFrame(b); });
  }, [tren, ms]);
  await cho(250);
}

async function quay() {
  const { tr, ctx, p } = await moTrang();
  const ketQua = {};
  try {
    await dangNhap(p);
    await p.goto(`${WEB}/van-ban`, { waitUntil: "networkidle" }); await cho(2000);
    await p.mouse.move(W / 2, H - 40);
    const mq = await mayQuay(ctx, p, "05");
    await cho(2000); mq.moc("danh-sach");
    mq.moc("ctrl-k");
    await p.keyboard.press("Control+KeyK");
    const o = p.getByRole("combobox", { name: "Từ khóa cần tìm" });
    await o.waitFor(); mq.moc("hop-mo", { hop: await mq.hop(p.locator(".tim-tc")) });
    await cho(1200);
    mq.moc("go");
    await o.pressSequentially("to cong nghe so", { delay: 140 });
    mq.moc("go-xong");
    const muc = p.locator(".tim-tc__muc").filter({ hasText: QD.trich_yeu }).first();
    await muc.waitFor({ timeout: 15_000 }); mq.moc("co-ket-qua", { hop: await mq.hop(p.locator(".tim-tc")) });
    ketQua.hop = await p.locator(".tim-tc").innerText();
    await cho(3200);
    const b = await muc.boundingBox();
    await p.mouse.move(b.x + b.width * 0.4, b.y + b.height / 2, { steps: 12 }); await cho(900);
    mq.moc("chon", { hop: await mq.hop(muc) });
    await muc.click();
    await p.waitForURL(/\/van-ban\/[0-9a-f-]{36}$/, { timeout: 30_000 });
    await p.locator(".vb-ct__thong-tin").waitFor(); mq.moc("chi-tiet", { hop: await mq.hop(p.locator(".vb-ct__thong-tin")) });
    ketQua.thongTin = await p.locator(".vb-ct__thong-tin").innerText();
    await cho(3500);
    const nutXem = p.getByRole("button", { name: `Xem ${QD.tep}` }).or(p.getByRole("button", { name: /^Xem .*\.jpg$/ })).first();
    await cuonToi(p, nutXem, 300, 1000); await cho(600);
    await mq.cham(nutXem, "cham-xem");
    const anh = p.locator(".vb-ct__xem img").first();
    await anh.waitFor({ timeout: 30_000 }); await cho(800);
    await cuonToi(p, anh, 60, 1200); mq.moc("ban-goc", { hop: await mq.hop(anh) });
    await cho(2500);
    await p.mouse.wheel(0, 380); await cho(1800);
    mq.moc("ban-goc-luot");
    await mq.dung();
    writeFileSync(`${RA}/web-05.ket-qua.json`, JSON.stringify(ketQua, null, 1));
    console.log(ketQua.hop, "\n---\n", ketQua.thongTin);
  } finally { await tr.close(); }
}

if (che === "xem") await xem();
else if (che === "giay") await giay();
else if (che === "tao") await tao();
else if (che === "quay") await quay();
else { console.log("Dùng: quay-05.mjs xem | giay | tao | quay"); process.exit(1); }
console.log("✓ xong");
process.exit(0);
