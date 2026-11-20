using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Centred panel that brews one menu item: runs the recipe's steps in order with one TimingBar each and emits
/// BrewFinished(accuracy) with the mean accuracy. Pastries (no steps) finish with 1.0 on the next frame without
/// showing anything. Call Begin after the panel has been added to the scene tree.
///
/// Godot notes: the panel is built in code in _Ready; PackedScene instancing creates the root node from
/// BrewPanel.tscn and runs this script. `CallDeferred` runs a method at the end of the current frame.
/// </summary>
public partial class BrewPanel : Control
{
	[Signal] public delegate void BrewFinishedEventHandler(double accuracy);

	/// <summary>Pause after a step stops so the player can read the feedback text.</summary>
	public const double StepPauseSeconds = 0.6;

	private PanelContainer _panel;
	private TextureRect _icon;
	private Label _name;
	private Label _stepLabel;
	private TimingBar _bar;
	private readonly Random _rng = new();

	private IReadOnlyList<BrewStep> _steps = Array.Empty<BrewStep>();
	private readonly List<double> _accuracies = new();
	private double _grind = 0.75;
	private int _stepIndex;
	private bool _active;
	private bool _finishPending;
	private int _pauseToken; // bumped to invalidate an outstanding pause timer
	private MenuItem _pendingItem; // Begin called before _Ready
	private bool _advancePending; // a stopped step is waiting out its feedback pause

	public int StepCount => _steps.Count;
	public int StepIndex => _stepIndex;
	public bool Active => _active;
	public string StepText => _stepLabel?.Text ?? "";
	public TimingBar Bar => _bar;
	public bool AdvancePending => _advancePending;

	public override void _Ready()
	{
		SetAnchorsPreset(LayoutPreset.FullRect);
		MouseFilter = MouseFilterEnum.Ignore;
		TextureFilter = TextureFilterEnum.Nearest;

		_panel = new PanelContainer();
		AddChild(_panel);

		var box = new VBoxContainer();
		box.AddThemeConstantOverride("separation", 4);
		_panel.AddChild(box);

		var head = new HBoxContainer();
		head.AddThemeConstantOverride("separation", 4);
		box.AddChild(head);
		_icon = new TextureRect { CustomMinimumSize = new Vector2(16, 16), StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered };
		head.AddChild(_icon);
		_name = new Label { ThemeTypeVariation = CafeTheme.HeadingLabel, SizeFlagsHorizontal = SizeFlags.ExpandFill };
		head.AddChild(_name);
		_stepLabel = new Label { ThemeTypeVariation = CafeTheme.SecondaryLabel };
		head.AddChild(_stepLabel);

		_bar = new TimingBar { CustomMinimumSize = new Vector2(TimingBar.BarWidth, 0) };
		box.AddChild(_bar);
		_bar.Stopped += OnStepStopped;

		_panel.Visible = false;
		Resized += Recenter; // Resized is a built-in Control signal; re-centre whenever layout gives us a real size
		if (_pendingItem != null)
		{
			var item = _pendingItem;
			_pendingItem = null;
			Begin(item, _grind);
		}
	}

	public override void _EnterTree()
	{
		// Re-added mid-pause (the timer was invalidated in _ExitTree): arm the advance again so the brew cannot stall.
		if (_advancePending)
			ScheduleAdvance();
	}

	public override void _ExitTree()
	{
		_pauseToken++; // any pending timer callback becomes a no-op; _advancePending stays set so _EnterTree re-arms
		// The Stopped subscription is made once in _Ready and is deliberately NOT removed here: _Ready runs once per
		// node lifetime, so unsubscribing would break the panel if it is removed and re-added. The bar is our child
		// and is freed with us, so nothing leaks.
	}

	/// <summary>Starts brewing <paramref name="item"/>; grindQuality (0.5..1) widens the perfect zone.</summary>
	public void Begin(MenuItem item, double grindQuality)
	{
		_grind = grindQuality;
		if (_bar == null) // not in the tree yet: start once _Ready has built the UI
		{
			_pendingItem = item;
			return;
		}

		_pauseToken++;
		_advancePending = false;
		_bar.Cancel(); // a bar still running from a previous Begin must not fire into the new recipe
		_steps = BrewRecipes.StepsFor(item.Id);
		_accuracies.Clear();
		_stepIndex = 0;
		_active = true;
		_finishPending = false;

		if (_steps.Count == 0)
		{
			_panel.Visible = false;
			_finishPending = true;
			CallDeferred(nameof(FinishPending)); // "next frame": fires at the end of the current one
			return;
		}

		_icon.Texture = ArtCatalog.Get(item.ArtKey);
		_name.Text = item.Name;
		_panel.Visible = true;
		StartStep();
	}

	/// <summary>Centres the panel by hand and floors so an odd size never lands it on a half pixel.</summary>
	private void Recenter()
	{
		if (_panel == null) return;
		_panel.ResetSize(); // shrink to minimum size so the centring uses the real panel size
		_panel.Position = ((Size - _panel.Size) / 2).Floor();
	}

	private void StartStep()
	{
		var step = _steps[_stepIndex];
		_stepLabel.Text = $"Step {_stepIndex + 1}/{_steps.Count}";
		_bar.Begin(LabelFor(step), TimingBarMath.ConfigFor(step, _grind), _rng);
		Recenter(); // the step text can change the panel's minimum size
	}

	public static string LabelFor(BrewStep step) => step switch
	{
		BrewStep.Shot => "Pull the shot",
		BrewStep.Milk => "Steam the milk",
		BrewStep.Matcha => "Whisk the matcha",
		BrewStep.Cocoa => "Stir the cocoa",
		BrewStep.Ice => "Add the ice",
		_ => "Brew",
	};

	private void OnStepStopped(double accuracy)
	{
		if (!_active || _stepIndex >= _steps.Count)
			return;
		AudioHooks.Instance?.PlaySfx(_steps[_stepIndex] switch
		{
			BrewStep.Shot => SfxId.ShotPull,
			BrewStep.Milk => SfxId.MilkSteam,
			_ => SfxId.Click,
		});
		_accuracies.Add(accuracy);

		ScheduleAdvance();
	}

	private void ScheduleAdvance()
	{
		_advancePending = true;
		int token = ++_pauseToken;
		GetTree().CreateTimer(StepPauseSeconds).Timeout += () =>
		{
			if (token == _pauseToken && IsInstanceValid(this) && IsInsideTree())
				AdvanceStep();
		};
	}

	private void AdvanceStep()
	{
		_pauseToken++;
		_advancePending = false;
		_stepIndex++;
		if (_stepIndex < _steps.Count)
		{
			StartStep();
			return;
		}
		_panel.Visible = false;
		_active = false;
		EmitSignal(SignalName.BrewFinished, TimingBarMath.Combine(_accuracies));
	}

	private void FinishPending()
	{
		if (!_finishPending)
			return;
		_finishPending = false;
		_active = false;
		EmitSignal(SignalName.BrewFinished, 1.0);
	}

	// ---- test hooks (used by scripts/dev/BrewSmoke.cs, which cannot wait for frames or timers) ----

	/// <summary>Test hook: runs the deferred "finish next frame" immediately (used for zero-step items).</summary>
	public void RunPendingForTests() => FinishPending();

	/// <summary>Test hook: skips the feedback pause after a stopped step and moves on.</summary>
	public void SkipPauseForTests()
	{
		if (_active && _accuracies.Count > _stepIndex)
			AdvanceStep();
	}
}
