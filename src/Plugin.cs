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
        public const string Version = "0.1.0";
        const string ReleasesApi = "https://api.github.com/repos/RGB-Outl4w/SkillDrasil/releases/latest";
        const string ReleasesPage = "https://github.com/RGB-Outl4w/SkillDrasil/releases/latest";

        public static ConfigEntry<float> XpBase, XpGrowth, SeveredSeconds;
        public static ConfigEntry<KeyCode> OpenKey;
        static ConfigEntry<bool> checkForUpdates;
        static string latestRelease;

        public static BepInEx.Logging.ManualLogSource Log;

        void Awake()
        {
            Log = Logger;
            XpBase = Config.BindConfig("Balance", "XpPerPointBase", 60f, "Branch XP needed for the first point.", synced: true);
            XpGrowth = Config.BindConfig("Balance", "XpPerPointGrowth", 0.25f, "Each earned point makes the next one cost this much more (fraction of base).", synced: true);
            SeveredSeconds = Config.BindConfig("Balance", "SeveredRootsSeconds", 600f, "Duration of the Severed Roots debuff after death.", synced: true);
            OpenKey = Config.BindConfig("Client", "OpenTreeKey", KeyCode.K, "Key that opens the skill tree.", synced: false);
            checkForUpdates = Config.BindConfig("Client", "CheckForUpdates", true, "Check GitHub for a newer release on the main menu.", synced: false);

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
            var go = GUIManager.Instance.CreateButton($"{ModName} {latestRelease} available - click to open",
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
