using System;

/// <summary>How a customer feels today. Drives patience, tips and the wording of their lines.</summary>
public enum Mood { Cheerful, Calm, Hurried, Sleepy }

/// <summary>Tuning numbers per mood (docs/GAME_RULES.md "Moods").</summary>
public static class MoodRules
{
	public const double CheerfulWeight = 30, CalmWeight = 35, HurriedWeight = 20, SleepyWeight = 15;
	public const double CheerfulPatience = 1.2, CalmPatience = 1.0, HurriedPatience = 0.7, SleepyPatience = 1.1;
	public const double CheerfulTip = 1.25, CalmTip = 1.0, HurriedTip = 0.9, SleepyTip = 1.0;

	public static double Weight(Mood m) => m switch
	{
		Mood.Cheerful => CheerfulWeight,
		Mood.Calm => CalmWeight,
		Mood.Hurried => HurriedWeight,
		Mood.Sleepy => SleepyWeight,
		_ => throw new ArgumentOutOfRangeException(nameof(m)),
	};

	public static double PatienceMultiplier(Mood m) => m switch
	{
		Mood.Cheerful => CheerfulPatience,
		Mood.Calm => CalmPatience,
		Mood.Hurried => HurriedPatience,
		Mood.Sleepy => SleepyPatience,
		_ => throw new ArgumentOutOfRangeException(nameof(m)),
	};

	public static double TipMultiplier(Mood m) => m switch
	{
		Mood.Cheerful => CheerfulTip,
		Mood.Calm => CalmTip,
		Mood.Hurried => HurriedTip,
		Mood.Sleepy => SleepyTip,
		_ => throw new ArgumentOutOfRangeException(nameof(m)),
	};
}
