# Runbook: which prompt to paste, in what order

Every prompt in this folder is **self-contained**: paste it into a fresh `claude` session (after `/clear`) and it reads the docs it needs. Background on agents, worktrees and `/clear`: [docs/AGENTS_GUIDE.md](../AGENTS_GUIDE.md).

Nothing here merges to `main` without you. Only the integration branch gets merged into, and you decide when it goes to `main`.

## The cycle for every work package
```
1. BUILD    paste WP-X prompt           (session in the WP's worktree)   → code + docs/reports/WP-X.md, committed on branch wp-x
2. /clear
3. REVIEW   paste REVIEW.md             (same worktree)                  → docs/reports/WP-X-review.md   (APPROVE / CHANGES REQUESTED)
4. if CHANGES REQUESTED:  /clear, paste FIX snippet (below), then go back to step 3
5. /clear
6. VERIFY   paste VERIFY.md             (same worktree)                  → docs/reports/WP-X-verify.md   (VERIFIED / NOT VERIFIED)
7. when APPROVE + VERIFIED for every package in the wave: MERGE.md (in the main checkout)
8. DOCS-REFRESH.md, then playtest, then tell me what felt wrong
```
`REVIEW.md`, `VERIFY.md` and the FIX snippet have a first line you edit: the package letter and branch.

## Wave 0 (done in this repo)
Docs, agents, test scaffold, smoke test, input actions. Commit `main` before starting wave 1 (worktrees branch from the current commit).

## Wave 1: A, B, C, D can all run at the same time (disjoint files)
Open four terminals. In each, from the main checkout (`~/cozy-brew`):
```sh
# A
git worktree add ../cozy-brew-wp-a -b wp-a && cd ../cozy-brew-wp-a
# B:  ../cozy-brew-wp-b / wp-b      C: ../cozy-brew-wp-c / wp-c      D: ../cozy-brew-wp-d / wp-d
export GODOT="$HOME/Downloads/Godot_mono.app/Contents/MacOS/Godot"
dotnet build && "$GODOT" --headless --path . --import      # one-time setup per worktree
claude
```
Then paste the package prompt: [WP-A](WP-A-core-logic.md), [WP-B](WP-B-screen-shell.md), [WP-C](WP-C-cafe-world.md), [WP-D](WP-D-brew.md). Run the cycle above in each worktree. Tips:
- Running all four at once is faster but you review four things at once; two at a time is a fine pace.
- A and D contain pure logic with tests and are the least risky. B and C produce scenes you can open in the Godot editor (open the worktree folder as a project) to eyeball.

**Merge wave 1** (main checkout, once all four are APPROVE + VERIFIED): paste [MERGE.md](MERGE.md) with the list `wp-a wp-b wp-c wp-d`. It creates the `integration` branch and merges in that order, running build, tests and smoke after each. Then [DOCS-REFRESH.md](DOCS-REFRESH.md). **Playtest checkpoint:** run the game; you should reach Title → Prep → Open stub → Close stub → Prep. Open `scenes/dev/RoomPreview.tscn` and the brew scene to eyeball the views.

## Wave 2: E (integration, one agent)
```sh
git worktree add ../cozy-brew-wp-e -b wp-e integration && cd ../cozy-brew-wp-e
```
Paste [WP-E](WP-E-integration.md), then the cycle. Merge with MERGE.md (`wp-e`), DOCS-REFRESH. **Playtest:** play a full day to Close and into day 2.

## Wave 3: F (tuning and polish)
```sh
git worktree add ../cozy-brew-wp-f -b wp-f integration && cd ../cozy-brew-wp-f
```
Paste [WP-F](WP-F-polish.md); it needs your playtest notes pasted in at the marked spot. Cycle, merge, DOCS-REFRESH. When you're happy, merge `integration` into `main` yourself (or ask a session to).

## FIX snippet (paste after /clear in the same worktree)
```
Package: WP-X   Branch: wp-x   (edit these two)
Use the godot-builder agent. Read docs/reports/WP-X.md and docs/reports/WP-X-review.md. Fix every Blocker and every
"Should fix" finding in the review; leave Nits unless trivial. Do not widen scope or edit files you don't own.
Rerun build (0 warnings), dotnet test and the smoke test. Append a "Review fixes" section to docs/reports/WP-X.md
listing each finding and how it was fixed (or why it was not). Commit on wp-x. Do not merge.
```

## If something goes wrong
- *Build fails in a new worktree:* run `dotnet build` once; check `$GODOT` points at your editor build.
- *Smoke test prints `Could not find type` or old script errors:* run the `--import` command, then `dotnet build`, then retry.
- *An agent edited a file it doesn't own:* ask the reviewer to flag it, then have the builder revert that file (`git checkout main -- <file>`) and note the request in the report.
- *Two packages disagree about a contract:* edit `docs/CONTRACTS.md` on `main` first, then rebase the branches (`git rebase main`) or tell the agents.
- *Clean up when finished:* `git worktree remove ../cozy-brew-wp-a` and `git branch -d wp-a`.
