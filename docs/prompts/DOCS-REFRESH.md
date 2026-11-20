# DOCS-REFRESH: bring docs back in line after a merge (fresh session in the MAIN checkout, on `integration`)

```
Use the docs-keeper agent on the `integration` branch of Little Bean Café (check `git branch --show-current`).
Compare the docs to the merged code and reports in docs/reports/, then update the Build status table in docs/ARCHITECTURE.md,
docs/CONTRACTS.md (only for legitimate deviations, with reasons), docs/GAME_RULES.md (if constants changed; the code is the truth),
art/ART_MANIFEST.md (if art changed) and README.md. It must not edit CLAUDE.md or any code: it lists proposed CLAUDE.md changes
and any doc-vs-code mismatches it left alone. Show me the list of edits and the proposals. Commit the doc edits on `integration`
with a message starting "docs:".
```
