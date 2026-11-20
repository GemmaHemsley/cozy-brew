/// <summary>
/// One group of headless checks. Any class implementing this (public, parameterless constructor) is discovered
/// and run automatically by SmokeTest, so each work package adds its OWN file (e.g. scripts/dev/BrewSmoke.cs)
/// and never edits a shared one.
/// </summary>
public interface ISmokeSuite
{
	string Name { get; }
	void Run(SmokeCheck check);
}
