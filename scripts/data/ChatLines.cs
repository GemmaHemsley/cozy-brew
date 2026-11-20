using System;
using System.Collections.Generic;

/// <summary>Dialogue text. Templates use {item} (lower-case item name). Pass a seeded Random for repeatable picks.</summary>
public static class ChatLines
{
	private static readonly Dictionary<Mood, string[]> Orders = new()
	{
		[Mood.Cheerful] = new[] { "Hi hi! Could I get a {item}, please?", "What a lovely day! One {item} for me!", "Hello! I'd love a {item}." },
		[Mood.Calm] = new[] { "Hello. A {item}, please.", "Could I have a {item}?", "I'll take a {item} today." },
		[Mood.Hurried] = new[] { "Quick one: {item}, please!", "Make it a {item}, and fast if you can!", "Hi! A {item}, I'm running late." },
		[Mood.Sleepy] = new[] { "*yawn* A {item}, please...", "Mm... a {item}, I think.", "Could I get a {item}? I need it." },
	};

	private static readonly Dictionary<Mood, string[]> Thanks = new()
	{
		[Mood.Cheerful] = new[] { "Thank you, it looks wonderful!", "Yay! Thanks so much!" },
		[Mood.Calm] = new[] { "Thank you, that's perfect.", "Thanks. See you soon." },
		[Mood.Hurried] = new[] { "Thanks! Gotta run!", "Perfect, thank you!" },
		[Mood.Sleepy] = new[] { "Mmm... thanks.", "Thank you... that smells so good." },
	};

	private static readonly Dictionary<RegularId, string[]> Talk = new()
	{
		[RegularId.Juniper] = new[]
		{
			"I come here every week. It feels like home.",
			"Biscuit says hello. Well, he blinked at me.",
			"The best part of my day is this little corner.",
		},
		[RegularId.Sage] = new[]
		{
			"I'm studying for exams. This is the only quiet place.",
			"Three chapters to go. Coffee helps.",
			"Do you mind if I stay a while? I have a lot of notes.",
		},
		[RegularId.Milo] = new[]
		{
			"My train leaves in ten minutes, so I can't stay long.",
			"Same commute every morning. This stop is the highlight.",
			"Don't tell my boss I'm late again.",
		},
		[RegularId.Rosie] = new[]
		{
			"Do you have anything sweeter? Always anything sweeter.",
			"I could eat a whole tray of cookies. Don't judge!",
			"Dessert first is my life motto.",
		},
		[RegularId.Theo] = new[]
		{
			"I was up all night again. Time is a strange thing.",
			"The café is nicest when the light is low and golden.",
			"I write best around midnight. Sorry, I mean morning.",
		},
	};

	public static string OrderLine(CustomerDef c, Mood m, MenuItem item, Random rng) =>
		Pick(Orders[m], rng).Replace("{item}", item.Name.ToLowerInvariant());

	public static string ThanksLine(CustomerDef c, Mood m, Random rng) => Pick(Thanks[m], rng);

	public static string SmallTalk(CustomerDef c, Mood m, Random rng) => Pick(Talk[c.Regular], rng);

	private static string Pick(string[] lines, Random rng) => lines[(rng ?? Random.Shared).Next(lines.Length)];
}
