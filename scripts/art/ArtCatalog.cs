using System.Collections.Generic;
using Godot;

/// <summary>
/// The single place that knows where art lives. Everything else asks for a texture by key
/// (see <see cref="ArtKeys"/>). Sizes come from art/ART_MANIFEST.md.
///
/// Missing or unknown art never crashes: you get a coloured rectangle with an Espresso outline
/// of the right size, and one warning in the Output panel.
///
/// To swap art later (e.g. move to sprite sheets), edit the entry here, or call <see cref="Register"/>;
/// callers don't change.
/// </summary>
public static class ArtCatalog
{
	public const string ArtDir = "res://art/";

	private static readonly Vector2I S16 = new(16, 16);
	private static readonly Vector2I S16x24 = new(16, 24);

	// File names / sizes measured in art/ART_MANIFEST.md. Fallback colours are only for missing art.
	private static readonly Dictionary<string, ArtEntry> Entries = new()
	{
		// Characters
		[ArtKeys.BaristaIdle] = new("barista_idle.png", S16x24, Palette.Fern),
		[ArtKeys.BaristaBob] = new("barista_bob.png", S16x24, Palette.Fern),
		[ArtKeys.BaristaBlink] = new("barista_blink.png", S16x24, Palette.Fern),
		[ArtKeys.CatIdle] = new("cat_idle.png", S16, Palette.Apricot),
		[ArtKeys.CatBlink] = new("cat_blink.png", S16, Palette.Apricot),
		[ArtKeys.CustBeanie] = new("cust_beanie.png", S16x24, Palette.Honey),
		[ArtKeys.CustDusk] = new("cust_dusk.png", S16x24, Palette.Dusk),
		[ArtKeys.CustFox] = new("cust_fox.png", S16x24, Palette.Apricot),
		[ArtKeys.CustRose] = new("cust_rose.png", S16x24, Palette.Rose),
		[ArtKeys.CustSage] = new("cust_sage.png", S16x24, Palette.Matcha),

		// Menu items
		[ArtKeys.Latte] = new("latte.png", S16, Palette.Latte),
		[ArtKeys.Espresso] = new("espresso.png", S16, Palette.Walnut),
		[ArtKeys.Matcha] = new("matcha.png", S16, Palette.Matcha),
		[ArtKeys.Cocoa] = new("cocoa.png", S16, Palette.Cinnamon),
		[ArtKeys.IcedCoffee] = new("iced_coffee.png", S16, Palette.Sky),
		[ArtKeys.Croissant] = new("croissant.png", S16, Palette.Honey),
		[ArtKeys.Muffin] = new("muffin.png", S16, Palette.Peach),
		[ArtKeys.Cookie] = new("cookie.png", S16, Palette.Oak),
		[ArtKeys.Cake] = new("cake.png", S16, Palette.Rose),

		// Tiles
		[ArtKeys.FloorWood] = new("floor_wood.png", S16, Palette.Oak),
		[ArtKeys.FloorChecker] = new("floor_checker.png", S16, Palette.Cream),
		[ArtKeys.Wallpaper] = new("wallpaper.png", S16, Palette.Cream),
		[ArtKeys.Wainscot] = new("wainscot.png", S16, Palette.Latte),
		[ArtKeys.Brick] = new("brick.png", S16, Palette.Cherry),
		[ArtKeys.CounterTop] = new("counter_top.png", S16, Palette.Honey),
		[ArtKeys.CounterFront] = new("counter_front.png", S16, Palette.Walnut),

		// Props
		[ArtKeys.EspressoMachine] = new("espresso_machine.png", new Vector2I(24, 24), Palette.Walnut),
		[ArtKeys.Window] = new("window.png", new Vector2I(32, 32), Palette.Sky),
		[ArtKeys.Chalkboard] = new("chalkboard.png", new Vector2I(32, 24), Palette.DarkRoast),
		[ArtKeys.Shelf] = new("shelf.png", new Vector2I(32, 16), Palette.Oak),
		[ArtKeys.Plant] = new("plant.png", S16, Palette.Fern),
		[ArtKeys.Table] = new("table.png", S16, Palette.Honey),
		[ArtKeys.Chair] = new("chair.png", S16, Palette.Oak),
		[ArtKeys.Lamp] = new("lamp.png", S16, Palette.Lamplight),

		// Icons (10x10, standalone)
		[ArtKeys.IconCoin] = new("icon_coin.png", new Vector2I(10, 10), Palette.Lamplight),
		[ArtKeys.IconHeart] = new("icon_heart.png", new Vector2I(10, 10), Palette.Cherry),
		[ArtKeys.IconStar] = new("icon_star.png", new Vector2I(10, 10), Palette.Honey),
		[ArtKeys.IconClock] = new("icon_clock.png", new Vector2I(10, 10), Palette.Walnut),

		// UI 9-slice sources
		[ArtKeys.UiPanel] = new("ui_panel.png", new Vector2I(24, 24), Palette.Cream),
		[ArtKeys.UiButton] = new("ui_button.png", new Vector2I(32, 14), Palette.Latte),
		[ArtKeys.UiButtonPressed] = new("ui_button_pressed.png", new Vector2I(32, 14), Palette.Honey),
	};

	private static readonly Dictionary<string, Texture2D> Cache = new();
	private static readonly HashSet<string> Warned = new();

	/// <summary>Adds or replaces an entry (e.g. to point a key at a sheet region). Clears that key's cache.</summary>
	public static void Register(string key, ArtEntry entry)
	{
		Entries[key] = entry;
		Cache.Remove(key);
		Warned.Remove(key);
	}

	/// <summary>Texture for a key. Always returns something drawable; never null, never throws.</summary>
	public static Texture2D Get(string key)
	{
		if (Cache.TryGetValue(key, out var cached) && GodotObject.IsInstanceValid(cached))
			return cached;

		var texture = Build(key);
		Cache[key] = texture;
		return texture;
	}

	/// <summary>True if the real PNG for this key exists (i.e. <see cref="Get"/> will not return a placeholder).</summary>
	public static bool HasArt(string key) =>
		Entries.TryGetValue(key, out var entry) && ResourceLoader.Exists(ArtDir + entry.File);

	/// <summary>Size in pixels of the frame for this key (from the manifest), or 16x16 for an unknown key.</summary>
	public static Vector2I SizeOf(string key) =>
		Entries.TryGetValue(key, out var entry) ? (entry.Region?.Size ?? entry.Size) : S16;

	private static Texture2D Build(string key)
	{
		if (!Entries.TryGetValue(key, out var entry))
		{
			WarnOnce(key, $"ArtCatalog: unknown art key '{key}', using a placeholder.");
			return MakePlaceholder(S16, Palette.Cherry);
		}

		string path = ArtDir + entry.File;
		Texture2D loaded = ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture2D>(path) : null;
		if (loaded == null)
		{
			WarnOnce(key, $"ArtCatalog: '{path}' is missing or failed to load, using a placeholder.");
			return MakePlaceholder(entry.Region?.Size ?? entry.Size, entry.Fallback);
		}

		if (entry.Region is Rect2I region)
			return new AtlasTexture { Atlas = loaded, Region = new Rect2(region.Position, region.Size) };

		return loaded;
	}

	private static void WarnOnce(string key, string message)
	{
		if (Warned.Add(key))
			GD.PushWarning(message);
	}

	/// <summary>A filled rectangle with a 1 px Espresso outline, so placeholders are visible against any background.</summary>
	private static ImageTexture MakePlaceholder(Vector2I size, Color fill)
	{
		var image = Image.CreateEmpty(size.X, size.Y, false, Image.Format.Rgba8);
		image.Fill(fill);
		for (int x = 0; x < size.X; x++)
		{
			image.SetPixel(x, 0, Palette.Espresso);
			image.SetPixel(x, size.Y - 1, Palette.Espresso);
		}
		for (int y = 0; y < size.Y; y++)
		{
			image.SetPixel(0, y, Palette.Espresso);
			image.SetPixel(size.X - 1, y, Palette.Espresso);
		}
		return ImageTexture.CreateFromImage(image);
	}
}
