using System;
using Xunit;

public class CafeMoneyTests
{
	private static MenuItem Latte => Menu.Get(ItemId.Latte);

	[Fact]
	public void Latte_perfect_cheerful_tips_4()
	{
		var r = TipCalculator.Calculate(Latte, 1, 1, Mood.Cheerful, false);
		Assert.Equal(4, r.Sale);
		Assert.Equal(4, r.Tip);
		Assert.Equal(8, r.Total);
	}

	[Fact]
	public void Latte_perfect_calm_tips_3() =>
		Assert.Equal(3, TipCalculator.Calculate(Latte, 1, 1, Mood.Calm, false).Tip);

	[Fact]
	public void Quality_zero_tips_nothing() =>
		Assert.Equal(0, TipCalculator.Calculate(Latte, 0, 0, Mood.Cheerful, false).Tip);

	[Fact]
	public void Cookie_half_quality_calm_rounds_up_to_1() =>
		Assert.Equal(1, TipCalculator.Calculate(Menu.Get(ItemId.Cookie), 0, 1, Mood.Calm, false).Tip);

	[Fact]
	public void Special_adds_one_even_at_quality_zero() =>
		Assert.Equal(1, TipCalculator.Calculate(Latte, 0, 0, Mood.Calm, true).Tip);

	[Fact]
	public void Half_rounds_away_from_zero()
	{
		// Cookie (2), quality 1, Calm: 1.5 -> 2 (Math.Round default would give 2 too, so also check 0.5 cases below).
		Assert.Equal(2, TipCalculator.Calculate(Menu.Get(ItemId.Cookie), 1, 1, Mood.Calm, false).Tip);
		// Espresso (3), quality 1, Calm: 2.25 -> 2.  Cake (5), quality 1, Calm: 3.75 -> 4.
		Assert.Equal(4, TipCalculator.Calculate(Menu.Get(ItemId.Cake), 1, 1, Mood.Calm, false).Tip);
		// Cookie (2), quality 0.5, Cheerful: 0.9375 -> 1.
		Assert.Equal(1, TipCalculator.Calculate(Menu.Get(ItemId.Cookie), 0.5, 0.5, Mood.Cheerful, false).Tip);
	}

	[Theory]
	[InlineData(Mood.Cheerful, 4)]
	[InlineData(Mood.Calm, 3)]
	[InlineData(Mood.Hurried, 3)]   // 4 x 0.9 x 0.75 = 2.7
	[InlineData(Mood.Sleepy, 3)]
	public void Every_mood_for_a_perfect_latte(Mood mood, int tip) =>
		Assert.Equal(tip, TipCalculator.Calculate(Latte, 1, 1, mood, false).Tip);

	[Fact]
	public void Inputs_are_clamped() =>
		Assert.Equal(3, TipCalculator.Calculate(Latte, 5, 9, Mood.Calm, false).Tip);
}

public class CafePatienceTests
{
	private static CustomerDef Def(float mult) => new(RegularId.Juniper, "x", "k", mult, 1f);

	[Fact]
	public void Drains_and_expires()
	{
		var m = new PatienceMeter(10);
		Assert.Equal(1, m.Remaining);
		m.Tick(2.5);
		Assert.Equal(0.75, m.Remaining, 6);
		Assert.False(m.Expired);
		m.Tick(100);
		Assert.True(m.Expired);
		Assert.Equal(0, m.Remaining);
		m.Tick(5);
		Assert.Equal(0, m.Remaining);
	}

	[Fact]
	public void Exactly_zero_is_expired()
	{
		var m = new PatienceMeter(4);
		m.Tick(4);
		Assert.True(m.Expired);
	}

	[Fact]
	public void Minimum_total_is_20_seconds() =>
		Assert.Equal(20, PatienceMeter.TotalSeconds(Def(0.1f), Mood.Hurried));

	[Theory]
	[InlineData(Mood.Cheerful, 72)]
	[InlineData(Mood.Calm, 60)]
	[InlineData(Mood.Hurried, 42)]
	[InlineData(Mood.Sleepy, 66)]
	public void Mood_multiplier(Mood mood, double seconds) =>
		Assert.Equal(seconds, PatienceMeter.TotalSeconds(Def(1f), mood), 4);

	[Fact]
	public void Customer_multiplier()
	{
		Assert.Equal(72, PatienceMeter.TotalSeconds(Roster.Get(RegularId.Sage), Mood.Calm), 4);
		Assert.Equal(42, PatienceMeter.TotalSeconds(Roster.Get(RegularId.Milo), Mood.Calm), 4);
		Assert.Equal(60 * 0.7 * 0.7 > 20 ? 60 * 0.7 * 0.7 : 20, PatienceMeter.TotalSeconds(Roster.Get(RegularId.Milo), Mood.Hurried), 4);
	}
}

public class CafeSummaryTests
{
	[Fact]
	public void Totals_add_up()
	{
		var s = new DaySummary();
		s.RecordServed(new TipResult(4, 3));
		s.RecordServed(new TipResult(3, 1));
		s.RecordWalkout();
		Assert.Equal(2, s.Served);
		Assert.Equal(1, s.WalkedOut);
		Assert.Equal(7, s.SalesCoins);
		Assert.Equal(4, s.TipCoins);
		Assert.Equal(11, s.TotalCoins);
		Assert.Empty(s.NewUnlocksTomorrow);
	}
}

public class CafeCounterSlotsTests
{
	[Fact]
	public void Fills_then_reports_full()
	{
		var c = new CounterSlots();
		Assert.Equal(3, c.Count);
		Assert.Equal(0, c.TryOccupy());
		Assert.Equal(1, c.TryOccupy());
		Assert.Equal(2, c.TryOccupy());
		Assert.False(c.HasFree);
		Assert.Equal(-1, c.TryOccupy());
	}

	[Fact]
	public void Release_frees_and_double_release_is_harmless()
	{
		var c = new CounterSlots();
		for (int i = 0; i < 3; i++) c.TryOccupy();
		c.Release(1);
		c.Release(1);
		c.Release(99);
		Assert.True(c.HasFree);
		Assert.Equal(1, c.TryOccupy());
		Assert.Equal(-1, c.TryOccupy());
	}
}

public class CafeChatLinesTests
{
	[Fact]
	public void Never_empty_for_any_combination()
	{
		var rng = new Random(1);
		var item = Menu.Get(ItemId.Latte);
		foreach (var c in Roster.All)
			foreach (var m in Enum.GetValues<Mood>())
				for (int i = 0; i < 20; i++)
				{
					Assert.False(string.IsNullOrWhiteSpace(ChatLines.OrderLine(c, m, item, rng)));
					Assert.False(string.IsNullOrWhiteSpace(ChatLines.ThanksLine(c, m, rng)));
					Assert.False(string.IsNullOrWhiteSpace(ChatLines.SmallTalk(c, m, rng)));
				}
	}

	[Fact]
	public void Order_line_names_the_item_and_has_no_placeholder()
	{
		var line = ChatLines.OrderLine(Roster.Get(RegularId.Rosie), Mood.Hurried, Menu.Get(ItemId.IcedCoffee), new Random(3));
		Assert.Contains("iced coffee", line);
		Assert.DoesNotContain("{", line);
	}

	[Fact]
	public void Deterministic_for_a_seed()
	{
		var c = Roster.Get(RegularId.Theo);
		for (int seed = 0; seed < 10; seed++)
			Assert.Equal(ChatLines.SmallTalk(c, Mood.Calm, new Random(seed)), ChatLines.SmallTalk(c, Mood.Calm, new Random(seed)));
	}

	[Fact]
	public void Each_regular_has_at_least_three_small_talk_lines()
	{
		foreach (var c in Roster.All)
		{
			var seen = new System.Collections.Generic.HashSet<string>();
			var rng = new Random(5);
			for (int i = 0; i < 200; i++) seen.Add(ChatLines.SmallTalk(c, Mood.Calm, rng));
			Assert.True(seen.Count >= 3, c.Name);
		}
	}

	[Fact]
	public void Order_and_thanks_have_at_least_two_templates()
	{
		var item = Menu.Get(ItemId.Latte);
		var orders = new System.Collections.Generic.HashSet<string>();
		var thanks = new System.Collections.Generic.HashSet<string>();
		var rng = new Random(7);
		var c = Roster.Get(RegularId.Juniper);
		for (int i = 0; i < 200; i++)
		{
			orders.Add(ChatLines.OrderLine(c, Mood.Calm, item, rng));
			thanks.Add(ChatLines.ThanksLine(c, Mood.Calm, rng));
		}
		Assert.True(orders.Count >= 2);
		Assert.True(thanks.Count >= 2);
	}
}
