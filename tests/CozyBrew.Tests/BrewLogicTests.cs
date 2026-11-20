using System;
using System.Linq;
using Xunit;

public class BrewLogicTests
{
	[Theory]
	[InlineData(0.7)]
	[InlineData(0.9)]
	[InlineData(1.0)]
	public void Position_triangle_wave(double speed)
	{
		double period = 1.0 / speed;
		Assert.Equal(0.0, TimingBarMath.Position(0, speed), 9);
		Assert.Equal(0.5, TimingBarMath.Position(period / 4, speed), 9);
		Assert.Equal(1.0, TimingBarMath.Position(period / 2, speed), 9);
		Assert.Equal(0.5, TimingBarMath.Position(period * 0.75, speed), 9);
		Assert.Equal(0.0, TimingBarMath.Position(period, speed), 9);
		Assert.Equal(0.5, TimingBarMath.Position(period * 1.25, speed), 9);
	}

	[Fact]
	public void Accuracy_is_one_inside_perfect_zone()
	{
		Assert.Equal(1.0, TimingBarMath.Accuracy(0.5, 0.5, 0.08, 0.35));
		Assert.Equal(1.0, TimingBarMath.Accuracy(0.58, 0.5, 0.08, 0.35), 9);
		Assert.Equal(1.0, TimingBarMath.Accuracy(0.42, 0.5, 0.08, 0.35), 9);
	}

	[Fact]
	public void Accuracy_is_zero_at_and_beyond_zero_distance()
	{
		Assert.Equal(0.0, TimingBarMath.Accuracy(0.85, 0.5, 0.08, 0.35), 9);
		Assert.Equal(0.0, TimingBarMath.Accuracy(1.0, 0.3, 0.08, 0.35));
		Assert.Equal(0.0, TimingBarMath.Accuracy(0.0, 0.7, 0.08, 0.35));
	}

	[Fact]
	public void Accuracy_is_linear_between()
	{
		// halfway between 0.08 and 0.35 is 0.215
		Assert.Equal(0.5, TimingBarMath.Accuracy(0.5 + 0.215, 0.5, 0.08, 0.35), 9);
		Assert.Equal(0.5, TimingBarMath.Accuracy(0.5 - 0.215, 0.5, 0.08, 0.35), 9);
	}

	[Fact]
	public void Accuracy_is_symmetric_and_bounded()
	{
		for (double p = 0; p <= 1.0; p += 0.01)
		{
			double a = TimingBarMath.Accuracy(0.5 + p / 2, 0.5, 0.07, 0.35);
			double b = TimingBarMath.Accuracy(0.5 - p / 2, 0.5, 0.07, 0.35);
			Assert.Equal(a, b, 9);
			Assert.InRange(a, 0.0, 1.0);
		}
	}

	[Theory]
	[InlineData(0.5, 0.07)]
	[InlineData(0.75, 0.085)]
	[InlineData(1.0, 0.10)]
	public void PerfectHalfWidth_for_grind(double grind, double expected)
	{
		Assert.Equal(expected, TimingBarMath.PerfectHalfWidth(grind), 9);
	}

	[Theory]
	[InlineData(BrewStep.Shot, 0.7)]
	[InlineData(BrewStep.Milk, 0.9)]
	[InlineData(BrewStep.Matcha, 0.8)]
	[InlineData(BrewStep.Cocoa, 0.8)]
	[InlineData(BrewStep.Ice, 1.0)]
	public void ConfigFor_uses_rule_speeds(BrewStep step, double speed)
	{
		var c = TimingBarMath.ConfigFor(step, 0.75);
		Assert.Equal(speed, c.CyclesPerSecond);
		Assert.Equal(0.085, c.PerfectHalfWidth, 9);
		Assert.Equal(0.35, c.ZeroAccuracyDistance);
	}

	[Theory]
	[InlineData(ItemId.Espresso, new[] { BrewStep.Shot })]
	[InlineData(ItemId.Latte, new[] { BrewStep.Shot, BrewStep.Milk })]
	[InlineData(ItemId.Matcha, new[] { BrewStep.Matcha, BrewStep.Milk })]
	[InlineData(ItemId.Cocoa, new[] { BrewStep.Cocoa, BrewStep.Milk })]
	[InlineData(ItemId.IcedCoffee, new[] { BrewStep.Shot, BrewStep.Ice, BrewStep.Milk })]
	[InlineData(ItemId.Croissant, new BrewStep[0])]
	[InlineData(ItemId.Muffin, new BrewStep[0])]
	[InlineData(ItemId.Cookie, new BrewStep[0])]
	[InlineData(ItemId.Cake, new BrewStep[0])]
	public void Recipes_match_rules(ItemId item, BrewStep[] expected)
	{
		Assert.Equal(expected, BrewRecipes.StepsFor(item).ToArray());
	}

	[Fact]
	public void Every_item_has_a_recipe_entry()
	{
		foreach (ItemId id in Enum.GetValues(typeof(ItemId)))
			Assert.NotNull(BrewRecipes.StepsFor(id));
	}

	[Fact]
	public void Combine_is_mean_and_empty_is_one()
	{
		Assert.Equal(1.0, TimingBarMath.Combine(new double[0]));
		Assert.Equal(0.5, TimingBarMath.Combine(new[] { 1.0, 0.0 }), 9);
		Assert.Equal(0.6, TimingBarMath.Combine(new[] { 1.0, 0.5, 0.3 }), 9);
	}
}
