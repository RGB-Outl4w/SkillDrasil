using System;
using System.Collections.Generic;
using System.Linq;
using SkillDrasil;

static class Program
{
    static void Check(bool ok, string what)
    {
        if (!ok) throw new Exception("FAILED: " + what);
    }

    static int Main()
    {
        // XP -> points with growing cost: 10 + 15 = 25 XP for two points at base 10, growth 0.5.
        var s = new TreeState();
        Check(s.AddXp(Branch.Warrior, 26f, 10f, 0.5f) == 2, "two points from 26 xp");
        Check(Math.Abs(s.Xp[0] - 1f) < 1e-4, "leftover xp");

        // Gating: mastery rank 2 needs 2 points spent in the branch.
        Check(s.Spend("swords"), "swords rank 1");
        Check(s.CanSpend("swords") != null, "swords rank 2 gated");
        Check(s.Spend("axes"), "axes rank 1");
        Check(s.CanSpend("swords") == "No points available.", "gate passed, out of points");

        // Free (milestone) points cover spending beyond branch points.
        Check(s.Claim("defeated_eikthyr") && !s.Claim("defeated_eikthyr"), "milestone claimed once");
        Check(s.Spend("swords") && s.FreeAvailable == 0, "free point spent");
        Check(s.CanSpend("shieldwall") == "No points available.", "perk tier 1 unlocked at 3 spent");

        // Round trip.
        var back = TreeState.Parse(s.Serialize());
        Check(back != null && back.Serialize() == s.Serialize(), "serialize round trip");

        // Respec refunds everything to where it came from.
        s.Respec();
        Check(s.BranchAvailable(Branch.Warrior) == 2 && s.FreeAvailable == 1, "respec refund");

        // Death: lose a share of in-progress XP only.
        s.LoseXp(0.25f);
        Check(Math.Abs(s.Xp[0] - 0.75f) < 1e-4 && s.Earned[0] == 2, "death xp loss");
        s.Spend("swords");
        s.Wipe();
        Check(s.Earned.Sum() == 0 && s.Ranks.Count == 0 && s.FreeEarned == 1, "wipe keeps milestones");

        // Bad or foreign data never parses.
        Check(TreeState.Parse(null) == null && TreeState.Parse("garbage") == null, "garbage");
        Check(TreeState.Parse("2|0,0,0,0,0|0,0,0,0,0|0||") == null, "unknown format version");
        Check(TreeState.Parse("1|x,0,0,0,0|0,0,0,0,0|0||") == null, "bad number");
        var dropped = TreeState.Parse("1|0,0,0,0,0|1,0,0,0,0|0|swords:9,removednode:1|");
        Check(dropped != null && dropped.Rank("swords") == 5 && dropped.Ranks.Count == 1, "clamp + drop unknown node");

        // Migration: every full 20 vanilla levels = 1 point in that skill's branch.
        var m = TreeState.Migrate(new Dictionary<int, float> { [1] = 45f, [7] = 19.9f, [9] = 100f, [999] = 50f });
        Check(m.Earned[(int)Branch.Warrior] == 2 && m.Earned[(int)Branch.Seidr] == 5 && m.Ranks.Count == 0, "migration");

        // Node ids are save keys: renaming a display name must not change them.
        Check(new[] { "swords", "elementalmagic", "bloodmagic", "woodcutting", "shieldwall", "hearthkeeper" }.All(TreeDef.ById.ContainsKey), "stable ids");

        // Every node in every branch is reachable (no gating deadlock).
        var all = new TreeState { FreeEarned = 1000 };
        for (bool progress = true; progress;)
        {
            progress = false;
            foreach (var n in TreeDef.Nodes) progress |= all.Spend(n.Id);
        }
        Check(TreeDef.Nodes.All(n => all.Rank(n.Id) == n.MaxRank), "all nodes reachable");

        // Update check version compare.
        Check(VersionUtil.IsNewer("v1.2.0", "1.1.9"), "newer");
        Check(!VersionUtil.IsNewer("1.1.9", "1.1.9"), "equal");
        Check(!VersionUtil.IsNewer("v0.1.0", "0.2.0"), "older");
        Check(!VersionUtil.IsNewer("nightly", "0.1.0") && !VersionUtil.IsNewer(null, "0.1.0"), "malformed tag");

        Console.WriteLine("SelfCheck OK");
        return 0;
    }
}
