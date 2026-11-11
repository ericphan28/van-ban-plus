// ============================================================================
// THỂ THỨC 23 — SOẠN, DUYỆT, KIỂM TRA BẢN THẢO: AI CHỊU TRÁCH NHIỆM VỀ NỘI DUNG, AI VỀ THỂ THỨC? THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 10 khoản 2: «Đơn vị hoặc cá nhân được giao chủ trì soạn thảo văn bản thực hiện các công việc: Xác định tên loại, nội dung và độ mật, mức độ khẩn của
//   văn bản cần soạn thảo; thu thập, xử lý thông tin có liên quan; soạn thảo văn bản đúng hình thức, thể thức và kỹ thuật trình bày.» Khoản 4: «Cá nhân được giao
//   nhiệm vụ soạn thảo văn bản chịu trách nhiệm trước người đứng đầu đơn vị và trước pháp luật về bản thảo văn bản trong phạm vi chức trách, nhiệm vụ được giao.»
//   Điều 11: «1. Bản thảo văn bản phải do người có thẩm quyền ký văn bản duyệt. 2. Trường hợp bản thảo văn bản đã được phê duyệt nhưng cần sửa chữa, bổ sung thì phải
//   trình người có thẩm quyền ký xem xét, quyết định.» Điều 12: «1. Người đứng đầu đơn vị soạn thảo văn bản phải kiểm tra và chịu trách nhiệm trước người đứng đầu cơ
//   quan, tổ chức và trước pháp luật về nội dung văn bản. 2. Người được giao trách nhiệm kiểm tra thể thức, kỹ thuật trình bày văn bản phải kiểm tra và chịu trách nhiệm … về
//   thể thức, kỹ thuật trình bày văn bản.»
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "23-soan-duyet-kiem-tra-ban-thao",
  tieuDe: "Thể thức 23 · Soạn, duyệt, kiểm tra bản thảo: ai chịu trách nhiệm về nội dung, ai về thể thức? Theo NĐ 30/2020",
  bia: { dong: ["Soạn, duyệt, kiểm tra:","ai chịu trách nhiệm?"], vai: "Nghị định 30 · Điều 10–12" },
  ket: {
    the: [
      "«Cá nhân được giao nhiệm vụ soạn thảo văn bản *chịu trách nhiệm trước người đứng đầu đơn vị và trước pháp luật* về bản thảo văn bản trong phạm vi chức trách, nhiệm vụ được giao.»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 10 · khoản 4"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Soạn xong bản thảo, ai duyệt, ai kiểm tra, và ai chịu trách nhiệm?", chu: "Soạn, duyệt, kiểm tra, *ai chịu trách nhiệm?*" },
      { loi: "Nghị định ba mươi phân rất rõ.", chu: "Nghị định 30 *phân rất rõ*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Người soạn, phải xác định tên loại, độ mật, mức độ khẩn, và nội dung.", chu: "Người soạn: xác định *tên loại* · *độ mật* · *mức độ khẩn* · *nội dung*" },
      { loi: "Bản thảo, phải do người có thẩm quyền ký văn bản duyệt.", chu: "Bản thảo: do *người có thẩm quyền ký* duyệt" },
      { loi: "Đã duyệt mà cần sửa, thì phải trình người có thẩm quyền ký, xem xét, quyết định.", chu: "Đã duyệt mà sửa → *trình lại người ký* quyết định" },
      { loi: "Về nội dung, người đứng đầu đơn vị soạn thảo, phải kiểm tra, và chịu trách nhiệm.", chu: "*Nội dung*: người đứng đầu *đơn vị soạn thảo* kiểm tra, chịu trách nhiệm" },
      { loi: "Về thể thức, người được giao kiểm tra, chịu trách nhiệm trước pháp luật.", chu: "*Thể thức*: người được giao kiểm tra, chịu trách nhiệm trước *pháp luật*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Cá nhân soạn thảo, chịu trách nhiệm trước người đứng đầu đơn vị, và trước pháp luật về bản thảo.", chu: "Người soạn: *chịu trách nhiệm* trước người đứng đầu đơn vị · trước *pháp luật*" },
      nguonCau("Điều mười", "Điều 10 · khoản 4"),
    ]),
  ],

  css: CSS_CC + `
.x-q{display:flex;flex-wrap:wrap;justify-content:center;align-items:center;gap:20px 24px;padding:44px 24px 50px}
.x-q span{font-size:46px;font-weight:900;color:#b91c1c;border:7px solid #b91c1c;border-radius:12px;padding:6px 24px;transform:rotate(-3deg);background:#fff;text-align:center;line-height:1.15}
.x-q i{font-style:normal;font-size:76px;font-weight:900;color:#b45309}
.x-the{padding:0}
.x-h{display:flex;align-items:center;gap:22px;padding:18px 26px}
.x-h .nh{flex:none;width:214px;text-align:center;font-size:32px;font-weight:900;color:#fff;border-radius:16px;padding:12px 6px;line-height:1.1}
.x-h .nh.g{background:#15803d}.x-h .nh.r{background:#dc2626}.x-h .nh.b{background:#1d4ed8}.x-h .nh.a{background:#d97706}.x-h .nh.v{background:#0f766e}
.x-h .tx{flex:1;font-size:42px;font-weight:900;color:#0f172a;line-height:1.15}
.x-h .tx small{display:block;margin-top:6px;font-size:30px;font-weight:800;color:#64748b}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Soạn — duyệt — kiểm tra:<br>*ai chịu trách nhiệm?*`, { tren: 262, co: 66, gach: "c0k2" })}
  ${the(`<div class="x-q"><span>SOẠN</span><span>DUYỆT</span><span>KIỂM TRA</span><i>?</i></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("📝 Từ soạn đến ký")}
  ${hang("SOẠN", "r", "Xác định tên loại · độ mật · độ khẩn", "người soạn phải xác định trước khi viết", "c0k1", 330)}
  ${hang("DUYỆT", "b", "Người có thẩm quyền ký thì duyệt", "bản thảo do người ký văn bản duyệt", "c1k1", 520)}
  ${hang("SỬA SAU DUYỆT", "a", "Phải trình lại người ký quyết định", "duyệt rồi mà sửa, bổ sung", "c2k1", 710)}
  ${hang("NỘI DUNG", "v", "Người đứng đầu đơn vị soạn thảo kiểm tra", "chịu trách nhiệm trước pháp luật", "c3k1", 900)}
  ${hang("THỂ THỨC", "g", "Người được giao kiểm tra thể thức", "và kỹ thuật trình bày", "c4k1", 1090)}`,
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
    ["02a-1", lucNode(tatCa, "ds", "c0k4w0")],
    ["02b-2", lucNode(tatCa, "ds", "c1k1w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k3w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k3w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k2w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
