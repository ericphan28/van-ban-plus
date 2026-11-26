// Clip 06 · Việc thật: sổ văn bản đến tự đánh số đến, mở máy là báo văn bản quá hạn — quay trên BẢN CÀI MÁY TÍNH.
// Quay THẬT 26/11/2026 bằng app dựng từ mã main (v1.0.22), dữ liệu demo («⋮ › Tạo dữ liệu demo», hạn tính theo ngày quay)
//   + 2 văn bản đến hư cấu nhập qua giao diện: 245/UBND-VP (UBND tỉnh B, số đến 18, hạn 03/12) và — trong lúc quay —
//   312/SNV-CCVC «V/v rà soát vị trí việc làm năm 2027» (Sở Nội vụ tỉnh B, số đến 19, hạn 04/12).
//   video/cong-cu/quay-app/quay-viec-that-06.ps1 -Canh vao-so | mo-may | lich → video/anh/viec-that/desk-06-<cảnh>.mp4
//   (ghi bằng PrintWindow, cửa sổ app ngoài màn — không con trỏ chuột, không rớt khung; mốc trong <tệp>.moc.tsv)
//   → desk-06-<cảnh>-30.mp4 (30 fps, GOP 15) + desk-06-<cảnh>-dau.png (khung đầu cho mtKhai). Khung quay 1424×860.
// Mốc (giây video):
//   vao-so: chạm Thêm mới 2,11 · hộp mở 2,46 · chạm Hướng 3,61 → «Văn bản đến», Số đến 19 + Ngày đến 26/11 hiện ~4,3
//           · Loại Công văn 6,89 · gõ Số 6,93 · Tiêu đề 7,67 · Cơ quan ban hành 9,73 · Hạn giải quyết 10,75 (04/12)
//           · chạm «Lưu & Thêm mới» 12,98 · hộp sau 14,01 — Số đến 20 · chạm Hủy 18,9
//   mo-may: app vừa mở lại, thông báo đáy «⚠ 3 VB quá hạn · ⏰ 6 VB sắp hết hạn  XEM CHI TIẾT» 0–~8 s; Trang chủ «CẦN XỬ LÝ»
//   lich  : chạm Lịch tổng hợp 1,12 → tháng 11/2026 hiện ~1,6
// SOÁT:
//  ✔ «Số đến tự nhảy số kế tiếp trong năm»: DocumentService.GetNextArrivalNumber — max(ArrivalNumber) của VB đến có
//    ArrivalDate trong năm hiện tại + 1; DocumentEditDialog.UpdateDirectionPanels tự điền khi chọn Hướng = Đến và ô trống
//    (ô «Số đến (tự động)» chỉ đọc). Tooltip app: «Theo Điều 22 NĐ 30/2020: Số đến liên tiếp theo thứ tự và trình tự thời gian trong năm.»
//  ✔ «ngày đến tự ghi hôm nay»: UpdateDirectionPanels — dpArrivalDate = DateTime.Now khi chưa có.
//  ✔ «nhập số, trích yếu, nơi gửi, và hạn giải quyết»: ô Số văn bản, Tiêu đề văn bản (tooltip «Trích yếu…»), Cơ quan ban
//    hành, Hạn giải quyết (dpDueDate) — đều có trên hộp. Không nói «chỉ nhập» (Loại, Ngày ban hành… cũng phải sửa).
//  ✔ «Lưu và thêm mới, văn bản sau tự lên số tiếp theo»: SaveAndAddNew_Click → DocumentListPage.AddDocument_Click mở hộp
//    mới + PreFillDefaults(Hướng) → UpdateDirectionPanels → GetNextArrivalNumber (quay thật: 19 → 20).
//  ✔ «mở máy, phần mềm báo ngay, ba văn bản quá hạn, sáu văn bản sắp hết hạn»: MainWindow.CheckOverdueOnStartupAsync chạy
//    lúc khởi động → Snackbar «⚠️ {n} VB quá hạn · ⏰ {m} VB sắp hết hạn» (GetOverdueDocuments: VB đến, hạn < hôm nay, chưa
//    Lưu hồ sơ/Phát hành; sắp hết hạn = 0–3 ngày). Số 3 / 6 là của dữ liệu quay, có trên hình. Trang chủ «CẦN XỬ LÝ» cùng số.
//  ✔ «Mở lịch, hạn trễ tô đỏ, sắp đến hạn tô cam»: CalendarPage.LoadMonthEvents — quá hạn đỏ (198,40,40), ≤ 3 ngày cam
//    (245,127,23), còn lại xanh nhạt; văn bản đặt ở ô ngày DueDate → «văn bản vừa vào sổ nằm ở ngày hết hạn» (312/SNV… ở 4/12).
//  ✔ Không gọi AI, không tốn credit; không nói giá trong lời riêng (bản cài máy — NOI-DUNG mục 5); màn kết của loạt giữ nguyên.
//  · Câu móc «quá hạn ba ngày» khớp dữ liệu: 030/HD-SNV hạn 23/11, quay 26/11 (lịch: ô 23 đỏ).
//  · Che: biểu tượng app (Quốc huy) trên thanh tiêu đề hộp «Thêm văn bản mới» (ô #F3F3F3, khung quay x 305–323, y 36–49).
//  · Lời chào «Chào buổi chiều» trên Trang chủ → lời không nói «sáng».
import { canhKet, canhTomTat, lucNode, chipKhai, mtKhai, goiKhai, cuaSoVideoKhai, videoKhai, mocKhai, lopKhai, cheKhai, sangKhai, hoiKhai, chamKhai } from "../kit.mjs";
import { NGU_CANH, CSS } from "./loat.mjs";

const A = (ten) => `viec-that/${ten}`;
const DESK = [1424, 860];
const VD_SO = A("desk-06-vao-so-30.mp4"), VD_MO = A("desk-06-mo-may-30.mp4"), VD_LICH = A("desk-06-lich-30.mp4");
const CHE_ICON = cheKhai([21.2, 3.9, 22.9, 6.5], "#F3F3F3");
const zoom = (id, u, vd, vung, tren, hien, an, khoa, phu, rong = 950, trai = 65) =>
  cuaSoVideoKhai(id, u(vd), DESK, vung, { trai, tren, rong }, hien, an, { khoa }, phu);

// cảnh «vao-so»: mốc cảnh → giây video
const K_SO = [["s", 1.4], ["c0k1w1", 2.11], ["c0k2w1", 3.61], ["c0e", 4.45], ["c1", 4.6], ["c1e", 6.6], ["c2k0w2", 6.95], ["c2k1", 7.7],
  ["c2k2", 9.75], ["c2k3", 10.8], ["c2e", 12.2], ["c3k0w1", 12.98], ["c3k1", 14.75], ["e", 17.5]];
const K_MO = [["s", 0.2], ["e", 7.8]];
const K_LICH = [["s", 0.4], ["c0k0w1", 1.12], ["c0k1", 2.6], ["e", 8.0]];

const V_HUONG = [0.5, 0.28, 0.765, 0.36];      // ô Hướng
const V_SODEN = [0.225, 0.585, 0.5, 0.672];    // ô «Số đến (tự động)»
const V_NGAY = [0.497, 0.585, 0.772, 0.672];   // ô «Ngày đến»
const V_SO_TD = [0.225, 0.135, 0.56, 0.285];   // Số văn bản · Tiêu đề
const V_HAN_CQ = [0.225, 0.665, 0.56, 0.82];   // Hạn giải quyết · Cơ quan ban hành
const V_BAO = [0.36, 0.9, 0.64, 0.97];         // thông báo khởi động
const V_CAN = [0.19, 0.315, 0.575, 0.64];      // Trang chủ «CẦN XỬ LÝ»
const V_DO = [0.192, 0.613, 0.476, 0.82];      // lịch: ô 16–19 / 23–26 (đỏ)
const V_CAM = [0.405, 0.613, 0.69, 0.82];      // lịch: ô 19–22 / 26–29 (cam)
const V_MOI = [0.405, 0.82, 0.69, 0.935];      // lịch: ô 3–6/12 (văn bản vừa vào sổ)

const SO_TAY = `
<div class="vt-giay vt-nhap" style="left:110px;top:620px;width:860px;transform:rotate(-2deg)">
  <div class="nhap-dau">Sổ VB đến — tháng 11</div>
  <div>16 · 21/11 · Sở TC — dự toán · hạn ?</div>
  <div>17 · 23/11 · Sở NV · <u>hạn 23/11</u></div>
  <div>?? · 26/11 · UBND tỉnh · hạn …</div>
  <div class="nhap-hoi">(số mấy rồi nhỉ… hạn ngày nào?)</div>
</div>`;

export default {
  ten: "06-so-van-ban-den",
  tieuDe: "Việc thật · 6 · Sổ văn bản đến",
  bia: { dong: ["Văn bản đến", "quá hạn 3 ngày?"], cat: [0.1, 0.62] },
  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản đến ghi sổ tay, hạn giải quyết nhớ trong đầu.", chu: "Văn bản đến — *ghi sổ tay, nhớ hạn trong đầu*" },
      { loi: "Đến khi bị hỏi, anh chị mới biết đã quá hạn ba ngày?", chu: "Bị hỏi mới biết — *quá hạn 3 ngày?*" },
    ]},
    { canh: "vao-so", cau: [
      { loi: "Nhận văn bản, bấm thêm mới, chọn văn bản đến.", chu: "Thêm mới → Hướng: *Văn bản đến*" },
      { loi: "Số đến tự nhảy số kế tiếp trong năm, ngày đến tự ghi hôm nay.", chu: "*Số đến* tự cấp · *ngày đến* tự ghi" },
      { loi: "Anh chị nhập số, trích yếu, nơi gửi, và hạn giải quyết.", chu: "Nhập số · trích yếu · nơi gửi · *hạn giải quyết*" },
      { loi: "Bấm lưu và thêm mới, văn bản sau tự lên số tiếp theo.", chu: "Lưu & Thêm mới → *số đến tự tăng*" },
    ]},
    { canh: "mo-may", cau: [
      { loi: "Lần sau mở máy, phần mềm báo ngay, ba văn bản quá hạn, sáu văn bản sắp hết hạn.", chu: "Mở máy là báo — *3 quá hạn · 6 sắp hết hạn*" },
    ]},
    { canh: "lich", cau: [
      { loi: "Mở lịch, hạn trễ tô đỏ, sắp đến hạn tô cam, văn bản vừa vào sổ nằm sẵn ở ngày hết hạn.", chu: "Lịch: *đỏ = quá hạn* · cam = sắp hạn" },
    ]},
    canhTomTat("Sổ văn bản đến tự lo", [
      ["Số đến tự cấp", "Số đến tự cấp"],
      ["Mở máy là báo quá hạn", "Mở máy báo quá hạn"],
      ["Lịch tô đỏ việc trễ hạn", "Lịch tô đỏ việc trễ hạn"],
    ]),
    canhKet(),
  ],
  css: CSS + `
.vt-nhap{padding:34px 40px;font:italic 500 38px/1.55 "Segoe Print","Comic Sans MS",cursive;color:#1e3a8a;
  background:repeating-linear-gradient(#fff 0 56px,#cbd5e1 56px 58px);background-position:0 26px}
.vt-nhap .nhap-dau{font-weight:700;color:#334155;margin-bottom:6px}
.vt-nhap u{color:#dc2626;text-decoration-thickness:3px}
.vt-nhap .nhap-hoi{color:#64748b;font-size:32px}
`,
  canh: {
    "moc": { khai: true, html: () => `
      ${mocKhai(`${NGU_CANH("🖥 Việc thật · sổ văn bản đến")}Văn bản đến,<br>*quá hạn ba ngày mới biết?*`, { tren: 250, co: 68, gach: "c1k1w5" })}
      ${lopKhai(SO_TAY, "s-2", null, "left:0;top:0;width:100%;height:100%")}
      ${hoiKhai(880, 1180, "c1k0")}` },

    "vao-so": { khai: true, html: ({ u }) => `
      ${chipKhai("① Vào sổ văn bản đến · bản cài máy tính")}
      ${mtKhai("mt-so", { rong: 960, trai: 60, tren: 330, lop: [{ anh: A("desk-06-vao-so-dau.png") }],
        phu: videoKhai(u(VD_SO), { id: "mt-so-v", khoa: K_SO }) + lopKhai(CHE_ICON, "c0k1w1+0.4", null, "left:0;top:0;width:100%;height:100%")
          + chamKhai(81.9, 28.6, "c0k1w1") + chamKhai(63.1, 32.4, "c0k2w1") + chamKhai(67.8, 89.1, "c3k0w1") }, u)}
      ${zoom("cs-huong", u, VD_SO, V_HUONG, 990, "c0k2", "c1", K_SO, sangKhai([0.5, 19, 98, 92], "c0k2w1+0.4", "c1"), 760, 160)}
      ${zoom("cs-soden", u, VD_SO, V_SODEN, 990, "c1", "c1k1", K_SO, sangKhai([2, 15, 97, 88], "c1k0+0.2", "c1k1"))}
      ${zoom("cs-ngay", u, VD_SO, V_NGAY, 990, "c1k1", "c2", K_SO, sangKhai([1, 15, 96, 88], "c1k1+0.2", "c2"))}
      ${zoom("cs-so-td", u, VD_SO, V_SO_TD, 975, "c2", "c2k2", K_SO,
        sangKhai([2, 6, 99.5, 45], "c2k0", "c2k1") + sangKhai([2, 56.6, 99.5, 95.3], "c2k1", "c2k2"))}
      ${zoom("cs-han-cq", u, VD_SO, V_HAN_CQ, 975, "c2k2", "c3", K_SO,
        sangKhai([2, 56.3, 99.5, 93.8], "c2k2", "c2k3") + sangKhai([2, 7.6, 79.4, 45], "c2k3", "c3"))}
      ${zoom("cs-soden-2", u, VD_SO, V_SODEN, 990, "c3k1-0.2", "e", K_SO, sangKhai([2, 15, 97, 88], "c3k1", "e"))}
      ${goiKhai("✓ Văn bản sau: số đến 20", { trai: 300, tren: 1185, hien: "c3k1+0.5", an: "e", tu: "duoi" })}` },

    "mo-may": { khai: true, html: ({ u }) => `
      ${chipKhai("② Mở máy là biết")}
      ${mtKhai("mt-mo", { rong: 960, trai: 60, tren: 330, hien: "c0k0w3", lop: [{ anh: A("desk-06-mo-may-dau.png") }],
        phu: videoKhai(u(VD_MO), { id: "mt-mo-v", khoa: K_MO }) }, u)}
      ${zoom("cs-bao", u, VD_MO, V_BAO, 990, "c0k1", "e", K_MO,
        sangKhai([5, 22, 33, 78], "c0k2", "c0k3") + sangKhai([36, 22, 69.5, 78], "c0k3", "e"))}
      ${zoom("cs-can", u, VD_MO, V_CAN, 420, "c0k2", "e", K_MO,
        sangKhai([1.5, 17, 98, 32], "c0k2", "c0k3") + sangKhai([1.5, 33.5, 98, 48.5], "c0k3", "e"), 760, 160)}` },

    "lich": { khai: true, html: ({ u }) => `
      ${chipKhai("③ Lịch tổng hợp")}
      ${mtKhai("mt-lich", { rong: 960, trai: 60, tren: 330, lop: [{ anh: A("desk-06-lich-dau.png") }],
        phu: videoKhai(u(VD_LICH), { id: "mt-lich-v", khoa: K_LICH }) + chamKhai(7.8, 56.5, "c0k0w1") }, u)}
      ${zoom("cs-do", u, VD_LICH, V_DO, 780, "c0k1", "c0k2", K_LICH, sangKhai([26, 12, 74, 28], "c0k1", "c0k2") + sangKhai([1, 63, 23.5, 79], "c0k1", "c0k2"))}
      ${zoom("cs-cam", u, VD_LICH, V_CAM, 780, "c0k2", "c0k3", K_LICH, sangKhai([1.5, 63, 99.5, 95], "c0k2", "c0k3"))}
      ${zoom("cs-moi", u, VD_LICH, V_MOI, 900, "c0k3", "e", K_LICH, sangKhai([25, 22, 50, 58], "c0k3", "e"))}
      ${goiKhai("🔴 Quá hạn", { trai: 70, tren: 700, hien: "c0k1+0.2", an: "c0k2", tu: "trai" })}
      ${goiKhai("🟠 Sắp đến hạn", { trai: 70, tren: 700, hien: "c0k2+0.2", an: "c0k3", tu: "trai" })}
      ${goiKhai("📄 Văn bản vừa vào sổ — hạn 04/12", { trai: 70, tren: 790, hien: "c0k3+0.3", an: "e", tu: "trai" })}` },
  },
  mocThu: (tatCa) => [
    ["02-huong", lucNode(tatCa, "vao-so", "c0k2+0.6")], ["02-soden", lucNode(tatCa, "vao-so", "c1k1+0.3")],
    ["02-go", lucNode(tatCa, "vao-so", "c2k1+0.3")], ["02-han", lucNode(tatCa, "vao-so", "c2k3+0.5")],
    ["02-so20", lucNode(tatCa, "vao-so", "c3k1+0.8")],
    ["03-bao", lucNode(tatCa, "mo-may", "c0k2+0.3")], ["03-sap", lucNode(tatCa, "mo-may", "c0k3+0.5")],
    ["04-do", lucNode(tatCa, "lich", "c0k1+0.4")], ["04-moi", lucNode(tatCa, "lich", "c0k3+0.6")],
  ],
  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1+1.2"),
};
