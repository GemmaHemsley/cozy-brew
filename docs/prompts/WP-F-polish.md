# WP-F: tuning and polish (wave 3; worktree `../cozy-brew-wp-f`, branch `wp-f`, created from `integration`)

```
Use the godot-builder agent to implement work package WP-F (tuning and polish) of Little Bean Café, in this worktree on branch wp-f.
Waves 1 and 2 are merged into this branch's base; confirm build, tests and smoke already pass before starting.

Read first: CLAUDE.md, docs/ARCHITECTURE.md, docs/CONTRACTS.md, docs/GAME_RULES.md, all docs/reports/*.md, README.md (Commands).

My playtest notes (the human fills this in before pasting; if empty, skip to the checklist):
<<< PASTE PLAYTEST NOTES HERE: what felt too slow/fast, hard to read, broken, ugly, confusing >>>

Goal: make the existing loop feel good and robust. No new features.

You OWN: tuning constants (keep them in their single named constants and update docs/GAME_RULES.md to match), small UI/visual fixes anywhere,
  tests, docs/reports/WP-F.md. Touching any file is allowed here, but keep each change minimal and listed in the report.

Do, in this order:
1. Address each playtest note: reproduce or explain; fix or write why not.
2. Economy sanity test: add a Godot-free simulation test (tests/CozyBrew.Tests/Cafe*SimulationTests.cs) that plays 5 days with scripted
   average-skill players (accuracy 0.4, 0.7, 0.95) over the real spawner and TipCalculator. Assert a day-1 average-skill day earns roughly 20-45 coins
   total, earnings grow with day number, and a bad player (accuracy 0.4, patience 0.3) still earns > 0 (no fail state). If the numbers are off, adjust
   constants and record before/after in the report.
3. Robustness: temporarily rename one art PNG and one font file, run the smoke test and the loop smoke: nothing may throw (expect only WARNING lines); restore the files and confirm git status shows no change.
   Rapid-click and double-activation checks on every button (no double scene change, no double serve). Kill tweens on scene exit. Ensure no signal leaks (GameState signals disconnected on exit).
4. Legibility: confirm every label uses CafeTheme sizes (8 px body); list any text that overflows its panel at 320x180 and fix with layout, not smaller fonts.
5. Keyboard-only pass: Title -> Prep -> Open -> Close works with arrows + confirm/cancel; focus never gets lost.
6. Tidy: remove dead code, make sure scripts/dev smoke suites cover each screen, update README/ARCHITECTURE only for facts that changed.

Done when: build 0/0, dotnet test green, headless smoke SMOKE OK. Write docs/reports/WP-F.md with a before/after table of every tuning constant changed.
Commit on wp-f, do not merge. Reply with a short summary and the report path.
```
