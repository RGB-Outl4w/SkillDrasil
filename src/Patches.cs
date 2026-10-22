// Save-safety rule: vanilla Skill.m_level / m_accumulator are never written while the mod runs.
// We only change what readers see and block raise/lower, so Skills.Save writes the original values back.
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SkillDrasil
{
    // Binds the tree state to the local player and keeps it in Player.m_customData.
    static class Tree
    {
        static Player owner;
        static TreeState state;
        static int migratedPoints;
        static float milestoneTimer;
        public static bool InDeath, PendingSevered;

        public static TreeState Local => For(Player.m_localPlayer);

        public static TreeState For(Player p)
        {
            if (!p) return null;
            if (p == owner) return state;
            if (p != Player.m_localPlayer) return null;
            Bind(p);
            return state;
        }

        public static void Bind(Player p)
        {
            owner = p;
            migratedPoints = 0; // character select previews also bind; never carry their count over
            p.m_customData.TryGetValue(TreeState.Key, out var raw);
            state = TreeState.Parse(raw);
            if (state != null) return;
            if (!string.IsNullOrEmpty(raw))
            {
                Plugin.Log.LogError("Unreadable SkillDrasil data, kept as backup and starting over.");
                p.m_customData[TreeState.Key + ".bak"] = raw;
            }
            state = TreeState.Migrate(p.m_skills.m_skillData.Select(kv => new KeyValuePair<int, float>((int)kv.Key, kv.Value.m_level)));
            migratedPoints = state.Earned.Sum();
        }

        public static void Reset() => state = new TreeState();

        public static bool Severed(Player p) => p.GetSEMan().HaveStatusEffect(Perks.SeveredHash);

        public static bool PerkActive(string id)
        {
            var p = Player.m_localPlayer;
            var s = For(p);
            if (s == null || s.Rank(id) == 0) return false;
            return TreeDef.ById[id].Tier < 3 || !Severed(p);
        }

        public static void Save(Player p)
        {
            if (p == owner && state != null) p.m_customData[TreeState.Key] = state.Serialize();
        }

        public static void OnSpawned(Player p)
        {
            if (p != Player.m_localPlayer) return;
            For(p);
            if (PendingSevered)
            {
                PendingSevered = false;
                var se = p.GetSEMan().AddStatusEffect(Perks.SeveredHash, resetTime: true);
                if (se) se.m_ttl = Plugin.SeveredSeconds.Value;
            }
            if (migratedPoints > 0)
            {
                p.Message(MessageHud.MessageType.Center, L.T("msg_migrated", migratedPoints, Plugin.OpenKey.Value));
                migratedPoints = 0;
            }
        }

        public static void Tick(Player p)
        {
            if (p != Player.m_localPlayer || p.IsDead()) return;
            var seman = p.GetSEMan();
            if (!seman.HaveStatusEffect(Perks.CarrierHash)) seman.AddStatusEffect(Perks.CarrierHash);

            if ((milestoneTimer -= Time.deltaTime) > 0f || !ZoneSystem.instance) return;
            milestoneTimer = 5f;
            var s = For(p);
            var claimed = ZoneSystem.instance.GetGlobalKeys().Where(k => k.StartsWith(TreeDef.BossKeyPrefix))
                .Concat(p.m_knownBiome.Where(TreeDef.MilestoneBiomes.Contains))
                .Count(s.Claim);
            if (claimed > 0)
                p.Message(MessageHud.MessageType.TopLeft, L.T("msg_milestone", claimed));
        }
    }

    [HarmonyPatch]
    static class SkillPatches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Player), nameof(Player.Load))]
        static void Load(Player __instance) => Tree.Bind(__instance);

        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.Save))]
        static void Save(Player __instance) => Tree.Save(__instance);

        [HarmonyPostfix, HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        static void OnSpawned(Player __instance) => Tree.OnSpawned(__instance);

        [HarmonyPostfix, HarmonyPatch(typeof(Player), nameof(Player.Update))]
        static void Update(Player __instance) => Tree.Tick(__instance);

        // Effective level = 20 per mastery rank (one rank less while Severed), plus gear/mead bonuses.
        [HarmonyPrefix, HarmonyPatch(typeof(Skills), nameof(Skills.GetSkillLevel))]
        static bool GetSkillLevel(Skills __instance, Skills.SkillType skillType, ref float __result)
        {
            var s = Tree.For(__instance.m_player);
            if (s == null) return true;
            float level = 0f;
            if (TreeDef.BySkill.TryGetValue((int)skillType, out var n))
            {
                int rank = s.Rank(n.Id);
                if (rank > 0 && Tree.Severed(__instance.m_player)) rank--;
                level = rank * TreeDef.LevelsPerRank;
                __instance.m_player.GetSEMan().ModifySkillLevel(skillType, ref level);
            }
            __result = Mathf.Floor(level);
            return false;
        }

        // Skill use feeds the skill's branch instead of the skill.
        [HarmonyPrefix, HarmonyPatch(typeof(Skills), nameof(Skills.RaiseSkill))]
        static bool RaiseSkill(Skills __instance, Skills.SkillType skillType, float factor)
        {
            var s = Tree.For(__instance.m_player);
            if (s == null) return true;
            if (!TreeDef.BySkill.TryGetValue((int)skillType, out var n)) return false;
            var def = __instance.GetSkillDef(skillType);
            float xp = (def?.m_increseStep ?? 1f) * factor * Game.m_skillGainRate;
            if (s.AddXp(n.Branch, xp, Plugin.XpBase.Value, Plugin.XpGrowth.Value) > 0)
            {
                var p = __instance.m_player;
                p.OnSkillLevelup(skillType, 0f);
                p.Message(MessageHud.MessageType.TopLeft,
                    L.T("msg_point", L.Branch(n.Branch), s.BranchAvailable(n.Branch)), 0, def?.m_icon);
            }
            return false;
        }

        // Only called on a "hard" death (vanilla skips it on a quick re-death), so no stacking.
        [HarmonyPrefix, HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
        static bool OnDeath(Skills __instance)
        {
            var s = Tree.For(__instance.m_player);
            if (s == null) return true;
            s.LoseXp(__instance.m_DeathLowerFactor * Game.m_skillReductionRate);
            Tree.PendingSevered = true;
            __instance.m_player.Message(MessageHud.MessageType.TopLeft, L.T("msg_severed"));
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
        static void PlayerDeathStart() => Tree.InDeath = true;

        [HarmonyFinalizer, HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
        static void PlayerDeathEnd() => Tree.InDeath = false;

        // Clear() runs on death with the DeathSkillsReset world modifier (wipe the tree, keep vanilla data)
        // and on the resetcharacter command (reset both).
        [HarmonyPrefix, HarmonyPatch(typeof(Skills), nameof(Skills.Clear))]
        static bool Clear(Skills __instance)
        {
            var s = Tree.For(__instance.m_player);
            if (s == null) return true;
            if (!Tree.InDeath)
            {
                Tree.Reset();
                return true;
            }
            s.Wipe();
            __instance.m_player.Message(MessageHud.MessageType.TopLeft, L.T("msg_wither"));
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Skills), nameof(Skills.CheatRaiseSkill))]
        static bool CheatRaiseSkill() => BlockCheat();

        [HarmonyPrefix, HarmonyPatch(typeof(Skills), nameof(Skills.CheatResetSkill))]
        static bool CheatResetSkill() => BlockCheat();

        static bool BlockCheat()
        {
            Console.instance.Print("SkillDrasil replaces skills. Use skilldrasil_points <n> instead.");
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnOpenSkills))]
        static bool OnOpenSkills()
        {
            InventoryGui.instance.Hide();
            TreeUI.Open();
            return false;
        }
    }
}
