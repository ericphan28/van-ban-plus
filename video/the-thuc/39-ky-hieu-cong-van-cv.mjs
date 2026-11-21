// ============================================================================
// THỂ THỨC 39 — «SỐ: 15/CV-UBND» SAI Ở ĐÂU? KÝ HIỆU CÔNG VĂN VÀ DÒNG «V/v» THEO NĐ 30/2020
//
// Khuôn TRANH LUẬN (như clip 35): móc A/B + nhãn bình luận → quy định → dòng V/v → cảnh bản web (BẮT BUỘC) → thẻ kết + mời bình luận.
//
// SOÁT với bản gốc van-ban-goc/30.2020.NĐ-CP/Phuluc1.md + Phuluc3.md:
//   Phụ lục I II.3b: «Ký hiệu của văn bản bao gồm chữ viết tắt tên loại văn bản và chữ viết tắt tên cơ quan, tổ chức hoặc chức danh
//   nhà nước có thẩm quyền ban hành văn bản. Đối với công văn, ký hiệu bao gồm chữ viết tắt tên cơ quan, tổ chức hoặc chức danh nhà
//   nước ban hành công văn và chữ viết tắt tên đơn vị soạn thảo hoặc lĩnh vực được giải quyết.»
//   Phụ lục I mục V (mẫu chữ): «Số: 15/QĐ-BNV; Số: 05/BNV-VP» · Phụ lục III Mẫu 1.5 Công văn: «Số: …/…3…-…4… V/v»
//   Phụ lục III bảng chữ viết tắt tên loại: QĐ, TTr, KH, TB, BC, BB, HĐ, CĐ, GUQ, GM, GGT, GNP … — KHÔNG có «Công văn».
//   Phụ lục I II.5b: «Đối với công văn, trích yếu nội dung văn bản được trình bày tại ô số 5b …, sau chữ "V/v" bằng chữ in thường,
//   cỡ chữ từ 12 đến 13, kiểu chữ đứng; đặt canh giữa dưới số và ký hiệu văn bản …»
//   ✔ «UBND-VP», «15» = ví dụ minh họa (ghi trên hình). Chữ Latin chỉ trên hình, lời đọc nói «chữ viết tắt công văn».
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, dau, veCC, theWeb, CAU_WEB, binhLuanCau, theBinhLuan, nhanBinhLuan } from "./loat.mjs";

const so = (chu, ghi, hien, tren, lop = "") =>
  the(`<div class="x-so ${lop}"><span class="tn">${chu}</span><span class="gh">${ghi}</span></div>`, { tren, hien, lop: "x-the" });

export default {
  ten: "39-ky-hieu-cong-van-cv",
  tieuDe: "Thể thức 39 · «Số: 15/CV-UBND» sai ở đâu? Ký hiệu công văn và dòng «V/v» theo NĐ 30/2020",
  bia: { dong: ["«15/CV-UBND»", "sai ở đâu?"], vai: "Nghị định 30 · Phụ lục I, III" },
  ket: {
    the: [
      "«Đối với công văn, ký hiệu bao gồm chữ viết tắt *tên cơ quan, tổ chức* … và chữ viết tắt *tên đơn vị soạn thảo* hoặc lĩnh vực được giải quyết.»",
      theNguon("Phụ lục I · mục II.3b"),
      theBinhLuan("Cơ quan anh chị còn ghi «CV» không? *Bình luận bên dưới*"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Ký hiệu công văn, có chữ viết tắt công văn, đúng hay sai?", chu: "«Số: 15/*CV*-UBND» — *đúng hay sai?*" },
      { loi: "Anh chị chọn bên nào, bình luận trước, rồi xem tiếp.", chu: "Chọn *A hay B?* → *bình luận trước* rồi xem tiếp", nghiSau: 0.2 },
    ]},
    { canh: "qd", cau: [
      { loi: "Văn bản có tên loại, ký hiệu ghi chữ viết tắt tên loại, như quyết định, tờ trình.", chu: "Có tên loại → *QĐ*, *TTr*… trong ký hiệu" },
      { loi: "Riêng công văn, ký hiệu gồm tên cơ quan, và tên đơn vị soạn thảo.", chu: "Công văn: *tên cơ quan* + *đơn vị soạn thảo*" },
      { loi: "Bảng chữ viết tắt tên loại ở Phụ lục ba, không hề có công văn.", chu: "Bảng viết tắt (Phụ lục III): *không có «CV»*", nghiSau: 0.3 },
    ]},
    { canh: "vv", cau: [
      { loi: "Trích yếu công văn, đặt sau chữ về việc viết tắt, ngay dưới số và ký hiệu.", chu: "Trích yếu: sau *«V/v»* · *dưới số, ký hiệu*", nghiSau: 0.4 },
    ]},
    { canh: "app", cau: [CAU_WEB] },
    canhKet([
      { loi: "Đối với công văn, ký hiệu bao gồm chữ viết tắt tên cơ quan, và chữ viết tắt tên đơn vị soạn thảo.", chu: "Công văn: viết tắt *tên cơ quan* + *đơn vị soạn thảo*" },
      nguonCau("Phụ lục một", "Phụ lục I · mục II.3b"),
      binhLuanCau("Cơ quan anh chị còn ghi chữ viết tắt công văn không, bình luận cho mọi người cùng biết.", "Còn ghi «CV» không? *Bình luận bên dưới*"),
    ]),
  ],

  css: CSS_CC + `
.x-ab{position:absolute;left:70px;top:640px;width:940px;display:flex;flex-direction:column;gap:26px}
.x-ab .o{display:flex;align-items:center;gap:26px;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16);padding:26px 30px}
.x-ab .o b{flex:none;display:inline-flex;align-items:center;justify-content:center;width:92px;height:92px;border-radius:50%;color:#fff;font-size:56px;font-weight:900}
.x-ab .a b{background:#1d4ed8}.x-ab .b b{background:#d97706}
.x-ab .o p{margin:0;font-family:"Times New Roman",serif;font-size:62px;color:#0f172a}
.x-ab .o em{font-style:normal;color:#dc2626;font-weight:700}
.x-ab .o small{display:block;font-family:system-ui,sans-serif;font-size:26px;font-weight:800;color:#94a3b8}
.x-the{padding:0}
.x-so{display:flex;align-items:center;justify-content:space-between;gap:20px;padding:22px 34px}
.x-so .tn{font-family:"Times New Roman",serif;font-size:54px;color:#0f172a}
.x-so .tn em{font-style:normal;font-weight:700;color:#15803d;background:#dcfce7;border-radius:6px;padding:0 4px}
.x-so .gh{flex:none;font-size:30px;font-weight:800;color:#64748b;text-align:right}
.x-so.sai .tn em{color:#b91c1c;background:#fee2e2;text-decoration:line-through;text-decoration-thickness:5px}
.x-bang{padding:24px 30px}
.x-bang h4{margin:0 0 14px;font-size:30px;font-weight:900;color:#64748b;letter-spacing:.04em}
.x-bang .ds{display:flex;flex-wrap:wrap;gap:12px}
.x-bang .ds span{font-size:36px;font-weight:900;color:#334155;background:#f1f5f9;border-radius:12px;padding:6px 16px}
.x-bang .ds .kh{color:#b91c1c;background:#fee2e2;border:4px dashed #dc2626;text-decoration:line-through}
.x-nd{padding:28px 32px;text-align:center;font-size:44px;font-weight:900;color:#0f172a}
.x-nd b{color:var(--mau-chinh)}
.x-vb{padding:34px 40px;text-align:center;font-family:"Times New Roman",serif;color:#0f172a}
.x-vb .cq{font-size:40px;font-weight:700}
.x-vb .sk{margin-top:14px;font-size:44px}
.x-vb .vv{margin-top:10px;font-size:40px}
.x-vb .vv em{font-style:normal;background:#fef3c7;border-radius:6px;padding:0 6px}
.x-vb small{display:block;margin-top:18px;font-family:system-ui,sans-serif;font-size:26px;font-weight:800;color:#94a3b8}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Ký hiệu công văn —<br>*có «CV» không?*`, { tren: 262, co: 72, gach: "c0k1" })}
  <div class="x-ab">
    <div class="o a"><b>A</b><p>Số: 15/<em>CV</em>-UBND<small>ví dụ minh họa</small></p></div>
    <div class="o b"><b>B</b><p>Số: 15/<em>UBND-VP</em><small>ví dụ minh họa</small></p></div>
  </div>
  ${nhanBinhLuan("c1k0", 1060)}`,
      ve: veCC,
    },

    "qd": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục I · mục II.3b")}
  ${so("15/<em>QĐ</em>-UBND", "quyết định", "c0k1", 320)}
  ${so("15/<em>TTr</em>-UBND", "tờ trình", "c0k2", 470)}
  ${so("15/UBND-<em>VP</em>", "công văn<br>đơn vị soạn: Văn phòng", "c1k0", 620)}
  ${the(`<div class="x-bang"><h4>PHỤ LỤC III · BẢNG VIẾT TẮT TÊN LOẠI</h4><div class="ds"><span>QĐ</span><span>TTr</span><span>KH</span><span>TB</span><span>BC</span><span>BB</span><span>HĐ</span><span>GM</span><span>GUQ</span><span>…</span><span class="kh" data-k="lop" data-hien="c2k1" style="opacity:0">CV</span></div></div>`, { tren: 800, hien: "c2k0" })}
  ${dau("B ĐÚNG", "c2e-0.1", { tren: 1100, co: 76, lop: "xanh" })}`,
    },

    "vv": {
      khai: true,
      html: () => `
  ${chipKhai("📜 Phụ lục I · mục II.5b")}
  ${the(`<div class="x-vb"><div class="cq">ỦY BAN NHÂN DÂN<br>XÃ A</div><div class="sk">Số: 15/UBND-VP</div><div class="vv" data-k="lop" data-hien="c0k1" style="opacity:0"><em>V/v</em> sửa chữa nhà văn hóa ấp 3</div><small>ví dụ minh họa</small></div>`, { tren: 360, hien: "c0k0" })}`,
    },

    "app": {
      khai: true,
      html: () => `
  ${chipKhai("🤖 Phần mềm soạn văn bản")}
  ${theWeb("c0k0")}`,
    },
  },

  mocThu: (tatCa) => [
    ["01a-0.4s", 0.4],
    ["01b-bl", lucNode(tatCa, "moc", "c1k1")],
    ["02a-so", lucNode(tatCa, "qd", "c1e")],
    ["02b-bang", lucNode(tatCa, "qd", "c2e+0.2")],
    ["03a-vv", lucNode(tatCa, "vv", "c0e")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
