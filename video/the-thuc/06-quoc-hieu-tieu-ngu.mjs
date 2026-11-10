// ============================================================================
// THỂ THỨC 06 — QUỐC HIỆU VÀ TIÊU NGỮ (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục II.1 điểm a, b)
//
// Khuôn ĐẬP HIỂU LẦM: móc «Độc lập-Tự do-Hạnh phúc» viết liền → quy định gạch nối có cách chữ, chữ đầu mỗi cụm viết hoa,
// kẻ dưới dài bằng dòng chữ → cỡ chữ Quốc hiệu 12–13, Tiêu ngữ 13–14 → app → Tóm lại → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md dòng 29–30):
//   «a) Quốc hiệu “CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM”: Được trình bày bằng chữ in hoa, cỡ chữ từ 12 đến 13, kiểu chữ đứng,
//   đậm và ở phía trên cùng, bên phải trang đầu tiên của văn bản.»
//   «b) Tiêu ngữ “Độc lập - Tự do - Hạnh phúc”: Được trình bày bằng chữ in thường, cỡ chữ từ 13 đến 14, kiểu chữ đứng, đậm và
//   được canh giữa dưới Quốc hiệu; chữ cái đầu của các cụm từ được viết hoa, giữa các cụm từ có gạch nối (-), có cách chữ;
//   phía dưới có đường kẻ ngang, nét liền, có độ dài bằng độ dài của dòng chữ.»
//   ✔ «giữa các cụm từ có gạch nối, có cách chữ» → viết liền «Độc lập-Tự do-Hạnh phúc» là chưa đúng
//   ✔ «chữ cái đầu mỗi cụm viết hoa» · ✔ «phía dưới kẻ đường nét liền, dài bằng dòng chữ»
//   ✔ «Quốc hiệu chữ in hoa, chữ đứng và đậm, cỡ mười hai đến mười ba» · ✔ «Tiêu ngữ chữ thường, đứng, đậm, cỡ mười ba đến mười bốn»
//   ✔ cảnh app: như clip 01 (câu cố định). KHÔNG nói phần mềm tự trình bày đúng Quốc hiệu, Tiêu ngữ (chưa soát mã).
//   ✘ Không quốc huy, không tên cơ quan.
//   Khác KE-HOACH: không — bảng ghi «Quốc hiệu cỡ 12–13 in hoa đậm; Tiêu ngữ 13–14, gạch nối có cách chữ», khớp nguyên văn.
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, thanh, theApp, veCC } from "./loat.mjs";

export default {
  ten: "06-quoc-hieu-tieu-ngu",
  tieuDe: "Thể thức 06 · Tiêu ngữ: gạch nối viết liền hay có cách? Quốc hiệu, Tiêu ngữ theo NĐ 30/2020",
  bia: { dong: ["Tiêu ngữ:", "liền hay cách?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«…chữ cái đầu của các cụm từ được viết hoa, *giữa các cụm từ có gạch nối (-), có cách chữ*; phía dưới có đường kẻ ngang, nét liền, có độ dài bằng độ dài của dòng chữ.»",
      theNguon("Phụ lục I · mục II.1 · điểm a, b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Dòng Tiêu ngữ, gạch nối viết liền, hay có cách chữ?", chu: "Tiêu ngữ — gạch nối *viết liền hay có cách?*" },
      { loi: "Viết liền như thế này, là chưa đúng.", chu: "Viết liền — *chưa đúng*" },
    ]},
    { canh: "tieu-ngu", cau: [
      { loi: "Nghị định ba mươi quy định, giữa các cụm từ có gạch nối, có cách chữ.", chu: "Giữa các cụm từ: gạch nối — *có cách chữ*" },
      { loi: "Chữ cái đầu mỗi cụm viết hoa, phía dưới kẻ đường nét liền, dài bằng dòng chữ.", chu: "Chữ đầu cụm *viết hoa* · kẻ dưới *dài bằng dòng chữ*", nghiSau: 0.3 },
    ]},
    { canh: "co-chu", cau: [
      { loi: "Quốc hiệu viết chữ in hoa, chữ đứng và đậm, cỡ chữ mười hai đến mười ba.", chu: "Quốc hiệu: *IN HOA* · đứng · đậm · cỡ *12–13*" },
      { loi: "Tiêu ngữ chữ thường, cũng đứng và đậm, cỡ chữ mười ba đến mười bốn.", chu: "Tiêu ngữ: chữ thường · đứng · đậm · cỡ *13–14*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["Quốc hiệu in hoa cỡ mười hai đến mười ba", "Quốc hiệu: HOA · 12–13"],
      ["Tiêu ngữ đậm cỡ mười ba đến mười bốn", "Tiêu ngữ: đậm · 13–14"],
      ["gạch nối có cách chữ", "Gạch nối có cách chữ"],
    ]),
    canhKet([
      { loi: "Chữ cái đầu của các cụm từ được viết hoa, giữa các cụm từ có gạch nối, có cách chữ.", chu: "…chữ cái đầu của các cụm từ được viết hoa, giữa các cụm từ có gạch nối (-), có cách chữ" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.1 · điểm a, b"),
    ]),
  ],

  css: CSS_CC + `
.c06-tn{padding:42px 10px 50px;text-align:center;font-family:"Times New Roman",serif;font-size:62px;font-weight:700;white-space:nowrap;color:#111}
.c06-tn .lien{color:#b91c1c;background:#fee2e2;border-radius:6px;padding:0 3px}
.c06-tn .cach{white-space:pre;color:#15803d;background:#dcfce7;border-radius:8px}
.c06-tn .chu{position:relative;display:inline-block}
.c06-tn .chu i.ke{position:absolute;left:0;bottom:-16px;height:5px;width:0;background:#111}
.c06-tn u{text-decoration:none;border-radius:6px}
.c06-tn.hoa u{color:#fff;background:var(--mau-chinh)}
.c06-dau{padding:46px 20px 54px;text-align:center;font-family:"Times New Roman",serif;color:#111}
.c06-dau .qh,.c06-dau .tn{display:inline-block;font-weight:700;white-space:nowrap;padding:8px 14px;border-radius:12px}
.c06-dau .qh{font-size:36px}
.c06-dau .tn{margin-top:10px;font-size:40px}
.c06-dau .tn span{display:inline-block;padding-bottom:8px;border-bottom:3px solid #111}
.c06-dau .sang{background:#fef3c7;box-shadow:0 0 0 4px #f59e0b}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + Tiêu ngữ viết liền · câu 2: rung + dấu «VIẾT LIỀN»
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Tiêu ngữ: gạch nối —<br>*viết liền hay cách?*`, { tren: 262, co: 84, gach: "c0k2w2" })}
  ${the(`<div class="c06-tn">Độc lập<b class="lien">-</b>Tự do<b class="lien">-</b>Hạnh phúc</div>`, { tren: 720, rung: "c1k0w1" })}
  ${dau("✗ VIẾT LIỀN", "c1k1w1", { tren: 970, co: 92 })}`,
      ve: veCC,
    },

    // Tiêu ngữ: liền → có cách · chữ đầu cụm viết hoa · đường kẻ dài bằng dòng chữ
    "tieu-ngu": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${the(`<div class="c06-tn">Độc lập<b class="lien">-</b>Tự do<b class="lien">-</b>Hạnh phúc</div>`, { tren: 420, hien: "s-2", an: "c0k2" })}
  ${the(`<div class="c06-tn" data-cc="lop" data-lop="hoa" data-moc="c1k0" data-an="c1k2"><span class="chu"><u>Đ</u>ộc lập<b class="cach"> - </b><u>T</u>ự do<b class="cach"> - </b><u>H</u>ạnh phúc${thanh("c1k1w2@0>100", { lop: "ke", dai: 0.7 })}</span></div>`, { tren: 420, hien: "c0k2" })}
  ${goiKhai("<span>Gạch nối ( - ) <b>có cách chữ</b></span>", { trai: 60, tren: 660, hien: "c0k2+0.3", an: "c1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Chữ cái đầu mỗi cụm <b>viết hoa</b></span>", { trai: 60, tren: 660, hien: "c1k0+0.3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Kẻ nét liền · dài <b>bằng dòng chữ</b></span>", { trai: 60, tren: 800, hien: "c1k2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
    },

    // cỡ chữ: khối Quốc hiệu + Tiêu ngữ, sáng từng dòng
    "co-chu": {
      khai: true,
      html: () => `
  ${chipKhai("🔠 Cỡ chữ · kiểu chữ")}
  ${the(`<div class="c06-dau"><div class="qh" data-cc="lop" data-lop="sang" data-moc="c0" data-an="c1">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div><br>
    <div class="tn" data-cc="lop" data-lop="sang" data-moc="c1"><span>Độc lập - Tự do - Hạnh phúc</span></div></div>`, { tren: 400, hien: "s-2" })}
  ${goiKhai("<span>Quốc hiệu: <b>IN HOA</b> · đứng · đậm · cỡ <b>12–13</b></span>", { trai: 60, tren: 720, hien: "c0k2w2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Tiêu ngữ: chữ thường · đứng · đậm · cỡ <b>13–14</b></span>", { trai: 60, tren: 860, hien: "c1k2w2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}`,
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
    ["01b-lien", lucNode(tatCa, "moc", "c1k1w2+0.3")],
    ["02a-cach", lucNode(tatCa, "tieu-ngu", "c0k2w2")],
    ["02b-ke", lucNode(tatCa, "tieu-ngu", "c1k2w3")],
    ["03a-quoc-hieu", lucNode(tatCa, "co-chu", "c0k2w5")],
    ["03b-tieu-ngu", lucNode(tatCa, "co-chu", "c1k2w5")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1w2+0.3"),
};
