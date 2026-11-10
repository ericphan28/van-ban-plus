// ============================================================================
// THỂ THỨC 10 — VIỆN DẪN VĂN BẢN (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục II.6 điểm b)
//
// Khuôn ĐẬP HIỂU LẦM: móc «Thực hiện Nghị định số 30/2020/NĐ-CP, …» lần đầu chỉ ghi số, ký hiệu → chưa đủ → lần đầu ghi đủ
// tên loại, số, ký hiệu, thời gian, cơ quan ban hành, trích yếu (ghép dần từng mảnh) → lần sau chỉ ghi tên loại và số, ký
// hiệu → app → Tóm lại → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 63):
//   «b) Khi viện dẫn lần đầu văn bản có liên quan, phải ghi đầy đủ tên loại, số, ký hiệu của văn bản, thời gian ban hành văn
//   bản, tên cơ quan, tổ chức ban hành văn bản và trích yếu nội dung văn bản (đối với Luật và Pháp lệnh chỉ ghi tên loại và
//   tên của Luật, Pháp lệnh); trong các lần viện dẫn tiếp theo, chỉ ghi tên loại và số, ký hiệu của văn bản đó.»
//   ✔ móc «lần đầu chỉ ghi số và ký hiệu là chưa đủ» (lần đầu phải ghi đầy đủ)
//   ✔ «lần đầu viện dẫn, phải ghi đủ tên loại, số và ký hiệu» · ✔ «thêm thời gian ban hành, cơ quan ban hành, và trích yếu nội dung»
//   ✔ «từ lần viện dẫn tiếp theo, chỉ ghi tên loại, số và ký hiệu»
//   ✔ ví dụ chính Nghị định 30: «Nghị định số 30/2020/NĐ-CP ngày 05 tháng 3 năm 2020 của Chính phủ về công tác văn thư»
//     (README / noi-dung.md: ban hành ngày 05 tháng 3 năm 2020, của Chính phủ, tên đầy đủ «Nghị định về công tác văn thư»).
//   ✔ ngoặc «Luật, Pháp lệnh chỉ ghi tên loại và tên» chỉ ở thẻ màn kết / mô tả — không đọc (giữ clip ≤ 52 s).
//   ✔ cảnh app: như clip 01 (câu cố định). KHÔNG nói phần mềm tự viện dẫn đúng (chưa soát mã).
//   Khác KE-HOACH: không — bảng ghi «Viện dẫn lần đầu phải ghi đủ tên loại, số, ngày, cơ quan, trích yếu», khớp nguyên văn.
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, theApp, veCC } from "./loat.mjs";

/** một mảnh của câu viện dẫn: chữ + nhãn nhỏ bên dưới, hiện lúc `hien` */
const manh = (chu, nhan, hien, lop = "") =>
  `<span class="m ${lop}" data-k="lop" data-hien="${hien}" style="opacity:0">${chu}<small>${nhan}</small></span>`;

export default {
  ten: "10-vien-dan",
  tieuDe: "Thể thức 10 · Viện dẫn văn bản lần đầu: ghi số, ký hiệu đã đủ chưa? Theo NĐ 30/2020",
  bia: { dong: ["Viện dẫn lần đầu:", "ghi số là đủ?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Khi viện dẫn lần đầu văn bản có liên quan, phải ghi đầy đủ tên loại, số, ký hiệu của văn bản, thời gian ban hành văn bản, tên cơ quan, tổ chức ban hành văn bản và trích yếu nội dung văn bản (…); *trong các lần viện dẫn tiếp theo, chỉ ghi tên loại và số, ký hiệu của văn bản đó.*»",
      theNguon("Phụ lục I · mục II.6 · điểm b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Viện dẫn văn bản khác lần đầu, chỉ ghi số và ký hiệu, đã đủ chưa?", chu: "Viện dẫn lần đầu — *chỉ ghi số, ký hiệu đã đủ?*" },
      { loi: "Lần đầu mà ghi thế này, là chưa đủ.", chu: "Lần đầu ghi vậy — *chưa đủ*" },
    ]},
    { canh: "lan-dau", cau: [
      { loi: "Nghị định ba mươi quy định, lần đầu viện dẫn, phải ghi đủ tên loại, số và ký hiệu.", chu: "Lần đầu: ghi đủ *tên loại* · *số, ký hiệu*" },
      { loi: "Thêm thời gian ban hành, cơ quan ban hành, và trích yếu nội dung.", chu: "+ *thời gian ban hành* · *cơ quan ban hành* · *trích yếu*", nghiSau: 0.4 },
    ]},
    { canh: "lan-sau", cau: [
      { loi: "Từ lần viện dẫn tiếp theo, chỉ ghi tên loại, số và ký hiệu.", chu: "Lần sau: chỉ ghi *tên loại* · *số, ký hiệu*", nghiSau: 0.4 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["lần đầu ghi đầy đủ", "Lần đầu: ghi đầy đủ"],
      ["có ngày ban hành cơ quan và trích yếu", "Ngày · cơ quan · trích yếu"],
      ["lần sau chỉ ghi tên loại và số ký hiệu", "Lần sau: loại + số, ký hiệu"],
    ]),
    canhKet([
      { loi: "Trong các lần viện dẫn tiếp theo, chỉ ghi tên loại, và số, ký hiệu của văn bản đó.", chu: "…trong các lần viện dẫn tiếp theo, chỉ ghi tên loại và số, ký hiệu của văn bản đó" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.6 · điểm b"),
    ]),
  ],

  css: CSS_CC + `
.c10-cau{padding:40px 40px 46px;font-family:"Times New Roman",serif;font-size:50px;line-height:1.5;color:#111}
.c10-cau .vd{color:#b91c1c;background:#fee2e2;border-radius:8px;padding:0 4px}
.c10-ghep{padding:36px 34px 40px;display:flex;flex-wrap:wrap;gap:22px 16px;font-family:"Times New Roman",serif;font-size:50px;color:#111}
.c10-ghep .m{position:relative;display:inline-flex;flex-direction:column;align-items:center;padding:6px 14px 4px;border-radius:14px;background:#eff6ff;box-shadow:inset 0 0 0 3px #bfdbfe;white-space:nowrap}
.c10-ghep .m small{font-family:system-ui,sans-serif;font-size:24px;font-weight:800;color:#2563eb;margin-top:2px}
.c10-ghep .m.moi{background:#dcfce7;box-shadow:inset 0 0 0 3px #86efac}
.c10-ghep .m.moi small{color:#15803d}
.c10-sau{padding:34px 30px 40px;text-align:center;font-family:"Times New Roman",serif;color:#111}
.c10-sau .nhan{font-family:system-ui,sans-serif;font-size:30px;font-weight:800;color:#64748b;margin-bottom:10px}
.c10-sau .cau{font-size:56px}
.c10-sau .cau b{font-weight:400;color:#15803d;background:#dcfce7;border-radius:10px;padding:0 8px}
.c10-sau .mo{font-size:44px;color:#94a3b8;margin-top:14px}
.c10-sau .mo s{text-decoration-thickness:3px;text-decoration-color:#dc2626}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + câu «Thực hiện Nghị định số 30/2020/NĐ-CP, …» (lần đầu, thiếu) · câu 2: rung + dấu
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Viện dẫn lần đầu —<br>*chỉ ghi số đã đủ?*`, { tren: 262, co: 84, gach: "c0k2w2" })}
  ${the(`<div class="c10-cau">Thực hiện <span class="vd">Nghị định số 30/2020/NĐ-CP</span>, cơ quan triển khai …</div>`, { tren: 720, rung: "c1k0w3" })}
  ${dau("✗ CHƯA ĐỦ", "c1k1w1", { tren: 1010, co: 92 })}`,
      ve: veCC,
    },

    // lần đầu: ghép dần từng mảnh câu viện dẫn đầy đủ
    "lan-dau": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${the(`<div class="c10-ghep">
    ${manh("Nghị định", "tên loại", "c0k2w3")}
    ${manh("số 30/2020/NĐ-CP", "số, ký hiệu", "c0k3w0")}
    ${manh("ngày 05 tháng 3 năm 2020", "thời gian ban hành", "c1k0w1", "moi")}
    ${manh("của Chính phủ", "cơ quan ban hành", "c1k1w0", "moi")}
    ${manh("về công tác văn thư", "trích yếu", "c1k2w1", "moi")}
  </div>`, { tren: 400, hien: "s-2" })}
  ${goiKhai("<span>Lần đầu viện dẫn: <b>ghi đầy đủ</b></span>", { trai: 60, tren: 1010, hien: "c1k2w4", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // lần sau: chỉ tên loại + số, ký hiệu
    "lan-sau": {
      khai: true,
      html: () => `
  ${chipKhai("🔁 Từ lần viện dẫn thứ hai")}
  ${the(`<div class="c10-sau"><div class="nhan">LẦN ĐẦU</div><div class="mo">Nghị định số 30/2020/NĐ-CP <s>ngày 05 tháng 3 năm 2020 của Chính phủ về công tác văn thư</s></div></div>`, { tren: 400, hien: "s-2" })}
  ${the(`<div class="c10-sau"><div class="nhan">LẦN TIẾP THEO</div><div class="cau"><b>Nghị định</b> <b>số 30/2020/NĐ-CP</b></div></div>`, { tren: 720, hien: "c0k1w1" })}
  ${goiKhai("<span>Lần sau: chỉ <b>tên loại</b> + <b>số, ký hiệu</b></span>", { trai: 60, tren: 960, hien: "c0k2w1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-dau", lucNode(tatCa, "moc", "c1k1w2+0.3")],
    ["02a-so", lucNode(tatCa, "lan-dau", "c0k3w2")],
    ["02b-ngay", lucNode(tatCa, "lan-dau", "c1k0w4")],
    ["02c-du", lucNode(tatCa, "lan-dau", "c1k2w4+0.3")],
    ["03a-sau", lucNode(tatCa, "lan-sau", "c0k2w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1w2+0.3"),
};
