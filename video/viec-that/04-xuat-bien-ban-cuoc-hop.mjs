// Clip 04 · Việc thật: họp xong → xuất biên bản cuộc họp ra Word trên bản cài máy tính (không dùng AI, không tốn credit).
// Quay THẬT (23/11/2026): app dựng từ mã (sau khi sửa phần đầu biên bản — xem ⚠ dưới), cuộc họp HƯ CẤU tạo qua giao diện
//   «Họp giao ban tháng 11 năm 2026» · Ủy ban nhân dân xã A · chủ trì Nguyễn Văn B · thư ký Trần Thị C · 5 người dự.
//   video/cong-cu/quay-app/quay-viec-that-04.ps1 → video/anh/viec-that/desk-04.* (cửa sổ app ở x 1120 màn 2560: menu không tràn)
//   tệp Word xuất ra trong lúc quay → desk-04-bien-ban.docx → LibreOffice + PyMuPDF 200 dpi → desk-04-trang-1/2.png
//   ảnh hộp «Thêm cuộc họp mới» lúc nhập (vùng client, không thanh tiêu đề) → desk-04-thanh-phan.png, desk-04-noi-dung.png
// desk-04-cat.mp4 = 3 đoạn của desk-04.mp4 (17,6 s; máy rớt khung nên nhanh hơn thật ~1,5 lần — tua lại theo mốc):
//   0–10,2 → 0 · 10,6–14,35 → 10,2 · 14,9–17,6 → 13,95 (cắt 2 chỗ hộp nhảy chỗ)
//   mốc (giây bản cắt): chuột tới thẻ họp ~3,5 · chạm biểu tượng Word 6,8 · menu 7,3–9,9 · chạm «Xuất Biên bản cuộc họp» 9,9
//   · hộp «Lưu file Word» 10,2–13,9 (chạm Lưu 13,75) · hộp «Xuất Word thành công» 13,95–16,67
// SOÁT:
//  ✔ App: thẻ cuộc họp › biểu tượng Word (ToolTip «Xuất Word») › menu «📝 Xuất Biên bản cuộc họp» › hộp «Lưu file Word»
//    › «✅ Đã xuất file thành công!» (MeetingListPage.xaml.cs ShowExportMenu / ExportMeetingWord). Không gọi AI, không credit.
//  ✔ Nội dung tệp = dữ liệu đã nhập ở hộp cuộc họp (MeetingWordExportService.ExportBienBan): phần đầu, BIÊN BẢN + tên cuộc họp,
//    Thời gian / Địa điểm / Chủ trì / Thư ký, I. THÀNH PHẦN THAM DỰ (bảng), II. NỘI DUNG CUỘC HỌP, III. KẾT LUẬN CUỘC HỌP,
//    «Cuộc họp kết thúc vào 10 giờ 30, ngày 23 tháng 11 năm 2026.», chỗ ký THƯ KÝ / CHỦ TRÌ. Tệp .docx sửa được trong Word.
//  ✔ NĐ 30/2020 Phụ lục III, Mẫu 1.9 - Biên bản (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-III.md), nguyên văn:
//    «Thời gian bắt đầu: …» · «Địa điểm: …» · «Thành phần tham dự: …» · «Chủ trì (chủ toạ):» · «Thư ký (người ghi biên bản):»
//    · «Nội dung (theo diễn biến cuộc họp/hội nghị/hội thảo):» · «Cuộc họp (hội nghị, hội thảo) kết thúc vào …. giờ …., ngày …. tháng …. năm …../.»
//  ✔ Phụ lục I: «Quốc hiệu và Tiêu ngữ được trình bày tại ô số 1» · «Tên cơ quan, tổ chức ban hành văn bản được trình bày tại ô số 2»
//    · mục 4 c) «Địa danh và thời gian ban hành văn bản được trình bày trên cùng một dòng với số, ký hiệu văn bản … kiểu chữ nghiêng;
//    … sau địa danh có dấu phẩy (,)». Tệp ra: «ỦY BAN NHÂN DÂN XÃ A» trái · Quốc hiệu phải · «Số:        /BB-UBND» · «Xã A, ngày 23 tháng 11 năm 2026».
//    Số để trống (văn thư cấp khi ban hành) → lời nói «chỗ ghi số», không nói «có số».
//  · Lời KHÔNG nói «đúng thể thức»: lề trang của bản xuất (trái 2 cm, phải 1 cm) chưa theo NĐ 30 (trái 3–3,5 cm, phải 1,5–2 cm).
//  ⚠ Phần đầu hai cột, địa danh, khối ký không tách trang, «./.», cột Vai trò = mã sửa 23/11/2026 (MeetingWordExportService),
//    CHƯA có trong bản cài 1.0.21 — bản cài cũ xếp tên cơ quan dưới Quốc hiệu. Phát hành bản cài mới TRƯỚC khi đăng clip.
//  · Che: biểu tượng app (Quốc huy) trên thanh tiêu đề hộp «Lưu file Word» (ô #F3F2F5). Hộp «Xuất Word thành công» không có biểu tượng app.
import { canhKet, canhTomTat, lucNode, chipKhai, mtKhai, goiKhai, cuaSoKhai, cuaSoVideoKhai, videoKhai, mocKhai, lopKhai, cheKhai, sangKhai, hoiKhai } from "../kit.mjs";
import { NGU_CANH, CSS } from "./loat.mjs";

const A = (ten) => `viec-that/${ten}`;
const VD = A("desk-04-cat.mp4");
const DESK = [1424, 860];
const HOP = [934, 681];            // ảnh hộp «Thêm cuộc họp mới»
const TRANG = [1654, 2339];        // trang biên bản 200 dpi
const CHE_ICON = cheKhai([9.4, 9.9, 11.0, 12.2], "#F3F2F5");   // biểu tượng app trên thanh tiêu đề hộp «Lưu file Word»
const zoomD = (id, u, vung, tren, hien, an, khoa, phu) => cuaSoVideoKhai(id, u(VD), DESK, vung, { trai: 65, tren, rong: 950 }, hien, an, { khoa }, phu);

// cảnh «xuat»: chuột tới thẻ → biểu tượng Word → menu → Xuất Biên bản → hộp Lưu → Lưu → thành công
const K_XUAT = [["s", 2.0], ["c0k0e", 4.6], ["c0k1w3", 6.8], ["c0k2w2", 9.9], ["c0e", 10.15],
  ["c1", 10.25], ["c1k0w1", 13.75], ["c1k1", 14.25], ["c1e", 16.3], ["e", 16.6]];

const VUNG_MENU = [0.62, 0.42, 0.95, 0.72];    // biểu tượng Word + menu xuất
const VUNG_TEN = [0.11, 0.535, 0.44, 0.60];    // ô tên tệp trong hộp Lưu
const VUNG_BAO = [0.29, 0.345, 0.585, 0.595];  // hộp «Xuất Word thành công»

const NHAP = `
<div class="vt-giay vt-nhap" style="left:120px;top:600px;width:840px;transform:rotate(-2deg)">
  <div class="nhap-dau">Ghi chép họp giao ban — 23/11</div>
  <div>- đ/c C: tháng 11 các bộ phận xong nv…</div>
  <div>- đ/c E: thu NS 85% dự toán <s>80%</s></div>
  <div>- KL: TC-KT đôn đốc thu, bc trước 10/12 ??</div>
  <div>- VP chuẩn bị bc tổng kết… (hỏi lại)</div>
  <div>- vắng: … ?</div>
</div>`;

const HAI_TRANG = (u) => `
<img class="vt-giay" src="${u(A("desk-04-trang-1.png"))}" style="left:70px;top:430px;width:470px;transform:rotate(-2deg)">
<img class="vt-giay" src="${u(A("desk-04-trang-2.png"))}" style="left:545px;top:450px;width:470px;transform:rotate(2deg)">`;

export default {
  ten: "04-xuat-bien-ban-cuoc-hop",
  tieuDe: "Việc thật · 4 · Họp xong có biên bản",
  bia: { dong: ["Họp xong", "có biên bản ngay"], cat: [0.1, 0.62] },
  kichBan: [
    { canh: "moc", cau: [
      { loi: "Họp xong, lãnh đạo dặn, chiều nay phải có biên bản trình ký.", chu: "Họp xong — *chiều nay phải có biên bản*" },
      { loi: "Anh chị lại ngồi gõ lại từ đầu?", chu: "Lại ngồi *gõ lại từ đầu?*" },
    ]},
    { canh: "ghi", cau: [
      { loi: "Trong lúc họp, thư ký ghi người dự, nội dung, và kết luận vào phần mềm.", chu: "Trong lúc họp — ghi *người dự · nội dung · kết luận*" },
    ]},
    { canh: "xuat", cau: [
      { loi: "Họp xong, chạm biểu tượng Word, chọn xuất biên bản cuộc họp.", chu: "Biểu tượng Word → *Xuất Biên bản cuộc họp*" },
      { loi: "Bấm lưu, tệp Word ra ngay.", chu: "Bấm Lưu → *tệp Word ra ngay*" },
    ]},
    { canh: "ket-qua", cau: [
      { loi: "Phần đầu có tên cơ quan, quốc hiệu, chỗ ghi số, và ngày tháng.", chu: "Phần đầu — *cơ quan · Quốc hiệu · số · ngày tháng*" },
      { loi: "Đủ thời gian, địa điểm, chủ trì, thư ký, bảng người dự.", chu: "Thời gian · địa điểm · chủ trì · thư ký · *bảng người dự*" },
      { loi: "Kết luận, giờ kết thúc, và chỗ ký của thư ký, chủ trì.", chu: "Kết luận · giờ kết thúc · *chỗ ký thư ký, chủ trì*" },
      { loi: "Anh chị đọc lại, sửa câu chữ nếu cần, rồi trình ký.", chu: "Đọc lại · sửa nếu cần → *trình ký*" },
    ]},
    canhTomTat("Họp xong có biên bản", [
      ["Ghi ngay trong lúc họp", "Ghi ngay trong lúc họp"],
      ["Vài lần chạm ra tệp Word", "Vài lần chạm ra tệp Word"],
      ["Đủ các phần của biên bản", "Đủ các phần biên bản"],
    ]),
    canhKet(),
  ],
  css: CSS + `
.vt-nhap{padding:34px 40px;font:italic 500 38px/1.55 "Segoe Print","Comic Sans MS",cursive;color:#1e3a8a;
  background:repeating-linear-gradient(#fff 0 56px,#cbd5e1 56px 58px);background-position:0 26px}
.vt-nhap .nhap-dau{font-weight:700;color:#334155;margin-bottom:6px}
.vt-nhap s{color:#dc2626}
img.vt-giay{border-radius:2px}
`,
  canh: {
    "moc": { khai: true, html: () => `
      ${mocKhai(`${NGU_CANH("🖥 Việc thật · sau cuộc họp")}Họp xong,<br>*chiều nay phải có biên bản*`, { tren: 250, co: 68, gach: "c0k2w3" })}
      ${lopKhai(NHAP, "s-2", null, "left:0;top:0;width:100%;height:100%")}
      ${hoiKhai(880, 1180, "c1k0")}` },

    "ghi": { khai: true, html: ({ u }) => `
      ${chipKhai("① Ghi ngay trong lúc họp")}
      ${cuaSoKhai("cs-tp", u(A("desk-04-thanh-phan.png")), HOP, [0, 0, 1, 1], { trai: 65, tren: 360, rong: 950 }, "s", "c0k2",
        sangKhai([13, 46.5, 94.5, 86.5], "c0k1", "c0k2"))}
      ${cuaSoKhai("cs-nd", u(A("desk-04-noi-dung.png")), HOP, [0, 0, 1, 1], { trai: 65, tren: 360, rong: 950 }, "c0k2", "e",
        sangKhai([12.5, 34, 93.5, 52.5], "c0k2", "c0k3") + sangKhai([12.5, 57.5, 93.5, 76], "c0k3", "e"))}` },

    "xuat": { khai: true, html: ({ u }) => `
      ${chipKhai("② Xuất biên bản · bản cài máy tính")}
      ${mtKhai("mt-xuat", { rong: 960, trai: 60, tren: 330, lop: [{ anh: A("desk-04-dau.png") }],
        phu: videoKhai(u(VD), { id: "mt-xuat-v", khoa: K_XUAT }) + lopKhai(CHE_ICON, "c1", "c1k1", "left:0;top:0;width:100%;height:100%")
          + sangKhai([77.6, 65.6, 84.2, 69.2], "c1k0", "c1k0w1+0.4") }, u)}
      ${zoomD("cs-menu", u, VUNG_MENU, 620, "c0k1w2", "c0e", K_XUAT, sangKhai([25, 36, 85, 49], "c0k2", "c0e"))}
      ${zoomD("cs-ten", u, VUNG_TEN, 760, "c1", "c1k1", K_XUAT)}
      ${zoomD("cs-bao", u, VUNG_BAO, 640, "c1k1", "e", K_XUAT)}` },

    "ket-qua": { khai: true, html: ({ u }) => `
      ${chipKhai("③ Tệp Word vừa xuất")}
      ${cuaSoKhai("cs-dau-t", u(A("desk-04-trang-1.png")), TRANG, [0.08, 0.055, 0.42, 0.15], { trai: 160, tren: 400, rong: 760 }, "s", "c1",
        sangKhai([10, 9, 91, 42], "c0k0", "c1") + sangKhai([20, 66, 82, 93], "c0k2", "c1"))}
      ${cuaSoKhai("cs-dau-p", u(A("desk-04-trang-1.png")), TRANG, [0.42, 0.055, 0.94, 0.15], { trai: 65, tren: 750, rong: 950 }, "s", "c1",
        sangKhai([2, 10, 96, 56], "c0k1", "c1") + sangKhai([11, 66, 84, 93], "c0k3", "c1"))}
      ${cuaSoKhai("cs-tt", u(A("desk-04-trang-1.png")), TRANG, [0.08, 0.21, 0.97, 0.63], { trai: 65, tren: 330, rong: 950 }, "c1", "c2",
        sangKhai([6, 2.5, 76, 8], "c1k0", "c2") + sangKhai([6, 8.4, 57, 13.2], "c1k1", "c2") + sangKhai([6, 14.3, 66, 19], "c1k2", "c2")
        + sangKhai([6, 20.2, 30, 24.8], "c1k3", "c2") + sangKhai([1, 46, 99.5, 99.5], "c1k4", "c2"))}
      ${cuaSoKhai("cs-kl", u(A("desk-04-trang-2.png")), TRANG, [0.08, 0.10, 0.97, 0.475], { trai: 65, tren: 380, rong: 950 }, "c2", "c3",
        sangKhai([0.5, 2, 99.5, 43], "c2k0", "c3") + sangKhai([3, 47, 99, 61], "c2k1", "c3") + sangKhai([16, 67, 38, 98], "c2k2", "c3") + sangKhai([57, 67, 83, 98], "c2k3", "c3"))}
      ${lopKhai(HAI_TRANG(u), "c3", null, "left:0;top:0;width:100%;height:100%")}
      ${goiKhai("✎ Tệp Word — sửa được như văn bản thường", { trai: 60, tren: 330, rong: 900, hien: "c3k1", tu: "duoi" })}` },
  },
  mocThu: (tatCa) => [
    ["02-ghi-tp", lucNode(tatCa, "ghi", "c0k1+0.5")], ["02-ghi-kl", lucNode(tatCa, "ghi", "c0k3+0.5")],
    ["03-menu", lucNode(tatCa, "xuat", "c0k2+0.4")], ["03-luu", lucNode(tatCa, "xuat", "c1k0+0.3")], ["03-bao", lucNode(tatCa, "xuat", "c1k1+0.8")],
    ["04-dau", lucNode(tatCa, "ket-qua", "c0k3+0.3")], ["04-tt", lucNode(tatCa, "ket-qua", "c1k4+0.3")], ["04-kl", lucNode(tatCa, "ket-qua", "c2k3+0.3")],
    ["04-hai", lucNode(tatCa, "ket-qua", "c3k1+0.5")],
  ],
  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1+1.2"),
};
