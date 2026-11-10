// ============================================================================
// THỂ THỨC 10 (làm lại 10/11/2026) — CĂN CỨ VÀ VIỆN DẪN: LUẬT KHÁC NGHỊ ĐỊNH Ở CHỖ NÀO? (Nghị định 30/2020/NĐ-CP, Phụ lục I, II.6 a, b)
//
// Khuôn HỎI – ĐÁP CÓ MINH HỌA (thay bản cũ chỉ nói «lần đầu ghi đủ, lần sau ghi gọn» bị chê nhàm, ít kiến thức):
//   móc «viện dẫn Luật có ghi số, ký hiệu không? còn Nghị định?» → Luật/Pháp lệnh KHÔNG ghi số, ký hiệu, cơ quan (3 chữ bị gạch)
//   → Nghị định ghi đủ 6 thành phần (ghép dần từng mảnh bằng ví dụ thật: chính Nghị định 30) → lần sau chỉ ghi tên loại + số, ký
//   hiệu → thẻ nguyên văn về cách trình bày căn cứ (nghiêng; ; và .). Bỏ cảnh phần mềm + «Tóm lại» (tên VanBanPlus ở màn kết).
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 59–63):
//   «a) … Căn cứ ban hành văn bản được ghi đầy đủ tên loại văn bản, số, ký hiệu, cơ quan ban hành, ngày tháng năm ban hành văn bản và
//   trích yếu nội dung văn bản (riêng Luật, Pháp lệnh không ghi số, ký hiệu, cơ quan ban hành).
//   Căn cứ ban hành văn bản được trình bày bằng chữ in thường, kiểu chữ nghiêng, cỡ chữ từ 13 đến 14, trình bày dưới phần tên loại và
//   trích yếu nội dung văn bản; sau mỗi căn cứ phải xuống dòng, cuối dòng có dấu chẩm phẩy (;), dòng cuối cùng kết thúc bằng dấu chấm (.).
//   b) Khi viện dẫn lần đầu văn bản có liên quan, phải ghi đầy đủ tên loại, số, ký hiệu của văn bản, thời gian ban hành văn bản, tên cơ
//   quan, tổ chức ban hành văn bản và trích yếu nội dung văn bản (đối với Luật và Pháp lệnh chỉ ghi tên loại và tên của Luật, Pháp
//   lệnh); trong các lần viện dẫn tiếp theo, chỉ ghi tên loại và số, ký hiệu của văn bản đó.»
//   ✔ «Luật, Pháp lệnh không ghi số, ký hiệu, cơ quan ban hành» (a) — ba thứ bị gạch trên hình đúng bằng ba thứ này
//   ✔ «Nghị định: tên loại, số, ký hiệu, ngày ban hành, cơ quan, trích yếu» (a, b) — ví dụ «Nghị định số 30/2020/NĐ-CP ngày 05 tháng 3
//     năm 2020 của Chính phủ về công tác văn thư» đúng README / noi-dung.md của chính Nghị định 30
//   ✔ «lần sau chỉ ghi tên loại và số, ký hiệu» (b)
//   ✔ «chữ nghiêng; cuối dòng ; ; dòng cuối .» (a) — thẻ nguyên văn màn kết
//   ✔ «Căn cứ Luật Tổ chức Quốc hội» — tên Luật có trong Phụ lục II (ví dụ viết hoa); «ngày … tháng … năm …» để trống, không bịa ngày
//   ✘ KHÔNG nói phần mềm tự viện dẫn / tự lập phần căn cứ (chưa soát mã).
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

/** một mảnh của câu viện dẫn: chữ + nhãn nhỏ bên dưới, hiện lúc `hien` */
const manh = (chu, nhan, hien) =>
  `<span class="m" data-k="lop" data-hien="${hien}" style="opacity:0">${chu}<small>${nhan}</small></span>`;

/** chip bị gạch: hiện sẵn, gạch đỏ từ mốc `moc` */
const gach = (chu, moc) => `<span class="gc" data-cc="lop" data-lop="on" data-moc="${moc}">${chu}</span>`;

export default {
  ten: "10-can-cu-luat-nghi-dinh",
  tieuDe: "Thể thức 10 · Căn cứ, viện dẫn: Luật khác Nghị định ở chỗ nào? Có ghi số, ký hiệu không? Theo NĐ 30/2020",
  bia: { dong: ["Viện dẫn Luật:", "có ghi số không?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Căn cứ ban hành văn bản được trình bày bằng chữ in thường, kiểu chữ nghiêng, cỡ chữ từ 13 đến 14 (…); *sau mỗi căn cứ phải xuống dòng, cuối dòng có dấu chấm phẩy (;), dòng cuối cùng kết thúc bằng dấu chấm (.).*»",
      theNguon("Phụ lục I · mục II.6 · điểm a, b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Phần Căn cứ ở đầu văn bản, viện dẫn Luật, có ghi số và ký hiệu không?", chu: "Phần «Căn cứ» — viện dẫn *Luật*, *có ghi số, ký hiệu không?*" },
      { loi: "Còn viện dẫn Nghị định, thì sao?", chu: "Còn viện dẫn *Nghị định* thì sao?", nghiSau: 0.2 },
    ]},
    { canh: "luat", cau: [
      { loi: "Luật và Pháp lệnh, không ghi số, ký hiệu, và cơ quan ban hành.", chu: "Luật · Pháp lệnh: *không ghi* số · ký hiệu · cơ quan ban hành", nghiSau: 0.3 },
    ]},
    { canh: "nghi-dinh", cau: [
      { loi: "Nghị định thì ghi đầy đủ, tên loại, số, ký hiệu, ngày ban hành, cơ quan, và trích yếu.", chu: "Nghị định: ghi *đầy đủ* — loại · số · ký hiệu · ngày · cơ quan · trích yếu", nghiSau: 0.3 },
    ]},
    { canh: "lan-sau", cau: [
      { loi: "Từ lần viện dẫn tiếp theo, chỉ ghi tên loại, số và ký hiệu.", chu: "Lần sau: chỉ ghi *tên loại* · *số, ký hiệu*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["Luật không ghi số và ký hiệu", "Luật: không số, ký hiệu"],
      ["Nghị định ghi đầy đủ sáu mục", "Nghị định: ghi đủ 6 mục"],
      ["lần sau chỉ ghi loại và số ký hiệu", "Lần sau: loại + số, ký hiệu"],
    ]),
    canhKet([
      { loi: "Căn cứ viết chữ in thường, nghiêng, sau mỗi căn cứ phải xuống dòng, cuối dòng có dấu chấm phẩy, dòng cuối cùng kết thúc bằng dấu chấm.", chu: "Căn cứ: chữ nghiêng · cuối dòng *( ; )* · dòng cuối *( . )*" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.6 · điểm a, b"),
    ]),
  ],

  css: CSS_CC + `
.c10-cc{padding:40px 34px 46px;font-family:"Times New Roman",serif;font-style:italic;font-size:50px;line-height:1.5;color:#111}
.c10-cc .q{display:inline-block;margin-left:18px;padding:0 18px;border-radius:14px;background:#fef3c7;box-shadow:0 0 0 3px #f59e0b;font-family:system-ui,sans-serif;font-style:normal;font-size:34px;font-weight:900;color:#92400e;vertical-align:middle}
.c10-l{padding:38px 34px 30px;font-family:"Times New Roman",serif;font-style:italic;font-size:52px;line-height:1.4;color:#111}
.c10-l .rx{display:flex;gap:16px;margin-top:22px;font-style:normal;font-family:system-ui,sans-serif}
.c10-l .gc{padding:6px 20px;border-radius:14px;background:#f1f5f9;font-size:36px;font-weight:900;color:#334155;transition:none}
.c10-l .gc.on{background:#fee2e2;color:#b91c1c;text-decoration:line-through;text-decoration-thickness:5px}
.c10-ghep{padding:36px 34px 40px;display:flex;flex-wrap:wrap;gap:22px 16px;font-family:"Times New Roman",serif;font-size:50px;color:#111}
.c10-ghep .m{position:relative;display:inline-flex;flex-direction:column;align-items:center;padding:6px 14px 4px;border-radius:14px;background:#eff6ff;box-shadow:inset 0 0 0 3px #bfdbfe;white-space:nowrap}
.c10-ghep .m small{font-family:system-ui,sans-serif;font-size:24px;font-weight:800;color:#2563eb;margin-top:2px}
.c10-sau{padding:34px 30px 40px;text-align:center;font-family:"Times New Roman",serif;color:#111}
.c10-sau .nhan{font-family:system-ui,sans-serif;font-size:30px;font-weight:800;color:#64748b;margin-bottom:10px}
.c10-sau .cau{font-size:56px}
.c10-sau .cau b{font-weight:400;color:#15803d;background:#dcfce7;border-radius:10px;padding:0 8px}
.c10-sau .mo{font-size:44px;color:#94a3b8;margin-top:14px}
.c10-sau .mo s{text-decoration-thickness:3px;text-decoration-color:#dc2626}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + hai dòng «Căn cứ …» với dấu «?»
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Viện dẫn Luật —<br>*có ghi số không?*`, { tren: 262, co: 78, gach: "c0k2" })}
  ${the(`<div class="c10-cc"><div>Căn cứ Luật Tổ chức Quốc hội<span class="q">số, ký hiệu?</span></div><div>Căn cứ Nghị định<span class="q">?</span></div></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    // Luật / Pháp lệnh: không ghi số, ký hiệu, cơ quan ban hành → 3 chip bị gạch
    "luat": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Viện dẫn Luật, Pháp lệnh")}
  ${the(`<div class="c10-l">Căn cứ Luật Tổ chức Quốc hội ngày … tháng … năm …;<div class="rx">${gach("số", "c0k1w2")}${gach("ký hiệu", "c0k2")}${gach("cơ quan ban hành", "c0k3w1")}</div></div>`, { tren: 400, hien: "s-2" })}
  ${goiKhai("<span>Luật, Pháp lệnh: <b>không ghi</b> số · ký hiệu · cơ quan</span>", { trai: 60, tren: 790, hien: "c0k3w3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // Nghị định: ghép dần 6 thành phần của câu viện dẫn đầy đủ
    "nghi-dinh": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Viện dẫn Nghị định · lần đầu")}
  ${the(`<div class="c10-ghep">
    ${manh("Nghị định", "tên loại", "c0k1")}
    ${manh("số 30/2020/NĐ-CP", "số, ký hiệu", "c0k2")}
    ${manh("ngày 05 tháng 3 năm 2020", "ngày ban hành", "c0k4")}
    ${manh("của Chính phủ", "cơ quan ban hành", "c0k5")}
    ${manh("về công tác văn thư", "trích yếu", "c0k6")}
  </div>`, { tren: 400, hien: "s-2" })}
  ${goiKhai("<span>Ví dụ thật: chính <b>Nghị định 30</b> khi được viện dẫn</span>", { trai: 60, tren: 920, hien: "c0k6w1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // lần sau: chỉ tên loại + số, ký hiệu
    "lan-sau": {
      khai: true,
      html: () => `
  ${chipKhai("🔁 Từ lần viện dẫn thứ hai")}
  ${the(`<div class="c10-sau"><div class="nhan">LẦN ĐẦU</div><div class="mo">Nghị định số 30/2020/NĐ-CP <s>ngày 05 tháng 3 năm 2020 của Chính phủ về công tác văn thư</s></div></div>`, { tren: 400, hien: "s-2" })}
  ${the(`<div class="c10-sau"><div class="nhan">LẦN TIẾP THEO</div><div class="cau"><b>Nghị định</b> <b>số 30/2020/NĐ-CP</b></div></div>`, { tren: 720, hien: "c0k1w1" })}
  ${goiKhai("<span>Lần sau: chỉ <b>tên loại</b> + <b>số, ký hiệu</b></span>", { trai: 60, tren: 960, hien: "c0k2w1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-nghi-dinh", lucNode(tatCa, "moc", "c1k0w3")],
    ["02a-so", lucNode(tatCa, "luat", "c0k1w3")],
    ["02b-het", lucNode(tatCa, "luat", "c0k3w4")],
    ["03a-ngay", lucNode(tatCa, "nghi-dinh", "c0k4w3")],
    ["03b-du", lucNode(tatCa, "nghi-dinh", "c0k6w2")],
    ["04a-sau", lucNode(tatCa, "lan-sau", "c0k2w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2w1"),
};
