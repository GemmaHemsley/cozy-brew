/// <summary>Tuning for one timing-bar run. All values are in bar units (0..1) except the speed.</summary>
public readonly record struct TimingBarConfig(double CyclesPerSecond, double PerfectHalfWidth, double ZeroAccuracyDistance);
