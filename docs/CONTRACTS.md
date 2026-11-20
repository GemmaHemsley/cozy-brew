# Contracts

The exact public surface each work package (WP) must **provide** and may **consume**. This is what lets packages A–D be built in separate worktrees at the same time and still fit together in WP-E. If you need to deviate, **do not change another package's files**: implement what you can, and write the deviation under "Deviations" in your report.

Wave 1 rule: WP-B, WP-C and WP-D may only depend on the **Foundation** types below and on primitives (`string`, `double`, `int`, `Vector2`). They must **not** reference types from WP-A (`Mood`, `DayPlan`, ...). WP-E adapts between packages.

All code is in the global namespace. Signals use Godot `[Signal]` delegates. Enums cross signals as `int`.

## File ownership (exclusive per wave)
| Path | Wave 1 owner | Notes |
|---|---|---|
| `scripts/data/CustomerDef.cs`, `Roster.cs`, `ChatLines.cs`; `scripts/cafe/logic/**`; `tests/CozyBrew.Tests/Cafe*Tests.cs`; `scripts/dev/CafeLogicSmoke.cs`; `scripts/core/GameState.cs` | **A** | `GameState.cs` additions are listed under WP-A |
| `project.godot`; `scripts/core/SceneRouter.cs`, `Screen.cs`; `scenes/Title|Prep|Open|Close.tscn`; `scripts/screens/**`; `scenes/ui/Hud.tscn`, `scripts/ui/Hud.cs`; `scripts/dev/ShellSmoke.cs` | **B** | |
| `scenes/actors/**`; `scenes/ui/OrderTicket.tscn`, `DialogueBox.tscn`; `scripts/cafe/view/**`; `scenes/dev/RoomPreview.tscn`; `scripts/dev/WorldSmoke.cs` | **C** | |
| `scenes/brew/**`; `scripts/brew/**` (incl. `logic/`); `tests/CozyBrew.Tests/Brew*Tests.cs`; `scripts/dev/BrewSmoke.cs` | **D** | |
| `scripts/art/ArtKeys.cs`, `ArtCatalog.cs`, `scripts/ui/CafeTheme.cs`, `CLAUDE.md`, `art/**` | nobody (request changes in your report) | |
Edits to a file you don't own are rejected in review. New smoke suites are separate files implementing `ISmokeSuite`.

## Foundation (exists; read the source for details)
- `GameState.Instance`: `Coins`, `Day`, `ClockSeconds`, `ClockRunning`, `DayProgress` (0..1), `DayLengthSeconds`; `NewGame()`, `AdvanceDay()`, `StartDay()`, `SetClockRunning(bool)`, `TickClock(double)`, `GetClockText()` ("9:40 AM"), `AddCoins(int>0)`, `TrySpend(int)`, `IsUnlocked(ItemId)`, `UnlockedItems`, `GetHearts/SetHearts/AddHearts(RegularId, ...)`.
  Signals: `CoinsChanged(int)`, `DayChanged(int)`, `RecipeUnlocked(int itemId)`, `HeartsChanged(int regularId, int hearts)`, `DayTimeUp()`, `GameReset()`.
- `Menu.All`, `Menu.Get(ItemId)`, `Menu.AvailableOnDay(int)`; `MenuItem(Id, Name, Price, Kind, Recipe, ArtKey, UnlockDay, UnlockCost)`; enums `ItemId`, `ItemKind` (`Drink`/`Pastry`), `RegularId` (`Juniper, Sage, Milo, Rosie, Theo`).
- `ArtCatalog.Get(key)` (never null), `ArtCatalog.SizeOf(key)`, `ArtKeys.*`, `Palette.*`.
- `CafeTheme` type variations: `HeadingLabel`, `TitleLabel`, `SecondaryLabel`, `PatienceBar` (a `ProgressBar`; set `ShowPercentage = false`).
- `AudioHooks.Instance.PlaySfx(SfxId)`, `PlayMusic(MusicId)`.
- Input actions: `confirm` (Z, Enter), `cancel` (X, Esc); arrows are Godot's built-in `ui_*`.
- Smoke tests: implement `ISmokeSuite` (see `scripts/dev/`).

## WP-A: core logic (Godot-free, `scripts/cafe/logic/` and `scripts/data/`)
Must not `using Godot;`. Numbers come from `docs/GAME_RULES.md`, each in one named constant.
```csharp
public enum Mood { Cheerful, Calm, Hurried, Sleepy }

public sealed record CustomerDef(RegularId Regular, string Name, string ArtKey, float PatienceMultiplier, float PastryWeight);
public static class Roster { public static IReadOnlyList<CustomerDef> All; public static CustomerDef Get(RegularId id); }

public sealed class PatienceMeter {
    public PatienceMeter(double totalSeconds);
    public static double TotalSeconds(CustomerDef customer, Mood mood);
    public double Remaining { get; }   // 0..1 fraction left
    public bool Expired { get; }
    public void Tick(double deltaSeconds);
}

public readonly record struct TipResult(int Sale, int Tip) { public int Total => Sale + Tip; }
public static class TipCalculator {
    public static TipResult Calculate(MenuItem item, double patienceRemaining, double accuracy, Mood mood, bool isSpecial);
}

public sealed class DayPlan {
    public const int PastryCapacity = 12;
    public ItemId Special { get; set; }              // default Latte
    public double GrindQuality { get; set; }         // 0.5..1, default 0.75
    public int GetStock(ItemId pastry);
    public bool TrySetStock(ItemId pastry, int count);   // false if it would exceed capacity or count < 0
    public bool TryTakePastry(ItemId pastry);            // decrements stock
    public bool IsSpecial(ItemId item);
    public static DayPlan CreateDefault(IEnumerable<MenuItem> available);   // even split of capacity over pastries
}

public sealed record Arrival(double TimeSeconds, CustomerDef Customer, Mood Mood, ItemId Item);
public sealed class CustomerSpawner {
    public CustomerSpawner(int seed);
    public IReadOnlyList<Arrival> BuildSchedule(int day, double dayLengthSeconds, IReadOnlyCollection<MenuItem> available, DayPlan plan);
}

public sealed class CounterSlots {
    public CounterSlots(int count = 3);
    public int Count { get; }
    public bool HasFree { get; }
    public int TryOccupy();          // slot index, or -1 if full
    public void Release(int slot);
}

public sealed class DaySummary {
    public int Served { get; } public int WalkedOut { get; } public int SalesCoins { get; } public int TipCoins { get; }
    public int TotalCoins => SalesCoins + TipCoins;
    public List<ItemId> NewUnlocksTomorrow { get; }
    public void RecordServed(TipResult result);
    public void RecordWalkout();
}

public static class ChatLines {
    public static string OrderLine(CustomerDef c, Mood m, MenuItem item, System.Random rng);   // e.g. "Morning! Could I get a latte?"
    public static string ThanksLine(CustomerDef c, Mood m, System.Random rng);
    public static string SmallTalk(CustomerDef c, Mood m, System.Random rng);                  // ≥3 per regular, in character
}
```
`GameState` additions (WP-A edits `GameState.cs`): `DayPlan Plan { get; set; }` and `DaySummary LastSummary { get; set; }` (both reset in `NewGame`/`AdvanceDay`; `Plan` recreated with `DayPlan.CreateDefault`); `const int ChatPointsPerHeart = 3`; `int GetChatPoints(RegularId)`; `void AddChatPoint(RegularId)` (adds a point, converts every 3 points into a heart via `AddHearts`).
Also adds a `NewGame()`-time reset of chat points and emits nothing new.

## WP-B: screen shell
```csharp
public enum Screen { Title, Prep, Open, Close }          // scripts/core/Screen.cs
public partial class SceneRouter : Node {                // autoload "SceneRouter"
    public static SceneRouter Instance { get; }
    public void GoTo(Screen screen);                     // fade out, ChangeSceneToFile, fade in; ignores calls while fading
    public bool IsTransitioning { get; }
}
```
Scenes (root script class in brackets): `res://scenes/Title.tscn` (`TitleScreen`), `Prep.tscn` (`PrepScreen`), `Open.tscn` (`CafeScreen`), `Close.tscn` (`CloseScreen`). Stub behaviour: Title "New day" → `GameState.NewGame()` then Prep; Prep "Open shop" → Open; Open calls `GameState.StartDay()` in `_Ready` and has a "Close shop" button → Close; Close "Next day" → `GameState.AdvanceDay()` then Prep. `Main._Ready` routes to Title.
`res://scenes/ui/Hud.tscn` (`Hud : CanvasLayer`): top-left panel with coin icon + coins; top-right panel with clock icon + `Day N · 9:40 AM`; updates from `GameState` signals and a once-per-second clock refresh; disconnects in `_ExitTree`. Instanced by `Open.tscn` (and optionally Prep/Close). Project settings owned by B: register `SceneRouter` autoload, set main scene (unchanged).

## WP-C: café world views (primitives and foundation types only)
```csharp
public partial class CafeRoom : Node2D {                  // scenes/actors/CafeRoom.tscn; built from tiles/props via ArtCatalog
    public const int SlotCount = 3;
    public Vector2 GetSlotPosition(int slot);             // where a customer's feet stand (whole pixels)
    public Vector2 DoorPosition { get; }                  // spawn/exit point (off-screen or at the door)
    public Vector2 BaristaPosition { get; }
}
public partial class Barista : Node2D { }                 // scenes/actors/Barista.tscn: idle+bob at 4 fps, blink every 3-5 s
public partial class Biscuit : Node2D { }                 // scenes/actors/Biscuit.tscn: idle, blink every 3-5 s
public partial class Customer : Node2D {                  // scenes/actors/Customer.tscn
    [Signal] public delegate void ArrivedEventHandler();
    [Signal] public delegate void LeftEventHandler();
    public void Setup(string artKey, string displayName);
    public void SetPatience(double fraction01);           // drives a small PatienceBar above the head
    public void ShowPatience(bool visible);
    public void WalkTo(Vector2 target, double seconds);   // Tween slide; emits Arrived
    public void Leave(Vector2 exit, double seconds);      // Tween slide; emits Left (does NOT free itself)
}
public partial class OrderTicket : Control {              // scenes/ui/OrderTicket.tscn, per UI kit
    public void SetOrder(MenuItem item, string customerName, int orderNumber);
    public void SetProgress(double fraction01);           // Apricot progress bar
}
public partial class DialogueBox : Control {              // scenes/ui/DialogueBox.tscn: bottom panel per art/DESIGN_REFERENCE.md
    [Signal] public delegate void StartBrewingPressedEventHandler();
    [Signal] public delegate void ChatPressedEventHandler();
    public void Show(string customerName, string line);   // name in HeadingLabel
    public void SetPatience(double fraction01);
    public void SetButtons(bool startBrewing, bool chat);  // enabled flags (disabled style); hidden when both false and no line
    public void Hide();
}
```
Positions are whole pixels. `scenes/dev/RoomPreview.tscn` shows the room with barista, cat and three customers for visual checking.

## WP-D: brew
Godot-free in `scripts/brew/logic/`:
```csharp
public enum BrewStep { Shot, Milk, Matcha, Cocoa, Ice }
public static class BrewRecipes { public static IReadOnlyList<BrewStep> StepsFor(ItemId item); }   // pastries: empty
public readonly record struct TimingBarConfig(double CyclesPerSecond, double PerfectHalfWidth, double ZeroAccuracyDistance);
public static class TimingBarMath {
    public static double Position(double elapsedSeconds, double cyclesPerSecond);     // triangle wave 0..1
    public static double Accuracy(double position, double target, double perfectHalfWidth, double zeroDistance);
    public static double PerfectHalfWidth(double grindQuality);
    public static TimingBarConfig ConfigFor(BrewStep step, double grindQuality);
    public static double Combine(IReadOnlyList<double> stepAccuracies);                // mean; empty list → 1.0
}
```
Godot views:
```csharp
public partial class TimingBar : Control {                // scenes/brew/TimingBar.tscn; reusable (also used by Prep grinding)
    [Signal] public delegate void StoppedEventHandler(double accuracy);
    public void Begin(string label, TimingBarConfig config, System.Random rng);   // starts moving; stops on click or `confirm`
}
public partial class BrewPanel : Control {                // scenes/brew/BrewPanel.tscn
    [Signal] public delegate void BrewFinishedEventHandler(double accuracy);       // 0..1
    public void Begin(MenuItem item, double grindQuality);   // runs the recipe's steps in order; zero steps → emits BrewFinished(1.0) next frame
}
```
Neither view may reference WP-A types; `grindQuality` is a plain double.

## WP-E: integration (wave 2; may edit any file listed above, one agent)
Consumes everything. Owns: `CafeScreen`, `PrepScreen`, `CloseScreen` bodies; `scripts/dev/LoopSmoke.cs`. Keeps contracts intact; any change to a contract is recorded in this file in the same commit.
