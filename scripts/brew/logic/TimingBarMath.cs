using System;
using System.Collections.Generic;

/// <summary>Pure maths for the timing bar (no Godot). Numbers come from docs/GAME_RULES.md.</summary>
public static class TimingBarMath
{
	public const double ShotSpeed = 0.7;
	public const double MilkSpeed = 0.9;
	public const double MatchaSpeed = 0.8;
	public const double CocoaSpeed = 0.8;
	public const double IceSpeed = 1.0;

	/// <summary>Distance from the target at which accuracy reaches 0.</summary>
	public const double ZeroAccuracyDistance = 0.35;
	public const double PerfectHalfWidthBase = 0.04;
	public const double PerfectHalfWidthPerGrind = 0.06;
	public const double TargetMin = 0.30;
	public const double TargetMax = 0.70;

	/// <summary>Triangle wave 0..1..0: starts at 0, peaks at half a period (1 / cyclesPerSecond).</summary>
	public static double Position(double elapsedSeconds, double cyclesPerSecond)
	{
		double phase = elapsedSeconds * cyclesPerSecond;
		phase -= Math.Floor(phase); // 0..1 within the cycle
		return phase < 0.5 ? phase * 2 : (1 - phase) * 2;
	}

	/// <summary>1 inside the perfect zone, falling linearly to 0 at <paramref name="zeroDistance"/>.</summary>
	public static double Accuracy(double position, double target, double perfectHalfWidth, double zeroDistance)
	{
		double distance = Math.Abs(position - target);
		double span = zeroDistance - perfectHalfWidth;
		if (span <= 0)
			return distance <= perfectHalfWidth ? 1.0 : 0.0;
		return Math.Clamp(1.0 - (distance - perfectHalfWidth) / span, 0.0, 1.0);
	}

	public static double PerfectHalfWidth(double grindQuality) =>
		PerfectHalfWidthBase + PerfectHalfWidthPerGrind * grindQuality;

	public static TimingBarConfig ConfigFor(BrewStep step, double grindQuality)
	{
		double speed = step switch
		{
			BrewStep.Shot => ShotSpeed,
			BrewStep.Milk => MilkSpeed,
			BrewStep.Matcha => MatchaSpeed,
			BrewStep.Cocoa => CocoaSpeed,
			BrewStep.Ice => IceSpeed,
			_ => ShotSpeed,
		};
		return new TimingBarConfig(speed, PerfectHalfWidth(grindQuality), ZeroAccuracyDistance);
	}

	/// <summary>Mean of the step accuracies; an empty list (pastry) is a perfect 1.0.</summary>
	public static double Combine(IReadOnlyList<double> stepAccuracies)
	{
		if (stepAccuracies == null || stepAccuracies.Count == 0)
			return 1.0;
		double sum = 0;
		foreach (double a in stepAccuracies)
			sum += a;
		return sum / stepAccuracies.Count;
	}
}
