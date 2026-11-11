// ============================================================================
// THỂ THỨC 19 — VĂN BẢN ĐẾN: CHƯA ĐĂNG KÝ TẠI VĂN THƯ THÌ CÓ PHẢI GIẢI QUYẾT? (Nghị định 30/2020/NĐ-CP, Điều 4 khoản c, Điều 22, Điều 23)
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN: móc câu hỏi gây tò mò (đơn vị có phải giải quyết văn bản chưa đăng ký không?) → 4 hàng hiện theo lời:
// chưa đăng ký → không có trách nhiệm giải quyết · Văn thư đăng ký và trình trong ngày · văn bản khẩn ngay · đăng ký bằng Hệ thống phải in giấy,
// ký nhận, đóng sổ → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn về số đến.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md dòng 69, 335–345, 355):
//   Điều 4 khoản 1c: «Văn bản đi, văn bản đến thuộc ngày nào phải được đăng ký, phát hành hoặc chuyển giao trong ngày, chậm nhất là trong
//   ngày làm việc tiếp theo. Văn bản đến có các mức độ khẩn: "Hỏa tốc", "Thượng khẩn" và "Khẩn" … phải được đăng ký, trình và chuyển giao
//   ngay sau khi nhận được.»
//   Điều 22: «1. … Những văn bản đến không được đăng ký tại Văn thư cơ quan thì đơn vị, cá nhân không có trách nhiệm giải quyết, trừ những
//   loại văn bản đến được đăng ký riêng theo quy định của pháp luật. 2. Số đến của văn bản được lấy liên tiếp theo thứ tự và trình tự thời
//   gian tiếp nhận văn bản trong năm, thống nhất giữa văn bản giấy và văn bản điện tử. … b) … Văn bản đến được đăng ký vào Hệ thống phải
//   được in ra giấy đầy đủ các trường thông tin theo mẫu Sổ đăng ký văn bản đến, ký nhận và đóng sổ để quản lý.»
//   Điều 23: «1. Văn bản phải được Văn thư cơ quan trình trong ngày, chậm nhất là trong ngày làm việc tiếp theo đến người có thẩm quyền chỉ
//   đạo giải quyết … Văn bản đến có dấu chỉ các mức độ khẩn phải được trình và chuyển giao ngay sau khi nhận được.»
//   ✔ chưa đăng ký tại Văn thư → đơn vị, cá nhân không có trách nhiệm giải quyết (kèm ngoại lệ «trừ loại đăng ký riêng theo pháp luật» ghi trên hình)
//   ✔ Văn thư đăng ký và trình trong ngày, chậm nhất ngày làm việc tiếp theo · ✔ văn bản khẩn: đăng ký, trình, chuyển giao ngay
//   ✔ đăng ký bằng Hệ thống → in ra giấy đủ trường thông tin, ký nhận, đóng sổ · ✔ thẻ màn kết: số đến liên tiếp trong năm, thống nhất giấy / điện tử
//   ✘ KHÔNG nói phần mềm tự đăng ký văn bản đến (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

/** một hàng: nhãn màu + dòng chữ (có thể 2 dòng) */
const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="c19-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "c19-the" });

export default {
  ten: "19-van-ban-den-dang-ky-trinh",
  tieuDe: "Thể thức 19 · Văn bản đến chưa đăng ký tại Văn thư: đơn vị có phải giải quyết không? Theo NĐ 30/2020",
  bia: { dong: ["Văn bản đến", "chưa đăng ký?"], vai: "Nghị định 30 · Điều 22" },
  ket: {
    the: [
      "«Số đến của văn bản được lấy *liên tiếp theo thứ tự và trình tự thời gian tiếp nhận văn bản trong năm*, thống nhất giữa văn bản giấy và văn bản điện tử.»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 22 · khoản 2"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản đến chưa đăng ký tại Văn thư cơ quan, đơn vị có phải giải quyết không?", chu: "Văn bản đến *chưa đăng ký* tại Văn thư — đơn vị *có phải giải quyết không?*" },
      { loi: "Nghị định ba mươi trả lời rất rõ.", chu: "Nghị định 30 *trả lời rất rõ*", nghiSau: 0.2 },
    ]},
    { canh: "den", cau: [
      { loi: "Chưa đăng ký tại Văn thư cơ quan, thì đơn vị, cá nhân không có trách nhiệm giải quyết.", chu: "Chưa đăng ký tại Văn thư → đơn vị, cá nhân *không có trách nhiệm giải quyết*" },
      { loi: "Văn thư đăng ký và trình, trong ngày, chậm nhất là trong ngày làm việc tiếp theo.", chu: "Văn thư *đăng ký và trình* · *trong ngày* · chậm nhất *ngày làm việc tiếp theo*" },
      { loi: "Văn bản khẩn, phải đăng ký, trình, và chuyển giao ngay.", chu: "Văn bản *khẩn*: đăng ký · trình · chuyển giao *ngay*" },
      { loi: "Đăng ký bằng Hệ thống, thì phải in ra giấy đầy đủ, ký nhận, và đóng sổ.", chu: "Đăng ký bằng *Hệ thống* → *in ra giấy đầy đủ* · ký nhận · đóng sổ" },
      { loi: "Chuyển giao văn bản giấy, thì đơn vị, cá nhân phải ký nhận.", chu: "Chuyển giao văn bản *giấy* → đơn vị, cá nhân *phải ký nhận*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Số đến được lấy liên tiếp, theo thứ tự thời gian tiếp nhận trong năm, thống nhất giữa giấy và điện tử.", chu: "Số đến: liên tiếp theo *thứ tự thời gian tiếp nhận trong năm* · thống nhất giấy / điện tử" },
      nguonCau("Điều hai mươi hai", "Điều 22 · khoản 2"),
    ]),
  ],

  css: CSS_CC + `
.c19-q{padding:40px 30px 46px;text-align:center}
.c19-q .vb{display:inline-block;font-size:56px;font-weight:900;color:#0f172a;background:#f1f5f9;border-radius:20px;padding:10px 34px}
.c19-q .ar{font-size:50px;font-weight:900;color:#94a3b8;margin:10px 0}
.c19-q .vt{display:inline-block;font-size:48px;font-weight:900;color:#1d4ed8;background:#dbeafe;border-radius:20px;padding:10px 30px}
.c19-q .q{display:inline-block;margin-left:14px;font-size:56px;font-weight:900;color:#b45309}
.c19-the{padding:0}
.c19-h{display:flex;align-items:center;gap:22px;padding:18px 26px}
.c19-h .nh{flex:none;width:180px;text-align:center;font-size:34px;font-weight:900;color:#fff;border-radius:16px;padding:12px 0;line-height:1.1}
.c19-h .nh.g{background:#15803d}.c19-h .nh.r{background:#dc2626}.c19-h .nh.b{background:#1d4ed8}.c19-h .nh.a{background:#d97706}.c19-h .nh.v{background:#0f766e}
.c19-h .tx{flex:1;font-size:44px;font-weight:900;color:#0f172a;line-height:1.15}
.c19-h .tx small{display:block;margin-top:6px;font-size:31px;font-weight:800;color:#64748b}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Văn bản đến chưa đăng ký —<br>*có phải giải quyết?*`, { tren: 262, co: 70, gach: "c0k1" })}
  ${the(`<div class="c19-q"><span class="vb">VĂN BẢN ĐẾN</span><div class="ar">↓ chưa đăng ký tại</div><span class="vt">VĂN THƯ CƠ QUAN</span><span class="q">?</span></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "den": {
      khai: true,
      html: () => `
  ${chipKhai("📥 Văn bản đến")}
  ${hang("CHƯA ĐĂNG KÝ", "r", "Không có trách nhiệm giải quyết", "trừ loại văn bản được đăng ký riêng theo pháp luật", "c0k1", 330)}
  ${hang("VĂN THƯ", "b", "Đăng ký và trình lãnh đạo", "trong ngày · chậm nhất ngày làm việc tiếp theo", "c1k0", 520)}
  ${hang("KHẨN", "a", "Đăng ký, trình, chuyển giao NGAY", "hỏa tốc · thượng khẩn · khẩn", "c2k0", 710)}
  ${hang("HỆ THỐNG", "v", "In ra giấy đầy đủ · ký nhận · đóng sổ", "văn bản đăng ký bằng Hệ thống", "c3k1", 900)}
  ${hang("GIẤY", "g", "Ký nhận khi chuyển giao", "đơn vị, cá nhân nhận văn bản giấy", "c4k2", 1090)}`,
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
    ["02a-chua-dk", lucNode(tatCa, "den", "c0k2w3")],
    ["02b-trinh", lucNode(tatCa, "den", "c1k2w5")],
    ["02c-khan", lucNode(tatCa, "den", "c2k3w2")],
    ["02d-he-thong", lucNode(tatCa, "den", "c3k3w2")],
    ["02e-ky-nhan", lucNode(tatCa, "den", "c4k2w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
