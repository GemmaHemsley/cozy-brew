# Architecture (MVP)

Goal: a playable loop **Title → Prep → Open → Close → next day**. Friendship story scenes and decor are out of the MVP, but the data model leaves room for them (see the end).

Godot terms used here, for a C# developer: a **scene** (`.tscn`) is a saved tree of nodes you instance like a prefab; an **autoload** is a node Godot creates once at startup and keeps alive across scene changes (a singleton); a **signal** is Godot's observer pattern (like a C# event); a **Control** is a UI node; a **CanvasLayer** draws on a separate layer above the world; a **Tween** animates properties over time.

## Build status
| Piece | Status |
|---|---|
| `GameState` autoload, `Menu`/`MenuItem`, `ArtCatalog`/`ArtKeys`/`Palette`, `CafeTheme`, `AudioHooks` (no-op) | **Done** |
| xUnit project, headless smoke test, input actions (`confirm` Z/Enter, `cancel` X/Esc) | **Done** |
| WP-A core logic (moods, tips, spawner, day plan/summary, roster, chat lines) | To do |
| WP-B screen shell (`SceneRouter`, Title/Prep/Open/Close stubs, HUD) | To do |
| WP-C café world views (room, barista, cat, customer, ticket, dialogue box) | To do |
| WP-D brew (timing bar, brew panel, recipes) | To do |
| WP-E integration (the real loop, Prep, Close) | To do |
| WP-F tuning and polish | To do |

## Autoloads (registered in project.godot)
| Name | Job |
|---|---|
| `GameState` | All mutable game data: coins, day, clock, unlocked recipes, hearts, today's `DayPlan`, last `DaySummary`. Signals for changes. |
| `AudioHooks` | `PlaySfx(SfxId)` / `PlayMusic(MusicId)`; no-ops until audio exists. |
| `SceneRouter` (WP-B) | `GoTo(Screen)` with a short fade. Owns its own CanvasLayer for the fade. |

Static classes (no scene state, so not autoloads): `ArtCatalog`, `ArtKeys`, `Palette`, `Menu`, `Roster`, `ChatLines`, `TipCalculator`.
The project-wide `Theme` (`theme/cafe_theme.tres` → `CafeTheme`) is set in Project Settings > GUI > Theme.

## Flow
```
main.tscn (Main: boot only) ──► SceneRouter.GoTo(Title)
Title    "New day" ─► GameState.NewGame() ─► Prep
Prep     grind beans (TimingBar), fill pastry case, pick special ─► writes GameState.Plan ─► Open
Open     GameState.StartDay(); spawner schedule → customers arrive → order → brew → serve → tip → chat
         when DayTimeUp and the café is empty ─► GameState.LastSummary = summary ─► Close
Close    summary + "new tomorrow" ─► "Next day" ─► GameState.AdvanceDay() ─► Prep
```
No fail state: a slow day just means fewer tips.

## Open screen internals (WP-E wires these together)
`CafeScreen` (root of `Open.tscn`) owns a small state machine per customer: `Arriving → Waiting (patience drains) → Ordering (dialogue box) → Brewing (BrewPanel) → Served (tip) → Chatting (optional) → Leaving`; or `WalkedOut` if patience hits 0.
- Logic (Godot-free, unit tested): `CustomerSpawner`, `PatienceMeter`, `TipCalculator`, `CounterSlots`, `DayPlan`, `DaySummary`.
- Views (Godot): `CafeRoom`, `Barista`, `Biscuit`, `Customer`, `OrderTicket`, `DialogueBox`, `Hud`.
- Brew: `BrewPanel` hosts one `TimingBar` per recipe step.
No walk or seated sprites exist, so customers slide to a counter slot with a Tween and stand there; all positions are whole pixels.

## Folders
```
scripts/core/      GameState, AudioHooks, SceneRouter, Screen, SfxId, MusicId
scripts/data/      MenuItem, Menu, enums, CustomerDef, Roster, ChatLines          (Godot-free)
scripts/art/       ArtCatalog, ArtKeys, ArtEntry, Palette                          (only ArtKeys is Godot-free)
scripts/cafe/logic/  Mood, PatienceMeter, TipCalculator, CustomerSpawner, ...      (Godot-free)
scripts/cafe/view/   CafeRoom, Barista, Biscuit, Customer, OrderTicket, DialogueBox
scripts/brew/logic/  BrewStep, BrewRecipes, TimingBarMath, ...                     (Godot-free)
scripts/brew/        BrewPanel, TimingBar
scripts/screens/   TitleScreen, PrepScreen, CafeScreen, CloseScreen
scripts/ui/        CafeTheme, Hud
scripts/dev/       SmokeTest + one *Smoke.cs suite per package (not game code)
scenes/            Title, Prep, Open, Close (.tscn)   scenes/actors  scenes/brew  scenes/ui  scenes/dev
tests/CozyBrew.Tests/   xUnit, compiles only the Godot-free folders above
```
Every class is `public partial class` (Godot nodes) with filename == class name. Global namespace (no `namespace` blocks).

## Room for later (do not build now)
- **Friendship stories:** `RegularId` + `GameState` hearts and chat points exist; a future `StoryScene` keyed by `(RegularId, heart level)` plugs in where hearts change (`HeartsChanged` signal). `CustomerDef` already has room for a favourite ("usual") order.
- **Decor / coziness:** `GameState` will gain `OwnedDecor` and `Coziness`; `CustomerDef.MinCoziness` gates new customer types; `CustomerSpawner` already takes the available-customer list as input.
- **Recipe shop:** `MenuItem.UnlockCost` exists (0 = free on its unlock day).
- **Save/load:** `GameState` is plain data; serialise it by item/regular *names*, not numbers.
- **Audio:** fill in `AudioHooks`; call sites exist.
- **Pour art, walk cycles, seated poses:** art not made; swap in via `ArtCatalog`.
