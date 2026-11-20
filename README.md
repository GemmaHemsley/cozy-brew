# Little Bean Café

A cozy pixel-art café sim. Godot 4.7 (.NET / C#), 320x180 integer-scaled. Read [CLAUDE.md](CLAUDE.md) first: it is the source of truth for the game design, palette, art specs and code conventions.

## Docs
| File | What it is |
|---|---|
| [CLAUDE.md](CLAUDE.md) | Game design, palette, conventions (read by every agent) |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | MVP architecture: autoloads, scenes, scripts, flow, build status |
| [docs/CONTRACTS.md](docs/CONTRACTS.md) | The exact APIs work packages must provide/consume so parallel work fits together |
| [docs/GAME_RULES.md](docs/GAME_RULES.md) | Every tuning number (tips, patience, spawn waves, minigames) |
| [docs/AGENTS_GUIDE.md](docs/AGENTS_GUIDE.md) | How to use the agents and prompts in this repo |
| [docs/prompts/README.md](docs/prompts/README.md) | The runbook: which prompt to paste, in what order |
| [art/ART_MANIFEST.md](art/ART_MANIFEST.md) | Measured sizes and frames of every PNG |
| [art/DESIGN_REFERENCE.md](art/DESIGN_REFERENCE.md) | UI layouts from the design artifact |

## Commands
Run from the repo root. Set the Godot path once per terminal (your editor build lives in Downloads):

```sh
export GODOT="$HOME/Downloads/Godot_mono.app/Contents/MacOS/Godot"

dotnet build                                   # the game; must be 0 errors, 0 warnings
dotnet test tests/CozyBrew.Tests               # Godot-free logic tests (fast, no editor needed)
$GODOT --headless --path . --import            # (re)import assets; needed once in a fresh checkout/worktree
$GODOT --headless --path . res://scenes/dev/SmokeTest.tscn   # headless smoke test; exit code 0 = pass
$GODOT --path .                                # run the game (needs a display)
```

## Layout
```
art/        PNGs, fonts, ART_MANIFEST.md        scripts/    C# (core, data, art, ui, cafe, brew, dev)
scenes/     .tscn files (dev/ = test scenes)    theme/      cafe_theme.tres (project-wide UI theme)
audio/      empty (hooks only)                  tests/      xUnit project for Godot-free logic
docs/       design + agent docs                 .claude/    agent definitions
```

## Rules of thumb
- Godot 4 API only. All art through `ArtCatalog`. Colours from `Palette`. UI from `CafeTheme`.
- Logic that doesn't need Godot goes in Godot-free folders (`scripts/data`, `scripts/cafe/logic`, `scripts/brew/logic`) so it can be unit tested.
- Build only what the current prompt asks for; note ideas for later instead.
