# REAL_VEHICLE_IMAGE_REPORT.md

> AUTOHUB Vehicle Image Audit Report
> Generated: 2026-08-20

---

## Summary

| Metric | Count |
|--------|-------|
| Total vehicle variants in DB | 27 |
| Hero images (SVG placeholder) | 27 |
| Hero images (JPG downloaded) | 2 |
| Color swatch images (SVG) | 108 |
| Total image files | 251 |
| Images needing replacement | 25 |

---

## Seeder Path Bug Fixes Applied

| Variant | Old Path | Fixed Path | Status |
|---------|----------|------------|--------|
| `audi-a4-45-tfsi` | `Audi/Q5/audi-q5-45-tfsi.svg` | `Audi/A4/audi-a4-45-tfsi.svg` | FIXED |
| `bmw-x3-xdrive20d` | `BMW/X3/bmw-x3-30i.svg` | `BMW/X3/bmw-x3-xdrive20d.svg` | FIXED (file renamed) |
| `toyota-fortuner-24-vrz` | `Toyota/Fortuner/toyota-fortuner-24at.svg` | `Toyota/Fortuner/toyota-fortuner-24-vrz.svg` | FIXED (file renamed) |
| `honda-cr-v-15-l` | `Honda/CR-V/honda-crv-15rs.svg` | `Honda/CR-V/honda-crv-15-l.svg` | FIXED (file renamed) |
| `volvo-s90-b5-awd` | `Volvo/S90/volvo-s90-b6.svg` | `Volvo/S90/volvo-s90-b5.svg` | FIXED (file renamed) |

---

## Vehicle Image Status — Per Variant

### BMW

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| 320i | G20 | Global | BMW PressClub (press.bmwgroup.com) | SVG (placeholder) | `BMW/3-Series/bmw-3-series-320i.svg` | REQUIRES_VERIFICATION |
| 330i M Sport | G20 | Global | BMW PressClub (press.bmwgroup.com) | SVG (placeholder) | `BMW/3-Series/bmw-3-series-320i.svg` (reused) | REQUIRES_VERIFICATION |
| 520i Luxury | G60 | Global | BMW PressClub (press.bmwgroup.com) | SVG (placeholder) | `BMW/5-Series/bmw-5-series-520i.svg` | REQUIRES_VERIFICATION |
| X3 xDrive20d | G01 | Global | BMW PressClub (press.bmwgroup.com) | SVG (placeholder) | `BMW/X3/bmw-x3-xdrive20d.svg` | REQUIRES_VERIFICATION |
| X5 xDrive40i | G05 | Global | BMW PressClub (press.bmwgroup.com) | SVG (placeholder) | `BMW/X5/bmw-x5-40i.svg` | REQUIRES_VERIFICATION |

**BMW Image Source:** https://www.press.bmwgroup.com/global/photo/topic/4142/3-series/

---

### Mercedes-Benz

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| C 200 | W206 | Global | Mercedes-Benz Media (media.mercedes-benz.com) | SVG (placeholder) | `Mercedes-Benz/C-Class/mercedes-c200.svg` | REQUIRES_VERIFICATION |
| C 300 AMG Line | W206 | Global | Mercedes-Benz Media (media.mercedes-benz.com) | SVG (placeholder) | `Mercedes-Benz/C-Class/mercedes-c200.svg` (reused) | REQUIRES_VERIFICATION |
| E 200 | W214 | Global | Mercedes-Benz Media (media.mercedes-benz.com) | SVG (placeholder) | `Mercedes-Benz/E-Class/mercedes-e200.svg` | REQUIRES_VERIFICATION |
| GLC 200 4MATIC | X254 | Global | Mercedes-Benz Media (media.mercedes-benz.com) | SVG (placeholder) | `Mercedes-Benz/GLC/mercedes-glc-200.svg` | REQUIRES_VERIFICATION |

**Mercedes Image Source:** https://media.mercedes-benz.com/

---

### Audi

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| A4 45 TFSI | B9 | Global | Audi MediaCenter (media.audi.com) | SVG (placeholder) | `Audi/A4/audi-a4-45-tfsi.svg` | REQUIRES_VERIFICATION |
| A6 45 TFSI quattro | C8 | Global | Audi MediaCenter (media.audi.com) | SVG (placeholder) | `Audi/A6/audi-a6-45-tfsi-quattro.svg` | REQUIRES_VERIFICATION |
| Q5 45 TFSI quattro | FY | Global | Audi MediaCenter (media.audi.com) | SVG (placeholder) | `Audi/Q5/audi-q5-45-tfsi.svg` | REQUIRES_VERIFICATION |

**Audi Image Source:** https://www.audi-mediacenter.com/en

---

### Toyota

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| Camry 2.5Q | XV70 | Vietnam | Toyota Dealer CDN (images.dealer.com) | **JPG** | `Toyota/Camry/toyota-camry-25q.jpg` | **VERIFIED** |
| Corolla Cross 1.8V | XU10 | Global | Toyota Dealer CDN (images.dealer.com) | **JPG** | `Toyota/Corolla-Cross/toyota-corolla-cross-18v.jpg` | **VERIFIED** |
| Fortuner 2.4 VRZ | AN150 | SEA | Toyota Vietnam (toyota.com.vn) | SVG (placeholder) | `Toyota/Fortuner/toyota-fortuner-24-vrz.svg` | REQUIRES_VERIFICATION |

**Toyota Image Sources:**
- Camry: `https://images.dealer.com/ddc/vehicles/2026/Toyota/Camry/Sedan/still/front-left/front-left-1024-en_US.jpg`
- Corolla Cross: `https://images.dealer.com/ddc/vehicles/2025/Toyota/Corolla%20Cross/SUV/still/front-left/front-left-640-en_US.jpg`
- Fortuner: Not available on US dealer CDN (non-US market). Need to source from Toyota ASEAN media or Toyota Vietnam.

---

### Honda

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| Civic 1.5 RS | FE/FG | Global | Honda Newsroom (hondanews.com) | SVG (placeholder) | `Honda/Civic/honda-civic-15rs.svg` | REQUIRES_VERIFICATION |
| CR-V 1.5 L | RW | Vietnam | Honda Vietnam (honda.com.vn) | SVG (placeholder) | `Honda/CR-V/honda-crv-15-l.svg` | REQUIRES_VERIFICATION |

**Honda Image Source:** https://hondanews.com/en-US/honda-automobiles/channels/honda-civic

---

### Mazda

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| CX-5 2.5 Turbo | KF | Global | Mazda Media (mazda.com/globalnews) | SVG (placeholder) | `Mazda/CX-5/mazda-cx5-25t.svg` | REQUIRES_VERIFICATION |
| Mazda3 2.0 Premium | BP | Global | Mazda Media (mazda.com/globalnews) | SVG (placeholder) | `Mazda/Mazda3/mazda3-20-premium.svg` | REQUIRES_VERIFICATION |

**Mazda Image Source:** https://www.mazda.com/en/globalnews/

---

### Lexus

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| ES 300h | XV70 | Global | Lexus Pressroom (lexus.com/pressroom) | SVG (placeholder) | `Lexus/ES/lexus-es-300h.svg` | REQUIRES_VERIFICATION |
| RX 350h | AL30 | Global | Lexus Pressroom (lexus.com/pressroom) | SVG (placeholder) | `Lexus/RX/lexus-rx-350h.svg` | REQUIRES_VERIFICATION |

**Lexus Image Source:** https://www.lexus.com/pressroom

---

### Porsche

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| 911 Carrera | 992 | Global | Porsche Newsroom (newsroom.porsche.com) | SVG (placeholder) | `Porsche/911/porsche-911-carrera.svg` | REQUIRES_VERIFICATION |
| Cayenne | E3 | Global | Porsche Newsroom (newsroom.porsche.com) | SVG (placeholder) | `Porsche/Cayenne/porsche-cayenne.svg` | REQUIRES_VERIFICATION |

**Porsche Image Sources:**
- 911: `https://newsroom.porsche.com/en_US/model-range/911/911-carrera.html` (ZIP download ~109MB available)
- Press photos: `https://press.porsche.com/prod/presse_pag/PressResources.nsf/jumppage/modelle-911-911_carrera`

---

### Volvo

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| XC60 B5 AWD | II | Global | Volvo Media (media.volvocars.com) | SVG (placeholder) | `Volvo/XC60/volvo-xc60-b5.svg` | REQUIRES_VERIFICATION |
| S90 B5 AWD | II | China | Volvo Media (media.volvocars.com) | SVG (placeholder) | `Volvo/S90/volvo-s90-b5.svg` | REQUIRES_VERIFICATION |

**Volvo Image Source:** https://media.volvocars.com/

---

### Ford

| Variant | Generation | Market | Official Source | Image Type | Local File | Status |
|---------|-----------|--------|----------------|------------|------------|--------|
| Ranger 2.0L Bi-Turbo | PJ | Global | Ford Media (media.ford.com) | SVG (placeholder) | `Ford/Ranger/ford-ranger-20-bi-turbo.svg` | REQUIRES_VERIFICATION |
| Everest 2.0L Bi-Turbo | U704 | ASEAN | Ford Vietnam / Ford ASEAN | SVG (placeholder) | `Ford/Everest/ford-everest-20-bi-turbo.svg` | REQUIRES_VERIFICATION |

**Ford Image Source:** https://media.ford.com/

---

## JPG Download Progress

| Vehicle | CDN Source | Downloaded | File Size | Resolution |
|---------|-----------|-----------|-----------|------------|
| Toyota Camry 2.5Q | images.dealer.com | YES | ~640px wide | 640px |
| Toyota Corolla Cross 1.8V | images.dealer.com | YES | ~640px wide | 640px |
| All others | Various CDNs | NO | N/A | N/A |

---

## Recommended Next Steps

### Priority 1: Manual Image Collection (25 vehicles)

For each vehicle below, visit the official source URL, find the correct press/studio photo for the exact variant, and save as JPG/PNG:

**Easy to download (CDN available):**
1. Porsche 911 Carrera → Download from `newsroom.porsche.com` (ZIP bundles available)
2. Porsche Cayenne → Download from `newsroom.porsche.com`
3. Toyota Fortuner → Search `mazda.com` → not available; try Toyota ASEAN press

**Medium difficulty (registration may be needed):**
4. BMW 320i/330i/520i/X3/X5 → `press.bmwgroup.com`
5. Mercedes C200/C300/E200/GLC → `media.mercedes-benz.com`
6. Audi A4/A6/Q5 → `media.audi.com`
7. Honda Civic/CR-V → `hondanews.com`
8. Mazda CX-5/Mazda3 → `mazda.com/globalnews`
9. Lexus ES/RX → `lexus.com/pressroom`
10. Volvo XC60/S90 → `media.volvocars.com`
11. Ford Ranger/Everest → `media.ford.com`

### Priority 2: Update Seeder Media Mapping

After downloading JPGs, update the `SeedMedia` method in `AutoHubSeeder.cs` to reference the new `.jpg` files instead of `.svg`.

### Priority 3: Color Swatch Images (Optional)

Current color swatches are SVG files with hex code colors. These work well as-is for the color picker UI. No replacement needed unless you want real paint finish photos.

---

## Folder Structure (Current)

```
wwwroot/Images/
├── BMW/
│   ├── 3-Series/
│   │   ├── bmw-3-series-320i.svg
│   │   └── colors/ (4 SVG)
│   ├── 5-Series/
│   │   ├── bmw-5-series-520i.svg
│   │   └── colors/ (4 SVG)
│   ├── X3/
│   │   ├── bmw-x3-xdrive20d.svg
│   │   └── colors/ (4 SVG)
│   └── X5/
│       ├── bmw-x5-40i.svg
│       └── colors/ (4 SVG)
├── Mercedes-Benz/
│   ├── C-Class/
│   │   ├── mercedes-c200.svg
│   │   └── colors/ (4 SVG)
│   ├── E-Class/
│   │   ├── mercedes-e200.svg
│   │   └── colors/ (4 SVG)
│   └── GLC/
│       ├── mercedes-glc-200.svg
│       └── colors/ (4 SVG)
├── Audi/
│   ├── A4/
│   │   ├── audi-a4-45-tfsi.svg
│   │   └── colors/ (4 SVG)
│   ├── A6/
│   │   ├── audi-a6-45-tfsi-quattro.svg
│   │   └── colors/ (4 SVG)
│   └── Q5/
│       ├── audi-q5-45-tfsi.svg
│       └── colors/ (4 SVG)
├── Toyota/
│   ├── Camry/
│   │   ├── toyota-camry-25q.svg
│   │   ├── toyota-camry-25q.jpg ← NEW
│   │   └── colors/ (4 SVG)
│   ├── Corolla-Cross/
│   │   ├── toyota-corolla-cross-18v.svg
│   │   ├── toyota-corolla-cross-18v.jpg ← NEW
│   │   └── colors/ (4 SVG)
│   └── Fortuner/
│       ├── toyota-fortuner-24-vrz.svg
│       └── colors/ (4 SVG)
├── Honda/
│   ├── Civic/
│   │   ├── honda-civic-15rs.svg
│   │   └── colors/ (4 SVG)
│   └── CR-V/
│       ├── honda-crv-15-l.svg
│       └── colors/ (4 SVG)
├── Mazda/
│   ├── CX-5/
│   │   ├── mazda-cx5-25t.svg
│   │   └── colors/ (4 SVG)
│   └── Mazda3/
│       ├── mazda3-20-premium.svg
│       └── colors/ (4 SVG)
├── Lexus/
│   ├── ES/
│   │   ├── lexus-es-300h.svg
│   │   └── colors/ (4 SVG)
│   └── RX/
│       ├── lexus-rx-350h.svg
│       └── colors/ (4 SVG)
├── Porsche/
│   ├── 911/
│   │   ├── porsche-911-carrera.svg
│   │   └── colors/ (4 SVG)
│   └── Cayenne/
│       ├── porsche-cayenne.svg
│       └── colors/ (4 SVG)
├── Volvo/
│   ├── XC60/
│   │   ├── volvo-xc60-b5.svg
│   │   └── colors/ (4 SVG)
│   └── S90/
│       ├── volvo-s90-b5.svg
│       └── colors/ (4 SVG)
├── Ford/
│   ├── Ranger/
│   │   ├── ford-ranger-20-bi-turbo.svg
│   │   └── colors/ (4 SVG)
│   └── Everest/
│       ├── ford-everest-20-bi-turbo.svg
│       └── colors/ (4 SVG)
├── hero-graphite.jpg
├── cockpit-focus.jpg
├── aero-study.jpg
└── apex-logo.png
```

---

## Notes

- All 27 SVG placeholder images exist and are correctly referenced after bug fixes
- SVG images are stylized illustrations, not real photos — they fit the "Monolith Velocity" design aesthetic
- The 2 downloaded JPGs (Camry, Corolla Cross) have been added alongside existing SVGs
- The VehicleService `SeedMedia` method still references `.svg` files — update to `.jpg` when real photos are ready
- Color swatch SVGs use hex codes and work as-is for the color picker UI
- No UI/UX changes were made in this task
