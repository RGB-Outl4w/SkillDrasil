using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using SkillType = Skills.SkillType;

namespace SkillDrasil
{
    static class Perks
    {
        public static readonly int CarrierHash = "SkillDrasil_Perks".GetStableHashCode();
        public static readonly int SeveredHash = "SkillDrasil_Severed".GetStableHashCode();
        const string RuneName = "SkillDrasil_RuneOfForgetting";

        static StatusEffect severed, forget;

        public static void Register()
        {
            var carrier = ScriptableObject.CreateInstance<SE_Perks>();
            carrier.name = "SkillDrasil_Perks";
            carrier.m_name = "SkillDrasil";

            severed = ScriptableObject.CreateInstance<StatusEffect>();
            severed.name = "SkillDrasil_Severed";
            severed.m_name = "$sd_se_severed";
            severed.m_tooltip = "$sd_se_severed_tip";

            forget = ScriptableObject.CreateInstance<SE_Forget>();
            forget.name = "SkillDrasil_Forget";
            forget.m_name = "Rune of Forgetting";
            forget.m_ttl = 1f;

            foreach (var se in new[] { carrier, severed, forget })
                ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(se, false));

            PrefabManager.OnVanillaPrefabsAvailable += AddRune;
        }

        static void AddRune()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddRune;
            severed.m_icon = Icons.Get("se_severed");

            var cfg = new ItemConfig
            {
                Name = "$sd_item_rune",
                Description = "$sd_item_rune_desc",
                CraftingStation = CraftingStations.Workbench,
                Icons = new[] { Icons.Get("item_rune") },
            };
            cfg.AddRequirement("Stone", 10);
            cfg.AddRequirement("Resin", 10);
            cfg.AddRequirement("GreydwarfEye", 5);
            var rune = new CustomItem(RuneName, "MeadTasty", cfg);
            rune.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = forget;
            ItemManager.Instance.AddItem(rune);
        }
    }

    // Hidden, always-on status effect on the local player. Hosts every perk that a vanilla
    // StatusEffect hook can express. Status effects are not saved, so nothing lingers after uninstall.
    class SE_Perks : StatusEffect
    {
        // Only ever added to the local player (Tree.Tick), and PerkActive reads the local tree.
        static bool On(string id) => Tree.PerkActive(id);

        static bool IsMelee(SkillType s) =>
            s == SkillType.Swords || s == SkillType.Axes || s == SkillType.Clubs || s == SkillType.Polearms ||
            s == SkillType.Spears || s == SkillType.Knives || s == SkillType.Unarmed;

        public override void ModifyAttack(SkillType skill, ref HitData hit)
        {
            var p = m_character as Player;
            if (!p) return;
            if (On("berserkgang") && IsMelee(skill) && p.GetHealthPercentage() < 0.3f) hit.m_damage.Modify(1.2f);
            if (On("silentdeath")) hit.m_backstabBonus *= 1.5f;
            if (On("steadyaim") && (skill == SkillType.Bows || skill == SkillType.Crossbows) && p.IsCrouching()) hit.m_damage.Modify(1.15f);
            if (On("arcanesurge") && (skill == SkillType.ElementalMagic || skill == SkillType.BloodMagic)) hit.m_damage.Modify(1.15f);
        }

        public override void ModifyBlockStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            if (On("shieldwall")) staminaUse *= 0.75f;
        }

        public override void ModifyTimedBlockBonus(ref float timedBlockBonus)
        {
            if (On("riposte")) timedBlockBonus *= 1.3f;
        }

        public override void ModifyEitrRegen(ref float eitrRegen)
        {
            if (On("eitrwell")) eitrRegen *= 1.25f;
        }

        public override void ModifyRunStaminaDrain(float baseDrain, ref float drain, Vector3 dir)
        {
            if (On("fleetfoot")) drain *= 0.8f;
        }

        public override void ModifyDodgeStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            if (On("secondwind")) staminaUse *= 0.75f;
        }

        public override void ModifyFallDamage(float baseDamage, ref float damage)
        {
            if (On("featherfall")) damage *= 0.5f;
        }

        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
        {
            if (On("deeppockets")) limit += 50f;
        }

        public override void ModifyHomeItemStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            if (On("buildersease")) staminaUse *= 0.7f;
        }
    }

    // Added by consuming the Rune of Forgetting.
    class SE_Forget : StatusEffect
    {
        public override void Setup(Character character)
        {
            base.Setup(character);
            if (character != Player.m_localPlayer) return;
            Tree.Local?.Respec();
            character.Message(MessageHud.MessageType.Center, L.T("msg_respec"));
        }
    }

    // Perks with no StatusEffect hook.
    [HarmonyPatch]
    static class PerkPatches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Attack), nameof(Attack.UseAmmo))]
        static void FletchersLuck(Attack __instance, bool __result, ref ItemDrop.ItemData ammoItem)
        {
            if (!__result || ammoItem == null || __instance.m_character != Player.m_localPlayer) return;
            if (ammoItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable) return;
            var inv = __instance.m_character.GetInventory();
            if (!Tree.PerkActive("fletcher") || !inv.ContainsItem(ammoItem) || Random.value >= 0.25f) return;
            ammoItem.m_stack++;
            inv.Changed();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.UseEitr))]
        static void FocusedMind(Player __instance, ref float v)
        {
            if (__instance == Player.m_localPlayer && Tree.PerkActive("focusedmind")) v *= 0.8f;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.UpdateFood))]
        static void Hearthkeeper(Player __instance, ref float dt)
        {
            if (__instance == Player.m_localPlayer && Tree.PerkActive("hearthkeeper")) dt *= 0.8f;
        }
    }
}
