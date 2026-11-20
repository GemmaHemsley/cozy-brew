# WP-D review (re-review after 79941eb)

## Verdict: APPROVE (0 Blocker, 0 Should fix, 4 Nit)

`dotnet build`: 0 warnings, 0 errors. `dotnet test tests/CozyBrew.Tests`: 40 passed. I did not run the headless Godot smoke suite.

Checked: contract signatures against docs/CONTRACTS.md (BrewStep, BrewRecipes, TimingBarConfig, TimingBarMath, TimingBar.Begin and Stopped, BrewPanel.Begin and BrewFinished). Ownership (only `scenes/brew`, `scripts/brew`, the Brew tests, BrewSmoke and reports). Godot 4 API only. No `using Godot;` in `scripts/brew/logic`. GAME_RULES numbers. Palette-only colours. ArtCatalog use. Theme variations. Pixel rules. Token and timer handling. Signal wiring.

## Earlier findings: status

- Should fix 1, re-Begin crash: RESOLVED. `BrewPanel.cs:97` calls `_bar.Cancel()` (`TimingBar.cs:71-78`), `BrewPanel.cs:140` guards `OnStepStopped` against `!_active` and an out-of-range index, and the smoke suite covers it (`BrewSmoke.cs:63-69`).
- Should fix 2, half-pixel panel: RESOLVED. `BrewPanel.cs:118` floors the centred position, and the smoke suite checks it (`BrewSmoke.cs:81`).
- Should fix 3, re-entry hang: RESOLVED. The `_ExitTree` unsubscribe is gone, with an explanatory comment (`BrewPanel.cs:81-83`), and the smoke suite covers remove and re-add (`BrewSmoke.cs:72-78`).
- Nit, unused `PanelWidth`: RESOLVED.
- Nit, idle `_Process`: RESOLVED. `SetProcess` is toggled (`TimingBar.cs:67,76,88,108,128`).
- Nit, marker and zone mapping mismatch: RESOLVED. Both use `1 + pos * inner` (`TimingBar.cs:159-160,168`).
- Nit, `_pendingItem` declared after first use: RESOLVED. It is now declared with the other fields (`BrewPanel.cs:34`).

## Blocker

None.

## Should fix

None.

## Nit

1. `scripts/brew/BrewPanel.cs:78-84,150-155`. `_ExitTree` bumps `_pauseToken`. If the panel is removed from the tree during the feedback pause and then re-added, the step's accuracy is already recorded but the pending timer is now a no-op. The panel sits with the step stopped, `Active` is true, and `BrewFinished` never fires.
   - Scenario: the café scene hides the panel by removing it mid-order.
   - Fix: leave `_pauseToken` alone in `_ExitTree` and let `IsInstanceValid` guard the timer. Or document that removing the panel mid-brew abandons it.
2. `scripts/brew/BrewPanel.cs:90-94`. If `Begin` is called before `_Ready`, only `_pendingItem` is set. `Active` and `StepCount` read idle and 0 until `_Ready` runs, and a second `Begin` overwrites the first. The class doc says to call `Begin` after adding the panel to the tree, so this is minor.
3. `scripts/brew/BrewPanel.cs:117-118`. The panel is centred using `Size` when `Begin` runs. If `Begin` runs in the same frame as the first `AddChild`, `Size` may not be laid out yet. The panel would then sit at (0,0) minus half its own size until the next `Begin`. The smoke test does not exercise this. Check it in a real window, or centre in `_Notification(NotificationResized)`.
4. `scripts/brew/BrewPanel.cs:181-191`. The test hooks `RunPendingForTests` and `SkipPauseForTests` are in production code. This is acceptable given the smoke harness. The smoke suite still never exercises the real timer path (`CreateTimer` with the token check), so a regression there would only show up in play.

## Second opinion (/code-review)

`/code-review medium` on `main..wp-d`. Findings the reviewer agent missed are listed first. Two findings overlap with the nits above but were rated higher, so they are listed too.

### Missed by the reviewer (all Nit)

1. `scripts/brew/TimingBar.cs:112`. `_UnhandledInput` stops the bar on any `confirm` press while running, and `_GuiInput` handles any mouse press on the bar.
   - Scenario: the player presses Z on a menu item and the handler calls `BrewPanel.Begin`. If the event is still unhandled, it reaches the new bar and stops step 1 at about position 0 as "Missed".
   - Fix: ignore input for one frame or a short grace period, or consume the triggering event.
2. `scripts/brew/TimingBar.cs:86`. The label says "Perfect!" for accuracy >= 0.98, but the drawn perfect zone is accuracy 1.0.
   - Scenario: the marker stops just outside the green zone, accuracy is about 0.98, and the text says "Perfect!".
   - Fix: use `accuracy >= 1.0` or an epsilon so the text matches the zone.
3. `scripts/brew/BrewPanel.cs:30`. The root has `MouseFilter = Ignore`, and only the inner `PanelContainer` stops mouse input.
   - Scenario: while a brew runs, the player clicks a customer or the counter outside the panel. The click reaches the café scene and starts another action mid-brew.
   - Fix: add a full-rect Stop-filter backdrop while `_active`.
4. `scripts/brew/BrewPanel.cs:143`. Only the Shot and Milk steps get their own SFX hook. Matcha, Cocoa and Ice fall back to `SfxId.Click`.
   - Scenario: brewing matcha plays a generic click on the matcha step.
   - Fix: add hook IDs, or note it as intentional.

### Overlaps with the reviewer's nits, rated higher by /code-review

- `scripts/brew/BrewPanel.cs:82` (reviewer Nit 1). `_ExitTree` bumps `_pauseToken`, and nothing re-arms the advance when the panel is re-added.
  - Scenario: the player stops the Shot step and the panel leaves the tree during the pause. The latte stays at Step 1/2 with a stopped bar and `BrewFinished` never fires.
  - /code-review rated this *important*. The reviewer rated it Nit.
  - Fix: cancel the brew in `_ExitTree` (`_bar.Cancel()`, `_active = false`), or re-arm in `_EnterTree`.
- `scripts/brew/BrewPanel.cs:117` (reviewer Nit 3). The panel is centred once in `Begin` from `Size`, which may still be (0,0) on the `_pendingItem` path before layout, and it is never re-centred.
  - Scenario: `Begin` runs from `_Ready` with `Size` (0,0). The panel lands at `-(panelSize/2)` and draws partly off-screen.
  - /code-review rated this *important*. The reviewer rated it Nit.
  - Fix: re-centre on `Resized` and after each `StartStep`, using `.Floor()`.

## Final tally (merged)

Verdict: APPROVE (the reviewer's verdict stands). 0 Blocker, 0 Should fix, 8 Nit.

Note for the human: /code-review rated two of the overlapping nits (pause-token stall, centring before layout) as important. Neither affects the current call paths, but both are real failure modes if the café scene reparents the panel or calls `Begin` early. Consider promoting them to Should fix before merging.
