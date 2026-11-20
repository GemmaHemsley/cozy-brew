# Using agents in this repo

A plain-language guide so you can keep running this workflow, and change it, without me.

## Three different things people call "agents"
| Thing | What it is | Where it lives | Context |
|---|---|---|---|
| **A session** | One `claude` conversation in one terminal. You type, it works. | your terminal | its own context window |
| **A subagent** | A helper the *current session* hands a job to. It works in its **own fresh context**, then returns a summary. | defined by a markdown file in `.claude/agents/` | separate from the session; the session only sees the summary |
| **A prompt file** | Text you paste into a session. Not special software. | `docs/prompts/*.md` | n/a |

Why separate contexts matter: a long build fills a session's context. A reviewer that starts fresh hasn't "seen" the author's reasoning, so it judges the code, not the intentions. That independence is the whole point of review.

## Subagent files (`.claude/agents/*.md`)
A file is YAML frontmatter plus instructions:
```markdown
---
name: reviewer
description: When to use it (Claude reads this to decide when to hand work over)
tools: Read, Grep, Glob, Bash        # optional allowlist; omit to inherit all tools
---
The system prompt: who it is, what it must and must not do, what it reports back.
```
- `name`: how you refer to it. `description`: the trigger text; write it as "Use when …".
- `tools`: least privilege. The reviewer cannot edit files because `Write`/`Edit` aren't listed.
- Project agents (`.claude/agents/`, committed) are shared by everyone using the repo and travel into every git worktree. Personal ones go in `~/.claude/agents/`.
- Manage them with the `/agents` command inside a session (list, create, edit), or edit the files directly.
- Call one by name in plain language: *"Use the reviewer agent to review branch wp-a."* Subagents can't launch other subagents.

## The agents here
| Agent | Does | Can't |
|---|---|---|
| `godot-builder` | Implements one work package inside the files it owns | edit files it doesn't own; merge |
| `reviewer` | Reads a branch diff against the checklist; writes findings | edit code |
| `qa-verifier` | Runs build, tests, headless smoke; reports pass/fail | edit code |
| `docs-keeper` | Updates docs after merges | edit code |

## Worktrees (parallel work without collisions)
A git worktree is a second checkout of the same repo on its own branch in its own folder. Two sessions in two worktrees can't overwrite each other's files, and each has its own `.godot/` import cache.
```sh
git worktree add ../cozy-brew-wp-a -b wp-a          # new folder + new branch, from the current branch
cd ../cozy-brew-wp-a && claude                       # a fresh session, working only there
git worktree list                                    # see them all
git worktree remove ../cozy-brew-wp-a                # when merged and done
```
A new worktree has no `.godot/` folder: run the import command once (see README) before the smoke test.

## `/clear`, `/compact`, plan mode, `/code-review`
- `/clear` wipes the conversation. Use it between prompts so each step starts clean; that works here because every prompt is self-contained and reads the docs itself.
- `/compact` summarises a long conversation to free space while keeping the gist. Use it mid-task, not between tasks.
- `/plan` makes a session read-only until you approve a plan. Good for "think first" steps.
- `/code-review medium` (or `high`) reviews the current diff. The `reviewer` agent uses the repo checklist; `/code-review` is a general second opinion. Using both finds more.

## When parallel helps and when it hurts
Helps: packages touch **different files** (see the ownership table in `docs/CONTRACTS.md`), and each can be tested alone. Hurts: two agents editing one file (`project.godot`, `GameState.cs`), or one package needing the other's unfinished types. That's why contracts exist and why integration is a separate, single-agent step.
Rule of thumb: **parallelise reading and reviewing freely; parallelise writing only across disjoint files.**

## Recipe: add a new agent
1. Decide the one job and what it must *not* do.
2. Create `.claude/agents/<name>.md` with `name`, a "Use when…" `description`, a minimal `tools` list.
3. Write the instructions as a checklist and say what it reports back (a fixed format makes results easy to compare).
4. Try it: *"Use the <name> agent to …"*, then tighten the prompt wherever it surprised you.

## Recipe: add a new work package prompt
Copy `docs/prompts/WP-A-core-logic.md`; change: goal, **owned files**, **forbidden files**, the contract section in `docs/CONTRACTS.md` it implements, acceptance criteria, and the report path. Add the package to the ownership table. Keep it self-contained: it must name every file it wants read.

## Why the prompts are shaped the way they are
1. **Read-first:** they list docs to read, so a cleared session needs no memory.
2. **Owned/forbidden files:** stops two packages colliding.
3. **Acceptance criteria and a verify command:** "done" is something checkable, not a feeling.
4. **A written report:** the next session (or you) reads a file instead of scrolling a chat.
5. **Stop before merging:** a human (you) decides what merges.
