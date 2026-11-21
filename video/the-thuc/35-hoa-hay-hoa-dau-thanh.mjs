// ============================================================================
// THỂ THỨC 35 — «HÒA» HAY «HOÀ», «ỦY» HAY «UỶ»? CHÍNH NGHỊ ĐỊNH 30 VIẾT CẢ HAI KIỂU
//
// Khuôn TRANH LUẬN (từ clip 35, chủ dự án 22/11/2026: kêu gọi bình luận + đăng ký bản web): móc A/B + nhãn «bình luận
// trước khi xem tiếp» → bằng chứng nguyên văn → kết luận → cảnh phần mềm bản web (theWeb, BẮT BUỘC) → thẻ kết + câu mời bình luận.
//
// SOÁT với bản gốc docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/van-ban-goc/30.2020.NĐ-CP/*.md (chuyển từ .docx):
//   Điều 13 khoản 3: «… người đứng đầu cơ quan, tổ chức có thể ủy quyền cho người đứng đầu … ký thừa ủy quyền …» (dấu trên u)
//   Phụ lục I (Phần I mục II.7b): «Trường hợp ký thừa uỷ quyền thì phải ghi chữ viết tắt "TUQ."…» (dấu trên y)
//   Điều 4 khoản 2 điểm c: «Văn bản đến có các mức độ khẩn: "Hỏa tốc", "Thượng khẩn" và "Khẩn"…» (dấu trên o)
//   Phụ lục I (Phần I mục III.2): «… xác định độ khẩn theo các mức sau: hoả tốc, thượng khẩn, khẩn …» (dấu trên a)
//   ✔ grep «dấu thanh», «chính tả» trong Nghị định + 6 phụ lục: 0 kết quả → «không có quy định về chỗ đặt dấu thanh».
//   ✔ «chọn một kiểu, dùng thống nhất» ghi rõ là LỜI KHUYÊN trên hình, không gán cho Nghị định.
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

const dong = (nguon, chu, ghi, hien, tren) =>
  the(`<div class="x-d"><span class="ng">${nguon}</span><span class="ch">${chu}</span><span class="gh">${ghi}</span></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "35-hoa-hay-hoa-dau-thanh",
  tieuDe: "Thể thức 35 · «Hòa» hay «hoà», «ủy» hay «uỷ»? Chính Nghị định 30/2020 viết cả hai kiểu",
  bia: { dong: ["Hòa hay hoà?", "Dấu ở đâu?"], vai: "Nghị định 30 viết cả hai kiểu" },
  ket: {
    the: [
      "Điều 13: «… ký thừa *ủy* quyền …»<br>Phụ lục I: «… ký thừa *uỷ* quyền …»",
      theNguon("Điều 13 khoản 3 · Phụ lục I mục II.7"),
      theBinhLuan("Anh chị viết «hòa» hay «hoà»? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Hòa bình, dấu huyền đặt trên chữ o, hay trên chữ a?", chu: "«Hòa» hay «hoà»? *Dấu đặt ở đâu?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "ng", cau: [
      { loi: "Mở chính văn bản Nghị định ba mươi ra xem.", chu: "Mở *chính văn bản* Nghị định 30" },
      { loi: "Điều mười ba viết, ký thừa ủy quyền, dấu trên chữ u.", chu: "Điều 13: «ký thừa *ủy* quyền» — dấu trên *u*" },
      { loi: "Phụ lục một viết cùng cụm đó, nhưng dấu trên chữ y.", chu: "Phụ lục I: «ký thừa *uỷ* quyền» — dấu trên *y*" },
      { loi: "Điều bốn viết hỏa tốc dấu trên chữ o, Phụ lục một lại đặt trên chữ a.", chu: "Điều 4 «h*ỏ*a tốc» · Phụ lục I «ho*ả* tốc»", nghiSau: 0.3 },
    ]},
    { canh: "kl", cau: [
      { loi: "Nghị định ba mươi, không có quy định nào về chỗ đặt dấu thanh.", chu: "Nghị định 30 *không quy định* chỗ đặt dấu thanh" },
      { loi: "Lời khuyên, chọn một kiểu, và dùng thống nhất trong cả văn bản.", chu: "Lời khuyên: *chọn một kiểu* · *thống nhất* cả văn bản", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Cùng một cụm từ, Điều mười ba viết một kiểu, Phụ lục một viết kiểu khác.", chu: "Điều 13 «ủy» · Phụ lục I «uỷ»" },
      nguonCau("Điều mười ba và Phụ lục một", "Điều 13 · Phụ lục I"),
      binhLuanCau("Anh chị viết kiểu nào, bình luận cho mọi người cùng biết.", "Anh chị viết kiểu nào? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;gap:30px}
.x-ab .o{flex:1;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 20px 30px;text-align:center}
.x-ab .o b{display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:18px 0 0;font-family:"Times New Roman",serif;font-size:60px;font-weight:700;line-height:1.3;color:#0f172a}
.x-ab .o em{font-style:normal;color:#dc2626}
.x-ab .o small{display:block;margin-top:10px;font-size:30px;font-weight:800;color:#64748b}
.x-vb{padding:26px 34px;text-align:center;font-size:40px;font-weight:900;color:#0f172a;line-height:1.25}
.x-vb small{display:block;font-size:30px;font-weight:700;color:#64748b}
.x-the{padding:0}
.x-d{display:flex;align-items:center;gap:20px;padding:20px 26px}
.x-d .ng{flex:none;width:190px;text-align:center;font-size:32px;font-weight:900;color:#fff;background:#334155;border-radius:16px;padding:12px 6px}
.x-d .ch{flex:1;font-family:"Times New Roman",serif;font-size:50px;font-weight:700;color:#0f172a}
.x-d .ch em{font-style:normal;color:#dc2626;background:#fef3c7;border-radius:6px;padding:0 4px}
.x-d .gh{flex:none;font-size:30px;font-weight:800;color:#64748b}
.x-kl{padding:34px 38px;font-size:48px;font-weight:900;line-height:1.25;color:#0f172a;text-align:center}
.x-kl b{color:#15803d}
.x-kl small{display:block;margin-top:12px;font-size:30px;font-weight:700;color:#64748b}
.x-lk{padding:30px 38px;font-size:44px;font-weight:900;line-height:1.25;color:#0f172a;text-align:center;border:5px dashed #93c5fd}
.x-lk i{display:inline-block;font-style:normal;font-size:28px;font-weight:800;color:#fff;background:#1d4ed8;border-radius:999px;padding:6px 18px;margin-bottom:12px}
.x-lk b{color:var(--mau-chinh)}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}«Hòa» hay «hoà» —<br>*dấu đặt ở đâu?*`, { tren: 262, co: 70, gach: "c0k0" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>h<em>ò</em>a · <em>ủ</em>y<br>h<em>ỏ</em>a tốc</p><small>dấu trên chữ o, u</small></div>
    <div class="o b"><b>B</b><p>ho<em>à</em> · u<em>ỷ</em><br>ho<em>ả</em> tốc</p><small>dấu trên chữ a, y</small></div>
  </div>
  ${nhanBinhLuan("c1k0", 1110)}`,
      ve: veCC,
    },

    "ng": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Chính văn bản Nghị định 30")}
  ${the(`<div class="x-vb">Nghị định 30/2020/NĐ-CP<small>Về công tác văn thư · bản gốc</small></div>`, { tren: 300, hien: "c0k0" })}
  ${dong("Điều 13", "ký thừa <em>ủ</em>y quyền", "trên u", "c1k1", 500)}
  ${dong("Phụ lục I", "ký thừa u<em>ỷ</em> quyền", "trên y", "c2k0", 660)}
  ${dong("Điều 4", "H<em>ỏ</em>a tốc", "trên o", "c3k0", 820)}
  ${dong("Phụ lục I", "ho<em>ả</em> tốc", "trên a", "c3k1", 980)}
  ${dau("CẢ HAI KIỂU", "c3e-0.2", { tren: 1150, co: 74 })}`,
    },

    "kl": {
      khai: true,
      html: () => `
  ${chipKhai("✅ Kết luận")}
  ${the(`<div class="x-kl">Nghị định 30 <b>không có quy định</b><br>về chỗ đặt dấu thanh<small>tìm trong Nghị định và 6 phụ lục: không có</small></div>`, { tren: 380, hien: "c0k0" })}
  ${the(`<div class="x-lk"><i>LỜI KHUYÊN · không phải quy định</i><br>Chọn <b>một kiểu</b>,<br>dùng <b>thống nhất</b> cả văn bản</div>`, { tren: 760, hien: "c1k1" })}`,
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
    ["02a-dong4", lucNode(tatCa, "ng", "c3k1w2")],
    ["02b-dau", lucNode(tatCa, "ng", "c3e+0.3")],
    ["03a-kl", lucNode(tatCa, "kl", "c1k2")],
    ["04a-app", lucNode(tatCa, "app", "c0k3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
