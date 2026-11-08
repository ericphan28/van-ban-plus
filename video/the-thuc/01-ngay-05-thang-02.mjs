// ============================================================================
// THỂ THỨC 01 — NGÀY 5 HAY NGÀY 05? (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục II.4 điểm b)
//
// Khuôn ĐẬP HIỂU LẦM: móc hỏi cách ghi «ngày 5 tháng 3» → quy định ngày < 10 thêm 0, tháng chỉ 1 và 2 thêm 0 → hai ví dụ
// ghi đúng → giới thiệu phần mềm (chỉ trên hình tên VanBanPlus) → Tóm lại → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 52):
//   «Thời gian ban hành văn bản phải được viết đầy đủ; các số thể hiện ngày, tháng, năm dùng chữ số Ả Rập; đối với những số
//   thể hiện ngày nhỏ hơn 10 và tháng 1, 2 phải ghi thêm số 0 phía trước.»
//   ✔ «ngày nhỏ hơn mười, phải thêm số không phía trước» · ✔ «chỉ tháng một và tháng hai mới thêm số không»
//   ✔ ví dụ «ngày 05 tháng 3» (tháng 3 KHÔNG thêm 0) · «ngày 15 tháng 02»
//   ✔ dòng địa danh nghiêng, sau địa danh có dấu phẩy — điểm c cùng mục (chỉ trên hình).
//   ✔ cảnh app: AI soạn / kiểm tra chính tả, thể thức / xuất Word NĐ 30 — PROJECT_STATUS.md mục A; «tải miễn phí cho
//     Windows» — trang vanbanplus.giakiemso.com (08/11/2026). KHÔNG nói phần mềm tự ghi ngày đúng (chưa soát).
//   ✘ KHÔNG nói: tên cơ quan thật, «bắt buộc dùng phần mềm», «tốt nhất».
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai, lopKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, lich12, theApp, veCC } from "./loat.mjs";

const THANG = Array.from({ length: 12 }, (_, i) => ({
  t: "Tháng", gt: i < 2 ? `0${i + 1}` : String(i + 1), lop: i < 2 ? "vang" : "",
}));

export default {
  ten: "01-ngay-05-thang-02",
  tieuDe: "Thể thức 01 · Ngày 5 hay ngày 05? Quy định ghi ngày tháng theo NĐ 30/2020",
  bia: { dong: ["Ngày 5 hay", "ngày 05?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«…đối với những số thể hiện *ngày nhỏ hơn 10 và tháng 1, 2 phải ghi thêm số 0* phía trước.»",
      theNguon("Phụ lục I · mục II.4 · điểm b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản hành chính, ngày năm tháng ba, anh chị ghi thế nào?", chu: "Ngày 5 tháng 3 — *ghi thế nào cho đúng?*" },
      { loi: "Ghi ngày năm, là thiếu một số không.", chu: "Ghi «ngày 5» là *thiếu số 0*" },
    ]},
    { canh: "quy-dinh", cau: [
      { loi: "Nghị định ba mươi quy định, ngày nhỏ hơn mười, phải ghi thêm số không phía trước.", chu: "Ngày *nhỏ hơn 10* → thêm *số 0* phía trước" },
      { loi: "Còn tháng, chỉ tháng một và tháng hai, mới thêm số không.", chu: "Tháng: *chỉ tháng 1, 2* thêm số 0", nghiSau: 0.3 },
    ]},
    { canh: "vi-du", cau: [
      { loi: "Ngày năm tháng ba, ghi là ngày không năm, tháng ba.", chu: "ngày *05* tháng 3" },
      { loi: "Ngày mười lăm tháng hai, ghi là ngày mười lăm, tháng không hai.", chu: "ngày 15 tháng *02*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["ngày dưới mười thêm số không", "Ngày < 10 → thêm 0"],
      ["chỉ tháng một và tháng hai thêm số không", "Chỉ tháng 1, 2 thêm 0"],
      ["tháng ba trở đi giữ nguyên", "Tháng 3–12 giữ nguyên"],
    ]),
    canhKet([
      { loi: "Đối với những số thể hiện ngày nhỏ hơn mười, và tháng một, tháng hai, phải ghi thêm số không phía trước.", chu: "…ngày nhỏ hơn 10 và tháng 1, 2 phải ghi thêm số 0 phía trước." },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.4 · điểm b"),
    ]),
  ],

  css: CSS_CC + `
.c01-dong{padding:34px 10px;text-align:center;font-family:"Times New Roman",serif;font-style:italic;font-size:50px;white-space:nowrap;color:#111}
.c01-dong b{font-style:italic;color:#15803d;background:#dcfce7;padding:0 8px;border-radius:8px}
.c01-dong b.sai{color:#b91c1c;background:#fee2e2}
.c01-lon{position:absolute;left:0;width:100%;text-align:center;font-weight:900;font-size:190px;letter-spacing:-.02em;color:#0f172a}
.c01-lon b{color:var(--mau-chinh)}
.c01-lon s{color:#94a3b8;text-decoration-thickness:12px;text-decoration-color:#dc2626}
.c01-lich{padding:26px}
.c01-lich .o{height:104px}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + dòng ngày SAI «ngày 5» · câu 2: dấu «THIẾU SỐ 0»
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Ngày 5 tháng 3 —<br>*ghi sao cho đúng?*`, { tren: 262, co: 84, gach: "c0k2w2" })}
  ${the(`<div class="c01-dong">Đồng Nai, ngày <b class="sai">5</b> tháng 3 năm 2026</div>`, { tren: 720, rung: "c1k0w2" })}
  ${dau("THIẾU SỐ 0", "c1k1w1", { tren: 960, co: 96 })}`,
      ve: veCC,
    },

    // quy định: «5 → 05» rồi lưới 12 tháng, tô tháng 01 02
    "quy-dinh": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${lopKhai(`<div class="c01-lon"><s>5</s> → <b>05</b></div>`, "c0k1", "c1", "left:0;top:420px;width:100%;height:260px")}
  ${goiKhai("<span>Ngày <b>nhỏ hơn 10</b> → thêm số 0</span>", { trai: 60, tren: 760, hien: "c0k1+0.5", an: "c1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${the(lich12(THANG), { tren: 360, hien: "c1", lop: "c01-lich cc-lich-the" })}
  ${goiKhai("<span>Chỉ <b>tháng 1 và tháng 2</b> thêm số 0</span>", { trai: 60, tren: 1010, hien: "c1k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // hai dòng ghi đúng
    "vi-du": {
      khai: true,
      html: () => `
  ${chipKhai("✓ Ghi đúng")}
  ${the(`<div class="c01-dong">Đồng Nai, ngày <b>05</b> tháng 3 năm 2026</div>`, { tren: 420, hien: "c0k0w2" })}
  ${goiKhai("<span>Tháng 3 <b>không</b> thêm số 0</span>", { trai: 60, tren: 600, hien: "c0k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${the(`<div class="c01-dong">Đồng Nai, ngày 15 tháng <b>02</b> năm 2026</div>`, { tren: 800, hien: "c1" })}
  ${goiKhai("<span>Ngày 15 giữ nguyên · tháng 2 → <b>02</b></span>", { trai: 60, tren: 980, hien: "c1k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // giới thiệu phần mềm (tên chỉ trên hình)
    "app": {
      khai: true,
      html: () => `
  ${chipKhai("🤖 Phần mềm soạn văn bản")}
  ${theApp("c0k0")}`,
    },
  },

  mocThu: (tatCa) => [
    ["01a-0.4s", 0.4],
    ["01b-thieu", lucNode(tatCa, "moc", "c1k1w3")],
    ["02a-05", lucNode(tatCa, "quy-dinh", "c0k2w3")],
    ["02b-thang", lucNode(tatCa, "quy-dinh", "c1k1w5")],
    ["03a-dung", lucNode(tatCa, "vi-du", "c1k1w4")],
    ["04a-app", lucNode(tatCa, "app", "c0k2w2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1w3"),
};
