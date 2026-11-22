// Clip 03 · Việc thật: công văn sắp trình ký có 4 lỗi thể thức cài sẵn → AI Kiểm tra trên bản cài máy tính → thẻ lỗi + bản đã sửa.
// Quay THẬT, AI chạy thật (22/11/2026): app 1.0.21 dựng từ mã, tài khoản «Nguyễn Văn A (free)» (73 credit trước khi quay),
//   video/cong-cu/quay-app/quay-viec-that-03.ps1 → video/anh/viec-that/desk-03.* ; nguyên văn kết quả: desk-03.ket-qua.txt
// desk-03-cat.mp4 = 2 đoạn của desk-03.mp4 (262,6 s): 8,0–45,0 → 0 · 228,0–262,6 → 37,0
//   mốc (giây bản cắt): chọn VB 1,4 · chạm AI Kiểm tra 5,0 · hộp nhảy chỗ 5,5–6,0 (CẮT) · chạm Kiểm tra 13,0 · hộp xác nhận 15,5–17,6
//   · AI chạy 18→35,7 (tua) · thẻ «CV» 37–41 · cuộn thẻ 44–58 (thẻ ngày 48 · thẻ Nơi nhận 53,5) · cột bản sửa ở đầu tới 62, cuộn 63–71,6 (Lưu: VT 71,5)
// SOÁT:
//  ✔ Văn bản demo HƯ CẤU «15/CV-UBND» UBND xã A gửi Sở VH-TT-DL tỉnh B (quay-app/02-cv-4-loi.txt). 4 lỗi cài: ký hiệu có «CV» ·
//    «Xã A ngày 5» (thiếu phẩy, thiếu số 0) · «Kính gửi» thiếu hai chấm · Nơi nhận thiếu «Lưu: VT».
//  ✔ NĐ 30/2020 Phụ lục I (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-I.md), nguyên văn:
//    · «Đối với công văn, ký hiệu bao gồm chữ viết tắt tên cơ quan, tổ chức hoặc chức danh nhà nước ban hành công văn và chữ viết tắt
//      tên đơn vị soạn thảo hoặc lĩnh vực được giải quyết.» (không có chữ viết tắt tên loại)
//    · «sau địa danh có dấu phẩy (,)» · «đối với những số thể hiện ngày nhỏ hơn 10 và tháng 1, 2 phải ghi thêm số 0 phía trước.»
//    · «sau từ “Kính gửi” có dấu hai chấm (:)»
//    · «dòng cuối cùng bao gồm chữ “Lưu” sau có dấu hai chấm (:), tiếp theo là chữ viết tắt “VT”, dấu phẩy (,), chữ viết tắt tên đơn vị…»
//  ✔ App: Xem nhanh › «AI Kiểm tra» › «Kiểm tra» › hộp «Xác nhận dùng credit» («dự kiến dùng 9–19 credit») → «6/10», «1 Nghiêm trọng ·
//    3 Cần xem xét · 1 Gợi ý»; THẺ RIÊNG cho 3 lỗi cài (CV · địa danh ngày tháng · Nơi nhận thiếu lưu), mỗi thẻ có «Văn bản gốc»,
//    «Đề xuất sửa», dòng căn cứ «Phụ lục I … Nghị định số 30/2020/NĐ-CP». Lỗi «Kính gửi» KHÔNG có thẻ → lời không nói «AI bắt 4 lỗi».
//  ✔ «VĂN BẢN SAU KHI SỬA»: «Số: 15/UBND-VP» · «Xã A, ngày 05 tháng 11 năm 2026» · «Kính gửi: Sở…» · «Lưu: VT, VP.» → đủ 4 chỗ.
//  ⚠ Bản sửa TỰ THÊM ý không có trong bản gốc («hệ thống điện và thiết bị…», «tuyên truyền nhiệm vụ chính trị», «Có Hồ sơ khảo sát…
//    đính kèm») → không phóng phần thân; lời nói rõ «câu chữ AI viết lại, soát kỹ rồi mới trình ký». Lỗi sản phẩm đã báo chủ dự án.
//  ✔ Không quay «Xuất Word» (bản 1.0.21 bọc bản sửa vào khuôn «BÁO CÁO» + cơ quan trong Cài đặt — lỗi sản phẩm đã báo).
//  · Che: biểu tượng app trên thanh tiêu đề hộp AI Kiểm tra (ô #F3F3F3).
import { canhKet, canhTomTat, lucNode, chipKhai, mtKhai, goiKhai, cuaSoVideoKhai, videoKhai, mocKhai, lopKhai, cheKhai, sangKhai, hoiKhai } from "../kit.mjs";
import { NGU_CANH, CSS } from "./loat.mjs";

const A = (ten) => `viec-that/${ten}`;
const VD = A("desk-03-cat.mp4");
const DESK = [1424, 860];
const CHE_ICON = cheKhai([16.8, 3.4, 18.6, 6.2], "#F3F3F3");   // biểu tượng app trên thanh tiêu đề hộp thoại
const zoomD = (id, u, vung, tren, hien, an, khoa, phu) => cuaSoVideoKhai(id, u(VD), DESK, vung, { trai: 65, tren, rong: 950 }, hien, an, { khoa }, phu);
const MT = (id, khoa, u, che) => mtKhai(id, { rong: 960, trai: 60, tren: 330, lop: [{ anh: A("desk-03-dau.png") }],
  phu: videoKhai(u(VD), { id: `${id}-v`, khoa }) + (che ? lopKhai(CHE_ICON, che, null, "left:0;top:0;width:100%;height:100%") : "") }, u);

// cảnh «kiem»: chọn VB → AI Kiểm tra (cắt 5,4→6,1 hộp nhảy chỗ) → Kiểm tra → xác nhận → AI chạy (tua) → có kết quả
const K_KIEM = [["s", 0.4], ["c0k1w0", 4.95], ["c0k1w2", 5.4], ["c0k1w2", 6.1], ["c0k2w1", 13.0], ["c0e", 14.6],
  ["c1", 15.5], ["c1k0e", 17.7], ["c1k1", 18.2], ["c1e", 35.9], ["e", 36.9]];
// cảnh «loi»: thẻ CV → thẻ ngày → thẻ Nơi nhận
const K_LOI = [["s", 37.0], ["c0e", 40.6], ["c1", 47.4], ["c1e", 48.6], ["c2", 53.2], ["c2e", 54.2], ["e", 54.6]];
// cảnh «ban-sua»: cột phải ở đầu (62) → nhảy tới cuối (Lưu: VT 71,5)
const K_SUA = [["s", 61.0], ["c0k4", 62.0], ["c0k4", 71.45], ["e", 71.5]];

const VUNG_THE = [0.17, 0.52, 0.48, 0.81];     // danh sách thẻ lỗi (cột trái hộp)
const VUNG_DAU = [0.50, 0.255, 0.805, 0.50];   // đầu bản đã sửa (Số · V/v · Quốc hiệu · ngày · Kính gửi)
const VUNG_CUOI = [0.50, 0.62, 0.805, 0.78];   // Nơi nhận · Lưu: VT

const GIAY = `
<div class="vt-giay vt-cv" style="left:110px;top:560px;width:860px;transform:rotate(-1.5deg)">
  <div class="cv-dau"><div><b>ỦY BAN NHÂN DÂN<br>XÃ A</b><br><span class="cv-loi" data-l="1">Số: 15/CV-UBND</span><br><i>V/v đề nghị hỗ trợ kinh phí<br>sửa chữa nhà văn hóa ấp 3</i></div>
  <div><b>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM<br>Độc lập - Tự do - Hạnh phúc</b><br><i class="cv-loi">Xã A ngày 5 tháng 11 năm 2026</i></div></div>
  <p class="cv-giua"><span class="cv-loi">Kính gửi Sở</span> Văn hóa, Thể thao và Du lịch tỉnh B</p>
  <p>Nhà văn hóa ấp 3 xuống cấp nghiêm trọng, mái tôn bị thủng, tường nứt nhiều chỗ…</p>
  <div class="cv-cuoi"><div><b><i>Nơi nhận:</i></b><br>- Như trên;<br><span class="cv-loi cv-trong">&nbsp;</span></div><div><b>TM. ỦY BAN NHÂN DÂN<br>CHỦ TỊCH</b><br><br>Nguyễn Văn B</div></div>
</div>`;

export default {
  ten: "03-kiem-loi-truoc-trinh-ky",
  tieuDe: "Việc thật · 3 · Kiểm lỗi trước khi trình ký",
  bia: { dong: ["Sắp trình ký", "có 4 lỗi?"], cat: [0.1, 0.62] },
  kichBan: [
    { canh: "moc", cau: [
      { loi: "Công văn sắp trình ký, có bốn lỗi thể thức nằm trong này.", chu: "Công văn sắp trình ký — *có 4 lỗi thể thức*" },
      { loi: "Anh chị nhìn ra mấy lỗi?", chu: "Anh chị nhìn ra *mấy lỗi?*" },
    ]},
    { canh: "kiem", cau: [
      { loi: "Trên bản cài máy tính, chạm AI Kiểm tra, rồi chạm Kiểm tra.", chu: "Bản cài máy tính → *AI Kiểm tra* → Kiểm tra" },
      { loi: "Xác nhận dùng credit, AI đọc cả văn bản.", chu: "Xác nhận credit → *AI đọc cả văn bản*" },
    ]},
    { canh: "loi", cau: [
      { loi: "Ký hiệu công văn, không có chữ viết tắt tên loại.", chu: "Ký hiệu công văn — *không có «CV»*" },
      { loi: "Sau địa danh thiếu dấu phẩy, ngày phải có số không.", chu: "Thiếu *dấu phẩy* sau địa danh · ngày *05*" },
      { loi: "Nơi nhận, thiếu dòng lưu văn thư.", chu: "Nơi nhận — *thiếu «Lưu: VT»*" },
    ]},
    { canh: "ban-sua", cau: [
      { loi: "Bên phải là bản đã sửa, ký hiệu, ngày tháng, dấu hai chấm sau Kính gửi, và dòng lưu.", chu: "Bản đã sửa — *đủ cả 4 chỗ*" },
      { loi: "Câu chữ AI viết lại, anh chị soát kỹ, rồi mới trình ký.", chu: "AI viết lại câu chữ — *soát kỹ rồi mới trình ký*" },
    ]},
    canhTomTat("Trước khi trình ký", [
      ["AI chỉ ra từng lỗi", "AI chỉ ra từng lỗi"],
      ["Kèm căn cứ Nghị định ba mươi", "Kèm căn cứ NĐ 30"],
      ["Có sẵn bản đã sửa", "Có sẵn bản đã sửa"],
    ]),
    canhKet(),
  ],
  css: CSS + `
.vt-cv{padding:34px 38px;font-family:"Times New Roman",serif;font-size:21px;line-height:1.38;color:#111}
.vt-cv p{margin:14px 0}
.cv-dau,.cv-cuoi{display:flex;justify-content:space-between;gap:20px;text-align:center}
.cv-dau>div:first-child{width:40%}.cv-dau>div:last-child{width:58%}
.cv-cuoi{text-align:left;margin-top:16px}.cv-cuoi>div:last-child{text-align:center;width:45%}
.cv-giua{text-align:center}
.cv-trong{display:inline-block;width:150px}
.vt-dem{position:absolute;font:800 46px/1 var(--phong-tieu-de,inherit);color:#fff;background:#dc2626;border-radius:50%;width:62px;height:62px;
  display:flex;align-items:center;justify-content:center;box-shadow:0 6px 16px rgba(0,0,0,.25)}
`,
  canh: {
    "moc": { khai: true, html: () => `
      ${mocKhai(`${NGU_CANH("🖥 Việc thật · trước khi trình ký")}Công văn sắp trình ký,<br>*có 4 lỗi thể thức*`, { tren: 250, co: 66, gach: "c0k1w4" })}
      ${lopKhai(GIAY, "s-2", null, "left:0;top:0;width:100%;height:100%")}
      ${hoiKhai(860, 1170, "c1k0")}` },

    "kiem": { khai: true, html: ({ u }) => `
      ${chipKhai("① AI Kiểm tra · bản cài máy tính")}
      ${MT("mt-kiem", K_KIEM, u, "c0k1w2+0.1")}
      ${goiKhai("⏩ tua nhanh · AI đang đọc", { trai: 40, tren: 980, hien: "c1k1", an: "c1e", tu: "trai", lop: "vt-tua" })}
      ${zoomD("cs-xac-nhan", u, [0.755, 0.465, 1.0, 0.70], 700, "c1", "c1k1", K_KIEM)}` },

    "loi": { khai: true, html: ({ u }) => `
      ${chipKhai("② Từng lỗi · kèm căn cứ")}
      ${MT("mt-loi", K_LOI, u, "s")}
      ${zoomD("cs-the", u, VUNG_THE, 640, "s", "e", K_LOI)}
      ${goiKhai(`<span class="vt-dem" style="position:static">1</span>`, { trai: 880, tren: 600, hien: "c0", tu: "phai" })}
      ${goiKhai(`<span class="vt-dem" style="position:static">2</span>`, { trai: 880, tren: 600, hien: "c1", tu: "phai" })}
      ${goiKhai(`<span class="vt-dem" style="position:static">3</span>`, { trai: 880, tren: 600, hien: "c2", tu: "phai" })}` },

    "ban-sua": { khai: true, html: ({ u }) => `
      ${chipKhai("③ Bản đã sửa")}
      ${MT("mt-sua", K_SUA, u, "s")}
      ${zoomD("cs-dau", u, VUNG_DAU, 560, "s", "c0k4", K_SUA,
        sangKhai([2, 21, 32, 33], "c0k1", "c0k4") + sangKhai([2, 70, 52, 82], "c0k2", "c0k4") + sangKhai([2, 86, 20.5, 98], "c0k3", "c0k4"))}
      ${zoomD("cs-cuoi", u, VUNG_CUOI, 620, "c0k4", "c1", K_SUA, sangKhai([2, 40, 32, 62], "c0k4w1", "c1"))}
      ${goiKhai("⚠ Câu chữ AI viết lại — soát kỹ trước khi ký", { trai: 60, tren: 1010, rong: 900, hien: "c1k0", tu: "duoi" })}` },
  },
  mocThu: (tatCa) => [
    ["02-bam", lucNode(tatCa, "kiem", "c0k2w1")], ["02-xac-nhan", lucNode(tatCa, "kiem", "c1+0.6")], ["02-tua", lucNode(tatCa, "kiem", "c1k1+1")],
    ["03-cv", lucNode(tatCa, "loi", "c0+1")], ["03-ngay", lucNode(tatCa, "loi", "c1+0.8")], ["03-luu", lucNode(tatCa, "loi", "c2+0.6")],
    ["04-dau", lucNode(tatCa, "ban-sua", "c0k3+0.3")], ["04-cuoi", lucNode(tatCa, "ban-sua", "c0k4+0.6")],
  ],
  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1e+0.3"),
};
