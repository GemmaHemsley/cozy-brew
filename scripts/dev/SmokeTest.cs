using System;
using System.Linq;
using Godot;

/// <summary>
/// Headless smoke test. Run from the repo root:
///   $GODOT --headless --path . res://scenes/dev/SmokeTest.tscn
/// Exit code 0 = all checks passed. It runs every <see cref="ISmokeSuite"/> in the assembly. Not part of the game.
/// </summary>
public partial class SmokeTest : Node
{
	public override void _Ready()
	{
		var check = new SmokeCheck(this);
		var suites = typeof(SmokeTest).Assembly.GetTypes()
			.Where(t => typeof(ISmokeSuite).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
			.OrderBy(t => t.Name);

		foreach (var type in suites)
		{
			var suite = (ISmokeSuite)Activator.CreateInstance(type);
			GD.Print($"[{suite.Name}]");
			try
			{
				suite.Run(check);
			}
			catch (Exception e)
			{
				check.That(false, $"{suite.Name} threw {e.GetType().Name}: {e.Message}");
			}
		}

		GD.Print(check.Failures == 0
			? $"SMOKE OK ({check.Passes} checks)"
			: $"SMOKE FAILED ({check.Failures} failed, {check.Passes} passed)");
		GetTree().Quit(check.Failures == 0 ? 0 : 1);
	}
}
