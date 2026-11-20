# REVIEW: independent review of one work package (fresh session, same worktree as the builder)

Edit the first line, then paste everything inside the block.

```
Package: WP-A   Branch: wp-a   Base: main        (edit these; Base is `integration` for WP-E and WP-F)

Use the reviewer agent to review branch <Branch> against <Base> for package <Package> of Little Bean Café.
It must follow its checklist, read every changed file in full, and run `dotnet build` and `dotnet test tests/CozyBrew.Tests`.
It writes docs/reports/<Package>-review.md with a verdict (APPROVE or CHANGES REQUESTED) and findings grouped Blocker / Should fix / Nit,
each with file:line and a concrete failure scenario.

Additionally, in this session (not the subagent), run `/code-review medium` on the same diff and merge any finding the reviewer agent
missed into the review file under a heading "Second opinion (/code-review)". Do not change any code.
Finally commit only the report: `git add docs/reports/<Package>-review.md && git commit -m "docs: <Package> review report"`.
Reply with the verdict and the counts of Blockers / Should fix / Nits.
```

Why two reviewers: the `reviewer` agent knows this repo's rules (contracts, ownership, pixel and palette rules); `/code-review` is a general bug hunt. They overlap little.
