// ============================================================================
// THỂ THỨC 07 — SỐ, KÝ HIỆU CỦA VĂN BẢN (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục II.3 điểm a, c)
//
// Khuôn ĐẬP HIỂU LẦM: móc «SỐ 5/QĐ-UBND» (3 chỗ chưa đúng) → sửa từ «Số»: chữ thường, dấu hai chấm, số < 10 thêm 0 →
// ký hiệu in hoa, gạch chéo giữa số và ký hiệu, gạch nối không cách chữ → app → Tóm lại → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 40, 44):
//   «a) Số của văn bản là số thứ tự văn bản do cơ quan, tổ chức ban hành trong một năm được đăng ký tại Văn thư cơ quan theo
//   quy định. Số của văn bản được ghi bằng chữ số Ả Rập.»
//   «c) Số, ký hiệu của văn bản được đặt canh giữa dưới tên cơ quan, tổ chức ban hành văn bản. Từ “Số” được trình bày bằng
//   chữ in thường, cỡ chữ 13, kiểu chữ đứng; sau từ “Số” có dấu hai chấm (:); với những số nhỏ hơn 10 phải ghi thêm số 0
//   phía trước. Ký hiệu của văn bản được trình bày bằng chữ in hoa, cỡ chữ 13, kiểu chữ đứng. Giữa số và ký hiệu văn bản có
//   dấu gạch chéo (/), giữa các nhóm chữ viết tắt trong ký hiệu văn bản có dấu gạch nối (-), không cách chữ.»
//   ✔ «từ Số viết chữ in thường, cỡ chữ mười ba» · ✔ «sau từ Số có dấu hai chấm» · ✔ «số nhỏ hơn mười thêm số không phía trước»
//   ✔ «ký hiệu chữ in hoa» · ✔ «giữa số và ký hiệu có gạch chéo» · ✔ «giữa các nhóm chữ viết tắt có gạch nối, không cách chữ»
//   ✔ móc «SỐ 5/QĐ-UBND»: 3 chỗ chưa đúng = «SỐ» in hoa · thiếu dấu hai chấm · 5 thiếu số 0 (đúng: «Số: 05/QĐ-UBND»).
//   ✔ ví dụ ký hiệu QĐ-UBND = tên loại (Quyết định) + cơ quan (Ủy ban nhân dân) — điểm b; không tên cơ quan thật.
//   ✔ cảnh app: như clip 01 (câu cố định). KHÔNG nói phần mềm tự ghi số, ký hiệu đúng (chưa soát mã).
//   Khác KE-HOACH: không — bảng ghi «Từ “Số” chữ thường, sau có dấu hai chấm; số dưới 10 ghi 0», khớp nguyên văn.
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, theApp, veCC } from "./loat.mjs";

export default {
  ten: "07-so-ky-hieu",
  tieuDe: "Thể thức 07 · Số văn bản: 5 hay 05? «Số:» viết thế nào cho đúng NĐ 30/2020",
  bia: { dong: ["Số văn bản:", "5 hay 05?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Từ “Số” được trình bày bằng chữ in thường, cỡ chữ 13, kiểu chữ đứng; *sau từ “Số” có dấu hai chấm (:); với những số nhỏ hơn 10 phải ghi thêm số 0 phía trước.*»",
      theNguon("Phụ lục I · mục II.3 · điểm c"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Số văn bản dưới mười, ghi số năm, hay không năm?", chu: "Số văn bản dưới 10 — *5 hay 05?*" },
      { loi: "Dòng này có ba chỗ, chưa đúng.", chu: "Dòng này có *3 chỗ chưa đúng*" },
    ]},
    { canh: "so", cau: [
      { loi: "Nghị định ba mươi quy định, từ Số viết chữ in thường, cỡ chữ mười ba.", chu: "Từ «Số»: *chữ in thường* · cỡ 13" },
      { loi: "Sau từ Số có dấu hai chấm, số nhỏ hơn mười, phải ghi thêm số không phía trước.", chu: "Sau «Số» có *dấu hai chấm* · số dưới 10 → *thêm 0*", nghiSau: 0.3 },
    ]},
    { canh: "ky-hieu", cau: [
      { loi: "Ký hiệu viết chữ in hoa, giữa số và ký hiệu có gạch chéo.", chu: "Ký hiệu: *IN HOA* · giữa số và ký hiệu: *gạch chéo /*" },
      { loi: "Giữa các nhóm chữ viết tắt có gạch nối, không cách chữ.", chu: "Nhóm chữ viết tắt: *gạch nối, không cách chữ*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["chữ Số viết thường có dấu hai chấm", "«Số:» chữ thường"],
      ["số dưới mười thêm số không", "Số dưới 10 → 05"],
      ["ký hiệu in hoa không cách chữ", "Ký hiệu HOA · liền"],
    ]),
    canhKet([
      { loi: "Sau từ Số có dấu hai chấm, với những số nhỏ hơn mười, phải ghi thêm số không phía trước.", chu: "…sau từ “Số” có dấu hai chấm (:); với những số nhỏ hơn 10 phải ghi thêm số 0 phía trước" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.3 · điểm c"),
    ]),
  ],

  css: CSS_CC + `
.c07-so{padding:44px 10px 52px;text-align:center;font-family:"Times New Roman",serif;font-size:84px;white-space:nowrap;color:#111}
.c07-so .sai{color:#b91c1c;background:#fee2e2;border-radius:8px;padding:0 4px}
.c07-so .moi{color:#15803d;background:#dcfce7;border-radius:8px;padding:0 4px}
.c07-so .b,.c07-so .c,.c07-so .d{display:none}
.c07-so.b1 .a1{display:none}.c07-so.b1 .b{display:inline}
.c07-so .b2.on .c{display:inline}
.c07-so .b3.on .d1{display:none}.c07-so .b3.on .d{display:inline}
.c07-so .b,.c07-so .c,.c07-so .d1,.c07-so .d{padding:0;border-radius:0}
.c07-so .b{border-radius:8px 0 0 8px}.c07-so .c{border-radius:0 8px 8px 0}
.c07-kh{padding:44px 10px 52px;text-align:center;font-family:"Times New Roman",serif;font-size:84px;white-space:nowrap;color:#111}
.c07-kh span{border-radius:4px;padding:0;transition:none}
.c07-kh .sang{background:#fef3c7;box-shadow:0 0 0 4px #f59e0b}
.c07-vd{text-align:center;font-size:30px;font-weight:700;color:#64748b;margin-top:-30px;padding-bottom:26px}
.c07-ss{display:flex;justify-content:center;gap:60px;padding:30px 20px;font-family:"Times New Roman",serif;font-size:64px;white-space:nowrap}
.c07-ss .x{color:#b91c1c}.c07-ss .v{color:#15803d}
.c07-ss small{display:block;text-align:center;font-family:system-ui,sans-serif;font-size:30px;font-weight:800}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + dòng «SỐ 5/QĐ-UBND» · câu 2: rung + dấu «3 CHỖ CHƯA ĐÚNG»
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Số văn bản dưới 10 —<br>*ghi 5 hay 05?*`, { tren: 262, co: 84, gach: "c0k1w2" })}
  ${the(`<div class="c07-so"><span class="sai">SỐ</span> <span class="sai">5</span>/QĐ-UBND</div>`, { tren: 720, rung: "c1k0w2" })}
  ${dau("✗ 3 CHỖ CHƯA ĐÚNG", "c1k1w1", { tren: 980, co: 80 })}`,
      ve: veCC,
    },

    // từ «Số»: SỐ → Số · thêm dấu hai chấm · 5 → 05 (lớp b1 / b2 / b3 bật theo mốc)
    "so": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${the(`<div class="c07-so" data-cc="lop" data-lop="b1" data-moc="c0k1w2"><span class="b2" data-cc="lop" data-lop="on" data-moc="c1k0w4"><span class="b3" data-cc="lop" data-lop="on" data-moc="c1k2w1">` +
    `<span class="a1 sai">SỐ</span><span class="b moi">Số</span><span class="c moi">:</span> <span class="d1 sai">5</span><span class="d moi">05</span>/QĐ-UBND</span></span></div>`, { tren: 420, hien: "s-2" })}
  ${goiKhai("<span>Từ «Số»: <b>chữ in thường</b> · cỡ 13</span>", { trai: 60, tren: 680, hien: "c0k1w2+0.3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Sau «Số» có <b>dấu hai chấm ( : )</b></span>", { trai: 60, tren: 820, hien: "c1k0w4+0.3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Số nhỏ hơn 10 → <b>thêm 0 phía trước</b></span>", { trai: 60, tren: 960, hien: "c1k2w1+0.3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // ký hiệu: in hoa · gạch chéo giữa số và ký hiệu · gạch nối không cách chữ
    "ky-hieu": {
      khai: true,
      html: () => `
  ${chipKhai("🔤 Ký hiệu văn bản")}
  ${the(`<div class="c07-kh">Số: 05<span data-cc="lop" data-lop="sang" data-moc="c0k1w6" data-an="c1">/</span><span data-cc="lop" data-lop="sang" data-moc="c0k0" data-an="c0k1">QĐ</span><span data-cc="lop" data-lop="sang" data-moc="c1k0w7">-</span><span data-cc="lop" data-lop="sang" data-moc="c0k0" data-an="c0k1">UBND</span></div>
    <div class="c07-vd">Ví dụ: QĐ = Quyết định · UBND = Ủy ban nhân dân</div>`, { tren: 400, hien: "s-2" })}
  ${goiKhai("<span>Ký hiệu: <b>IN HOA</b> · cỡ 13 · đứng</span>", { trai: 60, tren: 720, hien: "c0k0w3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Giữa số và ký hiệu: <b>gạch chéo ( / )</b></span>", { trai: 60, tren: 860, hien: "c0k1w6", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${the(`<div class="c07-ss"><div class="x">QĐ - UBND<small>✗ có cách</small></div><div class="v">QĐ-UBND<small>✓ không cách chữ</small></div></div>`, { tren: 1010, hien: "c1k1" })}`,
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
    ["01b-dau", lucNode(tatCa, "moc", "c1k1w1+0.6")],
    ["02a-thuong", lucNode(tatCa, "so", "c0k2w2")],
    ["02b-hai-cham", lucNode(tatCa, "so", "c1k1w1")],
    ["02c-so-0", lucNode(tatCa, "so", "c1k2w6")],
    ["03a-in-hoa", lucNode(tatCa, "ky-hieu", "c0k0w4")],
    ["03b-cheo", lucNode(tatCa, "ky-hieu", "c0k1w7")],
    ["03c-noi", lucNode(tatCa, "ky-hieu", "c1k1w2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1w1+0.6"),
};
