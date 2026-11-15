using Godot;

/// <summary>
/// The game's single UI Theme. A Theme is a Resource that restyles every Control (Button, Label, Panel...)
/// below whichever node it's assigned to, like a global stylesheet.
///
/// It is built in code from ui_panel.png / ui_button.png (9-slice, 4 px margins) and Pixelify Sans, then
/// saved as a tiny resource (theme/cafe_theme.tres) that points at this script, so you can assign it in
/// Project Settings. If art is missing it falls back to flat Palette-coloured boxes.
///
/// Type variations are named styles you pick per node: Inspector > Theme Overrides > Theme Type Variation
/// (or <c>label.ThemeTypeVariation = CafeTheme.HeadingLabel</c>).
/// </summary>
[GlobalClass]
public partial class CafeTheme : Theme
{
	// Type variation names.
	public const string HeadingLabel = "HeadingLabel";     // Cherry, bold: customer names, section headings
	public const string TitleLabel = "TitleLabel";         // Espresso, bold, large
	public const string SecondaryLabel = "SecondaryLabel"; // Walnut: captions, descriptions
	public const string PatienceBar = "PatienceBar";       // ProgressBar with a Fern fill (default fill is Apricot)

	// Font sizes in game pixels (viewport is 320x180). Check these in-game: Pixelify Sans may not read below ~8.
	public const int BodySize = 8;
	public const int TitleSize = 16;

	private const string FontDir = "res://art/fonts/Pixelify_Sans/static/";
	private const int SliceMargin = 4;

	public CafeTheme()
	{
		var regular = LoadFont("PixelifySans-Regular.ttf");
		var bold = LoadFont("PixelifySans-Bold.ttf") ?? regular;

		if (regular != null)
			DefaultFont = regular;
		DefaultFontSize = BodySize;

		BuildLabels(regular, bold);
		BuildPanel();
		BuildButton(bold ?? regular);
		BuildProgressBars();
	}

	private static Font LoadFont(string file)
	{
		string path = FontDir + file;
		if (ResourceLoader.Exists(path))
			return ResourceLoader.Load<Font>(path);
		GD.PushWarning($"CafeTheme: font '{path}' not found, using the engine default.");
		return null;
	}

	private void BuildLabels(Font regular, Font bold)
	{
		SetColor("font_color", "Label", Palette.Espresso);
		if (regular != null)
			SetFont("font", "Label", regular);
		SetFontSize("font_size", "Label", BodySize);

		AddLabelVariation(HeadingLabel, Palette.Cherry, bold, BodySize);
		AddLabelVariation(TitleLabel, Palette.Espresso, bold, TitleSize);
		AddLabelVariation(SecondaryLabel, Palette.Walnut, regular, BodySize);
	}

	private void AddLabelVariation(string name, Color color, Font font, int size)
	{
		SetTypeVariation(name, "Label");
		SetColor("font_color", name, color);
		if (font != null)
			SetFont("font", name, font);
		SetFontSize("font_size", name, size);
	}

	private void BuildPanel()
	{
		var panel = SliceBox(ArtKeys.UiPanel, Palette.Cream);
		panel.SetContentMargin(Side.Left, 6);
		panel.SetContentMargin(Side.Right, 6);
		panel.SetContentMargin(Side.Top, 5);
		panel.SetContentMargin(Side.Bottom, 5);

		SetStylebox("panel", "Panel", panel);
		SetStylebox("panel", "PanelContainer", panel);
	}

	private void BuildButton(Font font)
	{
		var normal = ButtonBox(ArtKeys.UiButton, Palette.Latte);

		var hover = ButtonBox(ArtKeys.UiButton, Palette.Latte);
		BrightenOrFade(hover, new Color(1.12f, 1.12f, 1.12f));

		// The pressed sprite draws its face 1 px lower, so push the label down 1 px to match.
		var pressed = ButtonBox(ArtKeys.UiButtonPressed, Palette.Honey);
		pressed.SetContentMargin(Side.Top, 3);
		pressed.SetContentMargin(Side.Bottom, 1);

		var disabled = ButtonBox(ArtKeys.UiButton, Palette.Latte);
		BrightenOrFade(disabled, new Color(1, 1, 1, 0.5f)); // UI kit: locked buttons are 50% opacity

		// Focus ring for keyboard/arrow-key navigation: a 1 px outline only, no fill.
		var focus = new StyleBoxFlat
		{
			DrawCenter = false,
			BorderColor = Palette.Apricot,
		};
		focus.SetBorderWidthAll(1);

		SetStylebox("normal", "Button", normal);
		SetStylebox("hover", "Button", hover);
		SetStylebox("pressed", "Button", pressed);
		SetStylebox("hover_pressed", "Button", pressed);
		SetStylebox("disabled", "Button", disabled);
		SetStylebox("focus", "Button", focus);

		if (font != null)
			SetFont("font", "Button", font);
		SetFontSize("font_size", "Button", BodySize);
		SetColor("font_color", "Button", Palette.Espresso);
		SetColor("font_hover_color", "Button", Palette.Espresso);
		SetColor("font_pressed_color", "Button", Palette.Espresso);
		SetColor("font_hover_pressed_color", "Button", Palette.Espresso);
		SetColor("font_focus_color", "Button", Palette.Espresso);
		SetColor("font_disabled_color", "Button", new Color(Palette.Espresso, 0.5f));
	}

	private void BuildProgressBars()
	{
		// Flat boxes with a 1 px Espresso border (the kit's 3 px border at 3x). The fill gets the same border
		// so the two line up exactly. Hide the % text on each bar node: ProgressBar.ShowPercentage = false.
		SetStylebox("background", "ProgressBar", BarBox(Palette.Latte));
		SetStylebox("fill", "ProgressBar", BarBox(Palette.Apricot)); // progress fill

		SetTypeVariation(PatienceBar, "ProgressBar");
		SetStylebox("background", PatienceBar, BarBox(Palette.Latte));
		SetStylebox("fill", PatienceBar, BarBox(Palette.Fern)); // patience fill
	}

	// ----- helpers -----

	/// <summary>9-slice box from an art key with 4 px margins, or a flat fallback if the art is missing.</summary>
	private static StyleBox SliceBox(string artKey, Color fallbackFill)
	{
		if (ArtCatalog.HasArt(artKey))
		{
			var box = new StyleBoxTexture { Texture = ArtCatalog.Get(artKey) };
			box.SetTextureMarginAll(SliceMargin);
			return box;
		}

		var flat = new StyleBoxFlat { BgColor = fallbackFill, BorderColor = Palette.Espresso };
		flat.SetBorderWidthAll(1);
		flat.SetContentMarginAll(SliceMargin);
		return flat;
	}

	private static StyleBox ButtonBox(string artKey, Color fallbackFill)
	{
		var box = SliceBox(artKey, fallbackFill);
		box.SetContentMargin(Side.Left, 4);
		box.SetContentMargin(Side.Right, 4);
		box.SetContentMargin(Side.Top, 2);
		box.SetContentMargin(Side.Bottom, 2);
		return box;
	}

	/// <summary>Tints a button box (works for both the texture and flat fallback variants).</summary>
	private static void BrightenOrFade(StyleBox box, Color modulate)
	{
		switch (box)
		{
			case StyleBoxTexture texture:
				texture.ModulateColor = modulate;
				break;
			case StyleBoxFlat flat:
				flat.BgColor *= modulate;
				flat.BorderColor *= modulate;
				break;
		}
	}

	private static StyleBoxFlat BarBox(Color fill)
	{
		var box = new StyleBoxFlat { BgColor = fill, BorderColor = Palette.Espresso };
		box.SetBorderWidthAll(1);
		return box;
	}
}
