// node video/cong-cu/thuong-hieu/chup.mjs [tên…] — chụp HTML thương hiệu ra PNG bằng Edge (playwright-core của clip-kit)
// → video/thuong-hieu/<tên>.png. Phông Be Vietnam Pro lấy từ D:/Thang/clip-kit/phong.
import { chromium } from "file:///D:/Thang/clip-kit/node_modules/playwright-core/index.mjs";
import { pathToFileURL, fileURLToPath } from "node:url";
import path from "node:path";
const D = path.dirname(fileURLToPath(import.meta.url));
const viec = [["avatar", 1080, 1080], ["banner-youtube", 2560, 1440], ["anh-bia-facebook", 1640, 624]];
const chon = process.argv.slice(2);
const b = await chromium.launch({ channel: "msedge", headless: true });
for (const [ten, w, h] of viec) {
  if (chon.length && !chon.includes(ten)) continue;
  const p = await b.newPage({ viewport: { width: w, height: h }, deviceScaleFactor: 1 });
  await p.goto(pathToFileURL(path.join(D, ten + ".html")).href);
  await p.evaluate(() => document.fonts.ready);
  await p.waitForTimeout(300);
  await p.screenshot({ path: path.join(D, "..", "..", "thuong-hieu", ten + ".png") });
  console.log("✓", ten);
  await p.close();
}
await b.close();
