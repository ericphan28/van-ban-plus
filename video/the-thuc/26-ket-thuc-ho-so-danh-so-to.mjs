// ============================================================================
// THỂ THỨC 26 — HỒ SƠ CÔNG VIỆC XONG RỒI CÒN PHẢI LÀM GÌ? LOẠI BẢN NHÁP, ĐÁNH SỐ TỜ, MỤC LỤC. THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 29 khoản 4: «a) Hồ sơ được kết thúc khi công việc đã giải quyết xong. b) Người lập hồ sơ có trách nhiệm: Rà soát lại toàn bộ văn bản, tài liệu có trong hồ sơ; loại ra khỏi hồ sơ
//   bản trùng, bản nháp; xác định lại thời hạn bảo quản của hồ sơ; chỉnh sửa tiêu đề, số và ký hiệu hồ sơ cho phù hợp; hoàn thiện, kết thúc hồ sơ. c) Đối với hồ sơ giấy: Người lập
//   hồ sơ thực hiện đánh số tờ đối với hồ sơ có thời hạn bảo quản từ 05 năm trở lên và viết Mục lục văn bản đối với hồ sơ có thời hạn bảo quản vĩnh viễn; viết chứng từ kết thúc đối với
//   tất cả hồ sơ. d) Đối với hồ sơ điện tử: Người lập hồ sơ có trách nhiệm cập nhật vào Hệ thống các thông tin còn thiếu. Việc biên mục văn bản trong hồ sơ được thực hiện bằng chức
//   năng của Hệ thống.»
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "26-ket-thuc-ho-so-danh-so-to",
  tieuDe: "Thể thức 26 · Hồ sơ công việc xong rồi còn phải làm gì? Loại bản nháp, đánh số tờ, mục lục. Theo NĐ 30/2020",
  bia: { dong: ["Kết thúc hồ sơ:","còn phải làm gì?"], vai: "Nghị định 30 · Điều 29" },
  ket: {
    the: [
      "«Đối với hồ sơ điện tử: … Việc biên mục văn bản trong hồ sơ *được thực hiện bằng chức năng của Hệ thống.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 29 · khoản 4 · điểm d"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Công việc xong rồi, lập hồ sơ còn phải làm gì nữa?", chu: "Công việc xong — lập hồ sơ *còn phải làm gì?*" },
      { loi: "Nghị định ba mươi nêu từng bước.", chu: "Nghị định 30 nêu *từng bước*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Hồ sơ kết thúc, khi công việc đã giải quyết xong.", chu: "Hồ sơ kết thúc khi *công việc giải quyết xong*" },
      { loi: "Người lập hồ sơ rà soát, loại ra bản trùng, và bản nháp.", chu: "Rà soát, *loại ra bản trùng, bản nháp*" },
      { loi: "Xác định lại thời hạn bảo quản, chỉnh sửa tiêu đề, số và ký hiệu hồ sơ.", chu: "Xác định lại *thời hạn bảo quản* · sửa *tiêu đề, số, ký hiệu*" },
      { loi: "Hồ sơ giấy, bảo quản từ năm năm trở lên, thì đánh số tờ.", chu: "Hồ sơ giấy, bảo quản *từ 5 năm* → *đánh số tờ*" },
      { loi: "Bảo quản vĩnh viễn, thì viết mục lục văn bản, và mọi hồ sơ đều có chứng từ kết thúc.", chu: "*Vĩnh viễn* → mục lục văn bản · *mọi hồ sơ* → chứng từ kết thúc", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Hồ sơ điện tử, thì việc biên mục văn bản, được thực hiện bằng chức năng của Hệ thống.", chu: "Hồ sơ điện tử: biên mục *bằng chức năng của Hệ thống*" },
      nguonCau("Điều hai mươi chín", "Điều 29 · khoản 4 · điểm d"),
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
  ${mocKhai(`${NGU_CANH}Công việc xong rồi —<br>*hồ sơ còn phải làm gì?*`, { tren: 262, co: 66, gach: "c0k1" })}
  ${the(`<div class="x-q"><span>BẢN NHÁP?</span><span>BẢN TRÙNG?</span><span>ĐÁNH SỐ TỜ?</span></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("🗂️ Kết thúc hồ sơ")}
  ${hang("KẾT THÚC", "r", "Công việc giải quyết xong", "hồ sơ được kết thúc", "c0k1", 330)}
  ${hang("LOẠI RA", "b", "Bản trùng · bản nháp", "người lập hồ sơ rà soát lại toàn bộ", "c1k1", 520)}
  ${hang("THỜI HẠN", "a", "Xác định lại thời hạn bảo quản", "sửa tiêu đề, số và ký hiệu hồ sơ", "c2k1", 710)}
  ${hang("ĐÁNH SỐ TỜ", "v", "Từ 5 năm trở lên", "đối với hồ sơ giấy", "c3k1", 900)}
  ${hang("MỤC LỤC", "g", "Vĩnh viễn: viết mục lục văn bản", "mọi hồ sơ: viết chứng từ kết thúc", "c4k1", 1090)}`,
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
    ["02b-2", lucNode(tatCa, "ds", "c1k2w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k2w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k2w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k2w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
