# SkillDrasil

A Valheim mod that replaces the vanilla skill system with a skill tree.

**Website:** https://rgb-outl4w.github.io/SkillDrasil/ (English / Русский)

Vanilla: repeat an action → that skill levels up; die → lose levels.
SkillDrasil: actions still earn XP, but into five **branches**. Full branch XP = one point you **choose** where to spend.

## How it works

| Branch | Masteries (vanilla skills) | Perks (tier 1 / 2 / 3) |
|---|---|---|
| Warrior | Swords, Axes, Clubs, Polearms, Spears, Unarmed, Blocking | Shield-Wall, Berserkgang, Riposte |
| Hunter | Bows, Crossbows, Knives, Sneak | Silent Death, Steady Aim, Fletcher's Luck |
| Seiðr | Elemental Magic, Blood Magic | Eitr Well, Focused Mind, Arcane Surge |
| Wanderer | Run, Jump, Swim, Dodge, Ride | Fleetfoot, Second Wind, Featherfall |
| Artisan | Pickaxes, Wood Cutting, Fishing, Cooking, Farming, Crafting | Deep Pockets, Builder's Ease, Hearthkeeper |

- **Masteries** have 5 ranks. Each rank = 20 levels of that vanilla skill, so all vanilla skill effects still apply. Gear and mead skill bonuses still stack on top.
- **Perks** add new effects. Tier N needs 3×N points already spent in the branch. Mastery rank R needs 2×(R−1).
- **Branch points** come from using that branch's skills and are spent in that branch.
- **Free points** (spend anywhere) come from milestones: each boss defeated in the world, each new biome discovered.
- **Death** (when vanilla would drain skills): lose part of every branch's in-progress XP. Spent points are never lost. You respawn with **Severed Roots** for 10 minutes: masteries count one rank lower and tier 3 perks are off. The "Death skills reset" world modifier wipes branch points instead (free points kept).
- **Respec**: craft a **Rune of Forgetting** at the workbench (10 Stone, 10 Resin, 5 Greydwarf Eye) and consume it.
- Open the tree with **K** (configurable) or the inventory's Skills button.

## Existing characters and uninstalling

- The mod never writes vanilla skill levels. Tree data lives in its own key inside the character file.
- **First load** of an existing character: every full 20 vanilla levels in a skill becomes 1 point in that skill's branch.
- **Uninstall**: your vanilla skill levels are exactly what they were when you installed. Reinstall: your tree is back.
- Exception: Runes of Forgetting in your inventory disappear if you load the character without the mod.

Back up your characters before trying any mod: `%USERPROFILE%\AppData\LocalLow\IronGate\Valheim\characters_local` (local saves) or `Steam\userdata\<id>\892970\remote\characters` (Steam Cloud saves).

## Multiplayer

Every player **and** the server must run SkillDrasil (same major.minor version). Jotunn rejects mismatched connections.
Balance settings (`Balance` section) are set by the server and synced to clients.

## Install

Requires [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) and [Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).
Put `SkillDrasil.dll` in `BepInEx/plugins/SkillDrasil/`.

## Config (`BepInEx/config/rgboutl4w.skilldrasil.cfg`)

| Key | Default | |
|---|---|---|
| Progression.FirstPointXp | 80 | Branch XP for the first point; point n costs `FirstPointXp × n^CostExponent` (server-synced) |
| Progression.CostExponent | 1.5 | 1 = linear, 1.5 = vanilla-like. About 57k XP for 20 points in one branch (server-synced) |
| Balance.SeveredRootsSeconds | 600 | Debuff duration (server-synced) |
| Client.OpenTreeKey | K | |
| Client.CheckForUpdates | true | On the main menu, ask GitHub for the latest release. Only a plain GET to `api.github.com`, nothing sent. |

Console (with `devcommands`): `skilldrasil_points <n>` adds free points.

## Languages

English and Russian. The mod follows Valheim's language setting.

## Build

1. Copy `Environment.props.example` to `Environment.props` and set your Valheim path.
2. `dotnet build -c Release` (copies the dll into your BepInEx plugins folder).
3. `dotnet run --project tests/SelfCheck` runs the core logic checks.
4. Icons: edit `tools/icons.py`, then `python tools/icons.py` (also refreshes the website's `docs/icons`) and `tools/render-icons.ps1` (needs Edge) to rebuild `assets/atlas.png`.
5. Website: static files in `docs/`, served by GitHub Pages. Preview with `python -m http.server 8765 --directory docs`.
