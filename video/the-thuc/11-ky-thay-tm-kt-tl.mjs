// ============================================================================
// THỂ THỨC 11 — KÝ THAY: TM., Q., KT., TL., TUQ. KHÁC NHAU THẾ NÀO? (Nghị định 30/2020/NĐ-CP, Phụ lục I, II.7 b, đ)
//
// Khuôn PHÂN BIỆT: móc «năm cách ký, năm chữ viết tắt» → bảng 5 hàng hiện dần theo lời (chữ viết tắt + nghĩa + ví dụ giữ chỗ)
// → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn về cách trình bày chữ viết tắt quyền hạn.
// Lời đọc KHÔNG đọc tên chữ viết tắt (tránh Vbee đọc từng chữ cái); chữ viết tắt chỉ hiện trên hình.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 76–81, 90):
//   «b) Việc ghi quyền hạn của người ký được thực hiện như sau:
//   Trường hợp ký thay mặt tập thể thì phải ghi chữ viết tắt “TM.” vào trước tên tập thể lãnh đạo hoặc tên cơ quan, tổ chức.
//   Trường hợp được giao quyền cấp trưởng thì phải ghi chữ viết tắt “Q.” vào trước chức vụ của người đứng đầu cơ quan, tổ chức.
//   Trường hợp ký thay người đứng đầu cơ quan, tổ chức thì phải ghi chữ viết tắt “KT.” vào trước chức vụ của người đứng đầu.
//   Trường hợp cấp phó được giao phụ trách hoặc điều hành thì thực hiện ký như cấp phó ký thay cấp trưởng.
//   Trường hợp ký thừa lệnh thì phải ghi chữ viết tắt “TL.” vào trước chức vụ của người đứng đầu cơ quan, tổ chức.
//   Trường hợp ký thừa uỷ quyền thì phải ghi chữ viết tắt “TUQ.” vào trước chức vụ của người đứng đầu cơ quan, tổ chức.»
//   «đ) … các chữ viết tắt quyền hạn như: “TM.”, “Q.”, “KT.”, “TL.”, “TUQ.” và quyền hạn chức vụ của người ký được trình bày bằng
//   chữ in hoa, cỡ chữ từ 13 đến 14, kiểu chữ đứng, đậm.»
//   ✔ 5 chữ viết tắt + trường hợp dùng = đúng nguyên văn; «cấp phó phụ trách / điều hành ký như KT.» = nguyên văn
//   ✔ ví dụ «BAN A», «GIÁM ĐỐC», «PHÓ GIÁM ĐỐC» chỉ là giữ chỗ, ghi «ví dụ minh họa» trên hình; không tên người, không cơ quan thật
//   ✘ KHÔNG nói phần mềm tự ghi quyền hạn người ký (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (ab, nghia, vd, hien, tren, ghiChu = "") =>
  the(`<div class="c11-h"><div class="ab">${ab}</div><div class="nd"><b>${nghia}</b><span>${vd}</span>${ghiChu}</div></div>`, { tren, hien, lop: "c11-the" });

export default {
  ten: "11-ky-thay-tm-kt-tl",
  tieuDe: "Thể thức 11 · Ký thay: TM., Q., KT., TL., TUQ. khác nhau thế nào? Theo NĐ 30/2020",
  bia: { dong: ["5 cách ký thay", "5 chữ viết tắt"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«…các chữ viết tắt quyền hạn như: “TM.”, “Q.”, “KT.”, “TL.”, “TUQ.” và quyền hạn chức vụ của người ký được trình bày bằng *chữ in hoa, cỡ chữ từ 13 đến 14, kiểu chữ đứng, đậm.*»",
      theNguon("Phụ lục I · mục II.7 · điểm b, đ"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Năm cách ký, năm chữ viết tắt khác nhau, anh chị phân biệt được không?", chu: "5 cách ký — 5 chữ viết tắt — *phân biệt được không?*" },
      { loi: "Mỗi chữ, một trường hợp ký riêng.", chu: "Mỗi chữ *một trường hợp* ký riêng", nghiSau: 0.2 },
    ]},
    { canh: "ky", cau: [
      { loi: "Ký thay mặt tập thể, ghi chữ viết tắt trước tên tập thể lãnh đạo, hoặc tên cơ quan.", chu: "Thay mặt *tập thể* → trước tên tập thể hoặc tên cơ quan" },
      { loi: "Ký thay người đứng đầu, ghi chữ viết tắt trước chức vụ của người đứng đầu.", chu: "Thay *người đứng đầu* → trước chức vụ người đứng đầu" },
      { loi: "Cấp phó được giao phụ trách hoặc điều hành, cũng ký như cấp phó ký thay cấp trưởng.", chu: "Cấp phó *phụ trách, điều hành* → ký như *KT.*" },
      { loi: "Được giao quyền cấp trưởng, thì thêm chữ viết tắt trước chức vụ của người đứng đầu.", chu: "Được *giao quyền* cấp trưởng → *Q.* trước chức vụ" },
      { loi: "Ký thừa lệnh, và ký thừa ủy quyền, cũng ghi chữ viết tắt riêng trước chức vụ.", chu: "*Thừa lệnh* · *thừa ủy quyền* → chữ viết tắt riêng", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Các chữ viết tắt quyền hạn, trình bày bằng chữ in hoa, cỡ chữ mười ba đến mười bốn, kiểu chữ đứng, đậm.", chu: "…chữ viết tắt quyền hạn: chữ in hoa · cỡ 13–14 · đứng · đậm" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.7 · điểm b, đ"),
    ]),
  ],

  css: CSS_CC + `
.c11-q{display:flex;flex-wrap:wrap;justify-content:center;gap:22px 24px;padding:44px 24px 50px}
.c11-q i{font-style:normal;font-family:"Times New Roman",serif;font-size:76px;font-weight:700;color:#1d4ed8;background:#dbeafe;border-radius:20px;padding:6px 30px}
.c11-the{padding:0}
.c11-h{display:flex;align-items:center;gap:28px;padding:16px 26px}
.c11-h .ab{flex:none;width:240px;text-align:center;font-family:"Times New Roman",serif;font-size:86px;font-weight:700;color:#fff;background:#1d4ed8;border-radius:18px;padding:4px 0}
.c11-h .nd b{display:block;font-size:42px;color:#0f172a;line-height:1.15}
.c11-h .nd span{display:block;margin-top:6px;font-family:"Times New Roman",serif;font-size:36px;font-weight:700;color:#475569}
.c11-h .nd em{display:inline-block;margin-top:4px;font-style:normal;font-size:28px;font-weight:800;color:#92400e;background:#fef3c7;border-radius:10px;padding:2px 12px}
.c11-vd{position:absolute;left:70px;top:1168px;width:940px;text-align:center;font-size:26px;font-weight:700;color:#94a3b8}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Ký thay —<br>*5 cách, 5 chữ viết tắt*`, { tren: 262, co: 78, gach: "c0k1" })}
  ${the(`<div class="c11-q"><i>TM.</i><i>Q.</i><i>KT.</i><i>TL.</i><i>TUQ.</i></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ky": {
      khai: true,
      html: () => `
  ${chipKhai("✍️ Quyền hạn người ký")}
  ${hang("TM.", "Thay mặt tập thể", "TM. BAN A", "c0k0", 340)}
  ${hang("KT.", "Ký thay người đứng đầu", "KT. GIÁM ĐỐC · PHÓ GIÁM ĐỐC", "c1k0", 505,
    `<em data-k="lop" data-hien="c2k1" style="opacity:0">+ cấp phó phụ trách, điều hành: ký như KT.</em>`)}
  ${hang("Q.", "Được giao quyền cấp trưởng", "Q. GIÁM ĐỐC", "c3k0", 670)}
  ${hang("TL.", "Ký thừa lệnh", "TL. GIÁM ĐỐC", "c4k0", 835)}
  ${hang("TUQ.", "Ký thừa ủy quyền", "TUQ. GIÁM ĐỐC", "c4k1", 1000)}
  <div class="c11-vd" data-k="lop" data-hien="c4k1" style="opacity:0">ví dụ minh họa</div>`,
    },

    "app": {
      khai: true,
      html: () => `
  ${chipKhai("🤖 Phần mềm soạn văn bản")}
  ${theApp("c0k0", true)}`,
    },
  },

  mocThu: (tatCa) => [
    ["01a-0.4s", 0.4],
    ["02a-tm", lucNode(tatCa, "ky", "c0k2w1")],
    ["02b-kt", lucNode(tatCa, "ky", "c2k1w3")],
    ["02c-q", lucNode(tatCa, "ky", "c3k0w3")],
    ["02d-tl", lucNode(tatCa, "ky", "c4k2w2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
