// ============================================================================
// THỂ THỨC 44 — «SAO VÀNG» HAY «SAO VÀNG» CHỮ V THƯỜNG? VIẾT HOA HUÂN CHƯƠNG, DANH HIỆU, DANH TỪ RIÊNG HÓA THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 39): móc A/B + nhãn bình luận → ví dụ của Nghị định → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc2.md:
//   V.2: «Tên các huân chương, huy chương, các danh hiệu vinh dự: Viết hoa chữ cái đầu của các âm tiết của các thành phần tạo thành
//   tên riêng và các từ chỉ thứ, hạng. Ví dụ: Huân chương Sao vàng, Nghệ sĩ Nhân dân, Anh hùng Lao động,...»
//   V.4: «Danh từ chung đã riêng hóa: Viết hoa chữ cái đầu của từ, cụm từ chỉ tên gọi đó trong trường hợp dùng trong một nhân xưng,
//   đứng độc lập và thể hiện sự trân trọng. Ví dụ: Bác, Người (chỉ Chủ tịch Hồ Chí Minh), Đảng (chỉ Đảng Cộng sản Việt Nam),…»
//   ✔ «Sao vàng» chữ «vàng» thường: đúng ví dụ (nói «ví dụ trong Nghị định viết …», không suy diễn quy tắc).
//   ✔ không nói về «hạng Nhất / hạng nhất» (Nghị định không có ví dụ).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

const dong = (chu, ghi, hien, tren, lop = "") =>
  the(`<div class="x-so ${lop}"><span class="tn">${chu}</span><span class="gh">${ghi}</span></div>`, { tren, hien, lop: "x-the" });

export default {
  ten: "44-huan-chuong-sao-vang",
  tieuDe: "Thể thức 44 · «Huân chương Sao Vàng» hay «Huân chương Sao vàng»? Viết hoa huân chương, danh hiệu theo NĐ 30/2020",
  bia: { dong: ["Sao Vàng", "hay Sao vàng?"], vai: "Nghị định 30 · Phụ lục II" },
  ket: {
    the: [
      "«Tên các huân chương, huy chương, các danh hiệu vinh dự: Viết hoa chữ cái đầu của các âm tiết của các thành phần tạo thành tên riêng và các từ chỉ thứ, hạng. Ví dụ: *Huân chương Sao vàng*, *Nghệ sĩ Nhân dân*, *Anh hùng Lao động*,...»",
      theNguon("Phụ lục II · mục V.2"),
      theBinhLuan("Anh chị viết «vàng» hoa hay thường? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Huân chương Sao vàng, chữ vàng viết hoa, hay viết thường?", chu: "«Sao *V*àng» hay «Sao *v*àng»? — *A hay B?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "hc", cau: [
      { loi: "Ví dụ trong Nghị định ba mươi, viết Huân chương Sao vàng, chữ vàng viết thường.", chu: "Ví dụ của Nghị định: Huân chương *S*ao *vàng*" },
      { loi: "Danh hiệu cũng vậy, Nghệ sĩ Nhân dân, Anh hùng Lao động.", chu: "*N*ghệ sĩ *N*hân dân · *A*nh hùng *L*ao động" },
      { loi: "Danh từ chung đã riêng hóa, như Bác, Người, chỉ Chủ tịch Hồ Chí Minh, thì viết hoa.", chu: "*Bác*, *Người* (chỉ Chủ tịch Hồ Chí Minh) → *viết hoa*" },
      { loi: "Đảng, chỉ Đảng Cộng sản Việt Nam, cũng viết hoa chữ đầu.", chu: "*Đảng* (chỉ Đảng Cộng sản Việt Nam) → *viết hoa*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Tên huân chương, danh hiệu, viết hoa chữ cái đầu, của các thành phần tạo thành tên riêng.", chu: "Huân chương, danh hiệu: hoa *chữ đầu các thành phần tạo thành tên riêng*" },
      nguonCau("Phụ lục hai", "Phụ lục II · mục V.2"),
      binhLuanCau("Văn bản ở cơ quan anh chị, đang viết theo kiểu nào, bình luận cho mọi người cùng biết.", "Anh chị viết «vàng» hoa hay thường? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;flex-direction:column;gap:26px}
.x-ab .o{display:flex;align-items:center;gap:26px;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 30px}
.x-ab .o b{flex:none;display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:0;font-family:"Times New Roman",serif;font-size:62px;color:#0f172a}
.x-ab .o em{font-style:normal;color:#dc2626;font-weight:700}
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
  ${mocKhai(`${NGU_CANH}Tên huân chương —<br>*viết hoa chữ nào?*`, { tren: 262, co: 72, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>Huân chương Sao <em>V</em>àng</p></div>
    <div class="o b"><b>B</b><p>Huân chương Sao <em>v</em>àng</p></div>
  </div>
  ${nhanBinhLuan("c1k0", 1060)}`,
      ve: veCC,
    },

    "hc": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục II · mục V.2, V.4")}
  ${dong("<em>H</em>uân chương <em>S</em>ao <u>vàng</u>", "ví dụ<br>của Nghị định", "c0k1", 320)}
  ${dong("<em>N</em>ghệ sĩ <em>N</em>hân dân", "danh hiệu", "c1k1", 470)}
  ${dong("<em>A</em>nh hùng <em>L</em>ao động", "danh hiệu", "c1k2", 620)}
  ${dong("<em>B</em>ác · <em>N</em>gười · <em>Đ</em>ảng", "riêng hóa", "c2k1", 770)}
  ${dau("B ĐÚNG", "c0e-0.1", { tren: 1010, co: 76, lop: "xanh" })}`,
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
    ["02a-vang", lucNode(tatCa, "hc", "c0e+0.2")],
    ["02b-het", lucNode(tatCa, "hc", "c3e+0.2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
