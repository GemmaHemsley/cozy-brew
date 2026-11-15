# Little Bean Café

Cozy pixel art café sim. Godot 4 (.NET / C#). **Godot 4 API only, never Godot 3** (no KinematicBody2D, yield, old signal syntax).
The developer is an experienced C#/.NET dev who is new to Godot: briefly explain Godot-specific concepts when you introduce them.

## Display
- Base resolution **320x180**, stretch mode `viewport`, aspect `keep`, scale mode `integer`
- Nearest texture filtering everywhere; all positions and sizes in whole pixels
- Font: Pixelify Sans (`art/fonts/`), imported with antialiasing off
- Controls: mouse first; arrow keys + Z (confirm) / X (cancel) as backup

## Game design (source of truth)
One in-game day is about 6 real minutes (keep it configurable). No fail state: a slow day just means fewer tips.

Core loop:
1. **Prep**: grind beans, fill the pastry case, pick today's special
2. **Open**: customers arrive in waves; each has a mood, an order, and a patience meter
3. **Brew**: short timing minigames (pull the shot, steam milk, pour art)
4. **Serve & chat**: speed + accuracy = tips; chatting builds friendship
5. **Close**: count coins, buy recipes, decorate, sleep, next day

Progression: coins unlock recipes (Matcha on day 2, Cake on day 5) and decor. Each regular has a 5-heart friendship track that unlocks a small story scene and their "usual" order. Decor raises shop coziness, which brings in new customer types.

### Menu (starting prices, in coins)
| Item | Recipe | Price |
|---|---|---|
| Latte | espresso + steamed milk | 4 |
| Espresso | espresso shot | 3 |
| Matcha | matcha + milk (unlock day 2) | 5 |
| Cocoa | cocoa + milk + marshmallow | 4 |
| Iced coffee | cold brew + ice + milk | 4 |
| Croissant | pastry case | 3 |
| Muffin | pastry case | 3 |
| Cookie | pastry case | 2 |
| Cake | pastry case (unlock day 5) | 5 |

### Cast
- **Barista** (player): green apron, behind the counter
- **Biscuit**: orange shop cat
- Regular customers (three body shapes, recoloured): **Juniper** (regular), **Sage** (studies here), **Milo** (commuter), **Rosie** (sweet tooth), **Theo** (night owl)

### Palette (outlines are Espresso, never pure black)
Espresso #2b1d1f · Dark Roast #4a2f2a · Walnut #6e4434 · Oak #9a6440 · Honey #c48a56 · Latte #e3b98a · Cream #f5e2c4 · Milk #fff6e6 · Cherry #b5453a · Apricot #e07a4f · Lamplight #f2c45a · Fern #5f8a52 · Matcha #8fb36a · Dusk #4f6d8a · Sky #8db1c4 · Rose #d98a9a · Peach #f0c3a0 · Cinnamon #b9785a

Text: Espresso. Secondary text: Walnut. Accent headings: Cherry. Patience bar fill: Fern. Progress fill: Apricot.

## Art (`art/`)
Intended specs: 16 px tile grid, characters 16x24, cat 16x16, menu items 16x16, icons 10x10 in 16 px cells.
Animation: idles at 4 fps, actions at 8 fps, blink every 3–5 s.

The art is **individual single-frame PNGs, not sprite sheets** (41 files). `art/ART_MANIFEST.md` is the source of truth for exact sizes, frames and known quirks. Summary:

| Files | Contents |
|---|---|
| `barista_idle/bob/blink.png` | Barista, 16x24 each (idle loop = idle + bob; blink separate) |
| `cat_idle/blink.png` | Biscuit, 16x16 |
| `cust_beanie/dusk/fox/rose/sage.png` | Customers, 16x24. Three body silhouettes, not one shared body. Name mapping to Juniper/Sage/Milo/Rosie/Theo is **unconfirmed** (guess: fox=Juniper, sage=Sage, beanie=Milo, rose=Rosie, dusk=Theo) |
| `latte, espresso, matcha, cocoa, iced_coffee, croissant, muffin, cookie, cake.png` | Menu items, 16x16 (`iced_coffee` is currently 16x15) |
| `floor_wood, floor_checker, wallpaper, wainscot, brick, counter_top, counter_front.png` | 16x16 tiles |
| `espresso_machine` 24x24, `window` 32x32, `chalkboard` 32x24, `shelf` 32x16 | Props |
| `plant, table, chair, lamp.png` | Props, 16x16 (chair: flip for left) |
| `icon_coin/heart/star/clock.png` | Icons, 10x10, standalone |
| `ui_panel.png` | 9-slice panel, 24x24, 4 px margins |
| `ui_button.png`, `ui_button_pressed.png` | 9-slice button, 32x14, 4 px margins |

`bg_cafe_reference.png` (full café mockup, characters baked in) is **reference only**; build the real room from tiles + props. It is not in `art/` yet.

`art/DESIGN_REFERENCE.md` holds the UI layouts (HUD, dialogue box, title) from the design artifact, in 3x board pixels with game-pixel conversions.

**Measure the actual PNG dimensions before slicing.** Layout and padding may not match the intended specs exactly. Record what you find in `art/ART_MANIFEST.md` and keep it up to date. `ArtCatalog` maps keys to files (region = whole image) so it can move to sheets later without changing callers.

Art not made yet (use placeholders built from palette colours): walk cycles, seated poses, brewing minigame screens, pour-art sprites, seasonal decor. No audio yet: add hooks only.

## Conventions
- Folders: `scripts/`, `scenes/`, `art/`, `audio/`
- `public partial class`, filename == class name
- `[Export]` for inspector values; `[Signal]` delegates or C# events for signals
- All art paths and frame regions go through one `ArtCatalog` so swapping art is trivial; missing art falls back to a coloured rect and never crashes
- UI uses a single Godot `Theme` built from `ui_panel.png` / `ui_button.png` (StyleBoxTexture, 4 px margins) and Pixelify Sans
- Simple `.tscn` edits are fine; for complex layout, explain the editor steps instead
- Build only what the current prompt asks for; note ideas for later instead of implementing them
- After changes, run `dotnet build` and fix all errors before reporting done
