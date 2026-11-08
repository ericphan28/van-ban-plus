// ============================================================================
// LOẠT «THỂ THỨC VĂN BẢN — NGHỊ ĐỊNH 30/2020» (video/the-thuc/) — mẹo trình bày văn bản hành chính cho cán bộ, công chức,
// văn thư. Mỗi clip MỘT quy định, soát nguyên văn ở docs/van-ban-phap-quy/nghi-dinh/30-2020-ND-CP/ (Phụ lục I).
//   · Xưng hô «anh chị». Giọng Minh Quân PRO 0.95 (du-an.mjs). Không đọc tên VanBanPlus — tên chỉ trên hình (cảnh «app»
//     + dòng ký tên màn kết). Hình vẽ HTML/CSS (tờ A4, dòng chữ) — không quốc huy, không tên cơ quan thật.
//   · Màn kết KIỂU THÔNG ĐIỆP: thẻ trích NGUYÊN VĂN (đoạn 1) + dòng nguồn «Nghị định 30/2020/NĐ-CP · Phụ lục I …» (đoạn 2).
//     Mỗi clip: canhKet([câu nguyên văn, nguonCau(…)]) + ket: { the: [nguyên văn, theNguon(…)] }.
//   · Hàm vẽ (so, thanh, dau, the, veCC) chép từ loạt KPI công chức (gikidi-kpi/video/cong-chuc/loat.mjs).
// ============================================================================

export default {};

/** dòng ngữ cảnh khung 0, đặt ĐẦU chữ của mocKhai */
export const NGU_CANH = '<b class="ngu-canh">📄 Thể thức văn bản · Nghị định 30</b><br>';

/** câu 2 màn kết: đọc nguồn. vd nguonCau("Phụ lục một", "Phụ lục I · mục II.4") */
export const nguonCau = (doc, chu) => ({
  loi: `Nghị định ba mươi, ${doc}.`,
  chu: `Nghị định 30/2020/NĐ-CP · ${chu}`,
});

/** đoạn 2 của thẻ màn kết (ket.the[1]) — khớp nguonCau */
export const theNguon = (chu) => `<b class="cc-dieu">Nghị định 30/2020/NĐ-CP<br>*${chu}*</b>`;

/** số chạy theo mốc (xem veCC) */
export const so = (khoa, { le = 0, truoc = "", sau = "", dai = 0.8, lop = "", style = "" } = {}) =>
  `<span class="${lop}" style="${style}" data-cc="so" data-khoa="${khoa}" data-le="${le}" data-truoc="${truoc}" data-sau="${sau}" data-dai="${dai}">${truoc}${Number(khoa.split("@")[1].split(">")[0])}${sau}</span>`;

/** CẢNH GIỚI THIỆU PHẦN MỀM (cuối mỗi clip, trước Tóm lại): thẻ VanBanPlus + 3 dòng tính năng đã soát */
export const theApp = (hien) => `<div class="tt-app" data-k="lop" data-hien="${hien}" style="opacity:0">
  <div class="dau"><i>AI</i><span>VanBanPlus</span></div>
  <ul><li>✍️ AI soạn bản nháp văn bản hành chính</li><li>🔎 AI kiểm tra lỗi chính tả, thể thức</li><li>📄 Xuất Word theo thể thức NĐ 30/2020</li></ul>
  <div class="tai">Tải miễn phí cho Windows<br><b>vanbanplus.giakiemso.com</b></div></div>`;

/** thanh ngang (rộng %) hoặc cột (chieu "cao", cao %) chạy theo `khoa` như `so` */
export const thanh = (khoa, { chieu = "rong", lop = "", dai = 0.8, style = "" } = {}) =>
  `<i class="${lop}" style="${style}" data-cc="${chieu}" data-khoa="${khoa}" data-dai="${dai}"></i>`;

/** dấu đóng xuống (to → vừa, hơi xoay) lúc `moc`, mờ đi lúc `an`; tự canh giữa ngang, đặt `tren` */
export const dau = (chu, moc, { tren, an = "", xoay = -7, lop = "", co = 92 } = {}) =>
  `<div class="cc-dau ${lop}" style="top:${tren}px;font-size:${co}px" data-cc="dau" data-moc="${moc}" data-an="${an}" data-xoay="${xoay}">${chu}</div>`;

/** thẻ trắng định vị tuyệt đối; `hien` = mờ vào (kit data-k="lop"), `rung` = mốc rung ngang */
export const the = (html, { trai = 70, tren, rong = 940, cao = "", hien = "", an = "", rung = "", lop = "", id = "" } = {}) =>
  `<div ${id ? `id="${id}" ` : ""}class="cc-the ${lop}"${hien ? ` data-k="lop" data-hien="${hien}"${an ? ` data-an="${an}"` : ""}` : ""}` +
  `${rung ? ` data-cc="rung" data-moc="${rung}"` : ""} style="left:${trai}px;top:${tren}px;width:${rong}px;${cao ? `height:${cao}px;` : ""}${hien ? "opacity:0" : ""}">${html}</div>`;

/**
 * PHIẾU CHẤM (bảng nhiều dòng): dau = [chữ trái, chữ phải] · dong = [{ ten, phu?, khoa (thanh + số %), do?: mốc dòng
 * đổi đỏ, doHet?: mốc hết đỏ, nhan?: html nhãn nổi trong dòng, hien?: mốc nhãn hiện }].
 * Trả về chuỗi HTML đặt trong the(…, { lop: "cc-phieu" }).
 */
export const phieu = ({ dau: [trai, phai], dong, le = 0 }) =>
  `<div class="cc-pdau"><span>${trai}</span><span>${phai}</span></div>` +
  dong.map((d) => `<div class="cc-dong"${d.do ? ` data-cc="lop" data-lop="do" data-moc="${d.do}"${d.doHet ? ` data-an="${d.doHet}"` : ""}` : ""}>
    <span class="ten">${d.ten}${d.phu ? `<small>${d.phu}</small>` : ""}</span>
    <div class="thanh">${thanh(d.khoa)}</div>${so(d.khoa, { le, sau: "%" , lop: "pt" })}
    ${d.nhan ? `<em class="cc-nhan" data-k="lop" data-hien="${d.hien}" style="opacity:0">${d.nhan}</em>` : ""}</div>`).join("");

/** hàng người (icon CSS, không người thật): n người, `vang` = số người tô vàng đầu hàng, `lop` thêm cỡ («nho») */
export const nguoi = (n, { vang = 0, lop = "", cot = 0 } = {}) =>
  `<div class="cc-nguoi${cot ? " luoi" : ""}"${cot ? ` style="grid-template-columns:repeat(${cot},auto)"` : ""}>` +
  Array.from({ length: n }, (_, i) => `<i class="cc-ng ${lop}${i < vang ? " vang" : ""}"></i>`).join("") + "</div>";

/** lưới 12 tháng (lịch / điểm từng tháng): o = [{ t: "T1", gt: "90", lop?: "do"|"vang"|"xam" }] (đủ 12 ô) */
export const lich12 = (o) =>
  `<div class="cc-lich">${o.map((x) => `<div class="o ${x.lop ?? ""}"><small>${x.t}</small><b>${x.gt ?? ""}</b></div>`).join("")}</div>`;

/**
 * VẼ CHUNG — gắn `ve: veCC` vào ĐÚNG MỘT cảnh mỗi clip (cảnh nào cũng được vẽ mỗi khung; thường là "moc").
 * Hàm bị kit NHÚNG BẰNG MÃ NGUỒN → chỉ dùng biến của trang (lucKhai, tien, eo, kep, dat), số liệu đọc từ data-*.
 *   data-cc="so"   chữ số theo data-khoa (định dạng vi-VN, data-le chữ số lẻ, data-truoc / data-sau)
 *   data-cc="rong" / "cao"   rộng / cao % theo data-khoa
 *   data-cc="dau"  dấu đóng xuống lúc data-moc, mờ lúc data-an (tuỳ), xoay data-xoay độ
 *   data-cc="lop"  thêm lớp data-lop từ data-moc (bỏ lại từ data-an nếu có)
 *   data-cc="rung" rung ngang 0,35 s từ data-moc
 */
export const veCC = (t) => {
  document.querySelectorAll("[data-cc]").forEach((el) => {
    const ten = el.closest(".canh").id.slice(2);
    const k = el.dataset.cc;
    if (k === "so" || k === "rong" || k === "cao") {
      const dai = Number(el.dataset.dai || 0.8);
      const ks = el.dataset.khoa.split(";").map((s) => {
        const [m, v] = s.split("@");
        const [a, b] = v.split(">").map(Number);
        return [lucKhai(m, ten), a, b];
      });
      let v = ks[0][1];
      ks.forEach(([l, a, b]) => { if (t >= l) v = a + (b - a) * eo(tien(t, l, l + dai)); });
      if (k === "so") {
        const le = Number(el.dataset.le || 0);
        el.textContent = el.dataset.truoc + v.toLocaleString("vi-VN", { minimumFractionDigits: le, maximumFractionDigits: le }) + el.dataset.sau;
      } else if (k === "rong") el.style.width = v + "%";
      else el.style.height = v + "%";
    } else if (k === "dau") {
      const luc = lucKhai(el.dataset.moc, ten);
      const an = el.dataset.an ? lucKhai(el.dataset.an, ten) : 1e9;
      const d = tien(t, luc - 0.05, luc + 0.22);
      dat(el, kep(d * 3) * (1 - tien(t, an - 0.25, an)), "rotate(" + el.dataset.xoay + "deg) scale(" + (1 + 1.3 * (1 - eo(d))) + ")");
    } else if (k === "lop") {
      const luc = lucKhai(el.dataset.moc, ten);
      const an = el.dataset.an ? lucKhai(el.dataset.an, ten) : 1e9;
      el.classList.toggle(el.dataset.lop, t >= luc && t < an);
    } else if (k === "rung") {
      const luc = lucKhai(el.dataset.moc, ten);
      const r = tien(t, luc, luc + 0.35);
      el.style.transform = r > 0 && r < 1 ? "translateX(" + (Math.sin(r * 28) * 9 * (1 - r)) + "px)" : "";
    }
  });
};

export const CSS_CC = `
.tt-dieu{display:block;font-size:40px;line-height:1.3;color:#334155}
.tt-to{position:absolute;background:#fff;box-shadow:0 22px 44px rgba(15,23,42,.2);border-radius:6px;font-family:"Times New Roman",serif;color:#111}
.tt-app{position:absolute;left:70px;top:330px;width:940px;box-sizing:border-box;background:#fff;border-radius:34px;padding:40px 46px;box-shadow:0 18px 34px rgba(15,23,42,.16)}
.tt-app .dau{display:flex;align-items:center;gap:22px;font-size:64px;font-weight:900;color:#0f172a}
.tt-app .dau i{font-style:normal;display:flex;align-items:center;justify-content:center;width:96px;height:118px;border-radius:14px;background:var(--mau-chinh);color:#fff;font-size:44px}
.tt-app ul{list-style:none;margin:30px 0 0;padding:0}
.tt-app li{font-size:40px;font-weight:700;line-height:1.25;padding:14px 0;border-top:2px solid #eef2ff;color:#1e293b}
.tt-app .tai{margin-top:26px;padding:22px;border-radius:22px;background:var(--mau-chinh-nhat, #dbeafe);text-align:center;font-size:36px;font-weight:800;color:#334155}
.tt-app .tai b{display:block;font-size:46px;color:var(--mau-chinh)}
.moc-khai b.ngu-canh{display:inline-block;font-size:40px;font-weight:800;letter-spacing:0;color:#334155;background:#fff;border:3px solid var(--mau-chinh);
  border-radius:999px;padding:8px 28px;margin-bottom:18px;box-shadow:0 8px 18px rgba(15,23,42,.08)}
.cc-the{position:absolute;box-sizing:border-box;background:#fff;border-radius:30px;box-shadow:0 18px 34px rgba(15,23,42,.16)}
.cc-phieu{padding:16px 30px 18px}
.cc-pdau{display:flex;justify-content:space-between;font-size:26px;font-weight:800;color:#64748b;letter-spacing:.05em;text-transform:uppercase;padding:6px 4px 10px}
.cc-dong{position:relative;display:flex;align-items:center;gap:20px;height:96px;padding:0 12px;margin:0 -12px;border-top:2px solid #f1f5f9;border-radius:16px}
.cc-dong .ten{flex:none;width:340px;font-size:33px;font-weight:700;line-height:1.1}
.cc-dong .ten small{display:block;margin-top:5px;font-size:23px;font-weight:700;color:#94a3b8}
.cc-dong .thanh{flex:1;height:20px;background:#f1f5f9;border-radius:99px;overflow:hidden}
.cc-dong .thanh i{display:block;height:100%;width:0;background:#16a34a;border-radius:99px}
.cc-dong .pt{flex:none;width:150px;text-align:right;font-size:46px;font-weight:900;color:#15803d;font-variant-numeric:tabular-nums}
.cc-dong.do{background:#fef2f2}
.cc-dong.do .ten small{color:#dc2626}
.cc-dong.do .thanh i{background:#dc2626}
.cc-dong.do .pt{color:#b91c1c}
.cc-nhan{position:absolute;right:178px;top:-20px;font-style:normal;font-size:30px;font-weight:900;color:#fff;background:#dc2626;padding:4px 16px;border-radius:999px;
  box-shadow:0 6px 14px rgba(220,38,38,.3)}
.cc-dau{position:absolute;left:0;right:0;margin:0 auto;width:max-content;border:12px solid #dc2626;color:#dc2626;border-radius:26px;padding:4px 40px;
  font-weight:900;white-space:nowrap;background:rgba(255,255,255,.94);box-shadow:0 14px 28px rgba(15,23,42,.18);opacity:0}
.cc-dau.xanh{border-color:#16a34a;color:#15803d}
.cc-trich{font-size:36px!important;line-height:1.42;font-style:italic;font-weight:600;color:#1e293b;border-left:12px solid var(--mau-chinh);
  flex-direction:column;align-items:flex-start!important}
.cc-trich span{display:block}
.cc-trich b{font-style:normal;font-weight:900;color:var(--mau-chinh);background:#fef3c7;padding:0 6px;border-radius:6px}
.cc-trich small{display:block;margin-top:12px;font-size:26px;font-style:normal;font-weight:700;color:#64748b}
.cc-nhan-lon{font-size:40px!important;font-weight:900;border-left:12px solid var(--mau-chinh)}
.cc-nhan-lon b{color:var(--mau-chinh)}
.cc-so-lon{position:absolute;left:0;width:100%;text-align:center;font-size:120px;font-weight:900;letter-spacing:-.03em;color:#0f172a;font-variant-numeric:tabular-nums}
.cc-so-lon small{display:block;font-size:38px;font-weight:800;letter-spacing:0;color:#64748b}
.cc-so-lon b{color:var(--mau-chinh)}
.cc-phan{position:absolute;left:70px;width:940px;text-align:center;font-weight:900;color:#0f172a}
.cc-phan .tu,.cc-phan .mau{font-size:62px;line-height:1.15}
.cc-phan .tu{color:#15803d}
.cc-phan hr{border:0;height:10px;border-radius:5px;background:#0f172a;margin:18px 80px}
.cc-phan small{display:block;margin-top:14px;font-size:30px;font-weight:700;color:#64748b}
.cc-cong{padding:26px 34px;font-size:48px;font-weight:800;line-height:1.3;text-align:center;color:#334155}
.cc-cong b{color:var(--mau-chinh);font-variant-numeric:tabular-nums}
.cc-cong small{display:block;font-size:28px;font-weight:700;color:#64748b}
.cc-khoi{position:absolute;left:70px;width:940px;display:flex;gap:34px;justify-content:center;align-items:flex-end}
.cc-khoi .k{position:relative;width:176px;height:300px;border-radius:22px;background:#f1f5f9;box-shadow:inset 0 0 0 4px #e2e8f0}
.cc-khoi .k i{position:absolute;left:0;bottom:0;width:100%;height:0;background:#16a34a;border-radius:0 0 22px 22px}
.cc-khoi .k.do i{background:#dc2626}
.cc-khoi .k.do b{color:#fff}
.cc-khoi .k b{position:absolute;left:0;top:96px;width:100%;text-align:center;font-size:52px;font-weight:900;color:#fff;text-shadow:0 2px 8px rgba(0,0,0,.25);font-variant-numeric:tabular-nums}
.cc-khoi .k small{position:absolute;left:0;bottom:-46px;width:100%;text-align:center;font-size:26px;font-weight:800;color:#64748b}
.cc-nguoi{display:flex;gap:26px;justify-content:center;align-items:flex-end}
.cc-nguoi.luoi{display:grid;gap:22px 30px;justify-content:center}
.cc-ng{position:relative;display:block;width:96px;height:150px;--m:#64748b}
.cc-ng::before{content:"";position:absolute;left:24px;top:0;width:48px;height:48px;border-radius:50%;background:var(--m)}
.cc-ng::after{content:"";position:absolute;left:8px;top:56px;width:80px;height:94px;border-radius:40px 40px 14px 14px;background:var(--m)}
.cc-ng.vang{--m:#ca8a04}
.cc-ng.nho{width:56px;height:88px}
.cc-ng.nho::before{left:14px;width:28px;height:28px}
.cc-ng.nho::after{left:5px;top:33px;width:46px;height:55px;border-radius:23px 23px 9px 9px}
.cc-lich{display:grid;grid-template-columns:repeat(4,1fr);gap:16px}
.cc-lich .o{height:118px;border-radius:20px;background:#f8fafc;border:3px solid #e2e8f0;display:flex;flex-direction:column;align-items:center;justify-content:center}
.cc-lich .o small{font-size:24px;font-weight:800;color:#94a3b8}
.cc-lich .o b{font-size:46px;font-weight:900;color:#0f172a;font-variant-numeric:tabular-nums}
.cc-lich .o.do{background:#fef2f2;border-color:#fecaca}.cc-lich .o.do b{color:#b91c1c}
.cc-lich .o.vang{background:#fefce8;border-color:#fde68a}
.cc-lich .o.xam{opacity:.45}
.cc-dieu{display:block;font-size:40px;line-height:1.3;color:#334155}
.cc-dieu span{color:var(--mau-chinh)}
.cc-xanh{color:#15803d}.cc-do{color:#b91c1c}.cc-xam{color:#64748b}
`;
