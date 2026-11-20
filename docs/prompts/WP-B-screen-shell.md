# WP-B: screen shell (paste into a fresh session in worktree `../cozy-brew-wp-b`, branch `wp-b`)

```
Use the godot-builder agent to implement work package WP-B (screen shell) of Little Bean Café, in this worktree on branch wp-b.

Read first: CLAUDE.md, docs/ARCHITECTURE.md, docs/CONTRACTS.md (section "WP-B" and the ownership table), docs/GAME_RULES.md (Day section),
art/DESIGN_REFERENCE.md (Title screen and HUD), README.md (Commands).
Also read: scripts/core/GameState.cs, scripts/core/AudioHooks.cs, scripts/ui/CafeTheme.cs, scripts/art/ArtCatalog.cs, project.godot, main.tscn, scripts/Main.cs.

Goal: the app boots to a Title screen and you can click through Title -> Prep -> Open -> Close -> Prep with stub screens,
a fade between screens, and a working HUD. No gameplay.

You OWN: project.godot (add the SceneRouter autoload only), scripts/core/SceneRouter.cs, scripts/core/Screen.cs,
  scenes/Title.tscn, Prep.tscn, Open.tscn, Close.tscn, scripts/screens/** (TitleScreen, PrepScreen, CafeScreen, CloseScreen),
  scenes/ui/Hud.tscn, scripts/ui/Hud.cs, scripts/Main.cs (route to Title), scripts/dev/ShellSmoke.cs, docs/reports/WP-B.md.
You must NOT edit: GameState.cs, ArtCatalog/ArtKeys, CafeTheme, CLAUDE.md, art/, anything under scripts/cafe, scripts/brew, scenes/actors, scenes/brew.
Do NOT reference types from other packages (Mood, DayPlan, Customer, BrewPanel...): this package may only use foundation types.

Build per the contract:
- SceneRouter autoload (owns a CanvasLayer with a full-screen Espresso ColorRect used for a ~0.2 s fade; GoTo ignores calls while transitioning).
- Four screens as simple scenes using the project theme (panels/buttons/labels via CafeTheme variations, 320x180 layout, whole pixels).
  Title: "Little Bean" TitleLabel + "Café" + "A cozy café sim" + buttons "New day" (and nothing for Continue/Settings). The `confirm` action activates the focused button;
  arrow keys move focus; the first button grabs focus so keyboard-only play works. Mouse works everywhere.
  Prep / Open / Close: a heading, a one-line placeholder description, and the one navigation button from the contract.
- Open.tscn instances the Hud and calls GameState.StartDay() in _Ready; Close "Next day" calls AdvanceDay(); Title "New day" calls NewGame().
- Hud per art/DESIGN_REFERENCE.md and the contract (coin icon + count; clock icon + "Day N - 9:40 AM"; ArtCatalog icons; refresh from signals, clock text once per second).
- Play AudioHooks.Instance?.PlaySfx(SfxId.Click/Confirm) on button presses.
- Main._Ready routes to Title via SceneRouter.

Smoke suite (scripts/dev/ShellSmoke.cs): SceneRouter autoload exists; each of the four scene files loads and instantiates;
the Hud shows coins and updates after GameState.AddCoins; GoTo(Screen.Title) works from the smoke scene without errors.

Done when: `dotnet build` 0 errors/0 warnings; `dotnet test` green; headless smoke SMOKE OK (run --import first in this fresh worktree).
Also confirm by describing (you cannot take screenshots headless) the node tree of each scene. Write docs/reports/WP-B.md,
commit on wp-b, do not merge. Reply with a short summary and the report path.
```
