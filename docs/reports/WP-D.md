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

## Review fixes
Findings from `docs/reports/WP-D-review.md`.

Should fix:
1. Re-Begin crash (BrewPanel): added `TimingBar.Cancel()` (stops the bar, clears feedback, no `Stopped` emit). `BrewPanel.Begin` calls it first, and `OnStepStopped` now returns if `!_active || _stepIndex >= _steps.Count`.
2. Half-pixel position: replaced the Center anchor preset with `_panel.Position = ((Size - _panel.Size) / 2).Floor()`.
3. `_ExitTree`/`_Ready` Stopped re-entry hang (second opinion): removed the unsubscribe from `_ExitTree`. The subscription is made once in `_Ready` and the bar is a child freed with the panel, so nothing leaks. Explained in a comment.

Nits fixed (trivial): removed unused `PanelWidth`; moved `_pendingItem` up with the fields; `TimingBar` now calls `SetProcess` so it is idle when not running or showing feedback; marker mapping aligned with the zone (centre = 1 + pos * inner, clamped).

Nits left: test hooks stay in production code (the synchronous smoke suite needs them); `Begin`-before-`_Ready` pending state is still documented only (the class comment says to call Begin after adding to the tree); label-height clipping is unverified without a window.

Smoke suite additions (covers the second-opinion nit): re-Begin pastry after a running drink, `StopNow` on a cancelled bar, remove and re-add the panel then run a step, whole-pixel panel position.

Results: `dotnet build` 0 warnings 0 errors; `dotnet test` 40 passed; headless smoke after `--import`: `SMOKE OK (26 checks)`, exit 0. Nothing skipped. No visual check in a real window.

## Review fixes (round 2)
Source: "Second opinion (/code-review)" in `docs/reports/WP-D-review.md`. The review verdict was APPROVE, so everything here is optional.

Fixed (rated important by /code-review):
1. `_ExitTree` / `_pauseToken` stall (BrewPanel): added `_advancePending`. Set when a step stops, cleared when the step advances or `Begin` runs. `_ExitTree` still invalidates the old timer; the new `_EnterTree` calls `ScheduleAdvance()` if a pause was pending, so a panel removed and re-added mid-pause finishes its brew. The timer callback also checks `IsInsideTree()`.
2. Centring before layout (BrewPanel): centring moved into `Recenter()` (`ResetSize()` then `((Size - panelSize) / 2).Floor()`). It runs on the `Resized` signal and after every `StartStep`, so the `Begin`-before-`_Ready` path and size changes both end up centred on whole pixels.

Nits fixed (trivial):
3. "Perfect!" threshold (TimingBar): `PerfectAccuracy` is now `1.0 - 1e-9`, so the label matches the drawn green zone (accuracy 1.0).
4. Input grace (TimingBar): mouse and `confirm` handlers ignore input on the frame a run begins (`Engine.GetProcessFrames()`), so the event that started the brew cannot stop step 1. `StopNow()` itself is not gated, so tests still drive it directly.

Left as is:
5. Mouse backdrop while brewing: whether the café blocks clicks, dims, or pauses during a brew is a scene-level decision for WP-E, which owns the café. Adding a Stop-filter backdrop here would be speculative. Recorded as a need for WP-E.
6. Per-step SFX for Matcha/Cocoa/Ice: `SfxId` is owned by another package and has no such IDs; `Click` fallback is intentional until audio exists.

Smoke additions (7 new checks, 26 -> 33 in the Brew suite; whole run 33 `ok` lines): advance pending after stop, stays pending after removal, re-armed on re-add, completes afterwards; centred after `Resized` for a panel that was `Begin`-ed before `_Ready`; re-centred on an odd size with whole pixels; Perfect threshold. The real timer firing is not exercised headlessly (the suite is synchronous), only the re-arm state.

Results: `dotnet build` 0 warnings 0 errors; `dotnet test` 40 passed; headless smoke `SMOKE OK (33 checks)`, exit 0. Nothing failed or was skipped. No visual check in a real window.

## Review fixes (round 3)
Source: second opinion in `docs/reports/WP-D-review.md` (verdict CHANGES REQUESTED, 0 Blocker).

Already fixed in earlier rounds (598f37e): reviewer Should fix 1 (stall on `_ExitTree` during pause) and 2 (centring before layout), Nits 1 (Perfect threshold) and 2 (same-frame input).

Fixed now:
1. Second-opinion Should fix (stale min-size when centring): `BrewPanel` now also subscribes to the inner panel's `MinimumSizeChanged` (emitted after Godot recalculates the size at end of frame) and calls `Recenter()`. The immediate call in `StartStep` stays. Self-signal of a child we own, freed with us, so no disconnect needed.
2. Reviewer Nit 3 (`TimingBar` before `_Ready`): label text is stored in `_labelText` and applied in `_Ready`; `StopNow` no longer dereferences a null `_feedback`.

Not changed (Nits, judgement calls): the same-frame input guard also applies when step 2 begins from the timer (very small window, a swallowed double-tap just keeps the bar running); `_EnterTree` re-arm restarts the full pause; test hooks remain public (noted for WP-E); smoke assertions for the Perfect threshold and async `Resized` timing left as is; Perfect threshold stays at the strict 1 - 1e-9 (matches the drawn zone).

Results: `dotnet build` 0 warnings 0 errors; `dotnet test` 40 passed; headless smoke `SMOKE OK (33 checks)`, exit 0 (the one ArtCatalog missing-art warning is the intentional fallback test). No window check.
