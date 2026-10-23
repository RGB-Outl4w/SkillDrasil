using System.Collections.Generic;
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
    // Bonuses scale with the server's PerkStrength; drawbacks stay fixed.
    class SE_Perks : StatusEffect
    {
        // Only ever added to the local player (Tree.Tick), and PerkActive reads the local tree.
        static bool On(string id) => Tree.PerkActive(id);
        static int Rank(string id) => Tree.PerkRank(id);

        // `pct` is the perk's bonus at PerkStrength 1, e.g. 0.2 for "+20%" or "20% less".
        public static float More(float pct) => 1f + pct * Plugin.PerkStrength.Value;
        public static float Less(float pct) => Mathf.Max(0f, 1f - pct * Plugin.PerkStrength.Value);
        static float Flat(float amount) => amount * Plugin.PerkStrength.Value;

        static readonly List<HitData.DamageModPair> runeWard = new List<HitData.DamageModPair>
        {
            new HitData.DamageModPair { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant },
            new HitData.DamageModPair { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant },
            new HitData.DamageModPair { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Resistant },
        };

        static bool IsMagic(SkillType s) => s == SkillType.ElementalMagic || s == SkillType.BloodMagic;
        static bool IsRanged(SkillType s) => s == SkillType.Bows || s == SkillType.Crossbows;
        SkillType Weapon => (m_character as Humanoid)?.GetCurrentWeapon()?.m_shared.m_skillType ?? SkillType.None;

        static bool IsMelee(SkillType s) =>
            s == SkillType.Swords || s == SkillType.Axes || s == SkillType.Clubs || s == SkillType.Polearms ||
            s == SkillType.Spears || s == SkillType.Knives || s == SkillType.Unarmed;

        public override void ModifyAttack(SkillType skill, ref HitData hit)
        {
            var p = m_character as Player;
            if (!p) return;
            if (On("berserkgang") && IsMelee(skill) && p.GetHealthPercentage() < 0.3f) hit.m_damage.Modify(More(0.2f));
            if (On("silentdeath")) hit.m_backstabBonus *= More(0.5f);
            if (On("steadyaim") && IsRanged(skill) && p.IsCrouching()) hit.m_damage.Modify(More(0.15f));
            if (On("arcanesurge") && IsMagic(skill)) hit.m_damage.Modify(More(0.15f));
            if (IsRanged(skill) && Rank("keeneye") > 0) hit.m_damage.Modify(More(0.05f * Rank("keeneye")));
            if (IsMagic(skill) && Rank("galdr") > 0) hit.m_damage.Modify(More(0.05f * Rank("galdr")));

            // Keystones
            if (On("bearshirt") && IsMelee(skill)) hit.m_damage.Modify(More(0.3f));
            if (On("lonewolf")) hit.m_damage.Modify(IsRanged(skill) || skill == SkillType.Knives ? More(0.3f) : IsMelee(skill) ? 0.7f : 1f);
            if (On("volvasoath")) hit.m_damage.Modify(IsMagic(skill) ? More(0.3f) : 0.7f);
        }

        // Incoming hits, on the owner's machine before armor and resistances.
        public override void OnDamaged(HitData hit, Character attacker)
        {
            if (On("homesteader")) hit.ApplyModifier(1.25f);
        }

        public override void ModifyArmorMods(ref float armor)
        {
            armor += Flat(3f * Rank("ironhide"));
            if (On("bearshirt")) armor *= 0.6f;
        }

        public override void ModifyAttackStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            var w = Weapon;
            if (IsMelee(w)) staminaUse *= Less(0.1f * Rank("battlefervor"));
            if (w == SkillType.Pickaxes) staminaUse *= Less(0.15f * Rank("minersgrit"));
        }

        public override void ModifyStagger(float baseValue, ref float use)
        {
            if (On("unshaken")) use *= Less(0.4f);
        }

        public override void ModifySneakStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            staminaUse *= Less(0.2f * Rank("shadowstep"));
        }

        // Lower stealth factor = harder to notice.
        public override void ModifyStealth(float baseStealth, ref float stealth)
        {
            if (On("ghost")) stealth *= Less(0.5f);
        }

        public override void ModifyHealthRegen(ref float regenMultiplier)
        {
            regenMultiplier += Flat(0.15f * Rank("vigor"));
        }

        public override void ModifyDamageMods(ref HitData.DamageModifiers modifiers)
        {
            if (On("runeward")) modifiers.Apply(runeWard);
        }

        public override void ModifySwimStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            staminaUse *= Less(0.2f * Rank("sealborn"));
        }

        public override void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            staminaUse *= Less(0.1f * Rank("springstep"));
        }

        public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir)
        {
            if (On("windrunner") && !character.IsSwimming()) speed += baseSpeed * Flat(0.08f);
        }

        public override void ModifyStaminaRegen(ref float staminaRegen)
        {
            if (On("hardworker")) staminaRegen += Flat(0.15f);
            if (On("endlessroad")) staminaRegen += Flat(0.4f);
        }

        // Feeds branch XP: Player.RaiseSkill multiplies the raise by this before SkillDrasil sees it.
        public override void ModifyRaiseSkill(SkillType skill, ref float value)
        {
            value += Flat(0.05f * Rank("quickstudy"));
            if (On("homesteader")) value += Flat(0.25f);
        }

        public override void ModifyBlockStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            if (On("shieldwall")) staminaUse *= Less(0.25f);
        }

        public override void ModifyTimedBlockBonus(ref float timedBlockBonus)
        {
            if (On("riposte")) timedBlockBonus *= More(0.3f);
        }

        public override void ModifyEitrRegen(ref float eitrRegen)
        {
            if (On("eitrwell")) eitrRegen *= More(0.25f);
            if (On("volvasoath")) eitrRegen *= More(0.25f);
        }

        public override void ModifyRunStaminaDrain(float baseDrain, ref float drain, Vector3 dir)
        {
            if (On("fleetfoot")) drain *= Less(0.2f);
        }

        public override void ModifyDodgeStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            if (On("secondwind")) staminaUse *= Less(0.25f);
        }

        public override void ModifyFallDamage(float baseDamage, ref float damage)
        {
            if (On("featherfall")) damage *= Less(0.5f);
        }

        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
        {
            if (On("deeppockets")) limit += Flat(50f);
            if (On("homesteader")) limit += Flat(100f);
            if (On("endlessroad")) limit -= 100f;
        }

        public override void ModifyHomeItemStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            if (On("buildersease")) staminaUse *= Less(0.3f);
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
            if (!Tree.PerkActive("fletcher") || !inv.ContainsItem(ammoItem) || Random.value >= 0.25f * Plugin.PerkStrength.Value) return;
            ammoItem.m_stack++;
            inv.Changed();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.UseEitr))]
        static void FocusedMind(Player __instance, ref float v)
        {
            if (__instance == Player.m_localPlayer && Tree.PerkActive("focusedmind")) v *= SE_Perks.Less(0.2f);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.UpdateFood))]
        static void Hearthkeeper(Player __instance, ref float dt)
        {
            if (__instance == Player.m_localPlayer && Tree.PerkActive("hearthkeeper")) dt /= SE_Perks.More(0.25f); // food lasts 25% longer
        }
    }
}
