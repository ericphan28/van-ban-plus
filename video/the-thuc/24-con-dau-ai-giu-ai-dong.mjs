// ============================================================================
// THỂ THỨC 24 — CON DẤU: AI GIỮ, AI ĐƯỢC ĐÓNG, ĐÓNG DẤU TRƯỚC KÝ SAU CÓ ĐƯỢC KHÔNG? THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 32 khoản 1: «Người đứng đầu cơ quan, tổ chức có trách nhiệm giao cho Văn thư cơ quan quản lý, sử dụng con dấu, thiết bị lưu khóa bí mật …» Khoản 2: «Văn thư cơ quan
//   có trách nhiệm a) Bảo quản an toàn, sử dụng con dấu … tại trụ sở cơ quan, tổ chức. b) Chỉ giao con dấu … cho người khác khi được phép bằng văn bản của người có thẩm quyền.
//   Việc bàn giao … phải được lập biên bản. c) Phải trực tiếp đóng dấu, ký số vào văn bản do cơ quan, tổ chức ban hành và bản sao văn bản. d) Chỉ được đóng dấu, ký số của cơ quan,
//   tổ chức vào văn bản đã có chữ ký của người có thẩm quyền và bản sao văn bản do cơ quan, tổ chức trực tiếp thực hiện.» Khoản 3: «Cá nhân có trách nhiệm tự bảo quản an toàn
//   thiết bị lưu khóa bí mật và khóa bí mật.»  ✔ «đóng dấu trước, ký sau» = trái điểm d (chỉ đóng vào văn bản đã có chữ ký)
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "24-con-dau-ai-giu-ai-dong",
  tieuDe: "Thể thức 24 · Con dấu: ai giữ, ai được đóng, đóng dấu trước ký sau có được không? Theo NĐ 30/2020",
  bia: { dong: ["Đóng dấu hộ,","đóng trước ký sau?"], vai: "Nghị định 30 · Điều 32" },
  ket: {
    the: [
      "«Cá nhân có trách nhiệm *tự bảo quản an toàn thiết bị lưu khóa bí mật và khóa bí mật.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 32 · khoản 3"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Nhờ người khác đóng dấu hộ, hoặc đóng dấu trước, ký sau, có được không?", chu: "*Đóng dấu hộ* · đóng dấu *trước, ký sau* — *có được không?*" },
      { loi: "Nghị định ba mươi trả lời rất rõ.", chu: "Nghị định 30 *trả lời rất rõ*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Con dấu, do Văn thư cơ quan quản lý, bảo quản an toàn tại trụ sở.", chu: "Con dấu: *Văn thư* quản lý · bảo quản *tại trụ sở*" },
      { loi: "Chỉ giao dấu cho người khác, khi được phép bằng văn bản, và phải lập biên bản.", chu: "Giao dấu: *phép bằng văn bản* · lập *biên bản*" },
      { loi: "Văn thư phải trực tiếp đóng dấu, ký số, vào văn bản ban hành, và bản sao.", chu: "*Văn thư trực tiếp* đóng dấu · ký số" },
      { loi: "Chỉ đóng dấu, vào văn bản, đã có chữ ký của người có thẩm quyền.", chu: "Chỉ đóng dấu vào văn bản *đã có chữ ký*" },
      { loi: "Bản sao cũng vậy, chỉ đóng dấu, khi do cơ quan trực tiếp thực hiện.", chu: "Bản sao: do *cơ quan trực tiếp thực hiện*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Cá nhân, có trách nhiệm tự bảo quản an toàn, thiết bị lưu khóa bí mật, và khóa bí mật.", chu: "Cá nhân: *tự bảo quản an toàn* · thiết bị lưu khóa bí mật · khóa bí mật" },
      nguonCau("Điều ba mươi hai", "Điều 32 · khoản 3"),
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
  ${mocKhai(`${NGU_CANH}Đóng dấu hộ, đóng dấu trước —<br>*có được không?*`, { tren: 262, co: 66, gach: "c0k2" })}
  ${the(`<div class="x-q"><span>DẤU HỘ</span><span>DẤU TRƯỚC<br>KÝ SAU</span><i>?</i></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("🔴 Con dấu cơ quan")}
  ${hang("GIỮ DẤU", "r", "Văn thư cơ quan quản lý", "bảo quản an toàn tại trụ sở", "c0k1", 330)}
  ${hang("GIAO DẤU", "b", "Chỉ giao khi có phép bằng văn bản", "bàn giao phải lập biên bản", "c1k1", 520)}
  ${hang("TRỰC TIẾP", "a", "Văn thư phải tự tay đóng dấu, ký số", "vào văn bản ban hành và bản sao", "c2k1", 710)}
  ${hang("CHỮ KÝ TRƯỚC", "v", "Chỉ đóng dấu vào văn bản đã có chữ ký", "của người có thẩm quyền", "c3k1", 900)}
  ${hang("BẢN SAO", "g", "Chỉ đóng dấu bản sao do cơ quan làm", "cơ quan trực tiếp thực hiện", "c4k1", 1090)}`,
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
    ["02b-2", lucNode(tatCa, "ds", "c1k2w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k3w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k2w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k2w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
