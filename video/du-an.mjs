// ============================================================================
// CẤU HÌNH CLIP — VanBanPlus, kênh «AI Tạo Văn Bản Hành Chính [GIKIDI]» (YouTube @gikidi.vanban, Trang Facebook
// «AI Tạo Văn Bản Hành Chính Gikidi», TikTok @gikidi.vanban «AI Văn Bản Hành Chính GIKIDI»). Lập 08/11/2026.
// Bộ máy dựng: D:\Thang\clip-kit (skill cá nhân tao-clip-quang-ba).
//
//   node D:/Thang/clip-kit/chay.mjs                     → liệt kê loạt + clip
//   node D:/Thang/clip-kit/chay.mjs the-thuc/01 --thu   → ảnh thử (video/.tam/the-thuc/01-…/thu/_hang-N.jpg)
//   máy chủ: clip-dung them AIVanBanCaNhan the-thuc/01 [--thu] [--ngay]
//
// Được / không được nói: video/NOI-DUNG.md (đọc trước khi viết lời). Loạt: video/the-thuc/ (mẹo thể thức NĐ 30/2020).
// Tài sản kênh: video/thuong-hieu/ — sinh lại bằng video/cong-cu/thuong-hieu/chup.mjs.
// ============================================================================
export default {
  thuongHieu: {
    ten: "VanBanPlus",          // chỉ hiện trên hình — KHÔNG đọc tên thương hiệu trong lời
    nhan: "",
    viTat: "VB+",
    tienToTep: "vanbanplus",    // vanbanplus-<tên clip bỏ số>.mp4
  },

  // Minh Quân PRO 0.95 — cùng giọng kênh GIKIDI KPI (chủ dự án chọn 08/11/2026).
  giong: { nhaCungCap: "vbee", ma: "hn_male_minhquan_yt_24k-pre", tocDo: 0.95 },

  tenRieng: [],                 // không đọc tên riêng nào (VanBanPlus chỉ trên hình)

  xungHo: "anh chị",

  mau: {
    chinh: "#1d4ed8",           // xanh thương hiệu (ảnh kênh)
    chinhNhat: "#dbeafe",
    phu: "#f59e0b",             // vàng ngôi sao AI
    nenSang: "#f5f8ff",
  },

  // MÀN KẾT KIỂU THÔNG ĐIỆP: thẻ nguyên văn Nghị định 30 + ký tên. Mỗi clip tự đọc bằng canhKet([...]) + ket.the.
  ket: {
    kieu: "thong-diep",
    cau: [
      { loi: "Soạn đúng thể thức ngay từ đầu.", chu: "Soạn đúng thể thức ngay từ đầu." },
      { loi: "Nghị định ba mươi, năm hai nghìn hai mươi.", chu: "Nghị định 30/2020/NĐ-CP" },
    ],
    the: ["Soạn *đúng thể thức* ngay từ đầu.", "<b class=\"tt-dieu\">Nghị định 30/2020/NĐ-CP</b>"],
    nhan: "THỂ THỨC VĂN BẢN · NGUYÊN VĂN",
    nguon: "VanBanPlus · tải miễn phí: vanbanplus.giakiemso.com",   // ký tên trên hình (không đọc)
    loat: "",
    soPhan: 0,
    tren: 430,
  },

  lenh: "node D:/Thang/clip-kit/chay.mjs",
};
