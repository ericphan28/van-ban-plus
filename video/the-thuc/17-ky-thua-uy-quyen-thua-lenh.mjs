// ============================================================================
// THỂ THỨC 17 — KÝ THỪA ỦY QUYỀN VÀ KÝ THỪA LỆNH: AI ĐƯỢC GIAO LẠI? (Nghị định 30/2020/NĐ-CP, Điều 13 khoản 3, 4, 6)
//
// Khuôn HỎI – ĐÁP CÓ BẢNG ĐÚNG/SAI: móc «người được ủy quyền có ủy quyền lại được không?» → danh sách điều kiện ký thừa ủy quyền
// (✓ ✓ ✓ ✓ rồi ✗ ủy quyền lại) → ký thừa lệnh khác thế nào (giao lại cho cấp phó; ghi trong quy chế) → mực xanh → cảnh phần mềm +
// Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn «dùng thể thức và dấu / chữ ký số của cơ quan ủy quyền».
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md dòng 194–204):
//   «3. Trong trường hợp đặc biệt, người đứng đầu cơ quan, tổ chức có thể ủy quyền cho người đứng đầu cơ quan, tổ chức, đơn vị thuộc
//   cơ cấu tổ chức của mình ký thừa ủy quyền một số văn bản mà mình phải ký. Việc giao ký thừa ủy quyền phải được thực hiện bằng văn
//   bản, giới hạn thời gian và nội dung được ủy quyền. Người được ký thừa ủy quyền không được ủy quyền lại cho người khác ký. Văn bản
//   ký thừa ủy quyền được thực hiện theo thể thức và đóng dấu hoặc ký số của cơ quan, tổ chức ủy quyền.
//   4. Người đứng đầu cơ quan, tổ chức có thể giao người đứng đầu đơn vị thuộc cơ quan, tổ chức ký thừa lệnh một số loại văn bản.
//   Người được ký thừa lệnh được giao lại cho cấp phó ký thay. Việc giao ký thừa lệnh phải được quy định cụ thể trong quy chế làm việc
//   hoặc quy chế công tác văn thư của cơ quan, tổ chức.
//   6. Đối với văn bản giấy, khi ký văn bản dùng bút có mực màu xanh, không dùng các loại mực dễ phai.»
//   ✔ 5 điều kiện ký thừa ủy quyền (đặc biệt · bằng văn bản · giới hạn thời gian · giới hạn nội dung · KHÔNG ủy quyền lại)
//   ✔ thừa lệnh: giao lại cho cấp phó ký thay · giao ký thừa lệnh phải ghi trong quy chế làm việc / quy chế công tác văn thư
//   ✔ mực xanh, không dùng mực dễ phai (văn bản giấy) · ✔ thẻ màn kết: thể thức + dấu / ký số của cơ quan ỦY QUYỀN
//   ✘ KHÔNG nói phần mềm tự kiểm tra thẩm quyền ký (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, veCC, theApp } from "./loat.mjs";

/** một hàng: nhãn màu + câu + ✓/✗ */
const hang = (nhan, mau, chu, ok, hien, tren) =>
  the(`<div class="c17-h"><span class="nh ${mau}">${nhan}</span><span class="tx">${chu}</span><span class="dk ${ok ? "ok" : "no"}">${ok ? "✓" : "✗"}</span></div>`,
    { tren, hien, lop: "c17-the" });

export default {
  ten: "17-ky-thua-uy-quyen-thua-lenh",
  tieuDe: "Thể thức 17 · Ký thừa ủy quyền: có ủy quyền lại được không? Ký thừa lệnh khác thế nào? Theo NĐ 30/2020",
  bia: { dong: ["Ký thừa ủy quyền:", "ủy quyền lại?"], vai: "Nghị định 30 · Điều 13" },
  ket: {
    the: [
      "«Văn bản ký thừa ủy quyền được thực hiện theo *thể thức và đóng dấu hoặc ký số của cơ quan, tổ chức ủy quyền.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 13 · khoản 3"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Ký thừa ủy quyền, người được ủy quyền, có ủy quyền lại cho người khác được không?", chu: "Ký thừa ủy quyền — người được ủy quyền *có ủy quyền lại được không?*" },
      { loi: "Còn ký thừa lệnh, thì sao?", chu: "Còn *ký thừa lệnh* thì sao?", nghiSau: 0.2 },
    ]},
    { canh: "ky", cau: [
      { loi: "Ký thừa ủy quyền, chỉ trong trường hợp đặc biệt, và phải giao bằng văn bản.", chu: "Thừa ủy quyền: *trường hợp đặc biệt* · giao *bằng văn bản*" },
      { loi: "Văn bản ủy quyền phải giới hạn thời gian, và nội dung được ủy quyền.", chu: "Giới hạn *thời gian* · giới hạn *nội dung*" },
      { loi: "Người được ủy quyền, không được ủy quyền lại cho người khác ký.", chu: "Người được ủy quyền: *không được ủy quyền lại*" },
      { loi: "Còn ký thừa lệnh, thì người ký được giao lại cho cấp phó ký thay.", chu: "Thừa lệnh: *được giao lại cho cấp phó* ký thay" },
      { loi: "Việc giao ký thừa lệnh, phải quy định trong quy chế làm việc, hoặc quy chế công tác văn thư.", chu: "Giao ký thừa lệnh: ghi trong *quy chế làm việc* hoặc *quy chế công tác văn thư*" },
      { loi: "Ký văn bản giấy, dùng bút mực màu xanh, không dùng mực dễ phai.", chu: "Ký văn bản giấy: *bút mực xanh* · *không dùng mực dễ phai*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Văn bản ký thừa ủy quyền, dùng thể thức, và đóng dấu hoặc ký số của cơ quan ủy quyền.", chu: "Ký thừa ủy quyền → thể thức + *dấu hoặc chữ ký số của cơ quan ủy quyền*" },
      nguonCau("Điều mười ba", "Điều 13 · khoản 3"),
    ]),
  ],

  css: CSS_CC + `
.c17-q{display:flex;flex-direction:column;gap:18px;padding:40px 30px 46px}
.c17-q div{display:flex;align-items:center;gap:20px;font-size:44px;font-weight:900;color:#0f172a}
.c17-q b{flex:none;font-family:"Times New Roman",serif;font-size:50px;color:#fff;background:#1d4ed8;border-radius:16px;padding:4px 22px}
.c17-q i{font-style:normal;font-size:50px;font-weight:900;color:#b45309;background:#fef3c7;border-radius:14px;padding:0 20px}
.c17-the{padding:0}
.c17-h{display:flex;align-items:center;gap:22px;padding:16px 26px}
.c17-h .nh{flex:none;width:200px;text-align:center;font-size:34px;font-weight:900;color:#fff;border-radius:14px;padding:8px 0;line-height:1.1}
.c17-h .nh.u{background:#1d4ed8}.c17-h .nh.l{background:#7c3aed}.c17-h .nh.g{background:#64748b}
.c17-h .tx{flex:1;font-size:44px;font-weight:800;color:#0f172a;line-height:1.15}
.c17-h .dk{flex:none;font-size:62px;font-weight:900}
.c17-h .dk.ok{color:#15803d}.c17-h .dk.no{color:#dc2626}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Ký thừa ủy quyền —<br>*ủy quyền lại được không?*`, { tren: 262, co: 74, gach: "c0k2" })}
  ${the(`<div class="c17-q"><div><b>TUQ.</b> người được ủy quyền → ủy quyền lại? <i>?</i></div><div><b style="background:#7c3aed">TL.</b> ký thừa lệnh → giao lại? <i>?</i></div></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ky": {
      khai: true,
      html: () => `
  ${chipKhai("✍️ Thừa ủy quyền · thừa lệnh")}
  ${hang("TUQ.", "u", "Chỉ trường hợp đặc biệt", true, "c0k1", 330)}
  ${hang("TUQ.", "u", "Giao bằng văn bản", true, "c0k2", 458)}
  ${hang("TUQ.", "u", "Giới hạn thời gian và nội dung", true, "c1k0", 586)}
  ${hang("TUQ.", "u", "Ủy quyền lại cho người khác", false, "c2k1", 714)}
  ${hang("TL.", "l", "Giao lại cho cấp phó ký thay", true, "c3k1", 842)}
  ${hang("TL.", "l", "Ghi trong quy chế", true, "c4k1", 970)}
  ${hang("Ký giấy", "g", "Bút mực xanh · không mực dễ phai", true, "c5k0", 1098)}`,
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
    ["02a-dac-biet", lucNode(tatCa, "ky", "c0k2w3")],
    ["02b-khong-uq-lai", lucNode(tatCa, "ky", "c2k1w4")],
    ["02c-thua-lenh", lucNode(tatCa, "ky", "c3k1w3")],
    ["02d-muc-xanh", lucNode(tatCa, "ky", "c5k1w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
