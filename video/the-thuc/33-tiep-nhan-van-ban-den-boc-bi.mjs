// ============================================================================
// THỂ THỨC 33 — NHẬN VĂN BẢN ĐẾN: VĂN THƯ KIỂM TRA GÌ, BÓC BÌ HAY KHÔNG BÓC BÌ? THƯ GỬI ĐÍCH DANH XỬ LÝ THẾ NÀO? THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 21 khoản 1: «a) Văn thư cơ quan kiểm tra số lượng, tình trạng bì, dấu niêm phong (nếu có), nơi gửi; đối chiếu số, ký hiệu ghi ngoài bì với số, ký hiệu của văn bản trong bì.
//   Trường hợp phát hiện có sai sót hoặc dấu hiệu bất thường, Văn thư cơ quan báo ngay người có trách nhiệm giải quyết và thông báo cho nơi gửi văn bản. b) Tất cả văn bản giấy đến (bao gồm cả văn bản có dấu chỉ
//   độ mật) gửi cơ quan, tổ chức thuộc diện đăng ký tại Văn thư cơ quan phải được bóc bì, đóng dấu "ĐẾN". Đối với văn bản gửi đích danh cá nhân hoặc tổ chức đoàn thể trong cơ quan, tổ chức thì Văn thư cơ quan
//   chuyển cho nơi nhận (không bóc bì). Những bì văn bản gửi đích danh cá nhân, nếu là văn bản liên quan đến công việc chung của cơ quan, tổ chức thì cá nhân nhận văn bản có trách nhiệm chuyển lại cho Văn thư cơ quan để đăng ký.»
//   Khoản 2: «a) Văn thư cơ quan phải kiểm tra tính xác thực và toàn vẹn của văn bản điện tử … c) … thông báo ngay trong ngày cho cơ quan, tổ chức gửi về việc đã nhận văn bản bằng chức năng của Hệ thống.»
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "33-tiep-nhan-van-ban-den-boc-bi",
  tieuDe: "Thể thức 33 · Nhận văn bản đến: Văn thư kiểm tra gì, bóc bì hay không bóc bì? Thư gửi đích danh xử lý thế nào? Theo NĐ 30/2020",
  bia: { dong: ["Văn bản gửi đích danh:","có được bóc bì?"], vai: "Nghị định 30 · Điều 21" },
  ket: {
    the: [
      "«Văn thư cơ quan phải kiểm tra *tính xác thực và toàn vẹn* của văn bản điện tử … Cơ quan, tổ chức nhận văn bản có trách nhiệm *thông báo ngay trong ngày* cho cơ quan, tổ chức gửi về việc đã nhận văn bản.»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 21 · khoản 2"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Nhận văn bản giấy, Văn thư kiểm tra gì, và khi nào không bóc bì?", chu: "Nhận văn bản đến: Văn thư *kiểm tra gì?* Khi nào *không bóc bì?*" },
      { loi: "Nghị định ba mươi quy định cụ thể.", chu: "Nghị định 30 *quy định cụ thể*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Văn thư kiểm tra số lượng, tình trạng bì, dấu niêm phong, và nơi gửi.", chu: "Kiểm tra *số lượng* · *tình trạng bì* · *dấu niêm phong* · *nơi gửi*" },
      { loi: "Đối chiếu số, ký hiệu ghi ngoài bì, với số, ký hiệu của văn bản trong bì.", chu: "Đối chiếu *số, ký hiệu ngoài bì* với *trong bì* · sai sót → *báo ngay*" },
      { loi: "Thuộc diện đăng ký tại Văn thư, phải bóc bì, đóng dấu đến, kể cả văn bản có dấu độ mật.", chu: "Thuộc diện đăng ký: *bóc bì* · *đóng dấu ĐẾN* · kể cả văn bản *có dấu độ mật*" },
      { loi: "Văn bản gửi đích danh cá nhân, hoặc tổ chức đoàn thể, thì chuyển cho nơi nhận, không bóc bì.", chu: "Gửi *đích danh cá nhân*, *tổ chức đoàn thể* → chuyển, *không bóc bì*" },
      { loi: "Nếu là việc chung của cơ quan, thì cá nhân phải chuyển lại Văn thư để đăng ký.", chu: "Nếu là *việc chung của cơ quan* → cá nhân *chuyển lại Văn thư* đăng ký", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Văn bản điện tử, phải kiểm tra tính xác thực và toàn vẹn, và thông báo đã nhận ngay trong ngày.", chu: "Văn bản điện tử: kiểm tra *xác thực* · *toàn vẹn* → báo đã nhận *ngay trong ngày*" },
      nguonCau("Điều hai mươi mốt", "Điều 21 · khoản 2"),
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
  ${mocKhai(`${NGU_CANH}Nhận văn bản đến —<br>*khi nào không bóc bì?*`, { tren: 262, co: 64, gach: "c0k2" })}
  ${the(`<div class="x-q"><span>BÓC BÌ?</span><span>ĐỂ NGUYÊN BÌ?</span><i>?</i></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("📥 Tiếp nhận văn bản đến")}
  ${hang("KIỂM TRA", "r", "Số lượng, tình trạng bì", "dấu niêm phong (nếu có), nơi gửi", "c0k1", 330)}
  ${hang("ĐỐI CHIẾU", "b", "Số, ký hiệu ghi ngoài bì", "với số, ký hiệu của văn bản trong bì", "c1k1", 520)}
  ${hang("BÓC BÌ", "a", "Bóc bì, đóng dấu ĐẾN", "kể cả văn bản có dấu chỉ độ mật", "c2k1", 710)}
  ${hang("ĐÍCH DANH", "v", "Gửi cá nhân: chuyển, không bóc bì", "tổ chức đoàn thể trong cơ quan cũng vậy", "c3k1", 900)}
  ${hang("VIỆC CHUNG", "g", "Việc chung của cơ quan", "cá nhân chuyển lại Văn thư để đăng ký", "c4k1", 1090)}`,
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
    ["02b-2", lucNode(tatCa, "ds", "c1k3w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k3w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k3w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k1w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
