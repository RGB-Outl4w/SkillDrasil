using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace SkillDrasil
{
    // Replaces the vanilla skills dialog: 5 branch columns, click a node to select it, Learn to buy a rank.
    static class TreeUI
    {
        const float ColumnWidth = 210f, RowHeight = 36f;
        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        static GameObject panel;
        static Text header, details;
        static Button learn;
        static readonly Text[] branchInfo = new Text[5];
        static readonly Dictionary<string, Text> labels = new Dictionary<string, Text>();
        static string selected;

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
            if (!panel) Build();
            panel.SetActive(true);
            GUIManager.BlockInput(true);
            Refresh();
        }

        public static void Close()
        {
            if (panel) panel.SetActive(false);
            GUIManager.BlockInput(false);
        }

        static Text Label(Transform parent, string text, Vector2 pos, float width, float height, int size, Color color)
        {
            var t = GUIManager.Instance.CreateText(text, parent, Center, Center, pos, GUIManager.Instance.AveriaSerifBold,
                size, color, true, Color.black, width, height, false).GetComponent<Text>();
            t.alignment = TextAnchor.MiddleCenter;
            return t;
        }

        static Button MakeButton(Transform parent, string text, Vector2 pos, float width, float height, UnityEngine.Events.UnityAction onClick)
        {
            var b = GUIManager.Instance.CreateButton(text, parent, Center, Center, pos, width, height).GetComponent<Button>();
            b.onClick.AddListener(onClick);
            return b;
        }

        static void Build()
        {
            var gui = GUIManager.Instance;
            panel = gui.CreateWoodpanel(GUIManager.CustomGUIFront.transform, Center, Center, Vector2.zero, 1120f, 700f, false);
            var root = panel.transform;

            header = Label(root, "", new Vector2(0f, 310f), 900f, 40f, 28, GUIManager.Instance.ValheimOrange);
            MakeButton(root, "Close", new Vector2(490f, 310f), 100f, 34f, Close);

            for (int b = 0; b < 5; b++)
            {
                float x = (b - 2) * (ColumnWidth + 10f);
                Label(root, TreeDef.BranchNames[b], new Vector2(x, 262f), ColumnWidth, 30f, 22, GUIManager.Instance.ValheimOrange);
                branchInfo[b] = Label(root, "", new Vector2(x, 234f), ColumnWidth, 26f, 14, Color.white);

                var nodes = TreeDef.Nodes.Where(n => (int)n.Branch == b).ToArray();
                for (int i = 0; i < nodes.Length; i++)
                {
                    string id = nodes[i].Id;
                    var button = MakeButton(root, "", new Vector2(x, 195f - i * RowHeight), ColumnWidth, RowHeight - 4f, () => Select(id));
                    labels[id] = button.GetComponentInChildren<Text>();
                }
            }

            details = Label(root, "", new Vector2(-110f, -285f), 840f, 80f, 16, Color.white);
            learn = MakeButton(root, "Learn", new Vector2(440f, -285f), 160f, 44f, Learn);
            selected = null;
        }

        static void Select(string id)
        {
            selected = id;
            Refresh();
        }

        static void Learn()
        {
            if (selected != null && Tree.Local?.Spend(selected) == true) Refresh();
        }

        static void Refresh()
        {
            var s = Tree.Local;
            if (s == null || !panel) return;
            var p = Player.m_localPlayer;
            bool severed = Tree.Severed(p);

            header.text = $"SkillDrasil  -  Free points: {s.FreeAvailable}" + (severed ? "  -  <color=#ff6060>Severed Roots</color>" : "");

            for (int b = 0; b < 5; b++)
            {
                var br = (Branch)b;
                float cost = TreeState.PointCost(s.Earned[b], Plugin.XpBase.Value, Plugin.XpGrowth.Value);
                branchInfo[b].text = $"XP {s.Xp[b]:0}/{cost:0}  |  {s.BranchAvailable(br)} pts  |  {s.Spent(br)} spent";
            }

            foreach (var n in TreeDef.Nodes)
            {
                int rank = s.Rank(n.Id);
                var label = labels[n.Id];
                label.text = n.IsMastery ? $"{n.Name}  {rank}/{n.MaxRank}" : $"{n.Name}" + (rank > 0 ? "  (learned)" : "");
                label.color = rank >= n.MaxRank ? GUIManager.Instance.ValheimYellow
                    : s.CanSpend(n.Id) == null ? Color.white
                    : new Color(0.55f, 0.55f, 0.55f);
                if (n.Id == selected) label.text = "> " + label.text + " <";
            }

            if (selected == null)
            {
                details.text = "Select a node. Points earned by a branch's skills are spent in that branch; free points go anywhere.";
                learn.interactable = false;
                return;
            }
            var node = TreeDef.ById[selected];
            string kind = node.IsMastery ? "Mastery" : $"Tier {node.Tier} perk";
            string reason = s.CanSpend(selected);
            details.text = $"{node.Name} ({TreeDef.BranchNames[(int)node.Branch]} {kind}, rank {s.Rank(selected)}/{node.MaxRank})\n" +
                           $"{node.Effect}\n" + (reason ?? "Ready to learn.");
            learn.interactable = reason == null;
        }
    }
}
