// ============================================================================
// THỂ THỨC 28 — HỒ SƠ KHÔNG CÓ TRONG DANH MỤC HỒ SƠ THÌ PHẢI LÀM SAO? MỞ HỒ SƠ THẾ NÀO? THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 28: «Danh mục hồ sơ do người đứng đầu cơ quan, tổ chức phê duyệt, được ban hành vào đầu năm và gửi các đơn vị, cá nhân liên quan làm căn cứ để lập hồ sơ. …»
//   Điều 29 khoản 2: «a) Cá nhân được giao nhiệm vụ giải quyết công việc có trách nhiệm mở hồ sơ theo Danh mục hồ sơ hoặc theo kế hoạch công tác. … c) Trường hợp các hồ sơ không có
//   trong Danh mục hồ sơ, cá nhân được giao nhiệm vụ giải quyết công việc tự xác định các thông tin: Tiêu đề hồ sơ, số và ký hiệu hồ sơ, thời hạn bảo quản hồ sơ, người lập hồ sơ và thời gian
//   bắt đầu.» Khoản 3: «… thu thập, cập nhật tất cả văn bản, tài liệu hình thành trong quá trình theo dõi, giải quyết công việc vào hồ sơ đã mở, bao gồm tài liệu phim, ảnh, ghi âm (nếu có) bảo đảm
//   sự toàn vẹn, đầy đủ của hồ sơ, tránh bị thất lạc.»
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "28-danh-muc-ho-so-mo-ho-so",
  tieuDe: "Thể thức 28 · Hồ sơ không có trong Danh mục hồ sơ thì phải làm sao? Mở hồ sơ thế nào? Theo NĐ 30/2020",
  bia: { dong: ["Danh mục hồ sơ:","không có thì sao?"], vai: "Nghị định 30 · Điều 28, 29" },
  ket: {
    the: [
      "«Thu thập, cập nhật *tất cả văn bản, tài liệu* … vào hồ sơ đã mở, bao gồm *tài liệu phim, ảnh, ghi âm* (nếu có) bảo đảm sự toàn vẹn, đầy đủ của hồ sơ, *tránh bị thất lạc.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 29 · khoản 3"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Hồ sơ công việc không có trong Danh mục hồ sơ, thì phải làm sao?", chu: "Hồ sơ *không có trong Danh mục* — *phải làm sao?*" },
      { loi: "Nghị định ba mươi có cách xử lý.", chu: "Nghị định 30 có *cách xử lý*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Danh mục hồ sơ, do người đứng đầu cơ quan phê duyệt.", chu: "Danh mục hồ sơ: *người đứng đầu cơ quan* phê duyệt" },
      { loi: "Ban hành vào đầu năm, và gửi các đơn vị, cá nhân, làm căn cứ lập hồ sơ.", chu: "Ban hành *đầu năm* · gửi đơn vị, cá nhân *làm căn cứ lập hồ sơ*" },
      { loi: "Người được giao việc, mở hồ sơ theo Danh mục, hoặc theo kế hoạch công tác.", chu: "Người được giao việc *mở hồ sơ* theo Danh mục · hoặc *kế hoạch công tác*" },
      { loi: "Hồ sơ không có trong Danh mục, thì tự xác định năm thông tin.", chu: "Không có trong Danh mục → *tự xác định 5 thông tin*" },
      { loi: "Tiêu đề, số và ký hiệu, thời hạn bảo quản, người lập hồ sơ, và thời gian bắt đầu.", chu: "*Tiêu đề* · *số, ký hiệu* · *thời hạn bảo quản* · *người lập* · *thời gian bắt đầu*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Tất cả tài liệu, kể cả phim, ảnh, ghi âm, phải cập nhật vào hồ sơ, tránh bị thất lạc.", chu: "Mọi tài liệu, kể cả *phim · ảnh · ghi âm* → cập nhật vào hồ sơ · *tránh thất lạc*" },
      nguonCau("Điều hai mươi chín", "Điều 29 · khoản 3"),
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
  ${mocKhai(`${NGU_CANH}Hồ sơ không có trong Danh mục —<br>*phải làm sao?*`, { tren: 262, co: 66, gach: "c0k1" })}
  ${the(`<div class="x-q"><span>DANH MỤC<br>HỒ SƠ</span><i>?</i><span>KHÔNG CÓ</span></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("📚 Danh mục và mở hồ sơ")}
  ${hang("DANH MỤC", "r", "Người đứng đầu cơ quan phê duyệt", "Danh mục hồ sơ do người đứng đầu duyệt", "c0k1", 330)}
  ${hang("ĐẦU NĂM", "b", "Ban hành vào đầu năm", "gửi đơn vị, cá nhân làm căn cứ lập hồ sơ", "c1k1", 520)}
  ${hang("MỞ HỒ SƠ", "a", "Theo Danh mục hồ sơ", "hoặc theo kế hoạch công tác", "c2k1", 710)}
  ${hang("KHÔNG CÓ", "v", "Hồ sơ ngoài Danh mục", "người giải quyết công việc tự xác định", "c3k1", 900)}
  ${hang("5 THÔNG TIN", "g", "Tiêu đề · Số, ký hiệu · Thời hạn bảo quản", "người lập hồ sơ · thời gian bắt đầu", "c4k1", 1090)}`,
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
    ["02a-1", lucNode(tatCa, "ds", "c0k1w0")],
    ["02b-2", lucNode(tatCa, "ds", "c1k3w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k2w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k1w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k4w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
