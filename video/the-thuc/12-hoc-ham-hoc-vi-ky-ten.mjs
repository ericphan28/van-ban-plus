// ============================================================================
// THỂ THỨC 12 — KÝ TÊN: CÓ ĐƯỢC GHI «TS.», «ThS.» TRƯỚC HỌ TÊN KHÔNG? (Nghị định 30/2020/NĐ-CP, Phụ lục I, II.7 điểm c)
//
// Khuôn HỎI – ĐÁP CÓ NGOẠI LỆ (nhắm cán bộ giáo dục, y tế): móc → trước họ tên không ghi học hàm, học vị (hình: «TS.» bị gạch)
// → họ tên gồm những gì → ngoại lệ 4 loại đơn vị do người đứng đầu cơ quan quản lý ngành quy định → chức vụ chính thức
// → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn về tổ chức tư vấn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 83–87):
//   «Chức vụ ghi trên văn bản là chức vụ lãnh đạo chính thức của người ký văn bản trong cơ quan, tổ chức; không ghi những chức vụ mà
//   Nhà nước không quy định.»
//   «Đối với những tổ chức tư vấn được phép sử dụng con dấu của cơ quan, tổ chức thì ghi chức danh của người ký văn bản trong tổ chức
//   tư vấn và chức vụ trong cơ quan, tổ chức. Đối với những tổ chức tư vấn không được phép sử dụng con dấu của cơ quan, tổ chức thì chỉ
//   ghi chức danh của người ký văn bản trong tổ chức tư vấn.»
//   «Họ và tên người ký văn bản bao gồm họ, tên đệm (nếu có) và tên của người ký văn bản. Trước họ tên của người ký, không ghi học hàm,
//   học vị và các danh hiệu danh dự khác. Việc ghi thêm quân hàm, học hàm, học vị trước họ tên người ký đối với văn bản của các đơn vị
//   vũ trang nhân dân, các tổ chức sự nghiệp giáo dục, y tế, khoa học do người đứng đầu cơ quan quản lý ngành, lĩnh vực quy định.»
//   ✔ «trước họ tên không ghi học hàm, học vị, danh hiệu danh dự» · ✔ «họ, tên đệm (nếu có), tên»
//   ✔ «ngoại lệ: văn bản của đơn vị vũ trang nhân dân, tổ chức sự nghiệp giáo dục, y tế, khoa học → do người đứng đầu cơ quan quản lý
//     ngành, lĩnh vực quy định» (KHÔNG nói «được ghi» mặc định — chỉ nói «do cơ quan quản lý ngành quy định»)
//   ✔ «chức vụ phải là chức vụ lãnh đạo chính thức, không ghi chức vụ Nhà nước không quy định»
//   ✔ thẻ màn kết: tổ chức tư vấn không được dùng con dấu của cơ quan → chỉ ghi chức danh trong tổ chức tư vấn
//   ✔ «Nguyễn Văn A», «GIÁM ĐỐC», «chức vụ tự đặt» chỉ là giữ chỗ, ghi «ví dụ minh họa»
//   ✘ KHÔNG nói phần mềm tự kiểm tra chữ ký (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, veCC, theApp } from "./loat.mjs";

const fx = (sai, dung, moc) =>
  `<span class="fx" data-cc="lop" data-lop="ok" data-moc="${moc}"><span class="s">${sai}</span><span class="d">${dung}</span></span>`;

export default {
  ten: "12-hoc-ham-hoc-vi-ky-ten",
  tieuDe: "Thể thức 12 · Ký tên có được ghi «TS.», «ThS.» trước họ tên không? Theo NĐ 30/2020",
  bia: { dong: ["Ký tên: ghi «TS.»", "trước họ tên?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Đối với những tổ chức tư vấn *không được phép sử dụng con dấu của cơ quan, tổ chức* thì chỉ ghi chức danh của người ký văn bản trong tổ chức tư vấn.»",
      theNguon("Phụ lục I · mục II.7 · điểm c"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Ký văn bản, trước họ tên có được ghi, tiến sĩ, thạc sĩ không?", chu: "Ký văn bản — trước họ tên *ghi «TS.», «ThS.»?*" },
      { loi: "Còn giáo viên, bác sĩ, thì sao?", chu: "Còn *giáo viên*, *bác sĩ* thì sao?", nghiSau: 0.2 },
    ]},
    { canh: "ho-ten", cau: [
      { loi: "Trước họ tên người ký, không ghi học hàm, học vị, và các danh hiệu danh dự khác.", chu: "Trước họ tên: *không ghi* học hàm · học vị · danh hiệu" },
      { loi: "Họ tên gồm họ, tên đệm nếu có, và tên.", chu: "Họ tên = *họ* + *tên đệm (nếu có)* + *tên*" },
      { loi: "Riêng đơn vị vũ trang, giáo dục, y tế, khoa học, thì do người đứng đầu cơ quan quản lý ngành quy định.", chu: "Riêng 4 loại đơn vị: *do cơ quan quản lý ngành quy định*", nghiSau: 0.3 },
    ]},
    { canh: "chuc-vu", cau: [
      { loi: "Chức vụ phải là chức vụ lãnh đạo chính thức, không ghi những chức vụ Nhà nước không quy định.", chu: "Chức vụ: *lãnh đạo chính thức* · không ghi chức vụ *Nhà nước không quy định*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Tổ chức tư vấn không được dùng con dấu của cơ quan, thì chỉ ghi chức danh trong tổ chức tư vấn.", chu: "Tổ chức tư vấn *không dùng con dấu cơ quan* → chỉ ghi *chức danh trong tổ chức tư vấn*" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.7 · điểm c"),
    ]),
  ],

  css: CSS_CC + `
.c12-ky{padding:40px 30px 46px;text-align:center;font-family:"Times New Roman",serif;color:#111}
.c12-ky .cv{font-size:58px;font-weight:700}
.c12-ky .kc{margin:10px 0;font-size:34px;font-style:italic;color:#64748b}
.c12-ky .ht{font-size:80px;font-weight:700}
.c12-ky .vd{margin-top:14px;font-family:system-ui,sans-serif;font-size:24px;font-weight:700;color:#94a3b8}
.c12-ky .fx .d{display:none}
.c12-ky .fx .s{color:#b91c1c;background:#fee2e2;border-radius:8px;text-decoration:line-through;text-decoration-thickness:5px}
.c12-ky .fx.ok .s{display:none}
.c12-ky .fx.ok .d{display:inline}
.c12-ten{display:flex;justify-content:center;gap:16px;padding:36px 24px 40px;font-family:"Times New Roman",serif;font-size:64px;font-weight:700}
.c12-ten span{padding:6px 22px;border-radius:16px;background:#eff6ff;box-shadow:inset 0 0 0 3px #bfdbfe;white-space:nowrap}
.c12-ten small{display:block;text-align:center;font-family:system-ui,sans-serif;font-size:28px;font-weight:800;color:#2563eb}
.c12-nl{display:flex;flex-wrap:wrap;justify-content:center;gap:16px;padding:30px 24px 34px}
.c12-nl i{font-style:normal;font-size:40px;font-weight:900;color:#1d4ed8;background:#dbeafe;border-radius:14px;padding:8px 22px}
.c12-nl p{width:100%;margin:8px 0 0;text-align:center;font-size:36px;font-weight:800;color:#b45309}
.c12-cv{padding:40px 30px 46px;font-size:48px;font-weight:800;color:#0f172a}
.c12-cv .r{display:flex;align-items:center;gap:18px;margin:10px 0;padding:10px 20px;border-radius:16px;background:#f1f5f9}
.c12-cv .r.ok{background:#dcfce7;color:#15803d}
.c12-cv .r.sai{background:#fee2e2;color:#b91c1c;text-decoration:line-through;text-decoration-thickness:4px}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Ký tên —<br>*ghi «TS.» trước họ tên?*`, { tren: 262, co: 78, gach: "c0k1" })}
  ${the(`<div class="c12-ky"><div class="cv">GIÁM ĐỐC</div><div class="kc">(Chữ ký)</div><div class="ht">TS. Nguyễn Văn A</div><div class="vd">ví dụ minh họa</div></div>`, { tren: 700, rung: "c1k0w1" })}
  ${dau("?", "c1k1", { tren: 1010, co: 120 })}`,
      ve: veCC,
    },

    "ho-ten": {
      khai: true,
      html: () => `
  ${chipKhai("✍️ Họ tên người ký")}
  ${the(`<div class="c12-ky"><div class="cv">GIÁM ĐỐC</div><div class="kc">(Chữ ký)</div><div class="ht">${fx("TS. ", "", "c0k1")}Nguyễn Văn A</div><div class="vd">ví dụ minh họa</div></div>`, { tren: 330, hien: "s-2" })}
  ${the(`<div class="c12-ten"><span>Nguyễn<small>họ</small></span><span>Văn<small>tên đệm (nếu có)</small></span><span>A<small>tên</small></span></div>`, { tren: 700, hien: "c1k0" })}
  ${the(`<div class="c12-nl"><i>Vũ trang nhân dân</i><i>Giáo dục</i><i>Y tế</i><i>Khoa học</i><p>→ do người đứng đầu cơ quan quản lý ngành quy định</p></div>`, { tren: 940, hien: "c2k1" })}`,
    },

    "chuc-vu": {
      khai: true,
      html: () => `
  ${chipKhai("🏷️ Chức vụ ghi trên văn bản")}
  ${the(`<div class="c12-cv"><div class="r ok">✓ GIÁM ĐỐC — chức vụ lãnh đạo chính thức</div><div class="r sai" data-k="lop" data-hien="c0k1" style="opacity:0">✗ chức vụ tự đặt (ví dụ minh họa)</div></div>`, { tren: 400, hien: "s-2" })}
  ${goiKhai("<span>Không ghi chức vụ <b>Nhà nước không quy định</b></span>", { trai: 60, tren: 760, hien: "c0k1w2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-dau", lucNode(tatCa, "moc", "c1k1w1")],
    ["02a-bo", lucNode(tatCa, "ho-ten", "c0k1w3")],
    ["02b-ten", lucNode(tatCa, "ho-ten", "c1k2w1")],
    ["02c-ngoai-le", lucNode(tatCa, "ho-ten", "c2k4w3")],
    ["03a-chuc-vu", lucNode(tatCa, "chuc-vu", "c0k1w4")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1w2"),
};
