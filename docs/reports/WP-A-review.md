# WP-A review

## Verdict: APPROVE

`dotnet build` gives 0 warnings and 0 errors. `dotnet test` passes 67 of 67. I did not run the headless smoke test. I checked that `SmokeTest` discovers `CafeLogicSmoke` by reflection, and I read its assertions against `GameState`.

Scope is clean. Only owned files changed (plus the `.uid` files and `docs/reports/WP-A.md`). The contract signatures match. There is no `using Godot;` in `scripts/cafe` or `scripts/data`. Numbers match GAME_RULES and each lives in a named constant. `GameState.ResetDay` runs after `Day` is set and after `RefreshUnlocks`, so day 5 includes Cake.

## Blocker
None.

## Should fix
None strictly required. The first nit below is the one most worth fixing before WP-E.

## Nit
1. `scripts/data/ChatLines.cs:9-12,58`. Templates hard-code "a {item}". Espresso gives "A espresso, please." and Iced coffee gives "a iced coffee". A player sees these on day 1. Fix with an article helper (a/an by first vowel) or by rewording the templates ("One {item}, please").
2. `scripts/data/ChatLines.cs:57,60,62`. The `CustomerDef c` parameter is unused in `OrderLine` and `ThanksLine`, so order and thanks lines are not in character per regular. The contract only requires small talk to be in character, so this is acceptable. It is worth noting for WP-F.
3. `scripts/cafe/logic/CustomerSpawner.cs:67-77`. For a very short `dayLengthSeconds`, the backward pass can produce negative times. The report acknowledges the 8 s gap can be violated here, but negative times are not mentioned. This cannot happen at the 360 s day. Example: dayLength 100 with 8 customers gives a limit of 90 s and a span of 56 s, which is fine. dayLength 40 with 20 customers goes negative.
4. `scripts/cafe/logic/CustomerSpawner.cs:96`. Rosie's x2 is applied to the 0.3 kind weight (0.6 against 0.7 for drinks). GAME_RULES says "pastry orders x2 weight", which is ambiguous, and the report documents the choice. The test only asserts that Rosie orders more pastries, not by how much.
5. `scripts/core/GameState.cs:243-253`. At max hearts, chat points keep wrapping and `AddHearts` is a no-op. This is harmless and documented. `GetChatPoints` returns the remainder, not the lifetime total, which is consistent with the report.
6. `tests/CozyBrew.Tests/CafeMoneyTests.cs` holds the patience, summary, slots and chat-lines tests in one file. This is allowed by the `Cafe*Tests.cs` pattern but makes the file harder to find tests in.

## Checked
- Tip rounding uses `MidpointRounding.AwayFromZero`. The worked examples are covered: Latte Cheerful gives 4, Calm gives 3, quality 0 gives 0, and Cookie at quality 0.5 gives 1.
- `PatienceMeter` clamps `Remaining` to 0..1 and never goes negative. The 20 s minimum is applied in `TotalSeconds`.
- `CustomerSpawner` is deterministic through per-call `Random(seed*7919+day)`. The count formula and the cap of 20 are tested. Arrivals stay within 90% of the day and at least 8 s apart over 200 seeds. The same regular never appears twice in a row. Pastry orders never exceed stock, and day 1 orders no Matcha or Cake.
- `DayPlan` capacity, `TryTakePastry` at 0, and the 4/4/4 and 3/3/3/3 default splits are tested.
- `CounterSlots.Release` ignores out-of-range and already-free slots.
- No debug prints, no secrets, no dead code.

## Second opinion (/code-review)
Run in the main session with `/code-review medium` on `main..wp-a`. It returned 7 findings. Two repeat reviewer nits (ChatLines article, negative SpaceOut times). The rest are listed below. I checked each against the code. None changes the verdict (APPROVE), and I counted all of them as Nits.

1. **Nit** `scripts/cafe/logic/DayPlan.cs:12,31`. `Special` has no validation and `TrySetStock` accepts pastries that are not unlocked yet. GAME_RULES says the special must be an unlocked item. Scenario: on day 1, `plan.Special = ItemId.Cake` or `TrySetStock(ItemId.Cake, 6)` succeeds. Cake stock then uses up case capacity, but the spawner ignores it because Cake is not available. Confirmed in code. Validate in the UI (WP-E) or give `DayPlan` the unlocked list.
2. **Nit** `scripts/core/GameState.cs:54,57`. `Plan` and `LastSummary` have public setters, and `NewGame`/`AdvanceDay` replace the instances (line 131-132). Scenario: a screen that caches `gs.Plan` across a day change writes to a stale object. A null assignment would crash the spawner. Consider `private set` and tell screens to read `GameState.Plan` each time.
3. **Nit** `scripts/cafe/logic/TipCalculator.cs:17`. The +1 special bonus is applied even at quality 0, and `isSpecial` comes from the caller, not from `DayPlan`. Scenario: a special served at 0 patience and 0 accuracy tips 1. The Checked section confirms that "quality 0 gives 0", but only for non-specials. Check that this matches the intent in GAME_RULES.
4. **Nit** `scripts/cafe/logic/CustomerSpawner.cs:27,74`. Related to nit 3 above. Absolute 8 s spacing against fractional wave windows means that on a short configured day, waves smear into one stream, as well as going negative. This does not happen at 360 s. Only matters if day length gets tuned much lower.

Dismissed: "pastry stock is decremented when the schedule is built". The spawner copies stock into a local dictionary (`CustomerSpawner.cs:49`) and does not change `DayPlan`, so the plan's stock is untouched. Walk-outs still count against the pre-allocated stock, which is a design choice, not a bug.

Totals after merge: Blocker 0, Should fix 0, Nit 10 (6 from the reviewer, 4 added here).
