using System.Collections;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Extensions;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SkillDrasil
{
    [BepInPlugin(Guid, ModName, Version)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "rgboutl4w.skilldrasil";
        public const string ModName = "SkillDrasil";
        public const string Version = "0.3.1";
        const string ReleasesApi = "https://api.github.com/repos/RGB-Outl4w/SkillDrasil/releases/latest";
        const string ReleasesPage = "https://github.com/RGB-Outl4w/SkillDrasil/releases/latest";

        public static ConfigEntry<float> FirstPointXp, CostExponent, SeveredSeconds, PerkStrength;
        public static ConfigEntry<KeyCode> OpenKey;
        public static ConfigEntry<bool> ShowPointsHint;
        static ConfigEntry<bool> checkForUpdates;
        static string latestRelease;

        public static BepInEx.Logging.ManualLogSource Log;

        void Awake()
        {
            Log = Logger;
            FirstPointXp = Config.BindConfig("Progression", "FirstPointXp", 80f, "Branch XP for the first point. Point n costs FirstPointXp * n^CostExponent.", synced: true);
            CostExponent = Config.BindConfig("Progression", "CostExponent", 1.5f, "How steeply point cost grows. 1 = linear, 1.5 = vanilla-like.", synced: true);
            SeveredSeconds = Config.BindConfig("Balance", "SeveredRootsSeconds", 600f, "Duration of the Severed Roots debuff after death.", synced: true);
            PerkStrength = Config.BindConfig("Balance", "PerkStrength", 1f, "Scales every perk and keystone bonus (drawbacks stay fixed). 0.5 = half as strong, 2 = twice.", synced: true,
                acceptableValues: new AcceptableValueRange<float>(0f, 3f));
            ShowPointsHint = Config.BindConfig("Client", "ShowPointsHint", true, "Show a HUD hint while you have unspent points.", synced: false);
            OpenKey = Config.BindConfig("Client", "OpenTreeKey", KeyCode.K, "Key that opens the skill tree.", synced: false);
            checkForUpdates = Config.BindConfig("Client", "CheckForUpdates", true, "Check GitHub for a newer release on the main menu.", synced: false);

            L.Register();
            Perks.Register();
            CommandManager.Instance.AddConsoleCommand(new PointsCommand());
            new Harmony(Guid).PatchAll();

            if (!GUIManager.IsHeadless() && checkForUpdates.Value)
            {
                GUIManager.OnCustomGUIAvailable += ShowUpdateNotice;
                StartCoroutine(CheckForUpdate());
            }
        }

        void Update() => TreeUI.Tick();

        IEnumerator CheckForUpdate()
        {
            using (var req = UnityWebRequest.Get(ReleasesApi))
            {
                req.SetRequestHeader("User-Agent", ModName);
                req.timeout = 5;
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Log.LogDebug($"Update check failed: {req.error}");
                    yield break;
                }
                var m = Regex.Match(req.downloadHandler.text, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
                if (!m.Success || !VersionUtil.IsNewer(m.Groups[1].Value, Version)) yield break;
                latestRelease = m.Groups[1].Value;
                Log.LogWarning($"{ModName} {latestRelease} is available (installed {Version}): {ReleasesPage}");
                ShowUpdateNotice();
            }
        }

        static void ShowUpdateNotice()
        {
            if (latestRelease == null || !FejdStartup.instance || !GUIManager.CustomGUIFront) return;
            var go = GUIManager.Instance.CreateButton(L.T("update", latestRelease),
                GUIManager.CustomGUIFront.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), 480f, 36f);
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                Application.OpenURL(ReleasesPage);
                Destroy(go);
            });
        }
    }

    class PointsCommand : ConsoleCommand
    {
        public override string Name => "skilldrasil_points";
        public override string Help => "Add free SkillDrasil points: skilldrasil_points <n>";
        public override bool IsCheat => true;

        public override void Run(string[] args)
        {
            var s = Tree.Local;
            if (s == null) { Console.instance.Print("No local player."); return; }
            int n = args.Length > 0 && int.TryParse(args[0], out var v) ? v : 1;
            s.FreeEarned += n;
            Console.instance.Print($"SkillDrasil: +{n} free points ({s.FreeAvailable} available).");
        }
    }
}
