using System.Collections.Generic;
using System.Linq;
using Xunit;

public class CafeSpawnerTests
{
	private const double Day = 360;

	private static IReadOnlyList<Arrival> Build(int seed, int day, DayPlan plan = null, double len = Day)
	{
		var avail = Menu.AvailableOnDay(day).ToList();
		plan ??= DayPlan.CreateDefault(avail);
		return new CustomerSpawner(seed).BuildSchedule(day, len, avail, plan);
	}

	[Fact]
	public void Same_seed_same_schedule()
	{
		Assert.Equal(Build(42, 3), Build(42, 3));
	}

	[Fact]
	public void Repeated_calls_on_one_spawner_match()
	{
		var avail = Menu.AvailableOnDay(2).ToList();
		var plan = DayPlan.CreateDefault(avail);
		var s = new CustomerSpawner(9);
		Assert.Equal(s.BuildSchedule(2, Day, avail, plan), s.BuildSchedule(2, Day, avail, plan));
	}

	[Fact]
	public void Different_seeds_differ() =>
		Assert.NotEqual(Build(1, 3).Select(a => a.TimeSeconds), Build(2, 3).Select(a => a.TimeSeconds));

	[Theory]
	[InlineData(1, 8)]
	[InlineData(2, 10)]
	[InlineData(5, 16)]
	[InlineData(7, 20)]
	[InlineData(30, 20)]
	public void Customer_count(int day, int expected)
	{
		Assert.Equal(expected, CustomerSpawner.CustomerCount(day));
		Assert.Equal(expected, Build(5, day).Count);
	}

	[Fact]
	public void Timing_invariants_hold_over_many_seeds()
	{
		for (int seed = 0; seed < 200; seed++)
			foreach (int day in new[] { 1, 4, 7, 12 })
			{
				var s = Build(seed, day);
				for (int i = 0; i < s.Count; i++)
				{
					Assert.InRange(s[i].TimeSeconds, 0, Day * 0.9 + 1e-9);
					if (i > 0) Assert.True(s[i].TimeSeconds - s[i - 1].TimeSeconds >= 8 - 1e-9, $"seed {seed} day {day} gap at {i}");
				}
			}
	}

	[Fact]
	public void Sorted()
	{
		var t = Build(3, 6).Select(a => a.TimeSeconds).ToList();
		Assert.Equal(t.OrderBy(x => x).ToList(), t);
	}

	[Fact]
	public void Never_the_same_regular_twice_in_a_row()
	{
		for (int seed = 0; seed < 100; seed++)
		{
			var s = Build(seed, 7);
			for (int i = 1; i < s.Count; i++)
				Assert.NotEqual(s[i - 1].Customer.Regular, s[i].Customer.Regular);
		}
	}

	[Fact]
	public void Pastry_orders_never_exceed_stock()
	{
		for (int seed = 0; seed < 200; seed++)
		{
			var avail = Menu.AvailableOnDay(5).ToList();
			var plan = DayPlan.CreateDefault(avail);
			plan.TrySetStock(ItemId.Cake, 0);
			plan.TrySetStock(ItemId.Croissant, 1);
			plan.TrySetStock(ItemId.Muffin, 2);
			plan.TrySetStock(ItemId.Cookie, 0);
			var s = new CustomerSpawner(seed).BuildSchedule(5, Day, avail, plan);
			foreach (var id in new[] { ItemId.Croissant, ItemId.Muffin, ItemId.Cookie, ItemId.Cake })
				Assert.True(s.Count(a => a.Item == id) <= plan.GetStock(id), $"seed {seed} {id}");
		}
	}

	[Fact]
	public void Empty_pastry_case_means_drinks_only()
	{
		var avail = Menu.AvailableOnDay(1).ToList();
		var plan = new DayPlan();
		var s = new CustomerSpawner(1).BuildSchedule(1, Day, avail, plan);
		Assert.All(s, a => Assert.Equal(ItemKind.Drink, Menu.Get(a.Item).Kind));
	}

	[Fact]
	public void Only_available_items_and_day1_has_no_matcha_or_cake()
	{
		for (int seed = 0; seed < 100; seed++)
		{
			var s = Build(seed, 1);
			Assert.DoesNotContain(s, a => a.Item == ItemId.Matcha || a.Item == ItemId.Cake);
			var ids = Menu.AvailableOnDay(1).Select(i => i.Id).ToHashSet();
			Assert.All(s, a => Assert.Contains(a.Item, ids));
		}
	}

	[Fact]
	public void Special_is_ordered_more_often()
	{
		int special = 0, other = 0;
		for (int seed = 0; seed < 300; seed++)
		{
			var avail = Menu.AvailableOnDay(2).ToList();
			var plan = DayPlan.CreateDefault(avail);
			plan.Special = ItemId.Cocoa;
			foreach (var a in new CustomerSpawner(seed).BuildSchedule(2, Day, avail, plan))
				if (a.Item == ItemId.Cocoa) special++; else if (a.Item == ItemId.Espresso) other++;
		}
		Assert.True(special > other * 1.5, $"special {special} vs normal {other}");
	}

	[Fact]
	public void Rosie_orders_more_pastries()
	{
		int rosie = 0, rosieN = 0, others = 0, othersN = 0;
		for (int seed = 0; seed < 300; seed++)
			foreach (var a in Build(seed, 7))
			{
				bool pastry = Menu.Get(a.Item).Kind == ItemKind.Pastry;
				if (a.Customer.Regular == RegularId.Rosie) { rosieN++; if (pastry) rosie++; }
				else { othersN++; if (pastry) others++; }
			}
		Assert.True((double)rosie / rosieN > (double)others / othersN);
	}

	[Fact]
	public void Moods_all_appear()
	{
		var moods = Enumerable.Range(0, 50).SelectMany(s => Build(s, 7)).Select(a => a.Mood).Distinct().ToList();
		Assert.Equal(4, moods.Count);
	}

	[Fact]
	public void Roster_matches_rules()
	{
		Assert.Equal(5, Roster.All.Count);
		Assert.Equal(ArtKeys.CustFox, Roster.Get(RegularId.Juniper).ArtKey);
		Assert.Equal(0.7f, Roster.Get(RegularId.Milo).PatienceMultiplier);
		Assert.Equal(2f, Roster.Get(RegularId.Rosie).PastryWeight);
	}
}
