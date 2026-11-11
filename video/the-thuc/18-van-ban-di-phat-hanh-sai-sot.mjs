// ============================================================================
// THỂ THỨC 18 — VĂN BẢN ĐÃ PHÁT HÀNH MÀ SAI SÓT: SỬA THẾ NÀO? PHÁT HÀNH TRONG BAO LÂU? (Nghị định 30/2020/NĐ-CP, Điều 18 khoản 1, 3; Điều 15 khoản 2)
//
// Khuôn HỎI – ĐÁP CÓ SƠ ĐỒ: móc «đã phát hành mới thấy sai» → sai NỘI DUNG (sửa đổi / thay thế bằng văn bản tương đương) khác sai
// THỂ THỨC (đính chính bằng công văn) → phát hành trong ngày ký, chậm nhất ngày làm việc tiếp theo; văn bản khẩn gửi ngay → cảnh phần mềm
// + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn về cấp số văn bản giấy.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md dòng 232, 270–274):
//   Điều 18: «1. Văn bản đi phải hoàn thành thủ tục tại Văn thư cơ quan và phát hành trong ngày văn bản đó được ký, chậm nhất là trong
//   ngày làm việc tiếp theo. Văn bản khẩn phải được phát hành và gửi ngay sau khi ký văn bản. …
//   3. Văn bản đã phát hành nhưng có sai sót về nội dung phải được sửa đổi, thay thế bằng văn bản có hình thức tương đương. Văn bản đã
//   phát hành nhưng có sai sót về thể thức, kỹ thuật trình bày, thủ tục ban hành phải được đính chính bằng công văn của cơ quan, tổ chức
//   ban hành văn bản.»
//   Điều 15 khoản 2: «Đối với văn bản giấy, việc cấp số, thời gian ban hành được thực hiện sau khi có chữ ký của người có thẩm quyền,
//   chậm nhất là trong ngày làm việc tiếp theo. Văn bản mật được cấp hệ thống số riêng.»
//   ✔ sai nội dung → «sửa đổi, thay thế bằng văn bản có hình thức tương đương» (ví dụ «Quyết định → Quyết định sửa đổi» chỉ minh họa,
//     ghi «ví dụ minh họa») · ✔ sai thể thức / kỹ thuật trình bày / thủ tục ban hành → «đính chính bằng công văn của cơ quan ban hành»
//   ✔ phát hành trong ngày ký, chậm nhất ngày làm việc tiếp theo; văn bản khẩn gửi ngay sau khi ký
//   ✔ thẻ màn kết: cấp số văn bản giấy sau khi có chữ ký, chậm nhất ngày làm việc tiếp theo
//   ✘ KHÔNG nói phần mềm tự đính chính / tự cấp số (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

export default {
  ten: "18-van-ban-di-phat-hanh-sai-sot",
  tieuDe: "Thể thức 18 · Văn bản đã phát hành mà sai sót: sửa thế nào? Phát hành trong bao lâu? Theo NĐ 30/2020",
  bia: { dong: ["Đã phát hành", "mới thấy sai?"], vai: "Nghị định 30 · Điều 18" },
  ket: {
    the: [
      "«Đối với văn bản giấy, việc cấp số, thời gian ban hành được thực hiện *sau khi có chữ ký của người có thẩm quyền, chậm nhất là trong ngày làm việc tiếp theo.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 15 · khoản 2"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản đã phát hành, mới phát hiện sai sót, sửa thế nào?", chu: "Văn bản *đã phát hành* — mới thấy *sai sót* — sửa thế nào?" },
      { loi: "Sai nội dung, và sai thể thức, sửa khác nhau.", chu: "Sai *nội dung* và sai *thể thức* — sửa *khác nhau*", nghiSau: 0.2 },
    ]},
    { canh: "sai-sot", cau: [
      { loi: "Sai về nội dung, phải sửa đổi, hoặc thay thế, bằng văn bản có hình thức tương đương.", chu: "Sai *nội dung* → *sửa đổi* hoặc *thay thế* bằng văn bản *hình thức tương đương*" },
      { loi: "Sai về thể thức, kỹ thuật trình bày, hoặc thủ tục ban hành, phải đính chính bằng công văn.", chu: "Sai *thể thức* · *kỹ thuật trình bày* · *thủ tục ban hành* → *đính chính bằng công văn*" },
      { loi: "Văn bản đi, phải phát hành trong ngày ký, chậm nhất là trong ngày làm việc tiếp theo.", chu: "Văn bản đi: phát hành *trong ngày ký* · chậm nhất *ngày làm việc tiếp theo*" },
      { loi: "Văn bản khẩn, phải phát hành và gửi ngay sau khi ký.", chu: "Văn bản *khẩn*: phát hành, gửi *ngay sau khi ký*" },
      { loi: "Văn bản mật, thì được cấp hệ thống số riêng.", chu: "Văn bản *mật*: cấp *hệ thống số riêng*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Văn bản giấy, cấp số sau khi có chữ ký, chậm nhất trong ngày làm việc tiếp theo.", chu: "Văn bản giấy: cấp số *sau khi có chữ ký* · chậm nhất *ngày làm việc tiếp theo*" },
      nguonCau("Điều mười lăm", "Điều 15 · khoản 2"),
    ]),
  ],

  css: CSS_CC + `
.c18-q{display:flex;justify-content:center;gap:26px;padding:46px 24px 52px}
.c18-q span{font-size:46px;font-weight:900;color:#b91c1c;background:#fee2e2;border-radius:20px;padding:14px 30px;text-align:center;line-height:1.15}
.c18-b{padding:32px 30px 36px}
.c18-b .t{font-size:30px;font-weight:900;color:#64748b;letter-spacing:.04em;margin-bottom:10px}
.c18-b .d{display:flex;align-items:center;gap:18px;font-size:42px;font-weight:900;color:#0f172a;line-height:1.15}
.c18-b .d .a{flex:none;font-size:56px;color:#2563eb}
.c18-b .d b{color:#15803d}
.c18-b .vd{margin-top:8px;font-size:26px;font-weight:700;color:#94a3b8}
.c18-the{padding:0}
.c18-tl{padding:30px 30px 34px}
.c18-tl .bar{position:relative;height:30px;border-radius:99px;background:linear-gradient(90deg,#16a34a 0 50%,#f59e0b 50% 100%);margin:16px 0 12px}
.c18-tl .hai{display:flex;justify-content:space-between;font-size:34px;font-weight:900;color:#0f172a}
.c18-tl .hai small{display:block;font-size:26px;font-weight:800;color:#64748b}
.c18-tl .hai div:last-child{text-align:right}
.c18-kh{padding:22px 34px;text-align:center;font-size:42px;font-weight:900;color:#b91c1c}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Đã phát hành mới thấy sai —<br>*sửa thế nào?*`, { tren: 262, co: 70, gach: "c0k2" })}
  ${the(`<div class="c18-q"><span>Sai<br>NỘI DUNG</span><span style="color:#92400e;background:#fef3c7">Sai<br>THỂ THỨC</span></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "sai-sot": {
      khai: true,
      html: () => `
  ${chipKhai("🛠️ Sửa sai sau khi phát hành")}
  ${the(`<div class="c18-b"><div class="t">SAI NỘI DUNG</div><div class="d"><span>Quyết định</span><span class="a">→</span><b>Quyết định sửa đổi</b></div><div class="vd">hoặc thay thế · ví dụ minh họa</div></div>`, { tren: 330, hien: "c0k0", lop: "c18-the" })}
  ${the(`<div class="c18-b"><div class="t">SAI THỂ THỨC · TRÌNH BÀY · THỦ TỤC</div><div class="d"><span>Văn bản sai</span><span class="a">→</span><b>Công văn đính chính</b></div><div class="vd">của cơ quan ban hành văn bản · ví dụ minh họa</div></div>`, { tren: 590, hien: "c1k0", lop: "c18-the" })}
  ${the(`<div class="c18-tl"><div class="hai"><div>Ngày ký<small>phát hành trong ngày</small></div><div>Ngày làm việc tiếp theo<small>chậm nhất</small></div></div><div class="bar"></div></div>`, { tren: 850, hien: "c2k0", lop: "c18-the" })}
  ${the(`<div class="c18-kh"><div>KHẨN → gửi NGAY sau khi ký</div><div data-k="lop" data-hien="c4k1" style="opacity:0;color:#475569;margin-top:10px">MẬT → hệ thống số riêng</div></div>`, { tren: 1030, hien: "c3k0", lop: "c18-the" })}`,
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
    ["02a-noi-dung", lucNode(tatCa, "sai-sot", "c0k3w3")],
    ["02b-the-thuc", lucNode(tatCa, "sai-sot", "c1k3w3")],
    ["02c-trong-ngay", lucNode(tatCa, "sai-sot", "c2k2w4")],
    ["02d-khan", lucNode(tatCa, "sai-sot", "c3k1w3")],
    ["02e-mat", lucNode(tatCa, "sai-sot", "c4k1w4")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
