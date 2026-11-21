// ============================================================================
// THỂ THỨC 46 — «SAO Y BẢN CHÍNH» HAY «SAO Y»? THỂ THỨC BẢN SAO SANG ĐỊNH DẠNG GIẤY, SỐ BẢN SAO THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 39): móc A/B + nhãn bình luận → thể thức bản sao → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc1.md, Phần II mục II, + Phuluc3.md:
//   II.1a: «Hình thức sao: “SAO Y” hoặc “SAO LỤC” hoặc “TRÍCH SAO”.»
//   II.1c: «Số, ký hiệu bản sao bao gồm số thứ tự đăng ký (được đánh chung cho các loại bản sao do cơ quan, tổ chức thực hiện) và chữ
//   viết tắt tên loại bản sao theo Bảng chữ viết tắt … tại Mục I Phụ lục III … Số được ghi bằng chữ số Ả Rập, bắt đầu liên tiếp từ
//   số 01 vào ngày 01 tháng 01 và kết thúc vào ngày 31 tháng 12 hàng năm.»
//   II.2a: «Các thành phần thể thức bản sao được trình bày trên cùng một tờ giấy (khổ A4), ngay sau phần cuối cùng của văn bản cần sao
//   dưới một đường kẻ nét liền, kéo dài hết chiều ngang của vùng trình bày văn bản.»
//   II.2b: «Các cụm từ “SAO Y”, “SAO LỤC”, “TRÍCH SAO” được trình bày bằng chữ in hoa, cỡ chữ từ 13 đến 14, kiểu chữ đứng, đậm.»
//   Phụ lục III mục I: Bản sao y SY · Bản sao lục SL · Bản trích sao TrS. Mẫu 3.1: «Số: ..3../4- …5…» (số / ký hiệu bản sao - viết tắt tên cơ quan sao).
//   ✔ không nói «SAO Y BẢN CHÍNH» bị cấm — chỉ nói Nghị định 30 ghi đúng ba cụm. «UBND», «05», «XÃ A» = ví dụ minh họa.
//   ✔ không trùng clip 20 (định nghĩa sao y / sao lục / trích sao, giá trị pháp lý, thẩm quyền).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

export default {
  ten: "46-sao-y-ban-chinh-hay-sao-y",
  tieuDe: "Thể thức 46 · «SAO Y BẢN CHÍNH» hay «SAO Y»? Thể thức bản sao, số bản sao theo NĐ 30/2020",
  bia: { dong: ["SAO Y BẢN CHÍNH", "hay SAO Y?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Hình thức sao: *“SAO Y”* hoặc *“SAO LỤC”* hoặc *“TRÍCH SAO”*.»",
      theNguon("Phụ lục I · Phần II · mục II.1a"),
      theBinhLuan("Cơ quan anh chị còn dùng dấu «SAO Y BẢN CHÍNH»? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Bản sao văn bản, ghi sao y bản chính, hay chỉ ghi sao y?", chu: "«SAO Y BẢN CHÍNH» hay «SAO Y»? — *A hay B?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "sao", cau: [
      { loi: "Nghị định ba mươi, chỉ ghi ba hình thức, sao y, sao lục, trích sao.", chu: "Chỉ 3 hình thức: *SAO Y* · *SAO LỤC* · *TRÍCH SAO*" },
      { loi: "In hoa, cỡ chữ mười ba đến mười bốn, đứng, đậm.", chu: "*In hoa* · cỡ *13–14* · *đứng, đậm*" },
      { loi: "Phần sao, trình bày ngay sau cuối văn bản, dưới một đường kẻ nét liền.", chu: "Ngay sau cuối văn bản · *dưới đường kẻ nét liền*" },
      { loi: "Số bản sao đánh chung, cho các loại bản sao, bắt đầu lại từ số không một mỗi năm.", chu: "Số bản sao *đánh chung* các loại · *từ 01 mỗi năm*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Hình thức sao, sao y, hoặc sao lục, hoặc trích sao.", chu: "Hình thức sao: *SAO Y* hoặc *SAO LỤC* hoặc *TRÍCH SAO*" },
      nguonCau("Phụ lục một", "Phụ lục I · Phần II · mục II.1a"),
      binhLuanCau("Cơ quan anh chị còn dùng dấu sao y bản chính không, bình luận cho mọi người cùng biết.", "Còn dùng dấu «SAO Y BẢN CHÍNH»? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;flex-direction:column;gap:26px}
.x-ab .o{display:flex;align-items:center;gap:26px;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 30px}
.x-ab .o b{flex:none;display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:0;font-family:"Times New Roman",serif;font-size:62px;font-weight:700;color:#0f172a}
.x-the{padding:0}
.x-ba{display:flex;gap:16px;justify-content:center;padding:26px 20px}
.x-ba span{font-family:"Times New Roman",serif;font-size:50px;font-weight:700;color:#15803d;background:#dcfce7;border-radius:10px;padding:6px 16px}
.x-ba s{font-family:"Times New Roman",serif;font-size:40px;font-weight:700;color:#b91c1c;text-decoration-thickness:4px}
.x-to{padding:30px 40px 34px;font-family:"Times New Roman",serif;color:#0f172a}
.x-to .vb{font-size:30px;line-height:1.5;color:#94a3b8}
.x-to .ke{height:0;border-top:4px solid #0f172a;margin:14px 0 22px}
.x-to .hang{display:flex;justify-content:space-between;align-items:flex-start}
.x-to .tr{text-align:center;font-size:32px}
.x-to .tr b{display:block;font-size:34px}
.x-to .sy{font-size:42px;font-weight:700;background:#fef3c7;border-radius:8px;padding:2px 10px}
.x-to .so{display:inline-block;margin-top:8px;font-size:34px;background:#dcfce7;border-radius:8px;padding:0 8px}
.x-to small{display:block;margin-top:16px;font-family:system-ui,sans-serif;font-size:26px;font-weight:800;color:#94a3b8;text-align:center}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Bản sao văn bản —<br>*ghi chữ gì?*`, { tren: 262, co: 72, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>SAO Y BẢN CHÍNH</p></div>
    <div class="o b"><b>B</b><p>SAO Y</p></div>
  </div>
  ${nhanBinhLuan("c1k0", 1060)}`,
      ve: veCC,
    },

    "sao": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục I · Phần II · mục II")}
  ${the(`<div class="x-ba"><span>SAO Y</span><span>SAO LỤC</span><span>TRÍCH SAO</span></div><div class="x-ba" style="padding-top:0"><s>SAO Y BẢN CHÍNH</s></div>`, { tren: 320, hien: "c0k1" })}
  ${the(`<div class="x-to">
    <div class="vb">… phần cuối văn bản cần sao …<br>Nơi nhận: …</div>
    <div class="ke" data-k="lop" data-hien="c2k1" style="opacity:0"></div>
    <div class="hang" data-k="lop" data-hien="c2k1" style="opacity:0">
      <div class="tr"><b>ỦY BAN NHÂN DÂN XÃ A</b><span class="so" data-k="lop" data-hien="c3k0" style="opacity:0">Số: 05/SY-UBND</span></div>
      <div class="tr"><span class="sy" data-k="lop" data-hien="c1k0" style="opacity:0">SAO Y</span></div>
    </div>
    <small>ví dụ minh họa · Mẫu 3.1 Phụ lục III</small></div>`, { tren: 590, hien: "c1k0" })}
  ${dau("B ĐÚNG", "c0e-0.1", { tren: 1110, co: 76, lop: "xanh" })}`,
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
    ["02a-ba", lucNode(tatCa, "sao", "c0e+0.2")],
    ["02b-het", lucNode(tatCa, "sao", "c3e+0.2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
