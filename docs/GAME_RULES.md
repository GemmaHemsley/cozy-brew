# Game rules and tuning numbers (first pass)

These are **starting values** so work packages aren't blocked. They are tuning knobs: change a number here first, then tell the agent (or edit the constant). Each number should live in exactly one named constant in code. CLAUDE.md is the source of truth for prices, unlock days and the day length.

## Day
- Day length: 360 real seconds (`GameState.DayLengthSeconds`). HUD clock maps it to 8:00 AM–6:00 PM.
- No customers arrive after 90% of the day. When the clock hits 100% (`DayTimeUp`), the café stays open until everyone present has been served or has left.

## Moods
| Mood | Weight | Patience × | Tip × |
|---|---|---|---|
| Cheerful | 30 | 1.2 | 1.25 |
| Calm | 35 | 1.0 | 1.00 |
| Hurried | 20 | 0.7 | 0.90 |
| Sleepy | 15 | 1.1 | 1.00 |

## Customers (the five regulars; all customers are regulars in the MVP)
| Regular | Art key | Patience × | Notes |
|---|---|---|---|
| Juniper | `cust_fox` | 1.0 | Regular |
| Sage | `cust_sage` | 1.2 | Studies here |
| Milo | `cust_beanie` | 0.7 | Commuter, in a hurry |
| Rosie | `cust_rose` | 1.0 | Sweet tooth: pastry orders ×2 weight |
| Theo | `cust_dusk` | 1.2 | Night owl |
(Name-to-sprite mapping is a guess; change `Roster` if wrong.)

## Arrivals (`CustomerSpawner`, seeded `System.Random`, fully deterministic for a seed)
- Customers per day: `min(8 + 2 × (day − 1), 20)`.
- Three waves; share of customers 40% / 35% / 25%; arrival windows as a fraction of day length: 0.04–0.24, 0.40–0.60, 0.68–0.88.
- Arrivals sorted by time with a minimum gap of 8 s. The same regular never arrives twice in a row.
- Mood drawn by the weights above.

## Orders
- 70% drink, 30% pastry (pick the kind first, then an item uniformly among available items of that kind).
- The special item gets ×2 weight; Rosie ×2 weight on pastries.
- A pastry is only ordered while stock remains: the schedule never contains more orders of a pastry than `DayPlan` stock.
- Customers order exactly one item.

## Counter and patience
- 3 counter slots (`CounterSlots`). A customer who arrives with no free slot waits at the door until one frees; patience starts when they reach a slot.
- Patience seconds = `max(20, 60 × moodMultiplier × customerMultiplier)`. It drains in real time while waiting, ordering and brewing; it stops when served.
- Patience reaching 0 → the customer leaves (`WalkedOut`): no sale, no tip, nothing else lost.

## Money (`TipCalculator`)
- `Sale` = item price (always paid on serving).
- `quality` = `0.5 × patienceRemaining + 0.5 × accuracy`, both 0..1. Pastries have accuracy 1.0 (no brewing).
- `Tip` = `round(price × quality × moodTipMultiplier × 0.75)` (round half away from zero), plus **+1** if the item is today's special.
- Worked examples: Latte (4), perfect, Cheerful → 4×1×1.25×0.75 = 3.75 → **4**; Calm → **3**; quality 0 → **0**; Cookie (2), quality 0.5, Calm → 0.75 → **1**.
- Starting coins 0. No spending in the MVP (shop is later).

## Prep
- **Grind beans:** one `TimingBar`; `GrindQuality = 0.5 + 0.5 × accuracy` (0.5–1.0). Skipping uses 0.75. It widens the perfect zone of every brew step that day: `perfectHalfWidth = 0.04 + 0.06 × GrindQuality`.
- **Pastry case:** capacity 12 pastries total. Default: split evenly among unlocked pastries (day 1: croissant 4, muffin 4, cookie 4). Player adjusts with +/− buttons; cannot exceed capacity.
- **Special:** any unlocked item; default Latte. Effects: ×2 order weight and +1 tip.

## Brewing (`TimingBar`)
- A marker moves back and forth across 0..1 as a triangle wave at `cyclesPerSecond`. The player clicks (or presses `confirm`) to stop it. A target position is chosen randomly in 0.30..0.70.
- `distance = |marker − target|`. `accuracy = clamp(1 − (distance − perfectHalfWidth) / (0.35 − perfectHalfWidth), 0, 1)`; accuracy is 1.0 inside the perfect zone and 0 at distance ≥ 0.35.
- Step speeds (cycles/s): Shot 0.7, Milk 0.9, Matcha 0.8, Cocoa 0.8, Ice 1.0.
- Recipes: Espresso [Shot]; Latte [Shot, Milk]; Matcha [Matcha, Milk]; Cocoa [Cocoa, Milk]; Iced coffee [Shot, Ice, Milk]; pastries [] (no minigame).
- Final accuracy = mean of step accuracies. There is no failing a brew; a bad one just tips less.

## Chat and friendship
- After a customer is served they may be chatted with **once per visit**: shows a line from `ChatLines`, adds 1 chat point for that regular.
- 3 chat points = 1 heart; hearts cap at 5 (`GameState.MaxHearts`). Nothing unlocks yet; the HUD does not show hearts in the MVP.

## Unlocks
- Matcha on day 2, Cake on day 5, automatically (see `Menu`). The Close screen shows "New tomorrow: …" for anything that unlocks next day.

## Win/lose
There is none. Days continue forever.
