# WP-E: integration (wave 2; worktree `../cozy-brew-wp-e`, branch `wp-e`, created from `integration`)

```
Use the godot-builder agent to implement work package WP-E (integration) of Little Bean Café, in this worktree on branch wp-e.
Wave 1 (WP-A to WP-D) is already merged into this branch's base. Confirm that first: `git log --oneline` should show those merges
and `dotnet build`, `dotnet test`, and the smoke test should already pass. If not, stop and tell me.

Read first: CLAUDE.md, docs/ARCHITECTURE.md, docs/CONTRACTS.md (all of it), docs/GAME_RULES.md (all of it), art/DESIGN_REFERENCE.md,
docs/reports/WP-A.md, WP-B.md, WP-C.md, WP-D.md (note every "Deviations" and "Needs from other packages" entry), README.md (Commands).
Then read the actual source of every contract type you will call; the code is the truth if it differs from the doc.

Goal: a fully playable day. Wire the packages together; add no new systems.

You OWN: scripts/screens/** (CafeScreen, PrepScreen, CloseScreen, TitleScreen tweaks), scenes/Open.tscn, Prep.tscn, Close.tscn, scenes/ui additions you need,
  scripts/dev/LoopSmoke.cs, tests for any new Godot-free logic you add, docs/CONTRACTS.md (record any contract change you had to make, with reason),
  docs/reports/WP-E.md. You may make the small fixes to other packages' files that integration strictly requires; list each in the report under
  "Touched outside my files" with the reason.

Build:
1. CafeScreen (Open.tscn): instance CafeRoom, Barista, Biscuit, Hud, DialogueBox, BrewPanel (hidden), an OrderTicket strip for active orders.
   _Ready: GameState.StartDay(); build the schedule with CustomerSpawner(seed from day) using GameState.Plan and GameState.UnlockedItems;
   run the per-customer state machine from docs/ARCHITECTURE.md: Arriving (WalkTo a free CounterSlots slot; wait at the door if full) -> Waiting
   (PatienceMeter ticks; Customer.SetPatience) -> Ordering (click the customer: DialogueBox shows ChatLines.OrderLine, "Start brewing" enabled)
   -> Brewing (BrewPanel.Begin(item, plan.GrindQuality); pastries skip via zero steps and decrement stock with DayPlan.TryTakePastry)
   -> Served (TipCalculator, GameState.AddCoins(total), DaySummary.RecordServed, ThanksLine, coin SFX hook)
   -> optional Chat (once per visit: ChatLines.SmallTalk, GameState.AddChatPoint) -> Leaving (Customer.Leave then free the node, release the slot).
   Patience hitting 0 => RecordWalkout, customer leaves, slot freed. Pause GameState clock only if you decide to (default: clock keeps running during brewing).
   End of day: after GameState.DayTimeUp and no customers remain, compute NewUnlocksTomorrow, set GameState.LastSummary, GoTo(Close).
   Several customers may be present; the player picks which to serve by clicking them. Keyboard: arrows move a focus marker between customers, confirm acts.
2. PrepScreen: grind beans (one TimingBar, GrindQuality = 0.5 + 0.5*accuracy, skippable), pastry case (+/- buttons per unlocked pastry, capacity 12, shows remaining),
   special picker (cycle through unlocked items with arrow buttons), "Open shop" writes GameState.Plan and goes to Open.
3. CloseScreen: summary panel from GameState.LastSummary (served, walked out, sales, tips, total, coins now), "New tomorrow: ..." line, "Next day".
4. Title: unchanged except fixes you need. Never leave a screen with no focusable control.
5. Everything via the project theme; no new colours; missing art never crashes.

Tests: any new Godot-free helper gets xUnit tests. Smoke suite scripts/dev/LoopSmoke.cs: instantiate Open.tscn with a short test day
(set GameState.DayLengthSeconds small, e.g. 12) and a scripted driver that serves every customer; assert DaySummary totals equal GameState.Coins gained,
no exceptions, and that the scene reaches the Close screen. Also instantiate Prep and Close.

Done when: build 0/0, dotnet test green, headless smoke SMOKE OK (--import first in a fresh worktree). Describe the full manual test path I should follow
(click by click) in the report. Write docs/reports/WP-E.md, commit on wp-e, do not merge. Reply with a short summary and the report path.
```
