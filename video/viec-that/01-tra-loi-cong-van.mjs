// Clip 01 · Việc thật: 15:00 nhận công văn, 17:00 phải trả lời → AI soạn văn bản trả lời trên trình duyệt → tải Word.
// SOÁT (prod 22/11/2026, tài khoản demo-web-clip, quay bằng video/cong-cu/quay-web/quay.mjs 01 → video/anh/viec-that/web-01.*):
//  ✔ Công văn đến «845/SNNMT-MT» là giấy giả lập HƯ CẤU (tỉnh B, xã A; giay.mjs), hạn «trước 17 giờ 00 ngày 23/11/2026».
//  ✔ Soạn thảo AI › Bước 1/3 «Công văn» › Bước 2/3 các ô «Kính gửi (nơi nhận chính)», «Trích yếu (V/v)», «Nội dung chính»,
//    «Trả lời công văn số (nếu là công văn phúc đáp)» (loai-soan.ts); cơ quan + người ký điền sẵn từ Cài đặt.
//  ✔ Nút «Soạn bằng AI · 5–20 credit» → «AI đang xử lý» 11,4 s (tua nhanh) → «Đã trừ 6 credit · còn 18» — AI chạy thật.
//  ✔ Bản nháp: «Phúc đáp Công văn số 845/SNNMT-MT ngày 23/11/2026…», mục 1 Kết quả · 2 Khó khăn · 3 Đề xuất, đủ số 6,5 tấn,
//    92%, 2.140/2.320 hộ, 03 điểm, 02 xe, 180 hộ ấp 4–5, 01 xe nhỏ (web-01.ket-qua.json).
//  ✔ «Xem bố cục» → trang giấy; ghi chú nguyên văn «Khi xuất Word, phông, cỡ chữ, lề và khối chữ ký tự áp theo Nghị định 30/2020.»
//  ✔ «Xuất Word» tải .docx thật (web-01-cong-van.docx), 2 trang; ảnh trang = tệp đó dựng bằng LibreOffice (word-01-trang*.png).
//  · Bản nháp ghi «A, ngày … tháng … năm 2026» (địa danh thiếu chữ «Xã») — không phóng vùng đó, không nói về ngày tháng.
import { canhKet, canhTomTat, lucNode, chipKhai, dtKhai, sangKhai, chamKhai, goiKhai, cuaSoKhai, cuaSoVideoKhai, videoKhai, mocKhai, lopKhai } from "../kit.mjs";
import { NGU_CANH, CSS, WEB } from "./loat.mjs";

const A = (ten) => `viec-that/${ten}`;
const V = A("web-01.mp4");
const DT = { rong: 500, trai: 470, tren: 200, xoay: 0 };   // điện thoại lệch phải (chừa nhãn cảnh), cao ≈ 1104 → đáy 1304

/** điện thoại chạy bản quay theo `khoa` (+ phu thêm) */
const dtVideo = (id, khoa, u, phu = "") => dtKhai(id, { ...DT, lop: [{ anh: A("web-01-dau.png") }],
  phu: videoKhai(u(V), { id: `${id}-v`, khoa }) + phu }, u);
const zoom = (id, u, vung, tren, hien, an, khoa, phu = "") => cuaSoVideoKhai(id, u(V), WEB, vung, { trai: 65, tren, rong: 950 }, hien, an, { khoa }, phu);

const K_DIEN = [["s", 1.3], ["c0k0w1", 1.9], ["c0k1", 3.1], ["c0k2", 6.1], ["c0k3", 9.7], ["c0e", 18.1], ["c1", 18.2], ["c1e", 20.1]];   // gõ xong ~20,0; từ ~20,4 trang cuộn tới nút AI
const K_AI = [["s", 22.7], ["c0k0w1", 23.13], ["c0k1", 23.5], ["c0e", 34.6], ["c1", 36.4], ["c1e", 42.1]];
const K_WORD = [["s", 42.6], ["c0k0w1", 43.34], ["c0k1-0.2", 48.0], ["c0k1w2", 51.83], ["e", 54.6]];

export default {
  ten: "01-tra-loi-cong-van",
  tieuDe: "Việc thật · 1 · Trả lời công văn trước 17 giờ",
  bia: { dong: ["Nhận 15:00", "Trả lời 17:00?"], cat: [0.08, 0.55] },
  kichBan: [
    { canh: "moc", cau: [
      { loi: "Ba giờ chiều nhận công văn, năm giờ phải có văn bản trả lời.", chu: "15:00 nhận công văn — *17:00 phải trả lời*" },
      { loi: "Kịp không, nếu làm bằng AI ngay trên trình duyệt?", chu: "Làm bằng AI trên trình duyệt — *kịp không?*" },
    ]},
    { canh: "dien", cau: [
      { loi: "Chọn Công văn, gõ nơi nhận, trích yếu, và mấy con số của xã.", chu: "Công văn → nơi nhận · trích yếu · *số liệu của xã*" },
      { loi: "Ghi thêm số công văn đang trả lời.", chu: "Trả lời công văn số *845/SNNMT-MT*" },
    ]},
    { canh: "ai", cau: [
      { loi: "Chạm Soạn bằng AI, rồi chờ AI viết.", chu: "Chạm *Soạn bằng AI*" },
      { loi: "Bản nháp mở đầu bằng câu phúc đáp, chia ba mục, đủ số liệu đã gõ.", chu: "Phúc đáp CV 845 · *3 mục · đủ số liệu*" },
    ]},
    { canh: "word", cau: [
      { loi: "Xem bố cục, rồi xuất Word, phông chữ, lề, chữ ký, theo Nghị định ba mươi.", chu: "*Xuất Word* — phông, lề, chữ ký theo NĐ 30" },
    ]},
    { canh: "cam-ve", cau: [
      { loi: "Tệp Word hai trang, mở ra là sửa, in, rồi trình ký.", chu: "Tệp Word 2 trang — *sửa · in · trình ký*" },
    ]},
    canhTomTat("Trả lời công văn", [
      ["Gõ số liệu của xã", "Gõ số liệu của xã"],
      ["AI viết bản nháp phúc đáp", "AI viết bản phúc đáp"],
      ["Tải Word để trình ký", "Tải Word · trình ký"],
    ]),
    canhKet(),
  ],
  css: CSS,
  canh: {
    "moc": { khai: true, html: ({ u }) => `
      ${mocKhai(`${NGU_CANH()}15:00 nhận công văn,<br>*17:00 phải trả lời*`, { tren: 250, co: 70, gach: "c0k1w1" })}
      ${cuaSoKhai("cs-giay", u(A("giay-01-cv845-chup.jpg")), [1200, 1600], [0.05, 0.08, 0.95, 0.55], { trai: 90, tren: 560, rong: 900 }, "s-2", null,
        sangKhai([79, 79, 95, 84.6], "c0k1", "e") + sangKhai([15.5, 84.4, 38.5, 90], "c0k1", "e"))}
      ${goiKhai("🕒 15:00 · nhận", { trai: 60, tren: 1215, hien: "c0k0", tu: "trai" })}
      ${goiKhai("⏰ 17:00 · hạn trả lời", { trai: 520, tren: 1215, hien: "c0k1", tu: "phai" })}` },

    "dien": { khai: true, html: ({ u }) => `
      ${chipKhai("① Gõ số liệu của xã")}
      ${dtVideo("dt-dien", K_DIEN, u)}
      ${goiKhai("⏩ tua nhanh", { trai: 40, tren: 300, hien: "c0k1", an: "c1", tu: "trai", lop: "vt-tua" })}
      ${zoom("cs-noi-dung", u, [0.03, 0.58, 0.97, 0.82], 720, "c0k3+0.2", "c1", K_DIEN)}
      ${zoom("cs-tra-loi", u, [0.03, 0.79, 0.97, 0.90], 760, "c1k0+0.4", "c1e", K_DIEN)}` },

    "ai": { khai: true, html: ({ u }) => `
      ${chipKhai("② AI viết bản nháp")}
      ${dtVideo("dt-ai", K_AI, u)}
      ${goiKhai("⏩ tua nhanh · AI đang viết", { trai: 40, tren: 300, hien: "c0k1", an: "c0e+0.3", tu: "trai", lop: "vt-tua" })}
      ${zoom("cs-ban-nhap", u, [0.03, 0.25, 0.97, 0.70], 260, "c1k0+0.2", "e", K_AI)}` },

    "word": { khai: true, html: ({ u }) => `
      ${chipKhai("③ Xem bố cục · Xuất Word")}
      ${dtVideo("dt-word", K_WORD, u)}
      ${zoom("cs-bo-cuc", u, [0.03, 0.10, 0.97, 0.55], 260, "c0k0+0.6", "c0k1", K_WORD)}
      ${zoom("cs-ghi-chu", u, [0.02, 0.20, 0.98, 0.40], 300, "c0k2-0.2", "e", K_WORD)}` },

    "cam-ve": { khai: true, html: ({ u }) => `
      ${chipKhai("📄 Cầm về: tệp Word vừa tải")}
      ${lopKhai(`<img class="vt-giay" src="${u(A("word-01-trang2.png"))}" style="left:400px;top:330px;width:560px;transform:rotate(4deg)">`, "s", null, "left:0;top:0;width:100%;height:100%")}
      ${lopKhai(`<img class="vt-giay" src="${u(A("word-01-trang1.png"))}" style="left:110px;top:290px;width:640px;transform:rotate(-2deg)">`, "s+0.2", null, "left:0;top:0;width:100%;height:100%")}
      ${goiKhai("✓ 2 trang · sửa được ngay", { trai: 560, tren: 1130, hien: "c0k1", tu: "phai" })}` },
  },
  mocThu: (tatCa) => [
    ["02-noi-dung", lucNode(tatCa, "dien", "c0k3+1.2")], ["02-tra-loi", lucNode(tatCa, "dien", "c1k0+1.2")],
    ["03-tua", lucNode(tatCa, "ai", "c0k1+0.4")], ["04-ghi-chu", lucNode(tatCa, "word", "c0k3")],
  ],
  anhBia: (tatCa) => lucNode(tatCa, "moc", "c1e+0.3"),
};
