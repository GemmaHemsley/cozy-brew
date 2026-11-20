using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Today's prep choices: the special, grind quality and pastry-case stock.</summary>
public sealed class DayPlan
{
	public const int PastryCapacity = 12;
	public const double MinGrindQuality = 0.5, MaxGrindQuality = 1.0, DefaultGrindQuality = 0.75;

	private readonly Dictionary<ItemId, int> _stock = new();
	private double _grind = DefaultGrindQuality;

	public ItemId Special { get; set; } = ItemId.Latte;

	public double GrindQuality
	{
		get => _grind;
		set => _grind = Math.Clamp(value, MinGrindQuality, MaxGrindQuality);
	}

	public int GetStock(ItemId pastry) => _stock.GetValueOrDefault(pastry);

	public int TotalStock => _stock.Values.Sum();

	/// <summary>Sets the stock of one pastry. False (no change) if not a pastry, negative, or over capacity.</summary>
	public bool TrySetStock(ItemId pastry, int count)
	{
		if (count < 0 || Menu.Get(pastry).Kind != ItemKind.Pastry) return false;
		if (TotalStock - GetStock(pastry) + count > PastryCapacity) return false;
		_stock[pastry] = count;
		return true;
	}

	/// <summary>Takes one pastry out of the case. False if none left.</summary>
	public bool TryTakePastry(ItemId pastry)
	{
		int have = GetStock(pastry);
		if (have <= 0) return false;
		_stock[pastry] = have - 1;
		return true;
	}

	public bool IsSpecial(ItemId item) => item == Special;

	/// <summary>Splits capacity evenly over the available pastries (leftovers go to the earlier ones).</summary>
	public static DayPlan CreateDefault(IEnumerable<MenuItem> available)
	{
		var plan = new DayPlan();
		var pastries = available.Where(i => i.Kind == ItemKind.Pastry).ToList();
		if (pastries.Count == 0) return plan;
		int each = PastryCapacity / pastries.Count, extra = PastryCapacity % pastries.Count;
		for (int i = 0; i < pastries.Count; i++)
			plan._stock[pastries[i].Id] = each + (i < extra ? 1 : 0);
		return plan;
	}
}
