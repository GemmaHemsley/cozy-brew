using System;

public readonly record struct TipResult(int Sale, int Tip)
{
	public int Total => Sale + Tip;
}

/// <summary>Money for one served order (docs/GAME_RULES.md "Money").</summary>
public static class TipCalculator
{
	public const double PatienceShare = 0.5;     // quality = 0.5 x patience + 0.5 x accuracy
	public const double TipFactor = 0.75;
	public const int SpecialBonus = 1;

	public static TipResult Calculate(MenuItem item, double patienceRemaining, double accuracy, Mood mood, bool isSpecial)
	{
		double quality = PatienceShare * Math.Clamp(patienceRemaining, 0, 1)
			+ (1 - PatienceShare) * Math.Clamp(accuracy, 0, 1);
		double raw = item.Price * quality * MoodRules.TipMultiplier(mood) * TipFactor;
		int tip = (int)Math.Round(raw, MidpointRounding.AwayFromZero) + (isSpecial ? SpecialBonus : 0);
		return new TipResult(item.Price, tip);
	}
}
