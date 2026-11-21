// ============================================================================
// THỂ THỨC 43 — «LÊ-NIN» HAY «LÊ-NIN» VIẾT HOA TỪNG ÂM TIẾT? TÊN NGƯỜI, TÊN ĐỊA LÝ NƯỚC NGOÀI PHIÊN ÂM THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 39): móc A/B + nhãn bình luận → quy định + ví dụ → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc2.md:
//   II.2a: «Trường hợp phiên âm sang âm Hán - Việt: Viết theo quy tắc viết tên người Việt Nam. Ví dụ: Kim Nhật Thành, Mao Trạch Đông,
//   Thành Cát Tư Hãn,...»
//   II.2b: «Trường hợp phiên âm không sang âm Hán - Việt (phiên âm trực tiếp sát cách đọc của nguyên ngữ): Viết hoa chữ cái đầu âm
//   tiết thứ nhất trong mỗi thành phần. Ví dụ: Vla-đi-mia I-lích Lê-nin, Phri-đrích Ăng-ghen,...»
//   III.2a: «Tên địa lý đã được phiên âm sang âm Hán - Việt: Viết theo quy tắc viết hoa tên địa lý Việt Nam. Ví dụ: Bắc Kinh, Bình Nhưỡng, Pháp, Anh,...»
//   III.2b: «… Viết hoa theo quy tắc viết hoa tên người nước ngoài … Ví dụ: Mát-xcơ-va, Men-bơn,...»
//   ✔ gạch nối: chỉ nói «trong ví dụ của Nghị định, các âm tiết nối bằng gạch nối» (không có câu quy định riêng).
//   ✔ tên nước ngoài chỉ trên hình; lời đọc không đọc tên phiên âm (tránh máy đọc sai).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

const dong = (chu, ghi, hien, tren, lop = "") =>
  the(`<div class="x-so ${lop}"><span class="tn">${chu}</span><span class="gh">${ghi}</span></div>`, { tren, hien, lop: "x-the" });

export default {
  ten: "43-ten-nuoc-ngoai-phien-am",
  tieuDe: "Thể thức 43 · «Lê-Nin» hay «Lê-nin»? Viết hoa tên người, tên địa lý nước ngoài phiên âm theo NĐ 30/2020",
  bia: { dong: ["Lê-Nin", "hay Lê-nin?"], vai: "Nghị định 30 · Phụ lục II" },
  ket: {
    the: [
      "«Trường hợp phiên âm không sang âm Hán - Việt (phiên âm trực tiếp sát cách đọc của nguyên ngữ): *Viết hoa chữ cái đầu âm tiết thứ nhất trong mỗi thành phần*. Ví dụ: Vla-đi-mia I-lích Lê-nin …»",
      theNguon("Phụ lục II · mục II.2b"),
      theBinhLuan("Anh chị chọn đúng không? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Tên người nước ngoài phiên âm, viết hoa từng âm tiết, hay chỉ âm tiết đầu?", chu: "Tên nước ngoài phiên âm — *hoa từng âm* hay *chỉ âm đầu?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "ten", cau: [
      { loi: "Phiên âm sát cách đọc nguyên ngữ, chỉ viết hoa âm tiết thứ nhất, trong mỗi thành phần.", chu: "Phiên âm sát nguyên ngữ → hoa *âm tiết thứ nhất* mỗi thành phần" },
      { loi: "Ví dụ trong Nghị định, các âm tiết còn lại viết thường, nối bằng gạch nối.", chu: "Âm còn lại *viết thường* · nối bằng *gạch nối*" },
      { loi: "Phiên âm sang Hán Việt, thì viết như tên người Việt Nam, hoa tất cả.", chu: "Phiên âm *Hán - Việt* → viết như *tên người Việt Nam*" },
      { loi: "Tên địa lý nước ngoài, cũng theo đúng hai cách này.", chu: "Tên địa lý nước ngoài: *theo đúng hai cách này*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Phiên âm không sang âm Hán Việt, viết hoa chữ cái đầu âm tiết thứ nhất, trong mỗi thành phần.", chu: "Không sang Hán - Việt → hoa *âm tiết thứ nhất trong mỗi thành phần*" },
      nguonCau("Phụ lục hai", "Phụ lục II · mục II.2b"),
      binhLuanCau("Anh chị chọn đúng không, bình luận cho mọi người cùng biết.", "Anh chị chọn đúng không? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;flex-direction:column;gap:26px}
.x-ab .o{display:flex;align-items:center;gap:26px;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 30px}
.x-ab .o b{flex:none;display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:0;font-family:"Times New Roman",serif;font-size:56px;color:#0f172a}
.x-ab .o small{display:block;font-family:system-ui,sans-serif;font-size:26px;font-weight:800;color:#94a3b8}
.x-the{padding:0}
.x-so{display:flex;align-items:center;justify-content:space-between;gap:20px;padding:22px 34px}
.x-so .tn{font-family:"Times New Roman",serif;font-size:52px;color:#0f172a}
.x-so .tn em{font-style:normal;font-weight:700;color:#15803d;background:#dcfce7;border-radius:6px;padding:0 4px}
.x-so.sai .tn em{color:#b91c1c;background:#fee2e2;text-decoration:line-through;text-decoration-thickness:5px}
.x-so .gh{flex:none;font-size:30px;font-weight:800;color:#64748b;text-align:right}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Tên nước ngoài —<br>*viết hoa chữ nào?*`, { tren: 262, co: 72, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>Vla-Đi-Mia I-Lích Lê-Nin<small>hoa từng âm tiết</small></p></div>
    <div class="o b"><b>B</b><p>Vla-đi-mia I-lích Lê-nin<small>hoa âm tiết đầu mỗi thành phần</small></p></div>
  </div>
  ${nhanBinhLuan("c1k0", 1100)}`,
      ve: veCC,
    },

    "ten": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục II · mục II.2, III.2")}
  ${dong("Vla-<em>Đ</em>i-<em>M</em>ia I-<em>L</em>ích Lê-<em>N</em>in", "bên A", "c0k0", 320, "sai")}
  ${dong("<em>V</em>la-đi-mia <em>I</em>-lích <em>L</em>ê-nin", "ví dụ", "c0k1", 470)}
  ${dong("<em>P</em>hri-đrích <em>Ă</em>ng-ghen", "ví dụ", "c1k1", 620)}
  ${dong("<em>M</em>ao <em>T</em>rạch <em>Đ</em>ông", "Hán - Việt", "c2k1", 770)}
  ${dong("<em>M</em>át-xcơ-va · <em>B</em>ắc <em>K</em>inh", "địa lý", "c3k0", 920)}
  ${dau("B ĐÚNG", "c1e-0.1", { tren: 1110, co: 76, lop: "xanh" })}`,
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
    ["02a-b", lucNode(tatCa, "ten", "c1e+0.2")],
    ["02b-het", lucNode(tatCa, "ten", "c3e+0.2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
