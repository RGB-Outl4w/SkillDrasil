# Changelog

## 0.3.2
- **Active abilities**, one per branch, unlocked by a tier 1 perk of that branch:
  War Cry (you and allies: +20% damage, 15 s), Hunter's Mark (enemies take +25% damage from everyone, 15 s),
  Völva's Chant (30 eitr: you and allies heal 40% over 10 s), Gale Step (+50% stamina, +20% speed, 8 s),
  Tireless Toil (no stamina for chopping, mining, building, repairing, 30 s).
  Cast with Alt + 1–5 (configurable); while Alt is held, number keys don't use the hotbar. Cooldowns show bottom right.
- Faster progression: point n now costs `60 × n^1.25` XP (was `80 × n^1.5`): about 5,300 XP for 10 points and 24,000 for 20 (was 10,000 and 57,000). Config files still on the old defaults are updated automatically; custom values are kept.
- Safety: branch XP ignores NaN/infinite amounts and a zero or negative point cost, so a bad config can never hang the game.

## 0.3.1
- Compatibility: skills added by other mods are no longer frozen at 0. They keep vanilla levelling, death loss and effects, and are listed under the new **Other skills** button.
- `Balance.PerkStrength` (server-synced, 0–3, default 1) scales every perk and keystone bonus; drawbacks stay fixed.
- Cursor tooltip on tree nodes: name, rank and what's needed to learn it.
- HUD hint while you have unspent points (`Client.ShowPointsHint`).
- Learning a node plays the level-up flash and sound.
- Redrawn icons: Polearms, Fists, Wood Cutting, Ghost, Windrunner.

## 0.3.0
- Deeper trees: every branch now has two perks per tier (15 new perks) and a **keystone** at 15 points spent.
- Keystones: Bear Shirt, Lone Wolf, Völva's Oath, Endless Road, Homesteader. Big bonus, real drawback, only one in the whole tree. The Rune of Forgetting frees the choice.
- Multi-rank perks: Iron Hide, Battle Fervor, Shadowstep, Keen Eye, Völva's Vigor, Galdr, Seal-Born, Spring Step, Quick Study, Miner's Grit (2–3 ranks).
- Severed Roots now also silences your keystone.
- New icons for every new node; English and Russian text.
- Saves from 0.2 load unchanged (new nodes start unlearned). Multiplayer: everyone needs 0.3.x.

## 0.2.0
- First Thunderstore release.
- MIT license.
- Package icon; manifest links to the website.
- Not compatible in multiplayer with 0.1.x: everyone needs 0.2.x.

## 0.1.1
- Tree-shaped UI: one tab per branch; masteries are roots, the trunk fills as you spend, perks form the crown (3 / 6 / 9 points).
- Custom icons for every mastery, perk, branch, the Rune of Forgetting and Severed Roots.
- Detail card: effect, lore, current level, requirement, Learn button, branch XP bar.
- Full Russian translation, following Valheim's language setting.
- Slower progression: point n costs `80 × n^1.5` branch XP (`Progression.FirstPointXp`, `Progression.CostExponent`).
- Fixed: wrong "points granted from your old skills" message on new characters; cut-off card text.

## 0.1.0
- First build: five branches covering all 24 vanilla skills, 15 perks, milestone free points, Severed Roots on death, Rune of Forgetting respec, save-safe install and uninstall, Jotunn-enforced multiplayer.
