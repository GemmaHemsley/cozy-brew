using Godot;

/// <summary>Passed to each suite: record a pass/fail and reach the scene tree.</summary>
public sealed class SmokeCheck
{
	public Node Root { get; }
	public int Failures { get; private set; }
	public int Passes { get; private set; }

	public SmokeCheck(Node root) => Root = root;

	public void That(bool condition, string description)
	{
		GD.Print((condition ? "  ok   " : "  FAIL ") + description);
		if (condition) Passes++; else Failures++;
	}
}
