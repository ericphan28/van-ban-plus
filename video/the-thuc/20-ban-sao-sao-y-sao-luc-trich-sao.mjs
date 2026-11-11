// ============================================================================
// THỂ THỨC 20 — BẢN SAO: SAO Y, SAO LỤC, TRÍCH SAO KHÁC NHAU THẾ NÀO? (Nghị định 30/2020/NĐ-CP, Điều 3 khoản 10–12, Điều 25, 26, 27)
//
// Khuôn PHÂN BIỆT CÓ BẢNG: móc «ba loại bản sao khác nhau thế nào? có giá trị như bản chính không?» → 5 hàng hiện theo lời (sao y · sao lục ·
// trích sao · giấy → điện tử phải số hóa + ký số · giá trị pháp lý) → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn về
// thẩm quyền sao văn bản.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md dòng 46–48, 375–411):
//   Điều 3: «10. "Bản sao y" là bản sao đầy đủ, chính xác nội dung của bản gốc hoặc bản chính văn bản, được trình bày theo thể thức và kỹ
//   thuật quy định. 11. "Bản sao lục" là bản sao đầy đủ, chính xác nội dung của bản sao y, … 12. "Bản trích sao" là bản sao chính xác phần
//   nội dung của bản gốc hoặc phần nội dung của bản chính văn bản cần trích sao, …»
//   Điều 25: «1. … c) Sao y từ văn bản giấy sang văn bản điện tử được thực hiện bằng việc số hóa văn bản giấy và ký số của cơ quan, tổ chức.
//   2. … b) Sao lục được thực hiện bằng việc in, chụp từ bản sao y. 3. … b) Bản trích sao được thực hiện bằng việc tạo lập lại đầy đủ thể thức,
//   phần nội dung văn bản cần trích sao.»
//   Điều 26: «Bản sao y, bản sao lục và bản trích sao được thực hiện theo đúng quy định tại Nghị định này có giá trị pháp lý như bản chính.»
//   Điều 27 khoản 1: «Người đứng đầu cơ quan, tổ chức quyết định việc sao văn bản do cơ quan, tổ chức ban hành, văn bản do các cơ quan, tổ
//   chức khác gửi đến và quy định thẩm quyền ký các bản sao văn bản.»
//   ✔ sao y = đầy đủ, chính xác, từ bản gốc / bản chính · ✔ sao lục = đầy đủ, chính xác, từ BẢN SAO Y (in, chụp) · ✔ trích sao = chỉ phần nội dung cần trích
//   ✔ sao y giấy → điện tử: số hóa + ký số của cơ quan · ✔ đúng quy định → giá trị pháp lý như bản chính · ✔ thẻ màn kết: thẩm quyền sao
//   ✘ KHÔNG nói phần mềm tự sao y (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="c20-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "c20-the" });

export default {
  ten: "20-ban-sao-sao-y-sao-luc-trich-sao",
  tieuDe: "Thể thức 20 · Sao y, sao lục, trích sao khác nhau thế nào? Bản sao có giá trị như bản chính không? Theo NĐ 30/2020",
  bia: { dong: ["Sao y, sao lục,", "trích sao khác gì?"], vai: "Nghị định 30 · Điều 25" },
  ket: {
    the: [
      "«Người đứng đầu cơ quan, tổ chức *quyết định việc sao văn bản* … và *quy định thẩm quyền ký các bản sao văn bản.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 27 · khoản 1"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Sao y, sao lục, trích sao, ba loại bản sao này khác nhau thế nào?", chu: "*Sao y* · *sao lục* · *trích sao* — khác nhau thế nào?" },
      { loi: "Và bản sao có giá trị như bản chính không?", chu: "Bản sao có *giá trị như bản chính* không?", nghiSau: 0.2 },
    ]},
    { canh: "ba-loai", cau: [
      { loi: "Sao y, là sao đầy đủ, chính xác, từ bản gốc, hoặc bản chính.", chu: "*Sao y* = đầy đủ · chính xác · từ *bản gốc* hoặc *bản chính*" },
      { loi: "Sao lục, là sao đầy đủ, chính xác, từ bản sao y.", chu: "*Sao lục* = đầy đủ · chính xác · từ *bản sao y*" },
      { loi: "Trích sao, chỉ sao một phần nội dung cần trích.", chu: "*Trích sao* = chỉ *một phần nội dung* cần trích" },
      { loi: "Sao y từ văn bản giấy sang điện tử, thì số hóa, và ký số của cơ quan.", chu: "Sao y giấy → điện tử: *số hóa* + *ký số của cơ quan*" },
      { loi: "Bản sao thực hiện đúng quy định, có giá trị pháp lý như bản chính.", chu: "Đúng quy định → *giá trị pháp lý như bản chính*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Người đứng đầu cơ quan quyết định việc sao văn bản, và quy định thẩm quyền ký các bản sao.", chu: "Người đứng đầu: *quyết định việc sao* · *quy định thẩm quyền ký bản sao*" },
      nguonCau("Điều hai mươi bảy", "Điều 27 · khoản 1"),
    ]),
  ],

  css: CSS_CC + `
.c20-q{display:flex;flex-wrap:wrap;justify-content:center;gap:20px 24px;padding:44px 24px 50px}
.c20-q i{font-style:normal;font-size:46px;font-weight:900;color:#b91c1c;border:7px solid #b91c1c;border-radius:12px;padding:6px 24px;transform:rotate(-3deg);background:#fff}
.c20-the{padding:0}
.c20-h{display:flex;align-items:center;gap:22px;padding:18px 26px}
.c20-h .nh{flex:none;width:250px;text-align:center;font-size:40px;font-weight:900;color:#fff;border-radius:16px;padding:12px 0;line-height:1.1}
.c20-h .nh.r{background:#dc2626}.c20-h .nh.b{background:#1d4ed8}.c20-h .nh.v{background:#7c3aed}.c20-h .nh.t{background:#0f766e}.c20-h .nh.g{background:#15803d}
.c20-h .tx{flex:1;font-size:48px;font-weight:900;color:#0f172a;line-height:1.15}
.c20-h .tx small{display:block;margin-top:8px;font-size:32px;font-weight:800;color:#64748b}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Ba loại bản sao —<br>*khác nhau thế nào?*`, { tren: 262, co: 74, gach: "c0k3" })}
  ${the(`<div class="c20-q"><i>SAO Y</i><i>SAO LỤC</i><i>TRÍCH SAO</i></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ba-loai": {
      khai: true,
      html: () => `
  ${chipKhai("📄 Các hình thức bản sao")}
  ${hang("SAO Y", "r", "Đầy đủ · chính xác", "từ bản gốc hoặc bản chính", "c0k1", 330)}
  ${hang("SAO LỤC", "b", "Đầy đủ · chính xác", "từ BẢN SAO Y (in, chụp)", "c1k1", 500)}
  ${hang("TRÍCH SAO", "v", "Chỉ một phần nội dung", "phần nội dung cần trích", "c2k1", 670)}
  ${hang("GIẤY → ĐIỆN TỬ", "t", "Số hóa + ký số của cơ quan", "với sao y", "c3k1", 840)}
  ${hang("GIÁ TRỊ", "g", "Như bản chính", "khi thực hiện đúng quy định của Nghị định", "c4k1", 1010)}`,
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
    ["02a-sao-y", lucNode(tatCa, "ba-loai", "c0k4w1")],
    ["02b-sao-luc", lucNode(tatCa, "ba-loai", "c1k3w3")],
    ["02c-trich-sao", lucNode(tatCa, "ba-loai", "c2k1w4")],
    ["02d-so-hoa", lucNode(tatCa, "ba-loai", "c3k2w3")],
    ["02e-gia-tri", lucNode(tatCa, "ba-loai", "c4k1w4")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k3"),
};
