// ============================================================================
// THỂ THỨC 08 — TÊN CƠ QUAN, TỔ CHỨC BAN HÀNH (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục II.2 điểm b)
//
// Khuôn ĐẬP HIỂU LẦM: móc hai dòng tên cơ quan đều in đậm → chủ quản in hoa, đứng; cơ quan ban hành in hoa, ĐẬM, canh giữa
// dưới → đường kẻ 1/3–1/2 dòng chữ, cân đối, hai dòng cách nhau dòng đơn → app → Tóm lại → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 35–37):
//   «b) Tên cơ quan, tổ chức ban hành văn bản được trình bày bằng chữ in hoa, cỡ chữ từ 12 đến 13, kiểu chữ đứng, đậm, được
//   đặt canh giữa dưới tên cơ quan, tổ chức chủ quản trực tiếp; phía dưới có đường kẻ ngang, nét liền, có độ dài bằng từ 1/3
//   đến 1/2 độ dài của dòng chữ và đặt cân đối so với dòng chữ.
//   Tên cơ quan, tổ chức chủ quản trực tiếp được trình bày bằng chữ in hoa, cỡ chữ từ 12 đến 13, kiểu chữ đứng.
//   Tên cơ quan, tổ chức ban hành văn bản và tên cơ quan, tổ chức chủ quản trực tiếp được trình bày cách nhau dòng đơn. …»
//   ✔ «chủ quản: chữ in hoa, cỡ mười hai đến mười ba, kiểu chữ đứng» (nguyên văn không ghi «đậm» cho dòng chủ quản)
//   ✔ «cơ quan ban hành: cũng in hoa, thêm chữ đậm, canh giữa bên dưới» (đứng, đậm, canh giữa dưới tên chủ quản)
//   ✔ móc «in đậm cả hai dòng là chưa đúng»: chỉ dòng cơ quan ban hành được ghi «đậm».
//   ✔ «đường kẻ nét liền, dài từ một phần ba đến một nửa dòng chữ» (= 1/3 đến 1/2; câu trùng clip 04, lấy từ đệm giọng)
//   ✔ «đặt cân đối so với dòng chữ» · ✔ «hai dòng tên cách nhau dòng đơn»
//   ✔ chữ trên hình «TÊN CQ, TC CHỦ QUẢN» / «TÊN CƠ QUAN, TỔ CHỨC» như mẫu của Nghị định — không tên cơ quan thật.
//   ✔ cảnh app: như clip 01 (câu cố định). KHÔNG nói phần mềm tự trình bày đúng tên cơ quan (chưa soát mã).
//   Khác KE-HOACH: không — bảng ghi «Tên cơ quan ban hành in hoa đậm, đường kẻ 1/3–1/2», khớp nguyên văn.
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, thanh, theApp, veCC } from "./loat.mjs";

export default {
  ten: "08-ten-co-quan",
  tieuDe: "Thể thức 08 · Tên cơ quan ban hành: dòng nào in đậm? Đường kẻ dài bao nhiêu theo NĐ 30/2020",
  bia: { dong: ["Tên cơ quan:", "dòng nào đậm?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Tên cơ quan, tổ chức ban hành văn bản được trình bày bằng chữ in hoa, cỡ chữ từ 12 đến 13, *kiểu chữ đứng, đậm*, được đặt canh giữa dưới tên cơ quan, tổ chức chủ quản trực tiếp…»",
      theNguon("Phụ lục I · mục II.2 · điểm b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Tên cơ quan chủ quản, và tên cơ quan ban hành, dòng nào in đậm?", chu: "Hai dòng tên cơ quan — *dòng nào in đậm?*" },
      { loi: "In đậm cả hai dòng, là chưa đúng.", chu: "Đậm cả hai dòng — *chưa đúng*" },
    ]},
    { canh: "hai-dong", cau: [
      { loi: "Tên cơ quan chủ quản, chữ in hoa, cỡ mười hai đến mười ba, kiểu chữ đứng.", chu: "Cơ quan chủ quản: *IN HOA* · cỡ 12–13 · *chữ đứng*" },
      { loi: "Tên cơ quan ban hành, cũng in hoa, thêm chữ đậm, canh giữa bên dưới.", chu: "Cơ quan ban hành: IN HOA · *đậm* · canh giữa bên dưới", nghiSau: 0.3 },
    ]},
    { canh: "ke", cau: [
      { loi: "Đường kẻ nét liền, dài từ một phần ba, đến một nửa dòng chữ.", chu: "Nét liền · dài *1/3 đến 1/2* dòng chữ" },
      { loi: "Đặt cân đối so với dòng chữ, hai dòng tên cách nhau dòng đơn.", chu: "Đặt *cân đối* · hai dòng tên cách nhau *dòng đơn*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["chủ quản in hoa chữ đứng", "Chủ quản: HOA · đứng"],
      ["cơ quan ban hành in hoa đậm", "Ban hành: HOA · đậm"],
      ["kẻ dưới một phần ba đến một nửa", "Kẻ dưới: 1/3 – 1/2"],
    ]),
    canhKet([
      { loi: "Tên cơ quan, tổ chức ban hành văn bản, được trình bày bằng chữ in hoa, cỡ chữ từ mười hai đến mười ba, kiểu chữ đứng, đậm.", chu: "Tên cơ quan, tổ chức ban hành văn bản được trình bày bằng chữ in hoa, cỡ chữ từ 12 đến 13, kiểu chữ đứng, đậm" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.2 · điểm b"),
    ]),
  ],

  css: CSS_CC + `
.c08-cq{padding:48px 10px 64px;text-align:center;font-family:"Times New Roman",serif;color:#111;line-height:1.15}
.c08-cq .d{display:inline-block;white-space:nowrap;font-size:54px;padding:4px 14px;border-radius:12px}
.c08-cq .d1{font-weight:400}
.c08-cq .d2{position:relative;font-weight:700}
.c08-cq.dam .d1{font-weight:700;color:#b91c1c;background:#fee2e2}
.c08-cq.dam .d2{color:#b91c1c;background:#fee2e2}
.c08-cq .sang{background:#fef3c7;box-shadow:0 0 0 4px #f59e0b}
.c08-cq i.ke{position:absolute;left:50%;transform:translateX(-50%);bottom:-22px;height:5px;width:40%;background:#111}
.c08-cq i.ke.dai{width:0}
.c08-cq .moc3,.c08-cq .moc2{position:absolute;bottom:-62px;height:26px;border:3px solid #2563eb;border-top:0;font-family:system-ui,sans-serif;font-size:30px;font-weight:800;color:#2563eb;box-sizing:border-box;opacity:0;transition:none}
.c08-cq .moc3{left:33.33%;width:33.33%}
.c08-cq .moc2{left:25%;width:50%;bottom:-100px}
.c08-cq .moc3 b,.c08-cq .moc2 b{position:absolute;right:-66px;top:-8px}
.c08-cq.thuoc .moc3,.c08-cq.thuoc .moc2{opacity:1}
.c08-cq.ke-the{padding-bottom:130px}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + hai dòng đều đậm · câu 2: rung + dấu «ĐẬM CẢ HAI»
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Tên cơ quan —<br>*dòng nào in đậm?*`, { tren: 262, co: 84, gach: "c0k2w3" })}
  ${the(`<div class="c08-cq dam"><span class="d d1">TÊN CQ, TC CHỦ QUẢN</span><br><span class="d d2">TÊN CƠ QUAN, TỔ CHỨC<i class="ke"></i></span></div>`, { tren: 720, rung: "c1k0w2" })}
  ${dau("✗ ĐẬM CẢ HAI", "c1k1w1", { tren: 1020, co: 88 })}`,
      ve: veCC,
    },

    // chủ quản: in hoa, đứng (bỏ đậm) · cơ quan ban hành: in hoa, đậm, canh giữa dưới
    "hai-dong": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${the(`<div class="c08-cq" data-cc="lop" data-lop="dam" data-moc="s-2" data-an="c0k3"><span class="d d1" data-cc="lop" data-lop="sang" data-moc="c0k0" data-an="c1">TÊN CQ, TC CHỦ QUẢN</span><br><span class="d d2" data-cc="lop" data-lop="sang" data-moc="c1k0">TÊN CƠ QUAN, TỔ CHỨC<i class="ke"></i></span></div>`, { tren: 420, hien: "s-2" })}
  ${goiKhai("<span>Chủ quản: <b>IN HOA</b> · cỡ 12–13 · <b>chữ đứng</b></span>", { trai: 60, tren: 740, hien: "c0k3w1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Ban hành: IN HOA · <b>đậm</b> · canh giữa dưới</span>", { trai: 60, tren: 880, hien: "c1k2w1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // đường kẻ: mọc từ giữa → 1/3 → 1/2 → dừng cân đối · thước 1/3, 1/2 · hai dòng cách nhau dòng đơn
    "ke": {
      khai: true,
      html: () => `
  ${chipKhai("📐 Đường kẻ dưới tên cơ quan")}
  ${the(`<div class="c08-cq ke-the" data-cc="lop" data-lop="thuoc" data-moc="c0k1w3"><span class="d d1">TÊN CQ, TC CHỦ QUẢN</span><span class="d d2">TÊN CƠ QUAN, TỔ CHỨC${thanh("c0k1w3@0>33;c0k2w2@33>50;c1k0w1@50>42", { lop: "ke dai", dai: 0.6 })}<span class="moc3"><b>1/3</b></span><span class="moc2"><b>1/2</b></span></span></div>`, { tren: 400, hien: "s-2" })}
  ${goiKhai("<span>Nét liền · dài <b>1/3 đến 1/2</b> dòng chữ</span>", { trai: 60, tren: 820, hien: "c0k2w4", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Đặt <b>cân đối</b> so với dòng chữ</span>", { trai: 60, tren: 960, hien: "c1k0w2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Hai dòng tên cách nhau <b>dòng đơn</b></span>", { trai: 60, tren: 1100, hien: "c1k1w4", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // giới thiệu phần mềm (tên chỉ trên hình)
    "app": {
      khai: true,
      html: () => `
  ${chipKhai("🤖 Phần mềm soạn văn bản")}
  ${theApp("c0k0")}`,
    },
  },

  mocThu: (tatCa) => [
    ["01a-0.4s", 0.4],
    ["01b-dau", lucNode(tatCa, "moc", "c1k1w2+0.3")],
    ["02a-chu-quan", lucNode(tatCa, "hai-dong", "c0k3w2")],
    ["02b-ban-hanh", lucNode(tatCa, "hai-dong", "c1k3w2")],
    ["03a-1-3", lucNode(tatCa, "ke", "c0k1w4")],
    ["03b-1-2", lucNode(tatCa, "ke", "c0k2w4+0.4")],
    ["03c-can-doi", lucNode(tatCa, "ke", "c1k1w6")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1w2+0.3"),
};
