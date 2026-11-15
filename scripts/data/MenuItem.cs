/// <summary>
/// Static definition of one menu item. Prices and unlock days are the starting values from CLAUDE.md.
/// <paramref name="UnlockDay"/> is the first day the item is available; <paramref name="UnlockCost"/> is reserved
/// for a future "buy recipes" shop (0 = free, unlocked automatically on that day).
/// </summary>
public sealed record MenuItem(
	ItemId Id,
	string Name,
	int Price,
	ItemKind Kind,
	string Recipe,
	string ArtKey,
	int UnlockDay = 1,
	int UnlockCost = 0);
