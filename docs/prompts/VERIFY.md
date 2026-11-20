# VERIFY: automated verification of one work package (fresh session, same worktree)

Edit the first line, then paste everything inside the block.

```
Package: WP-A   Branch: wp-a        (edit these)

Use the qa-verifier agent in this worktree for package <Package>: confirm the current branch is <Branch> and the working tree is clean
(`git status --short` empty except for docs/reports files). Run its full sequence: dotnet build, dotnet test tests/CozyBrew.Tests,
Godot --import, the headless smoke test, and report any stray files. It writes docs/reports/<Package>-verify.md with PASS/FAIL per step
and a verdict VERIFIED or NOT VERIFIED. Do not fix anything. Reply with the verdict and the key numbers (tests passed, smoke checks passed, warnings).
```
