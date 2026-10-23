using System.Collections.Generic;
using Jotunn.Managers;

namespace SkillDrasil
{
    // All player-facing text. English comes from TreeDef plus the table below; other languages override by key.
    // Valheim picks the language, so the mod follows the game's language setting.
    static class L
    {
        static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            ["free"] = "Free points: {0}",
            ["severed_banner"] = "Severed Roots: masteries -1 rank, tier 3 perks and keystone off",
            ["close"] = "Close",
            ["branch_kind"] = "Branch",
            ["branch_help"] = "This branch's skills earn its points.\nBosses and new biomes give free points.",
            ["hover_help"] = "Hover a node for details, click to select.",
            ["kind_mastery"] = "Mastery  ·  {0}",
            ["kind_perk"] = "Tier {0} perk  ·  {1}",
            ["kind_keystone"] = "Keystone  ·  {0}",
            ["hud_points"] = "SkillDrasil: {0} points to spend  [{1}]",
            ["tip_learn"] = "Click, then Learn  ({0} → {1})",
            ["tip_maxed"] = "Fully learned",
            ["other_skills"] = "Other skills",
            ["other_help"] = "Skills added by other mods are not part of the tree. They level up the vanilla way.",
            ["other_level"] = "Level {0}",
            ["rank_of"] = "Rank {0} / {1}",
            ["block_keystone"] = "Only one keystone. Use a Rune of Forgetting to choose again",
            ["mastery_effect"] = "+{0} {1} skill levels per rank.",
            ["level_max"] = "Level {0} (max)",
            ["level_next"] = "Level {0}  →  {1} at rank {2}",
            ["learned"] = "Learned",
            ["learned_inactive"] = "Learned (inactive: Severed Roots)",
            ["not_learned"] = "Not learned",
            ["ready"] = "Ready to learn",
            ["click_select"] = "click to select",
            ["learn_branch"] = "Learn ({0} point)",
            ["learn_free"] = "Learn (free point)",
            ["branch_line"] = "{0}:  {1} unspent  ·  {2} spent",
            ["xp"] = "{0} / {1} XP to next point",
            ["block_maxed"] = "Maxed",
            ["block_need"] = "Needs {0} points spent in {1}",
            ["block_none"] = "No points available",
            ["msg_point"] = "{0} point earned ({1} unspent)",
            ["msg_milestone"] = "Milestone reached! Free points +{0}",
            ["msg_severed"] = "Your roots are severed: branch XP lost",
            ["msg_wither"] = "Your tree withers: all branch points lost",
            ["msg_migrated"] = "SkillDrasil: {0} points granted from your old skills. Press {1}.",
            ["msg_respec"] = "Your roots are unbound. All points refunded.",
            ["se_severed"] = "Severed Roots",
            ["se_severed_tip"] = "Masteries count one rank lower. Tier 3 perks and your keystone are inactive.",
            ["item_rune"] = "Rune of Forgetting",
            ["item_rune_desc"] = "Consume to refund every point spent in your SkillDrasil tree.",
            ["update"] = "SkillDrasil {0} available - click to open",
        };

        static readonly Dictionary<string, string> Russian = new Dictionary<string, string>
        {
            ["free"] = "Свободные очки: {0}",
            ["severed_banner"] = "Подрубленные корни: мастерство −1 ранг, перки 3-го уровня и камень отключены",
            ["close"] = "Закрыть",
            ["branch_kind"] = "Ветвь",
            ["branch_help"] = "Навыки ветви приносят её очки.\nБоссы и новые биомы дают свободные очки.",
            ["hover_help"] = "Наведите на узел — подробности, нажмите — выбор.",
            ["kind_mastery"] = "Мастерство  ·  {0}",
            ["kind_perk"] = "Перк {0}-го уровня  ·  {1}",
            ["kind_keystone"] = "Краеугольный камень  ·  {0}",
            ["hud_points"] = "SkillDrasil: очков к распределению: {0}  [{1}]",
            ["tip_learn"] = "Нажмите, затем «Изучить»  ({0} → {1})",
            ["tip_maxed"] = "Изучено полностью",
            ["other_skills"] = "Другие навыки",
            ["other_help"] = "Навыки из других модов не входят в древо и растут по-обычному.",
            ["other_level"] = "Уровень {0}",
            ["rank_of"] = "Ранг {0} / {1}",
            ["block_keystone"] = "Камень может быть только один. Руна забвения позволит выбрать заново",
            ["mastery_effect"] = "+{0} к уровню навыка «{1}» за ранг.",
            ["level_max"] = "Уровень {0} (макс.)",
            ["level_next"] = "Уровень {0}  →  {1} на ранге {2}",
            ["learned"] = "Изучено",
            ["learned_inactive"] = "Изучено (неактивно: подрубленные корни)",
            ["not_learned"] = "Не изучено",
            ["ready"] = "Можно изучить",
            ["click_select"] = "нажмите, чтобы выбрать",
            ["learn_branch"] = "Изучить (очко ветви «{0}»)",
            ["learn_free"] = "Изучить (свободное очко)",
            ["branch_line"] = "{0}:  свободно {1}  ·  потрачено {2}",
            ["xp"] = "{0} / {1} опыта до следующего очка",
            ["block_maxed"] = "Максимум",
            ["block_need"] = "Нужно потратить в ветви «{1}» очков: {0}",
            ["block_none"] = "Нет доступных очков",
            ["msg_point"] = "Получено очко ветви «{0}» (свободно: {1})",
            ["msg_milestone"] = "Веха пройдена! Свободные очки +{0}",
            ["msg_severed"] = "Ваши корни подрублены: часть опыта ветвей потеряна",
            ["msg_wither"] = "Ваше древо увядает: все очки ветвей потеряны",
            ["msg_migrated"] = "SkillDrasil: за прежние навыки начислено очков: {0}. Нажмите {1}.",
            ["msg_respec"] = "Ваши корни освобождены. Все очки возвращены.",
            ["se_severed"] = "Подрубленные корни",
            ["se_severed_tip"] = "Мастерство считается на один ранг ниже. Перки 3-го уровня и краеугольный камень неактивны.",
            ["item_rune"] = "Руна забвения",
            ["item_rune_desc"] = "Используйте, чтобы вернуть все очки, потраченные в древе SkillDrasil.",
            ["update"] = "Доступна SkillDrasil {0} — нажмите, чтобы открыть",

            ["branch_0"] = "Воин", ["branch_1"] = "Охотник", ["branch_2"] = "Сейд", ["branch_3"] = "Странник", ["branch_4"] = "Ремесленник",
            ["branchlore_0"] = "Сталь, щит и воля выстоять.",
            ["branchlore_1"] = "Терпение, тишина и точный удар.",
            ["branchlore_2"] = "Колдовство эйтра, искусство вёльвы.",
            ["branchlore_3"] = "Ветер за спиной, небо под ногами.",
            ["branchlore_4"] = "Руки, что создают мир.",

            ["name_shieldwall"] = "Стена щитов",
            ["effect_shieldwall"] = "Блок расходует на 25% меньше выносливости.",
            ["lore_shieldwall"] = "Сомкнуть щиты. Держать строй.",
            ["name_berserkgang"] = "Берсеркганг",
            ["effect_berserkgang"] = "При здоровье ниже 30%: +20% к урону в ближнем бою.",
            ["lore_berserkgang"] = "Ярость медвежьей шкуры просыпается, когда течёт кровь.",
            ["name_riposte"] = "Ответный удар",
            ["effect_riposte"] = "Парирование на 30% сильнее.",
            ["lore_riposte"] = "Обрати их силу против них.",

            ["name_silentdeath"] = "Тихая смерть",
            ["effect_silentdeath"] = "Скрытные атаки наносят на 50% больше бонусного урона.",
            ["lore_silentdeath"] = "Бей, прежде чем тебя заметят.",
            ["name_steadyaim"] = "Твёрдая рука",
            ["effect_steadyaim"] = "+15% к урону луков и арбалетов, пока вы пригнулись.",
            ["lore_steadyaim"] = "Вдох. Колено. Выстрел.",
            ["name_fletcher"] = "Бережливый стрелок",
            ["effect_fletcher"] = "Шанс 25% не потратить боеприпас.",
            ["lore_fletcher"] = "Хороший охотник ничего не тратит зря.",

            ["name_eitrwell"] = "Источник эйтра",
            ["effect_eitrwell"] = "+25% к восстановлению эйтра.",
            ["lore_eitrwell"] = "Пей из колодца под корнями.",
            ["name_focusedmind"] = "Ясный разум",
            ["effect_focusedmind"] = "Все затраты эйтра на 20% меньше.",
            ["lore_focusedmind"] = "Спокойный разум тратит меньше себя.",
            ["name_arcanesurge"] = "Всплеск сейда",
            ["effect_arcanesurge"] = "+15% к урону магией стихий и крови.",
            ["lore_arcanesurge"] = "Пусть эйтр горит ярко.",

            ["name_fleetfoot"] = "Быстроногий",
            ["effect_fleetfoot"] = "Бег расходует на 20% меньше выносливости.",
            ["lore_fleetfoot"] = "Дорога длинна. Твой шаг длиннее.",
            ["name_secondwind"] = "Второе дыхание",
            ["effect_secondwind"] = "Уклонение расходует на 25% меньше выносливости.",
            ["lore_secondwind"] = "Ещё один вдох. Ещё один шаг.",
            ["name_featherfall"] = "Лёгкое падение",
            ["effect_featherfall"] = "Урон от падения на 50% меньше.",
            ["lore_featherfall"] = "Опустись, как лист с кроны Иггдрасиля.",

            ["name_deeppockets"] = "Бездонные карманы",
            ["effect_deeppockets"] = "+50 к переносимому весу.",
            ["lore_deeppockets"] = "Неси мир на своих плечах.",
            ["name_buildersease"] = "Рука мастера",
            ["effect_buildersease"] = "Строительство и ремонт расходуют на 30% меньше выносливости.",
            ["lore_buildersease"] = "Молот помнит работу.",
            ["name_hearthkeeper"] = "Хранитель очага",
            ["effect_hearthkeeper"] = "Еда действует на 25% дольше.",
            ["lore_hearthkeeper"] = "Сытый желудок переживёт долгую ночь.",

            ["name_ironhide"] = "Железная шкура",
            ["effect_ironhide"] = "+3 к броне за ранг.",
            ["lore_ironhide"] = "Шрамы — вторая кольчуга.",
            ["name_battlefervor"] = "Боевой пыл",
            ["effect_battlefervor"] = "Атаки ближнего боя расходуют на 10% меньше выносливости за ранг.",
            ["lore_battlefervor"] = "Битва питает сама себя.",
            ["name_unshaken"] = "Непоколебимый",
            ["effect_unshaken"] = "Вы получаете на 40% меньше ошеломления.",
            ["lore_unshaken"] = "Корни глубоки. Дуб не падает.",
            ["name_bearshirt"] = "Медвежья рубаха",
            ["effect_bearshirt"] = "+30% к урону в ближнем бою.\nНедостаток: броня на 40% ниже.",
            ["lore_bearshirt"] = "Сбрось кольчугу. Стань медведем.",

            ["name_shadowstep"] = "Шаг тени",
            ["effect_shadowstep"] = "Скрытное передвижение расходует на 20% меньше выносливости за ранг.",
            ["lore_shadowstep"] = "Двигайся, как движется туман.",
            ["name_keeneye"] = "Зоркий глаз",
            ["effect_keeneye"] = "+5% к урону луков и арбалетов за ранг.",
            ["lore_keeneye"] = "Глаз ястреба ничего не упускает.",
            ["name_ghost"] = "Призрак",
            ["effect_ghost"] = "При скрытном передвижении вас на 50% труднее заметить.",
            ["lore_ghost"] = "Был здесь — и исчез.",
            ["name_lonewolf"] = "Одинокий волк",
            ["effect_lonewolf"] = "+30% к урону луков, арбалетов и ножей.\nНедостаток: остальное оружие ближнего боя наносит на 30% меньше.",
            ["lore_lonewolf"] = "Ни братьев по щиту. Ни пощады.",

            ["name_vigor"] = "Сила вёльвы",
            ["effect_vigor"] = "+15% к восстановлению здоровья за ранг.",
            ["lore_vigor"] = "Провидица переживёт воина.",
            ["name_galdr"] = "Гальдр",
            ["effect_galdr"] = "+5% к урону магией стихий и крови за ранг.",
            ["lore_galdr"] = "Пой руны — и они ответят.",
            ["name_runeward"] = "Рунный оберег",
            ["effect_runeward"] = "Сопротивление огню, холоду и молнии.",
            ["lore_runeward"] = "Вырезанные на коже, руны сдерживают бурю.",
            ["name_volvasoath"] = "Клятва вёльвы",
            ["effect_volvasoath"] = "+30% к урону магией, +25% к восстановлению эйтра.\nНедостаток: оружие наносит на 30% меньше урона.",
            ["lore_volvasoath"] = "Отложи клинок. Возьми посох навсегда.",

            ["name_sealborn"] = "Рождённый тюленем",
            ["effect_sealborn"] = "Плавание расходует на 20% меньше выносливости за ранг.",
            ["lore_sealborn"] = "Фьорд — всего лишь ещё одна дорога.",
            ["name_springstep"] = "Пружинистый шаг",
            ["effect_springstep"] = "Прыжки расходуют на 10% меньше выносливости за ранг.",
            ["lore_springstep"] = "Перепрыгни ручей. Перепрыгни стену.",
            ["name_windrunner"] = "Бегущий с ветром",
            ["effect_windrunner"] = "+8% к скорости передвижения.",
            ["lore_windrunner"] = "Обгони бурю.",
            ["name_endlessroad"] = "Бесконечный путь",
            ["effect_endlessroad"] = "+40% к восстановлению выносливости.\nНедостаток: −100 к переносимому весу.",
            ["lore_endlessroad"] = "Путешествуй налегке. Путешествуй вечно.",

            ["name_quickstudy"] = "Быстрый ученик",
            ["effect_quickstudy"] = "+5% к опыту всех ветвей за ранг.",
            ["lore_quickstudy"] = "Каждое дело учит того, кто готов учиться.",
            ["name_minersgrit"] = "Хватка рудокопа",
            ["effect_minersgrit"] = "Удары киркой расходуют на 15% меньше выносливости за ранг.",
            ["lore_minersgrit"] = "Гора уступает терпеливым.",
            ["name_hardworker"] = "Трудяга",
            ["effect_hardworker"] = "+15% к восстановлению выносливости.",
            ["lore_hardworker"] = "Отдыхать будем в Вальхалле.",
            ["name_homesteader"] = "Хозяин усадьбы",
            ["effect_homesteader"] = "+100 к переносимому весу, +25% к опыту всех ветвей.\nНедостаток: вы получаете на 25% больше урона.",
            ["lore_homesteader"] = "Построй чертог. Пусть другие держат стену щитов.",
        };

        public static void Register()
        {
            for (int b = 0; b < TreeDef.BranchNames.Length; b++)
            {
                English["branch_" + b] = TreeDef.BranchNames[b];
                English["branchlore_" + b] = TreeDef.BranchLore[b];
            }
            foreach (var n in TreeDef.Nodes)
            {
                if (n.IsMastery) continue; // mastery names come from the game's own skill names
                English["name_" + n.Id] = n.Name;
                English["effect_" + n.Id] = n.Effect;
                English["lore_" + n.Id] = n.Lore;
            }
            var loc = LocalizationManager.Instance.GetLocalization();
            loc.AddTranslation("English", Prefixed(English));
            loc.AddTranslation("Russian", Prefixed(Russian));
        }

        static Dictionary<string, string> Prefixed(Dictionary<string, string> d)
        {
            var r = new Dictionary<string, string>();
            foreach (var kv in d) r["sd_" + kv.Key] = kv.Value;
            return r;
        }

        public static string Language => Localization.instance?.GetSelectedLanguage() ?? "English";

        public static string T(string key, params object[] args)
        {
            string s = Localization.instance?.Localize("$sd_" + key);
            if (string.IsNullOrEmpty(s) || s[0] == '[') English.TryGetValue(key, out s);
            s = s ?? key;
            return args.Length > 0 ? string.Format(s, args) : s;
        }

        public static string Branch(Branch b) => T("branch_" + (int)b);
        public static string BranchLore(Branch b) => T("branchlore_" + (int)b);

        public static string Name(Node n)
        {
            if (!n.IsMastery) return T("name_" + n.Id);
            string s = Localization.instance?.Localize("$skill_" + ((Skills.SkillType)n.Skill).ToString().ToLowerInvariant());
            return string.IsNullOrEmpty(s) || s[0] == '[' ? n.Name : s;
        }

        public static string Effect(Node n) => n.IsMastery ? T("mastery_effect", TreeDef.LevelsPerRank, Name(n)) : T("effect_" + n.Id);
        public static string Lore(Node n) => T("lore_" + n.Id);

        public static string Reason(Block block, int need, Branch b) =>
            block == Block.Maxed ? T("block_maxed")
            : block == Block.NeedSpent ? T("block_need", need, Branch(b))
            : block == Block.KeystoneTaken ? T("block_keystone")
            : block == Block.NoPoints ? T("block_none")
            : T("ready");
    }
}
