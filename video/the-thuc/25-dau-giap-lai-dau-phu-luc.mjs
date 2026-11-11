// ============================================================================
// THỂ THỨC 25 — DẤU GIÁP LAI ĐÓNG Ở ĐÂU, TỐI ĐA MẤY TỜ? DẤU LÊN PHỤ LỤC THẾ NÀO? THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 33 khoản 1: «a) Dấu đóng phải rõ ràng, ngay ngắn, đúng chiều và dùng đúng mực dấu màu đỏ theo quy định. b) Khi đóng dấu lên chữ ký, dấu đóng phải trùm lên khoảng 1/3 chữ ký
//   về phía bên trái. c) Các văn bản ban hành kèm theo văn bản chính hoặc phụ lục: Dấu được đóng lên trang đầu, trùm một phần tên cơ quan, tổ chức hoặc tiêu đề phụ lục. d) Việc đóng
//   dấu treo, dấu giáp lai, đóng dấu nổi trên văn bản giấy do người đứng đầu cơ quan, tổ chức quy định. đ) Dấu giáp lai được đóng vào khoảng giữa mép phải của văn bản hoặc phụ lục
//   văn bản, trùm lên một phần các tờ giấy; mỗi dấu đóng tối đa 05 tờ văn bản.»
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "25-dau-giap-lai-dau-phu-luc",
  tieuDe: "Thể thức 25 · Dấu giáp lai đóng ở đâu, tối đa mấy tờ? Dấu lên phụ lục thế nào? Theo NĐ 30/2020",
  bia: { dong: ["Dấu giáp lai:","tối đa mấy tờ?"], vai: "Nghị định 30 · Điều 33" },
  ket: {
    the: [
      "«Việc đóng dấu treo, dấu giáp lai, đóng dấu nổi trên văn bản giấy *do người đứng đầu cơ quan, tổ chức quy định.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 33 · khoản 1 · điểm d"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Dấu giáp lai, đóng ở đâu, và mỗi dấu đóng được tối đa bao nhiêu tờ?", chu: "*Dấu giáp lai* — đóng *ở đâu*, tối đa *bao nhiêu tờ?*" },
      { loi: "Nghị định ba mươi có số cụ thể.", chu: "Nghị định 30 có *số cụ thể*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Dấu giáp lai, đóng vào khoảng giữa mép phải, của văn bản, hoặc phụ lục.", chu: "Dấu giáp lai: *giữa mép phải* · văn bản hoặc phụ lục" },
      { loi: "Dấu trùm lên một phần các tờ giấy, mỗi dấu đóng tối đa năm tờ.", chu: "Trùm một phần các tờ · *tối đa 5 tờ*" },
      { loi: "Văn bản kèm theo, đóng dấu lên trang đầu, trùm một phần tên cơ quan, hoặc tiêu đề phụ lục.", chu: "Văn bản kèm theo: *trang đầu* · trùm *tên cơ quan* hoặc *tiêu đề phụ lục*" },
      { loi: "Dấu phải rõ ràng, ngay ngắn, đúng chiều, và dùng mực đỏ.", chu: "Dấu: *rõ ràng* · *ngay ngắn* · *đúng chiều* · mực *đỏ*" },
      { loi: "Đóng lên chữ ký, dấu trùm khoảng một phần ba chữ ký, về phía bên trái.", chu: "Đóng lên chữ ký: trùm *khoảng 1/3* · *bên trái*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Việc đóng dấu treo, dấu giáp lai, dấu nổi, do người đứng đầu cơ quan quy định.", chu: "Dấu *treo* · *giáp lai* · *nổi*: do *người đứng đầu* cơ quan quy định" },
      nguonCau("Điều ba mươi ba", "Điều 33 · khoản 1 · điểm d"),
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
  ${mocKhai(`${NGU_CANH}Dấu giáp lai —<br>*đóng ở đâu, mấy tờ?*`, { tren: 262, co: 70, gach: "c0k2" })}
  ${the(`<div class="x-q"><span>GIÁP LAI</span><span>TỐI ĐA<br>? TỜ</span></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("🔴 Đóng dấu văn bản giấy")}
  ${hang("GIÁP LAI", "r", "Giữa mép phải của văn bản", "hoặc của phụ lục văn bản", "c0k1", 330)}
  ${hang("TỐI ĐA", "b", "Mỗi dấu đóng tối đa 5 tờ", "trùm lên một phần các tờ giấy", "c1k1", 520)}
  ${hang("PHỤ LỤC", "a", "Đóng lên trang đầu", "trùm tên cơ quan hoặc tiêu đề phụ lục", "c2k1", 710)}
  ${hang("ĐÚNG CÁCH", "v", "Rõ ràng · ngay ngắn · đúng chiều", "dùng đúng mực dấu màu đỏ", "c3k1", 900)}
  ${hang("LÊN CHỮ KÝ", "g", "Trùm khoảng 1/3 chữ ký bên trái", "khi đóng dấu lên chữ ký", "c4k1", 1090)}`,
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
    ["02a-1", lucNode(tatCa, "ds", "c0k3w0")],
    ["02b-2", lucNode(tatCa, "ds", "c1k1w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k3w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k3w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k2w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
