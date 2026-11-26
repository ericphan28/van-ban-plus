// Clip 07 · Việc thật: chiều giao, sáng mai nộp kế hoạch tổ chức hội nghị → chọn mẫu có sẵn → AI soạn theo mẫu → tệp Word 4 trang.
// SOÁT (prod 26/11/2026, tài khoản demo-web-clip-2, quay bằng video/cong-cu/quay-web/quay-07.mjs → video/anh/viec-that/web-07*):
//  ✔ «Mẫu văn bản» (/mau): ô «Tìm mẫu (gõ không dấu cũng được)»; gõ «ke hoach» → 80 mẫu lọc còn 14 (web-07a.mp4, ket-qua-07a.json).
//  ✔ Mẫu có sẵn «Kế hoạch tổ chức sự kiện» (chung:san-ke-hoach-to-chuc, nhóm Hành chính): «Thông tin cần điền khi soạn» 7 ô;
//    «Nội dung mẫu» khung I. MỤC ĐÍCH · II. THỜI GIAN, ĐỊA ĐIỂM · III. NỘI DUNG CHƯƠNG TRÌNH · IV. PHÂN CÔNG NHIỆM VỤ · V. KINH PHÍ
//    (5 phần — GET /api/v2/mau/chung/san-ke-hoach-to-chuc).
//  ✔ Nút «Soạn bằng AI theo mẫu» → /soan-thao?mau=chung:san-ke-hoach-to-chuc → «Soạn theo mẫu: Kế hoạch tổ chức sự kiện»
//    (SoanThao.tsx docThamSoMau → docMau → ô dựng từ truong[] của mẫu).
//    Máy chủ nhận ten_mau + các ô; prompt KeHoach (vanbanplus-api prompt.ts keHoach) dựng I. Mục đích, yêu cầu · II. Nội dung
//    · III. Tổ chức thực hiện · IV. Kinh phí. Nội dung khung mẫu KHÔNG gửi lên AI → lời nói «theo mẫu», không nói «AI chép khung mẫu».
//  ✔ Chỉ gõ 4 ô: Tên sự kiện «Hội nghị tổng kết năm 2026» · Thời gian, địa điểm «8 giờ ngày 26/12/2026, Hội trường xã» ·
//    Kinh phí «25 triệu đồng» · Chức danh người ký «Chủ tịch» (ô Mục đích, Chương trình, Người ký để trống).
//  ✔ «Soạn bằng AI · 5–19 credit» → «AI đang xử lý» 22,2 s (tua nhanh) → «Đã trừ 11 credit · còn 8» — AI chạy thật.
//  ✔ Bản nháp (web-07.ket-qua.json): I. MỤC ĐÍCH, YÊU CẦU · II. NỘI DUNG, THỜI GIAN VÀ ĐỊA ĐIỂM (chương trình từng mốc giờ
//    07h30 → 10h45) · III. TỔ CHỨC THỰC HIỆN (6 bộ phận) · IV. KINH PHÍ (25.000.000 đồng, chia 4 khoản = 25 triệu).
//    Căn cứ «Luật Tổ chức chính quyền địa phương ngày 16 tháng 6 năm 2025», mời «UBND tỉnh B, các Sở» — đúng chính quyền hai cấp.
//  ✔ «Xem bố cục» → «Xuất Word» tải .docx thật (web-07-ke-hoach.docx), 4 trang; ảnh trang = tệp đó dựng bằng LibreOffice (word-07-trang*.png).
//  ✔ Ghi chú nguyên văn dưới nút: «Khi xuất Word, phông, cỡ chữ, lề và khối chữ ký tự áp theo Nghị định 30/2020.» — lời KHÔNG nhắc.
//  · AI tự đề xuất số đại biểu (120), mốc giờ, hạn 22–24/12, chia kinh phí — không có trong ô đã gõ → lời nói «chỗ AI đề xuất,
//    anh chị soát lại». AI gõ sai «Hường trực» (II.2) và địa danh «A, ngày … tháng 12» thiếu chữ «Xã» → không phóng hai chỗ đó.
//  · Tài khoản 19 credit: đủ 7 ô thì ước tính 5–20 (bị chặn «Số dư 19 credit chưa đủ») → quay bằng 4 ô ngắn (5–19).
//  · web-07.mp4 lần đầu bấm mẫu khi danh sách CHƯA lọc → phần tìm mẫu lấy từ web-07a.mp4 (quay lại, không bấm AI).
import { canhKet, canhTomTat, lucNode, chipKhai, dtKhai, goiKhai, cuaSoKhai, cuaSoVideoKhai, videoKhai, mocKhai, lopKhai, hoiKhai, sangKhai } from "../kit.mjs";
import { NGU_CANH, CSS, WEB } from "./loat.mjs";

const A = (ten) => `viec-that/${ten}`;
const VA = A("web-07a.mp4"), VB = A("web-07.mp4");
const DT = { rong: 500, trai: 470, tren: 200, xoay: 0 };   // điện thoại lệch phải (chừa nhãn cảnh), cao ≈ 1104 → đáy 1304
const TRANG = [1241, 1754];

/** điện thoại chạy bản quay `v` theo `khoa` (+ phu thêm) */
const dtVideo = (id, v, dau, khoa, u, phu = "") => dtKhai(id, { ...DT, lop: [{ anh: A(dau) }],
  phu: videoKhai(u(v), { id: `${id}-v`, khoa }) + phu }, u);
const zoom = (id, v, u, vung, tren, hien, an, khoa, phu = "") => cuaSoVideoKhai(id, u(v), WEB, vung, { trai: 65, tren, rong: 950 }, hien, an, { khoa }, phu);

const K_MAU = [["s", 1.2], ["c0k0w2", 1.9], ["c0k1", 2.3], ["c0k2", 5.2], ["c0k2w4", 6.85], ["c0e", 9.8], ["c1", 16.1], ["c1e", 16.6]];
const K_DIEN = [["s", 21.3], ["c0k0w2", 22.03], ["c0k1", 24.9], ["c0k2", 27.3], ["c0k4", 29.95], ["c0e", 33.4], ["e", 35.0]];
const K_AI = [["s", 35.6], ["c0k0w2", 37.26], ["c0k1", 38.6], ["c0e", 59.5], ["c1", 69.3], ["c1k1", 70.5], ["c1e", 70.6]];   // giữ khung chương trình theo giờ → III. Tổ chức thực hiện (né dòng «Hường trực»)
const K_WORD = [["s", 70.5], ["c0k0w1", 71.08], ["c0k0e+0.3", 74.6], ["c0k1w2", 79.6], ["e", 82.4]];

/** tờ giấy trống «KẾ HOẠCH» của cảnh móc (vẽ bằng HTML) */
const GIAY_TRONG = `<div class="vt-giay k7-giay">
  <div class="k7-dau"><span>ỦY BAN NHÂN DÂN<br>XÃ A</span><span>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM<br>Độc lập - Tự do - Hạnh phúc</span></div>
  <div class="k7-ten">KẾ HOẠCH</div><div class="k7-trich">Tổ chức Hội nghị tổng kết năm 2026</div>
  ${["I.", "II.", "III.", "IV."].map((m) => `<div class="k7-muc"><b>${m}</b><i></i></div><i class="k7-dong"></i><i class="k7-dong k7-ngan"></i>`).join("")}
</div>`;

export default {
  ten: "07-ke-hoach-theo-mau",
  tieuDe: "Việc thật · 7 · Kế hoạch dài — AI dựng khung theo mẫu",
  bia: { dong: ["Chiều giao việc", "mai nộp kế hoạch"], cat: [0.08, 0.55] },
  kichBan: [
    { canh: "moc", cau: [
      { loi: "Chiều nay giao, sáng mai phải nộp kế hoạch tổ chức hội nghị.", chu: "Chiều giao — *sáng mai nộp kế hoạch*" },
      { loi: "Mấy trang giấy, bắt đầu từ đâu?", chu: "Mấy trang giấy — *bắt đầu từ đâu?*" },
    ]},
    { canh: "mau", cau: [
      { loi: "Mở kho mẫu, gõ không dấu, chọn kế hoạch tổ chức sự kiện.", chu: "Mẫu văn bản → «ke hoach» → *Kế hoạch tổ chức sự kiện*" },
      { loi: "Mẫu có sẵn khung năm phần.", chu: "Mẫu có sẵn *khung 5 phần*" },
    ]},
    { canh: "dien", cau: [
      { loi: "Soạn theo mẫu, gõ tên hội nghị, giờ, nơi họp, kinh phí.", chu: "Soạn theo mẫu → *gõ vài dòng*" },
    ]},
    { canh: "ai", cau: [
      { loi: "Chạm Soạn bằng AI, chờ chừng hai mươi giây.", chu: "Chạm *Soạn bằng AI*" },
      { loi: "Bản nháp có chương trình theo giờ, phân công từng bộ phận.", chu: "Chương trình theo giờ · *phân công từng bộ phận*" },
    ]},
    { canh: "word", cau: [
      { loi: "Xem bố cục, rồi xuất ra tệp văn bản.", chu: "Xem bố cục → *Xuất Word*" },
    ]},
    { canh: "cam-ve", cau: [
      { loi: "Tệp bốn trang, đủ mục đích, chương trình, phân công, kinh phí.", chu: "Tệp Word 4 trang — *đủ 4 phần*" },
      { loi: "Chỗ AI đề xuất, anh chị soát lại, rồi trình ký.", chu: "Chỗ AI đề xuất — *soát lại rồi trình ký*" },
    ]},
    canhTomTat("Kế hoạch theo mẫu", [
      ["Chọn mẫu có sẵn", "Chọn mẫu có sẵn"],
      ["Gõ vài dòng thông tin", "Gõ vài dòng"],
      ["AI dựng khung rồi xuất tệp", "AI dựng khung · tải Word"],
    ]),
    canhKet(),
  ],
  css: CSS + `
.k7-giay{position:absolute;inset:0;box-sizing:border-box;padding:34px 40px;font-family:"Times New Roman",serif;color:#111}
.k7-dau{display:flex;justify-content:space-between;font-size:15px;font-weight:700;text-align:center;line-height:1.35}
.k7-ten{text-align:center;font-weight:700;font-size:30px;margin-top:40px}
.k7-trich{text-align:center;font-weight:700;font-size:20px;margin:6px 0 26px}
.k7-muc{display:flex;align-items:center;gap:14px;font-size:24px;margin-top:22px}
.k7-muc i{flex:1;height:16px;border-radius:8px;background:#e2e8f0}
.k7-dong{display:block;height:13px;border-radius:7px;background:#eef2f7;margin:12px 0 0 46px}
.k7-ngan{width:55%}
`,
  canh: {
    "moc": { khai: true, html: () => `
      ${mocKhai(`${NGU_CANH()}Chiều giao việc,<br>*sáng mai nộp kế hoạch*`, { tren: 250, co: 70, gach: "c0k1w2" })}
      ${lopKhai(GIAY_TRONG, "s-2", null, "left:150px;top:560px;width:780px;height:640px")}
      ${hoiKhai(800, 1000, "c1k1")}
      ${goiKhai("🕒 Chiều nay · nhận việc", { trai: 60, tren: 1215, hien: "c0k0", tu: "trai" })}
      ${goiKhai("⏰ Sáng mai · nộp", { trai: 600, tren: 1215, hien: "c0k1", tu: "phai" })}` },

    "mau": { khai: true, html: ({ u }) => `
      ${chipKhai("① Chọn mẫu")}
      ${dtVideo("dt-mau", VA, "web-07a-dau.png", K_MAU, u)}
      ${goiKhai("🔎 gõ không dấu", { trai: 40, tren: 300, hien: "c0k1", an: "c0k2+0.6", tu: "trai" })}
      ${zoom("cs-ds", VA, u, [0.02, 0.34, 0.98, 0.56], 720, "c0k1+0.7", "c0k2w4+0.3", K_MAU, sangKhai([1, 36, 99, 62], "c0k2", "c0k2w4+0.3"))}
      ${cuaSoVideoKhai("cs-khung", u(VA), WEB, [0.03, 0.07, 0.97, 0.70], { trai: 190, tren: 265, rong: 700 }, "c1k0-0.2", "e", { khoa: K_MAU })}` },

    "dien": { khai: true, html: ({ u }) => `
      ${chipKhai("② Gõ vài dòng")}
      ${dtVideo("dt-dien", VB, "web-07-dau.png", K_DIEN, u)}
      ${goiKhai("⏩ tua nhanh", { trai: 40, tren: 300, hien: "c0k1", an: "c0e", tu: "trai", lop: "vt-tua" })}
      ${zoom("cs-o", VB, u, [0.03, 0.10, 0.97, 0.58], 300, "c0k4", "e", K_DIEN)}` },

    "ai": { khai: true, html: ({ u }) => `
      ${chipKhai("③ AI soạn")}
      ${dtVideo("dt-ai", VB, "web-07-dau.png", K_AI, u)}
      ${goiKhai("⏩ tua nhanh", { trai: 40, tren: 300, hien: "c0k1", an: "c0e+0.3", tu: "trai", lop: "vt-tua" })}
      ${zoom("cs-ban-nhap", VB, u, [0.03, 0.30, 0.97, 0.70], 260, "c1k0", "e", K_AI)}` },

    "word": { khai: true, html: ({ u }) => `
      ${chipKhai("④ Xuất Word")}
      ${dtVideo("dt-word", VB, "web-07-dau.png", K_WORD, u)}
      ${zoom("cs-bo-cuc", VB, u, [0.03, 0.09, 0.97, 0.55], 260, "c0k0+0.5", "c0k1+0.2", K_WORD)}` },

    "cam-ve": { khai: true, html: ({ u }) => `
      ${chipKhai("📄 Cầm về: tệp Word 4 trang")}
      ${[4, 3, 2, 1].map((n, i) => lopKhai(`<img class="vt-giay" src="${u(A(`word-07-trang${n}.png`))}" style="left:${470 - i * 120}px;top:${330 - i * 14}px;width:520px;transform:rotate(${6 - i * 3}deg)">`, `s+${i * 0.15}`, null, "left:0;top:0;width:100%;height:100%")).join("")}
      ${cuaSoKhai("cs-kinh-phi", u(A("word-07-trang4.png")), TRANG, [0.06, 0.07, 0.94, 0.42], { trai: 65, tren: 380, rong: 950 }, "c0k4-0.2", "c1k1")}
      ${goiKhai("✓ 4 trang · sửa được ngay", { trai: 560, tren: 1150, hien: "c0k1", an: "c1", tu: "phai" })}
      ${goiKhai("⚠ Giờ · đại biểu · chia tiền: AI đề xuất", { trai: 60, tren: 1150, hien: "c1k0", tu: "trai" })}` },
  },
  mocThu: (tatCa) => [
    ["02-ds", lucNode(tatCa, "mau", "c0k2+0.2")], ["02-khung", lucNode(tatCa, "mau", "c1k0+0.8")],
    ["03-o", lucNode(tatCa, "dien", "c0k4+0.8")], ["04-tua", lucNode(tatCa, "ai", "c0k1+0.4")],
    ["04-ban-nhap", lucNode(tatCa, "ai", "c1k1")], ["05-bo-cuc", lucNode(tatCa, "word", "c0k1")],
    ["06-kinh-phi", lucNode(tatCa, "cam-ve", "c0k4+0.6")], ["06-de-xuat", lucNode(tatCa, "cam-ve", "c1k1")],
  ],
  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1e-0.3"),
};
