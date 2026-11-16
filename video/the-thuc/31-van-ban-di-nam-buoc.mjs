// ============================================================================
// THỂ THỨC 31 — VĂN BẢN ĐI PHẢI QUA 5 BƯỚC NÀO, THEO THỨ TỰ NÀO? CẤP SỐ, ĐĂNG KÝ, ĐÓNG DẤU, PHÁT HÀNH, LƯU. THEO NĐ 30/2020
//
// Khuôn HỎI – ĐÁP CÓ HÀNG CHỈ DẪN (như clip 19): móc câu hỏi → 5 hàng hiện theo lời → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/noi-dung.md):
//   Điều 14: «Trình tự quản lý văn bản đi: 1. Cấp số, thời gian ban hành văn bản. 2. Đăng ký văn bản đi. 3. Nhân bản, đóng dấu cơ quan, tổ chức, dấu chỉ độ mật, mức độ khẩn
//   (đối với văn bản giấy); ký số của cơ quan, tổ chức (đối với văn bản điện tử). 4. Phát hành và theo dõi việc chuyển phát văn bản đi. 5. Lưu văn bản đi.»
//   Điều 15 khoản 2: «Đối với văn bản giấy, việc cấp số, thời gian ban hành được thực hiện sau khi có chữ ký của người có thẩm quyền, chậm nhất là trong ngày làm việc tiếp theo.»
//   Điều 16 khoản 2: «Văn bản được đăng ký bằng sổ hoặc bằng Hệ thống. … đăng ký bằng Hệ thống phải được in ra giấy đầy đủ các trường thông tin theo mẫu Sổ đăng ký văn bản đi, đóng sổ để quản lý.»
//   Điều 17 khoản 1 a): «Văn bản đi được nhân bản theo đúng số lượng được xác định ở phần nơi nhận của văn bản.» Điều 18 khoản 1: «… phát hành trong ngày văn bản đó được ký, chậm nhất là trong ngày làm việc tiếp theo.»
//   Điều 19 khoản 1 a): «Bản gốc văn bản được lưu tại Văn thư cơ quan và phải được đóng dấu ngay sau khi phát hành, sắp xếp theo thứ tự đăng ký.»
//   Điều 15 khoản 1: «Số và ký hiệu văn bản của cơ quan, tổ chức là duy nhất trong một năm, thống nhất giữa văn bản giấy và văn bản điện tử.»
//   ✔ mọi hàng trên hình / lời đọc bám sát đoạn nguyên văn trên; không bịa số, không thêm quy tắc
//   ✘ KHÔNG nói phần mềm tự làm đúng quy định này (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

const hang = (nhan, mau, chu, phu, hien, tren) =>
  the(`<div class="x-h"><span class="nh ${mau}">${nhan}</span><div class="tx">${chu}<small>${phu}</small></div></div>`,
    { tren, hien, lop: "x-the" });

export default {
  ten: "31-van-ban-di-nam-buoc",
  tieuDe: "Thể thức 31 · Văn bản đi phải qua 5 bước nào, theo thứ tự nào? Cấp số, đăng ký, đóng dấu, phát hành, lưu. Theo NĐ 30/2020",
  bia: { dong: ["Văn bản đi:","đúng 5 bước nào?"], vai: "Nghị định 30 · Điều 14–19" },
  ket: {
    the: [
      "«Số và ký hiệu văn bản của cơ quan, tổ chức là *duy nhất trong một năm, thống nhất giữa văn bản giấy và văn bản điện tử.*»",
      theNguon("Nghị định 30/2020/NĐ-CP · Điều 15 · khoản 1"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Văn bản đi phải qua mấy bước, và bước nào đứng trước?", chu: "Văn bản đi: *mấy bước?* Bước nào *đứng trước?*" },
      { loi: "Nghị định ba mươi xếp đúng năm bước.", chu: "Nghị định 30 xếp *đúng 5 bước*", nghiSau: 0.2 },
    ]},
    { canh: "ds", cau: [
      { loi: "Bước một, cấp số, văn bản giấy thì sau khi có chữ ký, chậm nhất ngày làm việc tiếp theo.", chu: "Bước *1* · *cấp số* — giấy: *sau khi có chữ ký*, chậm nhất ngày làm việc sau" },
      { loi: "Bước hai, đăng ký, bằng sổ, hoặc bằng Hệ thống, rồi phải in ra giấy, đóng sổ.", chu: "Bước *2* · *đăng ký*: sổ, hoặc Hệ thống → *in ra giấy, đóng sổ*" },
      { loi: "Bước ba, nhân bản đúng số lượng ở nơi nhận, đóng dấu, hoặc ký số của cơ quan.", chu: "Bước *3* · nhân bản *đúng số lượng nơi nhận* · *đóng dấu* hoặc *ký số*" },
      { loi: "Bước bốn, phát hành, trong ngày ký, chậm nhất ngày làm việc tiếp theo.", chu: "Bước *4* · *phát hành* trong ngày ký, chậm nhất ngày làm việc sau" },
      { loi: "Bước năm, lưu, bản gốc đóng dấu ngay sau phát hành, xếp theo thứ tự đăng ký.", chu: "Bước *5* · *lưu*: bản gốc *đóng dấu ngay* sau phát hành, xếp theo *số đăng ký*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Số, ký hiệu văn bản, là duy nhất trong một năm, thống nhất giữa văn bản giấy và văn bản điện tử.", chu: "Số, ký hiệu: *duy nhất trong một năm* · thống nhất *giấy và điện tử*" },
      nguonCau("Điều mười lăm", "Điều 15 · khoản 1"),
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
  ${mocKhai(`${NGU_CANH}Văn bản đi —<br>*mấy bước, bước nào trước?*`, { tren: 262, co: 66, gach: "c0k1" })}
  ${the(`<div class="x-q"><span>ĐÓNG DẤU</span><span>CẤP SỐ</span><span>ĐĂNG KÝ</span><i>?</i></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "ds": {
      khai: true,
      html: () => `
  ${chipKhai("📤 Văn bản đi · 5 bước")}
  ${hang("BƯỚC 1", "r", "Cấp số, thời gian ban hành", "giấy: sau chữ ký, chậm nhất ngày sau", "c0k1", 330)}
  ${hang("BƯỚC 2", "b", "Đăng ký văn bản đi", "sổ, hoặc Hệ thống rồi in ra giấy đóng sổ", "c1k1", 520)}
  ${hang("BƯỚC 3", "a", "Nhân bản, đóng dấu, ký số", "nhân bản đúng số lượng ở nơi nhận", "c2k1", 710)}
  ${hang("BƯỚC 4", "v", "Phát hành, theo dõi chuyển phát", "trong ngày ký, chậm nhất ngày sau", "c3k1", 900)}
  ${hang("BƯỚC 5", "g", "Lưu văn bản đi", "bản gốc đóng dấu ngay, xếp theo số", "c4k1", 1090)}`,
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
    ["02a-1", lucNode(tatCa, "ds", "c0k3w0")],
    ["02b-2", lucNode(tatCa, "ds", "c1k5w0")],
    ["02c-3", lucNode(tatCa, "ds", "c2k3w0")],
    ["02d-4", lucNode(tatCa, "ds", "c3k3w0")],
    ["02e-5", lucNode(tatCa, "ds", "c4k3w0")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k1"),
};
