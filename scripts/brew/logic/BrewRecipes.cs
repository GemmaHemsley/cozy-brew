using System;
using System.Collections.Generic;

/// <summary>Which minigame steps each menu item needs (docs/GAME_RULES.md, Brewing).</summary>
public static class BrewRecipes
{
	private static readonly BrewStep[] None = Array.Empty<BrewStep>();

	/// <summary>Steps in order. Pastries return an empty list (no minigame).</summary>
	public static IReadOnlyList<BrewStep> StepsFor(ItemId item) => item switch
	{
		ItemId.Espresso => new[] { BrewStep.Shot },
		ItemId.Latte => new[] { BrewStep.Shot, BrewStep.Milk },
		ItemId.Matcha => new[] { BrewStep.Matcha, BrewStep.Milk },
		ItemId.Cocoa => new[] { BrewStep.Cocoa, BrewStep.Milk },
		ItemId.IcedCoffee => new[] { BrewStep.Shot, BrewStep.Ice, BrewStep.Milk },
		_ => None,
	};
}
