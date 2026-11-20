# MERGE: bring approved packages into `integration` (fresh session in the MAIN checkout `~/cozy-brew`)

Edit the first line, then paste everything inside the block. Only merge packages whose review says APPROVE and whose verify says VERIFIED.

```
Packages to merge, in this order: wp-a wp-b wp-c wp-d        (edit: e.g. just `wp-e` in wave 2)

You are merging approved work-package branches of Little Bean Café into the `integration` branch. Do NOT touch `main`. Do NOT push.
1. `git status` must be clean. If the `integration` branch does not exist, create it from `main` (`git switch -c integration main`); otherwise `git switch integration`.
2. For each branch in the order given: confirm docs/reports/<WP>-review.md says APPROVE and docs/reports/<WP>-verify.md says VERIFIED on that branch
   (`git show <branch>:docs/reports/<file>`). If either is missing or negative, stop and tell me.
3. `git merge --no-ff <branch> -m "Merge <branch>: <package name>"`. If there is a conflict, do NOT resolve it by guessing: abort the merge
   (`git merge --abort`), show me the conflicting files and both sides, and stop. (Conflicts mean two packages touched the same file; the ownership table in docs/CONTRACTS.md says who should have.)
4. After EACH merge run: `dotnet build` (0 errors, 0 warnings), `dotnet test tests/CozyBrew.Tests`, `"$GODOT" --headless --path . --import`, and the headless smoke test
   (commands in README.md; export GODOT first). If anything fails, stop and report the exact output; do not continue to the next branch and do not "fix" code.
5. When all are merged and green, print `git log --oneline --graph -15` and a table: branch, merged yes/no, build, tests, smoke.
Reply with that table. Leave the worktrees in place (I may need them for fixes).
```
