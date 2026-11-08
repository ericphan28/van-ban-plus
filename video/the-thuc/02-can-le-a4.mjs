// ============================================================================
// THỂ THỨC 02 — CĂN LỀ VĂN BẢN HÀNH CHÍNH (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục I.1–I.4)
//
// Khuôn CON SỐ: móc «lề trái rộng nhất 30–35 mm» → trên/dưới 20–25, phải 15–20 → khổ giấy, phông, ví dụ đặt bằng cm →
// giới thiệu phần mềm (tên chỉ trên hình) → Tóm lại → thẻ nguyên văn mục I.3.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 20–23):
//   I.1 «Khổ giấy: Khổ A4 (210 mm x 297 mm).» · I.2 «Kiểu trình bày: Theo chiều dài của khổ A4…» (đặt dọc)
//   I.3 «Định lề trang: Cách mép trên và mép dưới 20 - 25 mm, cách mép trái 30 - 35 mm, cách mép phải 15 - 20 mm.»
//   I.4 «Phông chữ tiếng Việt Times New Roman … màu đen.» (tên phông chỉ trên hình)
//   ✔ ví dụ «trên 2, dưới 2, trái 3, phải 1,5 cm» = 20 / 20 / 30 / 15 mm — nằm trong khoảng cho phép (cận dưới).
//   ✔ cảnh app: như clip 01 (PROJECT_STATUS.md mục A; trang tải «Tải miễn phí cho Windows»).
//   ✘ KHÔNG nói: phần mềm tự căn lề (chưa soát), tên cơ quan thật.
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, theApp, veCC } from "./loat.mjs";

/** tờ A4 (480 × 679 px, 2,286 px/mm) với 4 dải lề tô màu theo mốc; lề vẽ ở mức 20 / 20 / 30 / 15 mm */
const toA4 = ({ tren, duoi, trai, phai }, top = 520) => {
  const dai = (cls, moc, st) => (moc ? `<i class="c02-dai ${cls}" data-k="lop" data-hien="${moc}" style="opacity:0;${st}"></i>` : "");
  return `<div class="tt-to c02-to" style="left:120px;top:${top}px;width:480px;height:679px">
    <div class="c02-vung">${Array.from({ length: 11 }, (_, i) => `<b style="width:${i === 0 ? 55 : i === 10 ? 40 : 92 + (i % 3) * 3}%"></b>`).join("")}</div>
    ${dai("ngang", tren, "left:0;top:0;width:100%;height:46px")}
    ${dai("ngang", duoi, "left:0;bottom:0;width:100%;height:46px")}
    ${dai("doc", trai, "left:0;top:0;width:69px;height:100%")}
    ${dai("doc", phai, "right:0;top:0;width:34px;height:100%")}
  </div>`;
};
const nhan = (html, moc, tren) => goiKhai(`<span>${html}</span>`, { trai: 640, tren, hien: moc, tu: "phai", rong: 380, lop: "c02-nhan" });

export default {
  ten: "02-can-le-a4",
  tieuDe: "Thể thức 02 · Căn lề văn bản hành chính đúng NĐ 30/2020: trái 30–35 mm",
  bia: { dong: ["Lề trái bao", "nhiêu là đúng?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Định lề trang: Cách mép trên và mép dưới *20 - 25 mm*, cách mép trái *30 - 35 mm*, cách mép phải *15 - 20 mm*.»",
      theNguon("Phụ lục I · mục I.3"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Căn lề văn bản hành chính, lề trái rộng nhất, ba mươi đến ba mươi lăm mi li mét.", chu: "Lề trái *rộng nhất*: *30–35 mm*" },
      { loi: "Anh chị đang để lề trái bao nhiêu?", chu: "Anh chị đang để *bao nhiêu?*" },
    ]},
    { canh: "can-le", cau: [
      { loi: "Lề trên và lề dưới, hai mươi đến hai mươi lăm mi li mét.", chu: "Trên, dưới: *20–25 mm*" },
      { loi: "Lề phải hẹp nhất, mười lăm đến hai mươi mi li mét.", chu: "Lề phải *hẹp nhất*: *15–20 mm*", nghiSau: 0.3 },
    ]},
    { canh: "giay", cau: [
      { loi: "Giấy khổ A bốn, đặt dọc, chữ màu đen.", chu: "Khổ *A4* · đặt dọc · chữ *màu đen*" },
      { loi: "Đặt bằng xen ti mét, trên hai, dưới hai, trái ba, phải một phẩy năm, là đúng.", chu: "Ví dụ: trên *2* · dưới *2* · trái *3* · phải *1,5* cm", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["lề trái ba mươi đến ba mươi lăm", "Trái 30–35 mm"],
      ["lề phải mười lăm đến hai mươi", "Phải 15–20 mm"],
      ["trên và dưới hai mươi đến hai mươi lăm", "Trên, dưới 20–25 mm"],
    ]),
    canhKet([
      { loi: "Định lề trang, cách mép trên và mép dưới hai mươi đến hai mươi lăm mi li mét, cách mép trái ba mươi đến ba mươi lăm, cách mép phải mười lăm đến hai mươi mi li mét.", chu: "Cách mép trên, dưới 20–25 mm · trái 30–35 mm · phải 15–20 mm" },
      nguonCau("Phụ lục một", "Phụ lục I · mục I.3"),
    ]),
  ],

  css: CSS_CC + `
.c02-to{overflow:hidden}
.c02-vung{position:absolute;left:69px;right:34px;top:46px;bottom:46px;border:3px dashed #93c5fd;padding:26px 18px;box-sizing:border-box}
.c02-vung b{display:block;height:12px;margin:0 0 22px;border-radius:6px;background:#e2e8f0}
.c02-dai{position:absolute;display:block}
.c02-dai.ngang{background:rgba(37,99,235,.28)}
.c02-dai.doc{background:rgba(220,38,38,.28)}
.c02-nhan{font-size:44px!important;font-weight:900;line-height:1.15;border-left:12px solid var(--mau-chinh)}
.c02-nhan b{display:block;font-size:62px;color:var(--mau-chinh)}
.c02-the{padding:34px 40px;font-size:46px;font-weight:800;line-height:1.45;color:#1e293b}
.c02-the b{color:var(--mau-chinh)}
.c02-the small{display:block;font-size:30px;font-weight:700;color:#64748b}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Lề trái —<br>*rộng nhất trang*`, { tren: 262, co: 84, gach: "c0k1w3" })}
  ${toA4({ trai: "c0k1" }, 600)}
  ${nhan("Lề trái<b>30–35 mm</b>", "c0k2", 700)}
  ${nhan("Đang để<b>bao nhiêu?</b>", "c1k0w2", 960)}`,
      ve: veCC,
    },

    "can-le": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${toA4({ trai: "s-1", tren: "c0k0w1", duoi: "c0k0w4", phai: "c1k0w1" }, 360)}
  ${nhan("Trên, dưới<b>20–25 mm</b>", "c0k1", 420)}
  ${nhan("Lề phải<b>15–20 mm</b>", "c1k1", 700)}`,
    },

    "giay": {
      khai: true,
      html: () => `
  ${chipKhai("📄 Khổ giấy · phông chữ")}
  ${the(`<div class="c02-the">📄 Khổ <b>A4</b> · 210 × 297 mm<br>↕️ Trình bày theo <b>chiều dài</b><br>🔤 Times New Roman · <b>màu đen</b></div>`, { tren: 360, hien: "c0k0" })}
  ${the(`<div class="c02-the"><small>Đặt lề bằng cm (ví dụ)</small>Trên <b>2</b> · Dưới <b>2</b><br>Trái <b>3</b> · Phải <b>1,5</b></div>`, { tren: 790, hien: "c1k1" })}`,
    },

    "app": {
      khai: true,
      html: () => `
  ${chipKhai("🤖 Phần mềm soạn văn bản")}
  ${theApp("c0k0")}`,
    },
  },

  mocThu: (tatCa) => [
    ["01a-0.4s", 0.4],
    ["01b-trai", lucNode(tatCa, "moc", "c0k2w5")],
    ["01c-hoi", lucNode(tatCa, "moc", "c1k0w5")],
    ["02a-tren", lucNode(tatCa, "can-le", "c0k1w5")],
    ["02b-phai", lucNode(tatCa, "can-le", "c1k1w5")],
    ["03a-cm", lucNode(tatCa, "giay", "c1k4w2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2w5"),
};
