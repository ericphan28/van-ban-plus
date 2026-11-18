// ============================================================================
// THỂ THỨC 29 — VĂN BẢN HÀNH CHÍNH GỒM 29 LOẠI: KỂ ĐƯỢC MẤY LOẠI? GIẤY NGHỈ PHÉP, THƯ CÔNG CÓ TRONG DANH SÁCH KHÔNG? THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 7: «Văn bản hành chính gồm các loại văn bản sau: Nghị quyết (cá biệt), quyết định (cá biệt), chỉ thị, quy chế, quy định, thông cáo, thông báo, hướng dẫn, chương trình,
//   kế hoạch, phương án, đề án, dự án, báo cáo, biên bản, tờ trình, hợp đồng, công văn, công điện, bản ghi nhớ, bản thỏa thuận, giấy uỷ quyền, giấy mời, giấy giới thiệu,
//   giấy nghỉ phép, phiếu gửi, phiếu chuyển, phiếu báo, thư công.»
//     ✔ ĐẾM theo thứ tự liệt kê = 29 loại; chia khúc 5 · 6 · 7 · 6 · 5 chỉ để dễ nhớ, KHÔNG phải phân nhóm của Nghị định.
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "29-loai-van-ban-hanh-chinh",
  tieuDe: "Thể thức 29 · Văn bản hành chính gồm 29 loại: kể được mấy loại? Giấy nghỉ phép, thư công có trong danh sách không? Theo NĐ 30/2020",
  bia: { dong: ["29 loại văn bản","kể được mấy loại?"], vai: "Nghị định 30 · Điều 7" },
  ket: {
    the: [
      "«Văn bản hành chính gồm các loại văn bản sau: Nghị quyết (cá biệt), quyết định (cá biệt), chỉ thị, … *công văn*, công điện, … giấy nghỉ phép, … *thư công.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 7"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản hành chính, có bao nhiêu loại, và kể được mấy loại?", chu: "Văn bản hành chính: *bao nhiêu loại?* Kể được *mấy loại?*" },
      { loi: "Nghị định ba mươi liệt kê đủ.", chu: "Nghị định 30 *liệt kê đủ*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Năm loại đầu, nghị quyết và quyết định cá biệt, chỉ thị, quy chế, quy định.", chu: "Loại *1–5*: nghị quyết · quyết định *(cá biệt)* · chỉ thị · quy chế · quy định" },
      { loi: "Sáu loại tiếp, thông cáo, thông báo, hướng dẫn, chương trình, kế hoạch, phương án.", chu: "Loại *6–11*: thông cáo · thông báo · hướng dẫn · chương trình · kế hoạch · phương án" },
      { loi: "Bảy loại tiếp, đề án, dự án, báo cáo, biên bản, tờ trình, hợp đồng, công văn.", chu: "Loại *12–18*: đề án · dự án · báo cáo · biên bản · tờ trình · hợp đồng · *công văn*" },
      { loi: "Sáu loại tiếp, công điện, bản ghi nhớ, bản thỏa thuận, giấy uỷ quyền, giấy mời, giấy giới thiệu.", chu: "Loại *19–24*: công điện · bản ghi nhớ · bản thỏa thuận · giấy uỷ quyền · giấy mời · giấy giới thiệu" },
      { loi: "Năm loại cuối, giấy nghỉ phép, phiếu gửi, phiếu chuyển, phiếu báo, thư công.", chu: "Loại *25–29*: *giấy nghỉ phép* · phiếu gửi · phiếu chuyển · phiếu báo · *thư công*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Công văn, chỉ là một trong hai mươi chín loại.", chu: "*Công văn* chỉ là *1 trong 29 loại* văn bản hành chính" },
      nguonCau("Điều bảy", "Điều 7"),
    ]),
  ],

  css: CSS_CC + `
.x-q{display:flex;flex-wrap:wrap;justify-content:center;align-items:center;gap:20px 24px;padding:44px 24px 50px}
.x-q span{font-size:46px;font-weight:900;color:#b91c1c;border:7px solid #b91c1c;border-radius:12px;padding:6px 24px;transform:rotate(-3deg);background:#fff;text-align:center;line-height:1.15}
.x-q i{font-style:normal;font-size:76px;font-weight:900;color:#b45309}
.x-the{padding:0}
.x-h{display:flex;align-items:center;gap:22px;padding:18px 26px}
.x-h .nh{flex:none;width:214px;text-align:center;font-size:32px;font-weight:900;color:#fff;border-radius:16px;padding:12px 6px;line-height:1.1}
.x-h .nh.g{background:#15803d}.x-h .nh.r{background:#dc2626}.x-h .nh.b{background:#1d4ed8}.x-h .nh.a{background:#d97706}.x-h .nh.v{background:#0f766e}
.x-h .tx{flex:1;font-size:42px;font-weight:900;color:#0f172a;line-height:1.15}
.x-h .tx small{display:block;margin-top:6px;font-size:30px;font-weight:800;color:#64748b}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Văn bản hành chính —<br>*có bao nhiêu loại?*`, { tren: 262, co: 66, gach: "c0k1" })}
  ${the(`<div class="x-q"><span>GIẤY<br>NGHỈ PHÉP?</span><span>THƯ CÔNG?</span><i>?</i></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("📋 Điều 7 · chia khúc cho dễ nhớ")}
  ${hang("LOẠI 1–5", "r", "Nghị quyết, quyết định (cá biệt)", "chỉ thị · quy chế · quy định", "c0k1", 330)}
  ${hang("LOẠI 6–11", "b", "Thông cáo · thông báo", "hướng dẫn · chương trình<br>kế hoạch · phương án", "c1k1", 520)}
  ${hang("LOẠI 12–18", "a", "Đề án · dự án · báo cáo · biên bản", "tờ trình · hợp đồng · công văn", "c2k1", 710)}
  ${hang("LOẠI 19–24", "v", "Công điện · bản ghi nhớ", "bản thỏa thuận · giấy uỷ quyền<br>giấy mời · giấy giới thiệu", "c3k1", 900)}
  ${hang("LOẠI 25–29", "g", "Giấy nghỉ phép · phiếu gửi", "phiếu chuyển · phiếu báo · thư công", "c4k1", 1090)}`,
    },

    "app": {
      khai: true,
      html: () => `
  ${chipKhai("🤖 Phần mềm soạn văn bản")}
  ${theApp("c0k0", true)}`,
    },
  },

  mocThu: (tatCa) => [
    ["01a-0.4s", 0.4],
    ["02a-1", lucNode(tatCa, "ds", "c0k4w0")],
    ["02b-2", lucNode(tatCa, "ds", "c1k6w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k7w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k6w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k5w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
