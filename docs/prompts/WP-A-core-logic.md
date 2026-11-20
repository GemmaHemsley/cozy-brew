# WP-A: core logic (paste into a fresh session in worktree `../cozy-brew-wp-a`, branch `wp-a`)

```
Use the godot-builder agent to implement work package WP-A (core logic) of Little Bean Café, in this worktree on branch wp-a.

Read first: CLAUDE.md, docs/ARCHITECTURE.md, docs/CONTRACTS.md (section "WP-A" and the ownership table), docs/GAME_RULES.md, README.md (Commands).
Also read the source you build on: scripts/core/GameState.cs, scripts/data/*.cs, scripts/art/ArtKeys.cs, tests/CozyBrew.Tests/MenuTests.cs.

Goal: all the Godot-free game logic for the café loop, fully unit tested, plus the GameState additions listed in the contract.

You OWN (create/edit): scripts/cafe/logic/**, scripts/data/CustomerDef.cs, scripts/data/Roster.cs, scripts/data/ChatLines.cs,
  scripts/core/GameState.cs (only the additions in the contract), tests/CozyBrew.Tests/Cafe*Tests.cs, scripts/dev/CafeLogicSmoke.cs,
  docs/reports/WP-A.md.
You must NOT edit: anything else (project.godot, scenes, other scripts, CLAUDE.md, art/, other tests). Request changes in your report.

Build exactly the types in the contract, with the numbers and rules from docs/GAME_RULES.md (each number one named constant):
Mood, CustomerDef, Roster (the five regulars, art keys and patience multipliers from GAME_RULES), PatienceMeter, TipCalculator,
DayPlan (+CreateDefault), CustomerSpawner (deterministic per seed), CounterSlots, DaySummary, ChatLines (>=3 small-talk lines per regular,
>=2 order-line and >=2 thanks-line templates; in character: Juniper friendly regular, Sage studying, Milo rushed commuter, Rosie sweet tooth,
Theo night owl), and the GameState members (Plan, LastSummary, ChatPointsPerHeart, GetChatPoints, AddChatPoint, resets).
Files under scripts/cafe/logic and scripts/data must not `using Godot;`. GameState.cs is the only Godot-dependent file you touch.

Tests (xUnit, in tests/CozyBrew.Tests/Cafe*Tests.cs) must cover at least:
- TipCalculator: the worked examples in GAME_RULES, quality 0, quality 1, special bonus, rounding half away from zero, every mood.
- PatienceMeter: draining, Expired at 0, never negative, min 20 s total, mood/customer multipliers.
- CustomerSpawner: same seed => identical schedule; count formula incl. the cap of 20; all arrivals <= 90% of the day; sorted; >= 8 s apart;
  no regular twice in a row; pastry orders never exceed DayPlan stock; the special appears more often over many seeds; only available items ordered;
  day 1 never orders Matcha or Cake.
- DayPlan: capacity limit, TryTakePastry at 0, CreateDefault splits 12 evenly over 3 pastries on day 1 and over 4 on day 5.
- CounterSlots: fills to 3, TryOccupy returns -1 when full, Release frees, double release is harmless.
- DaySummary totals. ChatLines: never returns empty/null, deterministic for a seeded Random.
A GameState test is not possible in xUnit (it is a Godot Node); cover GameState additions in scripts/dev/CafeLogicSmoke.cs (an ISmokeSuite):
chat points -> hearts every 3, hearts cap at 5, Plan/LastSummary reset on NewGame and AdvanceDay.

Done when: `dotnet build` 0 errors/0 warnings; `dotnet test tests/CozyBrew.Tests` green; the headless smoke test prints SMOKE OK
(run `"$GODOT" --headless --path . --import` first in this fresh worktree). Write docs/reports/WP-A.md (format in the agent file),
commit on wp-a, do not merge. Reply with a short summary and the report path.
```
