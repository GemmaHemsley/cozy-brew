/// <summary>Checks the WP-A additions to the GameState autoload (not testable in xUnit because it is a Godot Node).</summary>
public class CafeLogicSmoke : ISmokeSuite
{
	public string Name => "CafeLogic";

	public void Run(SmokeCheck t)
	{
		var gs = GameState.Instance;
		gs.NewGame();

		t.That(GameState.ChatPointsPerHeart == 3, "ChatPointsPerHeart is 3");
		t.That(gs.Plan != null && gs.Plan.GetStock(ItemId.Croissant) == 4 && gs.Plan.TotalStock == 12, "day 1 plan splits 12 pastries over 3");
		t.That(gs.LastSummary != null && gs.LastSummary.Served == 0, "LastSummary starts empty");

		// Chat points -> hearts
		var r = RegularId.Juniper;
		gs.AddChatPoint(r);
		gs.AddChatPoint(r);
		t.That(gs.GetChatPoints(r) == 2 && gs.GetHearts(r) == 0, "2 chat points: no heart yet");
		gs.AddChatPoint(r);
		t.That(gs.GetChatPoints(r) == 0 && gs.GetHearts(r) == 1, "3rd chat point becomes a heart");
		for (int i = 0; i < 3 * 10; i++) gs.AddChatPoint(r);
		t.That(gs.GetHearts(r) == GameState.MaxHearts, "hearts cap at 5");
		t.That(gs.GetHearts(RegularId.Sage) == 0, "other regulars unaffected");

		// Plan and summary reset
		gs.Plan.Special = ItemId.Cocoa;
		gs.Plan.TrySetStock(ItemId.Croissant, 0);
		gs.LastSummary.RecordServed(new TipResult(4, 2));
		gs.AdvanceDay();
		t.That(gs.Plan.Special == ItemId.Latte && gs.Plan.TotalStock == 12, "AdvanceDay recreates the plan");
		t.That(gs.LastSummary.Served == 0 && gs.LastSummary.TotalCoins == 0, "AdvanceDay resets LastSummary");
		gs.AdvanceDay(); gs.AdvanceDay(); gs.AdvanceDay(); // day 5
		t.That(gs.Plan.GetStock(ItemId.Cake) == 3, "day 5 plan includes Cake (3 each)");

		gs.Plan.Special = ItemId.Cocoa;
		gs.LastSummary.RecordWalkout();
		gs.NewGame();
		t.That(gs.Plan.Special == ItemId.Latte && gs.Plan.GetStock(ItemId.Croissant) == 4 && gs.LastSummary.WalkedOut == 0, "NewGame resets Plan and LastSummary");
		t.That(gs.GetChatPoints(r) == 0 && gs.GetHearts(r) == 0, "NewGame resets chat points and hearts");
	}
}
