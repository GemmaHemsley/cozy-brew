# WP-D: brew minigames (paste into a fresh session in worktree `../cozy-brew-wp-d`, branch `wp-d`)

```
Use the godot-builder agent to implement work package WP-D (brew) of Little Bean Café, in this worktree on branch wp-d.

Read first: CLAUDE.md, docs/ARCHITECTURE.md, docs/CONTRACTS.md (section "WP-D" and the ownership table), docs/GAME_RULES.md (Brewing section and Prep grind),
art/DESIGN_REFERENCE.md, README.md (Commands).
Also read: scripts/data/Menu.cs, scripts/data/ItemId.cs, scripts/ui/CafeTheme.cs, scripts/art/ArtCatalog.cs, tests/CozyBrew.Tests/MenuTests.cs.

Goal: the short timing minigames used to brew drinks (and to grind beans in Prep), with all maths in Godot-free, unit-tested code.

You OWN: scripts/brew/** (including scripts/brew/logic/**), scenes/brew/**, tests/CozyBrew.Tests/Brew*Tests.cs, scripts/dev/BrewSmoke.cs, docs/reports/WP-D.md.
You must NOT edit: GameState.cs, ArtCatalog/ArtKeys/CafeTheme, project.godot, any screen, scripts/cafe/**, scenes/actors.
Do NOT reference types from other packages (Mood, DayPlan...): foundation types (MenuItem, ItemId) and primitives only.

Build per the contract and GAME_RULES (each number a named constant):
- scripts/brew/logic (no `using Godot;`): BrewStep, BrewRecipes.StepsFor (Espresso [Shot]; Latte [Shot,Milk]; Matcha [Matcha,Milk]; Cocoa [Cocoa,Milk];
  IcedCoffee [Shot,Ice,Milk]; pastries []), TimingBarConfig, TimingBarMath (Position triangle wave, Accuracy, PerfectHalfWidth, ConfigFor, Combine).
- TimingBar (Control): a horizontal bar drawn in Palette colours inside a CafeTheme panel look: Latte track, Espresso outline, a highlighted target zone (the perfect zone) in Fern/Matcha,
  a moving marker in Apricot; label above (e.g. "Pull the shot"). Stops on mouse click (anywhere on the control) or the `confirm` action; emits Stopped(accuracy);
  shows brief feedback text ("Perfect!", "Good", "Missed") in SecondaryLabel for ~0.5 s. Random target in 0.30..0.70 per run using the Random passed in.
  Positions of the marker are snapped to whole pixels when drawn.
- BrewPanel (Control): centred panel that runs the recipe steps in order using one TimingBar per step, with a step counter ("Step 1/2") and the item icon from ArtCatalog;
  emits BrewFinished(accuracy) with Combine of the step accuracies; zero steps => emits BrewFinished(1.0) on the next frame without showing anything.
  Labels per step: Shot "Pull the shot", Milk "Steam the milk", Matcha "Whisk the matcha", Cocoa "Stir the cocoa", Ice "Add the ice".
- AudioHooks.Instance?.PlaySfx(SfxId.ShotPull) when the Shot step stops, SfxId.MilkSteam for Milk, SfxId.Click otherwise.

Tests (xUnit, tests/CozyBrew.Tests/Brew*Tests.cs): Position triangle wave at t=0, quarter, half, full period and for several speeds; Accuracy = 1 inside the perfect zone,
0 at >= zero distance, linear in between, symmetric, never outside 0..1; PerfectHalfWidth for grind 0.5 / 0.75 / 1.0; ConfigFor each step has the speeds from GAME_RULES;
BrewRecipes for every ItemId (pastries empty, others match the list); Combine mean and empty => 1.0.
Smoke suite (scripts/dev/BrewSmoke.cs): both scenes instantiate; BrewPanel.Begin for a pastry emits BrewFinished(1.0); for Latte it shows two steps;
TimingBar auto-completes when a test calls its stop path (expose an internal/public StopNow() for tests and document it).

Done when: `dotnet build` 0 errors/0 warnings; `dotnet test` green; headless smoke SMOKE OK (run --import first in this fresh worktree).
Describe each scene's node tree in the report (no headless screenshots). Write docs/reports/WP-D.md, commit on wp-d, do not merge.
Reply with a short summary and the report path.
```
