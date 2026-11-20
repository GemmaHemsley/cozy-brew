using System.Linq;
using Godot;

/// <summary>Checks for the foundation layer: autoloads, input map, theme, art catalogue, GameState.</summary>
public class FoundationSmoke : ISmokeSuite
{
	public string Name => "Foundation";

	public void Run(SmokeCheck t)
	{
		// Autoloads and project settings
		t.That(t.Root.GetNodeOrNull("/root/GameState") is GameState && GameState.Instance != null, "GameState autoload at /root/GameState");
		t.That(t.Root.GetNodeOrNull("/root/AudioHooks") is AudioHooks && AudioHooks.Instance != null, "AudioHooks autoload at /root/AudioHooks");
		foreach (var action in new[] { "confirm", "cancel" })
			t.That(InputMap.HasAction(action) && InputMap.ActionGetEvents(action).Count >= 1, $"input action '{action}' defined");
		var button = new Button { Text = "x" };
		t.Root.AddChild(button);
		t.That(button.GetThemeStylebox("normal") is StyleBoxTexture, "project theme applies (Button normal is a 9-slice)");
		button.QueueFree();

		// Art catalogue: every key loads real art at the manifest size
		var keys = typeof(ArtKeys).GetFields().Select(f => (string)f.GetRawConstantValue()).ToList();
		int bad = keys.Count(k => !ArtCatalog.HasArt(k) || ArtCatalog.Get(k).GetSize() != (Vector2)ArtCatalog.SizeOf(k));
		t.That(keys.Count == 41 && bad == 0, $"{keys.Count} art keys load real art at manifest size ({bad} bad)");
		t.That(ArtCatalog.Get(ArtKeys.IcedCoffee).GetSize() == new Vector2(16, 16), "iced_coffee is 16x16");

		// Fallbacks never crash
		ArtCatalog.Register("smoke_missing", new ArtEntry("does_not_exist.png", new Vector2I(16, 24), Palette.Fern));
		var placeholder = ArtCatalog.Get("smoke_missing");
		t.That(placeholder.GetSize() == new Vector2(16, 24), "missing art falls back to a placeholder of the right size");
		t.That(ArtCatalog.Get("smoke_unknown_key").GetSize() == new Vector2(16, 16), "unknown key falls back to a 16x16 placeholder");

		// GameState basics (uses the real autoload, so restore state afterwards)
		var gs = GameState.Instance;
		gs.NewGame();
		t.That(gs.Day == 1 && gs.UnlockedItems.Count() == 7, "day 1 starts with 7 recipes");
		gs.AdvanceDay();
		t.That(gs.IsUnlocked(ItemId.Matcha), "Matcha unlocks on day 2");
		gs.NewGame();
	}
}
