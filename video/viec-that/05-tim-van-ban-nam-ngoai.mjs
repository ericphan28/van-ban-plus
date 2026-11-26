// Clip 05 · Việc thật: lãnh đạo hỏi quyết định năm ngoái số mấy, ký ngày nào → Ctrl+K, gõ không dấu → mở văn bản + bản gốc.
// QUAY KHUNG MÁY TÍNH: Ctrl+K là phím tắt bàn phím (trên điện thoại hộp tìm chỉ mở qua menu tài khoản «Tìm mọi nơi…»).
// SOÁT (prod 26/11/2026, tài khoản demo-web-clip, quay bằng video/cong-cu/quay-web/quay-05.mjs → video/anh/viec-that/web-05.*):
//  ✔ Ctrl/Cmd+K mở hộp «Tìm mọi nơi» ở MỌI trang đã đăng nhập (useTimToanCuc trong vo/Vo.tsx bọc mọi route của App.tsx).
//  ✔ Chữ gợi ý nguyên văn «Tìm văn bản, cuộc họp, mẫu, pháp quy — gõ không dấu cũng được»; timToanCuc gọi song song 4 nơi
//    (vo/tim-toan-cuc.ts), mỗi nhóm ≤ 5 mục.
//  ✔ Tìm không dấu: cột tim = to_tsvector(kho_khong_dau(số, ký hiệu, trích yếu, cơ quan, nội dung)) — unaccent + bỏ đ
//    (010_kho.sql); tachTuKhoa bỏ dấu từ khóa; mọi từ khớp tiền tố (015_mo_ban.sql kho_van_ban_tim).
//  ✔ Gõ «to cong nghe so» → 0,84 s sau khi gõ xong hiện 2 văn bản (nhóm VĂN BẢN), mỗi dòng «số/ký hiệu · cơ quan · ngày ban hành».
//  ✔ Bấm dòng → /van-ban/<id>: Số / ký hiệu 86/QĐ-UBND · Ngày ban hành 12/08/2025 · Người ký Nguyễn Văn B – Chủ tịch.
//  ✔ «Tệp đính kèm» → nút «Xem» mở ảnh ngay trong trang (DanhSachTep.tsx); ảnh ở màn «cầm về» là CHÍNH tệp đã đính kèm.
//  ✔ Giá màn kết: GET /api/plans 26/11/2026 «Khởi đầu» price_per_month 99000 (không đổi so với loat.mjs).
//  · Dữ liệu demo HƯ CẤU tạo qua giao diện 26/11/2026 (quay-05.mjs tao): 86/QĐ-UBND (+ ảnh bản gốc web-05-giay-qd-chup.jpg),
//    512/SKHCN-CĐS, 45/KH-UBND, 212/BC-UBND, 1520/QĐ-UBND — tỉnh B / xã A, không tên người thật.
//  · Tên tệp «web-05-giay-qd-chup.jpg» trên khung xem → không phóng dòng tên tệp.
import { canhKet, canhTomTat, lucNode, chipKhai, mtKhai, sangKhai, chamKhai, goiKhai, cuaSoKhai, cuaSoVideoKhai, videoKhai, mocKhai, lopKhai, hoiKhai } from "../kit.mjs";
import { NGU_CANH, CSS } from "./loat.mjs";

const A = (ten) => `viec-that/${ten}`;
const V = A("web-05.mp4");
const MT = [2360, 1760];
const MAY = { rong: 940, trai: 70, tren: 270 };

/** máy tính chạy bản quay theo `khoa` */
const mtVideo = (id, khoa, u, phu = "", them = {}) => mtKhai(id, { ...MAY, ...them, lop: [{ anh: A("web-05-dau.png") }],
  phu: videoKhai(u(V), { id: `${id}-v`, khoa }) + phu }, u);
const zoom = (id, u, vung, cs, hien, an, khoa, phu = "") => cuaSoVideoKhai(id, u(V), MT, vung, cs, hien, an, { khoa }, phu);

// vùng hộp tìm (x 0,232–0,68 của khung): ô tìm + chữ gợi ý · ô tìm + danh sách kết quả
const V_O = [0.232, 0.115, 0.68, 0.205];
const V_DS = [0.232, 0.115, 0.68, 0.42];
const V_CT = [0.63, 0.18, 0.97, 0.47];      // khung chi tiết: trích yếu, số / ký hiệu, ngày, cơ quan, người ký
const V_GOC = [0.65, 0.255, 0.96, 0.80];    // khung xem ảnh bản gốc (dưới dòng tên tệp)

const K_TIM = [["s", 1.0], ["c0k0w2", 1.97], ["c0k1", 2.3], ["c0e", 3.25], ["c1k1", 3.3], ["c1k1e", 5.7], ["c1k2", 6.3], ["c1e", 7.6]];
const K_KQ = [["s", 7.6], ["c1", 9.9], ["c1k0w0", 10.72], ["c1k0w0+0.35", 10.8], ["c1k0w0+0.4", 11.86], ["c1k1", 12.0], ["c1e", 14.6]];   // cắt cứng qua lúc chuyển trang
const K_GOC = [["s", 14.6], ["c0k1", 17.3], ["c0k1w1", 17.62], ["c0k2", 18.6], ["c0k2e", 20.85], ["e", 23.5]];

const phim = (chu) => `<span class="vt05-phim">${chu}</span>`;

export default {
  ten: "05-tim-van-ban-nam-ngoai",
  tieuDe: "Việc thật · 5 · Tìm lại văn bản năm ngoái",
  bia: { dong: ["Văn bản cũ?", "Bấm Ctrl+K"], cat: [0.08, 0.5] },
  kichBan: [
    { canh: "moc", cau: [
      { loi: "Lãnh đạo hỏi, quyết định năm ngoái, số mấy, ký ngày nào?", chu: "Lãnh đạo hỏi: quyết định năm ngoái — *số mấy, ký ngày nào?*" },
      { loi: "Khỏi lục tủ hồ sơ, tìm ngay trên trình duyệt.", chu: "Khỏi lục tủ hồ sơ — *tìm ngay trên trình duyệt*" },
    ]},
    { canh: "tim", cau: [
      { loi: "Bấm hai phím tắt, hộp tìm bật lên, tìm cả văn bản, cuộc họp, mẫu, và pháp quy.", chu: "*Ctrl + K* — tìm văn bản · cuộc họp · mẫu · pháp quy" },
      { loi: "Gõ không dấu, tổ công nghệ số, kết quả hiện ngay.", chu: "Gõ không dấu *to cong nghe so* → ra ngay" },
    ]},
    { canh: "ket-qua", cau: [
      { loi: "Hai văn bản khớp, có đủ số, cơ quan, ngày ban hành.", chu: "2 văn bản khớp — *số · cơ quan · ngày*" },
      { loi: "Bấm vào quyết định, số tám mươi sáu, ký ngày mười hai tháng tám năm ngoái.", chu: "*86/QĐ-UBND* · ngày 12/08/2025" },
    ]},
    { canh: "ban-goc", cau: [
      { loi: "Ảnh bản gốc đính kèm cùng văn bản, bấm Xem, là mở ngay.", chu: "Bản gốc đính kèm — *bấm Xem là mở*", nghiSau: 0.5 },
    ]},
    { canh: "cam-ve", cau: [
      { loi: "Số, ngày ký, người ký, và bản gốc, đủ để trả lời lãnh đạo.", chu: "Số · ngày ký · người ký · *bản gốc*" },
    ]},
    canhTomTat("Tìm lại văn bản cũ", [
      ["Bấm hai phím tắt", "Bấm Ctrl + K"],
      ["Gõ không dấu cũng ra", "Gõ không dấu cũng ra"],
      ["Mở luôn bản gốc đính kèm", "Mở luôn bản gốc"],
    ]),
    canhKet(),
  ],
  css: CSS + `
.vt05-phim{display:inline-block;min-width:96px;padding:14px 26px;margin:0 6px;border-radius:18px;background:#fff;color:#0f172a;
  font-size:64px;font-weight:800;line-height:1;text-align:center;border:3px solid #cbd5e1;box-shadow:0 8px 0 #94a3b8,0 16px 30px rgba(15,23,42,.3)}
.vt05-cong{font-size:56px;font-weight:800;color:#0f172a;margin:0 4px}
.vt05-the{font-size:40px;line-height:1.35;font-weight:600}.vt05-the b{color:var(--mau-chinh)}`,
  canh: {
    "moc": { khai: true, html: ({ u }) => `
      ${mocKhai(`${NGU_CANH()}Quyết định năm ngoái —<br>*số mấy, ký ngày nào?*`, { tren: 250, co: 70, gach: "c0k2" })}
      ${mtKhai("mt-moc", { rong: 940, trai: 70, tren: 560, lop: [{ anh: A("web-05-dau.png") }] }, u)}
      ${hoiKhai(880, 640, "c0k2")}
      ${goiKhai("👔 Lãnh đạo hỏi · cần ngay", { trai: 60, tren: 1090, hien: "c0k0", tu: "trai" })}
      ${goiKhai("📂 Tủ hồ sơ? Không cần", { trai: 470, tren: 1200, hien: "c1k0", tu: "phai" })}` },

    "tim": { khai: true, html: ({ u }) => `
      ${chipKhai("① Ctrl + K · gõ không dấu")}
      ${mtVideo("mt-tim", K_TIM, u)}
      ${goiKhai(`${phim("Ctrl")}<span class="vt05-cong">+</span>${phim("K")}`, { trai: 300, tren: 760, hien: "c0k0w1", an: "c0k1+0.4", tu: "duoi" })}
      ${zoom("cs-o", u, V_O, { trai: 65, tren: 860, rong: 950 }, "c0k1+0.1", "c1k1", K_TIM,
        sangKhai([2, 18, 92, 75], "c0k2", "c1k0"))}
      ${zoom("cs-ds", u, V_DS, { trai: 65, tren: 760, rong: 950 }, "c1k1", "e", K_TIM)}` },

    "ket-qua": { khai: true, html: ({ u }) => `
      ${chipKhai("② Ra ngay · mở văn bản")}
      ${mtVideo("mt-kq", K_KQ, u)}
      ${zoom("cs-ds2", u, V_DS, { trai: 65, tren: 760, rong: 950 }, "s", "c1k0w0+0.35", K_KQ,
        sangKhai([1.5, 52, 89, 63], "c0k1", "c1") + sangKhai([1.5, 72, 89, 83], "c0k1", "c1") +
        sangKhai([1.5, 64.5, 89, 84], "c1k0", "c1k0w0+0.35") + chamKhai(40, 72, "c1k0w0-0.05"))}
      ${zoom("cs-ct", u, V_CT, { trai: 65, tren: 660, rong: 950 }, "c1k0w0+0.4", "e", K_KQ,
        sangKhai([1, 60, 45, 77], "c1k1", "e") + sangKhai([47, 60, 99, 77], "c1k2", "e"))}` },

    "ban-goc": { khai: true, html: ({ u }) => `
      ${chipKhai("③ Bản gốc đính kèm")}
      ${mtVideo("mt-goc", K_GOC, u, chamKhai(70.5, 62.9, "c0k1w1"))}
      ${zoom("cs-goc", u, V_GOC, { trai: 190, tren: 330, rong: 700 }, "c0k2e-0.1", "e", K_GOC)}` },

    "cam-ve": { khai: true, html: ({ u }) => `
      ${chipKhai("📄 Đủ để trả lời lãnh đạo")}
      ${lopKhai(`<img class="vt-giay" src="${u(A("web-05-giay-qd-chup.jpg"))}" style="left:60px;top:280px;width:700px;transform:rotate(-2deg)">`, "s", null, "left:0;top:0;width:100%;height:100%")}
      ${goiKhai(`<div class="vt05-the">Số <b>86/QĐ-UBND</b></div>`, { trai: 470, tren: 760, hien: "c0k0", tu: "phai" })}
      ${goiKhai(`<div class="vt05-the">Ngày ký <b>12/08/2025</b></div>`, { trai: 470, tren: 880, hien: "c0k1", tu: "phai" })}
      ${goiKhai(`<div class="vt05-the">Người ký <b>Chủ tịch</b></div>`, { trai: 470, tren: 1000, hien: "c0k2", tu: "phai" })}
      ${goiKhai(`<div class="vt05-the">✓ Bản gốc</div>`, { trai: 470, tren: 1120, hien: "c0k3", tu: "phai" })}` },
  },
  mocThu: (tatCa) => [
    ["02-phim", lucNode(tatCa, "tim", "c0k0w3")], ["02-goi-y", lucNode(tatCa, "tim", "c0k3")],
    ["02-ket-qua", lucNode(tatCa, "tim", "c1k2+0.3")], ["03-cham", lucNode(tatCa, "ket-qua", "c1k0w0+0.1")],
    ["03-chi-tiet", lucNode(tatCa, "ket-qua", "c1k2+0.3")], ["04-cham-xem", lucNode(tatCa, "ban-goc", "c0k1w1+0.1")],
    ["04-ban-goc", lucNode(tatCa, "ban-goc", "c0k2e+0.4")], ["04-ban-goc-2", lucNode(tatCa, "ban-goc", "c0k2e+0.9")],
  ],
  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1+0.6"),
};
