// ============================================================================
// THỂ THỨC 41 — «NGÀY QUỐC KHÁNH 2/9» HAY «ngày Quốc khánh 2-9»? VIẾT HOA TÊN NGÀY LỄ, NGÀY KỶ NIỆM THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 39): móc A/B + nhãn bình luận → ví dụ của Nghị định → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc2.md, mục V.5:
//   «5. Tên các ngày lễ, ngày kỷ niệm: Viết hoa chữ cái đầu của âm tiết tạo thành tên gọi ngày lễ, ngày kỷ niệm. Ví dụ: ngày
//   Quốc khánh 2-9, ngày Tổng tuyển cử đầu tiên, ngày Quốc tế Lao động 1-5, ngày Phụ nữ Việt Nam 20-10,...»
//   ✔ «ngày» viết thường, «Quốc khánh» chỉ hoa chữ Q (đúng ví dụ) · «Tổng tuyển cử»: chỉ hoa «Tổng» (đúng ví dụ)
//   ✔ gạch ngang «2-9»: chỉ nói «ví dụ của Nghị định ghi …», không khẳng định «2/9» bị cấm.
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

const dong = (chu, ghi, hien, tren, lop = "") =>
  the(`<div class="x-so ${lop}"><span class="tn">${chu}</span><span class="gh">${ghi}</span></div>`, { tren, hien, lop: "x-the" });

export default {
  ten: "41-ngay-quoc-khanh-viet-hoa",
  tieuDe: "Thể thức 41 · «Ngày Quốc Khánh 2/9» hay «ngày Quốc khánh 2-9»? Viết hoa tên ngày lễ theo NĐ 30/2020",
  bia: { dong: ["Quốc Khánh", "hay Quốc khánh?"], vai: "Nghị định 30 · Phụ lục II" },
  ket: {
    the: [
      "«Tên các ngày lễ, ngày kỷ niệm: Viết hoa chữ cái đầu của âm tiết tạo thành tên gọi ngày lễ, ngày kỷ niệm. Ví dụ: *ngày Quốc khánh 2-9*, *ngày Tổng tuyển cử đầu tiên* …»",
      theNguon("Phụ lục II · mục V.5"),
      theBinhLuan("Cơ quan anh chị viết kiểu nào? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Tên ngày lễ trong văn bản, viết bên A, hay bên B?", chu: "«Ngày Quốc Khánh» hay «ngày Quốc khánh»? — *A hay B?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "le", cau: [
      { loi: "Bên A sai hai chỗ, chữ Ngày, và chữ Khánh, không viết hoa.", chu: "A sai: *Ngày*, *Khánh* — không viết hoa" },
      { loi: "Ví dụ trong Nghị định ba mươi, chữ ngày viết thường, chỉ hoa chữ Quốc.", chu: "*ngày* viết thường · chỉ hoa chữ *Quốc*" },
      { loi: "Ngày tháng trong ví dụ, nối bằng gạch ngang, như hai gạch chín.", chu: "Ví dụ ghi *2-9* — gạch ngang" },
      { loi: "Tương tự, ngày Quốc tế Lao động, ngày Phụ nữ Việt Nam.", chu: "ngày *Q*uốc tế *L*ao động 1-5 · ngày *P*hụ nữ *V*iệt *N*am 20-10" },
      { loi: "Còn ngày Tổng tuyển cử đầu tiên, chỉ viết hoa một chữ Tổng.", chu: "ngày *T*ổng tuyển cử đầu tiên — *chỉ hoa chữ Tổng*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Tên các ngày lễ, ngày kỷ niệm, viết hoa chữ cái đầu của âm tiết, tạo thành tên gọi.", chu: "Ngày lễ: hoa *chữ cái đầu của âm tiết tạo thành tên gọi*" },
      nguonCau("Phụ lục hai", "Phụ lục II · mục V.5"),
      binhLuanCau("Cơ quan anh chị đang viết theo kiểu nào, bình luận cho mọi người cùng biết.", "Cơ quan anh chị viết kiểu nào? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;flex-direction:column;gap:26px}
.x-ab .o{display:flex;align-items:center;gap:26px;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 30px}
.x-ab .o b{flex:none;display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:0;font-family:"Times New Roman",serif;font-size:62px;color:#0f172a}
.x-ab .o em{font-style:normal;color:#dc2626;font-weight:700}
.x-ab .o small{display:block;font-family:system-ui,sans-serif;font-size:26px;font-weight:800;color:#94a3b8}
.x-the{padding:0}
.x-so{display:flex;align-items:center;justify-content:space-between;gap:20px;padding:22px 34px}
.x-so .tn{font-family:"Times New Roman",serif;font-size:54px;color:#0f172a}
.x-so .tn em{font-style:normal;font-weight:700;color:#15803d;background:#dcfce7;border-radius:6px;padding:0 4px}
.x-so .tn u{text-decoration:none;color:#64748b}
.x-so.sai .tn em{color:#b91c1c;background:#fee2e2;text-decoration:line-through;text-decoration-thickness:5px}
.x-so .gh{flex:none;font-size:30px;font-weight:800;color:#64748b;text-align:right}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Tên ngày lễ —<br>*viết hoa chữ nào?*`, { tren: 262, co: 72, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p><em>N</em>gày Quốc <em>K</em>hánh 2/9</p></div>
    <div class="o b"><b>B</b><p>ngày Quốc khánh 2-9</p></div>
  </div>
  ${nhanBinhLuan("c1k0", 1060)}`,
      ve: veCC,
    },

    "le": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục II · mục V.5")}
  ${dong("<em>N</em>gày Quốc <em>K</em>hánh 2/9", "bên A", "c0k0", 320, "sai")}
  ${dong("<u>ngày</u> <em>Q</em>uốc khánh <em>2-9</em>", "ví dụ<br>của Nghị định", "c1k0", 470)}
  ${dong("ngày <em>Q</em>uốc tế <em>L</em>ao động 1-5", "", "c3k1", 620)}
  ${dong("ngày <em>P</em>hụ nữ <em>V</em>iệt <em>N</em>am 20-10", "", "c3k2", 770)}
  ${dong("ngày <em>T</em>ổng tuyển cử đầu tiên", "chỉ hoa<br>chữ Tổng", "c4k0", 920)}
  ${dau("B ĐÚNG", "c4e-0.1", { tren: 1120, co: 76, lop: "xanh" })}`,
    },

    "app": {
      khai: true,
      html: () => `
  ${chipKhai("🤖 Phần mềm soạn văn bản")}
  ${theWeb("c0k0")}`,
    },
  },

  mocThu: (tatCa) => [
    ["01a-0.4s", 0.4],
    ["01b-bl", lucNode(tatCa, "moc", "c1k1")],
    ["02a-quoc", lucNode(tatCa, "le", "c2e")],
    ["02b-het", lucNode(tatCa, "le", "c4e+0.2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
