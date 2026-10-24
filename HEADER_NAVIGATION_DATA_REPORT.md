# HEADER NAVIGATION DATA REPORT — APEX / AUTOHUB

Interactive header navigation implementation report.
Scope: header + navigation + dropdown/mega-menu + routing behavior only.
All existing UI/UX structure preserved (logo position, header height 76px,
hero, cards, sections, footer, theme system, 3D-free stack untouched).

---

## 1. Current brands detected (canonical vehicle dataset)

Source of truth: `AutoHubDbContext` (SQL Server) seeded by `Data/AutoHubSeeder.cs`,
exposed via `IVehicleService.GetBrandItems()` (already filters `IsActive`
brands that have **at least one variant** and dedupes by name).

Detected brands — **15**, every one has ≥ 1 variant (total **32 variants**):

| Brand | Country | Variants |
|---|---|---|
| Audi | Germany | 3 |
| BMW | Germany | 5 |
| Ford | USA | 2 |
| Honda | Japan | 2 |
| Hyundai | South Korea | 1 |
| Kia | South Korea | 1 |
| Lexus | Japan | 2 |
| Mazda | Japan | 2 |
| Mercedes-Benz | Germany | 4 |
| Nissan | Japan | 1 |
| Porsche | Germany | 2 |
| Toyota | Japan | 3 |
| VinFast | Vietnam | 1 |
| Volkswagen | Germany | 1 |
| Volvo | Sweden | 2 |

## 2. Brands displayed in "Dòng xe"

**All 15 brands above** — rendered dynamically from `INavigationService`.
No brand is hard-coded; if the database changes, the menu changes.
Brands with zero vehicles would automatically disappear
(query condition: `Models.Any(m => m.Variants.Any())`).

## 3. Vehicle categories detected (body types with actual vehicles)

| Body type | Vehicles | Shown in menu |
|---|---|---|
| Sedan | 12 | ✔ (`/?q=Sedan`) |
| SUV | 17 | ✔ (`/?q=SUV`) |
| Coupe | 1 | ✔ (`/?q=Coupe`) |
| Hatchback | 1 | ✔ (`/?q=Hatchback`) |
| Pickup | 1 | ✔ (`/?q=Pickup`) |
| MPV | 0 | ✘ omitted (empty category) |
| Wagon | 0 | ✘ omitted (empty category) |
| Convertible | 0 | ✘ omitted (empty category) |

Counts displayed next to each entry are computed live from the DB.

## 4. Technology categories included

Only facets that exist as structured data in the dataset:

- **Nhiên liệu & động cơ**: Petrol ✔, Diesel ✔, Hybrid ✔, EV ✔
  (PHEV exists as a lookup row but has **0 vehicles** → omitted).
  EV uses the precise filter `/?flag=ev` (exact fuel match) to avoid
  false substring hits (e.g. "EV" inside "Everest").
- **Hệ thống dẫn động**: FWD ✔, RWD ✔, AWD ✔, 4WD ✔ (all have vehicles).
- **Khám phá thêm**: Press Signal news section (`/#press`),
  real specification sheet of a featured vehicle (`#specifications`).

No technical claims are made anywhere in the menus — entries are pure
navigation links backed by searchable data plus live counts.

## 5. Data sources used for verification

1. Canonical application database (`AUTOHUB`, SQL Server) — brands, models,
   variants, body/fuel/drivetrain lookups, flags (`IsFeatured`, `IsNewArrival`,
   `IsPopular`), specifications.
2. `AutoHubSeeder.cs` — seed definitions incl. official brand websites stored
   on brand entities (bmw.com, mercedes-benz.com, audi.com, toyota.com,
   honda.com, mazda.com, lexus.com, porsche.com, volvo.com, ford.com,
   vinfast.vn, hyundai.com, kia.com, volkswagen.com, nissan-global.com).
3. Live runtime smoke tests (HTTP 200 + content assertions) for every route
   listed in §8.

This task itself adds **zero** new automotive facts; it only navigates to
existing, already-published data.

## 6. Items removed / omitted because they could not be verified or lack data

| Item | Reason |
|---|---|
| "So sánh xe" (compare) | No comparison feature exists in project |
| "Xe hiệu năng" | No performance-tier facet in dataset |
| MPV / Wagon / Convertible / Sports-Car categories | Zero vehicles |
| PHEV category | Zero vehicles |
| Công nghệ an toàn / ADAS / Hỗ trợ lái | Not present anywhere in project data |
| Công nghệ pin / Công nghệ sạc / Quãng đường (as categories) | Only one EV variant exists; no dedicated content — covered by `Xe điện (EV)` link |
| Kết nối thông minh / AI trong ô tô | Would be an unverifiable claim |
| "BMW supports X"-style statements | Forbidden by task rules — none added |

**"Coming soon" mechanism (honest, non-clickable, badge “Sắp ra mắt”):**
Xem xe 3D · Xoay 360° · Video trải nghiệm (Trải nghiệm menu),
Câu hỏi thường gặp (Liên hệ menu). These features do not exist yet;
the UI does not pretend they do.

## 7. Duplicate items removed / prevented

- Brand normalization happens once in `VehicleService.GetBrandItems()`
  (group by trimmed name, `OrdinalIgnoreCase` → `BMW/bmw/Bmw` collapse).
- The menu renders from **one** source (`NavigationService`); no second
  brand list exists in HTML/JS/JSON.
- Desktop mega-menu and mobile accordion consume the same model
  (single `_HeaderNav.cshtml` partial, two render variants).

## 8. Routes used for each navigation item

| Menu | Entry | Route |
|---|---|---|
| Trang chủ | — | `/` (canonical home; never `history.back()`; smooth-scroll-to-top when already on clean `/`) |
| Dòng xe | Thương hiệu (15 chips w/ logos) | `/?brand={Name}#machines` |
| Dòng xe | Phân loại xe | `/?q={BodyType}#machines` |
| Dòng xe | Tất cả dòng xe | `/#machines` |
| Dòng xe | Xe nổi bật / Xe mới / Xe phổ biến / Xe điện | `/?flag=featured\|new\|popular\|ev#machines` |
| Trải nghiệm | Thẩm mỹ & khí động học | `/#experience` |
| Trải nghiệm | Cockpit & màn hình kỹ thuật số | `/#cockpit` (id added additively) |
| Trải nghiệm | Báo chí & Press Signal | `/#press` |
| Trải nghiệm | Bảng màu thực tế / Thông số cấu hình | `/Vehicles/{featured-slug}#colors` / `#specifications` |
| Trải nghiệm | 3D · 360° · Video | Coming soon (no route faked) |
| Công nghệ | Nhiên liệu & động cơ | `/?q=Petrol\|Diesel\|Hybrid#machines`, `/?flag=ev#machines` |
| Công nghệ | Hệ thống dẫn động | `/?q=FWD\|RWD\|AWD\|4WD#machines` |
| Công nghệ | Khám phá thêm | `/#press`, featured vehicle `#specifications` |
| Lái thử | Đăng ký lái thử | `/TestDrive` |
| Lái thử | Chọn mẫu xe / Ngày & khung giờ | `/TestDrive#chon-xe`, `/TestDrive#thoi-gian` (ids added additively) |
| Lái thử | Quy trình / Điều kiện lái thử | `/TestDrive#luu-y` (existing aside) |
| Lái thử | Showroom Hà Nội | `/Contact#showroom` |
| Liên hệ | Gửi tin nhắn về AUTOHUB | `/Contact` |
| Liên hệ | Đặt lịch tư vấn nhanh / Kênh liên hệ | Opens the **existing floating consultation hub** (`data-contact-hub-toggle`) — no second booking/contact system |
| Liên hệ | Thông tin showroom | `/Contact#showroom` (existing aside) |
| Liên hệ | FAQ | Coming soon |

On the homepage, `data-ajax-load` links reuse the site's existing AJAX
loader (`window.autohubLoadPage`) so brand/category/filter clicks swap the
vehicle grid without a full reload — same behavior as the existing brand
carousel. From any other page they perform normal navigation.

## 9. Interaction contract implemented

- Click top-level item → its panel opens; click again → closes;
  click another item → previous closes, new opens (one at a time).
- Outside click / backdrop click / `Escape` → close; navigating → close.
- Panels are absolutely positioned overlays (`top: calc(header bottom)`)
  with dimmed backdrop — **zero layout shift**, hero never moves.
- Active item: Signal-Red underline (existing visual language) + caret flip.
- Keyboard: Tab / Shift+Tab / Enter / Space / Escape / ArrowUp / ArrowDown,
  `aria-expanded` + `aria-haspopup` + `aria-controls` on triggers.
- Mobile (≤900px): tap → single-open accordion inside the **existing**
  `.mobile-nav`; drawer buttons still reuse the contact hub.
- Light/dark theme both styled; reduced-motion honored; viewport clamping
  prevents horizontal scroll at 1920→375px widths.

## 10. Files created / modified

**Created**
- `Services/NavigationService.cs` — centralized nav config (rule §20/§21)
- `Pages/Shared/_HeaderNav.cshtml` — single-source nav renderer (desktop + mobile)
- `wwwroot/css/header-nav.css` — overlay panels, backdrop, light theme, responsive
- `wwwroot/js/header-nav.js` — click menus, keyboard, accordion, drawer reuse
- `HEADER_NAVIGATION_DATA_REPORT.md` — this file

**Modified (minimal, additive)**
- `Program.cs` — register `INavigationService`
- `Services/VehicleService.cs` — optional `flag` filter (featured/new/popular/ev)
- `Pages/Index.cshtml(.cs)` — accept `flag` query param, persist in pagination URLs
- `Pages/Index.cshtml` — additive `id="cockpit"` anchor
- `Pages/TestDrive.cshtml` — additive anchors `#chon-xe`, `#thoi-gian`, `#luu-y`
- `Pages/Contact.cshtml` — additive anchor `#showroom`
- `Pages/Vehicles/Detail.cshtml` — additive anchor `#colors`
- `wwwroot/js/site.js` — expose existing loader as `window.autohubLoadPage`
- `Pages/Shared/_Layout.cshtml` — nav markup now rendered by `_HeaderNav`;
  logo, height, alignment, utilities unchanged
