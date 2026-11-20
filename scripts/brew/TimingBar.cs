using System;
using Godot;

/// <summary>
/// A horizontal timing bar: a marker sweeps back and forth, the player stops it (mouse click anywhere on the
/// control, or the `confirm` action) as close to the highlighted target zone as possible.
/// Reusable: BrewPanel uses one per recipe step and the Prep screen uses one for grinding beans.
///
/// Godot notes: a Control is a UI node with a rectangle; `_Process(delta)` runs every frame, `_Draw()` paints
/// custom shapes (call QueueRedraw() to repaint), `_GuiInput` receives mouse events that land on this control,
/// and a [Signal] delegate (ending in EventHandler) becomes a C# event: `bar.Stopped += acc => ...`.
/// All maths lives in the Godot-free TimingBarMath.
/// </summary>
public partial class TimingBar : Control
{
	/// <summary>Emitted once per run when the marker is stopped; accuracy is 0..1.</summary>
	[Signal] public delegate void StoppedEventHandler(double accuracy);

	// Layout, in whole game pixels.
	public const int BarWidth = 208;
	public const int LabelHeight = 10;
	public const int BarHeight = 12;
	public const int FeedbackHeight = 10;
	public const int Gap = 2;
	public const int MarkerWidth = 3;

	/// <summary>How long the "Perfect!/Good/Missed" text stays visible.</summary>
	public const double FeedbackSeconds = 0.5;
	/// <summary>Accuracy at or above this reads as "Perfect!"; at or above GoodAccuracy as "Good".</summary>
	public const double PerfectAccuracy = 0.98;
	public const double GoodAccuracy = 0.5;

	private Label _label;
	private Label _feedback;
	private TimingBarConfig _config;
	private double _target = 0.5;
	private double _elapsed;
	private double _feedbackLeft;
	private double _stoppedPosition;
	private bool _running;

	public bool Running => _running;
	/// <summary>Target centre (0..1) of the current run.</summary>
	public double Target => _target;
	/// <summary>Marker position (0..1) right now, or where it stopped.</summary>
	public double MarkerPosition => _running ? TimingBarMath.Position(_elapsed, _config.CyclesPerSecond) : _stoppedPosition;

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(BarWidth, LabelHeight + Gap + BarHeight + Gap + FeedbackHeight);
		MouseFilter = MouseFilterEnum.Stop;
		TextureFilter = TextureFilterEnum.Nearest;

		_label = new Label { Position = Vector2.Zero, Size = new Vector2(BarWidth, LabelHeight) };
		_label.HorizontalAlignment = HorizontalAlignment.Center;
		AddChild(_label);

		_feedback = new Label
		{
			Position = new Vector2(0, LabelHeight + Gap + BarHeight + Gap),
			Size = new Vector2(BarWidth, FeedbackHeight),
			ThemeTypeVariation = CafeTheme.SecondaryLabel,
			HorizontalAlignment = HorizontalAlignment.Center,
			Text = "",
		};
		AddChild(_feedback);
	}

	/// <summary>Starts a run: picks a random target in 0.30..0.70 using <paramref name="rng"/> and starts moving.</summary>
	public void Begin(string label, TimingBarConfig config, Random rng)
	{
		_config = config;
		_target = TimingBarMath.TargetMin + (rng ?? new Random()).NextDouble() * (TimingBarMath.TargetMax - TimingBarMath.TargetMin);
		_elapsed = 0;
		_feedbackLeft = 0;
		_running = true;
		if (_label != null) _label.Text = label;
		if (_feedback != null) _feedback.Text = "";
		QueueRedraw();
	}

	/// <summary>
	/// Stops the marker exactly as a click or `confirm` would. Public so smoke tests can drive it.
	/// Returns the accuracy, or -1 if the bar was not running.
	/// </summary>
	public double StopNow()
	{
		if (!_running)
			return -1;
		_stoppedPosition = TimingBarMath.Position(_elapsed, _config.CyclesPerSecond);
		_running = false;
		double accuracy = TimingBarMath.Accuracy(_stoppedPosition, _target, _config.PerfectHalfWidth, _config.ZeroAccuracyDistance);

		_feedback.Text = accuracy >= PerfectAccuracy ? "Perfect!" : accuracy >= GoodAccuracy ? "Good" : "Missed";
		_feedbackLeft = FeedbackSeconds;
		QueueRedraw();
		EmitSignal(SignalName.Stopped, accuracy);
		return accuracy;
	}

	public override void _Process(double delta)
	{
		if (_running)
		{
			_elapsed += delta;
			QueueRedraw();
		}
		if (_feedbackLeft > 0)
		{
			_feedbackLeft -= delta;
			if (_feedbackLeft <= 0 && _feedback != null)
				_feedback.Text = "";
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (_running && @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
		{
			AcceptEvent();
			StopNow();
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_running && @event.IsActionPressed("confirm"))
		{
			GetViewport().SetInputAsHandled();
			StopNow();
		}
	}

	public override void _Draw()
	{
		int barY = LabelHeight + Gap;
		var track = new Rect2(0, barY, BarWidth, BarHeight);
		DrawRect(track, Palette.Espresso);                                   // 1 px outline
		DrawRect(track.Grow(-1), Palette.Latte);                             // track

		// Target zone (the perfect zone) inside the 1 px border.
		double halfWidth = _config.PerfectHalfWidth > 0 ? _config.PerfectHalfWidth : TimingBarMath.PerfectHalfWidth(0.75);
		float inner = BarWidth - 2;
		int zoneLeft = (int)Math.Round(1 + (_target - halfWidth) * inner);
		int zoneRight = (int)Math.Round(1 + (_target + halfWidth) * inner);
		zoneLeft = Math.Max(zoneLeft, 1);
		zoneRight = Math.Min(zoneRight, BarWidth - 1);
		DrawRect(new Rect2(zoneLeft, barY + 1, Math.Max(zoneRight - zoneLeft, 1), BarHeight - 2), Palette.Matcha);
		DrawRect(new Rect2(zoneLeft, barY + 1, Math.Max(zoneRight - zoneLeft, 1), 1), Palette.Fern);

		// Marker, snapped to whole pixels.
		int x = (int)Math.Round(1 + MarkerPosition * (inner - MarkerWidth));
		DrawRect(new Rect2(x - 1, barY, MarkerWidth + 2, BarHeight), Palette.Espresso);
		DrawRect(new Rect2(x, barY + 1, MarkerWidth, BarHeight - 2), Palette.Apricot);
	}
}
