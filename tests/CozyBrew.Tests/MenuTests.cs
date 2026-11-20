using System.Linq;
using Xunit;

public class MenuTests
{
	[Theory]
	[InlineData(ItemId.Latte, 4)]
	[InlineData(ItemId.Espresso, 3)]
	[InlineData(ItemId.Matcha, 5)]
	[InlineData(ItemId.Cocoa, 4)]
	[InlineData(ItemId.IcedCoffee, 4)]
	[InlineData(ItemId.Croissant, 3)]
	[InlineData(ItemId.Muffin, 3)]
	[InlineData(ItemId.Cookie, 2)]
	[InlineData(ItemId.Cake, 5)]
	public void Prices_match_claude_md(ItemId id, int price)
	{
		Assert.Equal(price, Menu.Get(id).Price);
	}

	[Fact]
	public void Every_item_id_is_on_the_menu_once()
	{
		Assert.Equal(System.Enum.GetValues<ItemId>().Length, Menu.All.Count);
		Assert.Equal(Menu.All.Count, Menu.All.Select(i => i.Id).Distinct().Count());
	}

	[Theory]
	[InlineData(1, 7)]
	[InlineData(2, 8)]
	[InlineData(4, 8)]
	[InlineData(5, 9)]
	public void Unlocks_by_day(int day, int expectedCount)
	{
		Assert.Equal(expectedCount, Menu.AvailableOnDay(day).Count());
	}
}
