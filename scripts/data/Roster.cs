using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>The five regulars (docs/GAME_RULES.md "Customers"). The art-key mapping is a guess; fix it here if wrong.</summary>
public static class Roster
{
	public static readonly IReadOnlyList<CustomerDef> All = new[]
	{
		new CustomerDef(RegularId.Juniper, "Juniper", ArtKeys.CustFox, 1.0f, 1f),
		new CustomerDef(RegularId.Sage, "Sage", ArtKeys.CustSage, 1.2f, 1f),
		new CustomerDef(RegularId.Milo, "Milo", ArtKeys.CustBeanie, 0.7f, 1f),
		new CustomerDef(RegularId.Rosie, "Rosie", ArtKeys.CustRose, 1.0f, 2f),
		new CustomerDef(RegularId.Theo, "Theo", ArtKeys.CustDusk, 1.2f, 1f),
	};

	public static CustomerDef Get(RegularId id) =>
		All.FirstOrDefault(c => c.Regular == id) ?? throw new ArgumentOutOfRangeException(nameof(id), id, "Not in the roster.");
}
