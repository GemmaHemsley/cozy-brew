using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>The menu, with the starting prices from CLAUDE.md. All tuning for items lives here.</summary>
public static class Menu
{
	public static readonly IReadOnlyList<MenuItem> All = new[]
	{
		new MenuItem(ItemId.Latte, "Latte", 4, ItemKind.Drink, "espresso + steamed milk", ArtKeys.Latte),
		new MenuItem(ItemId.Espresso, "Espresso", 3, ItemKind.Drink, "espresso shot", ArtKeys.Espresso),
		new MenuItem(ItemId.Matcha, "Matcha", 5, ItemKind.Drink, "matcha + milk", ArtKeys.Matcha, UnlockDay: 2),
		new MenuItem(ItemId.Cocoa, "Cocoa", 4, ItemKind.Drink, "cocoa + milk + marshmallow", ArtKeys.Cocoa),
		new MenuItem(ItemId.IcedCoffee, "Iced coffee", 4, ItemKind.Drink, "cold brew + ice + milk", ArtKeys.IcedCoffee),
		new MenuItem(ItemId.Croissant, "Croissant", 3, ItemKind.Pastry, "pastry case", ArtKeys.Croissant),
		new MenuItem(ItemId.Muffin, "Muffin", 3, ItemKind.Pastry, "pastry case", ArtKeys.Muffin),
		new MenuItem(ItemId.Cookie, "Cookie", 2, ItemKind.Pastry, "pastry case", ArtKeys.Cookie),
		new MenuItem(ItemId.Cake, "Cake", 5, ItemKind.Pastry, "pastry case", ArtKeys.Cake, UnlockDay: 5),
	};

	private static readonly Dictionary<ItemId, MenuItem> ById = All.ToDictionary(item => item.Id);

	public static MenuItem Get(ItemId id) =>
		ById.TryGetValue(id, out var item) ? item : throw new ArgumentOutOfRangeException(nameof(id), id, "Not on the menu.");

	/// <summary>Items whose <see cref="MenuItem.UnlockDay"/> has been reached.</summary>
	public static IEnumerable<MenuItem> AvailableOnDay(int day) => All.Where(item => item.UnlockDay <= day);
}
