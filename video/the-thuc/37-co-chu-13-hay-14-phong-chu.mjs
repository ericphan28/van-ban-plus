// ============================================================================
// THỂ THỨC 37 — CỠ CHỮ 14 BẮT BUỘC HAY 13 CŨNG ĐƯỢC? DÙNG PHÔNG ARIAL CÓ ĐƯỢC KHÔNG? THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 35): móc A/B + nhãn bình luận → quy định → «chỗ cỡ chữ cố định» → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc1.md (Phụ lục I, Phần I):
//   I.4: «Phông chữ: Phông chữ tiếng Việt Times New Roman, bộ mã ký tự Unicode theo Tiêu chuẩn Việt Nam TCVN 6909:2001, màu đen.»
//   I.5: «Cỡ chữ và kiểu chữ: Theo quy định cụ thể cho từng yếu tố thể thức.»
//   II.6e: «Nội dung văn bản được trình bày bằng chữ in thường, được canh đều cả hai lề, kiểu chữ đứng; cỡ chữ từ 13 đến 14; …»
//   II.3c: «Từ "Số" được trình bày bằng chữ in thường, cỡ chữ 13 …» · II.9: «Từ "Nơi nhận" … cỡ chữ 12, kiểu chữ nghiêng, đậm»
//   III.1b: «Từ "Phụ lục" và số thứ tự của Phụ lục … cỡ chữ 14 …» · II.8: thông tin chữ ký số «… phông chữ Times New Roman … cỡ chữ 10, màu đen.»
//   ✔ «phông khác không đúng quy định» = suy ra từ I.4 (chỉ ghi một phông). Không đọc tên phông (chữ Latin) — chỉ trên hình.
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

const co = (ten, so, hien, tren) =>
  the(`<div class="x-co"><span class="tn">${ten}</span><span class="so">cỡ <b>${so}</b></span></div>`, { tren, hien, lop: "x-the" });

export default {
  ten: "37-co-chu-13-hay-14-phong-chu",
  tieuDe: "Thể thức 37 · Cỡ chữ 14 bắt buộc hay 13 cũng được? Dùng phông Arial có được không? Theo NĐ 30/2020",
  bia: { dong: ["Cỡ chữ 13 hay 14?", "Ai đúng?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Nội dung văn bản được trình bày bằng chữ in thường, … *cỡ chữ từ 13 đến 14* …»",
      theNguon("Phụ lục I · mục II.6e"),
      theBinhLuan("Cơ quan anh chị dùng cỡ 13 hay 14? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản hành chính, cỡ chữ mười bốn mới đúng, hay mười ba cũng được?", chu: "Cỡ *14 mới đúng*, hay *13 cũng được?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "qd", cau: [
      { loi: "Phông chữ, Phụ lục một chỉ ghi đúng một tên, bảng mã chuẩn, chữ màu đen.", chu: "Phông: *Times New Roman* · Unicode · *màu đen*" },
      { loi: "Phông khác, dù đẹp hơn, cũng không đúng quy định.", chu: "Arial, Tahoma… → *không đúng quy định*" },
      { loi: "Còn nội dung văn bản, cỡ chữ từ mười ba đến mười bốn, cả hai đều đúng.", chu: "Nội dung: cỡ *13 đến 14* → *cả hai đều đúng*", nghiSau: 0.3 },
    ]},
    { canh: "co", cau: [
      { loi: "Nhưng có những chỗ, cỡ chữ là cố định.", chu: "Nhưng có chỗ cỡ chữ *cố định*" },
      { loi: "Từ số cỡ mười ba, chữ nơi nhận cỡ mười hai, chữ phụ lục cỡ mười bốn.", chu: "«Số» *13* · «Nơi nhận» *12* · «Phụ lục» *14*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Nội dung văn bản, được trình bày bằng chữ in thường, cỡ chữ từ mười ba đến mười bốn.", chu: "Nội dung: chữ in thường · cỡ *13 đến 14*" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.6e"),
      binhLuanCau("Cơ quan anh chị dùng cỡ mấy, bình luận cho mọi người cùng biết.", "Cơ quan anh chị dùng cỡ mấy? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;gap:30px}
.x-ab .o{flex:1;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 20px 30px;text-align:center}
.x-ab .o b{display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:18px 0 0;font-size:46px;font-weight:900;line-height:1.25;color:#0f172a}
.x-ab .o p em{font-style:normal;color:#dc2626}
.x-ph{padding:28px 32px;text-align:center}
.x-ph .tn{font-family:"Times New Roman",serif;font-size:60px;font-weight:700;color:#0f172a}
.x-ph small{display:block;margin-top:8px;font-size:30px;font-weight:800;color:#64748b}
.x-sai{padding:22px 32px;display:flex;justify-content:center;gap:28px;align-items:center}
.x-sai span{font-size:50px;color:#94a3b8;text-decoration:line-through;text-decoration-color:#dc2626;text-decoration-thickness:6px}
.x-sai .ar{font-family:Arial,sans-serif}.x-sai .ta{font-family:Tahoma,sans-serif}.x-sai .ca{font-family:Calibri,sans-serif}
.x-nd{padding:28px 32px;text-align:center;font-size:46px;font-weight:900;color:#0f172a}
.x-nd b{color:#15803d}
.x-the{padding:0}
.x-co{display:flex;align-items:center;justify-content:space-between;padding:22px 34px}
.x-co .tn{font-family:"Times New Roman",serif;font-size:48px;font-weight:700;color:#0f172a}
.x-co .so{font-size:36px;font-weight:800;color:#64748b}
.x-co .so b{display:inline-block;min-width:80px;text-align:center;font-size:52px;font-weight:900;color:#fff;background:var(--mau-chinh);border-radius:16px;padding:2px 14px;margin-left:8px}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Cỡ chữ 13 hay 14 —<br>*ai đúng?*`, { tren: 262, co: 72, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>Phải cỡ <em>14</em><br>mới đúng</p></div>
    <div class="o b"><b>B</b><p>Cỡ <em>13</em> hay <em>14</em><br>đều đúng</p></div>
  </div>
  ${nhanBinhLuan("c1k0", 1080)}`,
      ve: veCC,
    },

    "qd": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục I · phông và cỡ chữ")}
  ${the(`<div class="x-ph"><span class="tn">Times New Roman</span><small>bộ mã Unicode · TCVN 6909:2001 · màu đen</small></div>`, { tren: 320, hien: "c0k0" })}
  ${the(`<div class="x-sai"><span class="ar">Arial</span><span class="ta">Tahoma</span><span class="ca">Calibri</span></div>`, { tren: 560, hien: "c1k0" })}
  ${the(`<div class="x-nd">Nội dung văn bản: cỡ <b>13 đến 14</b></div>`, { tren: 760, hien: "c2k0" })}
  ${dau("B ĐÚNG", "c2k2", { tren: 980, co: 76, lop: "xanh" })}`,
    },

    "co": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Chỗ cỡ chữ cố định")}
  ${co("Số:", "13", "c1k0", 340)}
  ${co("Nơi nhận:", "12", "c1k1", 500)}
  ${co("Phụ lục I", "14", "c1k2", 660)}`,
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
    ["02a-phong", lucNode(tatCa, "qd", "c1k1")],
    ["02b-dau", lucNode(tatCa, "qd", "c2e+0.2")],
    ["03a-co", lucNode(tatCa, "co", "c1e")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
