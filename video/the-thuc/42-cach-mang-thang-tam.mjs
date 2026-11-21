// ============================================================================
// THỂ THỨC 42 — «CÁCH MẠNG THÁNG 8» HAY «CÁCH MẠNG THÁNG TÁM»? VIẾT HOA SỰ KIỆN LỊCH SỬ, TRIỀU ĐẠI, NĂM ÂM LỊCH THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 39): móc A/B + nhãn bình luận → quy định + ví dụ → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc2.md:
//   V.9: «Tên các sự kiện lịch sử và các triều đại: Viết hoa chữ cái đầu của các âm tiết tạo thành sự kiện và tên sự kiện, trong
//   trường hợp có các con số chỉ mốc thời gian thì ghi bằng chữ và viết hoa chữ đó. Ví dụ: Triều Lý, Triều Trần, Phong trào Xô viết
//   Nghệ Tĩnh, Cách mạng tháng Tám,…»
//   V.8a: «Tên các năm âm lịch: Viết hoa chữ cái đầu của tất cả các âm tiết tạo thành tên gọi. Ví dụ: Kỷ Tỵ, Tân Hợi, Mậu Tuất, Mậu Thân,...»
//   ✔ «Cách mạng tháng Tám»: «tháng» thường, «Tám» hoa — đúng ví dụ · «Triều» viết hoa — đúng ví dụ.
//   ✔ không trùng clip 13 (tháng Năm, thứ Hai, Tết — mục V.8b, c).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

const dong = (chu, ghi, hien, tren, lop = "") =>
  the(`<div class="x-so ${lop}"><span class="tn">${chu}</span><span class="gh">${ghi}</span></div>`, { tren, hien, lop: "x-the" });

export default {
  ten: "42-cach-mang-thang-tam",
  tieuDe: "Thể thức 42 · «Cách mạng Tháng 8» hay «Cách mạng tháng Tám»? Viết hoa sự kiện lịch sử, năm âm lịch theo NĐ 30/2020",
  bia: { dong: ["Tháng 8", "hay tháng Tám?"], vai: "Nghị định 30 · Phụ lục II" },
  ket: {
    the: [
      "«… trong trường hợp có các con số chỉ mốc thời gian thì *ghi bằng chữ và viết hoa chữ đó*. Ví dụ: Triều Lý, Triều Trần, …, *Cách mạng tháng Tám*,…»",
      theNguon("Phụ lục II · mục V.9"),
      theBinhLuan("Anh chị đã chọn đúng chưa? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Cách mạng tháng Tám, viết bên A, hay bên B?", chu: "«Cách mạng Tháng 8» hay «Cách mạng tháng Tám»? — *A hay B?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "su-kien", cau: [
      { loi: "Sự kiện lịch sử có con số chỉ mốc thời gian, thì ghi bằng chữ, và viết hoa chữ đó.", chu: "Có *con số mốc thời gian* → *ghi bằng chữ* + *viết hoa chữ đó*" },
      { loi: "Bên A sai hai chỗ, ghi bằng chữ số, và viết hoa chữ tháng.", chu: "A sai: *ghi chữ số «8»* · *viết hoa «Tháng»*" },
      { loi: "Nên viết, Cách mạng tháng Tám, chữ tháng viết thường, chữ Tám viết hoa.", chu: "Cách mạng *tháng* *T*ám — *tháng* thường · *Tám* hoa" },
      { loi: "Triều đại cũng viết hoa, như Triều Lý, Triều Trần.", chu: "*T*riều *L*ý · *T*riều *T*rần" },
      { loi: "Tên năm âm lịch, viết hoa tất cả các âm tiết, như Mậu Thân, Tân Hợi.", chu: "Năm âm lịch: hoa *tất cả âm tiết* — *M*ậu *T*hân · *T*ân *H*ợi", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Trường hợp có các con số chỉ mốc thời gian, thì ghi bằng chữ, và viết hoa chữ đó.", chu: "Con số mốc thời gian → *ghi bằng chữ, viết hoa chữ đó*" },
      nguonCau("Phụ lục hai", "Phụ lục II · mục V.9"),
      binhLuanCau("Anh chị đã chọn đúng chưa, bình luận cho mọi người cùng biết.", "Anh chị đã chọn đúng chưa? *Bình luận bên dưới*"),
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
.x-so .tn u{text-decoration:none;color:#64748b}
.x-so.sai .tn em{color:#b91c1c;background:#fee2e2;text-decoration:line-through;text-decoration-thickness:5px}
.x-so .gh{flex:none;font-size:30px;font-weight:800;color:#64748b;text-align:right}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Sự kiện lịch sử —<br>*viết hoa thế nào?*`, { tren: 262, co: 72, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>Cách mạng Tháng 8</p></div>
    <div class="o b"><b>B</b><p>Cách mạng tháng Tám</p></div>
  </div>
  ${nhanBinhLuan("c1k0", 1060)}`,
      ve: veCC,
    },

    "su-kien": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục II · mục V.9, V.8")}
  ${dong("Cách mạng <em>Tháng 8</em>", "bên A", "c1k0", 320, "sai")}
  ${dong("Cách mạng <u>tháng</u> <em>T</em>ám", "số → chữ<br>viết hoa", "c2k1", 470)}
  ${dong("<em>T</em>riều <em>L</em>ý · <em>T</em>riều <em>T</em>rần", "triều đại", "c3k1", 620)}
  ${dong("<em>M</em>ậu <em>T</em>hân · <em>T</em>ân <em>H</em>ợi", "năm âm lịch", "c4k1", 770)}
  ${dau("B ĐÚNG", "c2e-0.1", { tren: 1010, co: 76, lop: "xanh" })}`,
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
    ["02a-tam", lucNode(tatCa, "su-kien", "c2e+0.2")],
    ["02b-het", lucNode(tatCa, "su-kien", "c4e+0.2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
