// Quay THẬT bản web VanBanPlus (prod) cho clip viec-that/07 — kế hoạch dài → chọn mẫu có sẵn → AI soạn theo mẫu → Word.
//   VBP_TK=2 node video/cong-cu/quay-web/quay-07.mjs thu   → chạy tới nút «Soạn bằng AI» (KHÔNG bấm, không tốn credit),
//                                                           chụp ảnh từng bước vào video/.tam/quay-07/ + in ước tính credit
//   VBP_TK=2 node video/cong-cu/quay-web/quay-07.mjs 07a   → quay phần TRƯỚC AI (tìm mẫu → mẫu → điền), không bấm AI → web-07a.mp4
//   VBP_TK=2 node video/cong-cu/quay-web/quay-07.mjs 07    → quay thật: /mau → tìm «ke hoach» → mở mẫu → Soạn bằng AI theo mẫu
//                                                           → điền → AI soạn (TRỪ CREDIT THẬT) → Xem bố cục → Xuất Word
// Ra: video/anh/viec-that/web-07.mp4 (30 fps, GOP 15, 1170×2532) + web-07.moc.json + web-07.ket-qua.json + web-07-ke-hoach.docx.
// Tách riêng khỏi quay.mjs (agent khác đang dùng); hàm đăng nhập / máy quay chép nguyên từ đó. Không in mật khẩu / token.
import { createRequire } from "node:module";
import { mkdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { spawnSync } from "node:child_process";

const { chromium } = createRequire("D:/AIVanBanCaNhan/vanbanplus-web/package.json")("playwright");
const WEB = "https://app.vanbanplus.giakiemso.com";
const RA = "D:/AIVanBanCaNhan/video/anh/viec-that";
const THU = "D:/AIVanBanCaNhan/video/.tam/quay-07";
const SO_TK = process.env.VBP_TK === "2" ? "-2" : "";
const TEP_TK = `C:/Users/PC/.gikidi/vanbanplus-demo-web-clip${SO_TK}.txt`;
const W = 390, H = 844;
const che = process.argv[2];
const MAU = "Kế hoạch tổ chức sự kiện";   // mẫu có sẵn san-ke-hoach-to-chuc (nhóm Hành chính)
// Việc hư cấu của UBND xã A / tỉnh B (không tên người thật). Khóa ô của mẫu khớp prompt KeHoach của máy chủ.
// CHỈ 4 Ô NGẮN: tài khoản demo 2 còn 19 credit, máy chủ giữ trước credit_max và từ chối khi số dư < credit_max.
// Đo ước tính 26/11/2026 (POST /api/v2/tac-vu/soan-thao/uoc-tinh, miễn phí): đủ 7 ô → 5–20 (bị chặn «Số dư 19 credit
// chưa đủ»); bộ 4 ô dưới đây → 5–19 (chạy được, đúng một lượt).
const O = [
  ["Tên sự kiện", "Hội nghị tổng kết năm 2026", 45],
  ["Thời gian, địa điểm", "8 giờ ngày 26/12/2026, Hội trường xã", 45],
  ["Kinh phí", "25 triệu đồng", 55],
  ["Chức danh người ký", "Chủ tịch", 55],
];
// Hồ sơ trình duyệt giữ lại giữa các lần chạy: AI xong mà bước sau hỏng thì bản nháp còn ở «Bản soạn chưa lưu → Mở lại».
const HO_SO = "D:/AIVanBanCaNhan/video/.tam/quay-07-ho-so";
mkdirSync(RA, { recursive: true }); mkdirSync(THU, { recursive: true });
const cho = (ms) => new Promise((r) => setTimeout(r, ms));

function taiKhoan() {
  const t = readFileSync(TEP_TK, "utf8");
  const g = (k) => (t.match(new RegExp(`^${k}=\\s*(.*)$`, "m")) || [])[1]?.trim();
  return { email: g("email"), matKhau: g("matkhau") };
}

async function moTrang() {
  const ctx = await chromium.launchPersistentContext(HO_SO, { channel: "msedge", viewport: { width: W, height: H }, deviceScaleFactor: 3,
    isMobile: true, hasTouch: true, colorScheme: "light", locale: "vi-VN", timezoneId: "Asia/Ho_Chi_Minh", acceptDownloads: true });
  const p = ctx.pages()[0] ?? await ctx.newPage();
  p.setDefaultTimeout(30_000);
  return { tr: ctx, ctx, p };
}

async function dangNhap(p) {
  const { email, matKhau } = taiKhoan();
  await p.goto(`${WEB}/dang-nhap`, { waitUntil: "networkidle" }); await cho(1500);
  if (!new URL(p.url()).pathname.startsWith("/dang-nhap")) return;   // hồ sơ còn phiên
  await p.getByLabel("Email").first().fill(email);
  await p.getByLabel("Mật khẩu", { exact: true }).fill(matKhau);
  await p.getByRole("button", { name: "Đăng nhập", exact: true }).click();
  await p.waitForURL((u) => !u.pathname.startsWith("/dang-nhap"), { timeout: 30_000 });
  await cho(2000);
}

/** Máy quay giả (chế độ thu): chỉ ghi mốc + chụp ảnh ở các mốc. */
function mayChup(p) {
  const goc = Date.now() / 1000;
  let i = 0;
  const ghi = async (nhan) => { console.log(`⏱ ${(Date.now() / 1000 - goc).toFixed(2)} ${nhan}`); await p.screenshot({ path: `${THU}/${String(i++).padStart(2, "0")}-${nhan}.png` }); };
  return {
    moc: (nhan) => ghi(nhan),
    async cham(loc, nhan, { bam = true } = {}) { await loc.scrollIntoViewIfNeeded(); await cho(350); await ghi(nhan); if (bam) await loc.tap(); },
    async dung() { await ghi("het"); },
  };
}

/** Máy quay: CDP screencast → khung JPEG có mốc giờ; dung() ghép thành MP4 30 fps đều khung (chép từ quay.mjs). */
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
  await cdp.send("Page.startScreencast", { format: "jpeg", quality: 92, maxWidth: W * 3, maxHeight: H * 3, everyNthFrame: 1 });
  const goc = Date.now() / 1000;
  const moc = [];
  const ghi = (nhan, them = {}) => { const s = +(Date.now() / 1000 - goc).toFixed(2); moc.push({ nhan, s, ...them }); console.log(`⏱ ${s.toFixed(2)} ${nhan}`); };
  return {
    moc: ghi,
    async cham(loc, nhan, { bam = true } = {}) {
      await loc.scrollIntoViewIfNeeded(); await cho(350);
      const b = await loc.boundingBox();
      const hop = b ? [b.x / W, b.y / H, (b.x + b.width) / W, (b.y + b.height) / H].map((v) => +(v * 100).toFixed(1)) : null;
      ghi(nhan, { hop });
      if (bam) await loc.tap();
    },
    async dung() {
      await cho(600);
      ghi("het");
      await cdp.send("Page.stopScreencast");
      await cho(300);
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
        "-vf", `fps=30,scale=${W * 3}:${H * 3}:flags=lanczos,format=yuv420p`, "-c:v", "libx264", "-crf", "17", "-preset", "medium",
        "-g", "15", "-keyint_min", "15", "-sc_threshold", "0", "-movflags", "+faststart", mp4], { stdio: "inherit" });
      if (r.status !== 0) throw new Error("ffmpeg ghép hỏng");
      writeFileSync(`${RA}/web-${ten}.moc.json`, JSON.stringify({ _ghi_chu: "s = giây trong MP4; hop = [x0,y0,x1,y1] % khung 390×844", moc }, null, 1));
      rmSync(thu, { recursive: true, force: true });
      console.log(`✓ ${mp4} (${khung.length} khung)`);
    },
  };
}

/** cuộn mượt cả trang (khung cuộn của os-kit hoặc cửa sổ) tới phần tử, đặt phần tử cách đỉnh `tren` px (chép từ quay.mjs) */
async function cuonToi(p, loc, tren = 90, ms = 900) {
  await loc.evaluate(async (el, [tren, ms]) => {
    const cuon = (() => { for (let e = el.parentElement; e; e = e.parentElement) { const s = getComputedStyle(e).overflowY; if ((s === "auto" || s === "scroll") && e.scrollHeight > e.clientHeight) return e; } return document.scrollingElement; })();
    const tu = cuon.scrollTop, den = tu + el.getBoundingClientRect().top - tren - (cuon === document.scrollingElement ? 0 : cuon.getBoundingClientRect().top);
    const t0 = performance.now();
    await new Promise((xong) => { const b = (t) => { const k = Math.min(1, (t - t0) / ms), e = k < .5 ? 2 * k * k : 1 - (-2 * k + 2) ** 2 / 2; cuon.scrollTop = tu + (den - tu) * e; k < 1 ? requestAnimationFrame(b) : xong(); }; requestAnimationFrame(b); });
  }, [tren, ms]);
  await cho(250);
}

/** kieu: "thu" (chụp ảnh, không quay) · "07a" (quay phần trước AI, không bấm AI) · "07" (quay trọn, AI thật) */
async function q07(kieu) {
  const thu = kieu !== "07";
  const { tr, ctx, p } = await moTrang();
  const ketQua = {};
  try {
    await dangNhap(p);
    await p.goto(`${WEB}/mau`, { waitUntil: "networkidle" }); await cho(1500);
    const tong = p.locator(".mau__tong");
    for (let i = 0; i < 40 && !/\d+ mẫu/.test(await tong.innerText().catch(() => "")); i++) await cho(250);
    const tongDau = await tong.innerText();
    const mq = kieu === "thu" ? mayChup(p) : await mayQuay(ctx, p, kieu);
    await cho(1500); await mq.moc("mau");
    // ① tìm mẫu, gõ không dấu — chờ danh sách lọc xong thật (bản 07 lần đầu bấm khi danh sách CHƯA lọc)
    const tim = p.getByRole("searchbox", { name: "Tìm mẫu" });
    await mq.cham(tim, "cham-tim");
    await tim.pressSequentially("ke hoach", { delay: 110 });
    for (let i = 0; i < 60 && (await tong.innerText()) === tongDau; i++) await cho(250);
    ketQua.tong = `${tongDau} → ${await tong.innerText()}`;
    const muc = p.getByRole("link", { name: new RegExp(`^${MAU}`) });
    await muc.waitFor({ timeout: 15_000 }); await cho(1500);
    ketQua.dsMau = (await p.getByRole("list", { name: "Mẫu" }).innerText()).split("\n").filter(Boolean).slice(0, 24);
    await mq.moc("ds-ke-hoach"); await cho(900);
    await mq.cham(muc, "cham-mau");
    const nutSoan = p.getByRole("link", { name: "Soạn bằng AI theo mẫu" });
    await nutSoan.waitFor({ timeout: 15_000 }); await cho(800);
    await mq.moc("chi-tiet"); await cho(1800);
    // lướt xem ô cần điền + khung mẫu
    const khung = p.getByRole("heading", { name: "Nội dung mẫu" });
    await cuonToi(p, p.getByRole("heading", { name: "Thông tin cần điền khi soạn" }), 80, 900); await mq.moc("o-can-dien"); await cho(1600);
    await cuonToi(p, khung, 70, 1100); await mq.moc("khung-mau"); await cho(1400);
    for (let i = 0; i < 3; i++) { await p.mouse.wheel(0, 280); await cho(700); }
    await mq.moc("khung-luot"); await cho(500);
    await cuonToi(p, nutSoan, 200, 900); await cho(400);
    // ② soạn theo mẫu
    await mq.cham(nutSoan, "cham-soan-theo-mau");
    await p.getByText(`Soạn theo mẫu: ${MAU}`).waitFor({ timeout: 20_000 });
    const oDau = p.getByLabel(O[0][0], { exact: true });
    await oDau.waitFor(); await cho(1200);
    await mq.moc("form-mau"); await cho(1200);
    for (const [nhan, chu, tre] of O) {
      const o = p.getByLabel(nhan, { exact: true }).first();
      await mq.cham(o, `go-${nhan}`);
      await o.pressSequentially(chu, { delay: tre }); await cho(300);
    }
    const nut = p.getByRole("button", { name: /^Soạn bằng AI · \d+–\d+ credit$/ });
    await nut.waitFor({ timeout: 60_000 }); await cho(2500);   // ước tính gọi trễ sau lần gõ cuối
    ketQua.nut = await nut.innerText();
    ketQua.nutKhoa = await nut.isDisabled();
    if (!thu && ketQua.nutKhoa) throw new Error(`Nút AI bị khóa (${ketQua.nut}) — số dư không đủ, dừng trước khi bấm`);
    await cuonToi(p, nut, 420); await cho(700);
    if (thu) {
      await mq.moc("nut-ai"); await cho(1500);
      if (kieu === "07a") await mq.dung();
      console.log(`ℹ ${ketQua.nut} · ${ketQua.tong}`);
      writeFileSync(`${THU}/ket-qua-${kieu}.json`, JSON.stringify(ketQua, null, 1));
      return;
    }
    // ③ AI soạn (trừ credit thật)
    await mq.cham(nut, "cham-soan-ai");
    await p.getByText("AI đang xử lý").waitFor({ timeout: 15_000 }); mq.moc("ai-dang-xu-ly");
    const kq = p.getByLabel("Văn bản soạn");
    await kq.waitFor({ timeout: 330_000 }); mq.moc("ai-xong");
    await cho(800);
    ketQua.ketLuan = await p.locator(".ket-luan-ai").first().innerText().catch(() => "");
    await cuonToi(p, p.locator(".ket-luan-ai").first(), 80, 700); mq.moc("ket-qua-dau"); await cho(2200);
    ketQua.banNhap = await kq.inputValue().catch(async () => kq.innerText());
    writeFileSync(`${RA}/web-07.ket-qua.json`, JSON.stringify(ketQua, null, 1));   // giữ bản nháp ngay, phòng bước sau hỏng
    try {
    for (let i = 0; i < 7; i++) { await p.mouse.wheel(0, 300); await cho(900); }
    mq.moc("luot-xong"); await cho(800);
    // ④ Xem bố cục → Xuất Word
    await mq.cham(p.getByRole("button", { name: "Xem bố cục" }), "cham-bo-cuc");
    const boCuc = p.getByRole("article", { name: "Xem bố cục trang giấy" });
    await boCuc.waitFor({ timeout: 15_000 }); await cho(600);
    await cuonToi(p, boCuc, 70, 900); mq.moc("bo-cuc"); await cho(2500);
    for (let i = 0; i < 3; i++) { await p.mouse.wheel(0, 300); await cho(900); }
    const word = p.getByRole("button", { name: "Xuất Word" });
    await cuonToi(p, word, 380, 800);
    const [tai] = await Promise.all([p.waitForEvent("download", { timeout: 60_000 }), mq.cham(word, "cham-xuat-word")]);
    await tai.saveAs(`${RA}/web-07-ke-hoach.docx`);
    ketQua.tepWord = tai.suggestedFilename();
    mq.moc("da-tai-word"); await cho(2000);
    } catch (e) { console.error(`⚠ sau AI hỏng: ${e.message} — vẫn ghép phần đã quay`); }
    await mq.dung();
    writeFileSync(`${RA}/web-07.ket-qua.json`, JSON.stringify(ketQua, null, 1));
    console.log(`ℹ ${ketQua.nut} · ${ketQua.ketLuan} · Word: ${ketQua.tepWord}`);
  } finally { await tr.close(); }
}

if (["thu", "07a", "07"].includes(che)) await q07(che);
else { console.log("Dùng: quay-07.mjs thu | 07a | 07"); process.exit(1); }
console.log("✓ xong");
process.exit(0);
