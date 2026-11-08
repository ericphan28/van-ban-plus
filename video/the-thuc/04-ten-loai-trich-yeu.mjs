// ============================================================================
// THỂ THỨC 04 — TÊN LOẠI, TRÍCH YẾU VÀ ĐƯỜNG KẺ BÊN DƯỚI (Nghị định 30/2020/NĐ-CP, Phụ lục I, Phần I, mục II.5 điểm b)
//
// SOÁT với nguyên văn (phu-luc-I.md dòng 56): «Tên loại văn bản được trình bày bằng chữ in hoa, cỡ chữ từ 13 đến 14, kiểu chữ
// đứng, đậm. Trích yếu nội dung văn bản được đặt ngay dưới tên loại văn bản, trình bày bằng chữ in thường, cỡ chữ từ 13 đến
// 14, kiểu chữ đứng, đậm. Bên dưới trích yếu nội dung văn bản có đường kẻ ngang, nét liền, có độ dài bằng từ 1/3 đến 1/2 độ
// dài của dòng chữ và đặt cân đối so với dòng chữ.»
//   ✔ «kẻ hết dòng là sai» (dài hơn 1/2 dòng chữ → trái quy định) · ✔ «nét liền, một phần ba đến một nửa dòng chữ»
//   ✔ «đặt cân đối» · ✔ «tên loại in hoa đậm» · ✔ «trích yếu chữ thường, in đậm, cỡ 13–14».
//   ✔ ví dụ «QUYẾT ĐỊNH · Về việc ban hành Quy chế làm việc» — chung chung, không cơ quan thật.
//   ✔ cảnh app: như clip 01.
// ============================================================================
import { canhKet, canhTomTat, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, theApp, veCC } from "./loat.mjs";

/** khối tên loại + trích yếu; `ke` = [{ rong %, mau, hien, an }] các đường kẻ chồng nhau */
const khoi = (ke) => `<div class="c04-khoi"><div class="ten">QUYẾT ĐỊNH</div><div class="ty">Về việc ban hành Quy chế làm việc</div>
  <div class="ke-hang">${ke.map((k) => `<i class="${k.mau}" data-k="lop" data-hien="${k.hien}"${k.an ? ` data-an="${k.an}"` : ""} style="opacity:0;width:${k.rong}%"></i>`).join("")}</div></div>`;

export default {
  ten: "04-ten-loai-trich-yeu",
  tieuDe: "Thể thức 04 · Đường kẻ dưới trích yếu dài bao nhiêu? Tên loại, trích yếu theo NĐ 30/2020",
  bia: { dong: ["Kẻ dưới trích", "yếu dài bao nhiêu?"], vai: "Nghị định 30 · Phụ lục I" },
  ket: {
    the: [
      "«Bên dưới trích yếu nội dung văn bản có đường kẻ ngang, nét liền, có độ dài bằng *từ 1/3 đến 1/2 độ dài của dòng chữ* và đặt cân đối so với dòng chữ.»",
      theNguon("Phụ lục I · mục II.5 · điểm b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Dưới dòng trích yếu, đường kẻ ngang dài bao nhiêu?", chu: "Đường kẻ dưới trích yếu — *dài bao nhiêu?*" },
      { loi: "Kẻ hết dòng, là chưa đúng.", chu: "Kẻ *hết dòng* là *chưa đúng*" },
    ]},
    { canh: "ke", cau: [
      { loi: "Đường kẻ nét liền, dài từ một phần ba, đến một nửa dòng chữ.", chu: "Nét liền · dài *1/3 đến 1/2* dòng chữ" },
      { loi: "Và đặt cân đối, ngay dưới trích yếu.", chu: "Đặt *cân đối* dưới trích yếu", nghiSau: 0.3 },
    ]},
    { canh: "chu", cau: [
      { loi: "Tên loại văn bản, viết chữ in hoa, chữ đậm.", chu: "Tên loại: *IN HOA* · *đậm*" },
      { loi: "Trích yếu viết chữ thường, cũng in đậm, cỡ chữ mười ba đến mười bốn.", chu: "Trích yếu: *chữ thường* · đậm · cỡ *13–14*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhTomTat("Tóm lại", [
      ["tên loại in hoa đậm", "Tên loại: IN HOA · đậm"],
      ["trích yếu chữ thường in đậm", "Trích yếu: thường · đậm"],
      ["kẻ một phần ba đến một nửa dòng", "Kẻ 1/3 – 1/2 dòng chữ"],
    ]),
    canhKet([
      { loi: "Bên dưới trích yếu có đường kẻ ngang, nét liền, độ dài bằng từ một phần ba, đến một nửa độ dài của dòng chữ.", chu: "…đường kẻ ngang, nét liền, dài từ 1/3 đến 1/2 độ dài của dòng chữ." },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.5 · điểm b"),
    ]),
  ],

  css: CSS_CC + `
.c04-khoi{padding:46px 30px 52px;text-align:center;font-family:"Times New Roman",serif;color:#111}
.c04-khoi .ten{font-size:64px;font-weight:700;letter-spacing:.02em}
.c04-khoi .ty{margin-top:8px;font-size:48px;font-weight:700;white-space:nowrap}
.c04-khoi .ke-hang{position:relative;height:6px;margin:26px auto 0;width:760px}
.c04-khoi .ke-hang i{position:absolute;left:50%;top:0;height:5px;transform:translateX(-50%);background:#111}
.c04-khoi .ke-hang i.do{background:#dc2626;height:6px}
.c04-khoi .ke-hang i.xanh{background:#16a34a;height:6px}
.c04-nhan{font-size:42px!important;font-weight:900;border-left:12px solid var(--mau-chinh)}
.c04-nhan b{color:var(--mau-chinh)}
.c04-thuoc{position:absolute;left:70px;width:940px;text-align:center;font-size:34px;font-weight:800;color:#64748b}
.c04-thuoc b{color:#16a34a}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Kẻ dưới trích yếu —<br>*dài bao nhiêu?*`, { tren: 262, co: 80, gach: "c0k1w5" })}
  ${the(khoi([{ rong: 100, mau: "do", hien: "s-2" }]), { tren: 640, rung: "c1k0w2" })}
  ${dau("✗ HẾT DÒNG", "c1k1w1", { tren: 1010, co: 80 })}`,
      ve: veCC,
    },

    "ke": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Nghị định 30 · Phụ lục I")}
  ${the(khoi([{ rong: 100, mau: "do", hien: "s-2", an: "c0k0w2" }, { rong: 33, mau: "xanh", hien: "c0k1", an: "c0k2" }, { rong: 50, mau: "xanh", hien: "c0k2" }]), { tren: 380 })}
  ${goiKhai("<span>Nét liền · dài <b>1/3 → 1/2</b> dòng chữ</span>", { trai: 60, tren: 760, hien: "c0k2w2", tu: "duoi", rong: 960, lop: "c04-nhan" })}
  ${goiKhai("<span>Đặt <b>cân đối</b> · ngay dưới trích yếu</span>", { trai: 60, tren: 900, hien: "c1k1", tu: "duoi", rong: 960, lop: "c04-nhan" })}`,
    },

    "chu": {
      khai: true,
      html: () => `
  ${chipKhai("🔤 Tên loại · trích yếu")}
  ${the(khoi([{ rong: 42, mau: "", hien: "s-2" }]), { tren: 380 })}
  ${goiKhai("<span>Tên loại: <b>IN HOA</b> · đậm</span>", { trai: 60, tren: 760, hien: "c0k1", tu: "duoi", rong: 960, lop: "c04-nhan" })}
  ${goiKhai("<span>Trích yếu: <b>chữ thường</b> · đậm · cỡ <b>13–14</b></span>", { trai: 60, tren: 900, hien: "c1k0w2", tu: "duoi", rong: 960, lop: "c04-nhan" })}`,
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
    ["01b-het-dong", lucNode(tatCa, "moc", "c1k1w2+0.3")],
    ["02a-mot-phan-ba", lucNode(tatCa, "ke", "c0k1w2")],
    ["02b-mot-nua", lucNode(tatCa, "ke", "c1k1w3")],
    ["03a-trich-yeu", lucNode(tatCa, "chu", "c1k2w4")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1k1w2+0.3"),
};
