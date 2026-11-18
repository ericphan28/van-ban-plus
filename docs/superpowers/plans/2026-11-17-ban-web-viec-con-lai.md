# Bản web VanBanPlus — việc còn lại tới khi mở bán

Ngày 17/11/2026. Đọc cùng spec `docs/superpowers/specs/2026-11-17-ban-web-dot-a-design.md` và hợp đồng
`docs/superpowers/plans/2026-11-17-hop-dong-web-dot-a.md`. Tệp này là **danh sách việc chung** của bản web: phiên nào làm
xong một mục thì đánh dấu `[x]`, ghi ngày + commit ở cuối dòng, commit riêng tệp này. Không xóa mục; mục bỏ thì gạch và ghi
lý do.

## 1. Trạng thái

| Luồng | Việc | Trạng thái |
| --- | --- | --- |
| L1 | API phiên + đăng nhập | Prod 17/11 |
| L2 | os-kit lớp nối + khung `vanbanplus-web` (`https://app.vanbanplus.giakiemso.com`) | Prod 17/11 — còn mục 2.3 |
| L3 | API kho + dung lượng + phí | Prod 17/11 |
| L4 | Web văn bản, quét, 6 tác vụ AI, xuất Word | Prod 17/11 (web `03debc5`; lớp API dùng chung `a5126f9`) — thử prod 10/10 |
| L5 | Mẫu văn bản + tra cứu pháp quy | Prod 17/11 (API `54cda8e` + migration 012, web `205ff6b`) |
| L6 | Lịch họp + album ảnh | Prod 17/11 (013, API `a07e17e`, web `7b674d9`) |
| Mở bán | Mục 3.2, 4.1–4.10, 5.4–5.6 (kế hoạch `2026-11-18-web-mo-ban.md`) | Prod 18/11 (API `109b632` + `63c338a`, migration 015; web `3cde839` … `716d7c4`) |

L4, L5, L6 chạy song song được (cả ba cùng sửa `vanbanplus-web`: worktree riêng, chỉ sửa dòng phân hệ của mình trong
`src/ung-dung.ts`, rebase lên `origin/main` + chạy lại kiểm trước khi đẩy; nguồn Storage trong CSP `public/serve.json` do L4
thêm, L6 dùng chung).

**Xong L4–L6 = đủ chức năng đợt A, CHƯA mở bán.** Mở bán khi mục 2–4 dưới đây xong hết. **18/11/2026: phần kỹ thuật đã xong hết;
đủ điều kiện mở bán: 2.1, 2.2, 3.1, 3.2 xong 18/11; 3.4 hoãn (không chặn); 3.3 chỉ còn theo dõi.**

## 2. Chờ chủ dự án (làm được ngay, không phải chờ L4–L6)

- [x] 2.1 **Xong 18/11/2026 05:30:** chủ dự án đổi ý — tạo dự án Google Cloud riêng `vanbanplus` (771010985698, tài khoản ericphan28,
  không billing) theo khuôn chợ: màn đồng ý «VanBanPlus» External, In production (chưa xác minh — gmail.send là phạm vi nhạy cảm, trần
  100 người dùng phạm vi đó, chỉ hộp thư gửi dùng); client web `771010985698-9c0nrmlh…` (nguồn JS app.vanbanplus + localhost:5173) →
  `VITE_GOOGLE_CLIENT_ID` (build, web) + `VBP_GOOGLE_CLIENT_ID` (chạy, API). Kiểm prod: nút Google hiện, popup «Choose an account to
  continue to giakiemso.com», không lỗi nguồn (chưa đăng nhập trọn để khỏi tạo tài khoản). Khóa ở `~/.gikidi/vanbanplus-google.env`
  (PC + dev). Ghi chú cũ: **Google OAuth Client ID** (loại Web; nguồn JS `https://app.vanbanplus.giakiemso.com`, `http://localhost:5173`).
  **18/11/2026 (chủ dự án: dùng client của chogiakiem.vn):** client `565276165016-slmdmd1jhddbj58umengbs8dda17gskt` (dự án Google
  Cloud `chogiakiem-48`, «Web client 1»; nguồn hiện có chogiakiem.vn, www, localhost:3000). Còn thiếu: chủ dự án đăng nhập tài khoản
  sở hữu `chogiakiem-48` (KHÔNG phải ericphan28 — tài khoản đó không có quyền) → Google Auth Platform › Clients › Web client 1 ›
  Authorized JavaScript origins thêm `https://app.vanbanplus.giakiemso.com` và `http://localhost:5173` › Save; nên đặt tên ứng dụng ở
  Branding thành tên chung (vd «Gia Kiệm Số») vì hộp đăng nhập Google hiện tên này cho cả hai sản phẩm. Xong thì đặt
  `VITE_GOOGLE_CLIENT_ID` (biến build, app web) + biến client id phía API (L1) rồi triển khai lại hai app.
  Có rồi: đặt biến build `VITE_GOOGLE_CLIENT_ID` cho app Coolify `csywi7ct…` (vanbanplus-web) + biến tương ứng phía API
  (L1), triển khai lại. Chưa có → nút Google ẩn.
- [x] 2.2 **Xong 18/11/2026 05:34 (chủ dự án: gửi từ ericphan28@gmail.com):** client Desktop `771010985698-c4svhn1i…` + refresh token
  phạm vi gmail.send → `VBP_GMAIL_*` (chỉ lúc chạy, API); tên người gửi «VanBanPlus». Thử OTP `dang-nhap` tới chính hộp thư → thư tới
  Hộp thư đến. Gmail cá nhân giới hạn khoảng 500 thư / ngày. Nhắc email kho (`vbp-kho-nhac`) nay tự chạy. Ghi chú cũ: **Hộp thư Gmail gửi email** + cấp quyền Gmail API (`VBP_GMAIL_*`, hợp đồng mục 4). Chưa có → đăng ký, đăng nhập
  bằng mã email, quên mật khẩu, nhắc gia hạn / vượt dung lượng đều chưa chạy thật (API ghi log thay vì gửi).
- [x] 2.3 **Không còn chặn (18/11/2026):** các kịch bản kiểm prod tự đăng ký tài khoản thử qua `/api/auth/register` rồi dọn bằng SQL
  (`cong-cu/thu-xuyen-luong.mjs`); mật khẩu `vanbanplus-demo-clip` vẫn cũ — chỉ cần cho clip. Ghi chú cũ: **Tài khoản thử cho kiểm production**: `~/.gikidi/vanbanplus-demo-clip.txt` có mật khẩu cũ (cả route cũ lẫn v2 trả
  `SAI_DANG_NHAP`). Cần mật khẩu đúng hoặc đồng ý đặt lại. Có rồi: chạy `node .tam/kiem-prod.mjs` trong `vanbanplus-web`
  (đăng nhập → tải lại giữ phiên → hai tab → thiết bị → đăng xuất, chụp `.tam/chup-prod/`) — bước 6 Task 11 kế hoạch L2.
  Đừng thử quá 5 lần sai (khóa 15 phút).

## 3. Điều kiện mở bán (spec §11, spec 09/11 §13)

- [x] 3.1 **Xong 18/11/2026 (chủ dự án đồng ý nâng cấp):** tài khoản thanh toán `0186F9-DE33FD-0C63AD` (yenphipham83) đã nâng từ dùng
  thử lên **Paid account** trên console — giữ ₫6,45 triệu credit còn lại, hết credit mới trừ thẻ theo mức dùng. Thêm ngân sách «VanBanPlus
  AI - canh bao 2 trieu/thang» (chỉ cảnh báo, không chặn chi; ₫2.000.000/tháng; email ở 50 / 90 / 100 %; phạm vi cả tài khoản thanh
  toán, gồm cả worker viral dùng chung credit). Ghi chú trước khi nâng: 18/11: chủ dự án nhớ là đã có. Kiểm bằng service account: dự án
  Vertex đang bật thanh toán, tài khoản thanh toán `0186F9-DE33FD-…`; service account không xem được tài khoản đó là dùng thử hay trả
  phí → chủ dự án mở Billing › Overview của `yenphipham83`: còn dải «Free trial / Activate full account» là CHƯA nâng. Ghi chú cũ: — khóa hệ thống đang chạy bằng credit dùng thử, hết hạn là 6
  tác vụ AI dừng (bộ nhớ `vanbanplus-khoa-gemini`).
- [x] 3.2 **Xong 18/11/2026 (chủ dự án chọn tên miền riêng 17/11):** web gọi tác vụ AI chữ (`/api/v2/tac-vu/<ten>` + `/uoc-tinh`,
  trừ `trich-xuat`) qua `https://vanbanplus-api.app.giakiemso.com` (chỉ DNS, không Cloudflare), Bearer, không cookie; `trich-xuat`
  giữ Cloudflare (Traefik cắt đọc yêu cầu 60 giây khi mạng chậm). Tên đi thẳng không vào được → web tự lùi về Cloudflare cho hết
  lần tải trang; Kuma id 59; lùi hẳn bằng biến build `VITE_API_GOC_TAC_VU`. Route tác vụ có trần thân 2 MB / 30 MB (413). Đo prod
  17/11: 29 lượt, lâu nhất 48,8 giây — chưa có lượt thật > 100 giây để đo trên đường đi thẳng. Ghi chú cũ: **Giới hạn 100 giây của Cloudflare** cho tác vụ AI dài (soạn thảo, báo cáo định kỳ) — chốt cách xử lý (chạy nền +
  hỏi kết quả, hoặc tên miền API riêng không qua proxy) trước khi bán. Đo 17/11 (L4): người dùng **hủy** qua Cloudflare thì
  API nhận ngắt → `NGAT_KET_NOI`, hoàn credit (`credit_tru = 0`), nhưng mô hình vẫn chạy xong (chi phí Vertex vẫn phát sinh);
  trường hợp Cloudflare tự cắt 524 ở 100 giây chưa đo — web hỏi xác nhận trước khi chạy lại sau `QUA_GIO_CHO` / `MAT_MANG`.
- [ ] 3.6 **Trước 10/12/2026 — chọn đường trả tiền Vertex sau khi credit dùng thử hết hạn 18/12** (chủ dự án 18/11: phải chắt bóp,
  có nhiều thẻ → muốn mở tài khoản dùng thử mới bằng thẻ khác). Lúc đó đo lại mức chi thật sau mở bán rồi quyết. Nếu đi đường dùng thử
  mới: chủ dự án tự tạo tài khoản Google mới + tự nhập thẻ (Claude không nhập thẻ), KHÔNG dính `ericphan28` (nợ + dự án bị treo) hay
  `yenphipham83`; Claude làm phần còn lại (dự án, bật Vertex, service account, đổi `GOOGLE_VERTEX_SA_B64` trên Coolify, chuyển cả worker
  viral, kiểm). Rủi ro: Google khóa vì lạm dụng dùng thử → API tự lùi về khóa AI Studio miễn phí 5 lượt/phút. Đường hợp lệ khác:
  Google for Startups (gói Start ~2.000 USD credit), đổi model rẻ hơn / giảm token suy luận.
- [ ] 3.3 **Việc treo của đợt credit**: theo dõi `tac_vu_chay.ma_loi` bản cài 1.0.20; hệ số ước tính `bao-cao-dinh-ky` hơi cao
  (trừ thật 5 < min 7) — bộ nhớ `vanbanplus-credit-dot-1`.

- [ ] 3.4 **HOÃN (chủ dự án 18/11/2026: tính sau, hiện ưu tiên sao lưu DMS) — không chặn mở bán.** Sao lưu tệp kho ra ngoài máy: restic đêm (`may-chu/sao-luu/sao-luu`) chép cả
  `/data/coolify` gồm Storage VanBanPlus lên Cloudflare R2; gói miễn phí R2 10 GB **tính chung cả tài khoản** (đang báo ở 7 GB); ghi chú
  «Quyết định 10» trong `sao-luu` nói tệp Storage lớn phải loại khỏi R2. Bán dung lượng 2–50 GB / tài khoản là vượt sớm → chọn trả phí R2
  theo dung lượng hay đích sao lưu khác cho kho VanBanPlus. (18/11: kho chưa có tệp nào.)
- [x] 3.5 **Xong 18/11/2026 (chủ dự án: Business 50 MB quá lớn, về khoảng 10 MB):** migration `016_gioi_han_tep_goi` trên prod (API
  `7a10e22`) — Chuyên nghiệp 20 → 10, Doanh nghiệp 50 → 10 để giữ thứ bậc gói (5 / 10 / 10 / 10 MB). Ghi chú cũ: **Giới hạn tệp gói Business:** `subscription_plans.max_file_size_mb` = 50 nhưng `proxyClientMaxBodySize` 30 MB
  (base64) → trích xuất thực tế ≤ ~22 MB; web cắt ảnh quét ở 20 MB. Chọn: hạ gói về 20, hoặc nâng cả `proxyClientMaxBodySize` +
  `TRAN_THAN_TEP` (`src/lib/tac-vu/doc-than.ts`). Trang «Mua thêm» đã ghi «Nhập từ ảnh/PDF tối đa min(N, 20) MB».

## 4. Chưa luồng nào nhận — phiên «Mở bán bản web» làm sau L4–L6

Đề xuất mở một phiên riêng gom mục 4 + 5 khi L4, L5, L6 đã lên `main`.

- [x] 4.1 **Xong 18/11/2026 (web `2b4c560`):** `cong-cu/thu-xuyen-luong.mjs` trên prod — mật khẩu, 2 lớp TOTP, quét ảnh, soạn AI
  (đi thẳng), Xem bố cục, Word, tìm không dấu + Ctrl+K, hạn xử lý, chỉ đọc + mua (tạo đơn, QR), 390 WebKit; tự dọn. 34/34 (một lượt)
  + lỗi V/v nhiều dòng bị cắt đã sửa (`716d7c4`). **Còn: OTP email (chờ 2.2), Google (chờ 2.1)** — chạy lại kịch bản khi có.
  Ghi chú cũ: **Kiểm Playwright xuyên luồng** (spec §10): một mạch đăng nhập (4 cách) → quét ảnh tạo văn bản → soạn AI → xuất
  Word → tìm kiếm không dấu → vượt dung lượng chỉ đọc; chụp PC 1440 + điện thoại 390. Mỗi luồng chỉ kiểm phần của mình.
- [x] 4.2 **Xong 18/11/2026:** timer `canh-kho-vanbanplus` (gikidi-server `ae3da7c`, `43b05e2`) đo `du -sb` thư mục Storage mỗi giờ,
  báo Telegram (tối đa mỗi 24 giờ), độc lập với app; API vốn đã báo hằng ngày theo tổng byte trong CSDL.
  Ghi chú cũ: **Cảnh báo Storage VanBanPlus vượt 200 GB** (spec §9; máy chủ 1 TB theo hợp đồng Megacore) — gắn vào giám sát
  máy chủ (repo `D:/Thang/gikidi-server`), báo Telegram.
- [x] 4.3 **Xong 18/11/2026:** phân hệ «Mua thêm» `/mua` (gói tháng/năm, credit, dung lượng; đơn + QR dựng tại máy từ `don.qr`, hỏi
  trạng thái 5 → 30 giây, «Kiểm tra lại» khi quá hạn); API mở CORS credentials cho `/api/plans`, `/api/goi-nap`; liên kết mua cũ trỏ
  về `/mua`. Ghi chú cũ: **Thanh toán ngay trong web**: trang mua credit / gói / dung lượng còn ở site API (`/bang-gia`, `/thanh-toan`) dùng
  đăng nhập kiểu cũ → người dùng web phải đăng nhập lại. Cần trang mua trong web dùng phiên Bearer (`/api/orders*` đã nhận
  Bearer từ L3) hoặc chuyển phiên sang site API.
- [x] 4.4 **Xong 18/11/2026 (API `fe68ffb`…`109b632`); chủ dự án giao tự quyết:** giữ mức khắc phục kích hoạt 03 ngày làm việc, hoàn
  tiền trong 07 ngày làm việc, trần bồi thường bằng phí 03 tháng; không ghi địa chỉ trụ sở (chưa có nguồn chắc chắn — tên + MST + email
  hỗ trợ đã định danh). Nội dung đã đăng: mục «Kho trực tuyến và dung lượng», chỉ đọc khi
  vượt, không mã hóa thêm, nhân viên không xem văn bản, nơi lưu tại Việt Nam; bỏ mọi nhắc đồng bộ cloud cũ; không hứa email nhắc
  (chờ 2.2). Ghi chú cũ: **Điều khoản + Chính sách bảo mật**: thêm lưu văn bản trên cloud, phí dung lượng, chế độ chỉ đọc khi vượt, không
  mã hóa thêm, quản trị không xem văn bản khách (spec §2, §8).
- [x] 4.5 **Rà 18/11/2026 trên prod:** (1) đăng nhập — mật khẩu ✔, 2 lớp ✔, OTP email + Google chờ 2.1/2.2; (2) văn bản cloud — thư
  mục, tìm không dấu, tệp, Word ✔; (3) 6 tác vụ AI v2 + khoảng credit + mã lỗi ✔ (trích xuất, soạn thảo, tóm tắt chạy thật phiên này;
  kiểm tra / tham mưu / báo cáo theo lượt thử L4); (4) mẫu, pháp quy, lịch họp, album ✔ (lượt thử prod L5 / L6); (5) dung lượng + mua
  thêm + chỉ đọc ✔; (6) giao diện PC + điện thoại ✔, chế độ tối trong cửa sổ ✔. Ghi chú cũ: Rà tiêu chí hoàn thành đợt A (spec §1, 6 mục) trên production, ghi kết quả vào tệp này.
- [x] 4.6 **Xong 18/11/2026** (015 + API `f25e0d7`, web `f36ef40`). Ghi chú cũ: **API kho: xếp / lọc theo `han_xu_ly` (`han_tu`, `han_den`) và lọc độ khẩn** — thẻ «Hạn xử lý gần» trên màn hình chính
  và bộ lọc văn bản hiện chỉ xét 100 văn bản có ngày ban hành mới nhất; độ khẩn chưa lọc được (L4 ghi chú phạm vi trên thẻ).
- [x] 4.7 **Xong 18/11/2026** (web `1ddf171`, `d782039`; menu «Tìm mọi nơi…» cho điện thoại). Ghi chú cũ: **Tìm toàn cục `Ctrl+K`** (spec §7) qua văn bản, cuộc họp, mẫu, pháp quy — chưa luồng nào làm.
- [x] 4.8 **Xong 18/11/2026** (web `7d39052`, `7b885e4`; V/v nhiều dòng `716d7c4`). Ghi chú cũ: **«Xem bố cục» cho trang giấy soạn thảo / báo cáo** — khung soạn là chữ thuần nên phần đầu hai cột (cơ quan | quốc
  hiệu) trông lệch; bản Word xuất ra đúng NĐ 30. Đề xuất nút xem bố cục dựng từ `phanTichVanBan` (L4).
- [x] 4.9 **Xong 18/11/2026** (bảng `nguoi_dung_co_quan` + `/api/v2/toi/co-quan`, web `10e4db6`; chuyển bản cũ từ trình duyệt lên
  một lần). Ghi chú cũ: **«Cơ quan» (địa phương cho soạn thảo) lưu trên máy chủ** — L4 đang lưu trình duyệt theo tài khoản
  (`vbp-co-quan:<user_id>`); cần cột / route `toi` để dùng chung giữa máy và bản cài.
- [x] 4.10 **Xong 18/11/2026** (`khoa_tao`: 015 + API `f25e0d7`, web `35512d7`…`da87a02`). Ghi chú cũ: **Kiểm trùng sau mất mạng khi tạo văn bản** so `tao_luc` máy chủ với giờ máy khách, dung sai 5 giây — máy lệch giờ
  có thể sinh văn bản trùng; nên để API trả khóa chống trùng (idempotency key) cho `POST /kho/van-ban`.

## 5. Sửa còn tồn

- [x] 5.1 (L1, API) Đặt lại mật khẩu trên tài khoản bật 2 lớp: lần gửi đầu chưa có `ma_2_lop` vẫn tính vào trần đoán mã 2 lớp
  và ghi `hai_lop_sai`. **Xong 17/11/2026, API `a027cbf`:** thiếu `ma_2_lop` → `CAN_MA_2_LOP` không tính lượt, không ghi
  nhật ký; mã email vẫn phải đúng trước (không lộ trạng thái 2 lớp). Mã email vẫn bị dùng → web tự gửi mã mới như cũ.
- [x] 5.2 (L1, API) Mất phản hồi `lam-moi` giữa chừng → web đăng xuất / thu hồi phiên. **Xong 17/11/2026, migration
  `014_phien_cap_lai` (prod, trong giao dịch) + API `a027cbf`:** token vừa dùng < 30 giây mà chưa có token mới hơn được dùng thì
  cấp lại (tối đa 3 lần, nhật ký `token_cap_lai`); xoay bình thường gom các nhánh còn lại; > 30 giây vẫn thu hồi. Còn tồn
  (nhỏ, hiếm): trần 3 lần có thể vượt khi tải đồng thời vì `now()` là giờ bắt đầu giao dịch (dùng `clock_timestamp()`); đoạn
  hoàn tác nên `LOCK TABLE phien_token` trước. Web (`khach.ts`): sau `MAT_MANG` lúc làm mới nên thử lại ngay một lần để kịp
  cửa sổ 30 giây — việc của luồng web kế tiếp.
- [x] 5.3 (L1, API) Đăng nhập sai mật khẩu mất ~7 giây. **Đóng 17/11/2026, không tái hiện:** đo lại email thật và email giả
  đều ~1 giây (bằng nhau, không dò được email); 5 hash trong `users` đều bcrypt cost 10. Lần 7 giây là chậm nhất thời.
- [x] 5.4 **Xong 18/11/2026** (web `8fd4ef3`, `4e0b297`, `3cde839`; cả việc 5.2 phía web). Ghi chú cũ: (L2 → L4 dọn khi đụng tới) — `vanbanplus-web`:
  - cờ tự tải lại `?lam-moi=1` hết hạn theo đồng hồ 10 giây thay vì khi trang tải thành công (`src/vo/lam-moi.ts`);
  - React Router không thấy việc gỡ cờ (`history.replaceState` trực tiếp);
  - lóe «Có lỗi khi hiển thị trang» trước khi tự tải lại;
  - ~~timeout 15 giây áp cả lệnh ghi dữ liệu~~ **xong (L4):** lệnh ghi kho hạn 60 giây; tạo văn bản gặp `MAT_MANG` thì tìm văn
    bản vừa tạo trước khi cho lưu lại (lệnh ghi của L5/L6 tự đặt hạn 60 giây);
  - nút `LoiTai` lệch trái; thông báo xanh tương phản 3,1:1 (dưới AA);
  - `api.dangXuat` lỗi máy chủ thì cookie làm mới còn sống (máy dùng chung) — sửa trước khi bán: giữ token khi đăng xuất lỗi
    hoặc cảnh báo ở màn đăng nhập.
- [x] 5.5 **Xong 18/11/2026:** os-kit 0.5.0 (`b526940`, prop `cheDoToi`, thanh tab theo số mục), web cài + `cheDoToi="theo-may"`
  (`dd98e29`, `be10c96`). Ghi chú cũ: (os-kit) Chế độ tối bên trong cửa sổ phân hệ (kit chưa có — web đang giữ nền sáng trong cửa sổ); thanh tab điện thoại
  cứng 5 cột (web đang đè tạm trong `goc.css`). Sửa ở `D:/Thang/os-kit` theo luật kit, rồi cài lại.
- [x] 5.6 **Xong 18/11/2026:** gikidi-truong `8bb18e4` + kit 0.5.0: `pnpm check`, `test:giao-dien` 99/99, `xay-thu pnpm build` xanh.
  Ghi chú cũ: (os-kit) Chạy `next build` cho gikidi-truong qua `xay-thu` lần cài os-kit ≥ 0.4.0 kế tiếp (L2 mới kiểm `tsc` +
  `test:giao-dien`).

## 5b. Tồn nhỏ sau phiên «Mở bán» (không chặn bán; chi tiết ở sổ SDD `.superpowers/sdd/2026-11-18-web-mo-ban/progress.md`)

- Web: cờ lùi về Cloudflare bật cả khi lần thử lại cũng hỏng (tải lại trang là hết); ~~Quét để AI chọn «Đi» cho văn bản giấy~~ **xong 18/11/2026** (web `80b1cb0`: Quét mặc định «Đến», chỉ lấy «Đi»/«Nội bộ» của AI khi cơ quan ban hành khớp tên Cơ quan trong hồ sơ); lề «Kính gửi» trong «Xem bố cục» tính theo `em`; Ctrl+K: không khóa cuộn nền, không chọn sẵn dòng đầu; kit nên tự lo màu
  mục tab đang chọn ở chế độ tối (web đang đè).
- API: chưa có bài kiểm mức route cho 413 / tạo trùng 200 / co-quan; chạy lại 010 thì phải chạy lại 015.

## 6. Sau đợt A (đã chốt làm sau, không chặn mở bán)

- Đợt B: tổ chức thuê trên cùng kho (`chu_loai = 'to_chuc'`).
- Bản cài máy nối kho chung (đồng bộ theo `phien_ban`, luật gộp khi sửa chồng).
- App iOS / Android (Capacitor): API trả `token_lam_moi` trong thân, CORS `capacitor://localhost` đã có.

## 7. Sổ cập nhật

| Ngày | Phiên | Đổi gì |
| --- | --- | --- |
| 17/11/2026 | L2 | Bản đầu |
| 17/11/2026 | L2 (sửa L1) | 5.1, 5.2 xong (014 prod + API `a027cbf`); 5.3 đóng — không tái hiện |
| 17/11/2026 | L4 | L4 lên prod; thêm 4.6–4.10; 5.4 mục timeout lệnh ghi xong; 3.2 thêm kết quả đo hủy qua Cloudflare |
| 18/11/2026 | Mở bán | 3.2, 4.1–4.10, 5.4–5.6 xong (prod); thêm 3.4 (sao lưu R2), 3.5 (giới hạn tệp Business), §5b tồn nhỏ; 2.3 hết chặn |
| 18/11/2026 | Mở bán (quyết định chủ dự án) | 2.1 dùng client chợ (chờ thêm nguồn JS); 3.1 cần xác nhận trong Billing; 3.4 hoãn; 3.5 xong (016, 10 MB); 4.4 tự quyết |
| 18/11/2026 | Mở bán (Google + Gmail) | 2.1, 2.2 xong: dự án GCP `vanbanplus`, đăng nhập Google + OTP email chạy thật trên prod |
| 18/11/2026 | Mở bán (Vertex) | 3.1 xong: tài khoản thanh toán Vertex nâng lên trả phí + ngân sách cảnh báo ₫2 triệu/tháng |
