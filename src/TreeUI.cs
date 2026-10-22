using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SkillDrasil
{
    // Replaces the vanilla skills dialog. One tab per branch, drawn as a tree:
    // masteries are the roots, the trunk grows with points spent, perks form the crown.
    static class TreeUI
    {
        static readonly Vector2 Mid = new Vector2(0.5f, 0.5f);
        static readonly Color Gold = new Color(1f, 0.74f, 0.28f, 1f);
        static readonly Color Pale = new Color(0.88f, 0.82f, 0.68f, 1f);
        static readonly Color Dim = new Color(0.38f, 0.36f, 0.33f, 1f);
        static readonly Color DimFrame = new Color(0.6f, 0.6f, 0.6f, 1f);
        static readonly Color Bark = new Color(0.28f, 0.2f, 0.13f, 1f);
        static readonly Color Sap = new Color(0.95f, 0.62f, 0.18f, 1f);
        static readonly Color Good = new Color(0.55f, 0.9f, 0.45f, 1f);
        static readonly Color Bad = new Color(1f, 0.45f, 0.4f, 1f);
        static readonly Color Shade = new Color(0f, 0f, 0f, 0.4f);

        // Trunk geometry inside the tree canvas: perk tier t sits where 3t points have been spent.
        const float TrunkBase = -120f, TrunkTop = 140f, MaxGate = 9f;

        class NodeView
        {
            public Image Ring, Frame, Icon;
            public Image[] Pips;
            public Text Label;
        }

        static GameObject panel;
        static string builtLanguage;
        static RectTransform canvas;
        static Text freeText, severedText;
        static readonly Button[] tabs = new Button[5];
        static readonly Text[] tabLabels = new Text[5];
        static readonly Image[] tabIcons = new Image[5];
        static readonly Dictionary<string, NodeView> views = new Dictionary<string, NodeView>();
        static readonly List<(Image line, string node)> roots = new List<(Image, string)>();
        static RectTransform trunkFill;

        static Image cardFrame, cardIcon, xpFill;
        static Text cardName, cardKind, cardEffect, cardLore, cardNow, cardReq, branchLine, xpText;
        static Image[] cardPips;
        static Button learn;
        static Text learnLabel;

        static Branch tab;
        static string selected, hovered;

        public static bool IsOpen => panel && panel.activeSelf;

        public static void Tick()
        {
            if (IsOpen && (ZInput.GetKeyDown(KeyCode.Escape) || ZInput.GetKeyDown(Plugin.OpenKey.Value) || !Player.m_localPlayer))
                Close();
            else if (!IsOpen && ZInput.GetKeyDown(Plugin.OpenKey.Value) && CanOpen())
                Open();
        }

        static bool CanOpen() =>
            Player.m_localPlayer && !Console.IsVisible() && !TextInput.IsVisible() && !Menu.IsVisible() &&
            !(Chat.instance && Chat.instance.HasFocus()) && !InventoryGui.IsVisible();

        public static void Open()
        {
            if (Tree.Local == null) return;
            if (panel && builtLanguage != L.Language) UnityEngine.Object.Destroy(panel); // language changed in settings
            if (!panel) Build();
            panel.SetActive(true);
            GUIManager.BlockInput(true);
            ShowTab(tab);
        }

        public static void Close()
        {
            if (panel) panel.SetActive(false);
            GUIManager.BlockInput(false);
        }

        // ---------- building blocks ----------

        // Norse has no Cyrillic glyphs; other languages get Averia for headings.
        static Font HeadingFont => L.Language == "English" ? GUIManager.Instance.NorseBold : GUIManager.Instance.AveriaSerifBold;

        static RectTransform Rect(Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("sd", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = Mid;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        static Image Img(Transform parent, Vector2 pos, Vector2 size, Color color, Sprite sprite = null, bool raycast = false)
        {
            var img = Rect(parent, pos, size).gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        static Text Label(Transform parent, string text, Vector2 pos, float width, float height, int size, Color color,
            Font font = null, TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var t = GUIManager.Instance.CreateText(text, parent, Mid, Mid, pos, font ?? GUIManager.Instance.AveriaSerifBold,
                size, color, true, Color.black, width, height, false).GetComponent<Text>();
            t.alignment = align;
            t.fontStyle = style;
            t.raycastTarget = false;
            return t;
        }

        static Button MakeButton(Transform parent, string text, Vector2 pos, float width, float height, Action onClick)
        {
            var b = GUIManager.Instance.CreateButton(text, parent, Mid, Mid, pos, width, height).GetComponent<Button>();
            b.onClick.AddListener(() => onClick());
            return b;
        }

        static Image Line(Transform parent, Vector2 a, Vector2 b, float thickness, Color color)
        {
            var img = Img(parent, a, new Vector2(Vector2.Distance(a, b), thickness), color);
            img.rectTransform.pivot = new Vector2(0f, 0.5f);
            img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            return img;
        }

        static Image[] Pips(Transform parent, Vector2 center, int count, float size)
        {
            var pips = new Image[count];
            float step = size + 2f;
            var sprite = Icons.Get("pip");
            for (int i = 0; i < count; i++)
                pips[i] = Img(parent, center + new Vector2((i - (count - 1) / 2f) * step, 0f), new Vector2(size, size), Bark, sprite);
            return pips;
        }

        static void SetPips(Image[] pips, int rank)
        {
            for (int i = 0; i < pips.Length; i++) pips[i].color = i < rank ? Gold : new Color(0.2f, 0.16f, 0.12f, 1f);
        }

        static void On(GameObject go, EventTriggerType type, Action action)
        {
            var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }

        static Sprite IconOf(Node n) => Icons.Get(n.Id);
        static Sprite BranchIcon(Branch b) => Icons.Get("branch_" + b.ToString().ToLowerInvariant());
        static Sprite FrameOf(Node n) => Icons.Get(n.IsMastery ? "frame_mastery" : "frame_perk");

        // ---------- layout ----------

        static void Build()
        {
            builtLanguage = L.Language;
            panel = GUIManager.Instance.CreateWoodpanel(GUIManager.CustomGUIFront.transform, Mid, Mid, Vector2.zero, 1180f, 760f, false);
            var root = panel.transform;

            Label(root, "SkillDrasil", new Vector2(-400f, 336f), 340f, 50f, 40, GUIManager.Instance.ValheimOrange,
                GUIManager.Instance.NorseBold, TextAnchor.MiddleLeft);
            freeText = Label(root, "", new Vector2(160f, 344f), 480f, 30f, 22, GUIManager.Instance.ValheimOrange, align: TextAnchor.MiddleRight);
            severedText = Label(root, "", new Vector2(160f, 318f), 480f, 24f, 15, Bad, align: TextAnchor.MiddleRight);
            MakeButton(root, L.T("close"), new Vector2(500f, 336f), 130f, 38f, Close);

            for (int b = 0; b < 5; b++)
            {
                var br = (Branch)b;
                var t = MakeButton(root, "", new Vector2(-470f + b * 235f, 272f), 225f, 48f, () => ShowTab(br));
                tabLabels[b] = t.GetComponentInChildren<Text>();
                tabLabels[b].rectTransform.offsetMin = new Vector2(34f, 0f);
                tabIcons[b] = Img(t.transform, new Vector2(-84f, 0f), new Vector2(36f, 36f), Pale, BranchIcon(br));
                tabs[b] = t;
            }

            canvas = Img(root, new Vector2(-195f, -60f), new Vector2(740f, 590f), Shade).rectTransform;
            BuildCard(Img(root, new Vector2(385f, -60f), new Vector2(390f, 590f), Shade).transform);
        }

        static void BuildCard(Transform card)
        {
            cardFrame = Img(card, new Vector2(0f, 218f), new Vector2(116f, 116f), Color.white);
            cardIcon = Img(card, new Vector2(0f, 218f), new Vector2(80f, 80f), Gold);
            cardName = Label(card, "", new Vector2(0f, 140f), 370f, 36f, 26, GUIManager.Instance.ValheimOrange, HeadingFont);
            cardKind = Label(card, "", new Vector2(0f, 112f), 370f, 24f, 15, Pale);
            cardPips = Pips(card, new Vector2(0f, 88f), 5, 16f);
            cardEffect = Label(card, "", new Vector2(0f, 40f), 350f, 66f, 18, Color.white);
            cardLore = Label(card, "", new Vector2(0f, -18f), 350f, 52f, 15, Pale, GUIManager.Instance.AveriaSerif, style: FontStyle.Italic);
            cardNow = Label(card, "", new Vector2(0f, -62f), 360f, 26f, 15, Color.white);
            cardReq = Label(card, "", new Vector2(0f, -90f), 360f, 26f, 15, Good);
            learn = MakeButton(card, "", new Vector2(0f, -136f), 300f, 46f, Learn);
            learnLabel = learn.GetComponentInChildren<Text>();

            Img(card, new Vector2(0f, -178f), new Vector2(340f, 2f), Bark);
            branchLine = Label(card, "", new Vector2(0f, -200f), 370f, 26f, 16, Pale);
            Img(card, new Vector2(0f, -228f), new Vector2(324f, 16f), Bark);
            xpFill = Img(card, new Vector2(-160f, -228f), new Vector2(0f, 10f), Sap);
            xpFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            xpText = Label(card, "", new Vector2(0f, -252f), 370f, 22f, 14, Color.white);
        }

        static void ShowTab(Branch b)
        {
            tab = b;
            if (selected != null && TreeDef.ById[selected].Branch != b) selected = null;
            hovered = null;
            for (int i = canvas.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(canvas.GetChild(i).gameObject);
            views.Clear();
            roots.Clear();
            BuildTree(b);
            Refresh();
        }

        static void BuildTree(Branch b)
        {
            var c = canvas.transform;
            var masteries = TreeDef.Nodes.Where(n => n.Branch == b && n.IsMastery).ToArray();
            var perks = TreeDef.Nodes.Where(n => n.Branch == b && !n.IsMastery).OrderBy(n => n.Tier).ToArray();

            // Root positions: a shallow arc under the trunk, outer roots a little higher.
            float spacing = masteries.Length > 1 ? Mathf.Min(150f, 660f / (masteries.Length - 1)) : 0f;
            var rootPos = masteries.Select((n, i) =>
            {
                float x = (i - (masteries.Length - 1) / 2f) * spacing;
                return new Vector2(x, -200f + 45f * (x / 330f) * (x / 330f));
            }).ToArray();
            var basePoint = new Vector2(0f, TrunkBase);
            for (int i = 0; i < masteries.Length; i++)
                roots.Add((Line(c, basePoint, rootPos[i], 6f, Bark), masteries[i].Id));

            // Trunk with its growth fill and tier gates.
            Img(c, new Vector2(0f, (TrunkBase + TrunkTop) / 2f), new Vector2(16f, TrunkTop - TrunkBase), Bark);
            trunkFill = Img(c, basePoint, new Vector2(8f, 0f), Sap).rectTransform;
            trunkFill.pivot = new Vector2(0.5f, 0f);
            var perkPos = new[] { new Vector2(-215f, GateY(1)), new Vector2(215f, GateY(2)), new Vector2(0f, 218f) };
            for (int t = 1; t <= 3; t++)
            {
                var gate = new Vector2(0f, GateY(t));
                Line(c, gate, perkPos[t - 1], 8f, Bark);
                Img(c, gate, new Vector2(26f, 6f), Pale);
                Label(c, $"{t * 3}", gate + new Vector2(t == 2 ? -30f : 30f, t == 3 ? -12f : 0f), 40f, 22f, 15, Pale);
            }

            for (int i = 0; i < masteries.Length; i++) views[masteries[i].Id] = BuildNode(c, masteries[i], rootPos[i], 64f);
            for (int i = 0; i < perks.Length; i++) views[perks[i].Id] = BuildNode(c, perks[i], perkPos[i], 78f);
        }

        static float GateY(int tier) => TrunkBase + (TrunkTop - TrunkBase) * Mathf.Min(1f, tier * 3f / MaxGate);

        static NodeView BuildNode(Transform parent, Node n, Vector2 pos, float size)
        {
            var v = new NodeView();
            v.Ring = Img(parent, pos, new Vector2(size + 14f, size + 14f), Color.clear,
                Icons.Get(n.IsMastery ? "ring_mastery" : "ring_perk"), raycast: true);
            v.Frame = Img(v.Ring.transform, Vector2.zero, new Vector2(size, size), Color.white, FrameOf(n));
            v.Icon = Img(v.Ring.transform, Vector2.zero, new Vector2(size * 0.62f, size * 0.62f), Gold, IconOf(n));
            float below = -size / 2f - 6f;
            if (n.IsMastery) v.Pips = Pips(v.Ring.transform, new Vector2(0f, below - 4f), n.MaxRank, 10f);
            v.Label = Label(v.Ring.transform, L.Name(n), new Vector2(0f, below - (n.IsMastery ? 22f : 12f)), 160f, 22f, 14, Color.white);

            string id = n.Id;
            On(v.Ring.gameObject, EventTriggerType.PointerEnter, () => { hovered = id; Refresh(); });
            On(v.Ring.gameObject, EventTriggerType.PointerExit, () => { if (hovered == id) { hovered = null; Refresh(); } });
            On(v.Ring.gameObject, EventTriggerType.PointerClick, () => { selected = id; Refresh(); });
            return v;
        }

        // ---------- state ----------

        static void Learn()
        {
            if (selected != null && Tree.Local?.Spend(selected) == true) Refresh();
        }

        static void Refresh()
        {
            var s = Tree.Local;
            if (s == null || !panel) return;
            bool severed = Tree.Severed(Player.m_localPlayer);

            freeText.text = L.T("free", s.FreeAvailable);
            severedText.text = severed ? L.T("severed_banner") : "";

            for (int b = 0; b < 5; b++)
            {
                int avail = s.BranchAvailable((Branch)b);
                tabLabels[b].text = L.Branch((Branch)b) + (avail > 0 ? $"  <color=#f0a030>{avail}</color>" : "");
                tabs[b].interactable = b != (int)tab;
                tabIcons[b].color = b == (int)tab ? Gold : Pale;
            }

            int spent = s.Spent(tab);
            trunkFill.sizeDelta = new Vector2(8f, (TrunkTop - TrunkBase) * Mathf.Min(1f, spent / MaxGate));
            foreach (var (line, node) in roots) line.color = s.Rank(node) > 0 ? Sap : Bark;

            foreach (var kv in views)
            {
                var n = TreeDef.ById[kv.Key];
                var v = kv.Value;
                int rank = s.Rank(n.Id);
                bool canLearn = s.CanSpend(n.Id) == Block.None;
                v.Icon.color = rank > 0 ? Gold : canLearn ? Pale : Dim;
                v.Frame.color = rank > 0 || canLearn ? Color.white : DimFrame;
                v.Ring.color = n.Id == selected ? Color.white
                    : rank >= n.MaxRank ? GUIManager.Instance.ValheimYellow
                    : canLearn ? Sap
                    : Color.clear;
                v.Label.color = rank > 0 ? GUIManager.Instance.ValheimOrange : canLearn ? Color.white : Dim;
                if (v.Pips != null) SetPips(v.Pips, rank);
            }

            RefreshCard(s, severed);
        }

        static void RefreshCard(TreeState s, bool severed)
        {
            int b = (int)tab;
            float cost = TreeState.PointCost(s.Earned[b], Plugin.XpBase.Value, Plugin.XpGrowth.Value);
            branchLine.text = TreeState.BranchCap > 0
                ? L.T("branch_line_cap", L.Branch(tab), s.BranchAvailable(tab), s.Spent(tab), TreeState.BranchCap)
                : L.T("branch_line", L.Branch(tab), s.BranchAvailable(tab), s.Spent(tab));
            xpFill.rectTransform.sizeDelta = new Vector2(320f * Mathf.Clamp01(s.Xp[b] / cost), 10f);
            xpText.text = L.T("xp", s.Xp[b].ToString("0"), cost.ToString("0"));

            string id = hovered ?? selected;
            if (id == null)
            {
                cardFrame.sprite = Icons.Get("frame_perk");
                cardIcon.sprite = BranchIcon(tab);
                cardIcon.color = Gold;
                cardName.text = L.Branch(tab);
                cardKind.text = L.T("branch_kind");
                foreach (var p in cardPips) p.enabled = false;
                cardEffect.text = L.T("branch_help");
                cardLore.text = L.BranchLore(tab);
                cardNow.text = L.T("hover_help");
                cardReq.text = "";
                learn.gameObject.SetActive(false);
                return;
            }

            var n = TreeDef.ById[id];
            int rank = s.Rank(id);
            var block = s.CanSpend(id, out int need);
            cardFrame.sprite = FrameOf(n);
            cardIcon.sprite = IconOf(n);
            cardIcon.color = rank > 0 ? Gold : block == Block.None ? Pale : Dim;
            cardName.text = L.Name(n);
            cardKind.text = n.IsMastery ? L.T("kind_mastery", L.Branch(tab)) : L.T("kind_perk", n.Tier, L.Branch(tab));
            foreach (var p in cardPips) p.enabled = n.IsMastery;
            SetPips(cardPips, rank);
            cardEffect.text = L.Effect(n);
            cardLore.text = n.IsMastery ? SkillDescription(n.Skill) : L.Lore(n);

            if (n.IsMastery)
            {
                int effective = severed && rank > 0 ? rank - 1 : rank;
                cardNow.text = rank >= n.MaxRank
                    ? L.T("level_max", effective * TreeDef.LevelsPerRank)
                    : L.T("level_next", effective * TreeDef.LevelsPerRank, (effective + 1) * TreeDef.LevelsPerRank, rank + 1);
            }
            else cardNow.text = rank > 0 ? L.T(n.Tier == 3 && severed ? "learned_inactive" : "learned") : L.T("not_learned");

            cardReq.text = block == Block.Maxed ? "" : L.Reason(block, need, n.Branch);
            cardReq.color = block == Block.None ? Good : Bad;

            bool showLearn = id == selected && rank < n.MaxRank;
            learn.gameObject.SetActive(showLearn);
            if (showLearn)
            {
                learn.interactable = block == Block.None;
                learnLabel.text = s.BranchAvailable(n.Branch) > 0 ? L.T("learn_branch", L.Branch(tab)) : L.T("learn_free");
            }
            else if (id != selected && block == Block.None) cardReq.text += "  ·  " + L.T("click_select");
        }

        static string SkillDescription(int skill)
        {
            var def = Player.m_localPlayer.m_skills.m_skills.FirstOrDefault(d => (int)d.m_skill == skill);
            return def != null && Localization.instance != null ? Localization.instance.Localize(def.m_description) : "";
        }
    }
}
