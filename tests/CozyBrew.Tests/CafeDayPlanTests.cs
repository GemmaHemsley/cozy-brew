using System.Linq;
using Xunit;

public class CafeDayPlanTests
{
	[Fact]
	public void Defaults()
	{
		var p = new DayPlan();
		Assert.Equal(ItemId.Latte, p.Special);
		Assert.Equal(0.75, p.GrindQuality);
		Assert.True(p.IsSpecial(ItemId.Latte));
		Assert.False(p.IsSpecial(ItemId.Cocoa));
	}

	[Fact]
	public void Grind_quality_is_clamped()
	{
		var p = new DayPlan { GrindQuality = 3 };
		Assert.Equal(1.0, p.GrindQuality);
		p.GrindQuality = 0;
		Assert.Equal(0.5, p.GrindQuality);
	}

	[Fact]
	public void Capacity_limit()
	{
		var p = new DayPlan();
		Assert.True(p.TrySetStock(ItemId.Croissant, 12));
		Assert.False(p.TrySetStock(ItemId.Muffin, 1));
		Assert.True(p.TrySetStock(ItemId.Croissant, 6));
		Assert.True(p.TrySetStock(ItemId.Muffin, 6));
		Assert.False(p.TrySetStock(ItemId.Cookie, 1));
		Assert.False(p.TrySetStock(ItemId.Cookie, -1));
		Assert.Equal(6, p.GetStock(ItemId.Croissant));
	}

	[Fact]
	public void Drinks_cannot_be_stocked() => Assert.False(new DayPlan().TrySetStock(ItemId.Latte, 1));

	[Fact]
	public void TryTakePastry_stops_at_zero()
	{
		var p = new DayPlan();
		p.TrySetStock(ItemId.Cookie, 2);
		Assert.True(p.TryTakePastry(ItemId.Cookie));
		Assert.True(p.TryTakePastry(ItemId.Cookie));
		Assert.False(p.TryTakePastry(ItemId.Cookie));
		Assert.Equal(0, p.GetStock(ItemId.Cookie));
		Assert.False(p.TryTakePastry(ItemId.Muffin));
	}

	[Fact]
	public void Default_day1_splits_12_over_3()
	{
		var p = DayPlan.CreateDefault(Menu.AvailableOnDay(1));
		Assert.Equal(4, p.GetStock(ItemId.Croissant));
		Assert.Equal(4, p.GetStock(ItemId.Muffin));
		Assert.Equal(4, p.GetStock(ItemId.Cookie));
		Assert.Equal(0, p.GetStock(ItemId.Cake));
	}

	[Fact]
	public void Default_day5_splits_12_over_4()
	{
		var p = DayPlan.CreateDefault(Menu.AvailableOnDay(5));
		foreach (var id in new[] { ItemId.Croissant, ItemId.Muffin, ItemId.Cookie, ItemId.Cake })
			Assert.Equal(3, p.GetStock(id));
	}

	[Fact]
	public void Default_with_uneven_split_still_uses_capacity()
	{
		var two = Menu.All.Where(i => i.Id == ItemId.Croissant || i.Id == ItemId.Cake).Concat(Menu.All.Where(i => i.Id == ItemId.Muffin));
		var p = DayPlan.CreateDefault(two.Take(3));
		Assert.Equal(12, p.TotalStock);
	}
}
