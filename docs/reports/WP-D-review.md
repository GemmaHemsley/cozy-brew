# WP-D review (fresh pass on wp-d @ 5134a00)

## Verdict: APPROVE (0 Blocker, 0 Should fix, 5 Nit)

`dotnet build`: 0 warnings, 0 errors. `dotnet test tests/CozyBrew.Tests`: 40 passed. I did not run the headless Godot smoke suite because `godot` is not on PATH here. I read `BrewSmoke.cs` by hand and it looks consistent with the code.

Checked:
- Contract signatures in `docs/CONTRACTS.md` match exactly: `BrewStep`, `BrewRecipes.StepsFor`, `TimingBarConfig`, every `TimingBarMath` method, `TimingBar.Stopped` and `Begin`, `BrewPanel.BrewFinished` and `Begin`.
- Scope: code changes are only under `scripts/brew`, `scenes/brew`, `scripts/dev/BrewSmoke.cs` and `tests/.../BrewLogicTests.cs`. Neither view references WP-A types. `scripts/brew/logic` has no `using Godot;`.
- GAME_RULES numbers all match: speeds, half width 0.04 + 0.06 x grind, zero distance 0.35, target 0.30..0.70, recipes, mean combine and empty list returning 1.0.
- Godot 4 API only. Conventions hold: `public partial class`, filename equals class name, global namespace, `[Signal]` delegates, Godot concepts explained in comments.
- Pixels and palette: `_Draw` snaps to whole pixels (`TimingBar.cs:164-176`). The panel is floored when centred (`BrewPanel.cs:139`). Only Palette colours are used, with no raw hex. Art comes via `ArtCatalog.Get`. The theme variations `HeadingLabel` and `SecondaryLabel` are used. There are no ad-hoc StyleBoxes.
- Robustness:
  - `AudioHooks.Instance?` is null-safe (`BrewPanel.cs:164`).
  - There are no autoload signal connections to disconnect.
  - Timer callbacks are guarded by a token plus `IsInstanceValid` and `IsInsideTree` (`BrewPanel.cs:178-183`).
  - A re-Begin cancels the bar and invalidates the pause (`BrewPanel.cs:109-111`).
  - A deferred pastry finish is dropped if Begin is called again (`FinishPending` guard).
  - `_Process` is switched off while idle (`TimingBar.cs:132`).
  - Re-centring is hooked to both `Resized` and the inner panel's `MinimumSizeChanged` (`BrewPanel.cs:74-75`). The stale finding from the previous review (a missing `MinimumSizeChanged` subscription) is fixed.
  - `confirm` is ignored while the panel is hidden (`IsVisibleInTree`) and on the frame the run started.
- Smoke registration: `ISmokeSuite` suites are discovered by reflection (`SmokeTest.cs:16`), so `BrewSmoke` needs no registration.

## Blocker
None.

## Should fix
None.

## Nit

1. `tests/CozyBrew.Tests/BrewLogicTests.cs`: edge cases in Godot-free code are not covered.
   - `TimingBarMath.Accuracy` with `zeroDistance <= perfectHalfWidth` (`TimingBarMath.cs:33-34`) has no test. A regression there would not be caught.
   - `Combine(null)` (`TimingBarMath.cs:58`) has no test.
   - `Position` with negative elapsed time has no test.
   - The `ConfigFor` default branch is unreachable with the current enum, so it is not worth testing.
   - Failure scenario: someone changes the degenerate branch to divide by zero and every test stays green.

2. `scripts/brew/BrewPanel.cs:214,217`: `RunPendingForTests` and `SkipPauseForTests` are public API on a production class. They are documented as test hooks and the prompt asked for something similar, so this is acceptable. Failure scenario: WP-E calls `SkipPauseForTests` by mistake and skips the feedback pause.

3. `scripts/dev/BrewSmoke.cs:100`: `TimingBar.PerfectAccuracy > 0.99` only tests a constant. It cannot catch a mislabelled "Perfect!" on a near-miss. It would be more meaningful to drive `StopNow` at a known marker position.

4. `scripts/brew/TimingBar.cs:56`: the top label has no `ThemeTypeVariation`. It uses the default Label look, while the feedback label uses `SecondaryLabel`. This is probably intentional (primary text is Espresso). The report could say so.

5. `docs/reports/WP-D-review.md` is committed on the branch. It was written by earlier review rounds and lies outside the WP-D owned file list (`docs/reports/WP-D.md` is the owned one). This file replaces it. Decide at merge whether review reports should live on the feature branch.

## Second opinion (/code-review)

`/code-review medium` on `main...wp-d` found no Blockers or Should fix items. It raised five Nits that the reviewer agent did not list. The agent's own five Nits above were not repeated by `/code-review`.

6. `scripts/brew/TimingBar.cs:177`: the marker is drawn off the scored position near the bar ends. The marker centre is `1 + pos*inner`, but x is clamped to `[1, BarWidth-1-MarkerWidth]`. At pos near 0 or 1 the sprite sits up to about 1.5 px inside the true position. `Math.Round` also uses banker's rounding on the .5 values this offset creates. Fix: clamp the centre rather than the left edge, or use `MidpointRounding.AwayFromZero`. Failure scenario: the marker is stopped at 0.0 or 1.0 and the sprite is 1-2 px off the scored position. The effect is small because targets lie in 0.30..0.70.

7. `scripts/brew/BrewPanel.cs:71`: the panel is modal for the mouse only. `MouseFilter = Stop` blocks clicks, but keyboard focus and `confirm` (Z/Enter) still reach other UI. Failure scenario: a focused HUD button is on screen when a brew starts and the player presses Z. Either the button consumes the event and the bar does not stop, or the bar takes it first, depending on node order. Fix: take focus while brewing, or handle `confirm` in BrewPanel and mark it handled.

8. `scripts/brew/BrewPanel.cs:87`: `Begin` does not validate its inputs. A null `item` throws a NullReferenceException on `item.Id`. A `grindQuality` outside 0.5..1 goes straight into `ConfigFor`, so the perfect zone can become a sliver or huge. Fix: guard `item == null` and `Math.Clamp(grindQuality, 0.5, 1.0)`.

9. `scripts/brew/BrewPanel.cs:177`: `OnStepStopped` only guards `_stepIndex >= _steps.Count`. The accuracy bookkeeping relies on `_accuracies.Count == _stepIndex`, which holds only because `Begin` cancels the bar and bumps the token. Failure scenario: a future path restarts a step without resetting `_accuracies`. The mean then includes stale entries and the tip is wrong. Fix: assert or guard the invariant before adding.

10. `scripts/brew/BrewPanel.cs:45`: `_Ready` forces `MouseFilter = Ignore` and `SetAnchorsPreset(FullRect)` in code, which overrides the scene. Failure scenario: BrewPanel is parented under a Container, which ignores the anchors, so `Recenter` centres the panel in the wrong box and the brew UI appears off-centre. Fix: document that it needs a non-Container parent, or set the anchors in `BrewPanel.tscn`.

## Totals (merged)

Verdict: APPROVE. 0 Blocker, 0 Should fix, 10 Nit (5 from the reviewer agent, 5 from the second opinion).
