// ============================================================================
// THỂ THỨC 30 — THỂ THỨC VĂN BẢN HÀNH CHÍNH GỒM MẤY THÀNH PHẦN? 9 THÀNH PHẦN CHÍNH VÀ 4 NHÓM BỔ SUNG. THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 8 khoản 2: «Thể thức văn bản hành chính bao gồm các thành phần chính: a) Quốc hiệu và Tiêu ngữ. b) Tên cơ quan, tổ chức ban hành văn bản. c) Số, ký hiệu của văn bản.
//   d) Địa danh và thời gian ban hành văn bản. đ) Tên loại và trích yếu nội dung văn bản. e) Nội dung văn bản. g) Chức vụ, họ tên và chữ ký của người có thẩm quyền. h) Dấu, chữ ký số
//   của cơ quan, tổ chức. i) Nơi nhận.» Khoản 3: «… văn bản có thể bổ sung các thành phần khác: a) Phụ lục. b) Dấu chỉ độ mật, mức độ khẩn, các chỉ dẫn về phạm vi lưu hành.
//   c) Ký hiệu người soạn thảo văn bản và số lượng bản phát hành. d) Địa chỉ cơ quan, tổ chức; thư điện tử; trang thông tin điện tử; số điện thoại; số Fax.»
//     ✔ ĐẾM: điểm a, b, c, d, đ, e, g, h, i = 9 thành phần chính; 4 nhóm bổ sung (a–d).
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "30-chin-thanh-phan-the-thuc",
  tieuDe: "Thể thức 30 · Thể thức văn bản hành chính gồm mấy thành phần? 9 thành phần chính và 4 nhóm bổ sung. Theo NĐ 30/2020",
  bia: { dong: ["Thể thức văn bản:","9 thành phần, đủ chưa?"], vai: "Nghị định 30 · Điều 8" },
  ket: {
    the: [
      "«… văn bản có thể *bổ sung* các thành phần khác: *Phụ lục* · *Dấu chỉ độ mật, mức độ khẩn* · *Ký hiệu người soạn thảo và số lượng bản phát hành* · *Địa chỉ cơ quan, tổ chức; thư điện tử; số điện thoại…*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 8 · khoản 3"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Thể thức văn bản hành chính, gồm mấy thành phần chính, kể đủ được không?", chu: "Thể thức văn bản: *mấy thành phần chính?* Kể *đủ không?*" },
      { loi: "Nghị định ba mươi nêu đúng chín thành phần.", chu: "Nghị định 30 nêu *đúng 9 thành phần*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Hai thành phần đầu, quốc hiệu và tiêu ngữ, rồi tên cơ quan ban hành.", chu: "*Quốc hiệu và Tiêu ngữ* · *tên cơ quan* ban hành" },
      { loi: "Tiếp theo, số và ký hiệu, địa danh và thời gian ban hành.", chu: "*Số, ký hiệu* · *địa danh* và *thời gian* ban hành" },
      { loi: "Rồi tên loại và trích yếu, và nội dung văn bản.", chu: "*Tên loại* và *trích yếu* · *nội dung* văn bản" },
      { loi: "Chức vụ, họ tên, chữ ký của người có thẩm quyền, rồi dấu, chữ ký số của cơ quan.", chu: "*Chức vụ · họ tên · chữ ký* · *dấu, chữ ký số* cơ quan" },
      { loi: "Cuối cùng, nơi nhận, là thành phần thứ chín.", chu: "Cuối cùng: *nơi nhận* — thành phần *thứ 9*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Ngoài chín thành phần chính, còn bốn nhóm thành phần bổ sung, như phụ lục, dấu độ mật, và địa chỉ cơ quan.", chu: "Ngoài 9 thành phần chính: *4 nhóm bổ sung* — *phụ lục* · *dấu độ mật* · *địa chỉ cơ quan*" },
      nguonCau("Điều tám", "Điều 8 · khoản 3"),
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
  ${mocKhai(`${NGU_CANH}Thể thức văn bản —<br>*gồm mấy thành phần chính?*`, { tren: 262, co: 64, gach: "c0k1" })}
  ${the(`<div class="x-q"><span>QUỐC HIỆU</span><span>NƠI NHẬN</span><span>DẤU?</span><i>?</i></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("🧩 Thể thức · 9 thành phần chính")}
  ${hang("ĐIỂM a · b", "r", "Quốc hiệu và Tiêu ngữ", "tên cơ quan, tổ chức ban hành", "c0k1", 330)}
  ${hang("ĐIỂM c · d", "b", "Số, ký hiệu của văn bản", "địa danh và thời gian ban hành", "c1k1", 520)}
  ${hang("ĐIỂM đ · e", "a", "Tên loại và trích yếu", "nội dung văn bản", "c2k1", 710)}
  ${hang("ĐIỂM g · h", "v", "Chức vụ, họ tên, chữ ký người có thẩm quyền", "dấu, chữ ký số của cơ quan, tổ chức", "c3k1", 900)}
  ${hang("ĐIỂM i", "g", "Nơi nhận", "thành phần thứ chín", "c4k1", 1090)}`,
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
    ["02c-3", lucNode(tatCa, "ds", "c2k1w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k4w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k2w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
