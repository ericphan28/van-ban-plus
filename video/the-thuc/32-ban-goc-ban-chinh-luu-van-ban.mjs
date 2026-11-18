// ============================================================================
// THỂ THỨC 32 — BẢN GỐC VÀ BẢN CHÍNH VĂN BẢN KHÁC NHAU THẾ NÀO, LƯU Ở ĐÂU? VĂN BẢN ĐIỆN TỬ LƯU RA SAO? THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 3 khoản 8: «"Bản gốc văn bản" là bản hoàn chỉnh về nội dung, thể thức văn bản, được người có thẩm quyền trực tiếp ký trên văn bản giấy hoặc ký số trên văn bản điện tử.»
//   Khoản 9: «"Bản chính văn bản giấy" là bản hoàn chỉnh về nội dung, thể thức văn bản, được tạo từ bản có chữ ký trực tiếp của người có thẩm quyền.»
//   Điều 19 khoản 1: «a) Bản gốc văn bản được lưu tại Văn thư cơ quan và phải được đóng dấu ngay sau khi phát hành, sắp xếp theo thứ tự đăng ký. b) Bản chính văn bản lưu tại hồ sơ công việc.»
//   Khoản 2: «a) Bản gốc văn bản điện tử phải được lưu trên Hệ thống của cơ quan, tổ chức ban hành văn bản. b) … Hệ thống đáp ứng theo quy định tại Phụ lục VI … thì sử dụng và lưu bản gốc
//   văn bản điện tử trên Hệ thống thay cho văn bản giấy. c) … Hệ thống chưa đáp ứng … thì Văn thư cơ quan tạo bản chính văn bản giấy theo quy định tại khoản 5 Điều 18 … để lưu tại Văn thư cơ quan và hồ sơ công việc.»
//   Điều 18 khoản 5: «… Văn thư cơ quan thực hiện in văn bản đã được ký số của người có thẩm quyền ra giấy, đóng dấu của cơ quan, tổ chức để tạo bản chính văn bản giấy và phát hành văn bản.»
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "32-ban-goc-ban-chinh-luu-van-ban",
  tieuDe: "Thể thức 32 · Bản gốc và bản chính văn bản khác nhau thế nào, lưu ở đâu? Văn bản điện tử lưu ra sao? Theo NĐ 30/2020",
  bia: { dong: ["Bản gốc, bản chính:","khác nhau thế nào?"], vai: "Nghị định 30 · Điều 3, 18, 19" },
  ket: {
    the: [
      "«Cơ quan, tổ chức có Hệ thống *chưa đáp ứng* … thì Văn thư cơ quan *tạo bản chính văn bản giấy* … để lưu tại Văn thư cơ quan và hồ sơ công việc.»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 19 · khoản 2 · điểm c"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Bản gốc và bản chính văn bản, khác nhau thế nào, và lưu ở đâu?", chu: "*Bản gốc* và *bản chính*: khác nhau *thế nào?* Lưu *ở đâu?*" },
      { loi: "Nghị định ba mươi phân biệt rất rõ.", chu: "Nghị định 30 *phân biệt rất rõ*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Bản gốc, là bản hoàn chỉnh, được người có thẩm quyền trực tiếp ký, hoặc ký số.", chu: "*Bản gốc*: người có thẩm quyền *trực tiếp ký* hoặc *ký số*" },
      { loi: "Bản chính văn bản giấy, được tạo từ bản có chữ ký trực tiếp.", chu: "*Bản chính* (giấy): *tạo từ bản* có chữ ký trực tiếp" },
      { loi: "Văn bản giấy, bản gốc lưu ở Văn thư, và phải đóng dấu ngay sau khi phát hành.", chu: "Bản gốc: lưu ở *Văn thư* · *đóng dấu ngay* sau phát hành" },
      { loi: "Còn bản chính, lưu tại hồ sơ công việc, bản gốc thì xếp theo thứ tự đăng ký.", chu: "*Bản chính*: lưu ở *hồ sơ công việc* · bản gốc xếp theo *số đăng ký*" },
      { loi: "Văn bản điện tử, bản gốc lưu trên Hệ thống, nếu Hệ thống đáp ứng quy định, thì thay cho văn bản giấy.", chu: "Điện tử: bản gốc *lưu trên Hệ thống* · Hệ thống đáp ứng → *thay bản giấy*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Hệ thống chưa đáp ứng, thì in văn bản ký số ra giấy, đóng dấu, để tạo bản chính.", chu: "Hệ thống chưa đáp ứng → *in ra giấy* · *đóng dấu cơ quan* = *bản chính*" },
      nguonCau("Điều mười chín", "Điều 19 · khoản 2 · điểm c"),
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
  ${mocKhai(`${NGU_CANH}Bản gốc và bản chính —<br>*khác nhau thế nào?*`, { tren: 262, co: 62, gach: "c0k2" })}
  ${the(`<div class="x-q"><span>BẢN GỐC</span><i>≠</i><span>BẢN CHÍNH</span></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("🗄️ Bản gốc · bản chính")}
  ${hang("BẢN GỐC", "r", "Người có thẩm quyền trực tiếp ký", "trên giấy, hoặc ký số trên điện tử", "c0k1", 330)}
  ${hang("BẢN CHÍNH", "b", "Tạo từ bản có chữ ký trực tiếp", "bản chính văn bản giấy", "c1k1", 520)}
  ${hang("LƯU GIẤY", "a", "Bản gốc lưu ở Văn thư cơ quan", "đóng dấu ngay sau khi phát hành", "c2k1", 710)}
  ${hang("HỒ SƠ", "v", "Bản chính lưu trong hồ sơ công việc", "bản gốc xếp theo thứ tự đăng ký", "c3k1", 900)}
  ${hang("ĐIỆN TỬ", "g", "Bản gốc lưu trên Hệ thống", "đáp ứng quy định thì thay bản giấy", "c4k1", 1090)}`,
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
    ["02c-3", lucNode(tatCa, "ds", "c2k2w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k2w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k3w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
