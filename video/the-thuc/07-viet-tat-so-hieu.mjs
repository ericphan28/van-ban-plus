// ============================================================================
// THỂ THỨC 07 (làm lại 10/11/2026) — CHỮ VIẾT TẮT TÊN LOẠI VÀ DÒNG «SỐ» (Nghị định 30/2020/NĐ-CP)
//   Phụ lục III (bảng chữ viết tắt) · Phụ lục I mục II.3 điểm c · Điều 15 (cấp số)
//
// Khuôn BẮT LỖI ĐIỂN HÌNH (thay bản cũ «5 hay 05?» bị chê nhàm, ít kiến thức): móc «3 loại văn bản viết tắt phải xen chữ
// thường» → bảng TTr · CTr · QyĐ (+ phân biệt QĐ ≠ QyĐ) → ghép vào dòng số (Số: 05/TTr-UBND) → số chạy theo năm 01/01–31/12
// → thẻ nguyên văn. Bỏ cảnh phần mềm + «Tóm lại» để dành thời lượng cho kiến thức (tên VanBanPlus vẫn ở màn kết).
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/):
//   Phụ lục III, bảng I (phu-luc-III.md dòng 223–253): «Quyết định (cá biệt) | QĐ» · «Quy định | QyĐ» · «Chương trình | CTr» ·
//   «Tờ trình | TTr» · «Kế hoạch | KH» · «Báo cáo | BC» · «Biên bản | BB» · «Bản trích sao | TrS».
//   Phụ lục I II.3c (phu-luc-I.md dòng 44): «Từ “Số” được trình bày bằng chữ in thường, cỡ chữ 13, kiểu chữ đứng; sau từ “Số” có
//   dấu hai chấm (:); với những số nhỏ hơn 10 phải ghi thêm số 0 phía trước. … giữa các nhóm chữ viết tắt trong ký hiệu văn bản
//   có dấu gạch nối (-), không cách chữ.»
//   Điều 15 khoản 1 (noi-dung.md dòng 224): «Số và thời gian ban hành văn bản được lấy theo thứ tự và trình tự thời gian ban hành
//   văn bản của cơ quan, tổ chức trong năm (bắt đầu liên tiếp từ số 01 vào ngày 01 tháng 01 và kết thúc vào ngày 31 tháng 12 hàng
//   năm). Số và ký hiệu văn bản của cơ quan, tổ chức là duy nhất trong một năm…»
//   ✔ «ba loại viết tắt có chữ thường» = TTr (Tờ trình), CTr (Chương trình), QyĐ (Quy định) — đúng bảng; (TrS cũng có chữ
//     thường nhưng là bản sao, không nói để khỏi loãng)
//   ✔ «Quyết định là QĐ, Quy định là QyĐ» — đúng bảng (hai dòng khác nhau)
//   ✔ «viết hoa hết là sai»: TTR, CTR, QYĐ không có trong bảng (suy từ bảng, không trích câu nào)
//   ✔ dòng số «Số: 05/TTr-UBND»: UBND chỉ là ví dụ minh họa chữ viết tắt cơ quan, không tên cơ quan thật; «ký hiệu của văn bản
//     gồm chữ viết tắt tên loại và chữ viết tắt tên cơ quan» (II.3b)
//   ✘ KHÔNG nói phần mềm tự cấp số / tự ghi ký hiệu (chưa soát mã); không nói «nhiều người sai» như một số liệu.
// ============================================================================
import { canhKet, lucNode, chipKhai, goiKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, dau, the, veCC } from "./loat.mjs";

/** ô «viết tắt»: tên loại + chữ viết tắt (chữ thường tô xanh lúc `bat`) + dạng sai viết hoa hết */
const o = (ten, truoc, thuong, sau, sai, hien, bat) =>
  the(`<div class="c07-o"><small>${ten}</small><div class="vt">${truoc}<b class="lw" data-cc="lop" data-lop="on" data-moc="${bat}">${thuong}</b>${sau}</div><s>${sai}</s></div>`,
    { tren: hien.tren, hien: hien.moc, lop: "c07-the" });

/** một chỗ sửa trên dòng số: sai (đỏ) → đúng (xanh) từ mốc `moc` */
const fx = (sai, dung, moc) =>
  `<span class="fx" data-cc="lop" data-lop="ok" data-moc="${moc}"><span class="s">${sai}</span><span class="d">${dung}</span></span>`;

export default {
  ten: "07-viet-tat-so-hieu",
  tieuDe: "Thể thức 07 · TTr, CTr, QyĐ: 3 chữ viết tắt hay viết sai · Dòng «Số:» đúng NĐ 30/2020",
  bia: { dong: ["Viết tắt: 3 loại", "có chữ thường"], vai: "Nghị định 30 · Phụ lục III" },
  ket: {
    the: [
      "«…sau từ “Số” có dấu hai chấm (:); *với những số nhỏ hơn 10 phải ghi thêm số 0 phía trước.* … giữa các nhóm chữ viết tắt trong ký hiệu văn bản có dấu gạch nối (-), không cách chữ.»",
      theNguon("Phụ lục I · mục II.3 · điểm c · Phụ lục III · Điều 15"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Có ba loại văn bản, viết tắt phải có chữ thường, ghi hoa hết là sai.", chu: "3 loại văn bản — viết tắt *có chữ thường* · *hoa hết là sai*" },
      { loi: "Anh chị đoán được, là ba loại nào?", chu: "Anh chị đoán được *3 loại nào?*" },
    ]},
    { canh: "bang", cau: [
      { loi: "Tờ trình, và Chương trình, đều kết thúc bằng chữ e rờ thường.", chu: "Tờ trình *TTr* · Chương trình *CTr* — *r thường* ở cuối" },
      { loi: "Quy định, có chữ i dài thường nằm ở giữa.", chu: "Quy định *QyĐ* — *y thường* ở giữa" },
      { loi: "Còn Quyết định, chỉ có hai chữ hoa, đừng nhầm.", chu: "Quyết định *QĐ* ≠ Quy định *QyĐ*", nghiSau: 0.3 },
    ]},
    { canh: "dong-so", cau: [
      { loi: "Ghép vào dòng số, từ Số viết thường, có dấu hai chấm, số dưới mười thêm số không.", chu: "Dòng số: «Số» *chữ thường* · *dấu hai chấm* · số dưới 10 *thêm 0*" },
      { loi: "Gạch nối giữa các nhóm chữ, không cách chữ.", chu: "Nhóm chữ viết tắt: *gạch nối*, *không cách chữ*" },
      { loi: "Mỗi năm, số bắt đầu lại từ không một vào ngày một tháng một, và kết thúc ngày ba mươi mốt tháng mười hai.", chu: "Mỗi năm: *01/01 → số 01* · hết *31/12*", nghiSau: 0.3 },
    ]},
    canhKet([
      { loi: "Với những số nhỏ hơn mười, phải ghi thêm số không phía trước.", chu: "…với những số nhỏ hơn 10 phải ghi thêm số 0 phía trước" },
      nguonCau("Phụ lục một, Phụ lục ba, và Điều mười lăm", "Phụ lục I (II.3) · Phụ lục III · Điều 15"),
    ]),
  ],

  css: CSS_CC + `
.c07-ba{display:flex;justify-content:center;gap:34px;padding:42px 20px 50px}
.c07-ba i{font-style:normal;display:flex;align-items:center;justify-content:center;width:240px;height:190px;border:8px dashed #94a3b8;border-radius:28px;
  font-family:"Times New Roman",serif;font-size:130px;font-weight:700;color:#94a3b8}
.c07-the{padding:0}
.c07-o{display:flex;align-items:center;gap:26px;padding:22px 34px 24px}
.c07-o small{flex:none;width:270px;font-size:36px;font-weight:800;color:#475569;line-height:1.15}
.c07-o .vt{flex:1;font-family:"Times New Roman",serif;font-size:100px;font-weight:700;color:#111;white-space:nowrap}
.c07-o .vt .lw{font-weight:700;border-radius:12px;padding:0 6px;transition:none}
.c07-o .vt .lw.on{color:#fff;background:#16a34a}
.c07-o s{flex:none;font-family:"Times New Roman",serif;font-size:44px;font-weight:700;color:#dc2626;text-decoration-thickness:5px}
.c07-kk{padding:34px 24px 40px;text-align:center}
.c07-kk .a{display:inline-block;margin:0 18px;font-family:"Times New Roman",serif;font-size:84px;font-weight:700;color:#111}
.c07-kk .a em{display:block;margin-top:2px;font-family:system-ui,sans-serif;font-style:normal;font-size:30px;font-weight:800;color:#64748b}
.c07-kk .ne{display:inline-block;font-size:90px;font-weight:900;color:#dc2626;vertical-align:top;margin-top:6px}
.c07-so{padding:50px 10px 58px;text-align:center;font-family:"Times New Roman",serif;font-size:84px;white-space:nowrap;color:#111}
.c07-so .fx .d{display:none}
.c07-so .fx .s{color:#b91c1c;background:#fee2e2;border-radius:8px}
.c07-so .fx.ok .s{display:none}
.c07-so .fx.ok .d{display:inline;color:#15803d;background:#dcfce7;border-radius:8px}
.c07-nam{padding:30px 34px 36px}
.c07-nam .thanh{position:relative;height:26px;border-radius:99px;background:linear-gradient(90deg,#16a34a,#2563eb);margin:18px 0 14px}
.c07-nam .hai{display:flex;justify-content:space-between;font-size:38px;font-weight:900;color:#0f172a}
.c07-nam .hai small{display:block;font-size:28px;font-weight:800;color:#64748b}
.c07-nam .hai div:last-child{text-align:right}
`,

  canh: {
    // khung 0: ngữ cảnh + chữ móc + 3 ô «?» · câu 1: dấu «HOA HẾT LÀ SAI» · câu 2: rung
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Ba loại văn bản —<br>*viết tắt có chữ thường*`, { tren: 262, co: 78, gach: "c0k2" })}
  ${the(`<div class="c07-ba"><i>?</i><i>?</i><i>?</i></div>`, { tren: 700, rung: "c1k0w2" })}
  ${dau("✗ HOA HẾT LÀ SAI", "c0k2w1", { tren: 1010, co: 80 })}`,
      ve: veCC,
    },

    // bảng: Tờ trình TTr · Chương trình CTr · Quy định QyĐ · so Quyết định QĐ
    "bang": {
      khai: true,
      html: () => `
  ${chipKhai("📖 Phụ lục III · Bảng chữ viết tắt")}
  ${o("Tờ trình", "TT", "r", "", "TTR ✗", { tren: 330, moc: "s-2" }, "c0k2")}
  ${o("Chương trình", "CT", "r", "", "CTR ✗", { tren: 540, moc: "c0k1" }, "c0k2")}
  ${o("Quy định", "Q", "y", "Đ", "QYĐ ✗", { tren: 750, moc: "c1k0" }, "c1k1")}
  ${the(`<div class="c07-kk"><span class="a">QĐ<em>Quyết định</em></span><span class="ne">≠</span><span class="a">QyĐ<em>Quy định</em></span></div>`, { tren: 960, hien: "c2k0" })}`,
    },

    // dòng số: SỐ 5/TTr - UBND → Số: 05/TTr-UBND, rồi thanh năm 01/01 → 31/12
    "dong-so": {
      khai: true,
      html: () => `
  ${chipKhai("📏 Dòng «Số, ký hiệu»")}
  ${the(`<div class="c07-so">${fx("SỐ", "Số", "c0k1")}${fx("", ":", "c0k2")} ${fx("5", "05", "c0k3")}/TTr${fx(" - ", "-", "c1k1")}UBND</div>`, { tren: 380, hien: "s-2" })}
  ${goiKhai("<span>Từ «Số»: <b>chữ thường</b> · sau có <b>dấu hai chấm</b></span>", { trai: 60, tren: 590, hien: "c0k1+0.3", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Số nhỏ hơn 10 → <b>thêm 0</b> phía trước</span>", { trai: 60, tren: 710, hien: "c0k3+0.2", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${goiKhai("<span>Nhóm chữ viết tắt: <b>gạch nối, không cách</b></span>", { trai: 60, tren: 830, hien: "c1k1", tu: "duoi", rong: 960, lop: "cc-nhan-lon" })}
  ${the(`<div class="c07-nam"><div class="hai"><div>01/01<small>số đầu năm: 01</small></div><div>31/12<small>hết năm</small></div></div><div class="thanh"></div><div class="hai"><div style="font-size:30px;color:#64748b;font-weight:800">Số và ký hiệu là duy nhất trong năm</div></div></div>`, { tren: 980, hien: "c2k1" })}`,
    },
  },

  mocThu: (tatCa) => [
    ["01a-0.4s", 0.4],
    ["01b-dau", lucNode(tatCa, "moc", "c0k2w2")],
    ["02a-tt-ct", lucNode(tatCa, "bang", "c0k2w3")],
    ["02b-quy-dinh", lucNode(tatCa, "bang", "c1k1w3")],
    ["02c-so-sanh", lucNode(tatCa, "bang", "c2k2")],
    ["03a-so", lucNode(tatCa, "dong-so", "c0k1w2")],
    ["03b-05", lucNode(tatCa, "dong-so", "c0k3w3")],
    ["03c-noi", lucNode(tatCa, "dong-so", "c1k1w2")],
    ["03d-nam", lucNode(tatCa, "dong-so", "c2k2w3")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2w2"),
};
