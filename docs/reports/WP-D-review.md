# WP-D review

## Verdict: CHANGES REQUESTED (0 Blocker, 2 Should fix, 4 Nit)

`dotnet build`: 0 warnings, 0 errors. `dotnet test tests/CozyBrew.Tests`: 40 passed. I did not run the headless Godot smoke suite.

Checked: contract signatures, ownership (only owned files plus the report), Godot 4 API only, no `using Godot;` in `scripts/brew/logic`, constants against GAME_RULES (speeds, 0.04 + 0.06 x grind, 0.35, 0.30..0.70, recipes), Palette use, and the `confirm` and mouse stop paths.

## Should fix

1. `scripts/brew/BrewPanel.cs:88-110, 137-139` (crash on re-Begin). `Begin` for a zero-step item never stops a bar that is already running. The bar stays running while the panel is hidden, and `TimingBar._UnhandledInput` still reacts to `confirm`.
   - Scenario: `Begin(Latte)`, then `Begin(Croissant)` before the first step stops. `_steps` is now empty.
   - The player presses Z. `Stopped` fires and `OnStepStopped` evaluates `_steps[_stepIndex]` on an empty array, so it throws `IndexOutOfRange`.
   - It can also add a stray accuracy.
   - Fix: stop or cancel the bar in `Begin` (add a `TimingBar.Cancel()` or similar), and guard `OnStepStopped` with `if (!_active || _stepIndex >= _steps.Count) return;`.

2. `scripts/brew/BrewPanel.cs:116-117` (half-pixel position). `_panel.SetAnchorsAndOffsetsPreset(LayoutPreset.Center)` centres with `/ 2`. If the panel's size is odd in either axis, the offsets land on .5 and the panel draws at a half pixel, which breaks the whole-pixel rule.
   - The height depends on theme margins and font metrics and is not guaranteed to be even.
   - Fix: round the position, for example `_panel.Position = ((Size - _panel.Size) / 2).Floor()`, or force an even size.
   - The report admits the layout was never checked in a real window.

## Nit

1. `scripts/brew/BrewPanel.cs:19`. `PanelWidth` is unused dead code.
2. `scripts/brew/BrewPanel.cs:181-188`, `scripts/brew/BrewPanel.cs:85`. Test hooks live in production code, and `_pendingItem` is declared mid-class after its first use. Move it up with the other fields.
3. `scripts/brew/TimingBar.cs:101-114`. `_Process` runs every frame even when idle with no feedback showing. Call `SetProcess(_running || _feedbackLeft > 0)` to avoid the idle work.
4. `scripts/brew/TimingBar.cs:144-152`. The target zone maps over `BarWidth - 2` (`inner`), but the marker maps over `inner - MarkerWidth`. The marker centre therefore drifts up to about 1.5 px from where its logical position sits against the zone.
   - Scenario: a stop at exactly the target looks about 1 px off.
   - The effect is cosmetic. Align the two mappings.
5. The `TimingBar` label heights (10 px) may clip the Pixelify Sans text. It is untested visually.

## Second opinion (/code-review)

`/code-review medium` on `main..wp-d`. Its re-Begin crash (same as Should fix 1) and marker/zone mapping mismatch (same as Nit 4) were already in the review above. Only findings the reviewer agent missed are listed here.

### Should fix

1. `scripts/brew/BrewPanel.cs:84` (re-entry hang). `_ExitTree` unsubscribes `_bar.Stopped -= OnStepStopped`, but the subscription is made only in `_Ready`, which runs once per node lifetime.
   - Scenario: the café scene removes `BrewPanel` from the tree between orders and re-adds it. The next `Begin` runs and the player stops the bar. It shows "Perfect!", but `OnStepStopped` is no longer wired, so the panel never advances and `BrewFinished` never fires. The order is stuck.
   - Fix: subscribe in `_EnterTree`, or drop the unsubscribe. The bar is a child and is freed with the panel.

### Nit

1. `scripts/brew/BrewPanel.cs:71`. A `Begin` before `_Ready` is stored only in `_pendingItem`, and a later `Begin` overwrites it.
   - Scenario: `Instantiate`, then `Begin`, then `AddChild`, then read `Active` or `StepCount`. Both report idle and 0 steps even though a brew is queued.
   - Fix: document or assert that `Begin` needs the node in the tree, or make the properties reflect the pending state.
2. `scripts/dev/BrewSmoke.cs:46`. The smoke suite never covers `Begin` while a bar is running, `Begin(pastry)` after a drink, panel re-entry, or the real timer and token path. It always completes through `SkipPauseForTests`, and the iced-coffee case ends with the bar still running.
   - Scenario: a timer or token regression passes the smoke suite and only shows up in play.
   - Fix: add checks for `Begin(drink)` then `Begin(pastry)` with `StopNow`, and for remove then re-add of the panel.

## Final tally (merged)

Verdict: CHANGES REQUESTED. 0 Blocker, 3 Should fix, 6 Nit.
