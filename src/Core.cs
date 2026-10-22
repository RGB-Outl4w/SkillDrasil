// Pure tree logic: no Unity or Valheim types, so tests/SelfCheck can compile it directly.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SkillDrasil
{
    public enum Branch { Warrior, Hunter, Seidr, Wanderer, Artisan }

    public sealed class Node
    {
        public readonly string Id;
        public readonly string Name;
        public readonly Branch Branch;
        public readonly int Skill;   // Skills.SkillType value for masteries, 0 for perks
        public readonly int Tier;    // perks only: 1..3
        public readonly string Effect;

        public Node(string id, string name, Branch branch, int skill, int tier, string effect)
        {
            Id = id; Name = name; Branch = branch; Skill = skill; Tier = tier; Effect = effect;
        }

        public bool IsMastery => Skill != 0;
        public int MaxRank => IsMastery ? 5 : 1;

        // Points that must already be spent in this branch before buying rank `nextRank`.
        public int RequiredSpent(int nextRank) => IsMastery ? (nextRank - 1) * 2 : Tier * 3;
    }

    public static class TreeDef
    {
        public const int LevelsPerRank = 20;
        public static readonly string[] BranchNames = { "Warrior", "Hunter", "Seiðr", "Wanderer", "Artisan" };

        static Node M(string name, Branch b, int skill) =>
            new Node(name.ToLowerInvariant(), name, b, skill, 0, $"+{LevelsPerRank} {name} skill level per rank.");
        static Node P(string id, string name, Branch b, int tier, string effect) => new Node(id, name, b, 0, tier, effect);

        public static readonly Node[] Nodes =
        {
            M("Swords", Branch.Warrior, 1), M("Axes", Branch.Warrior, 7), M("Clubs", Branch.Warrior, 3),
            M("Polearms", Branch.Warrior, 4), M("Spears", Branch.Warrior, 5), M("Unarmed", Branch.Warrior, 11),
            M("Blocking", Branch.Warrior, 6),
            P("shieldwall", "Shield-Wall", Branch.Warrior, 1, "Blocking costs 25% less stamina."),
            P("berserkgang", "Berserkgang", Branch.Warrior, 2, "Below 30% health: +20% melee damage."),
            P("riposte", "Riposte", Branch.Warrior, 3, "Parries are 30% stronger."),

            M("Bows", Branch.Hunter, 8), M("Crossbows", Branch.Hunter, 14), M("Knives", Branch.Hunter, 2),
            M("Sneak", Branch.Hunter, 101),
            P("silentdeath", "Silent Death", Branch.Hunter, 1, "Sneak attack multiplier x1.5."),
            P("steadyaim", "Steady Aim", Branch.Hunter, 2, "+15% bow and crossbow damage while crouched."),
            P("fletcher", "Fletcher's Luck", Branch.Hunter, 3, "25% chance to not consume ammo."),

            M("ElementalMagic", Branch.Seidr, 9), M("BloodMagic", Branch.Seidr, 10),
            P("eitrwell", "Eitr Well", Branch.Seidr, 1, "+25% eitr regeneration."),
            P("focusedmind", "Focused Mind", Branch.Seidr, 2, "All eitr costs 20% less."),
            P("arcanesurge", "Arcane Surge", Branch.Seidr, 3, "+15% magic damage."),

            M("Run", Branch.Wanderer, 102), M("Jump", Branch.Wanderer, 100), M("Swim", Branch.Wanderer, 103),
            M("Dodge", Branch.Wanderer, 108), M("Ride", Branch.Wanderer, 110),
            P("fleetfoot", "Fleetfoot", Branch.Wanderer, 1, "Running costs 20% less stamina."),
            P("secondwind", "Second Wind", Branch.Wanderer, 2, "Dodging costs 25% less stamina."),
            P("featherfall", "Featherfall", Branch.Wanderer, 3, "-50% fall damage."),

            M("Pickaxes", Branch.Artisan, 12), M("WoodCutting", Branch.Artisan, 13), M("Fishing", Branch.Artisan, 104),
            M("Cooking", Branch.Artisan, 105), M("Farming", Branch.Artisan, 106), M("Crafting", Branch.Artisan, 107),
            P("deeppockets", "Deep Pockets", Branch.Artisan, 1, "+50 carry weight."),
            P("buildersease", "Builder's Ease", Branch.Artisan, 2, "Building and repairing cost 30% less stamina."),
            P("hearthkeeper", "Hearthkeeper", Branch.Artisan, 3, "Food lasts 25% longer."),
        };

        public static readonly Dictionary<string, Node> ById = Nodes.ToDictionary(n => n.Id);
        public static readonly Dictionary<int, Node> BySkill = Nodes.Where(n => n.IsMastery).ToDictionary(n => n.Skill);

        // Boss kills (global keys) and these biomes each grant one free point, once per character.
        public const string BossKeyPrefix = "defeated_";
        public static readonly HashSet<string> MilestoneBiomes = new HashSet<string>
        {
            "$biome_blackforest", "$biome_swamp", "$biome_mountain", "$biome_plains",
            "$biome_mistlands", "$biome_ashlands", "$biome_deepnorth", "$biome_ocean",
        };
    }

    public sealed class TreeState
    {
        public const string Key = "SkillDrasil";
        const string Format = "1";
        const int Branches = 5;

        public readonly float[] Xp = new float[Branches];
        public readonly int[] Earned = new int[Branches];
        public int FreeEarned;
        public readonly Dictionary<string, int> Ranks = new Dictionary<string, int>();
        public readonly HashSet<string> Claimed = new HashSet<string>();

        public int Rank(string id) => Ranks.TryGetValue(id, out var r) ? r : 0;
        public int Spent(Branch b) => Ranks.Where(kv => TreeDef.ById[kv.Key].Branch == b).Sum(kv => kv.Value);
        public int BranchAvailable(Branch b) => Math.Max(0, Earned[(int)b] - Spent(b));

        // Free points cover whatever a branch spent beyond its own earned points.
        public int FreeAvailable
        {
            get
            {
                int used = 0;
                for (int b = 0; b < Branches; b++) used += Math.Max(0, Spent((Branch)b) - Earned[b]);
                return FreeEarned - used;
            }
        }

        public static float PointCost(int earned, float baseXp, float growth) => baseXp * (1f + growth * earned);

        // Returns points gained.
        public int AddXp(Branch b, float amount, float baseXp, float growth)
        {
            int i = (int)b, gained = 0;
            Xp[i] += amount;
            for (float cost; Xp[i] >= (cost = PointCost(Earned[i], baseXp, growth)); gained++)
            {
                Xp[i] -= cost;
                Earned[i]++;
            }
            return gained;
        }

        // null when the next rank can be bought, otherwise the reason it can't.
        public string CanSpend(string id)
        {
            var n = TreeDef.ById[id];
            int rank = Rank(id);
            if (rank >= n.MaxRank) return "Maxed.";
            int need = n.RequiredSpent(rank + 1);
            if (Spent(n.Branch) < need) return $"Needs {need} points spent in {TreeDef.BranchNames[(int)n.Branch]}.";
            if (BranchAvailable(n.Branch) == 0 && FreeAvailable <= 0) return "No points available.";
            return null;
        }

        public bool Spend(string id)
        {
            if (CanSpend(id) != null) return false;
            Ranks[id] = Rank(id) + 1;
            return true;
        }

        public void Respec() => Ranks.Clear();

        public void LoseXp(float factor)
        {
            float keep = 1f - Math.Max(0f, Math.Min(1f, factor));
            for (int i = 0; i < Branches; i++) Xp[i] *= keep;
        }

        // DeathSkillsReset world modifier. Milestone points stay claimed.
        public void Wipe()
        {
            Array.Clear(Xp, 0, Branches);
            Array.Clear(Earned, 0, Branches);
            Ranks.Clear();
        }

        public bool Claim(string key)
        {
            if (!Claimed.Add(key)) return false;
            FreeEarned++;
            return true;
        }

        // One-time conversion of vanilla levels: every full 20 levels = one earned point in that skill's branch.
        public static TreeState Migrate(IEnumerable<KeyValuePair<int, float>> vanillaLevels)
        {
            var s = new TreeState();
            foreach (var kv in vanillaLevels)
                if (TreeDef.BySkill.TryGetValue(kv.Key, out var n))
                    s.Earned[(int)n.Branch] += (int)(kv.Value / TreeDef.LevelsPerRank);
            return s;
        }

        public string Serialize()
        {
            var inv = CultureInfo.InvariantCulture;
            return string.Join("|",
                Format,
                string.Join(",", Xp.Select(x => x.ToString("R", inv))),
                string.Join(",", Earned),
                FreeEarned.ToString(inv),
                string.Join(",", Ranks.Select(kv => kv.Key + ":" + kv.Value)),
                string.Join(",", Claimed));
        }

        // null when the string is missing or unreadable.
        public static TreeState Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            var p = raw.Split('|');
            if (p.Length != 6 || p[0] != Format) return null;
            var inv = CultureInfo.InvariantCulture;
            try
            {
                var s = new TreeState();
                var xp = p[1].Split(',');
                var earned = p[2].Split(',');
                if (xp.Length != Branches || earned.Length != Branches) return null;
                for (int i = 0; i < Branches; i++)
                {
                    s.Xp[i] = float.Parse(xp[i], inv);
                    s.Earned[i] = int.Parse(earned[i], inv);
                }
                s.FreeEarned = int.Parse(p[3], inv);
                foreach (var e in p[4].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = e.Split(':');
                    // Nodes removed in a later version are dropped; their points return automatically.
                    if (kv.Length == 2 && TreeDef.ById.TryGetValue(kv[0], out var n))
                        s.Ranks[kv[0]] = Math.Min(n.MaxRank, int.Parse(kv[1], inv));
                }
                foreach (var c in p[5].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) s.Claimed.Add(c);
                return s;
            }
            catch (FormatException) { return null; }
            catch (OverflowException) { return null; }
        }
    }

    public static class VersionUtil
    {
        public static bool IsNewer(string tag, string current) =>
            System.Version.TryParse(tag?.TrimStart('v', 'V'), out var latest) &&
            System.Version.TryParse(current, out var mine) &&
            latest > mine;
    }
}
