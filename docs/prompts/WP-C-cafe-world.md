# WP-C: café world views (paste into a fresh session in worktree `../cozy-brew-wp-c`, branch `wp-c`)

```
Use the godot-builder agent to implement work package WP-C (café world views) of Little Bean Café, in this worktree on branch wp-c.

Read first: CLAUDE.md, docs/ARCHITECTURE.md, docs/CONTRACTS.md (section "WP-C" and the ownership table), docs/GAME_RULES.md,
art/ART_MANIFEST.md, art/DESIGN_REFERENCE.md (gameplay screen: dialogue box, order chip, order ticket), README.md (Commands).
Also read: scripts/art/ArtCatalog.cs, scripts/art/ArtKeys.cs, scripts/ui/CafeTheme.cs, scripts/data/MenuItem.cs.

Goal: the visual building blocks of the café as reusable scenes, none of which know about game rules.

You OWN: scenes/actors/** (CafeRoom, Barista, Biscuit, Customer), scenes/ui/OrderTicket.tscn, scenes/ui/DialogueBox.tscn,
  scripts/cafe/view/**, scenes/dev/RoomPreview.tscn, scripts/dev/WorldSmoke.cs, docs/reports/WP-C.md.
You must NOT edit: GameState.cs, ArtCatalog/ArtKeys/CafeTheme/Palette, project.godot, scenes/Open.tscn or any screen, scripts/cafe/logic, scripts/brew.
Do NOT reference types from other packages (Mood, DayPlan, CustomerDef...): use only foundation types (MenuItem, ItemId) and primitives.

Build per the contract:
- CafeRoom: the 320x180 room built in code from tiles and props through ArtCatalog (no baked mockup): wall with wallpaper + wainscot, window(s),
  chalkboard, bean shelf, pendant lamp(s), floor (wood or checker), counter (top + front tiles) with the espresso machine on it, a couple of tables/chairs/plants
  in the customer area. Keep the bottom ~50 px free for the DialogueBox. Expose GetSlotPosition(0..2), DoorPosition, BaristaPosition with whole-pixel values.
  Use y-sorted drawing (Node2D.YSortEnabled) or explicit ordering so characters stand correctly in front of/behind props. Explain this in a comment.
- Barista: idle + bob alternate at 4 fps; blink frame for ~0.12 s at random 3-5 s intervals. Biscuit: cat idle with the same random blink.
- Customer: Sprite from ArtCatalog by artKey (feet at the node origin: sprites are 16x24 with the base on the bottom row, so offset the sprite accordingly),
  name label optional, a small PatienceBar-variation ProgressBar above the head (Fern fill, ShowPercentage=false), WalkTo/Leave via Tween (whole-pixel positions each step: snap).
- OrderTicket and DialogueBox exactly as in the contract and the design reference (panel layout, HeadingLabel name in Cherry, SecondaryLabel for secondary text,
  Apricot progress fill on the ticket, Fern patience fill in the dialogue box, "Start brewing" and "Chat" buttons, disabled style when disabled).
- scenes/dev/RoomPreview.tscn: room + barista + cat + three customers standing in the slots + one order ticket + the dialogue box showing sample text,
  so a human can open it in the editor. No game logic.
- Missing art must never crash (ArtCatalog handles it). Honour confirm/cancel and mouse; DialogueBox buttons keyboard-focusable.
- AudioHooks.Instance?.PlaySfx(SfxId.Click) on button presses.

Smoke suite (scripts/dev/WorldSmoke.cs): instantiate every scene; CafeRoom slot positions are whole numbers, distinct, and inside 0..320 x 0..180;
Customer.WalkTo emits Arrived and Leave emits Left after the tween (use a short duration and await frames via the scene tree timer);
DialogueBox.SetButtons toggles Disabled; OrderTicket.SetOrder shows the item name.

Done when: `dotnet build` 0 errors/0 warnings; `dotnet test` green; headless smoke SMOKE OK (run --import first in this fresh worktree).
Describe each scene's node tree in the report (no headless screenshots). Write docs/reports/WP-C.md, commit on wp-c, do not merge.
Reply with a short summary and the report path.
```
