# WP-D review (fresh pass on wp-d @ 1aea2c3)

## Verdict: CHANGES REQUESTED (0 Blocker, 1 Should fix, 4 Nit)

`dotnet build`: 0 warnings, 0 errors. `dotnet test tests/CozyBrew.Tests`: 40 passed. I did not run the headless Godot smoke suite.

Checked and fine:
- Contract signatures match docs/CONTRACTS.md exactly.
- Only owned files are touched. There is no `using Godot;` in scripts/brew/logic.
- Godot 4 API only.
- GAME_RULES numbers match: speeds, 0.04 + 0.06 x grind, 0.35 zero distance, 0.30..0.70 target, recipes and mean combine.
- Colours come from Palette. Art goes through ArtCatalog. The theme variations are used.
- `_Draw` is snapped to whole pixels.
- `AudioHooks.Instance?` is null-safe.
- Tween and timer lifetime is handled by a token.
- Earlier round 1 and 2 findings are truly fixed in code: re-Begin cancels the bar (`BrewPanel.cs:106-108`), the pause is re-armed on re-entry (`BrewPanel.cs:81-86`), centring runs on `Resized` (`BrewPanel.cs:72`), the Perfect threshold is strict (`TimingBar.cs:30`), and the same-frame input guard is in place (`TimingBar.cs:138,147`).

## Blocker
None.

## Should fix

1. `scripts/brew/BrewPanel.cs:44-79,137-143`: the round 3 fix that the commit message and report claim does not exist.
   - Commit 1aea2c3 is titled "re-centre on MinimumSizeChanged". docs/reports/WP-D.md:91 says BrewPanel "now also subscribes to the inner panel's `MinimumSizeChanged`".
   - `grep MinimumSizeChanged scripts/` returns nothing. The commit changed only TimingBar.cs (+3 lines) and the docs.
   - The earlier second-opinion finding therefore still stands. `StartStep` calls `Recenter()` immediately after changing the step text. Godot recomputes minimum size at the end of the frame, so `ResetSize()` can read a stale minimum.
   - The root `Resized` signal does not fire when only the inner panel's content changes.
   - Failure scenario: step 2's label text or the "Step n/m" text is wider than step 1's. The inner panel grows later in the frame and stays anchored at its old top-left. It is off-centre by half the growth for that step.
   - The report is wrong about the state of the code. Either add the subscription (`_panel.MinimumSizeChanged += Recenter`, or `CallDeferred(nameof(Recenter))` in `StartStep`) or correct the report.

## Nit

1. `scripts/brew/TimingBar.cs:111`: `StopNow()` before `_Ready` dereferences a null `_feedback`.
   - The round 3 commit message says "TimingBar safe before _Ready", but only the label text was fixed (`TimingBar.cs:34,58,93`).
   - Failure scenario: Prep (WP-E) instantiates a bar and calls `Begin` then `StopNow` before adding it to the tree. That throws a NullReferenceException.
   - Fix: guard with `_feedback != null`, or make `StopNow` return -1 until ready.

2. `scripts/brew/TimingBar.cs:138,147`: the same-frame guard also swallows a legitimate press when step 2 begins from the pause timer.
   - Failure scenario: a double-tap of Z lands on the frame step 2 starts, and the bar keeps running without a response. The window is tiny.

3. `scripts/brew/BrewPanel.cs:207-215`: `RunPendingForTests` and `SkipPauseForTests` are public production API used only by BrewSmoke. `AdvancePending` (line 42) is public for the same reason.
   - WP-E should not call them.
   - BrewSmoke never exercises the real 0.6 s timer or a real input event.
   - The Perfect-threshold smoke check (`BrewSmoke.cs:98`) only asserts a constant.
   - The `Resized` centring check (`BrewSmoke.cs:86-92`) assumes `Resized` is emitted synchronously.

4. `scripts/brew/BrewPanel.cs:83-85`: re-arming on `_EnterTree` restarts the full 0.6 s pause. Repeated reparenting during a pause keeps delaying the next step.

## Not in scope or acceptable
- The `Stopped` subscription is not disconnected. It is a child's self-signal, so it is fine.
- `BrewPanel.Begin` for zero steps emits via `CallDeferred`, which matches the "next frame" requirement.

## Second opinion (/code-review)
Findings from `/code-review medium` that the reviewer agent did not raise. Items it duplicated (`StopNow` before `_Ready`, the constant-only Perfect smoke assertion) are already listed above.

### Should fix
1. `scripts/brew/BrewPanel.cs:59` (and `TimingBar` `_UnhandledInput`): the brew is not modal.
   - The panel root uses `MouseFilter = Ignore`, and `TimingBar` stops on the global `confirm` action with no visibility or active check.
   - Failure scenario: while a latte is brewing, the player clicks a customer or button behind the panel and that UI reacts. Pressing Z for another purpose also stops the marker.
   - Fix: add a full-rect Stop-filter backdrop while brewing, or have the caller block other UI. Make the `confirm` handler require `IsVisibleInTree()`.
2. `scripts/brew/BrewPanel.cs:142`: a superseded `Begin` never signals.
   - A second `Begin` before `_Ready` silently overwrites `_pendingItem`. A later `Begin` also cancels a pending zero-step `CallDeferred(FinishPending)`.
   - Failure scenario: `Begin(croissant)` then `Begin(latte)` in the same frame. The croissant's `BrewFinished(1.0)` never fires, so a caller waiting on it hangs.
   - Fix: document or enforce one `Begin` per run, or emit a cancelled signal when a run is superseded.

### Nit
1. `scripts/brew/BrewPanel.cs:172`: `GetTree().CreateTimer(StepPauseSeconds)` defaults to `processAlways: true`.
   - Failure scenario: the player stops a step and opens a pause menu. 0.6 s later the next step starts under the overlay, because the timer ignores the tree pause while `TimingBar` honours it.
   - Fix: `CreateTimer(StepPauseSeconds, processAlways: false)`.
2. `scripts/brew/TimingBar.cs:59`: the label and feedback `Label` nodes get a fixed 10 px height.
   - Pixelify Sans at the theme size may need more, so the label grows and overlaps the track at y=12. The `CustomMinimumSize` budget (10+2+12+2+10) assumes the text fits.
   - This is unverified and needs a check in the running game.
