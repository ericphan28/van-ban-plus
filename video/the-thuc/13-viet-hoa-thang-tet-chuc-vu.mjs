// ============================================================================
// THỂ THỨC 13 — VIẾT HOA: «THÁNG NĂM» HAY «THÁNG NĂM»? «TẾT» HAY «TẾT»? (Nghị định 30/2020/NĐ-CP, Phụ lục II, mục V và mục III.1b)
//
// Khuôn ĐỐ VUI CÓ ĐÁP ÁN: móc hai cặp dễ viết sai → thời gian (tháng, thứ, tết) → từ đặc biệt và chức vụ đi liền tên người → cảnh
// phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn về đơn vị hành chính gắn chữ số / tên người.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-II.md dòng 62–78, 28):
//   «1. Danh từ thuộc trường hợp đặc biệt: Nhân dân, Nhà nước.»
//   «3. Tên chức vụ, học vị, danh hiệu: Viết hoa tên chức vụ, học vị nếu đi liền với tên người cụ thể. Ví dụ: Chủ tịch Quốc hội, Thủ
//   tướng Chính phủ, Giáo sư Tôn Thất Tùng,...»
//   «8. … b) Tên các ngày tết: Viết hoa chữ cái đầu của âm tiết thứ nhất. Ví dụ: tết Nguyên đán, tết Đoan ngọ, tết Trung thu. Viết hoa
//   chữ Tết khi thay cho tết Nguyên đán. c) Tên các ngày trong tuần và tháng: Viết hoa chữ cái đầu khi không dùng chữ số. Ví dụ: thứ Hai,
//   thứ Tư, tháng Năm, tháng Tám,...»
//   «III.1 b) Trường hợp tên đơn vị hành chính được cấu tạo giữa danh từ chung kết hợp với chữ số, tên người, tên sự kiện lịch sử: Viết
//   hoa cả danh từ chung chỉ đơn vị hành chính đó. Ví dụ: Quận 1, Phường Điện Biên Phủ,...»
//   ✔ «tháng, thứ: viết hoa chữ đầu khi không dùng chữ số» (tháng Năm, thứ Hai; «tháng 5» dùng chữ số → không có chữ để viết hoa)
//   ✔ «tên ngày tết: viết hoa chữ cái đầu của âm tiết thứ nhất — tết Nguyên đán, tết Trung thu» (chữ «tết» đứng đầu viết thường,
//     chữ đầu của tên là «Nguyên», «Trung») · ✔ «Tết» viết hoa khi thay cho tết Nguyên đán
//   ✔ «Nhân dân», «Nhà nước» = trường hợp đặc biệt · ✔ chức vụ, học vị: viết hoa nếu đi liền tên người cụ thể
//   ✔ thẻ màn kết: Quận 1, Phường Điện Biên Phủ (nguyên văn)
//   ✘ KHÔNG đụng tới điểm mâu thuẫn «viết hoa khoản, điểm» của mục V.7 (quy tắc nói viết hoa nhưng ví dụ viết thường).
//   ✘ KHÔNG nói phần mềm tự viết hoa đúng (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

/** một hàng: chữ đúng (xanh) + ghi chú xám; `lop` thêm lớp (vd "xam" cho hàng dùng chữ số) */
const hang = (dung, ghi, hien, tren, lop = "") =>
  the(`<div class="c13-r ${lop}"><div class="d">${dung}</div><div class="g">${ghi}</div></div>`, { tren, hien, lop: "c13-the" });

export default {
  ten: "13-viet-hoa-thang-tet-chuc-vu",
  tieuDe: "Thể thức 13 · Viết hoa: «tháng Năm» hay «tháng năm»? «Tết» hay «tết»? Theo NĐ 30/2020",
  bia: { dong: ["Tháng Năm hay", "tháng năm? Tết?"], vai: "Nghị định 30 · Phụ lục II" },
  ket: {
    the: [
      "«Trường hợp tên đơn vị hành chính được cấu tạo giữa danh từ chung kết hợp với chữ số, tên người, tên sự kiện lịch sử: *viết hoa cả danh từ chung* chỉ đơn vị hành chính đó. Ví dụ: *Quận 1, Phường Điện Biên Phủ*,…»",
      theNguon("Phụ lục II · mục III.1 · điểm b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Tháng Năm, viết hoa chữ đầu, hay viết thường? Còn Tết, thì sao?", chu: "«Tháng Năm» hay «tháng năm»? «Tết» hay «tết»? — *viết hoa thế nào?*" },
      { loi: "Nghị định ba mươi có đáp án rõ ràng.", chu: "Nghị định 30 có *đáp án rõ ràng*", nghiSau: 0.2 },
    ]},
    { canh: "thoi-gian", cau: [
      { loi: "Tháng và thứ, viết hoa chữ đầu, khi không dùng chữ số.", chu: "Tháng · thứ: *viết hoa chữ đầu* khi *không dùng chữ số*" },
      { loi: "Tên các ngày tết, viết hoa chữ đầu của tên, như tết Nguyên đán, tết Trung thu.", chu: "Ngày tết: hoa *chữ đầu của tên* — tết *N*guyên đán · tết *T*rung thu" },
      { loi: "Nói tắt Tết, thay cho tết Nguyên đán, thì viết hoa chữ Tết.", chu: "Nói tắt *«Tết»* thay cho tết Nguyên đán → *viết hoa*", nghiSau: 0.3 },
    ]},
    { canh: "rieng", cau: [
      { loi: "Nhân dân, và Nhà nước, là hai trường hợp đặc biệt, viết hoa.", chu: "*Nhân dân* · *Nhà nước* — trường hợp đặc biệt, *viết hoa*" },
      { loi: "Chức vụ, học vị, chỉ viết hoa khi đi liền với tên người cụ thể.", chu: "Chức vụ · học vị: hoa *khi đi liền tên người cụ thể*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Đơn vị hành chính gắn với chữ số, tên người, hoặc sự kiện lịch sử, thì viết hoa cả danh từ chung.", chu: "…gắn với chữ số, tên người, sự kiện lịch sử → *viết hoa cả danh từ chung*" },
      nguonCau("Phụ lục hai", "Phụ lục II · mục III.1 · điểm b"),
    ]),
  ],

  css: CSS_CC + `
.c13-p{padding:40px 30px 46px}
.c13-p .l{display:flex;align-items:center;gap:24px;margin:12px 0;font-size:56px;font-weight:700;font-family:"Times New Roman",serif;color:#111}
.c13-p .l i{flex:none;font-style:normal;font-family:system-ui,sans-serif;font-size:34px;font-weight:900;color:#92400e;background:#fef3c7;border-radius:12px;padding:2px 16px}
.c13-the{padding:0}
.c13-r{display:flex;flex-direction:column;align-items:flex-start;gap:2px;padding:14px 34px}
.c13-r .d{font-family:"Times New Roman",serif;font-size:62px;font-weight:700;color:#15803d;white-space:nowrap}
.c13-r .g{font-size:33px;font-weight:800;color:#64748b;text-align:left;line-height:1.2}
.c13-r.xam .d{color:#64748b}
.c13-r.vang .d{color:#b45309}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Viết hoa —<br>*tháng Năm? Tết?*`, { tren: 262, co: 78, gach: "c0k2" })}
  ${the(`<div class="c13-p"><div class="l">tháng Năm <span style="color:#94a3b8;font-size:40px">/</span> tháng năm<i>?</i></div><div class="l">Tết <span style="color:#94a3b8;font-size:40px">/</span> tết<i>?</i></div></div>`, { tren: 700, rung: "c1k0w2" })}`,
      ve: veCC,
    },

    "thoi-gian": {
      khai: true,
      html: () => `
  ${chipKhai("📅 Tháng · thứ · ngày tết")}
  ${hang("tháng Năm · thứ Hai", "không dùng chữ số → viết hoa chữ đầu", "c0k0", 330)}
  ${hang("tháng 5", "dùng chữ số → không có chữ để viết hoa", "c0k2", 500, "xam")}
  ${hang("tết Nguyên đán", "hoa chữ đầu của tên", "c1k2", 670)}
  ${hang("tết Trung thu", "hoa chữ đầu của tên", "c1k3", 840)}
  ${hang("Tết", "nói tắt, thay cho tết Nguyên đán", "c2k0", 1010, "vang")}`,
    },

    "rieng": {
      khai: true,
      html: () => `
  ${chipKhai("🔠 Từ đặc biệt · chức vụ")}
  ${hang("Nhân dân · Nhà nước", "trường hợp đặc biệt: viết hoa", "c0k0", 380)}
  ${hang("Chủ tịch Quốc hội", "chức vụ đi liền tên người cụ thể", "c1k2", 590)}
  ${hang("Giáo sư Tôn Thất Tùng", "học vị đi liền tên người cụ thể", "c1k2", 790)}
  ${goiKhai("<span>Ví dụ nguyên văn của <b>Phụ lục II</b></span>", { trai: 60, tren: 990, hien: "c1k2w3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["02a-so", lucNode(tatCa, "thoi-gian", "c0k2w3")],
    ["02b-tet", lucNode(tatCa, "thoi-gian", "c1k3w2")],
    ["02c-tat", lucNode(tatCa, "thoi-gian", "c2k2w3")],
    ["03a-dac-biet", lucNode(tatCa, "rieng", "c0k1w2")],
    ["03b-chuc-vu", lucNode(tatCa, "rieng", "c1k2w5")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
