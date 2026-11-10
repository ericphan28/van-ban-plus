// ============================================================================
// THỂ THỨC 14 — VĂN BẢN KHẨN: AI QUYẾT ĐỊNH MỨC ĐỘ KHẨN? (Nghị định 30/2020/NĐ-CP, Phụ lục I, III.2 điểm a, b, c)
//
// Khuôn HỎI – ĐÁP CÓ MINH HỌA: móc «người soạn hay người ký quyết định?» → luồng đề xuất → quyết định → ba mức + ba con dấu vẽ
// ĐÚNG TỈ LỆ 30 : 40 : 20 × 8 mm, đổi sang mực đỏ tươi → chỉ dẫn phạm vi lưu hành → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC)
// → thẻ nguyên văn về dấu chỉ độ mật.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 120–127):
//   «a) … Việc xác định và đóng dấu chỉ độ mật (tuyệt mật, tối mật hoặc mật), dấu tài liệu thu hồi đối với văn bản có nội dung bí mật
//   nhà nước được thực hiện theo quy định hiện hành. Con dấu các độ mật (TUYỆT MẬT, TỐI MẬT hoặc MẬT) và dấu tài liệu thu hồi được khắc
//   sẵn theo quy định của pháp luật về bảo vệ bí mật nhà nước. …
//   b) Khi soạn thảo văn bản có tính chất khẩn, đơn vị hoặc cá nhân soạn thảo văn bản đề xuất mức độ khẩn trình người ký văn bản quyết
//   định. Tuỳ theo mức độ cần được chuyển phát nhanh, văn bản được xác định độ khẩn theo các mức sau: hoả tốc, thượng khẩn, khẩn.
//   Con dấu các mức độ khẩn được khắc sẵn hình chữ nhật có kích thước 30 mm x 8 mm, 40 mm x 8 mm và 20 mm x 8 mm, trên đó các từ
//   “HOẢ TỐC”, “THƯỢNG KHẨN” và “KHẨN”, trình bày bằng chữ in hoa, phông chữ Times New Roman, cỡ chữ từ 13 đến 14, kiểu chữ đứng, đậm và
//   đặt cân đối trong khung hình chữ nhật viền đơn. … Mực để đóng dấu chỉ mức độ khẩn dùng màu đỏ tươi.
//   c) … Đối với những văn bản có phạm vi, đối tượng, sử dụng hạn chế, sử dụng các chỉ dẫn về phạm vi lưu hành như “XEM XONG TRẢ LẠI”,
//   “LƯU HÀNH NỘI BỘ”. …»
//   ✔ «đề xuất — người ký quyết định» · ✔ ba mức hoả tốc, thượng khẩn, khẩn · ✔ kích thước theo ĐÚNG THỨ TỰ nguyên văn (30 → HOẢ TỐC,
//     40 → THƯỢNG KHẨN, 20 → KHẨN; khớp độ dài chữ) · ✔ khắc sẵn, hình chữ nhật, cao 8 mm · ✔ mực đỏ tươi
//   ✔ chỉ dẫn phạm vi lưu hành: «XEM XONG TRẢ LẠI», «LƯU HÀNH NỘI BỘ» · ✔ thẻ màn kết: độ mật gồm tuyệt mật, tối mật, mật
//   ✘ KHÔNG nói chi tiết cách xác định độ mật (Nghị định chỉ dẫn «theo quy định hiện hành»); KHÔNG nói phần mềm tự đóng dấu.
// ============================================================================
import { canhKet, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, veCC, theApp } from "./loat.mjs";

/** con dấu mức khẩn: rộng theo 9 px/mm; `moc` = mốc hiện; `mm` = mốc hiện nhãn kích thước */
const dauKhan = (chu, mm, w, moc, mocMm) =>
  `<div class="c14-d" data-k="lop" data-hien="${moc}" style="opacity:0"><div class="tem" data-cc="lop" data-lop="do" data-moc="c3k0" style="width:${w}px">${chu}</div><small data-k="lop" data-hien="${mocMm}" style="opacity:0">${mm}</small></div>`;

export default {
  ten: "14-van-ban-khan-ai-quyet-dinh",
  tieuDe: "Thể thức 14 · Văn bản khẩn: ai quyết định mức độ khẩn? Con dấu hỏa tốc, thượng khẩn, khẩn theo NĐ 30/2020",
  bia: { dong: ["Ai quyết định", "văn bản khẩn?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Việc xác định và đóng dấu chỉ độ mật (*tuyệt mật, tối mật hoặc mật*), dấu tài liệu thu hồi đối với văn bản có nội dung bí mật nhà nước được thực hiện theo quy định hiện hành.»",
      theNguon("Phụ lục I · mục III.2 · điểm a, b, c"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản khẩn, ai quyết định mức độ khẩn, người soạn thảo, hay người ký?", chu: "Văn bản khẩn — *ai quyết định* mức độ khẩn?" },
      { loi: "Chỉ một người quyết định.", chu: "Chỉ *một người* quyết định", nghiSau: 0.2 },
    ]},
    { canh: "khan", cau: [
      { loi: "Người soạn thảo chỉ đề xuất, người ký văn bản mới quyết định.", chu: "Người soạn thảo *đề xuất* · người ký *quyết định*" },
      { loi: "Có ba mức, hỏa tốc, thượng khẩn, và khẩn.", chu: "3 mức: *hỏa tốc* · *thượng khẩn* · *khẩn*" },
      { loi: "Con dấu khắc sẵn, hình chữ nhật, cao tám milimet, dài ba mươi, bốn mươi, hoặc hai mươi milimet.", chu: "Dấu khắc sẵn · chữ nhật · cao *8 mm* · dài *30 · 40 · 20 mm*" },
      { loi: "Mực đóng dấu khẩn, dùng màu đỏ tươi.", chu: "Mực đóng dấu khẩn: *đỏ tươi*", nghiSau: 0.3 },
    ]},
    { canh: "pham-vi", cau: [
      { loi: "Văn bản hạn chế sử dụng, thì có chỉ dẫn phạm vi lưu hành, như xem xong trả lại, hoặc lưu hành nội bộ.", chu: "Văn bản hạn chế sử dụng → *chỉ dẫn phạm vi lưu hành*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Độ mật gồm tuyệt mật, tối mật, và mật, xác định theo quy định hiện hành.", chu: "Độ mật: *tuyệt mật* · *tối mật* · *mật* — theo quy định hiện hành" },
      nguonCau("Phụ lục một", "Phụ lục I · mục III.2 · điểm a, b, c"),
    ]),
  ],

  css: CSS_CC + `
.c14-q{display:flex;justify-content:center;gap:28px;padding:46px 24px 52px}
.c14-q span{font-size:46px;font-weight:900;color:#1d4ed8;background:#dbeafe;border-radius:20px;padding:12px 28px}
.c14-q i{align-self:center;font-style:normal;font-size:60px;font-weight:900;color:#94a3b8}
.c14-f{display:flex;align-items:center;justify-content:center;gap:14px;padding:40px 14px 44px;font-size:44px;font-weight:900;color:#0f172a}
.c14-f .b{padding:10px 22px;border-radius:16px;background:#f1f5f9}
.c14-f .b.ok{background:#dcfce7;color:#15803d}
.c14-f .m{font-size:34px;font-weight:800;color:#64748b;text-align:center;line-height:1.1}
.c14-s{display:flex;justify-content:center;align-items:flex-start;gap:26px;padding:40px 20px 46px}
.c14-d{text-align:center}
.c14-d .tem{box-sizing:border-box;height:84px;display:flex;align-items:center;justify-content:center;border:5px solid #334155;color:#334155;font-family:"Times New Roman",serif;font-size:38px;font-weight:700;white-space:nowrap;border-radius:4px;transition:none}
.c14-d .tem.do{border-color:#ef1b1b;color:#ef1b1b}
.c14-d small{display:block;margin-top:10px;font-size:28px;font-weight:800;color:#2563eb}
.c14-pv{display:flex;flex-direction:column;align-items:center;gap:26px;padding:50px 20px 56px}
.c14-pv div{border:6px solid #0f172a;padding:10px 36px;font-family:"Times New Roman",serif;font-size:56px;font-weight:700;color:#0f172a;white-space:nowrap}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Văn bản khẩn —<br>*ai quyết định?*`, { tren: 262, co: 78, gach: "c0k2" })}
  ${the(`<div class="c14-q"><span>Người soạn thảo</span><i>?</i><span>Người ký</span></div>`, { tren: 700, rung: "c1k0w1" })}
  ${dau("✓ CHỈ MỘT NGƯỜI", "c1k0w3", { tren: 1000, co: 88, lop: "xanh" })}`,
      ve: veCC,
    },

    "khan": {
      khai: true,
      html: () => `
  ${chipKhai("🚨 Mức độ khẩn")}
  ${the(`<div class="c14-f"><span class="b">Người soạn thảo</span><span class="m">đề xuất<br>→</span><span class="b ok">Người ký</span><span class="m">quyết định<br>✓</span></div>`, { tren: 330, hien: "c0k0" })}
  ${the(`<div class="c14-s">${dauKhan("HOẢ TỐC", "30 mm", 270, "c1k1", "c2k3")}${dauKhan("THƯỢNG KHẨN", "40 mm", 360, "c1k2", "c2k4")}${dauKhan("KHẨN", "20 mm", 180, "c1k3", "c2k5")}</div>`, { tren: 620, hien: "c1k0" })}
  ${goiKhai("<span>Cao <b>8 mm</b> · khắc sẵn · hình chữ nhật</span>", { trai: 60, tren: 960, hien: "c2k2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Mực đóng dấu: <b>đỏ tươi</b></span>", { trai: 60, tren: 1090, hien: "c3k0", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    "pham-vi": {
      khai: true,
      html: () => `
  ${chipKhai("🔒 Phạm vi lưu hành")}
  ${the(`<div class="c14-pv"><div data-k="lop" data-hien="c0k2" style="opacity:0">XEM XONG TRẢ LẠI</div><div data-k="lop" data-hien="c0k3" style="opacity:0">LƯU HÀNH NỘI BỘ</div></div>`, { tren: 420, hien: "s-2" })}
  ${goiKhai("<span>Chữ in hoa · đậm · trong <b>khung hình chữ nhật viền đơn</b></span>", { trai: 60, tren: 860, hien: "c0k3w2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-dau", lucNode(tatCa, "moc", "c1k0w3")],
    ["02a-luong", lucNode(tatCa, "khan", "c0k1w3")],
    ["02b-ba-muc", lucNode(tatCa, "khan", "c1k3w1")],
    ["02c-kich-thuoc", lucNode(tatCa, "khan", "c2k5w2")],
    ["02d-muc-do", lucNode(tatCa, "khan", "c3k1w2")],
    ["03a-luu-hanh", lucNode(tatCa, "pham-vi", "c0k3w2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k0w3"),
};
