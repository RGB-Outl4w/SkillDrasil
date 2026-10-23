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
        public readonly int Tier;    // perks only: 1..3, 4 = keystone
        readonly int perkRanks;
        public readonly string Effect;
        public readonly string Icon; // perks: item prefab whose icon is shown
        public readonly string Lore;

        public Node(string id, string name, Branch branch, int skill, int tier, string effect, string icon = null, string lore = null, int ranks = 1)
        {
            Id = id; Name = name; Branch = branch; Skill = skill; Tier = tier; Effect = effect; Icon = icon; Lore = lore; perkRanks = ranks;
        }

        public const int KeystoneTier = 4, KeystoneSpent = 15;

        public bool IsMastery => Skill != 0;
        public bool IsKeystone => Tier == KeystoneTier;
        public int MaxRank => IsMastery ? 5 : perkRanks;

        // Points that must already be spent in this branch before buying rank `nextRank`.
        public int RequiredSpent(int nextRank) => IsMastery ? (nextRank - 1) * 2 : IsKeystone ? KeystoneSpent : Tier * 3;
    }

    public static class TreeDef
    {
        public const int LevelsPerRank = 20;
        public static readonly string[] BranchNames = { "Warrior", "Hunter", "Seiðr", "Wanderer", "Artisan" };
        public static readonly int[] BranchIconSkill = { 1, 8, 9, 102, 12 };
        public static readonly string[] BranchLore =
        {
            "Steel, shield and the will to stand.",
            "Patience, silence and the clean kill.",
            "The eitr-craft of the völva.",
            "Wind at your heels, sky beneath your feet.",
            "Hands that shape the world.",
        };

        // Id = name without spaces, lower case (stable: it is the save key).
        static Node M(string name, Branch b, int skill) =>
            new Node(name.Replace(" ", "").ToLowerInvariant(), name, b, skill, 0, $"+{LevelsPerRank} {name} skill levels per rank.");
        static Node P(string id, string name, Branch b, int tier, string effect, string icon, string lore, int ranks = 1) =>
            new Node(id, name, b, 0, tier, effect, icon, lore, ranks);
        static Node K(string id, string name, Branch b, string effect, string lore) =>
            new Node(id, name, b, 0, Node.KeystoneTier, effect, null, lore);

        public static readonly Node[] Nodes =
        {
            M("Swords", Branch.Warrior, 1), M("Axes", Branch.Warrior, 7), M("Clubs", Branch.Warrior, 3),
            M("Polearms", Branch.Warrior, 4), M("Spears", Branch.Warrior, 5), M("Unarmed", Branch.Warrior, 11),
            M("Blocking", Branch.Warrior, 6),
            P("shieldwall", "Shield-Wall", Branch.Warrior, 1, "Blocking costs 25% less stamina.", "ShieldBanded", "Lock shields. Hold the line."),
            P("berserkgang", "Berserkgang", Branch.Warrior, 2, "Below 30% health: +20% melee damage.", "AxeBerzerkr", "The bear-shirt's fury wakes when blood runs low."),
            P("riposte", "Riposte", Branch.Warrior, 3, "Parries are 30% stronger.", "ShieldBronzeBuckler", "Turn their strength against them."),
            P("ironhide", "Iron Hide", Branch.Warrior, 1, "+3 armor per rank.", null, "Scars are a second coat of mail.", 3),
            P("battlefervor", "Battle Fervor", Branch.Warrior, 2, "Melee attacks cost 10% less stamina per rank.", null, "The fight feeds itself.", 2),
            P("unshaken", "Unshaken", Branch.Warrior, 3, "Take 40% less stagger.", null, "Roots run deep. The oak does not fall."),
            K("bearshirt", "Bear Shirt", Branch.Warrior, "+30% melee damage.\nDrawback: 40% less armor.", "Shed the mail. Become the bear."),

            M("Bows", Branch.Hunter, 8), M("Crossbows", Branch.Hunter, 14), M("Knives", Branch.Hunter, 2),
            M("Sneak", Branch.Hunter, 101),
            P("silentdeath", "Silent Death", Branch.Hunter, 1, "Sneak attacks deal 50% more bonus damage.", "KnifeFlint", "Strike before they know you are there."),
            P("steadyaim", "Steady Aim", Branch.Hunter, 2, "+15% bow and crossbow damage while crouched.", "BowHuntsman", "Breathe. Kneel. Loose."),
            P("fletcher", "Fletcher's Luck", Branch.Hunter, 3, "25% chance to not use up ammo.", "ArrowWood", "A good hunter wastes nothing."),
            P("shadowstep", "Shadowstep", Branch.Hunter, 1, "Sneaking costs 20% less stamina per rank.", null, "Move as the mist moves.", 2),
            P("keeneye", "Keen Eye", Branch.Hunter, 2, "+5% bow and crossbow damage per rank.", null, "The hawk's eye misses nothing.", 3),
            P("ghost", "Ghost", Branch.Hunter, 3, "50% harder to notice while sneaking.", null, "Here, then gone."),
            K("lonewolf", "Lone Wolf", Branch.Hunter, "+30% bow, crossbow and knife damage.\nDrawback: other melee weapons deal 30% less.", "No shield-brothers. No mercy."),

            M("Elemental Magic", Branch.Seidr, 9), M("Blood Magic", Branch.Seidr, 10),
            P("eitrwell", "Eitr Well", Branch.Seidr, 1, "+25% eitr regeneration.", "Eitr", "Drink deep from the well beneath the roots."),
            P("focusedmind", "Focused Mind", Branch.Seidr, 2, "All eitr costs 20% less.", "MeadEitrMinor", "A quiet mind spends less of itself."),
            P("arcanesurge", "Arcane Surge", Branch.Seidr, 3, "+15% elemental and blood magic damage.", "StaffFireball", "Let the eitr burn bright."),
            P("vigor", "Völva's Vigor", Branch.Seidr, 1, "+15% health regeneration per rank.", null, "The seeress outlives the warrior.", 2),
            P("galdr", "Galdr", Branch.Seidr, 2, "+5% elemental and blood magic damage per rank.", null, "Sing the runes and they answer.", 3),
            P("runeward", "Rune Ward", Branch.Seidr, 3, "Resistant to fire, frost and lightning.", null, "Carved on the skin, the runes hold back the storm."),
            K("volvasoath", "Völva's Oath", Branch.Seidr, "+30% magic damage, +25% eitr regeneration.\nDrawback: weapons deal 30% less.", "Lay down the blade. Take up the staff for good."),

            M("Run", Branch.Wanderer, 102), M("Jump", Branch.Wanderer, 100), M("Swim", Branch.Wanderer, 103),
            M("Dodge", Branch.Wanderer, 108), M("Ride", Branch.Wanderer, 110),
            P("fleetfoot", "Fleetfoot", Branch.Wanderer, 1, "Running costs 20% less stamina.", "MeadStaminaMinor", "The road is long. Your stride is longer."),
            P("secondwind", "Second Wind", Branch.Wanderer, 2, "Dodging costs 25% less stamina.", "MeadStaminaLingering", "One more breath. One more step."),
            P("featherfall", "Featherfall", Branch.Wanderer, 3, "Take 50% less fall damage.", "CapeFeather", "Drift down like a leaf from Yggdrasil's crown."),
            P("sealborn", "Seal-Born", Branch.Wanderer, 1, "Swimming costs 20% less stamina per rank.", null, "The fjord is just another road.", 2),
            P("springstep", "Spring Step", Branch.Wanderer, 2, "Jumping costs 10% less stamina per rank.", null, "Leap the stream. Leap the wall.", 3),
            P("windrunner", "Windrunner", Branch.Wanderer, 3, "+8% movement speed.", null, "Outrun the storm."),
            K("endlessroad", "Endless Road", Branch.Wanderer, "+40% stamina regeneration.\nDrawback: -100 carry weight.", "Travel light. Travel forever."),

            M("Pickaxes", Branch.Artisan, 12), M("Wood Cutting", Branch.Artisan, 13), M("Fishing", Branch.Artisan, 104),
            M("Cooking", Branch.Artisan, 105), M("Farming", Branch.Artisan, 106), M("Crafting", Branch.Artisan, 107),
            P("deeppockets", "Deep Pockets", Branch.Artisan, 1, "+50 carry weight.", "BeltStrength", "Carry the world on your back."),
            P("buildersease", "Builder's Ease", Branch.Artisan, 2, "Building and repairing cost 30% less stamina.", "Hammer", "The hammer remembers the work."),
            P("hearthkeeper", "Hearthkeeper", Branch.Artisan, 3, "Food lasts 25% longer.", "CookedMeat", "A full belly lasts the long night."),
            P("quickstudy", "Quick Study", Branch.Artisan, 1, "+5% XP for every branch per rank.", null, "Every task teaches the willing.", 3),
            P("minersgrit", "Miner's Grit", Branch.Artisan, 2, "Pickaxe swings cost 15% less stamina per rank.", null, "The mountain yields to the patient.", 2),
            P("hardworker", "Hard Worker", Branch.Artisan, 3, "+15% stamina regeneration.", null, "Rest is for the dead."),
            K("homesteader", "Homesteader", Branch.Artisan, "+100 carry weight, +25% XP for every branch.\nDrawback: you take 25% more damage.", "Build the hall. Let others hold the shield wall."),
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

    public enum Block { None, Maxed, NeedSpent, KeystoneTaken, NoPoints }

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

        // Vanilla-like power curve: early points come fast, later ones take a playthrough.
        public static float PointCost(int earned, float firstXp, float exponent) => firstXp * (float)Math.Pow(earned + 1, exponent);

        // Returns points gained.
        public int AddXp(Branch b, float amount, float firstXp, float exponent)
        {
            int i = (int)b, gained = 0;
            Xp[i] += amount;
            for (float cost; Xp[i] >= (cost = PointCost(Earned[i], firstXp, exponent)); gained++)
            {
                Xp[i] -= cost;
                Earned[i]++;
            }
            return gained;
        }

        // Only one keystone in the whole tree; the Rune of Forgetting frees the choice.
        public string Keystone => Ranks.Keys.FirstOrDefault(id => Ranks[id] > 0 && TreeDef.ById[id].IsKeystone);

        public Block CanSpend(string id) => CanSpend(id, out _);

        // Block.None when the next rank can be bought. `need` = points required in the branch for NeedSpent.
        public Block CanSpend(string id, out int need)
        {
            var n = TreeDef.ById[id];
            int rank = Rank(id);
            need = n.RequiredSpent(rank + 1);
            if (rank >= n.MaxRank) return Block.Maxed;
            if (Spent(n.Branch) < need) return Block.NeedSpent;
            if (n.IsKeystone && rank == 0 && Keystone != null) return Block.KeystoneTaken;
            if (BranchAvailable(n.Branch) == 0 && FreeAvailable <= 0) return Block.NoPoints;
            return Block.None;
        }

        public bool Spend(string id)
        {
            if (CanSpend(id) != Block.None) return false;
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
