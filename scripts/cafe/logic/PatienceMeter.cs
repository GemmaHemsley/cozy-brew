using System;

/// <summary>Counts a customer's patience down in real seconds. Pure logic: the caller feeds it frame deltas.</summary>
public sealed class PatienceMeter
{
	public const double BaseSeconds = 60;
	public const double MinSeconds = 20;

	private readonly double _total;
	private double _left;

	public PatienceMeter(double totalSeconds)
	{
		_total = Math.Max(0, totalSeconds);
		_left = _total;
	}

	/// <summary>max(20, 60 x mood x customer multiplier).</summary>
	public static double TotalSeconds(CustomerDef customer, Mood mood) =>
		Math.Max(MinSeconds, BaseSeconds * MoodRules.PatienceMultiplier(mood) * customer.PatienceMultiplier);

	/// <summary>Fraction left, 0..1.</summary>
	public double Remaining => _total <= 0 ? 0 : Math.Clamp(_left / _total, 0, 1);

	public bool Expired => _left <= 0;

	public void Tick(double deltaSeconds)
	{
		if (deltaSeconds <= 0) return;
		_left = Math.Max(0, _left - deltaSeconds);
	}
}
