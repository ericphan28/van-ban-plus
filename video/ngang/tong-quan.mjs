// ============================================================================
// VIDEO NGANG «GIỚI THIỆU TỔNG QUAN» VanBanPlus — chiếu TV gian hàng chuyển đổi số GIKIDI, Ngày hội xã Gia Kiệm 21/11/2026
// (người xem: cán bộ xã, đoàn thể, lãnh đạo đi ngang). Ghép 8 clip dọc loạt the-thuc + lời thuyết minh riêng bản ngang.
//   node /D:/Thang/clip-kit/ghep-ngang.mjs video/ngang/tong-quan.mjs [--thu]
//
// Vì sao chọn các clip này: kho chỉ có loạt the-thuc (mẹo Nghị định 30, mỗi clip có cảnh thẻ VanBanPlus tĩnh), CHƯA có
// clip nào quay phần mềm chạy thật (loạt lam-that mới có một bản quay thô, chưa thành clip). Nên phần mềm được giới thiệu
// bằng lời dẫn mở đầu / chương / kết; clip gom theo nhóm tính năng mà quy định của clip gắn tới.
// Tên sản phẩm: «VanBanPlus» (video/du-an.mjs) — chỉ trên hình, KHÔNG đọc (NOI-DUNG.md luật 4).
//
// SOÁT lời dẫn (19/11/2026) với video/NOI-DUNG.md mục 4, PROJECT_STATUS.md mục A và mã:
//   ✔ soạn bản nháp bằng AI từ mẫu, 41 mẫu — NOI-DUNG mục 4; TemplateSeeder, AIComposeDialog («Sử dụng mẫu này» → AI Soạn thảo)
//   ✔ kiểm tra lỗi chính tả, thể thức — NOI-DUNG mục 4 (DocumentReviewService). KHÔNG nói AI bắt được lỗi cụ thể của clip.
//   ✔ xuất tệp văn bản theo thể thức Nghị định 30 — NOI-DUNG mục 4 («xuất Word thể thức NĐ 30/2020»)
//   ✔ tóm tắt văn bản dài — NOI-DUNG mục 4 (DocumentSummaryDialog)
//   ✔ quét ảnh văn bản giấy lấy chữ — NOI-DUNG mục 4 (ScanImportDialog, OCR)
//   ✔ theo dõi văn bản đến, đi — NOI-DUNG mục 1; sổ tay CÁ NHÂN, không nói là sổ đăng ký chính thức (PROJECT_STATUS «triết lý»)
//   ✔ số đến tự tăng theo năm — DocumentService.GetNextArrivalNumber (lam-that/KE-HOACH clip 06 đã soát)
//   ✔ nhắc văn bản sắp đến hạn (3 ngày) / quá hạn khi mở phần mềm — MainWindow.CheckOverdueOnStartupAsync
//   ✔ xuất biên bản cuộc họp — MeetingListPage menu «📝 Xuất Biên bản cuộc họp»
//   ✔ báo cáo định kỳ tuần / tháng / quý / năm bằng AI — PeriodicReportDialog, PeriodicReportService.GetPeriodTypes
//   ✔ tạo bản sao y, sao lục, trích sao — CopyDocumentDialog (DocumentListPage)
//   ✔ tải miễn phí, vanbanplus.giakiemso.com — du-an.mjs ket.nguon, NOI-DUNG mục 1
//   ✘ không giá, không số người dùng, không «tốt nhất / duy nhất / 100%», không «tiết kiệm giờ», chữ Latin chỉ trên thẻ
// ============================================================================
export default {
  ten: "tong-quan",
  tieuDe: "VanBanPlus",
  phuDe: "Giới thiệu tổng quan",
  nhanMuc: "Mục",
  biaLon: "Trợ lý <em>soạn văn bản</em> hành chính",
  biaThe: ["Soạn từ mẫu", "Kiểm tra lỗi", "Văn bản đến, đi", "Họp, báo cáo"],
  ket: { nhan: "Tải miễn phí", tieuDe: "cho Windows", url: "vanbanplus.giakiemso.com" },

  moDau: {
    nhan: "VanBanPlus · Công ty Gia Kiệm Số",
    tieuDe: "Trợ lý soạn văn bản hành chính",
    y: [
      "AI soạn nháp từ 41 mẫu, kiểm tra lỗi",
      "Xuất Word theo thể thức NĐ 30/2020",
      "Văn bản đến – đi · họp · báo cáo định kỳ",
    ],
    loi: "Phần mềm soạn văn bản hành chính bằng trí tuệ nhân tạo, của Công ty Gia Kiệm Số, cài trên máy tính của từng cán bộ. "
      + "Chọn một trong bốn mươi mốt mẫu, trí tuệ nhân tạo soạn bản nháp, kiểm tra lỗi, rồi xuất tệp văn bản theo thể thức Nghị định ba mươi. "
      + "Phần mềm còn theo dõi văn bản đến, văn bản đi, tóm tắt văn bản dài, xuất biên bản cuộc họp, và viết báo cáo định kỳ. "
      + "Mỗi phần kèm vài quy định đáng nhớ, trong Nghị định ba mươi.",
  },

  phan: [
    {
      ten: "Soạn văn bản từ mẫu",
      loi: "Phần một, soạn văn bản từ mẫu. Có sẵn bốn mươi mốt mẫu, trí tuệ nhân tạo viết bản nháp, để anh chị đọc lại, chỉnh sửa, rồi xuất ra tệp văn bản.",
      clip: [
        { ma: "the-thuc/30", nhan: "Chín thành phần thể thức", hoi: "Thể thức văn bản gồm mấy thành phần chính?",
          y: ["9 thành phần chính", "Từ Quốc hiệu đến Nơi nhận", "Thêm 4 nhóm thành phần bổ sung"] },
        { ma: "the-thuc/04", nhan: "Tên loại, trích yếu, đường kẻ", hoi: "Đường kẻ dưới trích yếu dài bao nhiêu?",
          y: ["Tên loại: IN HOA, đậm", "Trích yếu: chữ thường, đậm", "Kẻ 1/3 đến 1/2 dòng chữ"] },
        { ma: "the-thuc/02", nhan: "Căn lề trang A4", hoi: "Lề trái văn bản rộng bao nhiêu?",
          y: ["Trái 30–35 mm", "Phải 15–20 mm", "Trên, dưới 20–25 mm"] },
      ],
    },
    {
      ten: "Kiểm tra trước khi trình",
      loi: "Phần hai, kiểm tra trước khi trình ký. Trí tuệ nhân tạo đọc lại văn bản, chỉ ra lỗi chính tả, lỗi thể thức, để anh chị sửa.",
      clip: [
        { ma: "the-thuc/07", nhan: "Viết tắt và dòng «Số:»", hoi: "Viết tắt loại văn bản nào có chữ thường?",
          y: ["Tờ trình TTr · Chương trình CTr", "Quy định QyĐ khác Quyết định QĐ", "Số chạy lại từ 01 mỗi năm"] },
        { ma: "the-thuc/01", nhan: "Ngày 05, tháng 02", hoi: "Ngày 5 tháng 3 ghi thế nào cho đúng?",
          y: ["Ngày dưới 10: thêm số 0", "Chỉ tháng 1, 2 thêm số 0", "Tháng 3 đến 12 giữ nguyên"] },
      ],
    },
    {
      ten: "Văn bản đến, văn bản đi",
      loi: "Phần ba, theo dõi văn bản đến, văn bản đi. Quét ảnh văn bản giấy để lấy chữ, số đến tự tăng theo năm, văn bản sắp đến hạn hay quá hạn, được nhắc ngay khi mở phần mềm.",
      clip: [
        { ma: "the-thuc/19", nhan: "Văn bản đến: đăng ký, trình", hoi: "Văn bản đến chưa đăng ký có phải giải quyết?",
          y: ["Chưa đăng ký → không có trách nhiệm giải quyết", "Trình trong ngày, chậm nhất ngày làm việc sau", "Khẩn: trình, chuyển giao ngay"] },
        { ma: "the-thuc/31", nhan: "Văn bản đi: năm bước", hoi: "Văn bản đi phải qua mấy bước?",
          y: ["Cấp số → đăng ký → đóng dấu → phát hành → lưu", "Phát hành trong ngày ký", "Số, ký hiệu duy nhất trong một năm"] },
      ],
    },
    {
      ten: "Bản sao, họp, báo cáo",
      loi: "Phần cuối, bản sao, cuộc họp và báo cáo. Phần mềm tạo bản sao y, sao lục, trích sao, xuất biên bản cuộc họp, và để trí tuệ nhân tạo viết báo cáo định kỳ, theo tuần, tháng, quý, năm.",
      clip: [
        { ma: "the-thuc/20", nhan: "Sao y, sao lục, trích sao", hoi: "Sao y, sao lục, trích sao khác nhau thế nào?",
          y: ["Sao y: từ bản gốc hoặc bản chính", "Sao lục: từ bản sao y", "Trích sao: một phần nội dung"] },
      ],
    },
  ],

  ketThuc: {
    nhan: "Tải miễn phí",
    tieuDe: "Cài trên máy tính Windows",
    y: ["vanbanplus.giakiemso.com", "Hỏi cài thử ngay tại gian hàng", "Sản phẩm của Công ty Gia Kiệm Số"],
    loi: "Phần mềm tải miễn phí, cài trên máy tính, địa chỉ tải trên màn hình. Muốn cài thử, anh chị hỏi ngay tại gian hàng.",
  },
};
