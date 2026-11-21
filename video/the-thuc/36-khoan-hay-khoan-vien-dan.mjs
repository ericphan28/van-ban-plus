// ============================================================================
// THỂ THỨC 36 — «KHOẢN 2 ĐIỀU 5» HAY «khoản 2 Điều 5»? CHỮ NÀO VIẾT HOA KHI VIỆN DẪN, THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 35): móc A/B + nhãn bình luận → quy định → «bẫy» đầu câu → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc2.md (bản docs/…/phu-luc-II.md bị cụt câu này — đã sửa 22/11/2026):
//   Phụ lục II mục V.7: «Trường hợp viện dẫn phần, chương, mục, tiểu mục, điều, khoản, điểm của một văn bản cụ thể thì viết
//   hoa chữ cái đầu của phần, chương, mục, tiểu mục, điều. Ví dụ: - Căn cứ điểm a khoản 2 Điều 103 Mục 5 Chương XII Phần I
//   của Bộ luật Hình sự.»
//   Phụ lục II mục I: «Viết hoa chữ cái đầu âm tiết thứ nhất của một câu hoàn chỉnh: Sau dấu chấm câu (.); sau dấu chấm hỏi
//   (?); sau dấu chấm than (!) và khi xuống dòng.»
//   ✔ «khoản, điểm viết thường» = suy ra từ V.7 (chỉ liệt kê phần, chương, mục, tiểu mục, điều) + ví dụ nguyên văn viết thường.
//   ✔ «đầu câu, đầu dòng vẫn viết hoa» = mục I (phép đặt câu).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

const o = (chu, lop, hien) => `<span class="${lop}" data-k="lop" data-hien="${hien}" style="opacity:0">${chu}</span>`;

export default {
  ten: "36-khoan-hay-khoan-vien-dan",
  tieuDe: "Thể thức 36 · «Khoản 2 Điều 5» hay «khoản 2 Điều 5»? Chữ nào viết hoa khi viện dẫn, theo NĐ 30/2020",
  bia: { dong: ["Khoản hay khoản?", "Điều hay điều?"], vai: "Nghị định 30 · Phụ lục II" },
  ket: {
    the: [
      "«… viện dẫn … thì viết hoa chữ cái đầu của *phần, chương, mục, tiểu mục, điều.*»",
      theNguon("Phụ lục II · mục V.7"),
      theBinhLuan("Cơ quan anh chị viết «Khoản» hay «khoản»? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Căn cứ khoản hai Điều năm, chữ nào viết hoa, chữ nào viết thường?", chu: "«khoản 2 Điều 5»: chữ nào *viết hoa?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "qd", cau: [
      { loi: "Viết hoa chữ cái đầu của phần, chương, mục, tiểu mục, và điều.", chu: "VIẾT HOA: *Phần · Chương · Mục · Tiểu mục · Điều*" },
      { loi: "Khoản và điểm, không có trong danh sách, nên viết thường.", chu: "*khoản · điểm* → viết thường" },
      { loi: "Ví dụ trong Nghị định, điểm a khoản hai Điều một trăm linh ba, Bộ luật Hình sự.", chu: "Ví dụ: «điểm a khoản 2 *Điều* 103 … Bộ luật Hình sự»", nghiSau: 0.3 },
    ]},
    { canh: "bay", cau: [
      { loi: "Nhưng nếu khoản đứng đầu câu, hay đầu dòng, thì vẫn viết hoa.", chu: "Đứng *đầu câu, đầu dòng* → vẫn *viết hoa*" },
      { loi: "Đó là viết hoa, vì phép đặt câu.", chu: "Đó là viết hoa *vì phép đặt câu* (mục I)", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Trường hợp viện dẫn, thì viết hoa chữ cái đầu của phần, chương, mục, tiểu mục, điều.", chu: "Viện dẫn: viết hoa *phần, chương, mục, tiểu mục, điều*" },
      nguonCau("Phụ lục hai", "Phụ lục II · mục V.7"),
      binhLuanCau("Cơ quan anh chị đang viết thế nào, bình luận cho mọi người cùng biết.", "Cơ quan anh chị viết thế nào? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;flex-direction:column;gap:26px}
.x-ab .o{display:flex;align-items:center;gap:26px;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 30px}
.x-ab .o b{flex:none;display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:0;font-family:"Times New Roman",serif;font-size:56px;font-weight:700;color:#0f172a}
.x-ab .o em{font-style:normal;color:#dc2626}
.x-hoa{padding:28px 30px;text-align:center}
.x-hoa h4{margin:0 0 18px;font-size:34px;font-weight:900;color:#64748b;letter-spacing:.05em}
.x-hoa .ds{display:flex;flex-wrap:wrap;justify-content:center;gap:16px}
.x-hoa .ds span{font-size:44px;font-weight:900;border-radius:16px;padding:10px 24px}
.x-hoa .h{color:#fff;background:#15803d}
.x-hoa .t{color:#334155;background:#f1f5f9;border:4px solid #cbd5e1}
.x-vd{padding:30px 34px;font-family:"Times New Roman",serif;font-size:46px;line-height:1.4;color:#0f172a}
.x-vd small{display:block;font-family:inherit;font-size:28px;font-weight:800;color:#64748b;margin-bottom:8px;font-family:system-ui,sans-serif}
.x-vd em{font-style:normal;color:#15803d;font-weight:700;background:#dcfce7;border-radius:6px;padding:0 4px}
.x-vd i{font-style:normal;color:#64748b}
.x-bay{padding:30px 34px;font-family:"Times New Roman",serif;font-size:50px;line-height:1.4;color:#0f172a}
.x-bay small{display:block;font-size:28px;font-weight:800;color:#64748b;margin-bottom:6px;font-family:system-ui,sans-serif}
.x-ly{padding:28px 34px;text-align:center;font-size:46px;font-weight:900;color:#0f172a;line-height:1.25}
.x-ly b{color:var(--mau-chinh)}
.x-ly small{display:block;margin-top:10px;font-size:28px;font-weight:700;color:#64748b}
.x-bay em{font-style:normal;color:#b45309;font-weight:700;background:#fef3c7;border-radius:6px;padding:0 4px}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}«Khoản» hay «khoản» —<br>*chữ nào viết hoa?*`, { tren: 262, co: 68, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>Căn cứ <em>K</em>hoản 2 <em>Đ</em>iều 5</p></div>
    <div class="o b"><b>B</b><p>Căn cứ <em>k</em>hoản 2 <em>Đ</em>iều 5</p></div>
  </div>
  ${nhanBinhLuan("c1k0", 1060)}`,
      ve: veCC,
    },

    "qd": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục II · mục V.7")}
  ${the(`<div class="x-hoa"><h4>VIẾT HOA CHỮ CÁI ĐẦU</h4><div class="ds">${o("Phần", "h", "c0k0w6")}${o("Chương", "h", "c0k1")}${o("Mục", "h", "c0k2")}${o("Tiểu mục", "h", "c0k3")}${o("Điều", "h", "c0k4w1")}</div></div>`, { tren: 330, hien: "c0k0" })}
  ${the(`<div class="x-hoa"><h4>VIẾT THƯỜNG</h4><div class="ds">${o("khoản", "t", "c1k0")}${o("điểm", "t", "c1k0+0.2")}</div></div>`, { tren: 640, hien: "c1k0" })}
  ${the(`<div class="x-vd"><small>VÍ DỤ NGUYÊN VĂN TRONG NGHỊ ĐỊNH</small>Căn cứ điểm a khoản 2 <em>Điều</em> 103 <em>Mục</em> 5 <em>Chương</em> XII <em>Phần</em> I <i>của Bộ luật Hình sự.</i></div>`, { tren: 860, hien: "c2k0" })}
  ${dau("B ĐÚNG", "c2e-0.1", { tren: 1160, co: 76, lop: "xanh" })}`,
    },

    "bay": {
      khai: true,
      html: () => `
  ${chipKhai("⚠️ Trường hợp dễ cãi nhau")}
  ${the(`<div class="x-bay"><small>GIỮA CÂU → viết thường</small>… theo quy định tại <em>k</em>hoản 2 Điều 5 …</div>`, { tren: 360, hien: "c0k0" })}
  ${the(`<div class="x-bay"><small>ĐẦU CÂU, ĐẦU DÒNG → viết hoa</small><em>K</em>hoản 2 Điều 5 quy định …</div>`, { tren: 650, hien: "c0k1" })}
  ${the(`<div class="x-vb-ly">Viết hoa vì <b>phép đặt câu</b><small>Phụ lục II · mục I: sau dấu chấm, chấm hỏi, chấm than và khi xuống dòng</small></div>`, { tren: 940, hien: "c1k1", lop: "x-ly" })}`,
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
    ["02a-hoa", lucNode(tatCa, "qd", "c0k4w1+0.5")],
    ["02b-vd", lucNode(tatCa, "qd", "c2e+0.3")],
    ["03a-bay", lucNode(tatCa, "bay", "c1k1+0.6")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
