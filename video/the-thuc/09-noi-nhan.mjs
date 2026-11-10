// ============================================================================
// THỂ THỨC 09 — NƠI NHẬN (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục II.9 điểm d — phần ô số 9b)
//
// Khuôn HỎI – ĐÁP: móc «Nơi nhận: nghiêng hay đậm?» → cả hai → dòng «Nơi nhận:» một dòng riêng, sát lề trái, dấu hai
// chấm, chữ thường cỡ 12 nghiêng đậm → danh sách chữ đứng cỡ 11, gạch đầu dòng, chấm phẩy, dòng cuối «Lưu» → app →
// Tóm lại → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 105):
//   «Phần nơi nhận tại ô số 9b (áp dụng chung đối với các loại văn bản): Từ “Nơi nhận” được trình bày trên một dòng riêng
//   (ngang hàng với dòng chữ “quyền hạn, chức vụ của người ký” và sát lề trái), sau có dấu hai chấm (:), bằng chữ in thường,
//   cỡ chữ 12, kiểu chữ nghiêng, đậm; phần liệt kê các cơ quan, tổ chức, đơn vị và cá nhân nhận văn bản được trình bày bằng
//   chữ in thường, cỡ chữ 11, kiểu chữ đứng; tên mỗi cơ quan, tổ chức, đơn vị và cá nhân hoặc mỗi nhóm cơ quan, tổ chức,
//   đơn vị nhận văn bản được trình bày trên một dòng riêng, đầu dòng có gạch đầu dòng (-) sát lề trái, cuối dòng có dấu chấm
//   phẩy (;), dòng cuối cùng bao gồm chữ “Lưu” sau có dấu hai chấm (:), tiếp theo là chữ viết tắt “VT”, dấu phẩy (,), chữ
//   viết tắt tên đơn vị (hoặc bộ phận) soạn thảo văn bản và số lượng bản lưu, cuối cùng là dấu chấm (.).»
//   ✔ móc «cả hai, nghiêng và đậm» = «kiểu chữ nghiêng, đậm»
//   ✔ «từ Nơi nhận nằm một dòng riêng, sát lề trái, sau có dấu hai chấm» · ✔ «chữ in thường, cỡ mười hai, kiểu chữ nghiêng, đậm»
//   ✔ «danh sách bên dưới, chữ đứng, cỡ mười một, mỗi nơi một dòng» (tên mỗi nơi nhận / mỗi nhóm trên một dòng riêng)
//   ✔ «đầu dòng có gạch đầu dòng, cuối dòng có dấu chấm phẩy, dòng cuối là chữ Lưu»
//   ✔ dòng «- Lưu: VT, VP (2).» theo cách ghi của chính Nghị định («- Lưu: VT, HC (2).», phần ký cuối Nghị định).
//   ✔ «QUYỀN HẠN, CHỨC VỤ CỦA NGƯỜI KÝ» chỉ là chữ giữ chỗ như mẫu của Nghị định — không tên người, không tên cơ quan thật.
//   ✔ cảnh app: như clip 01 (câu cố định). KHÔNG nói phần mềm tự trình bày đúng nơi nhận (chưa soát mã).
//   Khác KE-HOACH: không — bảng ghi «Nơi nhận: chữ nghiêng đậm cỡ 12, danh sách cỡ 11», khớp nguyên văn. Ô 9a («Kính gửi»)
//   không nói trong clip này (để dành clip khác).
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, theApp, veCC } from "./loat.mjs";

/** cuối trang văn bản: cột trái Nơi nhận, cột phải chữ ký (giữ chỗ). `lop` = mốc bật các lớp tô sáng */
const cuoiTrang = (m = {}) => {
  const bat = (lop, moc) => (moc ? ` data-cc="lop" data-lop="${lop}" data-moc="${moc}"` : "");
  return `<div class="c09-tr"${bat("hl-le", m.le)}><div${bat("hl-g", m.g)}><div${bat("hl-c", m.c)}><div${bat("hl-l", m.l)}><div${bat("hl-nn", m.nn)}><div${bat("hl-hc", m.hc)}>
    <i class="le"></i>
    <div class="trai"><div class="nn">Nơi nhận<span class="hc">:</span></div>
      <div class="ds"><span class="g">-</span> Như trên<span class="c">;</span></div>
      <div class="ds"><span class="g">-</span> Lãnh đạo cơ quan<span class="c">;</span></div>
      <div class="ds"><span class="g">-</span> Các phòng chuyên môn<span class="c">;</span></div>
      <div class="ds luu"><span class="g">-</span> Lưu: VT, VP (2).</div></div>
    <div class="phai"><b>QUYỀN HẠN, CHỨC VỤ<br>CỦA NGƯỜI KÝ</b><small>(Chữ ký, dấu)</small><b>Họ và tên</b></div>
  </div></div></div></div></div></div>`;
};

export default {
  ten: "09-noi-nhan",
  tieuDe: "Thể thức 09 · «Nơi nhận»: in nghiêng hay in đậm? Trình bày nơi nhận theo NĐ 30/2020",
  bia: { dong: ["Nơi nhận:", "nghiêng hay đậm?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Từ “Nơi nhận” được trình bày trên một dòng riêng (…), sau có dấu hai chấm (:), bằng chữ in thường, *cỡ chữ 12, kiểu chữ nghiêng, đậm*; phần liệt kê (…) được trình bày bằng chữ in thường, *cỡ chữ 11, kiểu chữ đứng*…»",
      theNguon("Phụ lục I · mục II.9 · điểm d"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Chữ Nơi nhận cuối văn bản, in nghiêng, hay in đậm?", chu: "«Nơi nhận» — *in nghiêng hay in đậm?*" },
      { loi: "Câu trả lời là cả hai, nghiêng và đậm.", chu: "Cả hai — *nghiêng và đậm*" },
    ]},
    { canh: "dong", cau: [
      { loi: "Nghị định ba mươi quy định, từ Nơi nhận nằm một dòng riêng, sát lề trái, sau có dấu hai chấm.", chu: "«Nơi nhận» một dòng riêng · *sát lề trái* · sau có *dấu hai chấm*" },
      { loi: "Chữ in thường, cỡ mười hai, kiểu chữ nghiêng, đậm.", chu: "Chữ in thường · cỡ *12* · *nghiêng, đậm*", nghiSau: 0.3 },
    ]},
    { canh: "danh-sach", cau: [
      { loi: "Danh sách bên dưới, chữ đứng, cỡ mười một, mỗi nơi một dòng.", chu: "Danh sách: *chữ đứng* · cỡ *11* · mỗi nơi một dòng" },
      { loi: "Đầu dòng có gạch đầu dòng, cuối dòng có dấu chấm phẩy, dòng cuối là chữ Lưu.", chu: "Đầu dòng *( - )* · cuối dòng *( ; )* · dòng cuối *«Lưu»*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["Nơi nhận nghiêng đậm cỡ mười hai", "Nơi nhận: 12 · nghiêng đậm"],
      ["danh sách chữ đứng cỡ mười một", "Danh sách: đứng · cỡ 11"],
      ["dòng cuối là chữ Lưu", "Dòng cuối: «Lưu: VT, …»"],
    ]),
    canhKet([
      { loi: "Từ Nơi nhận được trình bày trên một dòng riêng, sau có dấu hai chấm, bằng chữ in thường, cỡ chữ mười hai, kiểu chữ nghiêng, đậm.", chu: "Từ “Nơi nhận” được trình bày trên một dòng riêng (…), sau có dấu hai chấm (:), bằng chữ in thường, cỡ chữ 12, kiểu chữ nghiêng, đậm" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.9 · điểm d"),
    ]),
  ],

  css: CSS_CC + `
.c09-mau{padding:46px 10px 54px;text-align:center;font-family:"Times New Roman",serif;font-size:110px;color:#111;white-space:nowrap}
.c09-mau .a{font-style:italic}
.c09-mau .b{font-weight:700}
.c09-mau .ok{font-style:italic;font-weight:700;color:#15803d}
.c09-mau small{display:block;margin-top:8px;font-family:system-ui,sans-serif;font-size:34px;font-weight:800;color:#64748b;font-style:normal}
.c09-tr{position:relative;padding:40px 24px 46px 70px;font-family:"Times New Roman",serif;color:#111}
.c09-tr .le{position:absolute;left:48px;top:20px;bottom:20px;border-left:4px dashed #cbd5e1}
.c09-tr.hl-le .le{border-left-color:#f59e0b}
.c09-tr .trai{display:inline-block;vertical-align:top;width:500px}
.c09-tr .phai{display:inline-block;vertical-align:top;width:330px;text-align:center;font-size:25px;line-height:1.3;white-space:nowrap}
.c09-tr .phai b{display:block}
.c09-tr .phai small{display:block;margin:34px 0 34px;font-size:26px;font-style:italic;color:#64748b}
.c09-tr .nn{font-size:48px;font-style:italic;font-weight:700;margin-bottom:10px;border-radius:8px}
.c09-tr .ds{font-size:41px;line-height:1.4;white-space:nowrap;border-radius:8px}
.c09-tr .g,.c09-tr .c,.c09-tr .hc{border-radius:4px}
.hl-nn .nn{background:#fef3c7;box-shadow:0 0 0 4px #f59e0b}
.hl-hc .hc{background:#f59e0b;color:#fff}
.hl-g .g{background:#f59e0b;color:#fff;padding:0 6px}
.hl-c .c{background:#f59e0b;color:#fff;padding:0 4px}
.hl-l .luu{background:#fef3c7;box-shadow:0 0 0 4px #f59e0b}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + «Nơi nhận:» đổi kiểu theo lời: thường → nghiêng → đậm → nghiêng + đậm (dấu xanh)
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}«Nơi nhận» —<br>*nghiêng hay đậm?*`, { tren: 262, co: 84, gach: "c0k2w2" })}
  ${the(`<div class="c09-mau"><span data-cc="lop" data-lop="a" data-moc="c0k1w0" data-an="c0k2w1"><span data-cc="lop" data-lop="b" data-moc="c0k2w1" data-an="c1k1w0"><span data-cc="lop" data-lop="ok" data-moc="c1k1w0">Nơi nhận:</span></span></span></div>`, { tren: 720 })}
  ${dau("✓ NGHIÊNG + ĐẬM", "c1k1w2", { tren: 1020, co: 84, lop: "xanh" })}`,
      ve: veCC,
    },

    // dòng «Nơi nhận:»: một dòng riêng · sát lề trái · dấu hai chấm · chữ thường cỡ 12 nghiêng đậm
    "dong": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${the(cuoiTrang({ nn: "c0k1w2", le: "c0k2", hc: "c0k3w3" }), { tren: 380, hien: "s-2" })}
  ${goiKhai("<span>Một dòng riêng · <b>sát lề trái</b></span>", { trai: 60, tren: 820, hien: "c0k2w1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Sau «Nơi nhận» có <b>dấu hai chấm ( : )</b></span>", { trai: 60, tren: 960, hien: "c0k3w3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Chữ thường · cỡ <b>12</b> · <b><i>nghiêng, đậm</i></b></span>", { trai: 60, tren: 1100, hien: "c1k2w2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // danh sách: chữ đứng cỡ 11 · gạch đầu dòng · chấm phẩy · dòng cuối «Lưu»
    "danh-sach": {
      khai: true,
      html: () => `
  ${chipKhai("📋 Danh sách nơi nhận")}
  ${the(cuoiTrang({ g: "c1k0w3", c: "c1k1w4", l: "c1k2w3" }), { tren: 380, hien: "s-2" })}
  ${goiKhai("<span>Danh sách: <b>chữ đứng</b> · cỡ <b>11</b> · mỗi nơi một dòng</span>", { trai: 60, tren: 820, hien: "c0k2w1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Đầu dòng <b>( - )</b> · cuối dòng <b>( ; )</b></span>", { trai: 60, tren: 960, hien: "c1k1w4", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Dòng cuối: <b>Lưu: VT, …</b> kết thúc bằng <b>( . )</b></span>", { trai: 60, tren: 1100, hien: "c1k2w3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-nghieng", lucNode(tatCa, "moc", "c0k1w1+0.2")],
    ["01c-dam", lucNode(tatCa, "moc", "c0k2w2+0.2")],
    ["01d-ca-hai", lucNode(tatCa, "moc", "c1k1w2+0.4")],
    ["02a-le", lucNode(tatCa, "dong", "c0k2w2")],
    ["02b-hai-cham", lucNode(tatCa, "dong", "c0k3w4")],
    ["03a-gach", lucNode(tatCa, "danh-sach", "c1k0w5")],
    ["03b-luu", lucNode(tatCa, "danh-sach", "c1k2w4+0.3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1w2+0.4"),
};
