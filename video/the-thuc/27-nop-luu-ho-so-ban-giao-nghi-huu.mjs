// ============================================================================
// THỂ THỨC 27 — NỘP LƯU HỒ SƠ VÀO LƯU TRỮ CƠ QUAN: HẠN BAO LÂU? NGHỈ HƯU, CHUYỂN CÔNG TÁC BÀN GIAO GÌ? NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 30 khoản 2: «a) Đối với hồ sơ, tài liệu xây dựng cơ bản: Trong thời hạn 03 tháng kể từ ngày công trình được quyết toán. b) Đối với hồ sơ, tài liệu khác: Trong thời hạn 01
//   năm kể từ ngày công việc kết thúc.» Khoản 3 a): «Khi nộp lưu tài liệu phải lập 02 bản "Mục lục hồ sơ, tài liệu nộp lưu" và 02 bản "Biên bản giao nhận hồ sơ, tài liệu" theo mẫu tại Phụ lục V …
//   Đơn vị, cá nhân nộp lưu tài liệu và Lưu trữ cơ quan giữ mỗi loại 01 bản.» Điều 31 khoản 3: «c) Đơn vị và cá nhân … có trách nhiệm nộp lưu những hồ sơ, tài liệu được xác định thời hạn bảo quản
//   từ 05 năm trở lên vào Lưu trữ cơ quan. d) Trường hợp đơn vị hoặc cá nhân có nhu cầu giữ lại hồ sơ, tài liệu đã đến hạn nộp lưu để phục vụ công việc thì phải được người đứng đầu cơ
//   quan, tổ chức đồng ý bằng văn bản và phải lập Danh mục hồ sơ, tài liệu giữ lại gửi Lưu trữ cơ quan. Thời hạn giữ lại … không quá 02 năm kể từ ngày đến hạn nộp lưu. đ) Cán bộ, công chức,
//   viên chức và người lao động … trước khi nghỉ hưu, thôi việc, chuyển công tác, đi học tập dài ngày phải bàn giao toàn bộ hồ sơ, tài liệu hình thành trong quá trình công tác cho đơn vị,
//   Lưu trữ cơ quan theo quy chế của cơ quan, tổ chức.»
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "27-nop-luu-ho-so-ban-giao-nghi-huu",
  tieuDe: "Thể thức 27 · Nộp lưu hồ sơ vào Lưu trữ cơ quan: hạn bao lâu? Nghỉ hưu, chuyển công tác bàn giao gì? NĐ 30/2020",
  bia: { dong: ["Nộp lưu hồ sơ:","hạn bao lâu?"], vai: "Nghị định 30 · Điều 30, 31" },
  ket: {
    the: [
      "«Cán bộ, công chức, viên chức và người lao động … trước khi *nghỉ hưu, thôi việc, chuyển công tác, đi học tập dài ngày* phải *bàn giao toàn bộ hồ sơ, tài liệu* hình thành trong quá trình công tác.»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 31 · khoản 3 · điểm đ"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Hồ sơ công việc, bao lâu phải nộp lưu, và nghỉ hưu phải bàn giao gì?", chu: "Hồ sơ công việc — *bao lâu phải nộp lưu?* — nghỉ hưu *bàn giao gì?*" },
      { loi: "Nghị định ba mươi có thời hạn cụ thể.", chu: "Nghị định 30 có *thời hạn cụ thể*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Hồ sơ xây dựng cơ bản, nộp lưu trong ba tháng, kể từ ngày công trình được quyết toán.", chu: "*Xây dựng cơ bản*: *3 tháng* từ ngày quyết toán" },
      { loi: "Hồ sơ, tài liệu khác, nộp lưu trong một năm, kể từ ngày công việc kết thúc.", chu: "Hồ sơ khác: *1 năm* từ ngày công việc kết thúc" },
      { loi: "Bảo quản từ năm năm trở lên, thì phải nộp lưu vào Lưu trữ cơ quan.", chu: "Bảo quản *từ 5 năm* → *phải nộp lưu*" },
      { loi: "Nộp lưu hồ sơ giấy, lập hai bản mục lục, và hai bản biên bản giao nhận.", chu: "Hồ sơ giấy: *2 bản mục lục* · *2 bản biên bản giao nhận*" },
      { loi: "Muốn giữ lại hồ sơ đã đến hạn, phải được người đứng đầu đồng ý bằng văn bản, không quá hai năm.", chu: "Giữ lại hồ sơ đến hạn: *văn bản đồng ý* · *không quá 2 năm*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Nghỉ hưu, thôi việc, chuyển công tác, hoặc đi học dài ngày, phải bàn giao toàn bộ hồ sơ, tài liệu.", chu: "*Nghỉ hưu* · thôi việc · chuyển công tác · đi học dài ngày → *bàn giao toàn bộ hồ sơ*" },
      nguonCau("Điều ba mươi mốt", "Điều 31 · khoản 3 · điểm đ"),
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
  ${mocKhai(`${NGU_CANH}Nộp lưu hồ sơ —<br>*hạn bao lâu, bàn giao gì?*`, { tren: 262, co: 66, gach: "c0k2" })}
  ${the(`<div class="x-q"><span>NỘP LƯU</span><span>BAO LÂU?</span><span>NGHỈ HƯU</span></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("🏛️ Nộp lưu Lưu trữ cơ quan")}
  ${hang("XÂY DỰNG CƠ BẢN", "r", "3 tháng", "kể từ ngày công trình được quyết toán", "c0k1", 330)}
  ${hang("HỒ SƠ KHÁC", "b", "1 năm", "kể từ ngày công việc kết thúc", "c1k1", 520)}
  ${hang("TỪ 5 NĂM", "a", "Bảo quản từ 5 năm trở lên", "phải nộp lưu vào Lưu trữ cơ quan", "c2k1", 710)}
  ${hang("HỒ SƠ GIẤY", "v", "2 bản mục lục + 2 bản biên bản", "đơn vị nộp và Lưu trữ giữ mỗi loại 1 bản", "c3k1", 900)}
  ${hang("GIỮ LẠI", "g", "Không quá 2 năm", "người đứng đầu đồng ý bằng văn bản", "c4k1", 1090)}`,
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
    ["02a-1", lucNode(tatCa, "ds", "c0k2w0")],
    ["02b-2", lucNode(tatCa, "ds", "c1k3w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k1w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k2w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k2w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k2"),
};
