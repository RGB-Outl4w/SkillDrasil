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
        // Cost curve: first * n^exp.
        Check(TreeState.PointCost(0, 80f, 1.5f) == 80f && Math.Abs(TreeState.PointCost(3, 80f, 1.5f) - 640f) < 1e-2, "power curve");

        // XP -> points: with exp = log2(1.5) costs are 10 then 15, so 26 XP buys two points.
        var s = new TreeState();
        Check(s.AddXp(Branch.Warrior, 26f, 10f, (float)Math.Log(1.5, 2)) == 2, "two points from 26 xp");
        Check(Math.Abs(s.Xp[0] - 1f) < 1e-3, "leftover xp");

        // Guard: bad config or bad XP never hangs the game.
        var g = new TreeState();
        Check(g.AddXp(Branch.Artisan, 50f, 0f, 1.5f) == 0, "zero first-point cost awards nothing, no hang");
        Check(g.AddXp(Branch.Artisan, float.PositiveInfinity, 80f, 1.5f) == 0 && !float.IsInfinity(g.Xp[4]), "infinite xp ignored");
        Check(g.AddXp(Branch.Artisan, float.NaN, 80f, 1.5f) == 0 && !float.IsNaN(g.Xp[4]), "NaN xp ignored");

        // Gating: mastery rank 2 needs 2 points spent in the branch.
        Check(s.Spend("swords"), "swords rank 1");
        Check(s.CanSpend("swords", out int need) == Block.NeedSpent && need == 2, "swords rank 2 gated");
        Check(s.Spend("axes"), "axes rank 1");
        Check(s.CanSpend("swords") == Block.NoPoints, "gate passed, out of points");

        // Free (milestone) points cover spending beyond branch points.
        Check(s.Claim("defeated_eikthyr") && !s.Claim("defeated_eikthyr"), "milestone claimed once");
        Check(s.Spend("swords") && s.FreeAvailable == 0, "free point spent");
        Check(s.CanSpend("shieldwall") == Block.NoPoints, "perk tier 1 unlocked at 3 spent");

        // Round trip.
        var back = TreeState.Parse(s.Serialize());
        Check(back != null && back.Serialize() == s.Serialize(), "serialize round trip");

        // Respec refunds everything to where it came from.
        s.Respec();
        Check(s.BranchAvailable(Branch.Warrior) == 2 && s.FreeAvailable == 1, "respec refund");

        // Death: lose a share of in-progress XP only.
        s.LoseXp(0.25f);
        Check(Math.Abs(s.Xp[0] - 0.75f) < 1e-3 && s.Earned[0] == 2, "death xp loss");
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
        Check(TreeDef.Nodes.Where(n => !n.IsKeystone).All(n => all.Rank(n.Id) == n.MaxRank), "all non-keystone nodes reachable");

        // Keystones: exactly one in the whole tree, each reachable on its own, freed by respec.
        Check(TreeDef.Nodes.Count(n => n.IsKeystone && all.Rank(n.Id) > 0) == 1, "only one keystone");
        Check(all.CanSpend("lonewolf") == Block.KeystoneTaken || all.Keystone == "lonewolf", "second keystone blocked");
        foreach (var k in TreeDef.Nodes.Where(n => n.IsKeystone))
        {
            var solo = new TreeState { FreeEarned = 1000 };
            for (bool progress = true; progress;)
            {
                progress = false;
                foreach (var n in TreeDef.Nodes.Where(n => n.Branch == k.Branch && !n.IsKeystone)) progress |= solo.Spend(n.Id);
            }
            Check(solo.Spend(k.Id) && solo.Keystone == k.Id, "keystone reachable: " + k.Id);
        }
        all.Respec();
        Check(all.Keystone == null, "respec frees keystone");

        // Keystone gate: 15 points in its branch.
        var gate = new TreeState { FreeEarned = 100 };
        for (int i = 0; i < 5; i++) gate.Spend("swords");
        for (int i = 0; i < 5; i++) gate.Spend("axes");
        Check(gate.CanSpend("bearshirt", out int kneed) == Block.NeedSpent && kneed == 15, "keystone needs 15 spent");

        // Multi-rank perks keep ranks through a save round trip.
        Check(gate.Spend("ironhide") && gate.Spend("ironhide") && TreeState.Parse(gate.Serialize()).Rank("ironhide") == 2, "multi-rank perk");
        Check(TreeDef.Nodes.Length == 59 && TreeDef.Nodes.Count(n => n.IsKeystone) == 5, "node count");


        // Update check version compare.
        Check(VersionUtil.IsNewer("v1.2.0", "1.1.9"), "newer");
        Check(!VersionUtil.IsNewer("1.1.9", "1.1.9"), "equal");
        Check(!VersionUtil.IsNewer("v0.1.0", "0.2.0"), "older");
        Check(!VersionUtil.IsNewer("nightly", "0.1.0") && !VersionUtil.IsNewer(null, "0.1.0"), "malformed tag");

        Console.WriteLine("SelfCheck OK");
        return 0;
    }
}
