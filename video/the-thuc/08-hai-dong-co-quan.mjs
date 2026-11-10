// ============================================================================
// THỂ THỨC 08 (làm lại 10/11/2026) — HAI DÒNG TÊN CƠ QUAN: DÒNG NÀO ĐƯỢC VIẾT TẮT? (Nghị định 30/2020/NĐ-CP, Phụ lục I, II.2 a, b)
//
// Khuôn HỎI – ĐÁP CÓ MINH HỌA (thay bản cũ chỉ đọc «in hoa, cỡ 12–13, đậm, kẻ 1/3–1/2» bị chê nhàm, ít kiến thức):
//   móc «chỉ một dòng được viết tắt» → dòng trên (chủ quản): viết tắt cụm từ thông dụng + ở địa phương phải có tên xã/huyện/tỉnh
//   → dòng dưới (cơ quan ban hành): tên chính thức, đầy đủ + in hoa đậm → đường kẻ 1/3–1/2 → thẻ nguyên văn.
//   Bỏ cảnh phần mềm + «Tóm lại» để dành thời lượng cho kiến thức (tên VanBanPlus vẫn ở màn kết).
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 33–36):
//   «a) Tên cơ quan, tổ chức ban hành văn bản là tên chính thức, đầy đủ của cơ quan, tổ chức hoặc chức danh nhà nước của người có
//   thẩm quyền ban hành văn bản. Tên cơ quan, tổ chức ban hành văn bản bao gồm tên của cơ quan, tổ chức ban hành văn bản và tên
//   của cơ quan, tổ chức chủ quản trực tiếp (nếu có).
//   Đối với tên cơ quan, tổ chức chủ quản trực tiếp ở địa phương có thêm tên tỉnh, thành phố trực thuộc trung ương hoặc huyện,
//   quận, thị xã, thành phố thuộc tỉnh, thành phố thuộc thành phố trực thuộc trung ương hoặc xã, phường, thị trấn nơi cơ quan, tổ
//   chức ban hành văn bản đóng trụ sở. Tên của cơ quan, tổ chức chủ quản trực tiếp được viết tắt những cụm từ thông dụng.»
//   «b) Tên cơ quan, tổ chức ban hành văn bản được trình bày bằng chữ in hoa, cỡ chữ từ 12 đến 13, kiểu chữ đứng, đậm, được đặt
//   canh giữa dưới tên cơ quan, tổ chức chủ quản trực tiếp; phía dưới có đường kẻ ngang, nét liền, có độ dài bằng từ 1/3 đến 1/2
//   độ dài của dòng chữ và đặt cân đối so với dòng chữ.
//   Tên cơ quan, tổ chức chủ quản trực tiếp được trình bày bằng chữ in hoa, cỡ chữ từ 12 đến 13, kiểu chữ đứng. …»
//   ✔ «dòng trên (chủ quản) được viết tắt những cụm từ thông dụng» — nguyên văn; ví dụ UBND chỉ là minh họa, không khẳng định bảng
//     viết tắt nào (Nghị định không liệt kê)
//   ✔ «ở địa phương, dòng chủ quản có thêm tên tỉnh, huyện hoặc xã nơi cơ quan ban hành đóng trụ sở» — nguyên văn
//   ✔ «dòng dưới là tên chính thức, đầy đủ» — nguyên văn (không có câu «cấm viết tắt», chỉ nói «chính thức, đầy đủ»; viết tắt
//     chỉ được nêu cho dòng chủ quản → suy ra «một dòng được viết tắt, dòng kia đầy đủ»)
//   ✔ «cả hai dòng in hoa, cỡ 12–13, chỉ dòng ban hành in đậm; đường kẻ nét liền dài 1/3–1/2, cân đối» — nguyên văn
//   ✔ dữ liệu minh họa «XÃ A», «VĂN PHÒNG» — giữ chỗ, không tên cơ quan thật; ghi «ví dụ minh họa» trên hình
//   ✘ KHÔNG nói phần mềm tự trình bày đúng tên cơ quan (chưa soát mã).
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, veCC, theApp } from "./loat.mjs";

/** chỗ đổi trên dòng tên: `sai` → `dung` từ mốc `moc` (mac dinh trung tinh = đen → xanh; `do` = đỏ → xanh) */
const fx = (sai, dung, moc, do_ = false) =>
  `<span class="fx${do_ ? " do" : ""}" data-cc="lop" data-lop="ok" data-moc="${moc}"><span class="s">${sai}</span><span class="d">${dung}</span></span>`;

/** thẻ hai dòng tên cơ quan: l1/l2 là HTML dòng trên/dưới; `ke` = HTML đường kẻ (tuỳ) */
const hai = (l1, l2, { cls = "", ke = "", extra = "" } = {}) =>
  `<div class="c08-cq ${cls}"${extra}><span class="d d1">${l1}</span><br><span class="d d2">${l2}${ke}</span><div class="vd">ví dụ minh họa</div></div>`;

export default {
  ten: "08-hai-dong-co-quan",
  tieuDe: "Thể thức 08 · Hai dòng tên cơ quan: dòng nào được viết tắt, dòng nào phải đầy đủ? Theo NĐ 30/2020",
  bia: { dong: ["Tên cơ quan:", "dòng nào viết tắt?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Tên cơ quan, tổ chức ban hành văn bản là *tên chính thức, đầy đủ* … Tên của cơ quan, tổ chức chủ quản trực tiếp *được viết tắt những cụm từ thông dụng.*»",
      theNguon("Phụ lục I · mục II.2 · điểm a, b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Hai dòng tên cơ quan trên đầu văn bản, dòng nào được viết tắt, dòng nào phải đầy đủ?", chu: "Hai dòng tên cơ quan — *dòng nào được viết tắt?*" },
      { loi: "Chỉ một dòng được viết tắt, dòng còn lại phải đầy đủ.", chu: "Chỉ *một dòng* được viết tắt", nghiSau: 0.2 },
    ]},
    { canh: "dong-tren", cau: [
      { loi: "Dòng trên là cơ quan chủ quản, được viết tắt những cụm từ thông dụng.", chu: "Dòng trên *cơ quan chủ quản* — được *viết tắt cụm từ thông dụng*" },
      { loi: "Ở địa phương, phải thêm tên tỉnh, huyện, hoặc xã, nơi cơ quan ban hành đóng trụ sở.", chu: "Địa phương: thêm *tên tỉnh · huyện · xã* nơi đóng trụ sở", nghiSau: 0.3 },
    ]},
    { canh: "dong-duoi", cau: [
      { loi: "Dòng dưới là cơ quan ban hành, phải ghi tên chính thức, đầy đủ.", chu: "Dòng dưới *cơ quan ban hành* — *tên chính thức, đầy đủ*" },
      { loi: "Cả hai dòng in hoa, cỡ chữ mười hai đến mười ba, chỉ dòng dưới in đậm.", chu: "Cả hai *IN HOA* · cỡ *12–13* · chỉ dòng dưới *đậm*" },
      { loi: "Đường kẻ nét liền, dài từ một phần ba, đến một nửa dòng chữ.", chu: "Kẻ nét liền · dài *1/3 đến 1/2* dòng chữ", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Tên của cơ quan chủ quản trực tiếp, được viết tắt những cụm từ thông dụng.", chu: "Tên của cơ quan, tổ chức chủ quản trực tiếp được viết tắt những cụm từ thông dụng" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.2 · điểm a, b"),
    ]),
  ],

  css: CSS_CC + `
.c08-cq{position:relative;padding:44px 10px 74px;text-align:center;font-family:"Times New Roman",serif;color:#111;line-height:1.2}
.c08-cq .d{display:inline-block;white-space:nowrap;font-size:56px;padding:4px 16px;border-radius:12px}
.c08-cq .d1{font-weight:400}
.c08-cq .d2{position:relative;font-weight:700;margin-top:6px}
.c08-cq .sang{background:#fef3c7;box-shadow:0 0 0 4px #f59e0b}
.c08-cq .fx .d{display:none}
.c08-cq .fx .s{color:#111}
.c08-cq .fx.do .s{color:#b91c1c;background:#fee2e2;border-radius:8px}
.c08-cq .fx.ok .s{display:none}
.c08-cq .fx.ok .d{display:inline;color:#15803d;background:#dcfce7;border-radius:8px;padding:0 4px}
.c08-cq .pl{border-radius:8px;padding:0 4px}
.c08-cq .pl.on{color:#15803d;background:#dcfce7}
.c08-cq .vd{position:absolute;right:16px;bottom:10px;font-family:system-ui,sans-serif;font-size:24px;font-weight:700;color:#94a3b8}
.c08-cq i.ke{position:absolute;left:50%;transform:translateX(-50%);bottom:-24px;height:6px;width:0;background:#111}
.c08-cq i.ke.on{width:40%}
.c08-cq .moc3,.c08-cq .moc2{position:absolute;height:26px;border:3px solid #2563eb;border-top:0;font-family:system-ui,sans-serif;font-size:30px;font-weight:800;color:#2563eb;box-sizing:border-box;opacity:0}
.c08-cq .moc3{left:33.33%;width:33.33%;bottom:-62px}
.c08-cq .moc2{left:25%;width:50%;bottom:-100px}
.c08-cq .moc3 b,.c08-cq .moc2 b{position:absolute;right:-66px;top:-8px}
.c08-cq.thuoc .moc3,.c08-cq.thuoc .moc2{opacity:1}
.c08-cq.ke-the{padding-bottom:130px}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + hai dòng nguyên (đầy đủ) với dấu «?» · câu 2: dấu «CHỈ MỘT DÒNG»
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Hai dòng tên cơ quan —<br>*dòng nào viết tắt?*`, { tren: 262, co: 78, gach: "c0k1w3" })}
  ${the(hai("ỦY BAN NHÂN DÂN XÃ A", "VĂN PHÒNG"), { tren: 700, rung: "c1k0w2" })}
  ${dau("✓ CHỈ MỘT DÒNG", "c1k1w1", { tren: 1000, co: 88, lop: "xanh" })}`,
      ve: veCC,
    },

    // dòng trên: viết tắt cụm từ thông dụng (ỦY BAN NHÂN DÂN → UBND) · thêm tên xã
    "dong-tren": {
      khai: true,
      html: () => `
  ${chipKhai("🏛️ Dòng trên · cơ quan chủ quản")}
  ${the(hai(`${fx("ỦY BAN NHÂN DÂN", "UBND", "c0k1w1")} <span class="pl" data-cc="lop" data-lop="on" data-moc="c1k1w4">XÃ A</span>`, "VĂN PHÒNG"), { tren: 400, hien: "s-2" })}
  ${goiKhai("<span>Chủ quản: <b>viết tắt cụm từ thông dụng</b></span>", { trai: 60, tren: 740, hien: "c0k1w2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Địa phương: thêm <b>tên xã · huyện · tỉnh</b></span>", { trai: 60, tren: 880, hien: "c1k1w4", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // dòng dưới: tên đầy đủ (VP → VĂN PHÒNG) · in hoa, đậm · đường kẻ 1/3–1/2
    "dong-duoi": {
      khai: true,
      html: () => `
  ${chipKhai("📌 Dòng dưới · cơ quan ban hành")}
  ${the(hai("UBND XÃ A", fx("VP", "VĂN PHÒNG", "c0k1", true), {
    cls: "ke-the",
    extra: ` data-cc="lop" data-lop="thuoc" data-moc="c2k1"`,
    ke: `<i class="ke" data-cc="lop" data-lop="on" data-moc="c2k0"></i><span class="moc3"><b>1/3</b></span><span class="moc2"><b>1/2</b></span>`,
  }), { tren: 380, hien: "s-2" })}
  ${goiKhai("<span>Ban hành: <b>tên chính thức, đầy đủ</b></span>", { trai: 60, tren: 760, hien: "c0k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Cả hai <b>IN HOA</b> · cỡ 12–13 · <b>dòng dưới đậm</b></span>", { trai: 60, tren: 890, hien: "c1k2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Kẻ nét liền · dài <b>1/3 đến 1/2</b> dòng chữ</span>", { trai: 60, tren: 1020, hien: "c2k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-dau", lucNode(tatCa, "moc", "c1k1w3")],
    ["02a-viet-tat", lucNode(tatCa, "dong-tren", "c0k1w4")],
    ["02b-xa", lucNode(tatCa, "dong-tren", "c1k1w6")],
    ["03a-day-du", lucNode(tatCa, "dong-duoi", "c0k1w4")],
    ["03b-dam", lucNode(tatCa, "dong-duoi", "c1k2w4")],
    ["03c-ke", lucNode(tatCa, "dong-duoi", "c2k1w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1w3"),
};
