// ============================================================================
// THỂ THỨC 15 — PHỤ LỤC KÈM THEO: ĐÁNH SỐ LA MÃ, SỐ TRANG RIÊNG, DÒNG CHỈ DẪN (Nghị định 30/2020/NĐ-CP, Phụ lục I, III.1 a–c, đ; Phụ lục III mẫu)
//
// Khuôn HỎI – ĐÁP CÓ MINH HỌA: móc hai câu hỏi (số La Mã? số trang riêng?) → «1, 2» đổi thành «I, II» → số trang chạy lại từ 1 ở mỗi
// phụ lục → dòng chỉ dẫn «Kèm theo…» nghiêng, giấy phải điền, điện tử không phải điền → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC)
// → thẻ nguyên văn về cách trình bày tên phụ lục.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 108–118; phu-luc-III.md dòng 69):
//   «a) Trường hợp văn bản có Phụ lục kèm theo thì trong văn bản phải có chỉ dẫn về Phụ lục đó. Văn bản có từ hai Phụ lục trở lên thì
//   các Phụ lục phải được đánh số thứ tự bằng chữ số La Mã.
//   b) Từ “Phụ lục” và số thứ tự của Phụ lục được trình bày thành một dòng riêng, canh giữa, bằng chữ in thường, cỡ chữ 14, kiểu chữ
//   đứng, đậm; tên Phụ lục (nếu có) được trình bày canh giữa, bằng chữ in hoa, cỡ chữ từ 13 đến 14, kiểu chữ đứng, đậm.
//   c) Thông tin chỉ dẫn kèm theo văn bản trên mỗi Phụ lục được ban hành bao gồm: Số, ký hiệu văn bản, thời gian ban hành văn bản và tên
//   cơ quan, tổ chức ban hành văn bản. Thông tin chỉ dẫn kèm theo văn bản được canh giữa phía dưới tên của Phụ lục, chữ in thường, cỡ chữ từ
//   13 đến 14, kiểu chữ nghiêng, … Thông tin chỉ dẫn kèm theo văn bản trên mỗi phụ lục (Kèm theo văn bản số …/…-… ngày …. tháng ….năm ….)
//   được ghi đầy đủ đối với văn bản giấy; đối với văn bản điện tử, không phải điền thông tin tại các vị trí này.
//   đ) Số trang của Phụ lục được đánh số riêng theo từng Phụ lục.»
//   Phụ lục III (mẫu 1.2): «(Kèm theo Quyết định số … /QĐ- … ngày … tháng … năm … của… )»
//   ✔ «từ hai phụ lục trở lên → số La Mã» · ✔ «trong văn bản phải có chỉ dẫn về Phụ lục» · ✔ «số trang đánh riêng theo từng phụ lục»
//   ✔ dòng chỉ dẫn gồm số, ký hiệu, thời gian ban hành, tên cơ quan ban hành; chữ nghiêng; giấy điền đủ, điện tử không phải điền
//   ✔ thẻ màn kết: «Phụ lục» + số thứ tự một dòng riêng, canh giữa, đậm; tên phụ lục in hoa
//   ✔ «TÊN PHỤ LỤC», «… » chỉ là giữ chỗ như mẫu của Nghị định; ghi «ví dụ minh họa» · ✘ KHÔNG nói phần mềm tự lập phụ lục.
// ============================================================================
import { canhKet, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const fx = (sai, dung, moc) =>
  `<span class="fx" data-cc="lop" data-lop="ok" data-moc="${moc}"><span class="s">${sai}</span><span class="d">${dung}</span></span>`;

export default {
  ten: "15-phu-luc-kem-theo-van-ban",
  tieuDe: "Thể thức 15 · Phụ lục kèm theo: đánh số I, II hay 1, 2? Số trang đánh riêng? Theo NĐ 30/2020",
  bia: { dong: ["Hai phụ lục:", "số 1, 2 hay I, II?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Từ “Phụ lục” và số thứ tự của Phụ lục được trình bày *thành một dòng riêng, canh giữa*, bằng chữ in thường, cỡ chữ 14, kiểu chữ đứng, đậm; *tên Phụ lục (nếu có) … bằng chữ in hoa*, cỡ chữ từ 13 đến 14, kiểu chữ đứng, đậm.»",
      theNguon("Phụ lục I · mục III.1 · điểm b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản có hai phụ lục, đánh số kiểu nào, một hai, hay La Mã?", chu: "2 phụ lục — đánh số *1, 2* hay *I, II*?" },
      { loi: "Còn số trang phụ lục, đánh tiếp, hay đánh riêng?", chu: "Số trang phụ lục — *đánh tiếp* hay *đánh riêng*?", nghiSau: 0.2 },
    ]},
    { canh: "danh-so", cau: [
      { loi: "Từ hai phụ lục trở lên, phải đánh số thứ tự, bằng chữ số La Mã.", chu: "Từ *2 phụ lục* trở lên → số thứ tự bằng *chữ số La Mã*" },
      { loi: "Trong văn bản, phải có chỉ dẫn về phụ lục đó.", chu: "Trong văn bản *phải có chỉ dẫn* về phụ lục" },
      { loi: "Số trang của phụ lục, đánh số riêng, theo từng phụ lục.", chu: "Số trang phụ lục: *đánh riêng* theo từng phụ lục", nghiSau: 0.3 },
    ]},
    { canh: "chi-dan", cau: [
      { loi: "Dưới tên phụ lục, có dòng chỉ dẫn kèm theo văn bản số, ngày tháng năm, và cơ quan ban hành, chữ nghiêng.", chu: "Dưới tên phụ lục: dòng *«Kèm theo…»* — số · ngày · cơ quan · *chữ nghiêng*" },
      { loi: "Văn bản giấy phải điền đầy đủ, văn bản điện tử thì không phải điền.", chu: "Văn bản *giấy*: điền đủ · văn bản *điện tử*: *không phải điền*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Từ Phụ lục và số thứ tự, trình bày một dòng riêng, canh giữa, chữ đậm, tên phụ lục viết in hoa.", chu: "«Phụ lục» + số thứ tự: một dòng riêng · canh giữa · đậm · *tên phụ lục IN HOA*" },
      nguonCau("Phụ lục một", "Phụ lục I · mục III.1 · điểm b"),
    ]),
  ],

  css: CSS_CC + `
.c15-q{padding:40px 30px 46px;display:flex;flex-direction:column;gap:18px;align-items:center;font-size:50px;font-weight:900;color:#0f172a}
.c15-q .p{display:flex;gap:30px;align-items:center}
.c15-q b{font-family:"Times New Roman",serif;font-size:60px;padding:6px 26px;border-radius:16px;background:#f1f5f9}
.c15-q i{font-style:normal;font-size:56px;color:#94a3b8}
.c15-pl{padding:36px 30px 40px;text-align:center;font-family:"Times New Roman",serif;color:#111}
.c15-pl .pl{font-size:70px;font-weight:700}
.c15-pl .ten{font-size:50px;font-weight:700;margin-top:6px}
.c15-pl .ck{font-size:40px;font-style:italic;margin-top:8px;line-height:1.35}
.c15-pl .vd{margin-top:10px;font-family:system-ui,sans-serif;font-size:24px;font-weight:700;color:#94a3b8}
.c15-pl .fx .d{display:none}
.c15-pl .fx .s{color:#b91c1c;background:#fee2e2;border-radius:8px}
.c15-pl .fx.ok .s{display:none}
.c15-pl .fx.ok .d{display:inline;color:#15803d;background:#dcfce7;border-radius:8px;padding:0 6px}
.c15-tr{padding:30px 30px 34px}
.c15-tr .hg{display:flex;align-items:center;gap:20px;margin:10px 0}
.c15-tr .hg em{flex:none;width:290px;font-style:normal;font-size:42px;font-weight:900;color:#0f172a}
.c15-tr .hg span{width:96px;height:118px;border-radius:10px;border:4px solid #94a3b8;background:#fff;display:flex;align-items:center;justify-content:center;font-size:56px;font-weight:900;color:#334155}
.c15-tr .hg.ri span{border-color:#16a34a;color:#15803d;background:#f0fdf4}
.c15-gd{display:flex;gap:22px;justify-content:center;padding:34px 24px 40px}
.c15-gd div{flex:1;text-align:center;border-radius:20px;padding:26px 14px;font-size:44px;font-weight:900;line-height:1.25}
.c15-gd .g{background:#dcfce7;color:#15803d}
.c15-gd .e{background:#dbeafe;color:#1d4ed8}
.c15-gd small{display:block;margin-top:8px;font-size:34px;font-weight:800}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Hai phụ lục —<br>*số 1, 2 hay I, II?*`, { tren: 262, co: 78, gach: "c0k3" })}
  ${the(`<div class="c15-q"><div class="p"><b>Phụ lục 1</b><i>/</i><b>Phụ lục I</b></div><div class="p" style="font-size:40px;color:#475569">Số trang:<b style="font-size:44px">4, 5, 6</b><i style="font-size:40px">/</i><b style="font-size:44px">1, 2, 3</b></div></div>`, { tren: 700, rung: "c1k0w2" })}`,
      ve: veCC,
    },

    "danh-so": {
      khai: true,
      html: () => `
  ${chipKhai("📎 Phụ lục kèm theo")}
  ${the(`<div class="c15-pl"><div class="pl">Phụ lục ${fx("1", "I", "c0k2")} · Phụ lục ${fx("2", "II", "c0k2")}</div></div>`, { tren: 340, hien: "s-2" })}
  ${the(`<div class="c15-pl"><div class="ck" style="font-style:normal">…nội dung chi tiết xem <b>Phụ lục I</b> kèm theo văn bản này…</div><div class="vd">ví dụ minh họa</div></div>`, { tren: 560, hien: "c1k0" })}
  ${the(`<div class="c15-tr"><div class="hg"><em>Văn bản</em><span>1</span><span>2</span><span>3</span></div><div class="hg ri"><em>Phụ lục I</em><span>1</span><span>2</span></div><div class="hg ri"><em>Phụ lục II</em><span>1</span></div></div>`, { tren: 850, hien: "c2k0" })}`,
    },

    "chi-dan": {
      khai: true,
      html: () => `
  ${chipKhai("🏷️ Dòng chỉ dẫn dưới tên phụ lục")}
  ${the(`<div class="c15-pl"><div class="pl">Phụ lục I</div><div class="ten">TÊN PHỤ LỤC</div><div class="ck">(Kèm theo Quyết định số …/QĐ-… ngày … tháng … năm … của …)</div><div class="vd">ví dụ minh họa — mẫu của Nghị định</div></div>`, { tren: 360, hien: "s-2" })}
  ${the(`<div class="c15-gd"><div class="g">VĂN BẢN GIẤY<small>điền đầy đủ</small></div><div class="e">VĂN BẢN ĐIỆN TỬ<small>không phải điền</small></div></div>`, { tren: 800, hien: "c1k0" })}`,
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
    ["02a-la-ma", lucNode(tatCa, "danh-so", "c0k2w2")],
    ["02b-chi-dan", lucNode(tatCa, "danh-so", "c1k1w3")],
    ["02c-so-trang", lucNode(tatCa, "danh-so", "c2k2w2")],
    ["03a-kem-theo", lucNode(tatCa, "chi-dan", "c0k2w2")],
    ["03b-giay-dien-tu", lucNode(tatCa, "chi-dan", "c1k1w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k3"),
};
