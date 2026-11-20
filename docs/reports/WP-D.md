# WP-D report: brew

## Summary
Godot-free brew maths (steps, recipes, timing-bar maths) with xUnit tests, plus two reusable views: `TimingBar` (also for Prep grinding) and `BrewPanel`. All numbers from GAME_RULES are named constants in `TimingBarMath`.

## Files added/changed
- `scripts/brew/logic/`: `BrewStep.cs`, `BrewRecipes.cs`, `TimingBarConfig.cs`, `TimingBarMath.cs`
- `scripts/brew/TimingBar.cs`, `scripts/brew/BrewPanel.cs`
- `scenes/brew/TimingBar.tscn`, `scenes/brew/BrewPanel.tscn` (root node + script only; children are built in `_Ready`)
- `tests/CozyBrew.Tests/BrewLogicTests.cs` (matches `Brew*Tests.cs`)
- `scripts/dev/BrewSmoke.cs`
- Generated `.uid` files.

## Node trees (runtime)
TimingBar (Control, min size 208x34, mouse filter Stop)
- Label (step label, centred, 10 px high at y=0)
- Label (feedback, `SecondaryLabel`, y=24)
- Custom `_Draw`: Espresso outline, Latte track, Matcha target zone with Fern top line, Apricot marker with Espresso outline at y=12 (12 px high); marker x snapped to whole pixels.

BrewPanel (Control, full rect, mouse filter Ignore)
- PanelContainer (centred; theme panel look)
  - VBoxContainer
    - HBoxContainer: TextureRect (16x16 item icon from ArtCatalog), Label (`HeadingLabel` item name), Label (`SecondaryLabel` "Step n/m")
    - TimingBar

## How verified
- `dotnet build`: 0 Warning(s), 0 Error(s)
- `dotnet test tests/CozyBrew.Tests`: Passed 40, Failed 0
- Headless smoke (after `--import`): `SMOKE OK (21 checks)`, exit 0; the [Brew] suite has 10 checks (scenes load, StopNow emits once, pastry finishes 1.0, latte 2 steps then finishes, iced coffee 3 steps).
- No screenshots (headless); visual layout unchecked in a real window.

## Deviations from the contract
- None to signatures. Extra public members: `TimingBar.StopNow()`, `Running`, `Target`, `MarkerPosition`; `BrewPanel.StepCount/StepIndex/StepText/Bar/Active`, static `LabelFor`.
- `BrewPanel` keeps the panel on screen for 0.6 s after each step (`StepPauseSeconds`) so the feedback text is readable; `BrewFinished` fires after the last pause. Zero steps: deferred (`CallDeferred`) emit of 1.0, nothing shown.
- If `Begin` is called before the panel is in the tree, it starts once `_Ready` runs.
- Test hooks `BrewPanel.RunPendingForTests()` and `SkipPauseForTests()` exist because the synchronous smoke suite cannot wait for frames/timers.
- `Accuracy` handles `zeroDistance <= perfectHalfWidth` safely (step function) instead of dividing by zero.

## Needs from other packages
- Callers must add `BrewPanel` to the tree before or right after `Begin`; WP-E should free it after `BrewFinished`. Prep grinding: instantiate `TimingBar`, `Begin("Grind the beans", TimingBarMath.ConfigFor(BrewStep.Shot, 0.75), rng)` then `GrindQuality = 0.5 + 0.5 * accuracy`. (A dedicated grind config may be wanted; GAME_RULES gives no grind speed, so the Shot speed is a suggestion.)
- The 208 px bar assumes it sits in a 320x180 view; BrewPanel does not dim the background.

## Open questions
- Grind speed is not specified in GAME_RULES.
- The `confirm` key is read with `_UnhandledInput`; a focused button consuming Z/Enter would take precedence.

## Ideas for later
- Moving-marker trail, perfect-zone flash, sound on perfect, pour-art step, difficulty scaling by day.
