using System;
using System.Collections.Generic;
using System.Linq;

public sealed record Arrival(double TimeSeconds, CustomerDef Customer, Mood Mood, ItemId Item);

/// <summary>Builds a day's arrival schedule. Deterministic: same seed + day + inputs => identical schedule.</summary>
public sealed class CustomerSpawner
{
	public const int BaseCustomers = 8, ExtraPerDay = 2, MaxCustomers = 20;
	public const double LastArrivalFraction = 0.90;
	public const double MinGapSeconds = 8;
	public const double DrinkWeight = 0.7, PastryBaseWeight = 0.3;
	public const double SpecialWeight = 2;

	// Wave share of the day's customers and arrival window (fractions of the day).
	private static readonly (double Share, double From, double To)[] Waves =
	{
		(0.40, 0.04, 0.24),
		(0.35, 0.40, 0.60),
		(0.25, 0.68, 0.88),
	};

	private readonly int _seed;

	public CustomerSpawner(int seed) => _seed = seed;

	public static int CustomerCount(int day) => Math.Min(BaseCustomers + ExtraPerDay * (Math.Max(1, day) - 1), MaxCustomers);

	public IReadOnlyList<Arrival> BuildSchedule(int day, double dayLengthSeconds, IReadOnlyCollection<MenuItem> available, DayPlan plan)
	{
		// A fresh Random per call keeps repeated calls identical (string/HashCode hashing is not stable, so use plain arithmetic).
		var rng = new Random(unchecked(_seed * 7919 + day));
		int total = CustomerCount(day);

		var times = new List<double>();
		int assigned = 0;
		for (int w = 0; w < Waves.Length; w++)
		{
			int n = w == Waves.Length - 1 ? total - assigned : (int)Math.Round(total * Waves[w].Share, MidpointRounding.AwayFromZero);
			assigned += n;
			for (int i = 0; i < n; i++)
				times.Add((Waves[w].From + rng.NextDouble() * (Waves[w].To - Waves[w].From)) * dayLengthSeconds);
		}
		times.Sort();
		SpaceOut(times, dayLengthSeconds * LastArrivalFraction);

		var stock = available.Where(i => i.Kind == ItemKind.Pastry).ToDictionary(i => i.Id, i => plan.GetStock(i.Id));
		var drinks = available.Where(i => i.Kind == ItemKind.Drink).ToList();
		var pastries = available.Where(i => i.Kind == ItemKind.Pastry).ToList();

		var result = new List<Arrival>();
		CustomerDef previous = null;
		foreach (double time in times)
		{
			var candidates = Roster.All.Where(c => c != previous).ToList();
			var customer = candidates[rng.Next(candidates.Count)];
			previous = customer;
			var mood = PickMood(rng);
			var item = PickItem(rng, customer, drinks, pastries, stock, plan);
			result.Add(new Arrival(time, customer, mood, item));
		}
		return result;
	}

	/// <summary>Forward pass enforces the minimum gap; if that pushes past the limit, a backward pass pulls them back.</summary>
	private static void SpaceOut(List<double> times, double limit)
	{
		for (int i = 1; i < times.Count; i++)
			times[i] = Math.Max(times[i], times[i - 1] + MinGapSeconds);
		if (times.Count > 0 && times[^1] > limit)
		{
			times[^1] = limit;
			for (int i = times.Count - 2; i >= 0; i--)
				times[i] = Math.Min(times[i], times[i + 1] - MinGapSeconds);
		}
	}

	private static Mood PickMood(Random rng)
	{
		var moods = Enum.GetValues<Mood>();
		double roll = rng.NextDouble() * moods.Sum(MoodRules.Weight);
		foreach (var m in moods)
		{
			roll -= MoodRules.Weight(m);
			if (roll < 0) return m;
		}
		return moods[^1];
	}

	private static ItemId PickItem(Random rng, CustomerDef customer, List<MenuItem> drinks, List<MenuItem> pastries,
		Dictionary<ItemId, int> stock, DayPlan plan)
	{
		var pastriesLeft = pastries.Where(p => stock[p.Id] > 0).ToList();
		double drinkW = drinks.Count > 0 ? DrinkWeight : 0;
		double pastryW = pastriesLeft.Count > 0 ? PastryBaseWeight * customer.PastryWeight : 0;
		if (drinkW + pastryW <= 0) throw new InvalidOperationException("Nothing available to order.");

		bool pastry = rng.NextDouble() * (drinkW + pastryW) >= drinkW;
		var pool = pastry ? pastriesLeft : drinks;
		double sum = pool.Sum(i => plan.IsSpecial(i.Id) ? SpecialWeight : 1);
		double roll = rng.NextDouble() * sum;
		MenuItem chosen = pool[^1];
		foreach (var i in pool)
		{
			roll -= plan.IsSpecial(i.Id) ? SpecialWeight : 1;
			if (roll < 0) { chosen = i; break; }
		}
		if (pastry) stock[chosen.Id]--;
		return chosen.Id;
	}
}
