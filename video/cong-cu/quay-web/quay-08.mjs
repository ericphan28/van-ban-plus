// Quay THẬT bản web VanBanPlus (prod) cho clip viec-that/08 «Lịch họp + album ảnh hoạt động lưu một chỗ».
// Khung điện thoại 390×844 ×3, CDP screencast (JPEG 92) — cùng cách của quay.mjs (hàm chép lại, quay.mjs chạy lệnh khi import).
//   node video/cong-cu/quay-web/quay-08.mjs xem       → dung lượng, album, cuộc họp hiện có (chỉ đọc)
//   node video/cong-cu/quay-web/quay-08.mjs chuan-bi  → 2 album cũ (sơ kết 6 tháng, đồng ruộng Hè Thu) qua giao diện; có rồi thì bỏ qua
//   node video/cong-cu/quay-web/quay-08.mjs don       → quay lại: xóa ĐÚNG cuộc họp + album do lệnh 08 tạo (không đụng dữ liệu khác)
//   node video/cong-cu/quay-web/quay-08.mjs 08        →thêm cuộc họp → tạo album từ cuộc họp → tải 10 ảnh → ảnh bìa → tìm không dấu
// Ảnh tải lên: video/anh/viec-that/web-08-anh/ (CC0 Wikimedia Commons, không mặt người — nguồn trong nguon.json).
// Ra: video/anh/viec-that/web-08.mp4 + web-08.moc.json + web-08.ket-qua.json. Không AI, không trừ credit. Không in mật khẩu.
import { createRequire } from "node:module";
import { mkdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { spawnSync } from "node:child_process";

const { chromium } = createRequire("D:/AIVanBanCaNhan/vanbanplus-web/package.json")("playwright");
const WEB = "https://app.vanbanplus.giakiemso.com";
const RA = "D:/AIVanBanCaNhan/video/anh/viec-that";
const ANH = `${RA}/web-08-anh`;
const TEP_TK = `C:/Users/PC/.gikidi/vanbanplus-demo-web-clip${process.env.VBP_TK === "2" ? "-2" : ""}.txt`;
const W = 390, H = 844;
const che = process.argv[2];
const cho = (ms) => new Promise((r) => setTimeout(r, ms));

const HOP = {
  ten: "Ra quân trồng cây xanh tuyến đường liên ấp",
  ngay: "2026-11-26", bat: "07:00", ket: "10:30",
  diaDiem: "Tuyến đường liên ấp 2 – ấp 3, xã A",
  chuTri: "Nguyễn Văn B, Chủ tịch UBND xã A",
};
const ANH_CHINH = Array.from({ length: 10 }, (_, k) => `${ANH}/IMG_${4711 + k}.jpg`);
const ANH_BIA = "IMG_4715.jpg";
const CHU_THICH = "Tuyến đường liên ấp sau đợt trồng cây";
const ALBUM_CU = [
  { ten: "Hội nghị sơ kết 6 tháng đầu năm 2026", ngay: "2026-07-10", tep: [3102, 3103, 3104] },
  { ten: "Kiểm tra đồng ruộng vụ Hè Thu 2026", ngay: "2026-06-18", tep: [2850, 2851, 2852] },
];

function taiKhoan() {
  const t = readFileSync(TEP_TK, "utf8");
  const g = (k) => (t.match(new RegExp(`^${k}=\\s*(.*)$`, "m")) || [])[1]?.trim();
  return { email: g("email"), matKhau: g("matkhau") };
}

async function moTrang({ chapNhan = false } = {}) {
  // --lang=vi-VN: ô ngày / giờ hiện «26/11/2026», «07:00 SA» thay kiểu Mỹ (locale của ngữ cảnh không đổi được ô này)
  const tr = await chromium.launch({ channel: "msedge", args: ["--lang=vi-VN"] });
  const ctx = await tr.newContext({ viewport: { width: W, height: H }, deviceScaleFactor: 3, isMobile: true, hasTouch: true,
    colorScheme: "light", locale: "vi-VN", timezoneId: "Asia/Ho_Chi_Minh", acceptDownloads: true });
  const p = await ctx.newPage();
  p.setDefaultTimeout(30_000);
  // hộp «Xóa…»: chỉ lệnh «don» (xóa đúng cuộc họp + album của clip 08) mới chấp nhận
  p.on("dialog", (d) => (chapNhan ? d.accept() : d.dismiss()).catch(() => {}));
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

async function cuonToi(p, loc, tren = 90, ms = 900) {
  await loc.evaluate(async (el, [tren, ms]) => {
    const cuon = (() => { for (let e = el.parentElement; e; e = e.parentElement) { const s = getComputedStyle(e).overflowY; if ((s === "auto" || s === "scroll") && e.scrollHeight > e.clientHeight) return e; } return document.scrollingElement; })();
    const tu = cuon.scrollTop, den = tu + el.getBoundingClientRect().top - tren - (cuon === document.scrollingElement ? 0 : cuon.getBoundingClientRect().top);
    const t0 = performance.now();
    await new Promise((xong) => { const b = (t) => { const k = Math.min(1, (t - t0) / ms), e = k < .5 ? 2 * k * k : 1 - (-2 * k + 2) ** 2 / 2; cuon.scrollTop = tu + (den - tu) * e; k < 1 ? requestAnimationFrame(b) : xong(); }; requestAnimationFrame(b); });
  }, [tren, ms]);
  await cho(250);
}

/** đọc chữ trang (tên album + số ảnh) */
const chuTrang = async (p) => (await p.locator("body").innerText()).split("\n").filter(Boolean);

async function xem() {
  const { tr, p } = await moTrang();
  try {
    await dangNhap(p);
    await p.goto(`${WEB}/tai-khoan`, { waitUntil: "networkidle" }); await cho(1500);
    console.log((await chuTrang(p)).filter((d) => /Dung lượng|MB|GB|credit/.test(d)).join("\n"));
    await p.goto(`${WEB}/album`, { waitUntil: "networkidle" }); await cho(1500);
    console.log("— album:", (await chuTrang(p)).slice(3, 20).join(" | "));
    await p.goto(`${WEB}/lich-hop?xem=danh-sach`, { waitUntil: "networkidle" }); await cho(1500);
    console.log("— lịch:", (await chuTrang(p)).slice(3, 20).join(" | "));
  } finally { await tr.close(); }
}

/** tải ảnh vào album đang mở, chờ bảng tải báo xong */
async function taiAnh(p, tep, mq) {
  const [chon] = await Promise.all([p.waitForEvent("filechooser"),
    mq ? mq.cham(p.getByRole("button", { name: "Thêm ảnh", exact: true }), "cham-them-anh") : p.getByRole("button", { name: "Thêm ảnh", exact: true }).tap()]);
  await chon.setFiles(tep);
  await p.getByRole("heading", { name: /^Đang tải ảnh lên/ }).waitFor({ timeout: 20_000 }).catch(() => {});
  mq?.moc("dang-tai");
  await p.getByRole("heading", { name: `Đã tải ${tep.length}/${tep.length} ảnh` }).waitFor({ timeout: 300_000 });
  mq?.moc("tai-xong");
}

async function chuanBi() {
  const { tr, p } = await moTrang();
  try {
    await dangNhap(p);
    for (const a of ALBUM_CU) {
      await p.goto(`${WEB}/album`, { waitUntil: "networkidle" }); await cho(1500);
      if ((await chuTrang(p)).includes(a.ten)) { console.log(`· đã có album «${a.ten}» — bỏ qua`); continue; }
      await p.getByRole("button", { name: "Tạo album", exact: true }).tap();
      await p.getByLabel("Tên album").fill(a.ten);
      await p.getByLabel("Ngày sự kiện").fill(a.ngay);
      await p.locator("form").getByRole("button", { name: "Tạo album", exact: true }).tap();
      await p.waitForURL(/\/album\/[0-9a-f-]{36}$/);
      await cho(1200);
      await taiAnh(p, a.tep.map((n) => `${ANH}/IMG_${n}.jpg`));
      console.log(`✓ album «${a.ten}» + ${a.tep.length} ảnh`);
    }
  } finally { await tr.close(); }
}

async function q08() {
  const { tr, ctx, p } = await moTrang();
  const kq = {};
  try {
    await dangNhap(p);
    await p.goto(`${WEB}/lich-hop?xem=danh-sach`, { waitUntil: "networkidle" }); await cho(1500);
    if ((await chuTrang(p)).some((d) => d.includes(HOP.ten))) throw new Error("Cuộc họp đã có — quay lại thì xóa cuộc họp + album cũ của clip 08 trước.");
    const mq = await mayQuay(ctx, p, "08");
    await cho(1500); mq.moc("lich");
    // ① thêm cuộc họp
    await mq.cham(p.getByRole("link", { name: "Thêm cuộc họp" }), "cham-them-hop");
    await p.getByRole("heading", { name: "Thêm cuộc họp" }).waitFor(); await cho(900); mq.moc("form");
    const go = async (nhan, chu, moc, tre = 45) => { const o = p.getByLabel(nhan, { exact: true }).first(); await mq.cham(o, moc); await o.pressSequentially(chu, { delay: tre }); await cho(350); };
    await go("Tên cuộc họp", HOP.ten, "go-ten");
    const tt = p.locator("label.bm__o", { has: p.locator("span", { hasText: /^Trạng thái$/ }) }).locator("select");
    await mq.cham(tt, "chon-trang-thai", { bam: false }); await tt.selectOption("da_hop"); await cho(500);
    const gio = async (nhan, gt, moc) => { const o = p.getByLabel(nhan, { exact: true }); await cuonToi(p, o, 300, 500); await mq.cham(o, moc, { bam: false }); await o.fill(gt); await cho(450); };
    await gio("Giờ bắt đầu", HOP.bat, "gio-bat");
    await gio("Giờ kết thúc", HOP.ket, "gio-ket");
    await go("Địa điểm", HOP.diaDiem, "go-dia-diem", 40);
    await go("Chủ trì", HOP.chuTri, "go-chu-tri", 40);
    const luu = p.getByRole("button", { name: "Thêm cuộc họp", exact: true });
    await cuonToi(p, luu, 420, 1100); await cho(400);
    await mq.cham(luu, "cham-luu-hop");
    await p.waitForURL(/\/lich-hop\/[0-9a-f-]{36}$/, { timeout: 30_000 }); await cho(700); mq.moc("chi-tiet");
    kq.cuocHop = p.url().split("/").at(-1);
    await cho(1800);
    // ② tạo album từ cuộc họp
    const taoAb = p.getByRole("link", { name: "Tạo album cho cuộc họp" });
    await cuonToi(p, taoAb, 380, 1300); mq.moc("khoi-album"); await cho(900);
    await mq.cham(taoAb, "cham-tao-album");
    const tenAb = p.getByLabel("Tên album");
    await tenAb.waitFor(); await cho(600); mq.moc("hop-album");
    kq.tenAlbum = await tenAb.inputValue();
    const ngay = p.getByLabel("Ngày sự kiện");
    await mq.cham(ngay, "ngay-su-kien", { bam: false }); await ngay.fill(HOP.ngay); await cho(900);
    await mq.cham(p.locator("form").getByRole("button", { name: "Tạo album", exact: true }), "cham-tao");
    await p.waitForURL(/\/album\/[0-9a-f-]{36}$/); await cho(600); mq.moc("album-moi");
    kq.album = p.url().split("/").at(-1);
    await cho(1800);
    // ③ tải 10 ảnh
    await taiAnh(p, ANH_CHINH, mq);
    kq.bangTai = (await p.locator(".ab-tai").innerText().catch(() => "")).split("\n").slice(0, 4);
    await cho(1800);
    await mq.cham(p.getByRole("button", { name: "Đóng", exact: true }), "cham-dong-bang");
    await cho(700); mq.moc("luoi");
    kq.dauAlbum = (await p.locator(".ab-dau").innerText()).split("\n");
    await cho(2200);
    // ④ xem ảnh, chú thích, đặt ảnh bìa
    await mq.cham(p.getByRole("button", { name: `Xem ảnh ${ANH_BIA}` }), "cham-anh");
    const hop = p.getByRole("dialog"); await hop.waitFor(); await cho(1500); mq.moc("xem-anh");
    const ct = hop.getByPlaceholder("Thêm chú thích…");
    await mq.cham(ct, "go-chu-thich"); await ct.pressSequentially(CHU_THICH, { delay: 45 }); await cho(500);
    await mq.cham(hop.getByRole("button", { name: "Đặt làm ảnh bìa" }), "cham-bia");
    await hop.getByRole("button", { name: "Đang là ảnh bìa" }).waitFor({ timeout: 15_000 }); mq.moc("da-bia"); await cho(1800);
    await mq.cham(hop.getByRole("button", { name: "Đóng" }), "dong-xem"); await cho(1200);
    // ⑤ «cuối năm»: danh sách album → tìm không dấu → mở → tải ảnh về
    await mq.cham(p.getByRole("link", { name: "‹ Album ảnh" }), "cham-ds-album");
    await p.getByRole("heading", { name: "Album ảnh", exact: true }).waitFor(); await cho(500);
    await p.locator(".ab-the__bia img").first().waitFor({ timeout: 15_000 }).catch(() => {});
    await cho(1200); mq.moc("ds-album");
    kq.dsAlbum = (await p.locator(".ab-ds").innerText()).split("\n");
    await cho(1800);
    const tim = p.getByRole("searchbox", { name: "Tìm album" });
    await mq.cham(tim, "go-tim"); await tim.pressSequentially("trong cay", { delay: 140 }); await cho(500);
    await tim.press("Enter"); mq.moc("enter-tim");
    await p.waitForFunction(() => document.querySelectorAll(".ab-ds li").length === 1, null, { timeout: 15_000 });
    await cho(700); mq.moc("ket-qua-tim");
    kq.ketQuaTim = (await p.locator(".ab-ds").innerText()).split("\n");
    await cho(1800);
    await mq.cham(p.locator(".ab-the").first(), "cham-album-tim");
    await p.waitForURL(/\/album\/[0-9a-f-]{36}$/); await p.locator(".ab-luoi img").first().waitFor(); await cho(1500); mq.moc("album-mo");
    await mq.cham(p.getByRole("button", { name: `Xem ảnh ${ANH_BIA}` }), "cham-anh-2");
    await p.getByRole("dialog").waitFor(); await cho(1300); mq.moc("xem-anh-2");
    const [tab] = await Promise.all([ctx.waitForEvent("page", { timeout: 20_000 }).catch(() => null),
      mq.cham(p.getByRole("dialog").getByRole("button", { name: "Tải ảnh gốc" }), "cham-tai-goc")]);
    if (tab) { await tab.waitForLoadState().catch(() => {}); kq.taiGoc = tab.url().split("?")[0].split("/").slice(-1)[0]; await tab.close(); }
    await cho(1500); mq.moc("sau-tai");
    await mq.dung();
    writeFileSync(`${RA}/web-08.ket-qua.json`, JSON.stringify(kq, null, 1));
    console.log(JSON.stringify(kq, null, 1));
  } finally { await tr.close(); }
}

/** quay lại: xóa ĐÚNG album «Ảnh: <HOP.ten>» và cuộc họp HOP.ten do lệnh 08 tạo (không đụng dữ liệu khác) */
async function don() {
  const { tr, p } = await moTrang({ chapNhan: true });
  try {
    await dangNhap(p);
    for (;;) {
      await p.goto(`${WEB}/album`, { waitUntil: "networkidle" }); await cho(1200);
      const the = p.locator(".ab-the", { has: p.locator(".ab-the__ten", { hasText: new RegExp(`^Ảnh: ${HOP.ten}$`) }) });
      if (!(await the.count())) break;
      await the.first().tap(); await p.waitForURL(/\/album\/[0-9a-f-]{36}$/); await cho(1000);
      await p.getByRole("button", { name: "Xóa album" }).tap(); await p.waitForURL(/\/album$/); console.log("✓ đã xóa album clip 08");
    }
    for (;;) {
      await p.goto(`${WEB}/lich-hop?xem=danh-sach`, { waitUntil: "networkidle" }); await cho(1200);
      const lk = p.getByRole("link", { name: HOP.ten });
      if (!(await lk.count())) break;
      await lk.first().tap(); await p.waitForURL(/\/lich-hop\/[0-9a-f-]{36}$/); await cho(1000);
      if ((await p.locator("h1").innerText()).trim() !== HOP.ten) throw new Error("mở nhầm cuộc họp");
      await p.getByRole("button", { name: "Xóa", exact: true }).tap(); await p.waitForURL(/\/lich-hop(\?|$)/); console.log("✓ đã xóa cuộc họp clip 08");
    }
  } finally { await tr.close(); }
}

if (che === "xem") await xem();
else if (che === "don") await don();
else if (che === "chuan-bi") await chuanBi();
else if (che === "08") await q08();
else { console.log("Dùng: quay-08.mjs xem | chuan-bi | don | 08"); process.exit(1); }
console.log("✓ xong");
process.exit(0);
