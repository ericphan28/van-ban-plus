// ============================================================================
// THỂ THỨC 03 — SỐ TRANG ĐẶT Ở ĐÂU? (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục I.7)
//
// SOÁT với nguyên văn (phu-luc-I.md dòng 26): «Số trang văn bản: Được đánh từ số 1, bằng chữ số Ả Rập, cỡ chữ 13 đến 14,
// kiểu chữ đứng, được đặt canh giữa theo chiều ngang trong phần lề trên của văn bản, không hiển thị số trang thứ nhất.»
//   ✔ «giữa lề trên», «không phải góc dưới» (quy định đặt trong lề trên — góc dưới là sai) · ✔ «đánh từ số một, cỡ 13–14,
//   chữ đứng» · ✔ «trang thứ nhất không hiện số» · ✔ «số hai hiện từ trang thứ hai» (đánh từ 1, ẩn trang 1 → trang 2 hiện 2).
//   ✔ cảnh app: như clip 01. ✘ KHÔNG nói phần mềm tự đánh số trang (chưa soát).
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, theApp, veCC } from "./loat.mjs";

/** tờ A4 nhỏ (360 × 509) với số trang giữa lề trên (hiện lúc `hien`, ẩn hẳn nếu `so` rỗng) và số SAI ở góc dưới */
const to = ({ trai, tren, so = "", hien = "", sai = "", saiAn = "", nhanTo = "" }) => `<div class="tt-to c03-to" style="left:${trai}px;top:${tren}px;width:360px;height:509px">
  ${so ? `<b class="c03-so" data-k="lop" data-hien="${hien}" style="opacity:0">${so}</b>` : ""}
  ${sai ? `<b class="c03-sai" data-k="lop" data-hien="${sai}"${saiAn ? ` data-an="${saiAn}"` : ""} style="opacity:0">${so || 1}</b>` : ""}
  <div class="c03-vung">${Array.from({ length: 9 }, (_, i) => `<i style="width:${i === 8 ? 45 : 90 + (i % 3) * 4}%"></i>`).join("")}</div>
  ${nhanTo ? `<em>${nhanTo}</em>` : ""}</div>`;

export default {
  ten: "03-so-trang",
  tieuDe: "Thể thức 03 · Số trang văn bản hành chính đặt ở đâu? Giữa lề trên, trang đầu không hiện số",
  bia: { dong: ["Số trang", "đặt ở đâu?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«…được đặt *canh giữa theo chiều ngang trong phần lề trên* của văn bản, *không hiển thị số trang thứ nhất*.»",
      theNguon("Phụ lục I · mục I.7"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản hành chính, số trang đặt ở đâu?", chu: "Số trang *đặt ở đâu?*" },
      { loi: "Không phải góc dưới, mà là giữa lề trên.", chu: "Không phải góc dưới — *giữa lề trên*" },
    ]},
    { canh: "quy-dinh", cau: [
      { loi: "Số trang đặt giữa, trong phần lề trên của văn bản.", chu: "Đặt *giữa* · trong *lề trên*" },
      { loi: "Đánh từ số một, cỡ chữ mười ba đến mười bốn, chữ đứng.", chu: "Đánh từ *1* · cỡ *13–14* · chữ đứng", nghiSau: 0.3 },
    ]},
    { canh: "trang-dau", cau: [
      { loi: "Trang thứ nhất, không hiện số trang.", chu: "Trang thứ nhất: *không hiện số*" },
      { loi: "Số hai bắt đầu hiện, từ trang thứ hai.", chu: "Từ trang 2 mới hiện *số 2*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["đặt giữa lề trên", "Giữa lề trên"],
      ["trang đầu không hiện số", "Trang 1 không hiện số"],
      ["cỡ chữ mười ba đến mười bốn", "Cỡ 13–14 · chữ đứng"],
    ]),
    canhKet([
      { loi: "Số trang được đặt canh giữa, trong phần lề trên của văn bản, không hiển thị số trang thứ nhất.", chu: "…canh giữa trong phần lề trên, không hiển thị số trang thứ nhất." },
      nguonCau("Phụ lục một", "Phụ lục I · mục I.7"),
    ]),
  ],

  css: CSS_CC + `
.c03-to{overflow:visible}
.c03-vung{position:absolute;left:52px;right:26px;top:60px;padding-top:24px}
.c03-vung i{display:block;height:11px;margin:0 0 20px;border-radius:6px;background:#e2e8f0}
.c03-so{position:absolute;left:0;width:100%;top:10px;text-align:center;font-size:40px;font-weight:700;color:#15803d}
.c03-sai{position:absolute;right:22px;bottom:12px;font-size:34px;font-weight:700;color:#b91c1c}
.c03-to em{position:absolute;left:0;width:100%;bottom:-62px;text-align:center;font-family:"Be Vietnam Pro",sans-serif;font-style:normal;font-size:32px;font-weight:800;color:#475569}
.c03-an{position:absolute;left:0;width:100%;top:6px;text-align:center;font-family:"Be Vietnam Pro",sans-serif;font-size:26px;font-weight:800;color:#94a3b8}
.c02-nhan,.c03-nhan{font-size:42px!important;font-weight:900;border-left:12px solid var(--mau-chinh)}
.c03-nhan b{color:var(--mau-chinh)}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Số trang —<br>*đặt ở đâu?*`, { tren: 262, co: 84, gach: "c0k1w4" })}
  ${to({ trai: 360, tren: 600, so: "2", hien: "c1k1w3", sai: "s-2", saiAn: "c1k1w3" })}
  ${dau("✗ GÓC DƯỚI", "c1k0w3", { tren: 1010, co: 72, an: "c1k1w3" })}
  ${goiKhai("<span>✓ <b>Giữa lề trên</b></span>", { trai: 60, tren: 1150, hien: "c1k1w3", tu: "duoi", rong: 960, lop: "c03-nhan" })}`,
      ve: veCC,
    },

    "quy-dinh": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${to({ trai: 360, tren: 360, so: "2", hien: "s" })}
  ${goiKhai("<span>Canh <b>giữa</b> · trong <b>lề trên</b></span>", { trai: 60, tren: 940, hien: "c0k1", tu: "duoi", rong: 960, lop: "c03-nhan" })}
  ${goiKhai("<span>Từ số <b>1</b> · cỡ <b>13–14</b> · chữ đứng</span>", { trai: 60, tren: 1080, hien: "c1k0", tu: "duoi", rong: 960, lop: "c03-nhan" })}`,
    },

    "trang-dau": {
      khai: true,
      html: () => `
  ${chipKhai("📄 Trang 1 · trang 2")}
  ${to({ trai: 110, tren: 380, nhanTo: "Trang 1" })}
  <div class="c03-an" data-k="lop" data-hien="c0k1" style="opacity:0;position:absolute;left:110px;width:360px;top:392px">(không hiện số)</div>
  ${to({ trai: 610, tren: 380, so: "2", hien: "c1k0w2", nhanTo: "Trang 2" })}
  ${goiKhai("<span>Trang 1 <b>ẩn số</b> · trang 2 hiện <b>2</b></span>", { trai: 60, tren: 1010, hien: "c1k1", tu: "duoi", rong: 960, lop: "c03-nhan" })}`,
    },

    "app": {
      khai: true,
      html: () => `
  ${chipKhai("🤖 Phần mềm soạn văn bản")}
  ${theApp("c0k0")}`,
    },
  },

  mocThu: (tatCa) => [
    ["01a-0.4s", 0.4],
    ["01b-goc", lucNode(tatCa, "moc", "c1k0w3+0.3")],
    ["01c-giua", lucNode(tatCa, "moc", "c1k1w4")],
    ["02a-giua", lucNode(tatCa, "quy-dinh", "c1k2w1")],
    ["03a-hai", lucNode(tatCa, "trang-dau", "c1k1w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1w4"),
};
