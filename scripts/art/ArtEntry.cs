using Godot;

/// <summary>
/// One catalogue entry. <paramref name="File"/> is relative to <see cref="ArtCatalog.ArtDir"/>.
/// <paramref name="Size"/> is the whole image (or the frame, for a future sheet) and is also the size of
/// the fallback rectangle. If <paramref name="Region"/> is set, the entry is a sub-rectangle of a sheet.
/// </summary>
public readonly record struct ArtEntry(string File, Vector2I Size, Color Fallback, Rect2I? Region = null);
