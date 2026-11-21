// ============================================================================
// THỂ THỨC 38 — KÝ VĂN BẢN BẰNG BÚT BI XANH CÓ SAI KHÔNG? MỰC KÝ, MỰC DẤU THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 35): móc A/B + nhãn bình luận → quy định → màu dấu / bản điện tử → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/30_2020_ND_CP.md + Phuluc1.md:
//   Điều 13 khoản 6: «Đối với văn bản giấy, khi ký văn bản dùng bút có mực màu xanh, không dùng các loại mực dễ phai.»
//   Điều 33 khoản 1 điểm a: «Dấu đóng phải rõ ràng, ngay ngắn, đúng chiều và dùng đúng mực dấu màu đỏ theo quy định.»
//   Phụ lục I II.7d: hình ảnh chữ ký số của người có thẩm quyền «… màu xanh, định dạng Portable Network Graphics (.png) nền trong suốt …»
//   Phụ lục I II.8a: hình ảnh chữ ký số của cơ quan «… là hình ảnh dấu của cơ quan, tổ chức … màu đỏ …»
//   ✔ «không nói bút máy hay bút bi» = Điều 13.6 chỉ ghi «bút có mực màu xanh» (grep «bút» cả Nghị định: chỉ câu này).
//   ✘ KHÔNG kết luận bút bi có «dễ phai» hay không — để người xem bình luận.
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

export default {
  ten: "38-ky-but-bi-xanh-muc-dau",
  tieuDe: "Thể thức 38 · Ký văn bản bằng bút bi xanh có sai không? Mực ký, mực dấu theo NĐ 30/2020",
  bia: { dong: ["Ký bút bi xanh", "có sai không?"], vai: "Nghị định 30 · Điều 13" },
  ket: {
    the: [
      "«Đối với văn bản giấy, khi ký văn bản dùng bút có *mực màu xanh*, không dùng các loại *mực dễ phai.*»",
      theNguon("Điều 13 · khoản 6"),
      theBinhLuan("Anh chị ký bút máy hay bút bi? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Ký văn bản bằng bút bi xanh, có sai quy định không?", chu: "Ký bằng *bút bi xanh*: *sai hay đúng?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "qd", cau: [
      { loi: "Điều mười ba Nghị định ba mươi, chỉ nêu hai điều kiện.", chu: "Điều 13 khoản 6: chỉ *hai điều kiện*" },
      { loi: "Bút có mực màu xanh, và không dùng loại mực dễ phai.", chu: "① mực *màu xanh* · ② *không dễ phai*" },
      { loi: "Nghị định không nói bút máy hay bút bi, còn mực đen, mực đỏ, là sai.", chu: "Không nói *bút máy hay bút bi* · mực *đen, đỏ* → sai", nghiSau: 0.3 },
    ]},
    { canh: "mau", cau: [
      { loi: "Còn dấu, phải dùng đúng mực dấu màu đỏ.", chu: "Dấu: mực *màu đỏ* (Điều 33)" },
      { loi: "Văn bản điện tử, hình chữ ký số màu xanh, hình con dấu màu đỏ.", chu: "Điện tử: chữ ký số *xanh* · hình dấu *đỏ*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Đối với văn bản giấy, khi ký văn bản dùng bút có mực màu xanh, không dùng các loại mực dễ phai.", chu: "Ký văn bản giấy: *mực xanh* · *không dễ phai*" },
      nguonCau("Điều mười ba", "Điều 13 · khoản 6"),
      binhLuanCau("Anh chị thường ký bằng bút gì, bình luận cho mọi người cùng biết.", "Anh chị ký bằng bút gì? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;gap:30px}
.x-ab .o{flex:1;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 20px 30px;text-align:center}
.x-ab .o b{display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:18px 0 0;font-size:42px;font-weight:900;line-height:1.25;color:#0f172a}
.x-ab .o small{display:block;margin-top:10px;font-size:30px;font-weight:800;color:#64748b}
.x-dk{padding:26px 32px}
.x-dk h4{margin:0 0 14px;font-size:30px;font-weight:900;color:#64748b;letter-spacing:.04em}
.x-dk div{display:flex;align-items:center;gap:20px;font-size:46px;font-weight:900;color:#0f172a;padding:10px 0}
.x-dk i{flex:none;font-style:normal;display:inline-flex;align-items:center;justify-content:center;width:64px;height:64px;border-radius:50%;background:#15803d;color:#fff;font-size:36px}
.x-but{padding:22px 30px;display:flex;justify-content:space-around;align-items:center}
.x-but span{display:flex;flex-direction:column;align-items:center;gap:8px;font-size:32px;font-weight:900;color:#334155}
.x-but span i{display:block;width:150px;height:22px;border-radius:11px}
.x-but .xa i{background:#1d4ed8}.x-but .de i{background:#0f172a}.x-but .do i{background:#dc2626}
.x-but .de,.x-but .do{color:#b91c1c}
.x-but .de em,.x-but .do em,.x-but .xa em{font-style:normal;font-size:26px}
.x-m{display:flex;align-items:center;gap:24px;padding:24px 32px}
.x-m .cham{flex:none;width:86px;height:86px;border-radius:50%}
.x-m .tx{font-size:44px;font-weight:900;color:#0f172a;line-height:1.2}
.x-m .tx small{display:block;margin-top:6px;font-size:28px;font-weight:700;color:#64748b}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Ký bằng bút bi xanh —<br>*có sai không?*`, { tren: 262, co: 70, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>SAI</p><small>phải dùng bút máy</small></div>
    <div class="o b"><b>B</b><p>ĐƯỢC</p><small>miễn mực xanh, không phai</small></div>
  </div>
  ${nhanBinhLuan("c1k0", 1060)}`,
      ve: veCC,
    },

    "qd": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Điều 13 khoản 6")}
  ${the(`<div class="x-dk"><h4>KÝ VĂN BẢN GIẤY · HAI ĐIỀU KIỆN</h4><div data-k="lop" data-hien="c1k0" style="opacity:0"><i>1</i>Bút có mực màu xanh</div><div data-k="lop" data-hien="c1k1" style="opacity:0"><i>2</i>Không dùng mực dễ phai</div></div>`, { tren: 330, hien: "c0k0" })}
  ${the(`<div class="x-but"><span class="xa"><i></i>xanh <em>✓</em></span><span class="de"><i></i>đen <em>✗</em></span><span class="do"><i></i>đỏ <em>✗</em></span></div>`, { tren: 680, hien: "c2k1" })}
  ${dau("KHÔNG NÓI LOẠI BÚT", "c2k0+0.3", { tren: 920, co: 60 })}`,
    },

    "mau": {
      khai: true,
      html: () => `
  ${chipKhai("🎨 Màu theo Nghị định 30")}
  ${the(`<div class="x-m"><span class="cham" style="background:#1d4ed8"></span><div class="tx">Chữ ký trên giấy: mực xanh<small>Điều 13 khoản 6</small></div></div>`, { tren: 330, hien: "c0k0" })}
  ${the(`<div class="x-m"><span class="cham" style="background:#dc2626"></span><div class="tx">Dấu: mực dấu màu đỏ<small>Điều 33 khoản 1</small></div></div>`, { tren: 520, hien: "c0k0+0.3" })}
  ${the(`<div class="x-m"><span class="cham" style="background:linear-gradient(90deg,#1d4ed8 50%,#dc2626 50%)"></span><div class="tx">Văn bản điện tử: chữ ký số xanh, hình dấu đỏ<small>Phụ lục I · mục II.7d, II.8a</small></div></div>`, { tren: 710, hien: "c1k0" })}`,
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
    ["02a-dk", lucNode(tatCa, "qd", "c1e")],
    ["02b-but", lucNode(tatCa, "qd", "c2e+0.2")],
    ["03a-mau", lucNode(tatCa, "mau", "c1e")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
