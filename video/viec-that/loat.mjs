// ============================================================================
// LOẠT «VIỆC THẬT» (video/viec-that/) — mỗi clip một việc thật của cán bộ, phần mềm giải xong, kết quả cầm về dùng ngay.
// Kế hoạch: KE-HOACH.md cùng thư mục. Hình: QUAY THẬT prod (video/cong-cu/quay-web/quay.mjs → video/anh/viec-that/).
//   · Xưng hô «anh chị», không đọc tên VanBanPlus, không chữ Latin trong lời đọc.
//   · Màn kết: địa chỉ bản web + giá ĐỌC TỪ PROD + dùng thử miễn phí.
//     Giá đọc 22/11/2026 (GET /api/plans): «Miễn phí» 0đ — «Dùng thử — 200MB cloud, AI 30 credit/tháng»;
//     «Khởi đầu» price_per_month 99000 — «Cá nhân — 2GB cloud, AI 400 credit/tháng». Đổi giá → sửa câu 2 + the[1].
// ============================================================================

export default {
  ket: {
    kieu: "thong-diep",
    cau: [
      { loi: "Dùng thử miễn phí, ngay trên trình duyệt.", chu: "Dùng thử miễn phí — ngay trên trình duyệt" },
      { loi: "Gói Khởi đầu chín mươi chín nghìn đồng một tháng, địa chỉ trên màn hình.", chu: "Gói Khởi đầu 99.000đ/tháng · app.vanbanplus.giakiemso.com" },
    ],
    the: [
      "Dùng thử *miễn phí*, ngay trên trình duyệt.",
      "Gói Khởi đầu *99.000đ/tháng*<b class=\"vt-dia-chi\">app.vanbanplus<br>.giakiemso.com</b>",
    ],
    nhan: "VIỆC THẬT · LÀM TRÊN PHẦN MỀM",
    nguon: "VanBanPlus · Công ty Gia Kiệm Số",
    loat: "",
    soPhan: 0,
    tren: 430,
  },
};

/** dòng ngữ cảnh khung 0, đặt ĐẦU chữ của mocKhai */
export const NGU_CANH = (chu = "🌐 Việc thật · làm trên trình duyệt") => `<b class="ngu-canh">${chu}</b><br>`;

/** kích thước bản quay web (khung 390×844 ×3) */
export const WEB = [1170, 2532];

export const CSS = `
.ngu-canh{display:inline-block;font-size:34px;font-weight:600;color:#475569;margin-bottom:14px;letter-spacing:.3px}
.vt-dia-chi{display:block;font-size:54px;color:var(--mau-chinh);line-height:1.15;margin-top:18px}
.vt-giay{position:absolute;background:#fff;box-shadow:0 18px 44px rgba(15,23,42,.28);border-radius:4px}
.vt-tua{font-weight:700}
`;
