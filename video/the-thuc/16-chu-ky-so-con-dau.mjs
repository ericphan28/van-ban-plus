// ============================================================================
// THỂ THỨC 16 — CHỮ KÝ SỐ VÀ CON DẤU: DẤU ĐỎ TRÙM CHỮ KÝ BAO NHIÊU? (Nghị định 30/2020/NĐ-CP, Phụ lục I, II.7 điểm d; II.8 điểm a, b)
//
// Khuôn HỎI – ĐÁP CÓ MINH HỌA ĐỘNG: móc «dấu trùm bao nhiêu?» → chữ ký xanh nền trong suốt đặt giữa chức vụ và họ tên → dấu đỏ kích
// thước thật → dấu NHẢY sang trùm 1/3 chữ ký về bên trái (có thước 1/3) → văn bản kèm theo không cùng tệp: ký số riêng, góc trên bên
// phải trang đầu → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn «không hiển thị hình, chỉ hiện số, ký hiệu, thời gian ký».
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 88, 92–97):
//   «d) Hình ảnh, vị trí chữ ký số của người có thẩm quyền là hình ảnh chữ ký của người có thẩm quyền trên văn bản giấy, màu xanh, định
//   dạng Portable Network Graphics (.png) nền trong suốt; đặt canh giữa chức vụ của người ký và họ tên người ký.»
//   «8. a) Hình ảnh, vị trí chữ ký số của cơ quan, tổ chức là hình ảnh dấu của cơ quan, tổ chức ban hành văn bản trên văn bản, màu đỏ,
//   kích thước bằng kích thước thực tế của dấu, định dạng (.png) nền trong suốt, trùm lên khoảng 1/3 hình ảnh chữ ký số của người có thẩm
//   quyền về bên trái.
//   b) Chữ ký số của cơ quan, tổ chức trên văn bản kèm theo văn bản chính được thể hiện như sau: Văn bản kèm theo cùng tệp tin với nội
//   dung văn bản điện tử, Văn thư cơ quan chỉ thực hiện ký số văn bản và không thực hiện ký số lên văn bản kèm theo; văn bản không cùng
//   tệp tin với nội dung văn bản điện tử, Văn thư cơ quan thực hiện ký số của cơ quan, tổ chức trên văn bản kèm theo.
//   Vị trí: Góc trên, bên phải, trang đầu của văn bản kèm theo.
//   Hình ảnh chữ ký số của cơ quan, tổ chức: Không hiển thị.
//   Thông tin: Số và ký hiệu văn bản; thời gian ký (ngày tháng năm; giờ phút giây; múi giờ Việt Nam theo tiêu chuẩn ISO 8601) được trình
//   bày bằng phông chữ Times New Roman, chữ in thường, kiểu chữ đứng, cỡ chữ 10, màu đen.»
//   ✔ chữ ký xanh, nền trong suốt, canh giữa chức vụ và họ tên · ✔ dấu đỏ, kích thước thật, trùm khoảng 1/3 chữ ký về bên trái
//   ✔ văn bản kèm theo không cùng tệp → Văn thư ký số riêng, góc trên bên phải trang đầu · ✔ hình không hiển thị, chỉ có số, ký hiệu,
//     thời gian ký (thẻ màn kết)
//   ✔ «GIÁM ĐỐC», «Nguyễn Văn A», hình chữ ký vẽ tay chỉ là giữ chỗ; ghi «ví dụ minh họa» · ✘ KHÔNG nói phần mềm tự ký số.
//   Lời đọc KHÔNG đọc «png», «ISO 8601», «Times New Roman» (chỉ viết trên hình nếu cần).
// ============================================================================
import { canhKet, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, veCC, theApp } from "./loat.mjs";

const SVG_KY = `<svg viewBox="0 0 320 120" width="320" height="120"><path d="M10 84 C 40 6, 66 128, 96 52 S 150 18, 176 72 S 236 116, 308 28" stroke="#1d4ed8" stroke-width="7" fill="none" stroke-linecap="round"/></svg>`;

export default {
  ten: "16-chu-ky-so-con-dau",
  tieuDe: "Thể thức 16 · Chữ ký số: dấu đỏ trùm chữ ký bao nhiêu? Màu chữ ký, vị trí theo NĐ 30/2020",
  bia: { dong: ["Dấu đỏ trùm", "chữ ký bao nhiêu?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Hình ảnh chữ ký số của cơ quan, tổ chức: *Không hiển thị.* Thông tin: *Số và ký hiệu văn bản; thời gian ký* (ngày tháng năm; giờ phút giây; múi giờ Việt Nam…) … chữ in thường, kiểu chữ đứng, cỡ chữ 10, màu đen.»",
      theNguon("Phụ lục I · mục II.8 · điểm b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Chữ ký số trên văn bản điện tử, dấu đỏ trùm lên chữ ký, trùm bao nhiêu là đúng?", chu: "Chữ ký số — dấu đỏ trùm lên chữ ký *bao nhiêu là đúng?*" },
      { loi: "Con số nằm ngay trong Nghị định.", chu: "Con số nằm *ngay trong Nghị định*", nghiSau: 0.2 },
    ]},
    { canh: "chu-ky", cau: [
      { loi: "Hình ảnh chữ ký màu xanh, nền trong suốt, đặt canh giữa chức vụ và họ tên.", chu: "Chữ ký: *màu xanh* · *nền trong suốt* · canh giữa *chức vụ* và *họ tên*" },
      { loi: "Dấu của cơ quan, màu đỏ, kích thước bằng dấu thật.", chu: "Dấu cơ quan: *màu đỏ* · kích thước *bằng dấu thật*" },
      { loi: "Dấu trùm lên khoảng một phần ba chữ ký, về bên trái.", chu: "Dấu trùm khoảng *1/3 chữ ký* · về *bên trái*", nghiSau: 0.3 },
    ]},
    { canh: "kem-theo", cau: [
      { loi: "Văn bản kèm theo không cùng tệp, thì Văn thư ký số riêng, ở góc trên bên phải, trang đầu.", chu: "Văn bản kèm theo *không cùng tệp* → ký số riêng · *góc trên bên phải* trang đầu", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Hình ảnh chữ ký số không hiển thị, chỉ hiện số, ký hiệu, và thời gian ký.", chu: "Hình chữ ký số *không hiển thị* · chỉ hiện *số, ký hiệu, thời gian ký*" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.8 · điểm b"),
    ]),
  ],

  css: CSS_CC + `
.c16-ky{position:relative;height:420px;font-family:"Times New Roman",serif;color:#111;text-align:center}
.c16-ky .cv{position:absolute;left:0;right:0;top:22px;font-size:52px;font-weight:700}
.c16-ky .ht{position:absolute;left:0;right:0;bottom:44px;font-size:62px;font-weight:700}
.c16-ky .sg{position:absolute;left:310px;top:112px;width:320px;height:120px;opacity:0}
.c16-ky .sg.on{opacity:1}
.c16-ky .sg svg{display:block}
.c16-ky .sl{position:absolute;left:310px;top:264px;width:320px;height:22px;border-left:4px solid #2563eb;border-right:4px solid #2563eb;opacity:0}
.c16-ky .sl.on{opacity:1}
.c16-ky .sl i{position:absolute;left:0;top:0;width:33.33%;height:16px;border-top:5px solid #2563eb;background:rgba(37,99,235,.12)}
.c16-ky .sl b{position:absolute;left:-206px;top:-8px;font-family:system-ui,sans-serif;font-size:32px;font-weight:900;color:#2563eb;white-space:nowrap}
.c16-ky .ds{position:absolute;left:64px;top:62px;width:190px;height:190px;border-radius:50%;border:9px solid #dc2626;background:rgba(220,38,38,.07);color:#dc2626;display:flex;align-items:center;justify-content:center;font-family:system-ui,sans-serif;font-size:40px;font-weight:900;opacity:0;line-height:1.05}
.c16-ky .ds.on{opacity:1}
.c16-ky .ds.tru{left:222px}
.c16-ky .vd{position:absolute;right:26px;bottom:10px;font-family:system-ui,sans-serif;font-size:24px;font-weight:700;color:#94a3b8}
.c16-tr{position:relative;height:500px;margin:0 auto;width:420px;border:5px solid #94a3b8;border-radius:12px;background:#fff}
.c16-tr .th{position:absolute;right:14px;top:12px;width:250px;font-family:"Times New Roman",serif;font-size:19px;line-height:1.3;color:#111;text-align:left;opacity:0}
.c16-tr .th.on{opacity:1;outline:3px solid #f59e0b;outline-offset:6px;border-radius:4px}
.c16-tr .ln{position:absolute;left:34px;right:34px;height:12px;border-radius:6px;background:#e2e8f0}
.c16-nt{display:flex;justify-content:center;gap:24px;padding:20px 20px 0;font-size:34px;font-weight:900;color:#0f172a}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Chữ ký số —<br>*dấu đỏ trùm bao nhiêu?*`, { tren: 262, co: 78, gach: "c0k2" })}
  ${the(`<div class="c16-ky" style="height:330px"><div class="cv" style="font-size:44px">GIÁM ĐỐC</div><div class="sg on" style="top:80px;left:310px">${SVG_KY}</div><div class="ds on tru" style="top:50px;width:170px;height:170px">DẤU</div><div class="ht" style="font-size:52px;bottom:26px">Nguyễn Văn A</div><div class="vd">ví dụ minh họa</div></div>`, { tren: 700, rung: "c1k0w2" })}
  ${dau("?", "c1k0w4", { tren: 1060, co: 120 })}`,
      ve: veCC,
    },

    "chu-ky": {
      khai: true,
      html: () => `
  ${chipKhai("🖋️ Chữ ký số · con dấu")}
  ${the(`<div class="c16-ky"><div class="cv">GIÁM ĐỐC</div>
    <div class="sg" data-cc="lop" data-lop="on" data-moc="c0k0">${SVG_KY}</div>
    <div class="ds" data-cc="lop" data-lop="on" data-moc="c1k0" data-an="c2k0">DẤU<br>ĐỎ</div>
    <div class="ds tru" data-cc="lop" data-lop="on" data-moc="c2k0">DẤU<br>ĐỎ</div>
    <div class="sl" data-cc="lop" data-lop="on" data-moc="c2k1"><i></i><b>1/3 chữ ký</b></div>
    <div class="ht">Nguyễn Văn A</div><div class="vd">ví dụ minh họa</div></div>`, { tren: 360, hien: "s-2" })}
  ${goiKhai("<span>Chữ ký <b>xanh</b> · nền trong suốt · giữa chức vụ và họ tên</span>", { trai: 60, tren: 830, hien: "c0k2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Dấu <b>đỏ</b> · kích thước <b>bằng dấu thật</b></span>", { trai: 60, tren: 960, hien: "c1k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Trùm khoảng <b>1/3</b> chữ ký, về <b>bên trái</b></span>", { trai: 60, tren: 1090, hien: "c2k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    "kem-theo": {
      khai: true,
      html: () => `
  ${chipKhai("📎 Văn bản kèm theo không cùng tệp")}
  ${the(`<div style="padding:40px 20px 44px"><div class="c16-tr"><div class="th" data-cc="lop" data-lop="on" data-moc="c0k2">Số và ký hiệu văn bản;<br>thời gian ký (ngày tháng năm; giờ phút giây; múi giờ)</div><div class="ln" style="top:170px"></div><div class="ln" style="top:210px"></div><div class="ln" style="top:250px;right:140px"></div><div class="ln" style="top:310px"></div><div class="ln" style="top:350px"></div><div class="ln" style="top:390px;right:200px"></div></div></div>`, { tren: 340, hien: "s-2" })}
  ${goiKhai("<span>Văn thư ký số <b>riêng</b> · góc <b>trên bên phải</b> trang đầu</span>", { trai: 60, tren: 1010, hien: "c0k2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-dau", lucNode(tatCa, "moc", "c1k0w4")],
    ["02a-xanh", lucNode(tatCa, "chu-ky", "c0k2w3")],
    ["02b-do", lucNode(tatCa, "chu-ky", "c1k2w2")],
    ["02c-1-3", lucNode(tatCa, "chu-ky", "c2k1w2")],
    ["03a-goc", lucNode(tatCa, "kem-theo", "c0k2w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k0w4"),
};
