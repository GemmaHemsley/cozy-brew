# Setting up a new worktree (do this before you run `claude`)

Do these steps in order, in a terminal tab, every time you create a worktree for a work package.
Replace `wp-a` with the package's branch name (`wp-b`, `wp-c`, `wp-d`, ...).

## One-time Mac setup (already done on this machine)
1. Godot lives in `/Applications/Godot_mono.app`, not in Downloads (see "Why" below).
2. This line is in `~/.zshrc`, so every new terminal tab knows where Godot is:
   ```sh
   export GODOT="/Applications/Godot_mono.app/Contents/MacOS/Godot"
   ```
   Check any time with `echo "$GODOT"` (it should print that path) and `"$GODOT" --version`.

## Per-worktree steps
```sh
# 1. Be in the main repo folder and make sure everything you want is committed
cd ~/cozy-brew
git status --short                       # should print nothing; if it prints files, commit them first

# 2. Create the worktree: a new folder next to the repo, on a new branch
git worktree add ../cozy-brew-wp-a -b wp-a

# 3. Go into it. Confirm where you are.
cd ../cozy-brew-wp-a
pwd                                      # should end in /cozy-brew-wp-a

# 4. Check Godot is known to this tab (if it prints nothing, run: source ~/.zshrc)
echo "$GODOT"

# 5. Build the C# code
dotnet build                             # expect 0 Warning(s), 0 Error(s)

# 6. Import the project into Godot's cache (once per worktree)
"$GODOT" --headless --path . --import

# 7. Now start Claude and paste the package prompt from docs/prompts/
claude
```

## Why each step exists
- **Worktree:** a second checkout of the repo with its own branch, so two Claude sessions never edit the same files. It only contains what was *committed* when you created it, hence step 1.
- **`dotnet build`:** compiles the C# once so you find out immediately if the setup is broken.
- **The Godot import (step 6):** Godot keeps a cache folder, `.godot/`, with converted art, fonts and compiled scene data. Git ignores it, so a brand-new worktree has none. Godot must build it before it can run the project, even headless. The smoke test and the `qa-verifier` agent fail without it.

## Why `$GODOT` has to be set (and "exported")
- Godot on a Mac is an **app bundle**: a folder named `Godot_mono.app` that Finder shows as one icon. The actual program is buried inside it at `Contents/MacOS/Godot`. Double-clicking opens the editor, but we need to run that inner program from the terminal with flags (`--headless` for no window, `--path .` for "this project", `--import` to build the cache). That is how builds, tests and the agents use Godot.
- That path is long, so we store it in a **shell variable** named `GODOT`. Then every command, doc and agent can say `"$GODOT"` instead of repeating the path (and if Godot ever moves, you change one line).
- `export` makes the variable available to programs started from that terminal, not just the shell itself. Claude and the scripts it runs inherit it, which is why the agents can use `"$GODOT"`.
- A variable set by typing `export ...` lasts only for **that one terminal tab**. A new tab starts empty, and `"$GODOT" ...` then fails with a confusing `permission denied` or `no such file` (it is trying to run an empty name). Putting the line in `~/.zshrc` fixes this: zsh runs that file at the start of every new tab.
- A tab you opened *before* the line was added needs `source ~/.zshrc` once to load it.

## Why Godot is in /Applications, not Downloads
macOS protects `~/Downloads`, `~/Desktop` and `~/Documents`. Programs launched from a terminal or by an agent can be blocked from reading them ("Operation not permitted"). `/Applications` has no such restriction, and it is where Mac apps normally live.

## Troubleshooting
| Symptom | Cause | Fix |
|---|---|---|
| `permission denied` or `no such file` running `"$GODOT" ...` | `$GODOT` is empty in this tab | `source ~/.zshrc`, then `echo "$GODOT"` |
| `echo "$GODOT"` prints the path but the file isn't found | Godot isn't at that path | `ls /Applications/Godot_mono.app/Contents/MacOS/Godot`; move the app if needed |
| macOS says the app is from an unidentified developer | Gatekeeper check on downloaded apps | System Settings, Privacy & Security, Open Anyway (one time) |
| Smoke test or `qa-verifier` fails right after creating a worktree | `.godot/` cache is missing | run step 6 |
| `dotnet build` shows errors in a brand-new worktree | wrong folder, or the branch isn't the one you meant | `pwd` and `git branch --show-current` |

## When a package is merged and you're done
```sh
cd ~/cozy-brew
git worktree remove ../cozy-brew-wp-a
git branch -d wp-a
```

## After packages are built: merge, refresh docs, playtest
Status as of the end of the first session: WP-A and WP-D are reviewed, verified and merged into `integration`. WP-B and WP-C are not started yet.

1. **Build the remaining wave-1 packages** (B and C, in two tabs), each with the per-worktree steps above, then REVIEW.md, then VERIFY.md.
2. **Merge into `integration`.** Git allows a branch to be checked out in only one folder. `integration` is currently checked out in `~/cozy-brew-wp-d`, so run the merge there:
   ```sh
   cd ~/cozy-brew-wp-d
   claude
   ```
   Paste `docs/prompts/MERGE.md` with its first line set to `Packages to merge, in this order: wp-b wp-c`.
   (If you would rather merge from the main folder, first free the branch with `git worktree remove ../cozy-brew-wp-d`; the `wp-d` branch itself is kept.)
3. **Refresh docs:** paste `docs/prompts/DOCS-REFRESH.md` once, after all four packages are merged.
4. **Playtest.** Nothing is playable until WP-B (screens) and WP-C (café world) are merged. Then, in the folder that has `integration` checked out:
   ```sh
   "$GODOT" --path .
   ```
   (no `--headless`: that runs without a window). Or open the folder as a project in the Godot editor and press F5.
5. **Next:** WP-E (integration wiring), then WP-F (polish). You merge `integration` into `main` yourself when happy.

## Tidy up finished worktrees
`~/cozy-brew-wp-a` is finished. Remove it any time: `cd ~/cozy-brew && git worktree remove ../cozy-brew-wp-a`. Keep `wp-d`'s folder while it is your `integration` checkout.

## Note: there is no git remote yet
Everything is local. To back it up, create an empty repo on GitHub and run `git remote add origin <url>` then `git push -u origin main integration`.
