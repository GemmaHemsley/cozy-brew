using System;
using Godot;

/// <summary>Headless checks for WP-D: both brew scenes instantiate and behave.</summary>
public class BrewSmoke : ISmokeSuite
{
	public string Name => "Brew";

	public void Run(SmokeCheck t)
	{
		var barScene = GD.Load<PackedScene>("res://scenes/brew/TimingBar.tscn");
		var panelScene = GD.Load<PackedScene>("res://scenes/brew/BrewPanel.tscn");
		t.That(barScene != null && panelScene != null, "TimingBar.tscn and BrewPanel.tscn load");

		// TimingBar: begin, stop, signal fires once with 0..1.
		var bar = barScene.Instantiate<TimingBar>();
		t.Root.AddChild(bar);
		int stops = 0;
		double acc = -1;
		bar.Stopped += a => { stops++; acc = a; };
		bar.Begin("Pull the shot", TimingBarMath.ConfigFor(BrewStep.Shot, 0.75), new Random(1));
		t.That(bar.Running && bar.Target >= 0.30 && bar.Target <= 0.70, "TimingBar runs with a target in 0.30..0.70");
		bar.StopNow();
		t.That(stops == 1 && acc >= 0 && acc <= 1 && !bar.Running, "TimingBar.StopNow emits Stopped(accuracy 0..1) once");
		bar.StopNow();
		t.That(stops == 1, "a second stop is ignored");
		bar.QueueFree();

		// BrewPanel with a pastry: finishes 1.0 on the next frame.
		var panel = panelScene.Instantiate<BrewPanel>();
		t.Root.AddChild(panel);
		int finished = 0;
		double result = -1;
		panel.BrewFinished += a => { finished++; result = a; };
		panel.Begin(Menu.Get(ItemId.Croissant), 0.75);
		t.That(finished == 0 && !panel.Bar.Running, "pastry: nothing shown, finish is deferred");
		panel.RunPendingForTests();
		t.That(finished == 1 && result == 1.0, "pastry: BrewFinished(1.0)");

		// Latte: two steps.
		finished = 0;
		panel.Begin(Menu.Get(ItemId.Latte), 0.75);
		t.That(panel.StepCount == 2 && panel.StepText == "Step 1/2" && panel.Bar.Running, "latte shows Step 1/2 of 2");
		panel.Bar.StopNow();
		panel.SkipPauseForTests();
		t.That(panel.StepText == "Step 2/2" && panel.Bar.Running, "latte advances to Step 2/2");
		panel.Bar.StopNow();
		panel.SkipPauseForTests();
		t.That(finished == 1 && result >= 0 && result <= 1, "latte emits BrewFinished(0..1) after both steps");

		// Iced coffee: three steps.
		panel.Begin(Menu.Get(ItemId.IcedCoffee), 1.0);
		t.That(panel.StepCount == 3, "iced coffee has 3 steps");

		// Re-Begin with a bar still running (iced coffee) as a pastry: the old bar must not fire into the empty recipe.
		finished = 0;
		panel.Begin(Menu.Get(ItemId.Croissant), 0.75);
		t.That(!panel.Bar.Running && panel.StepCount == 0, "re-Begin(pastry) cancels the running bar");
		t.That(panel.Bar.StopNow() == -1, "cancelled bar ignores StopNow (no crash)");
		panel.RunPendingForTests();
		t.That(finished == 1 && result == 1.0, "pastry after a drink finishes 1.0");

		// Panel removed and re-added keeps working (subscription survives re-entry).
		finished = 0;
		t.Root.RemoveChild(panel);
		t.Root.AddChild(panel);
		panel.Begin(Menu.Get(ItemId.Latte), 0.75);
		panel.Bar.StopNow();
		panel.SkipPauseForTests();
		t.That(panel.StepText == "Step 2/2", "panel advances after remove and re-add");

		// Panel sits on whole pixels.
		var pos = panel.GetChild<PanelContainer>(0).Position;
		t.That(pos == pos.Floor(), "panel position is whole pixels");

		// Removed during the feedback pause, then re-added: the advance is re-armed instead of stalling.
		panel.Begin(Menu.Get(ItemId.Latte), 0.75);
		panel.Bar.StopNow();
		t.That(panel.AdvancePending, "advance is pending after a step stops");
		t.Root.RemoveChild(panel);
		t.That(panel.AdvancePending, "advance stays pending when the panel leaves the tree");
		t.Root.AddChild(panel);
		t.That(panel.AdvancePending && panel.StepText == "Step 1/2", "re-added mid-pause: advance re-armed");
		panel.SkipPauseForTests();
		t.That(!panel.AdvancePending && panel.StepText == "Step 2/2", "advance completes after re-arm");

		// Centring: Begin before the panel has a size, then layout gives it one.
		var late = panelScene.Instantiate<BrewPanel>();
		late.Begin(Menu.Get(ItemId.Latte), 0.75); // before _Ready: pending path, Size is (0,0)
		t.Root.AddChild(late);
		late.SetAnchorsPreset(Control.LayoutPreset.TopLeft); // full-rect anchors would override the Size we set below
		late.Size = new Vector2(320, 180);
		var inner = late.GetChild<PanelContainer>(0);
		t.That(inner.Position == ((late.Size - inner.Size) / 2).Floor(), "panel centred after Resized");
		late.Size = new Vector2(321, 181);
		t.That(inner.Position == ((late.Size - inner.Size) / 2).Floor() && inner.Position == inner.Position.Floor(), "panel re-centred on odd size, whole pixels");
		late.QueueFree();

		// Accuracy just under 1.0 is not labelled Perfect.
		t.That(TimingBar.PerfectAccuracy > 0.99, "Perfect label needs the real perfect zone");
		panel.QueueFree();
	}
}
