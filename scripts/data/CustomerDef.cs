/// <summary>Static definition of a regular customer. PastryWeight scales how likely they are to order a pastry.</summary>
public sealed record CustomerDef(RegularId Regular, string Name, string ArtKey, float PatienceMultiplier, float PastryWeight);
