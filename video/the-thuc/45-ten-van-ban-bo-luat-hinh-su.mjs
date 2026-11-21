// ============================================================================
// THỂ THỨC 45 — «BỘ LUẬT HÌNH SỰ» VIẾT HOA THẾ NÀO? VIẾT HOA TÊN VĂN BẢN, TÊN SÁCH BÁO THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 39): móc A/B + nhãn bình luận → quy định + ví dụ → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc2.md:
//   V.6: «Tên các loại văn bản: Viết hoa chữ cái đầu của tên loại văn bản và chữ cái đầu của âm tiết thứ nhất tạo thành tên gọi của
//   văn bản trong trường hợp nói đến một văn bản cụ thể. Ví dụ: Bộ luật Hình sự, Luật Tổ chức Quốc hội,…»
//   V.10: «Tên các tác phẩm, sách báo, tạp chí: Viết hoa chữ cái đầu của âm tiết thứ nhất tạo thành tên tác phẩm, sách báo. Ví dụ:
//   từ điển Bách khoa toàn thư, tạp chí Cộng sản,…»
//   ✔ «Luật Tổ chức Quốc hội»: chỉ đưa ví dụ trên hình, không giải thích vì sao «Quốc hội» hoa (Nghị định không nói).
//   ✔ «tạp chí», «từ điển» viết thường: đúng ví dụ (nói «trong ví dụ»).
//   ✔ không trùng clip 36 (viện dẫn Điều, khoản — mục V.7).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

const dong = (chu, ghi, hien, tren, lop = "") =>
  the(`<div class="x-so ${lop}"><span class="tn">${chu}</span><span class="gh">${ghi}</span></div>`, { tren, hien, lop: "x-the" });

export default {
  ten: "45-ten-van-ban-bo-luat-hinh-su",
  tieuDe: "Thể thức 45 · «Bộ Luật Hình Sự» hay «Bộ luật Hình sự»? Viết hoa tên văn bản, tên sách báo theo NĐ 30/2020",
  bia: { dong: ["Bộ Luật Hình Sự", "sai ở đâu?"], vai: "Nghị định 30 · Phụ lục II" },
  ket: {
    the: [
      "«Tên các loại văn bản: Viết hoa *chữ cái đầu của tên loại văn bản* và *chữ cái đầu của âm tiết thứ nhất* tạo thành tên gọi của văn bản trong trường hợp nói đến một văn bản cụ thể.»",
      theNguon("Phụ lục II · mục V.6"),
      theBinhLuan("Anh chị đã viết đúng chưa? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Tên một bộ luật cụ thể, viết hoa thế nào, bên A, hay bên B?", chu: "«Bộ Luật Hình Sự» hay «Bộ luật Hình sự»? — *A hay B?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "vb", cau: [
      { loi: "Nói đến một văn bản cụ thể, viết hoa chữ đầu tên loại, và chữ đầu âm tiết thứ nhất của tên gọi.", chu: "Hoa *chữ đầu tên loại* + *âm tiết thứ nhất* của tên gọi" },
      { loi: "Nên viết, Bộ luật Hình sự, chỉ hoa chữ Bộ, và chữ Hình.", chu: "*B*ộ luật *H*ình sự — chỉ hoa *Bộ* và *Hình*" },
      { loi: "Tên sách báo, chỉ hoa âm tiết thứ nhất của tên, như tạp chí Cộng sản.", chu: "Sách báo: hoa *âm tiết thứ nhất* — tạp chí *C*ộng sản" },
      { loi: "Trong ví dụ, chữ tạp chí, chữ từ điển, đứng trước tên, đều viết thường.", chu: "*tạp chí*, *từ điển* đứng trước tên → *viết thường*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Viết hoa chữ cái đầu của tên loại văn bản, và chữ cái đầu của âm tiết thứ nhất, tạo thành tên gọi.", chu: "Hoa *tên loại* + *âm tiết thứ nhất của tên gọi*" },
      nguonCau("Phụ lục hai", "Phụ lục II · mục V.6"),
      binhLuanCau("Anh chị đã viết đúng chưa, bình luận cho mọi người cùng biết.", "Anh chị đã viết đúng chưa? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;flex-direction:column;gap:26px}
.x-ab .o{display:flex;align-items:center;gap:26px;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 30px}
.x-ab .o b{flex:none;display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:0;font-family:"Times New Roman",serif;font-size:62px;color:#0f172a}
.x-the{padding:0}
.x-so{display:flex;align-items:center;justify-content:space-between;gap:20px;padding:22px 34px}
.x-so .tn{font-family:"Times New Roman",serif;font-size:54px;color:#0f172a}
.x-so .tn em{font-style:normal;font-weight:700;color:#15803d;background:#dcfce7;border-radius:6px;padding:0 4px}
.x-so .tn u{text-decoration:none;color:#64748b;background:#f1f5f9;border-radius:6px;padding:0 4px}
.x-so.sai .tn em{color:#b91c1c;background:#fee2e2;text-decoration:line-through;text-decoration-thickness:5px}
.x-so .gh{flex:none;font-size:30px;font-weight:800;color:#64748b;text-align:right}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Tên bộ luật —<br>*viết hoa chữ nào?*`, { tren: 262, co: 72, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>Bộ Luật Hình Sự</p></div>
    <div class="o b"><b>B</b><p>Bộ luật Hình sự</p></div>
  </div>
  ${nhanBinhLuan("c1k0", 1060)}`,
      ve: veCC,
    },

    "vb": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục II · mục V.6, V.10")}
  ${dong("Bộ <em>L</em>uật Hình <em>S</em>ự", "bên A", "c0k1", 320, "sai")}
  ${dong("<em>B</em>ộ luật <em>H</em>ình sự", "ví dụ", "c1k1", 470)}
  ${dong("<em>L</em>uật <em>T</em>ổ chức Quốc hội", "ví dụ", "c1k2", 620)}
  ${dong("<u>tạp chí</u> <em>C</em>ộng sản", "sách báo", "c2k2", 770)}
  ${dong("<u>từ điển</u> <em>B</em>ách khoa toàn thư", "", "c3k2", 920)}
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
    ["02a-b", lucNode(tatCa, "vb", "c1e+0.2")],
    ["02b-het", lucNode(tatCa, "vb", "c3e+0.2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
