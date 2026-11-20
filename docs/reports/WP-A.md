# WP-A report: core logic

## Summary
All Godot-free café logic from the contract, with 67 xUnit tests, plus the `GameState` additions and a smoke suite. Numbers come from `docs/GAME_RULES.md`, each as a named constant (`MoodRules`, `PatienceMeter`, `TipCalculator`, `CustomerSpawner`, `DayPlan`).

## Files added/changed
- Added `scripts/cafe/logic/`: `Mood.cs` (enum + `MoodRules` weights/multipliers), `PatienceMeter.cs`, `TipCalculator.cs` (+`TipResult`), `DayPlan.cs`, `CustomerSpawner.cs` (+`Arrival`), `CounterSlots.cs`, `DaySummary.cs`.
- Added `scripts/data/`: `CustomerDef.cs`, `Roster.cs`, `ChatLines.cs`.
- Changed `scripts/core/GameState.cs`: `Plan`, `LastSummary`, `ChatPointsPerHeart`, `GetChatPoints`, `AddChatPoint`; reset in `NewGame` (via `ResetState`) and `AdvanceDay`.
- Added tests `tests/CozyBrew.Tests/Cafe{Money,DayPlan,Spawner}Tests.cs` (Money file also holds patience, summary, counter slots and chat-lines tests); `scripts/dev/CafeLogicSmoke.cs`.

## How verified
- `dotnet build`: 0 errors, 0 warnings.
- `dotnet test tests/CozyBrew.Tests`: Passed 67, Failed 0.
- `$GODOT --headless --path . --import` then `res://scenes/dev/SmokeTest.tscn`: `SMOKE OK (23 checks)`, exit 0 (includes CafeLogic suite: chat points to hearts every 3, cap at 5, Plan/LastSummary reset on NewGame and AdvanceDay).

## Deviations from the contract
- `ChatPointsPerHeart` is `public const int` on `GameState` (as specified). `GetChatPoints` returns the remainder towards the next heart (0..2), not a lifetime total. At max hearts points keep wrapping harmlessly.
- `LastSummary` is reset to a fresh empty `DaySummary` (never null) on NewGame/AdvanceDay. WP-E must read/store the finished day's summary before calling `AdvanceDay`.
- `CustomerSpawner.BuildSchedule` creates its `Random` per call from `seed` and `day` (plain arithmetic), so repeated calls on one spawner give the same schedule, and different days differ.
- `PastryWeight` is applied to the 0.3 pastry-kind weight (Rosie: 0.6 vs 0.7 drink); the special's x2 applies within the chosen kind.
- Min 8 s gap: forward pass, then backward pass from the 90% limit if the last arrival would overshoot. Only violable for absurdly short day lengths.
- `DayPlan.TrySetStock` also returns false for non-pastry items. `DayPlan.GrindQuality` setter clamps to 0.5..1. Extra public `DayPlan.TotalStock`, `CustomerSpawner.CustomerCount(day)`.
- `BuildSchedule` throws if nothing at all is orderable (no drinks and no pastry stock).

## Needs from other packages
- WP-E: record `LastSummary` and fill `NewUnlocksTomorrow` (e.g. `Menu.AvailableOnDay(Day+1)` minus `AvailableOnDay(Day)`) before `AdvanceDay`; call `Plan.TryTakePastry` when serving a pastry.

## Open questions
- Is the sprite-to-name mapping in `Roster` right (still the guess from CLAUDE.md)?

## Ideas for later
- Mood-specific small talk; friendship-gated lines (hearts) and "usual" orders; weight regulars by hearts.
