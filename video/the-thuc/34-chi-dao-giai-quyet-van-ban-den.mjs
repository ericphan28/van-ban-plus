// ============================================================================
// THỂ THỨC 34 — VĂN BẢN ĐẾN SAU KHI ĐĂNG KÝ: Ý KIẾN CHỈ ĐẠO GHI Ở ĐÂU, AI CHỦ TRÌ, AI PHỐI HỢP? THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 23 khoản 1: «Văn bản phải được Văn thư cơ quan trình trong ngày, chậm nhất là trong ngày làm việc tiếp theo đến người có thẩm quyền chỉ đạo giải quyết …» Khoản 2: «Căn cứ nội dung của văn bản đến;
//   quy chế làm việc …; chức năng, nhiệm vụ và kế hoạch công tác được giao cho đơn vị, cá nhân, người có thẩm quyền ghi ý kiến chỉ đạo giải quyết. Đối với văn bản liên quan đến nhiều đơn vị hoặc cá nhân thì xác định
//   rõ đơn vị hoặc cá nhân chủ trì, phối hợp và thời hạn giải quyết.» Khoản 3: «Trình, chuyển giao văn bản giấy: Ý kiến chỉ đạo giải quyết được ghi vào mục "Chuyển" trong dấu "ĐẾN" hoặc Phiếu giải quyết
//   văn bản đến theo mẫu tại Phụ lục IV … Khi chuyển giao văn bản giấy đến cho đơn vị, cá nhân phải ký nhận văn bản.» Điều 24 khoản 2: «… Những văn bản đến có dấu chỉ các mức độ khẩn phải được giải quyết ngay.»
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "34-chi-dao-giai-quyet-van-ban-den",
  tieuDe: "Thể thức 34 · Văn bản đến sau khi đăng ký: ý kiến chỉ đạo ghi ở đâu, ai chủ trì, ai phối hợp? Theo NĐ 30/2020",
  bia: { dong: ["Ý kiến chỉ đạo","ghi ở đâu?"], vai: "Nghị định 30 · Điều 23, 24" },
  ket: {
    the: [
      "«Những văn bản đến *có dấu chỉ các mức độ khẩn* phải được *giải quyết ngay.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 24 · khoản 2"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản đến liên quan nhiều đơn vị, thì ai chủ trì, ai phối hợp, và ghi ở đâu?", chu: "Văn bản đến *nhiều đơn vị*: ai *chủ trì*, ai *phối hợp?* Ghi *ở đâu?*" },
      { loi: "Nghị định ba mươi có đáp án.", chu: "Nghị định 30 *có đáp án*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Văn thư trình trong ngày, chậm nhất ngày làm việc tiếp theo, đến người có thẩm quyền.", chu: "Văn thư *trình trong ngày*, chậm nhất *ngày làm việc tiếp theo*" },
      { loi: "Người có thẩm quyền, căn cứ nội dung văn bản, và chức năng, nhiệm vụ, ghi ý kiến chỉ đạo giải quyết.", chu: "Căn cứ *nội dung*, *chức năng, nhiệm vụ* → ghi *ý kiến chỉ đạo*" },
      { loi: "Văn bản liên quan nhiều đơn vị, thì phải xác định rõ đơn vị chủ trì, phối hợp, và thời hạn giải quyết.", chu: "Nhiều đơn vị → xác định *chủ trì* · *phối hợp* · *thời hạn*" },
      { loi: "Văn bản giấy, ghi vào mục chuyển trong dấu đến, hoặc phiếu giải quyết văn bản đến.", chu: "Văn bản giấy: mục *«Chuyển»* trong dấu ĐẾN · hoặc *Phiếu giải quyết*" },
      { loi: "Chuyển giao văn bản giấy, thì phải ký nhận văn bản.", chu: "Chuyển giao văn bản giấy → *phải ký nhận*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Văn bản đến có dấu chỉ các mức độ khẩn, thì phải được giải quyết ngay.", chu: "Văn bản đến có dấu *khẩn* → phải *giải quyết ngay*" },
      nguonCau("Điều hai mươi bốn", "Điều 24 · khoản 2"),
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
  ${mocKhai(`${NGU_CANH}Văn bản đến, nhiều đơn vị —<br>*ai chủ trì, ai phối hợp?*`, { tren: 262, co: 62, gach: "c0k1" })}
  ${the(`<div class="x-q"><span>CHỦ TRÌ</span><span>PHỐI HỢP</span><span>THỜI HẠN?</span></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("📨 Chỉ đạo giải quyết")}
  ${hang("TRÌNH", "r", "Trình người có thẩm quyền", "trong ngày, chậm nhất ngày làm việc sau", "c0k1", 330)}
  ${hang("Ý KIẾN", "b", "Ghi ý kiến chỉ đạo giải quyết", "căn cứ nội dung, chức năng, nhiệm vụ", "c1k1", 520)}
  ${hang("NHIỀU ĐƠN VỊ", "a", "Xác định chủ trì, phối hợp", "và thời hạn giải quyết", "c2k1", 710)}
  ${hang("GHI Ở ĐÂU", "v", "Mục «Chuyển» trong dấu ĐẾN", "hoặc Phiếu giải quyết văn bản đến", "c3k1", 900)}
  ${hang("KHI GIAO", "g", "Chuyển giao văn bản giấy", "phải ký nhận văn bản", "c4k1", 1090)}`,
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
    ["02a-1", lucNode(tatCa, "ds", "c0k2w0")],
    ["02b-2", lucNode(tatCa, "ds", "c1k4w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k3w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k2w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k1w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
