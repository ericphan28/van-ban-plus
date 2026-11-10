// ============================================================================
// THỂ THỨC 05 — DÒNG ĐỊA DANH (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục II.4 điểm c)
//
// Khuôn ĐẬP HIỂU LẦM: móc dòng «Đồng Nai ngày 15…» chữ đứng, thiếu dấu phẩy → quy định viết hoa chữ cái đầu + dấu phẩy sau
// địa danh → chữ thường, nghiêng, cỡ 13–14, canh giữa dưới Quốc hiệu, Tiêu ngữ → app → Tóm lại → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 53):
//   «c) Địa danh và thời gian ban hành văn bản được trình bày trên cùng một dòng với số, ký hiệu văn bản, tại ô số 4 Mục IV
//   Phần I Phụ lục này, bằng chữ in thường, cỡ chữ từ 13 đến 14, kiểu chữ nghiêng; các chữ cái đầu của địa danh phải viết
//   hoa; sau địa danh có dấu phẩy (,); địa danh và ngày, tháng, năm được đặt dưới, canh giữa so với Quốc hiệu và Tiêu ngữ.»
//   ✔ «các chữ cái đầu của địa danh, phải viết hoa» · ✔ «sau địa danh, có dấu phẩy» (rồi đến ngày tháng năm — cùng dòng)
//   ✔ «chữ in thường, kiểu chữ nghiêng, cỡ chữ mười ba đến mười bốn» · ✔ «đặt dưới, canh giữa so với Quốc hiệu, Tiêu ngữ»
//   ✔ móc «chữ đứng, quên dấu phẩy là chưa đúng» — trái «kiểu chữ nghiêng» và «sau địa danh có dấu phẩy».
//   ✔ «Đồng Nai» là tên tỉnh (địa danh mẫu như clip 01), không tên cơ quan thật; ngày 15 tháng 3 đúng điểm b (không thêm 0).
//   ✔ cảnh app: như clip 01 (câu cố định). KHÔNG nói phần mềm tự trình bày đúng dòng địa danh (chưa soát mã).
//   Khác KE-HOACH: không — bảng ghi «chữ cái đầu viết hoa», nguyên văn «các chữ cái đầu của địa danh phải viết hoa».
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai, lopKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, theApp, veCC } from "./loat.mjs";

const NGAY = "ngày 15 tháng 3 năm 2026";

export default {
  ten: "05-dia-danh",
  tieuDe: "Thể thức 05 · Dòng địa danh trình bày thế nào? Địa danh, ngày tháng theo NĐ 30/2020",
  bia: { dong: ["Dòng địa danh", "viết sao đúng?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«…bằng chữ in thường, cỡ chữ từ 13 đến 14, kiểu chữ nghiêng; *các chữ cái đầu của địa danh phải viết hoa; sau địa danh có dấu phẩy (,)*…»",
      theNguon("Phụ lục I · mục II.4 · điểm c"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Dòng địa danh, ngày tháng, anh chị trình bày thế nào?", chu: "Địa danh, ngày tháng — *trình bày thế nào?*" },
      { loi: "Chữ đứng, quên dấu phẩy, là chưa đúng.", chu: "Chữ đứng · quên dấu phẩy — *chưa đúng*" },
    ]},
    { canh: "quy-dinh", cau: [
      { loi: "Nghị định ba mươi quy định, các chữ cái đầu của địa danh, phải viết hoa.", chu: "Chữ cái đầu của địa danh — *viết hoa*" },
      { loi: "Sau địa danh, có dấu phẩy, rồi đến ngày tháng năm.", chu: "Sau địa danh — *dấu phẩy* — rồi đến ngày tháng", nghiSau: 0.3 },
    ]},
    { canh: "kieu-chu", cau: [
      { loi: "Cả dòng viết chữ in thường, kiểu chữ nghiêng, cỡ chữ mười ba đến mười bốn.", chu: "Chữ thường · *nghiêng* · cỡ *13–14*" },
      { loi: "Dòng này đặt dưới, canh giữa so với Quốc hiệu, Tiêu ngữ.", chu: "Đặt dưới · *canh giữa* so với Quốc hiệu, Tiêu ngữ", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["chữ cái đầu địa danh viết hoa", "Chữ đầu địa danh: HOA"],
      ["sau địa danh có dấu phẩy", "Sau địa danh: dấu phẩy"],
      ["chữ thường nghiêng cỡ mười ba đến mười bốn", "Thường · nghiêng · 13–14"],
    ]),
    canhKet([
      { loi: "Kiểu chữ nghiêng, các chữ cái đầu của địa danh phải viết hoa, sau địa danh có dấu phẩy.", chu: "…kiểu chữ nghiêng; các chữ cái đầu của địa danh phải viết hoa; sau địa danh có dấu phẩy (,)" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.4 · điểm c"),
    ]),
  ],

  css: CSS_CC + `
.c05-dong{padding:34px 10px;text-align:center;font-family:"Times New Roman",serif;font-size:52px;white-space:nowrap;color:#111}
.c05-dong.nghieng{font-style:italic}
.c05-dong .o-trong{display:inline-block;width:34px;height:60px;margin:0 8px -10px;border:6px dashed #dc2626;border-radius:8px;background:#fee2e2}
.c05-dong .phay{display:inline-block;color:#fff;background:#16a34a;border-radius:8px;padding:0 10px;margin-left:14px;font-weight:700}
.c05-dong .ngay.sang{background:#dbeafe;border-radius:8px;box-shadow:0 0 0 6px #dbeafe}
.c05-lon{position:absolute;left:0;width:100%;text-align:center;font-family:"Times New Roman",serif;font-size:132px;line-height:1.15;color:#0f172a}
.c05-lon s{color:#94a3b8;font-size:104px;text-decoration-thickness:10px;text-decoration-color:#dc2626}
.c05-lon b{color:#fff;background:var(--mau-chinh);border-radius:14px;padding:0 8px;font-weight:700}
.c05-dau{position:relative;padding:40px 20px 46px;text-align:center;font-family:"Times New Roman",serif;color:#111}
.c05-dau .qh{font-size:34px;font-weight:700;white-space:nowrap}
.c05-dau .tn{display:inline-block;margin-top:6px;padding-bottom:8px;font-size:38px;font-weight:700;white-space:nowrap;border-bottom:3px solid #111}
.c05-dau .dd{margin-top:36px;font-size:44px;font-style:italic;white-space:nowrap}
.c05-dau .truc{position:absolute;left:calc(50% - 2px);top:14px;bottom:14px;border-left:5px dashed var(--mau-chinh)}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + dòng SAI (chữ đứng, thiếu dấu phẩy) · câu 2: rung, ô trống chỗ dấu phẩy, dấu «CHƯA ĐÚNG»
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Dòng địa danh —<br>*trình bày thế nào?*`, { tren: 262, co: 84, gach: "c0k2w2" })}
  ${the(`<div class="c05-dong">Đồng Nai<b class="o-trong" data-k="lop" data-hien="c1k1w1" style="opacity:0"></b> ${NGAY}</div>`, { tren: 720, rung: "c1k0w1" })}
  ${dau("✗ CHƯA ĐÚNG", "c1k2w1", { tren: 960, co: 92 })}`,
      ve: veCC,
    },

    // quy định: «đồng nai» → «Đồng Nai» (chữ đầu viết hoa) · rồi dòng có dấu phẩy sau địa danh
    "quy-dinh": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${lopKhai(`<div class="c05-lon"><s>đồng nai</s><br><b>Đ</b>ồng <b>N</b>ai</div>`, "c0k1", "c1", "left:0;top:400px;width:100%;height:360px")}
  ${goiKhai("<span>Chữ cái đầu của địa danh: <b>viết hoa</b></span>", { trai: 60, tren: 820, hien: "c0k2", an: "c1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${the(`<div class="c05-dong nghieng">Đồng Nai<b class="phay" data-k="lop" data-hien="c1k1" style="opacity:0">,</b> <span class="ngay" data-cc="lop" data-lop="sang" data-moc="c1k2">${NGAY}</span></div>`, { tren: 420, hien: "c1" })}
  ${goiKhai("<span>Sau địa danh có <b>dấu phẩy ( , )</b></span>", { trai: 60, tren: 640, hien: "c1k1+0.3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // kiểu chữ: dòng đứng → nghiêng, cỡ 13–14 · rồi khối Quốc hiệu, Tiêu ngữ, địa danh canh giữa
    "kieu-chu": {
      khai: true,
      html: () => `
  ${chipKhai("🔤 Kiểu chữ · vị trí")}
  ${the(`<div class="c05-dong">Đồng Nai, ${NGAY}</div>`, { tren: 420, hien: "s-2", an: "c0k1w2" })}
  ${the(`<div class="c05-dong nghieng">Đồng Nai, ${NGAY}</div>`, { tren: 420, hien: "c0k1w2", an: "c1" })}
  ${goiKhai("<span>Chữ <b>in thường</b> · kiểu chữ <b>nghiêng</b></span>", { trai: 60, tren: 640, hien: "c0k1+0.3", an: "c1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Cỡ chữ <b>13 – 14</b></span>", { trai: 60, tren: 780, hien: "c0k2w2", an: "c1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${the(`<div class="c05-dau"><div class="qh">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div><div class="tn">Độc lập - Tự do - Hạnh phúc</div>
    <div class="dd">Đồng Nai, ${NGAY}</div><i class="truc" data-k="lop" data-hien="c1k1w1" style="opacity:0"></i></div>`, { tren: 400, hien: "c1" })}
  ${goiKhai("<span><b>Canh giữa</b> dưới Quốc hiệu, Tiêu ngữ</span>", { trai: 60, tren: 820, hien: "c1k1+0.4", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-sai", lucNode(tatCa, "moc", "c1k2w1+0.3")],
    ["02a-viet-hoa", lucNode(tatCa, "quy-dinh", "c0k2w2")],
    ["02b-phay", lucNode(tatCa, "quy-dinh", "c1k2w3")],
    ["03a-nghieng", lucNode(tatCa, "kieu-chu", "c0k2w5")],
    ["03b-canh-giua", lucNode(tatCa, "kieu-chu", "c1k2w1")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k2w1+0.3"),
};
