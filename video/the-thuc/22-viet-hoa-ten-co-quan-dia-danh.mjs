// ============================================================================
// THỂ THỨC 22 — VIẾT HOA TÊN CƠ QUAN VÀ TÊN ĐỊA DANH: CHỮ NÀO HOA, CHỮ NÀO THƯỜNG? (Nghị định 30/2020/NĐ-CP, Phụ lục II, mục III và IV)
//
// Khuôn ĐỐ VUI CÓ ĐÁP ÁN NGUYÊN VĂN: móc «chữ nào hoa, chữ nào thường?» → 3 tên cơ quan (chữ hoa tô xanh) → tên địa danh (thành phố Thái Nguyên,
// Thủ đô Hà Nội, sông Vàm Cỏ…) → cảnh phần mềm + Công ty Gia Kiệm Số (BẮT BUỘC) → thẻ nguyên văn về tên cơ quan nước ngoài viết tắt.
//
// SOÁT với nguyên văn (docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/phu-luc/phu-luc-II.md dòng 28–46, 54–66):
//   III.1 a) «Tên đơn vị hành chính được cấu tạo giữa danh từ chung (…) với tên riêng của đơn vị hành chính đó: Viết hoa chữ cái đầu của các âm tiết
//   tạo thành tên riêng và không dùng gạch nối. Ví dụ: thành phố Thái Nguyên, tỉnh Nam Định,…» · c) «Trường hợp viết hoa đặc biệt: Thủ đô Hà Nội,
//   Thành phố Hồ Chí Minh.» · d) «… Trường hợp danh từ chung chỉ địa hình đi liền với danh từ riêng: Không viết hoa danh từ chung mà chỉ viết hoa danh
//   từ riêng. Ví dụ: biển Cửa Lò, chợ Bến Thành, sông Vàm Cỏ, vịnh Hạ Long,…»
//   IV.1 a) «Viết hoa chữ cái đầu của các từ, cụm từ chỉ loại hình cơ quan, tổ chức; chức năng, lĩnh vực hoạt động của cơ quan, tổ chức. Ví dụ: Ban Chỉ
//   đạo trung ương về Phòng chống tham nhũng, Văn phòng Chủ tịch nước, Bộ Tài nguyên và Môi trường, Tập đoàn Điện lực Việt Nam, Hội đồng nhân dân tỉnh
//   Sơn La, Sở Tài chính,…»
//   IV.2 b) «Tên cơ quan, tổ chức nước ngoài viết tắt: Viết bằng chữ in hoa như nguyên ngữ. Ví dụ: WTO, UNDP, UNESCO, ASEAN,…»
//   ✔ mọi ví dụ trên hình lấy NGUYÊN VĂN từ các ví dụ này (Sở Tài chính; Bộ Tài nguyên và Môi trường; Hội đồng nhân dân tỉnh Sơn La; thành phố Thái
//     Nguyên; tỉnh Nam Định; Thủ đô Hà Nội; Thành phố Hồ Chí Minh; sông Vàm Cỏ; vịnh Hạ Long; chợ Bến Thành; biển Cửa Lò) — không bịa thêm
//   ✔ «Hội đồng nhân dân tỉnh Sơn La — chỉ hoa Hội và Sơn La» đúng cách viết của ví dụ nguyên văn (không nói thêm quy tắc nào ngoài ví dụ)
//   ✘ KHÔNG nói phần mềm tự viết hoa đúng (chưa soát mã).
// ============================================================================
import { canhKet, lucNode, chipKhai, mocKhai } from "../kit.mjs";
import { CSS_CC, NGU_CANH, nguonCau, theNguon, the, veCC, theApp } from "./loat.mjs";

/** hàng: chữ có phần viết hoa tô xanh (dùng <i>…</i> cho chữ hoa), ghi chú nhỏ bên dưới */
const hang = (chu, ghi, hien, tren) =>
  the(`<div class="c22-h"><div class="d">${chu}</div><div class="g">${ghi}</div></div>`, { tren, hien, lop: "c22-the" });

export default {
  ten: "22-viet-hoa-ten-co-quan-dia-danh",
  tieuDe: "Thể thức 22 · Viết hoa tên cơ quan, tên địa danh: chữ nào hoa, chữ nào thường? Theo NĐ 30/2020",
  bia: { dong: ["Viết hoa tên cơ quan,", "địa danh: sao cho đúng?"], vai: "Nghị định 30 · Phụ lục II" },
  ket: {
    the: [
      "«Tên cơ quan, tổ chức nước ngoài viết tắt: *Viết bằng chữ in hoa như nguyên ngữ.* Ví dụ: *WTO, UNDP, UNESCO, ASEAN*,…»",
      theNguon("Phụ lục II · mục IV.2 · điểm b"),
    ],
  },

  kichBan: [
    { canh: "moc", cau: [
      { loi: "Viết hoa tên cơ quan, và tên địa danh, chữ nào hoa, chữ nào thường?", chu: "Tên cơ quan · tên địa danh — *chữ nào hoa, chữ nào thường?*" },
      { loi: "Nghị định ba mươi có ví dụ rất rõ.", chu: "Nghị định 30 có *ví dụ rất rõ*", nghiSau: 0.2 },
    ]},
    { canh: "co-quan", cau: [
      { loi: "Tên cơ quan, viết hoa chữ đầu của loại hình, chức năng, và lĩnh vực hoạt động.", chu: "Tên cơ quan: hoa *chữ đầu* của *loại hình* · *chức năng* · *lĩnh vực*" },
      { loi: "Hội đồng nhân dân tỉnh Sơn La, chỉ viết hoa chữ Hội, và tên riêng Sơn La.", chu: "*Hội* đồng nhân dân tỉnh *Sơn La* — chỉ hoa *Hội* và *tên riêng*" },
    ]},
    { canh: "dia-danh", cau: [
      { loi: "Thành phố, tỉnh, viết thường, chỉ viết hoa tên riêng, không dùng gạch nối.", chu: "*thành phố*, *tỉnh* viết thường · hoa *tên riêng* · *không gạch nối*" },
      { loi: "Thủ đô Hà Nội, và Thành phố Hồ Chí Minh, là hai trường hợp viết hoa đặc biệt.", chu: "*Thủ đô Hà Nội* · *Thành phố Hồ Chí Minh* — viết hoa *đặc biệt*" },
      { loi: "Sông, vịnh, chợ, đi liền tên riêng, thì không viết hoa, như sông Vàm Cỏ, vịnh Hạ Long.", chu: "sông · vịnh · chợ *đi liền tên riêng* → *không viết hoa*", nghiSau: 0.3 },
    ]},
    { canh: "app", cau: [
      { loi: "Soạn văn bản nhanh hơn, có phần mềm dùng trí tuệ nhân tạo của Công ty Gia Kiệm Số, tải miễn phí, đường dẫn trên màn hình.", chu: "Phần mềm AI soạn văn bản · *Công ty Gia Kiệm Số* · *tải miễn phí*", nghiSau: 0.4 },
    ]},
    canhKet([
      { loi: "Tên cơ quan nước ngoài viết tắt, viết bằng chữ in hoa như nguyên ngữ.", chu: "Tên cơ quan nước ngoài *viết tắt* → *chữ in hoa như nguyên ngữ*" },
      nguonCau("Phụ lục hai", "Phụ lục II · mục IV.2 · điểm b"),
    ]),
  ],

  css: CSS_CC + `
.c22-q{display:flex;flex-direction:column;gap:18px;padding:40px 30px 46px;align-items:center}
.c22-q div{font-size:56px;font-weight:700;color:#475569;font-family:"Times New Roman",serif}
.c22-q i{font-style:normal;font-family:system-ui,sans-serif;font-size:54px;font-weight:900;color:#b45309;background:#fef3c7;border-radius:14px;padding:0 20px;margin-left:12px}
.c22-the{padding:0}
.c22-h{padding:18px 30px}
.c22-h .d{font-family:"Times New Roman",serif;font-size:56px;font-weight:700;color:#111;line-height:1.15}
.c22-h .d i{font-style:normal;color:#15803d;background:#dcfce7;border-radius:8px;padding:0 4px}
.c22-h .g{margin-top:4px;font-size:30px;font-weight:800;color:#64748b}
`,

  canh: {
    "moc": {
      khai: true,
      html: () => `
  ${mocKhai(`${NGU_CANH}Viết hoa —<br>*chữ nào hoa, chữ nào thường?*`, { tren: 262, co: 66, gach: "c0k3" })}
  ${the(`<div class="c22-q"><div>sở tài chính<i>?</i></div><div>tỉnh nam định<i>?</i></div></div>`, { tren: 700, rung: "c1k0w1" })}`,
      ve: veCC,
    },

    "co-quan": {
      khai: true,
      html: () => `
  ${chipKhai("🏛️ Tên cơ quan, tổ chức")}
  ${hang("<i>S</i>ở <i>T</i>ài chính", "viết hoa chữ đầu của loại hình, chức năng", "c0k1", 330)}
  ${hang("<i>B</i>ộ <i>T</i>ài nguyên và <i>M</i>ôi trường", "viết hoa chữ đầu của loại hình, chức năng, lĩnh vực", "c0k3", 520)}
  ${hang("<i>H</i>ội đồng nhân dân tỉnh <i>S</i>ơn <i>L</i>a", "chỉ hoa chữ Hội và tên riêng", "c1k0", 710)}`,
    },

    "dia-danh": {
      khai: true,
      html: () => `
  ${chipKhai("🗺️ Tên địa danh")}
  ${hang("thành phố <i>T</i>hái <i>N</i>guyên<br>tỉnh <i>N</i>am <i>Đ</i>ịnh", "hoa tên riêng · không dùng gạch nối", "c0k3", 330)}
  ${hang("<i>T</i>hủ đô <i>H</i>à <i>N</i>ội<br><i>T</i>hành phố <i>H</i>ồ <i>C</i>hí <i>M</i>inh", "trường hợp viết hoa đặc biệt", "c1k0", 580)}
  ${hang("sông <i>V</i>àm <i>C</i>ỏ · vịnh <i>H</i>ạ <i>L</i>ong<br>chợ <i>B</i>ến <i>T</i>hành", "danh từ chung chỉ địa hình đi liền tên riêng: không viết hoa", "c2k5", 830)}`,
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
    ["02a-co-quan", lucNode(tatCa, "co-quan", "c0k3w3")],
    ["02b-hoi-dong", lucNode(tatCa, "co-quan", "c1k2w2")],
    ["03a-thanh-pho", lucNode(tatCa, "dia-danh", "c0k4w2")],
    ["03b-ha-noi", lucNode(tatCa, "dia-danh", "c1k2w4")],
    ["03c-song", lucNode(tatCa, "dia-danh", "c2k5w2")],
  ],

  anhBia: (tatCa) => lucNode(tatCa, "moc", "c0k3"),
};
