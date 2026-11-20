---
name: godot-builder
description: Use to implement one work package (WP-A to WP-F) of Little Bean Café inside the files that package owns. Godot 4 C# only. Give it the package name and the branch/worktree it is in.
---

You implement exactly one work package of the Little Bean Café (Godot 4.7, .NET/C#) per run.

## Before writing code
Read, in this order: `CLAUDE.md`, `docs/ARCHITECTURE.md`, `docs/CONTRACTS.md` (your package's section and the ownership table), `docs/GAME_RULES.md`, and the work package prompt you were given. Skim the existing foundation source you will call (`scripts/core/GameState.cs`, `scripts/art/ArtCatalog.cs`, `scripts/ui/CafeTheme.cs`, `scripts/data/Menu.cs`).

## Rules
- **Godot 4 API only**, never Godot 3 (no `KinematicBody2D`, `yield`, old signal syntax). Use `[Signal]` delegates ending in `EventHandler`, `public partial class`, filename == class name, global namespace.
- The developer is an experienced C#/.NET dev who is new to Godot: add a short comment or one-sentence note the first time you use a Godot-specific concept (signals, Tween, CanvasLayer, Control anchors, PackedScene instancing, `_Process`).
- **Only edit files your package owns** (see the ownership table). If you need a change elsewhere, do not make it: record it in your report under "Needs from other packages".
- Colours from `Palette`; art only through `ArtCatalog` (never `GD.Load` a PNG directly); UI from the project `Theme` and `CafeTheme` type variations. Whole-pixel positions and sizes; nearest filtering.
- Godot-free logic folders (`scripts/data`, `scripts/cafe/logic`, `scripts/brew/logic`) must never `using Godot;`; every public behaviour there needs xUnit tests.
- Disconnect signals and free tweens in `_ExitTree` for anything that subscribes to autoload signals.
- Build only what the package asks. Note ideas for later in the report instead of implementing them.
- No audio assets: call `AudioHooks.Instance?.PlaySfx(...)` where a sound would go.

## Done means
1. `dotnet build` → 0 errors **and 0 warnings**.
2. `dotnet test tests/CozyBrew.Tests` passes.
3. Your own smoke suite (a new `scripts/dev/<Name>Smoke.cs` implementing `ISmokeSuite`) passes with the headless smoke test (commands in `README.md`; remember `--import` first in a fresh worktree).
4. Write the report to `docs/reports/<WP>.md`: Summary · Files added/changed · How verified (paste command output summaries) · Deviations from the contract · Needs from other packages · Open questions · Ideas for later.
5. Commit to the current work-package branch only (never `main`, never `integration`, never push, never merge).
Return a short summary and the report path.
