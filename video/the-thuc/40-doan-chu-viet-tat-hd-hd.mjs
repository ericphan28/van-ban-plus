// ============================================================================
// THỂ THỨC 40 — ĐOÁN CHỮ VIẾT TẮT: HƯỚNG DẪN, ĐỀ ÁN, BẢN TRÍCH SAO, BẢN GHI NHỚ? HD HAY HĐ, ĐA HAY DA — THEO NĐ 30/2020
//
// Khuôn ĐỐ VUI (biến thể khuôn tranh luận clip 35): móc 4 câu đố + nhãn «ghi đáp án vào bình luận» → lật đáp án từng dòng →
// cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận số câu đúng.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc3.md, mục I «Bảng chữ viết tắt tên loại văn bản hành chính và bản sao văn bản»:
//   Hướng dẫn HD · Hợp đồng HĐ · Đề án ĐA · Dự án DA · Bản trích sao TrS · Bản ghi nhớ BGN · Thông cáo TC · Thông báo TB
//   ✔ chữ Latin chỉ trên hình; lời đọc tả bằng «nét gạch», «ba chữ», «e rờ viết thường», «chữ bê» (giọng Bắc: «rờ» → «e rờ»).
//   ✔ không trùng clip 07 (TTr, CTr, QyĐ).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan } from "./loat.mjs";

/** một dòng đố: tên loại · ô đáp án (hiện lúc `lat`) · ghi chú bẫy */
const do_ = (stt, ten, dap, bay, lat, tren) =>
  the(`<div class="x-q"><b>${stt}</b><span class="tn">${ten}</span><span class="o"><i>?</i>${lat ? `<em data-k="lop" data-hien="${lat}" style="opacity:0">${dap}</em>` : ""}</span>${bay ? `<small data-k="lop" data-hien="${lat}" style="opacity:0">${bay}</small>` : ""}</div>`,
    { tren, lop: "x-the" });

export default {
  ten: "40-doan-chu-viet-tat-hd-hd",
  tieuDe: "Thể thức 40 · Đoán chữ viết tắt: Hướng dẫn, Đề án, Bản trích sao, Bản ghi nhớ? HD hay HĐ, ĐA hay DA — NĐ 30/2020",
  bia: { dong: ["HD hay HĐ?", "Đoán 4 chữ"], vai: "Nghị định 30 · Phụ lục III" },
  ket: {
    the: [
      "<b style=\"display:block;font-size:48px;line-height:1.4\">Hướng dẫn *HD* · Hợp đồng *HĐ*<br>Đề án *ĐA* · Dự án *DA*<br>Trích sao *TrS* · Ghi nhớ *BGN*</b>",
      theNguon("Phụ lục III · mục I · bảng chữ viết tắt"),
      theBinhLuan("Anh chị đúng mấy câu? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Bốn tên loại văn bản, viết tắt thế nào, anh chị đoán được mấy câu?", chu: "*4 chữ viết tắt* — anh chị *đoán đúng mấy?*" },
      { loi: "Ghi đáp án vào bình luận trước, rồi xem tiếp.", chu: "*Ghi đáp án* vào bình luận → rồi xem tiếp", nghiSau: 0.3 },
    ]},
    { canh: "dap", cau: [
      { loi: "Hướng dẫn và hợp đồng, chỉ khác nhau một nét gạch ngang.", chu: "Hướng dẫn *HD* · Hợp đồng *HĐ*" },
      { loi: "Đề án và dự án, cũng chỉ khác nhau một nét gạch.", chu: "Đề án *ĐA* · Dự án *DA*" },
      { loi: "Bản trích sao, không phải hai chữ, mà ba chữ, có e rờ viết thường.", chu: "Bản trích sao *TrS* — không phải TS" },
      { loi: "Bản ghi nhớ, cũng ba chữ, chữ bê đứng đầu.", chu: "Bản ghi nhớ *BGN* — không phải GN" },
      { loi: "Bảng đầy đủ, nằm ở Phụ lục ba Nghị định ba mươi.", chu: "Bảng đầy đủ: *Phụ lục III* NĐ 30", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Hướng dẫn, hợp đồng, đề án, dự án, trích sao, ghi nhớ, sáu chữ viết tắt dễ nhầm.", chu: "6 chữ viết tắt *dễ nhầm*" },
      nguonCau("Phụ lục ba", "Phụ lục III · mục I"),
      binhLuanCau("Anh chị đoán đúng mấy câu, bình luận cho mọi người cùng biết.", "Anh chị đúng mấy câu? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-the{padding:0}
.x-q{position:relative;display:flex;align-items:center;gap:22px;padding:22px 28px}
.x-q b{flex:none;display:inline-flex;align-items:center;justify-content:center;width:70px;height:70px;border-radius:50%;background:var(--mau-chinh);color:#fff;font-size:38px;font-weight:900}
.x-q .tn{flex:1;font-size:46px;font-weight:900;color:#0f172a}
.x-q .o{position:relative;flex:none;width:200px;height:84px;border-radius:18px;background:#f1f5f9;border:4px dashed #cbd5e1;display:flex;align-items:center;justify-content:center}
.x-q .o i{font-style:normal;font-size:50px;font-weight:900;color:#94a3b8}
.x-q .o em{position:absolute;inset:-4px;display:flex;align-items:center;justify-content:center;border-radius:18px;background:#15803d;color:#fff;font-style:normal;font-family:"Times New Roman",serif;font-size:56px;font-weight:700}
.x-q small{position:absolute;right:30px;bottom:-30px;font-size:26px;font-weight:900;color:#fff;background:#dc2626;border-radius:999px;padding:4px 16px;z-index:2}
.x-ghi{position:absolute;left:70px;width:940px;box-sizing:border-box;text-align:center;font-size:38px;font-weight:800;color:#fff;
  background:#0f172a;border-radius:999px;padding:18px 28px;box-shadow:0 14px 28px rgba(15,23,42,.22)}
.x-ghi b{color:#fde68a}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Đoán chữ viết tắt —<br>*đúng mấy câu?*`, { tren: 262, co: 72, gach: "c0k2" })}
  ${do_(1, "Hướng dẫn", "HD", "", "", 610)}
  ${do_(2, "Đề án", "ĐA", "", "", 750)}
  ${do_(3, "Bản trích sao", "TrS", "", "", 890)}
  ${do_(4, "Bản ghi nhớ", "BGN", "", "", 1030)}
  <div class="x-ghi" data-k="lop" data-hien="c1k0" style="opacity:0;top:1180px">💬 Ghi đáp án vào <b>bình luận</b> trước</div>`,
      ve: veCC,
    },

    "dap": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục III · bảng chữ viết tắt")}
  ${do_(1, "Hướng dẫn", "HD", "Hợp đồng: HĐ", "c0k0+0.4", 330)}
  ${do_(2, "Đề án", "ĐA", "Dự án: DA", "c1k0+0.4", 500)}
  ${do_(3, "Bản trích sao", "TrS", "không phải TS", "c2k0+0.4", 670)}
  ${do_(4, "Bản ghi nhớ", "BGN", "không phải GN", "c3k0+0.4", 840)}`,
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
    ["01b-ghi", lucNode(tatCa, "moc", "c1k1")],
    ["02a-hai", lucNode(tatCa, "dap", "c1e")],
    ["02b-het", lucNode(tatCa, "dap", "c4k0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
