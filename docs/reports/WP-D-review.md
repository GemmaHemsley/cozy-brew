# WP-D review (independent pass on wp-d @ fbc4f2b, plus uncommitted changes)

## Verdict: CHANGES REQUESTED (0 Blocker, 2 Should fix, 4 Nit)

`dotnet build`: 0 warnings, 0 errors. `dotnet test tests/CozyBrew.Tests`: 40 passed. I did not run the headless Godot smoke suite.

Checked: contract signatures against docs/CONTRACTS.md (all match). Ownership: the diff touches only scenes/brew, scripts/brew, the Brew tests, BrewSmoke and reports. Godot 4 API only. No `using Godot;` in scripts/brew/logic. GAME_RULES numbers: speeds, 0.04 + 0.06 x grind, 0.35 zero distance, 0.30..0.70 target and recipes all match. Palette-only colours, ArtCatalog use and theme variations are fine. Pixel snapping in `_Draw` is fine. Logic tests cover the edge cases: empty recipe, zero distance, symmetry and every ItemId.

Two things in the committed state are real defects. The working-tree changes fix both, so the practical request is to commit them (with the Nit 1 and Nit 2 changes below).

## Blocker
None.

## Should fix

1. Committed `scripts/brew/BrewPanel.cs:78-84,150-155`: the brew stalls if the panel leaves the tree during the feedback pause.
   - `_ExitTree` bumps `_pauseToken`. The pending 0.6 s timer then fails its token check and never calls `AdvanceStep`.
   - Nothing re-arms it when the panel is re-added.
   - Failure scenario: WP-E hides the brew by removing the panel while the player is on step 1 of a latte, for example on a screen swap or customer timeout. It re-adds the panel later and the bar never starts step 2. `BrewFinished` never fires and the order is stuck.
   - The smoke suite only tested re-add between steps (`BrewSmoke.cs:63-70`), so it does not catch this.
   - The uncommitted change (`_advancePending`, `_EnterTree`, `ScheduleAdvance`) fixes it, with a smoke test. It needs committing.

2. Committed `scripts/brew/BrewPanel.cs:116-118`: the panel is centred once, using `Size` at `Begin` time.
   - On the pending path (`Begin` before `_Ready`, `BrewPanel.cs:70-75`), `Size` is still (0,0) because layout has not run.
   - `(Vector2.Zero - _panel.Size) / 2` then puts the panel at a negative position, partly off the left and top of the screen. It is not re-centred when layout later resizes the root.
   - The uncommitted `Recenter` on `Resized` fixes this, with a smoke test.

## Nit

1. Committed `scripts/brew/TimingBar.cs:30,106`: `PerfectAccuracy = 0.98` means a stop just outside the green zone shows "Perfect!". The drawn zone is exactly accuracy 1.0, so the label disagrees with the picture. The uncommitted change sets 1 - 1e-9. The accompanying smoke assertion (`TimingBar.PerfectAccuracy > 0.99`) is trivial and only restates the constant.

2. Committed `TimingBar.cs:131-146`: a click or `confirm` on the frame `Begin` runs is accepted. If `confirm` started the brew (for example Z on a prep button), the same press can stop step 1 immediately. The uncommitted `_beginFrame` guard fixes this. Caveat: `Engine.GetProcessFrames()` plus `StopNow()` used by the smoke test bypasses the guard, so the guard itself is untested.

3. `TimingBar.cs:81-92,106`: `Begin` called before `_Ready` silently drops the label text (`_label` is null). `StopNow` before `_Ready` would throw a NullReferenceException on `_feedback.Text`. BrewPanel avoids this, but the class is meant to be reusable by Prep (WP-E). Store the label in a field and apply it in `_Ready`.

4. `BrewPanel.cs:184-191` (test hooks `RunPendingForTests`, `SkipPauseForTests`) and the uncommitted `AdvancePending` property are public production API that exists only for BrewSmoke. This is acceptable, but note it for WP-E. Also, `BrewSmoke` never asserts the real timer path (`StepPauseSeconds`) or a real input event.

## Uncommitted changes (BrewPanel.cs, TimingBar.cs, BrewSmoke.cs)
- Fix Should fix 1 and 2 and Nit 1 and 2 above.
- No new problems found: `_EnterTree` runs before `_Ready` on first entry, where `_advancePending` is false, so it is harmless. `Resized += Recenter` is a self-signal and needs no disconnect.
- The branch should not be merged with the working tree dirty. Commit these changes first.

## Note on commit state
The review above was done on fbc4f2b plus the then-uncommitted working-tree changes. Those changes were committed afterwards as 598f37e (WP-D round 2), so Should fix 1 and 2 and Nits 1 and 2 are resolved on the branch head. The second opinion below was run against the 598f37e state. No code was changed by this review.

## Second opinion (/code-review medium)
Findings the reviewer agent did not raise. Overlaps (Perfect threshold, trivial smoke assertion) are already covered above and omitted.

### Should fix
1. `scripts/brew/BrewPanel.cs:126-142`: `StartStep()` calls `Recenter()` straight after changing the step text. Godot defers minimum-size recalculation to the end of the frame, and `Resized` fires only when the BrewPanel root resizes, not the inner panel.
   - Failure scenario: step text or label width differs between steps (e.g. "Step 1/2" to a longer label). `ResetSize()` reads the stale minimum size and the panel sits off-centre until a resize that may never come.
   - Fix: `CallDeferred(nameof(Recenter))`, or connect to the inner panel's `MinimumSizeChanged`.
   - Not verified at runtime; the smoke test only checks centring after a manual `Size` assignment.

### Nit
1. `scripts/brew/TimingBar.cs:135,144`: the same-frame input guard also fires when `Begin` is called from the pause timer in `AdvanceStep`, not only from the starting click. Failure scenario: a double-tap of Z where the second press lands on the frame step 2 begins is swallowed and the bar keeps running without a response. Fix: ignore input only for the event that started the run, or only when `Begin` is called from an input handler.
2. `scripts/brew/BrewPanel.cs:83`: `_EnterTree` re-arms with a full `StepPauseSeconds`. Repeated reparenting during a pause restarts the pause each time (low likelihood). Consider storing the remaining time.
3. `scripts/dev/BrewSmoke.cs:89`: the centring assertions run right after `late.Size = ...` and assume `Resized` is emitted synchronously. The real case, where layout assigns the size on a later frame, is untested. Await a process frame before asserting.
4. `scripts/dev/BrewSmoke.cs:98`: the Perfect-threshold check only tests a constant. Drive `StopNow()` at accuracy of about 0.99 and assert the label reads "Good" (overlaps reviewer Nit 1's remark).
5. `scripts/brew/TimingBar.cs:30`: the second opinion questions the strict 1e-9 threshold, because the drawn zone is rounded to whole pixels and can differ from the logical zone by about 1 px. This is a judgement call and conflicts with reviewer Nit 1. Deciding the label from the logical half-width distance would avoid the mismatch.

## Final tally (combined)
Verdict: CHANGES REQUESTED. 0 Blocker, 3 Should fix (2 resolved in 598f37e, 1 new from second opinion), 8 Nit (4 reviewer, of which 2 resolved in 598f37e, plus 4 new from second opinion; the second opinion's Perfect-threshold items overlap and are not double-counted).
