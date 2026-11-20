using System.Collections.Generic;

/// <summary>What happened today, for the Close screen.</summary>
public sealed class DaySummary
{
	public int Served { get; private set; }
	public int WalkedOut { get; private set; }
	public int SalesCoins { get; private set; }
	public int TipCoins { get; private set; }
	public int TotalCoins => SalesCoins + TipCoins;
	public List<ItemId> NewUnlocksTomorrow { get; } = new();

	public void RecordServed(TipResult result)
	{
		Served++;
		SalesCoins += result.Sale;
		TipCoins += result.Tip;
	}

	public void RecordWalkout() => WalkedOut++;
}
