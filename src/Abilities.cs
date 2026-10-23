using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using SkillType = Skills.SkillType;

namespace SkillDrasil
{
    // Active abilities: one per branch, cast with Modifier + 1..5 (branch order), cooldown per ability.
    // Effects on other characters go through vanilla networking: AddStatusEffect and Heal RPC to the owner.
    static class Abilities
    {
        class Def
        {
            public string Id, Fx, Sfx;
            public float Cooldown;
            public int Hash;
        }

        static readonly Def[] defs =
        {
            new Def { Id = "warcry", Cooldown = 120f, Fx = "fx_GP_Activation", Sfx = "sfx_goblinbrute_shout" },
            new Def { Id = "huntersmark", Cooldown = 90f, Fx = "fx_guardstone_activate", Sfx = "sfx_StaffLightning_charge" },
            new Def { Id = "volvaschant", Cooldown = 120f, Fx = "vfx_HealthUpgrade", Sfx = "sfx_greydwarf_shaman_heal" },
            new Def { Id = "galestep", Cooldown = 60f, Fx = "vfx_StaminaUpgrade", Sfx = "sfx_Potion_stamina_Start" },
            new Def { Id = "tirelesstoil", Cooldown = 180f, Fx = "vfx_Potion_stamina_medium", Sfx = "sfx_Potion_stamina_Start_lingering" },
        };

        const float AllyRange = 15f, MarkRange = 25f, ChantEitr = 30f;
        static readonly Dictionary<string, float> readyAt = new Dictionary<string, float>();

        public static Node NodeOf(Branch b) => TreeDef.Nodes.First(n => n.IsAbility && n.Branch == b);
        public static float CooldownOf(string id) => defs.First(d => d.Id == id).Cooldown;
        public static float Remaining(string id) => readyAt.TryGetValue(id, out var t) ? Mathf.Max(0f, t - Time.time) : 0f;
        public static string KeyLabel(Branch b) => $"{ShortKey(Plugin.AbilityModifier.Value)}+{ShortKey(Plugin.AbilityKeys[(int)b].Value)}";

        static string ShortKey(KeyCode k) =>
            k >= KeyCode.Alpha0 && k <= KeyCode.Alpha9 ? ((int)(k - KeyCode.Alpha0)).ToString()
            : k == KeyCode.LeftAlt || k == KeyCode.RightAlt ? "Alt"
            : k == KeyCode.LeftControl || k == KeyCode.RightControl ? "Ctrl"
            : k == KeyCode.LeftShift || k == KeyCode.RightShift ? "Shift"
            : k.ToString();

        public static void Register()
        {
            foreach (var se in new StatusEffect[]
            {
                Make<SE_WarCry>("warcry", 15f), Make<SE_Marked>("huntersmark", 15f), Make<SE_Chant>("volvaschant", 10f),
                Make<SE_Gale>("galestep", 8f), Make<SE_Toil>("tirelesstoil", 30f),
            })
                ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(se, false));
            foreach (var d in defs) d.Hash = ("SkillDrasil_" + d.Id).GetStableHashCode();
            PrefabManager.OnVanillaPrefabsAvailable += SetIcons;
        }

        static readonly List<StatusEffect> made = new List<StatusEffect>();

        static T Make<T>(string id, float ttl) where T : StatusEffect
        {
            var se = ScriptableObject.CreateInstance<T>();
            se.name = "SkillDrasil_" + id;
            se.m_name = "$sd_name_" + id;
            se.m_tooltip = "$sd_effect_" + id;
            se.m_ttl = ttl;
            made.Add(se);
            return se;
        }

        static void SetIcons()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= SetIcons;
            foreach (var se in made) se.m_icon = Icons.Get(se.name.Substring("SkillDrasil_".Length));
        }

        // Alt+number would also use the hotbar slot; swallow that while the modifier is held.
        public static bool ModifierHeld => ZInput.GetKey(Plugin.AbilityModifier.Value, false);

        public static void Tick()
        {
            var p = Player.m_localPlayer;
            if (p && !p.IsDead() && ModifierHeld && !TreeUI.IsOpen && !Console.IsVisible() && !TextInput.IsVisible() &&
                !Menu.IsVisible() && !(Chat.instance && Chat.instance.HasFocus()) && !InventoryGui.IsVisible())
            {
                for (int b = 0; b < 5; b++)
                    if (ZInput.GetKeyDown(Plugin.AbilityKeys[b].Value, false)) Cast(p, NodeOf((Branch)b));
            }
            UpdateBar(p);
        }

        static void Cast(Player p, Node n)
        {
            var s = Tree.For(p);
            if (s == null || s.Rank(n.Id) == 0) return;
            float left = Remaining(n.Id);
            if (left > 0f)
            {
                p.Message(MessageHud.MessageType.TopLeft, L.T("ability_cooldown", L.Name(n), Mathf.CeilToInt(left)));
                return;
            }
            var def = defs.First(d => d.Id == n.Id);
            var pos = p.transform.position;
            switch (n.Id)
            {
                case "warcry":
                case "volvaschant":
                    if (n.Id == "volvaschant")
                    {
                        if (!p.HaveEitr(ChantEitr))
                        {
                            Hud.instance.EitrBarEmptyFlash();
                            p.Message(MessageHud.MessageType.Center, "$hud_eitrrequired");
                            return;
                        }
                        p.UseEitr(ChantEitr);
                    }
                    var players = new List<Player>();
                    Player.GetPlayersInRange(pos, AllyRange, players);
                    foreach (var ally in players) ally.GetSEMan().AddStatusEffect(def.Hash, resetTime: true);
                    break;
                case "huntersmark":
                    var chars = new List<Character>();
                    Character.GetCharactersInRange(pos, MarkRange, chars);
                    int marked = 0;
                    foreach (var c in chars.Where(c => c != p && !c.IsDead() && BaseAI.IsEnemy(p, c)))
                    {
                        c.GetSEMan().AddStatusEffect(def.Hash, resetTime: true);
                        marked++;
                    }
                    if (marked == 0)
                    {
                        p.Message(MessageHud.MessageType.TopLeft, L.T("ability_no_targets"));
                        return; // no cooldown spent on an empty cast
                    }
                    break;
                case "galestep":
                    p.AddStamina(p.GetMaxStamina() * 0.5f);
                    p.GetSEMan().AddStatusEffect(def.Hash, resetTime: true);
                    break;
                case "tirelesstoil":
                    p.GetSEMan().AddStatusEffect(def.Hash, resetTime: true);
                    break;
            }
            PlayFx(def.Fx, p);
            PlayFx(def.Sfx, p);
            readyAt[n.Id] = Time.time + def.Cooldown;
            p.Message(MessageHud.MessageType.TopLeft, L.Name(n) + "!");
        }

        static void PlayFx(string prefabName, Player p)
        {
            var prefab = ZNetScene.instance ? ZNetScene.instance.GetPrefab(prefabName) : null;
            if (!prefab) return;
            new EffectList { m_effectPrefabs = new[] { new EffectList.EffectData { m_prefab = prefab, m_enabled = true, m_attach = true } } }
                .Create(p.transform.position, p.transform.rotation, p.transform);
        }

        // ---------- HUD bar: learned abilities with key and cooldown sweep, bottom right ----------

        class Slot
        {
            public GameObject Root;
            public Image Icon, Sweep;
            public Text Key, Time;
        }

        static readonly Slot[] slots = new Slot[5];

        static void UpdateBar(Player p)
        {
            var s = p && Hud.instance ? Tree.For(p) : null;
            bool show = s != null && !TreeUI.IsOpen && GUIManager.CustomGUIFront && !p.IsDead();
            int col = 0;
            for (int b = 0; b < 5; b++)
            {
                var n = NodeOf((Branch)b);
                bool learned = show && s.Rank(n.Id) > 0;
                if (!learned)
                {
                    if (slots[b] != null && slots[b].Root) slots[b].Root.SetActive(false);
                    continue;
                }
                if (slots[b] == null || !slots[b].Root) slots[b] = MakeSlot(n);
                var slot = slots[b];
                slot.Root.SetActive(true);
                ((RectTransform)slot.Root.transform).anchoredPosition = new Vector2(-50f - col * 66f, 96f);
                col++;
                float left = Remaining(n.Id);
                slot.Sweep.fillAmount = left / CooldownOf(n.Id);
                slot.Time.text = left > 0f ? Mathf.CeilToInt(left).ToString() : "";
                slot.Icon.color = left > 0f ? new Color(0.5f, 0.45f, 0.4f) : new Color(1f, 0.74f, 0.28f);
                slot.Key.text = KeyLabel((Branch)b);
            }
        }

        static Slot MakeSlot(Node n)
        {
            var corner = new Vector2(1f, 0f);
            var root = new GameObject("SkillDrasil_" + n.Id, typeof(RectTransform));
            var rt = (RectTransform)root.transform;
            rt.SetParent(GUIManager.CustomGUIFront.transform, false);
            rt.anchorMin = rt.anchorMax = corner;
            rt.sizeDelta = new Vector2(58f, 58f);

            Image Add(string name, Sprite sprite, float size, Color color)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Image));
                var r = (RectTransform)go.transform;
                r.SetParent(rt, false);
                r.sizeDelta = new Vector2(size, size);
                var img = go.GetComponent<Image>();
                img.sprite = sprite;
                img.color = color;
                img.raycastTarget = false;
                return img;
            }

            var slot = new Slot { Root = root };
            Add("frame", Icons.Get("frame_perk"), 58f, Color.white);
            slot.Icon = Add("icon", Icons.Get(n.Id), 36f, Color.white);
            slot.Sweep = Add("sweep", Icons.Get("frame_perk"), 52f, new Color(0f, 0f, 0f, 0.65f));
            slot.Sweep.type = Image.Type.Filled;
            slot.Sweep.fillMethod = Image.FillMethod.Radial360;
            slot.Sweep.fillOrigin = (int)Image.Origin360.Top;
            slot.Sweep.fillClockwise = false;
            slot.Time = GUIManager.Instance.CreateText("", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                GUIManager.Instance.AveriaSerifBold, 20, Color.white, true, Color.black, 58f, 30f, false).GetComponent<Text>();
            slot.Time.alignment = TextAnchor.MiddleCenter;
            slot.Key = GUIManager.Instance.CreateText("", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -38f),
                GUIManager.Instance.AveriaSerifBold, 13, new Color(0.88f, 0.82f, 0.68f), true, Color.black, 80f, 20f, false).GetComponent<Text>();
            slot.Key.alignment = TextAnchor.MiddleCenter;
            return slot;
        }

        [HarmonyPatch]
        static class Patches
        {
            [HarmonyPrefix, HarmonyPatch(typeof(Player), nameof(Player.UseHotbarItem))]
            static bool KeepHotbarFree(Player __instance) => __instance != Player.m_localPlayer || !ModifierHeld;
        }
    }

    // ---------- ability status effects (registered on every client, so allies and enemies can carry them) ----------

    class SE_WarCry : StatusEffect
    {
        public override void ModifyAttack(SkillType skill, ref HitData hit) => hit.m_damage.Modify(1.2f);
    }

    // On a marked enemy: every hit, from anyone, lands 25% harder (runs on the enemy's owner).
    class SE_Marked : StatusEffect
    {
        public override void OnDamaged(HitData hit, Character attacker) => hit.ApplyModifier(1.25f);
    }

    class SE_Chant : StatusEffect
    {
        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            if (m_character.IsOwner()) m_character.Heal(m_character.GetMaxHealth() * 0.04f * dt, false); // 40% over 10 s
        }
    }

    class SE_Gale : StatusEffect
    {
        public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir) => speed += baseSpeed * 0.2f;
    }

    class SE_Toil : StatusEffect
    {
        public override void ModifyAttackStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            var w = (m_character as Humanoid)?.GetCurrentWeapon()?.m_shared.m_skillType ?? SkillType.None;
            if (w == SkillType.Pickaxes || w == SkillType.Axes) staminaUse = 0f;
        }

        public override void ModifyHomeItemStaminaUsage(float baseStaminaUse, ref float staminaUse) => staminaUse = 0f;
    }
}
