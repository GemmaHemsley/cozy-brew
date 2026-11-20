# Art Manifest

Measured with a script (Pillow) on 2026-11-15. All sizes are **actual PNG dimensions**.

## Summary

- **41 PNGs, all individual single-frame files. There are no sprite sheets.** CLAUDE.md describes sheets (`barista.png`, `customers.png`, `menu_items.png`, `tiles.png`, `props_a.png`, `props_b.png`, `icons.png`); none of those files exist. Every "frame" below is a whole file, so every frame region is `(0, 0, w, h)`.
- All pixels are on the project palette. No pure black, no off-palette colours, no semi-transparent pixels (alpha is 0 or 255 only), so no antialiasing or filtering artefacts.
- Imports: all 41 use lossless compression (`compress/mode=0`), no mipmaps. Project default texture filter is Nearest (`default_texture_filter=0`).
- Characters, props and items have the **feet / base on the bottom row** of the file. Use a bottom-centre origin when placing them.
- `bg_cafe_reference.png` is **missing** (reference only, not needed to build the room).

## Characters (16x24, one frame per file)

| File | Size | Cell | Frame | Notes |
|---|---|---|---|---|
| `barista_idle.png` | 16x24 | 16x24 | Idle, neutral pose | Opaque bbox x1-15, y1-24 |
| `barista_bob.png` | 16x24 | 16x24 | Idle, "bob" pose | Same as idle but body shifted down 1 px (92 px differ). Idle loop = `idle` + `bob` at 4 fps |
| `barista_blink.png` | 16x24 | 16x24 | Eyes closed | Only 4 px differ from idle (rows 8-9). Play briefly every 3-5 s |
| `cust_beanie.png` | 16x24 | 16x24 | Customer, standing | Body shape A (identical silhouette to `cust_dusk`) |
| `cust_dusk.png` | 16x24 | 16x24 | Customer, standing | Body shape A (Dusk-blue palette) |
| `cust_fox.png` | 16x24 | 16x24 | Customer, standing | Body shape B (own silhouette; differs from A by 12 px, from C by 22 px) |
| `cust_rose.png` | 16x24 | 16x24 | Customer, standing | Body shape C (identical silhouette to `cust_sage`) |
| `cust_sage.png` | 16x24 | 16x24 | Customer, standing | Body shape C (green palette) |

Customer to cast mapping is **not stated anywhere**. Inferred from file names, please confirm:
`cust_fox` = Juniper?, `cust_sage` = Sage, `cust_beanie` = Milo (commuter)?, `cust_rose` = Rosie, `cust_dusk` = Theo (night owl)?

## Cat (16x16)

| File | Size | Cell | Frame | Notes |
|---|---|---|---|---|
| `cat_idle.png` | 16x16 | 16x16 | Biscuit sitting | Opaque bbox x2-16, y2-15 (touches right edge) |
| `cat_blink.png` | 16x16 | 16x16 | Eyes closed | 4 px differ from idle (rows 6-7) |

## Menu items (16x16)

| File | Size | Item | Opaque bbox |
|---|---|---|---|
| `latte.png` | 16x16 | Latte | x1-15, y1-15 |
| `espresso.png` | 16x16 | Espresso | x2-13, y3-15 |
| `matcha.png` | 16x16 | Matcha | x1-15, y1-15 |
| `cocoa.png` | 16x16 | Cocoa | x1-15, y1-15 |
| `iced_coffee.png` | 16x16 | Iced coffee | x3-13, y1-15 (padded from 16x15 with one transparent row on top so its baseline matches the other items) |
| `croissant.png` | 16x16 | Croissant | x0-16, y5-12 (full width) |
| `muffin.png` | 16x16 | Muffin | x2-14, y2-12 |
| `cookie.png` | 16x16 | Cookie | x1-15, y3-14 |
| `cake.png` | 16x16 | Cake | x2-16, y2-14 (touches right edge) |

## Tiles (16x16, fully opaque, no padding)

| File | Size | Tile |
|---|---|---|
| `floor_wood.png` | 16x16 | Wood floor (3 colours) |
| `floor_checker.png` | 16x16 | Checker floor (2 colours) |
| `wallpaper.png` | 16x16 | Wallpaper |
| `wainscot.png` | 16x16 | Wainscot (lower wall panel) |
| `brick.png` | 16x16 | Brick |
| `counter_top.png` | 16x16 | Counter top surface |
| `counter_front.png` | 16x16 | Counter front face (the "Counter" in CLAUDE.md) |

## Props

| File | Size | Cell | Content | Opaque bbox |
|---|---|---|---|---|
| `espresso_machine.png` | 24x24 | 24x24 | Espresso machine | x2-22, y1-24 |
| `window.png` | 32x32 | 32x32 | Window (fully opaque) | full |
| `chalkboard.png` | 32x24 | 32x24 | Chalkboard (fully opaque) | full |
| `shelf.png` | 32x16 | 32x16 | Bean shelf | x0-32, y2-16 |
| `plant.png` | 16x16 | 16x16 | Plant | x1-15, y1-16 |
| `table.png` | 16x16 | 16x16 | Table | x1-15, y6-16 |
| `chair.png` | 16x16 | 16x16 | Chair (faces one way; flip H for the other) | x3-14, y1-16 |
| `lamp.png` | 16x16 | 16x16 | Pendant lamp | x1-15, y0-11 (hangs from top edge) |

## Icons (10x10, one per file)

| File | Size | Notes |
|---|---|---|
| `icon_coin.png` | 10x10 | Coin, 4 colours |
| `icon_heart.png` | 10x10 | Heart; content only fills y0-8 (rows 8-9 empty) |
| `icon_star.png` | 10x10 | Star |
| `icon_clock.png` | 10x10 | Clock |

## UI (9-slice)

| File | Size | Margins | Notes |
|---|---|---|---|
| `ui_panel.png` | 24x24 | 4 px all sides | Corner pixels transparent (rounded). Border is 3 px thick; centre (inside 3 px) is one flat colour, so a 4 px margin is safe |
| `ui_button.png` | 32x14 | 4 px all sides | Normal state. Same structure as panel |
| `ui_button_pressed.png` | 32x14 | 4 px all sides | Pressed state (**not in CLAUDE.md**). Face is 1 px lower (bbox y1-14) |

## Fonts

`art/fonts/Pixelify_Sans/` holds the variable TTF plus `static/` Regular, Medium, SemiBold, Bold. The variable font's import has `antialiasing=0` (good) but `hinting=3` and `subpixel_positioning=4` (auto). See flags.

## Flags: mismatches with CLAUDE.md

1. **No sprite sheets exist.** CLAUDE.md's table lists 7 sheets plus `bg_cafe_reference.png`; reality is 41 single-frame files. Either CLAUDE.md should be updated, or the art regenerated as sheets. `ArtCatalog` should map keys to single files for now (region = whole image) and can switch to sheet regions later without callers changing.
2. ~~`iced_coffee.png` was 16x15~~ **Fixed**: padded to 16x16 (one transparent row added on top; content unchanged).
3. **Customer mapping is undocumented** and customers are **not one shared body recoloured**: there are 3 silhouettes (A: beanie/dusk, B: fox, C: rose/sage). Colour counts also differ (5-8). Fine to use as-is, but names above are guesses.
4. **Animation frames are limited.** Barista has 2 idle frames (`idle`, `bob`) plus blink. Cat has idle plus blink only (no tail wag). Customers have 1 static frame each. Matches "walk cycles, seated poses not made yet".
5. **Icons are 10x10 flat files**, not "10x10 in 16 px cells". Harmless; draw them at native size.
6. **Extra file:** `ui_button_pressed.png` is not in CLAUDE.md.
7. **Missing:** `bg_cafe_reference.png`.
8. **Font location:** CLAUDE.md says `art/fonts/`; actual path is `art/fonts/Pixelify_Sans/`. Font import has hinting/subpixel settings that can blur pixel fonts at non-native sizes (see the architecture note on font settings).
9. **Project skeleton notes (not art):** `screens/` exists but CLAUDE.md specifies `scenes/`; `main.tscn` root has `scale = (0.8238636, 12)`, which looks accidental and would distort everything under it; `project.godot` does not set `stretch/aspect` explicitly (default is `keep`, so OK).
