# Design reference (from the "Little Bean Café" design artifact)

Source: claude.ai artifact `c5caf2ef-245d-4fbf-94fe-19b6c661ded4` (8 boards). Boards are 960x540, which is **3x** the 320x180 game viewport. Divide board pixel values by 3 for game pixels. This file only holds what is readable from the boards' HTML; the images themselves could not be downloaded (see bottom).

## Gameplay screen (board `Main`)
- **Background:** full café mockup, reference only (`bg_cafe_reference.png`).
- **Top-left HUD panel** (9-slice panel) at ~(5, 5): coin icon, coin count (e.g. 128), thin divider, then 3 hearts, the third dimmed (35% opacity). Meaning of the hearts is not stated (shop rating? friendship of the current customer?). Decide before building.
- **Top-right HUD panel** at ~(right 5, top 5): clock icon + `Day 3 · 9:40 AM`.
- **Order chip** (panel) mid-counter at ~(163, 87): item icon + `×1`.
- **Dialogue box** (panel) along the bottom, margins ~8 px left/right, ~5 px bottom:
  - Row 1: customer name (Cherry, bold) on the left; "Patience" label + bar on the right. Bar is ~40x4 px with a 1 px Espresso border, Latte background and Fern fill.
  - Row 2: customer line of dialogue, e.g. "Morning! Could I get a honey latte? Extra foam, if the cat doesn't mind."
  - Row 3, right-aligned buttons: **Start brewing**, **Chat**.

## Title screen (board `Title`)
- Café mockup as background, dimmed with Espresso at 55% opacity.
- Centred stack: `A COZY CAFÉ SIM` (Lamplight, letter-spaced), `Little Bean` (Milk, large, Espresso drop shadow, Cherry offset), `Café` (Apricot).
- Three buttons, 260/3 = ~87 px wide: **New day**, **Continue**, **Settings**.
- Footer hint: `Press Enter to open the shop` (Cream).
- MVP scope note: Continue and Settings are not in the MVP plan; show them disabled or leave them out.

## UI kit (board `UIKit`)
- Two 9-slice sprites build every panel and button (4 px margins), as in CLAUDE.md.
- **Disabled button:** 50% opacity (artifact example "Locked").
- **Order ticket** (panel, ~80 px wide): header row `ORDER #07` (Walnut) and customer name; item icon + item name (bold) + modifiers line (e.g. "oat milk · extra foam", Walnut); progress bar with Apricot fill.
- **Stat chip:** coin icon + number; star icon + `4.8` (suggests a star rating exists in the design; not in CLAUDE.md).
- **Type scale (given at 3x, so game px = value / 3):** Title 40 (~13), Heading 24 (8), Body 18 (6), Caption 14 (~5). **Check legibility:** Pixelify Sans at 6 px and 5 px may be too small to read. Likely need 8 px minimum; test in-engine before committing to the scale.
- Colours: names Cherry, secondary text Walnut, text Espresso, as in CLAUDE.md.

## Cast and sheets
- Characters board describes customers as sharing **one body, recoloured**, shown left to right: Juniper (Regular), Sage (Studies here), Milo (Commuter), Rosie (Sweet tooth), Theo (Night owl). The delivered art actually has three body shapes (see ART_MANIFEST.md).
- Original sheet images in the artifact (barista 3 frames, cat 2, customers 5, menu 9 in a row, tiles 7 in a row, props, icons 4 in a row, plus ui_panel, ui_button, and the full mockup) correspond to the sheet names in the old CLAUDE.md. The 41 single files in `art/` were cut from these.

## Differences from CLAUDE.md worth deciding
1. Hearts in the HUD (and the star rating chip): not in the game design. Hearts could be the current regular's friendship; ignore for MVP.
2. Title has Continue/Settings. Not MVP.
3. Order tickets show modifiers ("oat milk · extra foam") and the dialogue mentions "honey latte, extra foam". CLAUDE.md has no modifiers. Left out of MVP; keep `OrderLine` extensible.
4. Clock shows in-game time (`9:40 AM`) while CLAUDE.md says one day is ~6 real minutes. Plan: map day progress to an in-game clock display.
5. Design notes board mentions audio: lo-fi loop, milk steam, cup clink, door bell, cat purr. Use as the `SfxId` list for `AudioHooks`.
6. Design notes board says touch input; CLAUDE.md says mouse first, keyboard backup. Keep CLAUDE.md.

## Not imported
All 13 assets in the artifact are marked unreadable by the artifacts tool, so no images were pulled. To get `bg_cafe_reference.png`, download it from the artifact in the browser (the café background image used on the Gameplay and Title boards) and drop it in `art/`.
