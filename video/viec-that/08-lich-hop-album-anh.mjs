// Clip 08 · Việc thật: cuối năm làm báo cáo cần ảnh từng hoạt động → ghi cuộc họp vào lịch họp → tạo album từ cuộc họp →
// tải 10 ảnh một lúc → chú thích + ảnh bìa → tháng 12 gõ không dấu tìm lại, mở ảnh, tải về.
// SOÁT (prod 26/11/2026, tài khoản demo-web-clip, quay bằng video/cong-cu/quay-web/quay-08.mjs 08 → video/anh/viec-that/web-08.*):
//  ✔ «Lịch họp» › «Thêm cuộc họp»: ô «Tên cuộc họp», «Trạng thái», «Giờ bắt đầu», «Giờ kết thúc», «Địa điểm», «Chủ trì»
//    (FormCuocHop.tsx) → trang cuộc họp «Thứ Năm, 26/11/2026 · 07:00–10:30», Địa điểm, Chủ trì (ChiTietCuocHop.tsx).
//    Cuộc họp HƯ CẤU «Ra quân trồng cây xanh tuyến đường liên ấp» (UBND xã A, Nguyễn Văn B) tạo qua giao diện.
//  ✔ Trang cuộc họp › khối «Album ảnh» › «Tạo album cho cuộc họp» → hộp «Tạo album», tên điền sẵn «Ảnh: <tên cuộc họp>»
//    (HopAlbum.tsx: `Ảnh: ${cuocHop.tieu_de}`), album gắn cuộc họp («Cuộc họp: …» trên trang album).
//  ✔ «Thêm ảnh» nhận nhiều ảnh một lần (input multiple, hang-tai.ts lô 4); ô «Giảm cỡ ảnh lớn trước khi tải lên (cạnh dài
//    2560 px, đỡ tốn dung lượng)» bật sẵn (giamCo mặc định true, anh.ts). Thật: 10 ảnh gốc 38,2 MB → «10 ảnh · 14,8 MB».
//  ✔ Xem ảnh: ô «Thêm chú thích…», nút «Đặt làm ảnh bìa» → «Đang là ảnh bìa», «Tải ảnh gốc» (XemAnh.tsx).
//  ✔ «Tìm album (gõ không dấu được)»: «trong cay» → đúng 1 album (kho_album.tim = kho_khong_dau(tên + mô tả), tiền tố, 013).
//  ✔ Danh sách album xếp theo ngày sự kiện, có ảnh bìa + «26/11/2026 · 10 ảnh» (DanhSachAlbum.tsx, kho_album_ds).
//  ✔ «Mỗi hoạt động một album, có ngày, có ảnh bìa»: cảnh cầm về = khung thật giây 53,6 của web-08.mp4 (3 album demo:
//    26/11 · 10 ảnh, 10/07 · 3 ảnh, 18/06 · 3 ảnh; hai album cũ tạo qua giao diện bằng quay-08.mjs chuan-bi).
//  ✔ Màn kết: giá đọc lại prod /api/plans 26/11/2026 — «Khởi đầu» price_per_month 99000 (không đổi so với loat.mjs).
//  ✔ Lời không nói: đồng bộ bản cài máy, nhiều người dùng chung kho (NOI-DUNG §9), dung lượng gói (màn kết của loạt nói gói).
//  · Ảnh demo: 16 ảnh CC0 Wikimedia Commons (gốc Unsplash + vườn ươm), không mặt người — nguồn: anh/viec-that/web-08-anh/nguon.json.
//  · «Tải ảnh gốc» mở tab mới (không quay được) → chỉ thấy ngón tay chạm nút.
import { canhKet, canhTomTat, lucNode, chipKhai, dtKhai, goiKhai, cuaSoVideoKhai, videoKhai, mocKhai, lopKhai, hoiKhai } from "../kit.mjs";
import { NGU_CANH, CSS, WEB } from "./loat.mjs";

const A = (ten) => `viec-that/${ten}`;
const V = A("web-08.mp4");
const N = (so) => A(`web-08-anh/nho/IMG_${so}.jpg`);
const DT = { rong: 500, trai: 470, tren: 200, xoay: 0 };   // điện thoại lệch phải, cao ≈ 1104 → đáy 1304

const dtVideo = (id, khoa, u, phu = "") => dtKhai(id, { ...DT, lop: [{ anh: A("web-08-dau.png") }],
  phu: videoKhai(u(V), { id: `${id}-v`, khoa }) + phu }, u);
const zoom = (id, u, vung, tren, hien, an, khoa) => cuaSoVideoKhai(id, u(V), WEB, vung, { trai: 65, tren, rong: 950 }, hien, an, { khoa });

const K_LICH = [["s", 1.3], ["c0k0w1", 1.94], ["c0k1", 8.3], ["c0k2", 10.8], ["c0k3", 13.2], ["c0k3w1", 17.3], ["c0e", 19.6], ["e", 20.6]];
const K_AB = [["s", 20.6], ["c0k1w1", 23.4], ["c0k2", 24.2], ["c0e", 25.6], ["e", 26.9]];
const K_ANH = [["s", 27.0], ["c0k0w1", 28.87], ["c0k1", 29.3], ["c0k2", 37.4], ["c0e", 39.6],
  ["c1", 41.3], ["c1k0w2", 43.3], ["c1k1", 46.0], ["c1k1w2", 46.41], ["c1e", 47.8], ["e", 48.6]];
const K_TIM = [["s", 50.6], ["c0k0", 51.0], ["c0k1", 54.9], ["c0k2", 57.8], ["c0k3", 60.1], ["c0k3w1", 62.9], ["c0k4", 64.2],
  ["c0k4w1", 64.61], ["c0e", 65.5], ["e", 65.8]];

/** ảnh rải trong cảnh móc: [số ảnh, trái, trên, rộng, góc, mốc hiện] */
const RAI = [
  [4714, 70, 600, 330, -7, "s-2"], [3102, 400, 560, 300, 5, "s-2"], [4719, 700, 620, 300, -4, "c0k1"],
  [2851, 90, 880, 300, 6, "c0k1"], [4711, 420, 840, 250, -3, "c0k2"], [4716, 690, 900, 320, 8, "c0k2"],
  [3104, 120, 1110, 290, -5, "c0k2w3"], [4713, 450, 1090, 300, 4, "c0k2w3"], [2850, 760, 1080, 210, -8, "c0k2w5"],
];

export default {
  ten: "08-lich-hop-album-anh",
  tieuDe: "Việc thật · 8 · Ảnh hoạt động lưu theo cuộc họp",
  bia: { dong: ["Ảnh cả năm", "tìm ở đâu?"], cat: [0.10, 0.52] },
  kichBan: [
    { canh: "moc", cau: [
      { loi: "Cuối năm làm báo cáo, cần ảnh từng hoạt động, mà ảnh lẫn hết trong điện thoại.", chu: "Cuối năm làm báo cáo — *ảnh lẫn hết trong điện thoại*" },
      { loi: "Lưu thế nào, để cần là tìm ra ngay?", chu: "Lưu thế nào — *cần là tìm ra?*" },
    ]},
    { canh: "lich", cau: [
      { loi: "Ghi buổi ra quân vào lịch họp, giờ, địa điểm, người chủ trì.", chu: "Lịch họp → *giờ · địa điểm · chủ trì*" },
    ]},
    { canh: "album", cau: [
      { loi: "Trong cuộc họp, chạm Tạo album, tên album điền sẵn.", chu: "Tạo album cho cuộc họp — *tên điền sẵn*" },
    ]},
    { canh: "anh", cau: [
      { loi: "Chạm Thêm ảnh, chọn một lúc mười tấm, ảnh lớn được giảm cỡ cho đỡ tốn dung lượng.", chu: "Thêm *10 ảnh một lúc* — ảnh lớn tự giảm cỡ" },
      { loi: "Ghi chú thích, chọn một tấm làm ảnh bìa.", chu: "Chú thích · *ảnh bìa*" },
    ]},
    { canh: "tim", cau: [
      { loi: "Tháng mười hai cần ảnh, gõ không dấu, album hiện ra, mở ảnh, tải về máy.", chu: "Gõ *trong cay* → album hiện ra → *tải ảnh về*" },
    ]},
    { canh: "cam-ve", cau: [
      { loi: "Mỗi hoạt động một album, có ngày, có ảnh bìa, làm báo cáo chỉ việc chọn ảnh.", chu: "Mỗi hoạt động một album — *có ngày · có ảnh bìa*" },
    ]},
    canhTomTat("Ảnh hoạt động một chỗ", [
      ["Ghi cuộc họp vào lịch", "Ghi cuộc họp vào lịch"],
      ["Mỗi cuộc họp một album ảnh", "Mỗi cuộc họp một album"],
      ["Gõ không dấu là tìm ra", "Gõ không dấu là tìm ra"],
    ]),
    canhKet(),
  ],
  css: CSS + `
.vt-rai{position:absolute;border:10px solid #fff;border-bottom-width:26px;box-shadow:0 16px 36px rgba(15,23,42,.30);border-radius:4px}
`,
  canh: {
    "moc": { khai: true, html: ({ u }) => `
      ${mocKhai(`${NGU_CANH()}Cuối năm làm báo cáo,<br>*ảnh ở đâu?*`, { tren: 250, co: 76, gach: "c0k2" })}
      ${RAI.map(([so, x, y, r, g, h]) => lopKhai(`<img class="vt-rai" src="${u(N(so))}" style="left:${x}px;top:${y}px;width:${r}px;transform:rotate(${g}deg)">`, h, null, "left:0;top:0;width:100%;height:100%")).join("")}
      ${goiKhai("📱 ảnh công việc lẫn ảnh riêng", { trai: 60, tren: 1390, hien: "c0k2", an: "c1", tu: "trai" })}
      ${hoiKhai(540, 1010, "c1k1")}` },

    "lich": { khai: true, html: ({ u }) => `
      ${chipKhai("① Lịch họp")}
      ${dtVideo("dt-lich", K_LICH, u)}
      ${goiKhai("⏩ tua nhanh", { trai: 40, tren: 300, hien: "c0k0+0.3", an: "c0k3w1", tu: "trai", lop: "vt-tua" })}
      ${zoom("cs-hop", u, [0.03, 0.045, 0.97, 0.36], 330, "c0k3w2", "e", K_LICH)}` },

    "album": { khai: true, html: ({ u }) => `
      ${chipKhai("② Tạo album")}
      ${dtVideo("dt-ab", K_AB, u)}
      ${zoom("cs-ten-ab", u, [0.03, 0.44, 0.97, 0.70], 380, "c0k2", "e", K_AB)}` },

    "anh": { khai: true, html: ({ u }) => `
      ${chipKhai("③ Thêm ảnh")}
      ${dtVideo("dt-anh", K_ANH, u)}
      ${goiKhai("⏩ tua nhanh · đang tải ảnh", { trai: 40, tren: 300, hien: "c0k1", an: "c0k2", tu: "trai", lop: "vt-tua" })}
      ${zoom("cs-tai", u, [0.03, 0.42, 0.97, 0.82], 330, "c0k1+0.3", "c0k2", K_ANH)}
      ${zoom("cs-giam", u, [0.03, 0.12, 0.97, 0.48], 330, "c0k2", "c0e+0.4", K_ANH)}
      ${goiKhai("✓ Đặt làm ảnh bìa", { trai: 60, tren: 1150, hien: "c1k1w3", an: "e", tu: "trai" })}` },

    "tim": { khai: true, html: ({ u }) => `
      ${chipKhai("④ Tìm lại")}
      ${dtVideo("dt-tim", K_TIM, u)}
      ${goiKhai("⏩ tua nhanh", { trai: 40, tren: 300, hien: "c0k0+0.3", an: "c0k3", tu: "trai", lop: "vt-tua" })}
      ${zoom("cs-tim", u, [0.03, 0.13, 0.97, 0.58], 300, "c0k2", "c0k3w1", K_TIM)}` },

    "cam-ve": { khai: true, html: ({ u }) => `
      ${chipKhai("📁 Cầm về: ảnh cả năm theo album")}
      ${cuaSoVideoKhai("cs-ds", u(V), WEB, [0.03, 0.17, 0.97, 0.72], { trai: 150, tren: 265, rong: 780 }, "s", null, { khoa: [["s", 53.6]] })}
` },
  },
  mocThu: (tatCa) => [
    ["02-lich-zoom", lucNode(tatCa, "lich", "c0k3w2+0.4")], ["03-ten-ab", lucNode(tatCa, "album", "c0k2+0.6")],
    ["04-dang-tai", lucNode(tatCa, "anh", "c0k1+1")], ["04-giam", lucNode(tatCa, "anh", "c0k2w5")],
    ["05-ds-album", lucNode(tatCa, "tim", "c0k0w4")], ["05-tim", lucNode(tatCa, "tim", "c0k2+0.6")],
  ],
  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1e-0.3"),
};
