// ============================================================================
// THỂ THỨC 09 (làm lại 10/11/2026) — «KÍNH GỬI» VÀ «NƠI NHẬN»: LOẠI NÀO CÓ CẢ HAI? (Nghị định 30/2020/NĐ-CP, Phụ lục I, II.9 a–d)
//
// Khuôn HỎI – ĐÁP CÓ MINH HỌA (thay bản cũ chỉ đọc «nghiêng đậm cỡ 12, danh sách cỡ 11» bị chê nhàm, ít kiến thức):
//   móc «chỉ ba loại có cả Kính gửi và Nơi nhận» + «Quyết định có Kính gửi không?» → bảng loại nào có gì → cách ghi «Kính gửi»
//   (một nơi / từ hai nơi) → «Nơi nhận» mở bằng «Như trên» → thẻ nguyên văn. Bỏ cảnh phần mềm + «Tóm lại» (tên VanBanPlus ở màn kết).
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 99–105):
//   «b) Đối với Tờ trình, Báo cáo (cơ quan, tổ chức cấp dưới gửi cơ quan, tổ chức cấp trên) và Công văn, nơi nhận bao gồm:
//   Phần thứ nhất bao gồm từ “Kính gửi”, sau đó là tên các cơ quan, tổ chức hoặc đơn vị, cá nhân trực tiếp giải quyết công việc.
//   Phần thứ hai bao gồm từ “Nơi nhận”, phía dưới là từ “Như trên”, tiếp theo là tên các cơ quan, tổ chức, đơn vị và cá nhân có
//   liên quan khác nhận văn bản.»
//   «c) Đối với những văn bản khác, nơi nhận bao gồm từ “Nơi nhận” và phần liệt kê các cơ quan, tổ chức, đơn vị và cá nhân nhận văn bản.»
//   «d) … Phần nơi nhận tại ô số 9a (áp dụng đối với Tờ trình, Báo cáo của cơ quan, tổ chức cấp dưới gửi cơ quan, tổ chức cấp trên và
//   Công văn): … sau từ “Kính gửi” có dấu hai chấm (:). Nếu văn bản gửi cho một cơ quan, tổ chức hoặc một cá nhân thì từ “Kính gửi” và
//   tên cơ quan, tổ chức hoặc cá nhân được trình bày trên cùng một dòng; trường hợp văn bản gửi cho hai cơ quan, tổ chức hoặc cá nhân
//   trở lên thì xuống dòng, tên mỗi cơ quan, tổ chức, cá nhân hoặc mỗi nhóm … được trình bày trên một dòng riêng, đầu dòng có gạch đầu
//   dòng (-), cuối dòng có dấu chấm phẩy (;), cuối dòng cuối cùng có dấu chấm (.); các gạch đầu dòng được trình bày thẳng hàng với nhau
//   dưới dấu hai chấm (:).
//   Phần nơi nhận tại ô số 9b (áp dụng chung đối với các loại văn bản): … dòng cuối cùng bao gồm chữ “Lưu” sau có dấu hai chấm (:), tiếp
//   theo là chữ viết tắt “VT”, dấu phẩy (,), chữ viết tắt tên đơn vị (hoặc bộ phận) soạn thảo văn bản và số lượng bản lưu, cuối cùng là
//   dấu chấm (.).»
//   ✔ «chỉ ba loại có cả Kính gửi và Nơi nhận» = Tờ trình, Báo cáo (cấp dưới gửi cấp trên), Công văn (b, d-9a)
//   ✔ «loại khác như Quyết định chỉ có Nơi nhận» = (c) «đối với những văn bản khác, nơi nhận bao gồm từ Nơi nhận và phần liệt kê»
//     (suy từ c + phạm vi áp dụng của ô 9a; Nghị định không nêu riêng chữ «Quyết định» trong đoạn này)
//   ✔ «một nơi: cùng một dòng; từ hai nơi: xuống dòng, gạch đầu dòng, cuối dòng ; và dòng cuối .» (d)
//   ✔ «Nơi nhận mở bằng Như trên, rồi các nơi liên quan khác» (b); «dòng cuối Lưu: VT, đơn vị soạn thảo, số bản lưu, dấu chấm» (d, ô 9b)
//   ✘ KHÔNG giải nghĩa «VT» (Nghị định không giải nghĩa); tên «Sở A», «Sở B», «Sở C», «VP» chỉ là ví dụ minh họa, không tên cơ quan thật.
//   ✘ KHÔNG nói phần mềm tự trình bày nơi nhận (chưa soát mã).
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, veCC, theApp } from "./loat.mjs";

/** dòng của bảng «loại văn bản có gì»: ten, kg (Kính gửi: true/false), nn (Nơi nhận: true/false), hien = mốc hiện */
const dong = (ten, phu, kg, nn, hien, tren) =>
  the(`<div class="c09-d"><div class="t">${ten}<small>${phu}</small></div><div class="c ${kg ? "co" : "khong"}">${kg ? "✓" : "✗"}</div><div class="c ${nn ? "co" : "khong"}">${nn ? "✓" : "✗"}</div></div>`,
    { tren, hien, lop: "c09-the" });

export default {
  ten: "09-kinh-gui-va-noi-nhan",
  tieuDe: "Thể thức 09 · «Kính gửi» và «Nơi nhận»: loại văn bản nào có cả hai? Cách ghi theo NĐ 30/2020",
  bia: { dong: ["Kính gửi, Nơi nhận:", "loại nào có cả hai?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Phần thứ nhất bao gồm từ “Kính gửi” … Phần thứ hai bao gồm từ “Nơi nhận”, *phía dưới là từ “Như trên”*, tiếp theo là tên các cơ quan, tổ chức, đơn vị và cá nhân có liên quan khác nhận văn bản.»",
      theNguon("Phụ lục I · mục II.9 · điểm b, d"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Kính gửi, và Nơi nhận, chỉ ba loại văn bản có cả hai.", chu: "«Kính gửi» + «Nơi nhận» — *chỉ 3 loại* có cả hai" },
      { loi: "Còn Quyết định, có Kính gửi không?", chu: "Còn Quyết định, *có «Kính gửi» không?*", nghiSau: 0.2 },
    ]},
    { canh: "bang", cau: [
      { loi: "Tờ trình, Báo cáo gửi lên cấp trên, và Công văn.", chu: "*Tờ trình* · *Báo cáo* gửi cấp trên · *Công văn*" },
      { loi: "Các loại khác, như Quyết định, chỉ có Nơi nhận.", chu: "Loại khác, như *Quyết định*: chỉ *«Nơi nhận»*", nghiSau: 0.3 },
    ]},
    { canh: "kinh-gui", cau: [
      { loi: "Gửi một nơi, ghi cùng một dòng.", chu: "Gửi *một nơi*: cùng *một dòng*" },
      { loi: "Từ hai nơi, xuống dòng, gạch đầu dòng, cuối dòng chấm phẩy.", chu: "Từ *hai nơi*: xuống dòng · gạch đầu dòng · cuối dòng *( ; )*", nghiSau: 0.3 },
    ]},
    { canh: "noi-nhan", cau: [
      { loi: "Có Kính gửi, thì Nơi nhận mở đầu bằng, Như trên.", chu: "Có «Kính gửi» → «Nơi nhận» mở đầu bằng *«Như trên»*" },
      { loi: "Rồi các nơi khác, dòng cuối là Lưu.", chu: "Rồi *các nơi khác* · dòng cuối *«Lưu: VT, …»*" },
      { loi: "Quyết định không có Kính gửi, nên Nơi nhận không có Như trên.", chu: "Quyết định: không có «Kính gửi» → *bỏ «Như trên»*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Phần thứ hai bao gồm từ Nơi nhận, phía dưới là từ Như trên.", chu: "…từ “Nơi nhận”, phía dưới là từ “Như trên”…" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.9 · điểm b, d"),
    ]),
  ],

  css: CSS_CC + `
.c09-kn{display:flex;justify-content:center;gap:30px;padding:46px 20px 52px;font-family:"Times New Roman",serif;font-size:64px;font-weight:700;color:#111}
.c09-kn span{padding:8px 26px;border-radius:18px;background:#f1f5f9}
.c09-kn span.b{font-style:italic}
.c09-qd{padding:36px 30px 42px;text-align:center;font-family:"Times New Roman",serif}
.c09-qd b{font-size:56px}
.c09-qd .h{display:inline-block;margin-left:26px;padding:4px 22px;border-radius:14px;background:#fef3c7;box-shadow:0 0 0 4px #f59e0b;font-size:60px;font-weight:700}
.c09-the{padding:0}
.c09-d{display:flex;align-items:center;padding:20px 34px}
.c09-d .t{flex:1;font-size:46px;font-weight:900;color:#0f172a;line-height:1.1}
.c09-d .t small{display:block;margin-top:6px;font-size:26px;font-weight:700;color:#64748b}
.c09-d .c{flex:none;width:150px;text-align:center;font-size:60px;font-weight:900}
.c09-d .co{color:#15803d}.c09-d .khong{color:#dc2626}
.c09-cot{display:flex;justify-content:flex-end;gap:0;padding:6px 34px 0;font-size:28px;font-weight:800;color:#64748b;text-align:center}
.c09-cot span{width:150px}
.c09-kg{padding:34px 40px 40px;font-family:"Times New Roman",serif;font-size:54px;line-height:1.35;color:#111}
.c09-kg .nh{font-family:system-ui,sans-serif;font-size:26px;font-weight:800;color:#64748b;margin-bottom:10px}
.c09-kg .ds{padding-left:216px}
.c09-kg .hl{border-radius:6px}
.c09-kg .hl.on{color:#fff;background:#f59e0b;padding:0 5px}
.c09-nn{padding:34px 40px 42px;font-family:"Times New Roman",serif;font-size:50px;line-height:1.4;color:#111}
.c09-nn .t{font-style:italic;font-weight:700;font-size:54px;margin-bottom:6px}
.c09-nn .nt{border-radius:8px}
.c09-nn .g.gach{color:#dc2626;text-decoration:line-through;text-decoration-thickness:5px}
.c09-nn .nt.on{background:#fef3c7;box-shadow:0 0 0 4px #f59e0b}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + hai nhãn «Kính gửi:» «Nơi nhận:» · câu 2: «Quyết định — Kính gửi?»
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Kính gửi, Nơi nhận —<br>*loại nào có cả hai?*`, { tren: 262, co: 78, gach: "c0k1w2" })}
  ${the(`<div class="c09-kn"><span>Kính gửi:</span><span class="b">Nơi nhận:</span></div>`, { tren: 700, rung: "c0k1w1" })}
  ${the(`<div class="c09-qd"><b>QUYẾT ĐỊNH</b><span class="h">Kính gửi: ?</span></div>`, { tren: 940, hien: "c1k0" })}`,
      ve: veCC,
    },

    // bảng: loại nào có «Kính gửi», loại nào có «Nơi nhận»
    "bang": {
      khai: true,
      html: () => `
  ${chipKhai("📋 Loại nào có gì?")}
  <div class="c09-cot" style="position:absolute;left:70px;top:316px;width:940px"><span>Kính gửi</span><span>Nơi nhận</span></div>
  ${dong("Tờ trình", "cấp dưới gửi cấp trên", true, true, "s-2", 360)}
  ${dong("Báo cáo", "cấp dưới gửi cấp trên", true, true, "c0k1", 520)}
  ${dong("Công văn", "", true, true, "c0k2", 680)}
  ${dong("Quyết định", "và các loại khác", false, true, "c1k0", 860)}`,
    },

    // «Kính gửi»: một nơi (cùng dòng) · từ hai nơi (xuống dòng, gạch đầu dòng thẳng hàng dưới dấu hai chấm, ; và .)
    "kinh-gui": {
      khai: true,
      html: () => `
  ${chipKhai("✉️ Cách ghi «Kính gửi»")}
  ${the(`<div class="c09-kg"><div class="nh">GỬI MỘT NƠI</div>Kính gửi: Sở A.</div>`, { tren: 380, hien: "s-2" })}
  ${the(`<div class="c09-kg"><div class="nh">GỬI TỪ HAI NƠI TRỞ LÊN</div>Kính gửi:<div class="ds"><span class="hl" data-cc="lop" data-lop="on" data-moc="c1k2">-</span> Sở A<span class="hl" data-cc="lop" data-lop="on" data-moc="c1k3w2">;</span></div><div class="ds"><span class="hl" data-cc="lop" data-lop="on" data-moc="c1k2">-</span> Sở B<span class="hl" data-cc="lop" data-lop="on" data-moc="c1k3w3">.</span></div></div>`, { tren: 690, hien: "c1k0" })}
  ${goiKhai("<span>Gạch đầu dòng <b>thẳng hàng</b> dưới dấu hai chấm</span>", { trai: 60, tren: 1070, hien: "c1k3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // «Nơi nhận» mở bằng «Như trên» · dòng cuối «Lưu»
    "noi-nhan": {
      khai: true,
      html: () => `
  ${chipKhai("📮 Cách ghi «Nơi nhận»")}
  ${the(`<div class="c09-nn"><div class="t">Nơi nhận:</div>
    <div><span class="nt" data-cc="lop" data-lop="on" data-moc="c0k2"><span class="g" data-cc="lop" data-lop="gach" data-moc="c2k1">- Như trên;</span></span></div>
    <div>- Sở C;</div>
    <div><span class="nt" data-cc="lop" data-lop="on" data-moc="c1k1">- Lưu: VT, VP (2).</span></div></div>`, { tren: 380, hien: "s-2" })}
  ${goiKhai("<span><b>«Như trên»</b> = các nơi đã ghi ở «Kính gửi»</span>", { trai: 60, tren: 840, hien: "c0k2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Dòng cuối: <b>Lưu</b> · VT · đơn vị soạn · số bản · <b>( . )</b></span>", { trai: 60, tren: 960, hien: "c1k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Quyết định: <b>bỏ «Như trên»</b> — chỉ liệt kê nơi nhận</span>", { trai: 60, tren: 1080, hien: "c2k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-qd", lucNode(tatCa, "moc", "c1k0w2")],
    ["02a-ba-loai", lucNode(tatCa, "bang", "c0k2w2")],
    ["02b-quyet-dinh", lucNode(tatCa, "bang", "c1k1w2")],
    ["03a-mot-noi", lucNode(tatCa, "kinh-gui", "c0k1w2")],
    ["03b-hai-noi", lucNode(tatCa, "kinh-gui", "c1k3w2")],
    ["04a-nhu-tren", lucNode(tatCa, "noi-nhan", "c0k2w1")],
    ["04b-luu", lucNode(tatCa, "noi-nhan", "c1k1w3")],
    ["04c-bo", lucNode(tatCa, "noi-nhan", "c2k1w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k0w2"),
};
