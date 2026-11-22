// Clip 02 · Việc thật: văn bản đến bản giấy hai trang → quét → AI đọc chữ, lưu văn bản đến → AI tóm tắt để báo lãnh đạo.
// Quay THẬT, AI chạy thật (22/11/2026):
//   web (prod, tài khoản demo-web-clip-2): video/cong-cu/quay-web/quay.mjs 02 → video/anh/viec-that/web-02.* (khung 390×844 ×3)
//   desktop (app 1.0.21 dựng từ mã, tài khoản «Nguyễn Văn A (free)»): video/cong-cu/quay-app/quay-viec-that-02a/b.ps1 → desk-02.*
// SOÁT:
//  ✔ Giấy giả lập HƯ CẤU «2150/SNV-VP» Sở Nội vụ tỉnh B, 2 trang (giay.mjs); 5 mục, 4 hạn 25/12/2026 · 31/12/2026 · 05/01/2027 · 15/01/2027.
//  ✔ Web › Quét «Nhập từ ảnh hoặc PDF»: nút «Chụp ảnh» (màn cảm ứng) · «Tính credit» · «Đọc văn bản · 4–15 credit» → «Đã trừ 4 credit»
//    → «Soát và lưu» điền sẵn Số 2150, Cơ quan ban hành, Trích yếu, Hướng «Đến», «Kèm 2 tệp»; «Soát lại các ô trước khi lưu — AI có thể đọc sai.»
//  ✔ «Lưu văn bản» → trang văn bản: Đến · Công văn · 2150/SNV-VP · 20/11/2026 · SỞ NỘI VỤ · Lê Thị D.
//  ✔ «Trợ lý AI» › thẻ «Tóm tắt» › «Tóm tắt · 5–17 credit» → «Đã trừ 7 credit» (~14 s, tua nhanh) → Ý chính 5 mục, Căn cứ pháp lý,
//    Mốc hiệu lực (đủ 4 hạn), Số liệu chính, Tác động, Ghi chú (web-02.ket-qua.json). «Ghi vào trường Tóm tắt» → «Đã ghi vào trường Tóm tắt.»
//  ✔ Desktop › Quản lý văn bản › «Nhập từ scan» › «Thêm file PDF / Ảnh scan» › «Phân tích bằng AI» › hộp «Xác nhận dùng credit»
//    → điền Số, Trích yếu… → «Lưu văn bản vào hệ thống» («Đã nhập VB: 2150/SNV-VP…») › Xem nhanh › «AI Tóm tắt» › «Tóm tắt»
//    → «CÁC MỐC THỜI GIAN» 5 dòng (ban hành + 4 hạn), «CĂN CỨ PHÁP LÝ».
//  ✔ Không nói «đồng bộ» web ↔ máy tính (chưa có — NOI-DUNG mục 9): hai đoạn là HAI lần làm riêng, lời chỉ nói «bản cài cũng làm được».
//  · Che: biểu tượng app trên thanh tiêu đề hộp thoại desktop (ô #F3F3F3); cắt bỏ đoạn hộp chọn tệp; ô ngày kiểu Mỹ «11/20/2026»
//    trên biểu mẫu Soát web bị cắt qua (nhảy thẳng tới nút Lưu).
import { canhKet, canhTomTat, lucNode, chipKhai, dtKhai, mtKhai, goiKhai, cuaSoKhai, cuaSoVideoKhai, videoKhai, mocKhai, lopKhai, cheKhai } from "../kit.mjs";
import { NGU_CANH, CSS, WEB } from "./loat.mjs";

const A = (ten) => `viec-that/${ten}`;
const VW = A("web-02.mp4"), VD = A("desk-02-cat.mp4");
const DESK = [1424, 860];
const DT = { rong: 500, trai: 470, tren: 200, xoay: 0 };
const CHE_ICON = cheKhai([12.3, 2.8, 13.9, 5.2], "#F3F3F3");   // biểu tượng app trên thanh tiêu đề hộp thoại

const dtVideo = (id, khoa, u) => dtKhai(id, { ...DT, lop: [{ anh: A("web-02-dau.png") }],
  phu: videoKhai(u(VW), { id: `${id}-v`, khoa }) }, u);
const zoomW = (id, u, vung, tren, hien, an, khoa) => cuaSoVideoKhai(id, u(VW), WEB, vung, { trai: 65, tren, rong: 950 }, hien, an, { khoa });
const zoomD = (id, u, vung, tren, hien, an, khoa) => cuaSoVideoKhai(id, u(VD), DESK, vung, { trai: 65, tren, rong: 950 }, hien, an, { khoa });

// mốc bản quay web-02: chụp 1,93 · tính credit 3,85 · đọc 9,85 → xong 22,27 · lưu 31,53 · trang văn bản 34,5
const K_QUET = [["s", 1.3], ["c0k1", 1.9], ["c0e", 3.4], ["c1", 3.6], ["c1k0w2", 9.85], ["c1k1", 10.2], ["c1e", 22.6],
  ["c2", 30.6], ["c2k0w3", 31.53], ["c2k1", 34.6], ["e", 37.0]];
// trợ lý 38,2 · thẻ 39,47 · chạm Tóm tắt 40,79 → xong 55,24 · đầu kết quả 57,2 → cuộn tới 65,8 · ghi 67,34 → đã ghi 68,2
const K_TOM = [["s", 37.6], ["c0k0w3", 39.47], ["c0k1w0+0.1", 40.79], ["c0k1w2", 41.5], ["c0e", 55.3], ["c1", 57.0], ["c1k1", 63.8], ["c1e", 65.0],
  ["c2", 66.4], ["c2k0w1", 67.34], ["e", 70.5]];
// desktop: chạm Nhập từ scan 4,33 · thêm tệp 7,27 (CẮT hộp chọn tệp → 11,6) · phân tích 16,73 · xác nhận 20,4–22,4 · đọc xong 30,9
//          · lưu 47,68 (CẮT) · AI Tóm tắt 81,1 · chạm Tóm tắt 85,1 · xác nhận 85,6–87,7 (CẮT chờ) → kết quả 99,6 · cuộn 292,3–304
// desk-02-cat.mp4 = 4 đoạn của desk-02.mp4 (556 s, quá nặng cho trang dựng): 3,6–32,5 → 0 · 80,6–87,8 → 28,9 · 99,6–100,6 → 36,1 · 292,3–304,5 → 37,1
const K_DESK = [["s", 0], ["c0k0w4", 0.73], ["c0k1-0.15", 3.8], ["c0k1-0.15", 8.0], ["c0k2-0.1", 13.13], ["c0k2+0.25", 18.9], ["c0k3-0.05", 27.3],
  ["c0k3", 28.85], ["c0k3+0.6", 29.6], ["c0e", 33.4], ["c0e", 36.1], ["c1", 36.2], ["c1", 37.1], ["c1k1", 48.8]];

export default {
  ten: "02-quet-tom-tat-bao-lanh-dao",
  tieuDe: "Việc thật · 2 · Văn bản giấy → tóm tắt báo lãnh đạo",
  bia: { dong: ["Văn bản giấy", "tóm tắt cho sếp?"], cat: [0.1, 0.62] },
  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản giấy hai trang vừa đến, lãnh đạo cần nắm ngay ý chính.", chu: "Văn bản giấy 2 trang — *lãnh đạo cần ý chính ngay*" },
      { loi: "Đọc hết rồi tóm tắt tay, mất bao lâu?", chu: "Đọc hết rồi tóm tắt tay — *mất bao lâu?*" },
    ]},
    { canh: "quet", cau: [
      { loi: "Trên điện thoại, chạm Chụp ảnh, chụp hai trang giấy.", chu: "Chạm *Chụp ảnh* — chụp 2 trang giấy" },
      { loi: "Chạm Đọc văn bản, AI đọc chữ, tự điền số, cơ quan, trích yếu.", chu: "AI đọc chữ → *tự điền số · cơ quan · trích yếu*" },
      { loi: "Soát lại rồi lưu, thành văn bản đến.", chu: "Soát lại → lưu thành *văn bản đến*" },
    ]},
    { canh: "ai-tom", cau: [
      { loi: "Mở Trợ lý AI, chạm Tóm tắt.", chu: "Trợ lý AI → *Tóm tắt*" },
      { loi: "Ý chính năm mục, đủ các hạn nộp, gọn trên một màn.", chu: "5 ý chính · *đủ các hạn nộp*" },
      { loi: "Chạm Ghi vào trường Tóm tắt, để lưu cùng văn bản.", chu: "*Ghi vào trường Tóm tắt* — lưu cùng văn bản" },
    ]},
    { canh: "may-tinh", cau: [
      { loi: "Ở cơ quan dùng máy tính, bản cài, cũng quét, và tóm tắt được như vậy.", chu: "Bản cài máy tính — *cũng quét · cũng tóm tắt*" },
      { loi: "Hạn nào, trước ngày nào, hiện rõ từng dòng.", chu: "Mỗi hạn nộp — *một dòng rõ ràng*" },
    ]},
    canhTomTat("Văn bản đến bản giấy", [
      ["Chụp hai trang giấy", "Chụp 2 trang giấy"],
      ["AI đọc và điền sẵn", "AI đọc · điền sẵn"],
      ["Tóm tắt kèm các hạn nộp", "Tóm tắt · đủ hạn nộp"],
    ]),
    canhKet(),
  ],
  css: CSS,
  canh: {
    "moc": { khai: true, html: ({ u }) => `
      ${mocKhai(`${NGU_CANH("📄 Việc thật · văn bản đến bản giấy")}Văn bản giấy 2 trang,<br>*lãnh đạo cần ý chính ngay*`, { tren: 250, co: 66, gach: "c0k1w3" })}
      ${lopKhai(`<img class="vt-giay" src="${u(A("giay-02-trang2.png"))}" style="left:470px;top:600px;width:470px;transform:rotate(5deg)">`, "s-2", null, "left:0;top:0;width:100%;height:100%")}
      ${lopKhai(`<img class="vt-giay" src="${u(A("giay-02-trang1.png"))}" style="left:150px;top:560px;width:500px;transform:rotate(-3deg)">`, "s-2", null, "left:0;top:0;width:100%;height:100%")}
      ${goiKhai("⏳ đọc hết 2 trang…", { trai: 560, tren: 1170, hien: "c1k0", tu: "phai" })}` },

    "quet": { khai: true, html: ({ u }) => `
      ${chipKhai("① Chụp · AI đọc · lưu")}
      ${dtVideo("dt-quet", K_QUET, u)}
      ${goiKhai("⏩ tua nhanh · AI đang đọc", { trai: 40, tren: 300, hien: "c1k1", an: "c1e", tu: "trai", lop: "vt-tua" })}
      ${zoomW("cs-soat", u, [0.03, 0.20, 0.97, 0.53], 540, "c1e-0.4", "c2", K_QUET)}
      ${zoomW("cs-den", u, [0.03, 0.07, 0.97, 0.46], 450, "c2k1+0.3", "e", K_QUET)}` },

    "ai-tom": { khai: true, html: ({ u }) => `
      ${chipKhai("② AI tóm tắt")}
      ${dtVideo("dt-tom", K_TOM, u)}
      ${goiKhai("⏩ tua nhanh · AI đang tóm tắt", { trai: 40, tren: 300, hien: "c0k1w2", an: "c0e", tu: "trai", lop: "vt-tua" })}
      ${zoomW("cs-y-chinh", u, [0.03, 0.04, 0.97, 0.50], 250, "c1", "c1k1", K_TOM)}
      ${zoomW("cs-han", u, [0.03, 0.06, 0.97, 0.42], 300, "c1k1", "c2", K_TOM)}` },

    "may-tinh": { khai: true, html: ({ u }) => `
      ${chipKhai("🖥 Bản cài máy tính")}
      ${mtKhai("mt", { rong: 960, trai: 60, tren: 330, lop: [{ anh: A("desk-02-dau.png") }],
        phu: videoKhai(u(VD), { id: "mt-v", khoa: K_DESK })
          + lopKhai(CHE_ICON, "c0k0w4+0.3", "c0k3", "left:0;top:0;width:100%;height:100%")
          + lopKhai(CHE_ICON, "c0k3+0.5", null, "left:0;top:0;width:100%;height:100%") }, u)}
      ${goiKhai("⏩ tua nhanh", { trai: 40, tren: 270, hien: "c0k1", an: "c1", tu: "trai", lop: "vt-tua" })}
      ${zoomD("cs-mt-han", u, [0.125, 0.62, 0.42, 0.82], 895, "c1k1", "e", K_DESK)}` },
  },
  mocThu: (tatCa) => [
    ["02-soat", lucNode(tatCa, "quet", "c1e")], ["02-den", lucNode(tatCa, "quet", "c2k1+1")],
    ["03-y-chinh", lucNode(tatCa, "ai-tom", "c1+0.8")], ["03-han", lucNode(tatCa, "ai-tom", "c1k1+1")],
    ["04-mt-doc", lucNode(tatCa, "may-tinh", "c0k2+0.6")], ["04-mt-tom", lucNode(tatCa, "may-tinh", "c0k3+0.8")],
  ],
  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1e+0.3"),
};
