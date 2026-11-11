// ============================================================================
// THỂ THỨC 21 — TRÌNH BÀY NỘI DUNG VĂN BẢN: THỤT ĐẦU DÒNG, GIÃN DÒNG, GIÃN ĐOẠN BAO NHIÊU? (Nghị định 30/2020/NĐ-CP, Phụ lục I, II.6 điểm đ, e)
//
// Khuôn «THÔNG SỐ CẦN NHỚ» — nghe là làm được ngay trong Word: móc ba câu hỏi (thụt dòng, giãn dòng, giãn đoạn) → 5 hàng số liệu hiện theo
// lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn về cách trình bày «Điều».
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 66–71):
//   «Thứ tự các điểm trong mỗi khoản dùng các chữ cái tiếng Việt theo thứ tự bảng chữ cái tiếng Việt, sau có dấu đóng ngoặc đơn, bằng chữ
//   in thường, cỡ chữ bằng cỡ chữ của phần lời văn, kiểu chữ đứng.»
//   «e) Nội dung văn bản được trình bày bằng chữ in thường, được canh đều cả hai lề, kiểu chữ đứng; cỡ chữ từ 13 đến 14; khi xuống dòng, chữ
//   đầu dòng lùi vào 1 cm hoặc 1,27 cm; khoảng cách giữa các đoạn văn tối thiểu là 6pt; khoảng cách giữa các dòng tối thiểu là dòng đơn, tối
//   đa là 1,5 lines.»
//   «Từ “Điều”, số thứ tự và tiêu đề của điều được trình bày bằng chữ in thường, lùi đầu dòng 1 cm hoặc 1,27 cm. Số thứ tự của điều dùng chữ
//   số Ả Rập, sau số thứ tự có dấu chấm (.); cỡ chữ bằng cỡ chữ của phần lời văn, kiểu chữ đứng, đậm.»
//   ✔ chữ in thường · canh đều hai lề · đứng · cỡ 13–14 · lùi 1 cm hoặc 1,27 cm · giữa các đoạn tối thiểu 6pt · giữa các dòng tối thiểu dòng đơn,
//     tối đa 1,5 lines · điểm dùng chữ cái tiếng Việt + dấu «)» — «sau chữ d là chữ đ» đúng với chính cách đánh a) b) c) d) đ) e) g) của mục II.6
//     (không nói «bỏ chữ f» vì Nghị định không nêu)
//   ✔ thẻ màn kết: «Điều» — lùi đầu dòng, số Ả Rập, sau số có dấu chấm, kiểu chữ đứng, đậm
//   ✔ lời đọc: «pt» đọc «pi ti»; «lines» đọc «dòng»; «cm» đọc «xen ti mét» (NOI-DUNG.md mục 3)
//   ✘ KHÔNG nói phần mềm tự căn lề / giãn dòng đúng (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (gt, nhan, hien, tren, lop = "") =>
  the(`<div class="c21-h ${lop}"><div class="gt">${gt}</div><div class="nh">${nhan}</div></div>`, { tren, hien, lop: "c21-the" });

export default {
  ten: "21-trinh-bay-noi-dung-thut-dong-gian-dong",
  tieuDe: "Thể thức 21 · Thụt đầu dòng, giãn dòng, giãn đoạn bao nhiêu? Trình bày nội dung văn bản theo NĐ 30/2020",
  bia: { dong: ["Thụt dòng, giãn", "dòng: bao nhiêu?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Từ “Điều”, số thứ tự và tiêu đề của điều được trình bày bằng chữ in thường, *lùi đầu dòng 1 cm hoặc 1,27 cm*. Số thứ tự của điều dùng chữ số Ả Rập, *sau số thứ tự có dấu chấm (.)*; … kiểu chữ đứng, đậm.»",
      theNguon("Phụ lục I · mục II.6 · điểm đ"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Soạn văn bản trên máy tính, thụt đầu dòng bao nhiêu, giãn dòng bao nhiêu, giãn đoạn bao nhiêu?", chu: "Soạn văn bản — *thụt đầu dòng* · *giãn dòng* · *giãn đoạn* bao nhiêu?" },
      { loi: "Nghị định ba mươi có số cụ thể.", chu: "Nghị định 30 có *số cụ thể*", nghiSau: 0.2 },
    ]},
    { canh: "thong-so", cau: [
      { loi: "Nội dung văn bản, chữ in thường, canh đều hai lề, cỡ chữ mười ba đến mười bốn.", chu: "Nội dung: chữ thường · *canh đều hai lề* · cỡ *13–14*" },
      { loi: "Xuống dòng, chữ đầu dòng lùi vào, một xen ti mét, hoặc một phẩy hai mươi bảy xen ti mét.", chu: "Xuống dòng: lùi vào *1 cm* hoặc *1,27 cm*" },
      { loi: "Khoảng cách giữa các đoạn văn, tối thiểu là sáu pi ti.", chu: "Giữa các đoạn văn: tối thiểu *6 pt*" },
      { loi: "Khoảng cách giữa các dòng, tối thiểu là dòng đơn, tối đa là một phẩy năm dòng.", chu: "Giữa các dòng: tối thiểu *dòng đơn* · tối đa *1,5 lines*" },
      { loi: "Điểm trong khoản, đánh chữ cái tiếng Việt, sau có dấu đóng ngoặc, và sau chữ dê, là chữ đê.", chu: "Điểm: chữ cái tiếng Việt + *dấu )* — sau *d* là *đ*", nghiSau: 0.1 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.1 },
    ]},
    canhKet([
      { loi: "Từ Điều, số thứ tự và tiêu đề, lùi đầu dòng, sau số thứ tự có dấu chấm, kiểu chữ đứng, đậm.", chu: "«Điều»: *lùi đầu dòng* · số Ả Rập + *dấu chấm* · đứng · đậm" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.6 · điểm đ"),
    ]),
  ],

  css: CSS_CC + `
.c21-q{display:flex;flex-direction:column;gap:18px;padding:40px 30px 46px}
.c21-q div{display:flex;align-items:center;justify-content:space-between;font-size:48px;font-weight:900;color:#0f172a;background:#f1f5f9;border-radius:18px;padding:12px 28px}
.c21-q i{font-style:normal;color:#b45309;background:#fef3c7;border-radius:14px;padding:0 22px}
.c21-the{padding:0}
.c21-h{display:flex;align-items:center;gap:24px;padding:20px 30px}
.c21-h .gt{flex:none;width:400px;font-size:66px;font-weight:900;color:#1d4ed8;font-family:"Times New Roman",serif;line-height:1}
.c21-h .nh{flex:1;font-size:44px;font-weight:900;color:#0f172a;line-height:1.2}
.c21-h.dai .gt{font-size:50px}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Trình bày nội dung —<br>*thụt, giãn bao nhiêu?*`, { tren: 262, co: 72, gach: "c0k3" })}
  ${the(`<div class="c21-q"><div>Thụt đầu dòng <i>?</i></div><div>Giãn dòng <i>?</i></div><div>Giãn đoạn <i>?</i></div></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "thong-so": {
      khai: true,
      html: () => `
  ${chipKhai("📐 Thông số nội dung văn bản")}
  ${hang("13 – 14", "Cỡ chữ · chữ in thường · canh đều hai lề", "c0k3", 330)}
  ${hang("1 · 1,27 cm", "Lùi đầu dòng khi xuống dòng", "c1k2", 520)}
  ${hang("≥ 6 pt", "Khoảng cách giữa các đoạn văn", "c2k1", 710)}
  ${hang("1,0 → 1,5", "Khoảng cách giữa các dòng (dòng đơn → 1,5 lines)", "c3k1", 900)}
  ${hang("a) b) c) d) đ)", "Điểm: chữ cái tiếng Việt, sau có dấu đóng ngoặc", "c4k1", 1090, "dai")}`,
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
    ["02a-lui", lucNode(tatCa, "thong-so", "c1k3w3")],
    ["02b-doan", lucNode(tatCa, "thong-so", "c2k1w3")],
    ["02c-dong", lucNode(tatCa, "thong-so", "c3k2w4")],
    ["02d-diem", lucNode(tatCa, "thong-so", "c4k3w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k3"),
};
