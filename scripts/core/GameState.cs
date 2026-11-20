using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Autoload singleton holding all mutable game state: coins, day, the day clock, unlocked recipes and
/// friendship hearts per regular. Register it in Project Settings > Globals > Autoload as "GameState".
///
/// Other scripts read it with <c>GameState.Instance</c> and listen to its signals.
/// It holds data only; it never touches the scene tree beyond ticking the clock.
/// </summary>
public partial class GameState : Node
{
	public const int MaxHearts = 5;

	public static GameState Instance { get; private set; }

	// Signals (Godot's observer pattern; roughly C# events, but visible to the editor).
	// Enum values travel as ints because signal arguments must be Godot Variant types.
	[Signal] public delegate void CoinsChangedEventHandler(int coins);
	[Signal] public delegate void DayChangedEventHandler(int day);
	[Signal] public delegate void RecipeUnlockedEventHandler(int itemId);
	[Signal] public delegate void HeartsChangedEventHandler(int regularId, int hearts);
	[Signal] public delegate void DayTimeUpEventHandler();
	/// <summary>Emitted at the end of <see cref="NewGame"/>. UI that mirrors state (menus, hearts) should rebuild itself.</summary>
	[Signal] public delegate void GameResetEventHandler();

	// Tuning. [Export] values show in the inspector when this script is attached to a node in a scene.
	// As a plain-script autoload they use these defaults; to tune them in the editor, make a scene with a
	// GameState node and register that .tscn as the autoload instead.
	[Export]
	public float DayLengthSeconds // ~6 real minutes per in-game day; clamped so a 0 in the inspector can't end the day instantly
	{
		get => _dayLengthSeconds;
		set => _dayLengthSeconds = Mathf.Max(1f, value);
	}
	[Export] public int DayStartHour { get; set; } = 8;          // in-game clock shown on the HUD
	[Export] public int DayEndHour { get; set; } = 18;
	[Export] public int StartingCoins { get; set; } = 0;

	public int Coins { get; private set; }
	public int Day { get; private set; } = 1;

	/// <summary>Real seconds elapsed in the current day.</summary>
	public double ClockSeconds { get; private set; }
	public bool ClockRunning { get; private set; }

	/// <summary>0 at opening, 1 at closing time.</summary>
	public double DayProgress => Math.Clamp(ClockSeconds / DayLengthSeconds, 0.0, 1.0);

	private float _dayLengthSeconds = 360f;
	private bool _initialised;
	private readonly HashSet<ItemId> _unlocked = new();
	private readonly Dictionary<RegularId, int> _hearts = new();

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}

	public override void _Ready()
	{
		// Set up defaults once, silently. If something (e.g. a future save loader) already initialised the
		// state before this node became ready, don't wipe it.
		if (!_initialised)
			ResetState();
	}

	// _Process runs every frame; delta is seconds since the last frame (like Time.deltaTime in Unity).
	public override void _Process(double delta)
	{
		TickClock(delta);
	}

	// ----- Game / day lifecycle -----

	/// <summary>Resets everything to day 1 and tells listeners to refresh.</summary>
	public void NewGame()
	{
		var oldHearts = _hearts.ToDictionary(pair => pair.Key, pair => pair.Value);
		ResetState();

		EmitSignal(SignalName.CoinsChanged, Coins);
		EmitSignal(SignalName.DayChanged, Day);
		foreach (var (regular, before) in oldHearts)
		{
			if (before != GetHearts(regular))
				EmitSignal(SignalName.HeartsChanged, (int)regular, GetHearts(regular));
		}
		EmitSignal(SignalName.GameReset);
	}

	/// <summary>Sets every field to its day-1 value without emitting signals.</summary>
	private void ResetState()
	{
		_initialised = true;
		Day = 1;
		Coins = StartingCoins;
		ClockSeconds = 0;
		ClockRunning = false;

		_unlocked.Clear();
		RefreshUnlocks(announce: false);

		_hearts.Clear();
		foreach (RegularId regular in Enum.GetValues<RegularId>())
			_hearts[regular] = 0;
	}

	/// <summary>Moves to the next day: stops and resets the clock and unlocks anything due.</summary>
	public void AdvanceDay()
	{
		Day++;
		ClockSeconds = 0;
		ClockRunning = false;
		RefreshUnlocks(announce: true);
		EmitSignal(SignalName.DayChanged, Day);
	}

	// ----- Clock -----

	/// <summary>Resets the clock to opening time and starts it.</summary>
	public void StartDay()
	{
		ClockSeconds = 0;
		ClockRunning = true;
	}

	/// <summary>Pause or resume the clock (e.g. during a dialogue or menu).</summary>
	public void SetClockRunning(bool running)
	{
		ClockRunning = running && ClockSeconds < DayLengthSeconds;
	}

	/// <summary>Advances the clock. Called from _Process; public so tests can drive it.</summary>
	public void TickClock(double delta)
	{
		if (!ClockRunning)
			return;

		ClockSeconds += delta;
		if (ClockSeconds >= DayLengthSeconds)
		{
			ClockSeconds = DayLengthSeconds;
			ClockRunning = false;
			EmitSignal(SignalName.DayTimeUp);
		}
	}

	/// <summary>In-game time for the HUD, e.g. "9:40 AM". Spans DayStartHour..DayEndHour over the day.</summary>
	public string GetClockText()
	{
		double hours = DayStartHour + DayProgress * (DayEndHour - DayStartHour);
		int totalMinutes = (int)(hours * 60);
		int hour24 = totalMinutes / 60 % 24;
		int minute = totalMinutes % 60;
		int hour12 = hour24 % 12 == 0 ? 12 : hour24 % 12;
		return $"{hour12}:{minute:00} {(hour24 >= 12 ? "PM" : "AM")}";
	}

	// ----- Coins -----

	/// <summary>Earns coins (tips, sales). Ignores zero and negative amounts: spend with <see cref="TrySpend"/>.</summary>
	public void AddCoins(int amount)
	{
		if (amount <= 0)
			return;
		Coins += amount;
		EmitSignal(SignalName.CoinsChanged, Coins);
	}

	/// <summary>Spends coins if the player can afford it. Returns false (and changes nothing) otherwise.</summary>
	public bool TrySpend(int amount)
	{
		if (amount < 0 || amount > Coins)
			return false;
		Coins -= amount;
		EmitSignal(SignalName.CoinsChanged, Coins);
		return true;
	}

	// ----- Recipes / menu -----

	public bool IsUnlocked(ItemId id) => _unlocked.Contains(id);

	public IEnumerable<MenuItem> UnlockedItems => Menu.All.Where(item => _unlocked.Contains(item.Id));

	/// <summary>Unlocks every item whose unlock day has been reached. Items with a UnlockCost are for a future shop.</summary>
	private void RefreshUnlocks(bool announce)
	{
		foreach (var item in Menu.AvailableOnDay(Day))
		{
			if (_unlocked.Add(item.Id) && announce)
				EmitSignal(SignalName.RecipeUnlocked, (int)item.Id);
		}
	}

	// ----- Friendship -----

	public int GetHearts(RegularId regular) => _hearts.GetValueOrDefault(regular);

	public void SetHearts(RegularId regular, int hearts)
	{
		hearts = Math.Clamp(hearts, 0, MaxHearts);
		if (GetHearts(regular) == hearts)
			return;
		_hearts[regular] = hearts;
		EmitSignal(SignalName.HeartsChanged, (int)regular, hearts);
	}

	public void AddHearts(RegularId regular, int amount) => SetHearts(regular, GetHearts(regular) + amount);
}
