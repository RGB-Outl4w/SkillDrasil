// Interactive branch explorer for the SkillDrasil site. Mirrors the in-game panel (src/TreeUI.cs).
// Data mirrors src/Core.cs (English) and src/Lang.cs (Russian); mastery names are Valheim's own.
(function () {
  const ROOT = document.currentScript.src.replace(/tree\.js.*$/, '');
  const LANG = document.documentElement.lang === 'ru' ? 'ru' : 'en';
  const t = (o) => o[LANG];

  const UI = {
    branch: { en: 'Branch', ru: 'Ветвь' },
    mastery: { en: 'Mastery', ru: 'Мастерство' },
    perk: { en: (n) => `Tier ${n} perk`, ru: (n) => `Перк ${n}-го уровня` },
    keystone: { en: 'Keystone', ru: 'Краеугольный камень' },
    ability: { en: 'Active ability', ru: 'Активная способность' },
    abilityRule: { en: 'Needs a tier 1 perk of this branch. Cast with Alt + the branch number (1–5).', ru: 'Нужен перк 1-го уровня этой ветви. Применение: Alt + номер ветви (1–5).' },
    ranks: { en: (n) => `${n} ranks.`, ru: (n) => `Рангов: ${n}.` },
    keystoneRule: { en: 'Unlocks after 15 points spent in this branch. Only one keystone in the whole tree.', ru: 'Открывается после 15 очков в ветви. Камень может быть только один на всё древо.' },
    masteryEffect: { en: (s) => `+20 ${s} skill levels per rank (5 ranks, up to level 100).`, ru: (s) => `+20 к уровню навыка «${s}» за ранг (5 рангов, до уровня 100).` },
    unlock: { en: (n) => `Unlocks after ${n} points spent in this branch.`, ru: (n) => `Открывается после ${n} очков, потраченных в этой ветви.` },
    rankGate: { en: 'Rank R needs 2×(R−1) points already spent in this branch.', ru: 'Ранг R требует 2×(R−1) уже потраченных в ветви очков.' },
    help: { en: 'Hover or tap a node to inspect it.', ru: 'Наведите или нажмите на узел, чтобы изучить его.' },
  };

  const M = (id, en, ru) => ({ id, name: { en, ru } });
  const A = (id, en, ru, effEn, effRu, loreEn, loreRu) =>
    ({ id, ability: true, name: { en, ru }, effect: { en: effEn, ru: effRu }, lore: { en: loreEn, ru: loreRu } });
  const P = (id, tier, en, ru, effEn, effRu, loreEn, loreRu, ranks = 1) =>
    ({ id, tier, ranks, name: { en, ru }, effect: { en: effEn, ru: effRu }, lore: { en: loreEn, ru: loreRu } });

  const BRANCHES = [
    {
      ability: A('warcry', 'War Cry', 'Боевой клич', 'You and allies within 15 m deal 20% more damage for 15 s. Cooldown 2 min.', 'Вы и союзники в радиусе 15 м наносите на 20% больше урона 15 с. Перезарядка 2 мин.', 'One voice, and the line holds.', 'Один голос — и строй устоит.'),
      id: 'warrior', name: { en: 'Warrior', ru: 'Воин' },
      lore: { en: 'Steel, shield and the will to stand.', ru: 'Сталь, щит и воля выстоять.' },
      masteries: [M('swords', 'Swords', 'Мечи'), M('axes', 'Axes', 'Топоры'), M('clubs', 'Clubs', 'Дубины'),
        M('polearms', 'Polearms', 'Древковое оружие'), M('spears', 'Spears', 'Копья'), M('unarmed', 'Fists', 'Без оружия'),
        M('blocking', 'Blocking', 'Блокирование')],
      perks: [
        P('shieldwall', 1, 'Shield-Wall', 'Стена щитов', 'Blocking costs 25% less stamina.', 'Блок расходует на 25% меньше выносливости.', 'Lock shields. Hold the line.', 'Сомкнуть щиты. Держать строй.'),
        P('berserkgang', 2, 'Berserkgang', 'Берсеркганг', 'Below 30% health: +20% melee damage.', 'При здоровье ниже 30%: +20% к урону в ближнем бою.', "The bear-shirt's fury wakes when blood runs low.", 'Ярость медвежьей шкуры просыпается, когда течёт кровь.'),
        P('riposte', 3, 'Riposte', 'Ответный удар', 'Parries are 30% stronger.', 'Парирование на 30% сильнее.', 'Turn their strength against them.', 'Обрати их силу против них.'),
        P('ironhide', 1, 'Iron Hide', 'Железная шкура', '+3 armor per rank.', '+3 к броне за ранг.', 'Scars are a second coat of mail.', 'Шрамы — вторая кольчуга.', 3),
        P('battlefervor', 2, 'Battle Fervor', 'Боевой пыл', 'Melee attacks cost 10% less stamina per rank.', 'Атаки ближнего боя расходуют на 10% меньше выносливости за ранг.', 'The fight feeds itself.', 'Битва питает сама себя.', 2),
        P('unshaken', 3, 'Unshaken', 'Непоколебимый', 'Take 40% less stagger.', 'Вы получаете на 40% меньше ошеломления.', 'Roots run deep. The oak does not fall.', 'Корни глубоки. Дуб не падает.'),
        P('bearshirt', 4, 'Bear Shirt', 'Медвежья рубаха', '+30% melee damage. Drawback: 40% less armor.', '+30% к урону в ближнем бою. Недостаток: броня на 40% ниже.', 'Shed the mail. Become the bear.', 'Сбрось кольчугу. Стань медведем.'),
      ],
    },
    {
      ability: A('huntersmark', "Hunter's Mark", 'Метка охотника', 'Enemies within 25 m take 25% more damage from everyone for 15 s. Cooldown 90 s.', 'Враги в радиусе 25 м получают на 25% больше урона от всех 15 с. Перезарядка 90 с.', 'Point, and the pack follows.', 'Укажи — и стая последует.'),
      id: 'hunter', name: { en: 'Hunter', ru: 'Охотник' },
      lore: { en: 'Patience, silence and the clean kill.', ru: 'Терпение, тишина и точный удар.' },
      masteries: [M('bows', 'Bows', 'Луки'), M('crossbows', 'Crossbows', 'Арбалеты'), M('knives', 'Knives', 'Ножи'), M('sneak', 'Sneak', 'Скрытность')],
      perks: [
        P('silentdeath', 1, 'Silent Death', 'Тихая смерть', 'Sneak attacks deal 50% more bonus damage.', 'Скрытные атаки наносят на 50% больше бонусного урона.', 'Strike before they know you are there.', 'Бей, прежде чем тебя заметят.'),
        P('steadyaim', 2, 'Steady Aim', 'Твёрдая рука', '+15% bow and crossbow damage while crouched.', '+15% к урону луков и арбалетов, пока вы пригнулись.', 'Breathe. Kneel. Loose.', 'Вдох. Колено. Выстрел.'),
        P('fletcher', 3, "Fletcher's Luck", 'Бережливый стрелок', '25% chance to not use up ammo.', 'Шанс 25% не потратить боеприпас.', 'A good hunter wastes nothing.', 'Хороший охотник ничего не тратит зря.'),
        P('shadowstep', 1, 'Shadowstep', 'Шаг тени', 'Sneaking costs 20% less stamina per rank.', 'Скрытное передвижение расходует на 20% меньше выносливости за ранг.', 'Move as the mist moves.', 'Двигайся, как движется туман.', 2),
        P('keeneye', 2, 'Keen Eye', 'Зоркий глаз', '+5% bow and crossbow damage per rank.', '+5% к урону луков и арбалетов за ранг.', "The hawk's eye misses nothing.", 'Глаз ястреба ничего не упускает.', 3),
        P('ghost', 3, 'Ghost', 'Призрак', '50% harder to notice while sneaking.', 'При скрытном передвижении вас на 50% труднее заметить.', 'Here, then gone.', 'Был здесь — и исчез.'),
        P('lonewolf', 4, 'Lone Wolf', 'Одинокий волк', '+30% bow, crossbow and knife damage. Drawback: other melee weapons deal 30% less.', '+30% к урону луков, арбалетов и ножей. Недостаток: остальное оружие ближнего боя наносит на 30% меньше.', 'No shield-brothers. No mercy.', 'Ни братьев по щиту. Ни пощады.'),
      ],
    },
    {
      ability: A('volvaschant', "Völva's Chant", 'Песнь вёльвы', 'Costs 30 eitr. You and allies within 15 m regain 40% health over 10 s. Cooldown 2 min.', 'Стоит 30 эйтра. Вы и союзники в радиусе 15 м восстанавливаете 40% здоровья за 10 с. Перезарядка 2 мин.', 'The old songs mend what steel has torn.', 'Старые песни лечат то, что порвала сталь.'),
      id: 'seidr', name: { en: 'Seiðr', ru: 'Сейд' },
      lore: { en: 'The eitr-craft of the völva.', ru: 'Колдовство эйтра, искусство вёльвы.' },
      masteries: [M('elementalmagic', 'Elemental Magic', 'Магия стихий'), M('bloodmagic', 'Blood Magic', 'Магия крови')],
      perks: [
        P('eitrwell', 1, 'Eitr Well', 'Источник эйтра', '+25% eitr regeneration.', '+25% к восстановлению эйтра.', 'Drink deep from the well beneath the roots.', 'Пей из колодца под корнями.'),
        P('focusedmind', 2, 'Focused Mind', 'Ясный разум', 'All eitr costs 20% less.', 'Все затраты эйтра на 20% меньше.', 'A quiet mind spends less of itself.', 'Спокойный разум тратит меньше себя.'),
        P('arcanesurge', 3, 'Arcane Surge', 'Всплеск сейда', '+15% elemental and blood magic damage.', '+15% к урону магией стихий и крови.', 'Let the eitr burn bright.', 'Пусть эйтр горит ярко.'),
        P('vigor', 1, "Völva's Vigor", 'Сила вёльвы', '+15% health regeneration per rank.', '+15% к восстановлению здоровья за ранг.', 'The seeress outlives the warrior.', 'Провидица переживёт воина.', 2),
        P('galdr', 2, 'Galdr', 'Гальдр', '+5% elemental and blood magic damage per rank.', '+5% к урону магией стихий и крови за ранг.', 'Sing the runes and they answer.', 'Пой руны — и они ответят.', 3),
        P('runeward', 3, 'Rune Ward', 'Рунный оберег', 'Resistant to fire, frost and lightning.', 'Сопротивление огню, холоду и молнии.', 'Carved on the skin, the runes hold back the storm.', 'Вырезанные на коже, руны сдерживают бурю.'),
        P('volvasoath', 4, "Völva's Oath", 'Клятва вёльвы', '+30% magic damage, +25% eitr regeneration. Drawback: weapons deal 30% less.', '+30% к урону магией, +25% к восстановлению эйтра. Недостаток: оружие наносит на 30% меньше урона.', 'Lay down the blade. Take up the staff for good.', 'Отложи клинок. Возьми посох навсегда.'),
      ],
    },
    {
      ability: A('galestep', 'Gale Step', 'Шаг бури', 'Restore 50% stamina and move 20% faster for 8 s. Cooldown 60 s.', 'Восстанавливает 50% выносливости, +20% к скорости на 8 с. Перезарядка 60 с.', "Borrow the storm's legs.", 'Одолжи у бури её ноги.'),
      id: 'wanderer', name: { en: 'Wanderer', ru: 'Странник' },
      lore: { en: 'Wind at your heels, sky beneath your feet.', ru: 'Ветер за спиной, небо под ногами.' },
      masteries: [M('run', 'Run', 'Бег'), M('jump', 'Jump', 'Прыжок'), M('swim', 'Swim', 'Плавание'), M('dodge', 'Dodge', 'Уклонение'), M('ride', 'Riding', 'Верховая езда')],
      perks: [
        P('fleetfoot', 1, 'Fleetfoot', 'Быстроногий', 'Running costs 20% less stamina.', 'Бег расходует на 20% меньше выносливости.', 'The road is long. Your stride is longer.', 'Дорога длинна. Твой шаг длиннее.'),
        P('secondwind', 2, 'Second Wind', 'Второе дыхание', 'Dodging costs 25% less stamina.', 'Уклонение расходует на 25% меньше выносливости.', 'One more breath. One more step.', 'Ещё один вдох. Ещё один шаг.'),
        P('featherfall', 3, 'Featherfall', 'Лёгкое падение', 'Take 50% less fall damage.', 'Урон от падения на 50% меньше.', "Drift down like a leaf from Yggdrasil's crown.", 'Опустись, как лист с кроны Иггдрасиля.'),
        P('sealborn', 1, 'Seal-Born', 'Рождённый тюленем', 'Swimming costs 20% less stamina per rank.', 'Плавание расходует на 20% меньше выносливости за ранг.', 'The fjord is just another road.', 'Фьорд — всего лишь ещё одна дорога.', 2),
        P('springstep', 2, 'Spring Step', 'Пружинистый шаг', 'Jumping costs 10% less stamina per rank.', 'Прыжки расходуют на 10% меньше выносливости за ранг.', 'Leap the stream. Leap the wall.', 'Перепрыгни ручей. Перепрыгни стену.', 3),
        P('windrunner', 3, 'Windrunner', 'Бегущий с ветром', '+8% movement speed.', '+8% к скорости передвижения.', 'Outrun the storm.', 'Обгони бурю.'),
        P('endlessroad', 4, 'Endless Road', 'Бесконечный путь', '+40% stamina regeneration. Drawback: −100 carry weight.', '+40% к восстановлению выносливости. Недостаток: −100 к переносимому весу.', 'Travel light. Travel forever.', 'Путешествуй налегке. Путешествуй вечно.'),
      ],
    },
    {
      ability: A('tirelesstoil', 'Tireless Toil', 'Неустанный труд', 'For 30 s chopping, mining, building and repairing cost no stamina. Cooldown 3 min.', '30 с рубка, добыча, строительство и ремонт не тратят выносливость. Перезарядка 3 мин.', 'Work until the work is done.', 'Работай, пока работа не сделана.'),
      id: 'artisan', name: { en: 'Artisan', ru: 'Ремесленник' },
      lore: { en: 'Hands that shape the world.', ru: 'Руки, что создают мир.' },
      masteries: [M('pickaxes', 'Pickaxes', 'Кирки'), M('woodcutting', 'Wood Cutting', 'Рубка древесины'), M('fishing', 'Fishing', 'Рыбалка'),
        M('cooking', 'Cooking', 'Кулинария'), M('farming', 'Farming', 'Земледелие'), M('crafting', 'Crafting', 'Ремесло')],
      perks: [
        P('deeppockets', 1, 'Deep Pockets', 'Бездонные карманы', '+50 carry weight.', '+50 к переносимому весу.', 'Carry the world on your back.', 'Неси мир на своих плечах.'),
        P('buildersease', 2, "Builder's Ease", 'Рука мастера', 'Building and repairing cost 30% less stamina.', 'Строительство и ремонт расходуют на 30% меньше выносливости.', 'The hammer remembers the work.', 'Молот помнит работу.'),
        P('hearthkeeper', 3, 'Hearthkeeper', 'Хранитель очага', 'Food lasts 25% longer.', 'Еда действует на 25% дольше.', 'A full belly lasts the long night.', 'Сытый желудок переживёт долгую ночь.'),
        P('quickstudy', 1, 'Quick Study', 'Быстрый ученик', '+5% XP for every branch per rank.', '+5% к опыту всех ветвей за ранг.', 'Every task teaches the willing.', 'Каждое дело учит того, кто готов учиться.', 3),
        P('minersgrit', 2, "Miner's Grit", 'Хватка рудокопа', 'Pickaxe swings cost 15% less stamina per rank.', 'Удары киркой расходуют на 15% меньше выносливости за ранг.', 'The mountain yields to the patient.', 'Гора уступает терпеливым.', 2),
        P('hardworker', 3, 'Hard Worker', 'Трудяга', '+15% stamina regeneration.', '+15% к восстановлению выносливости.', 'Rest is for the dead.', 'Отдыхать будем в Вальхалле.'),
        P('homesteader', 4, 'Homesteader', 'Хозяин усадьбы', '+100 carry weight, +25% XP for every branch. Drawback: you take 25% more damage.', '+100 к переносимому весу, +25% к опыту всех ветвей. Недостаток: вы получаете на 25% больше урона.', 'Build the hall. Let others hold the shield wall.', 'Построй чертог. Пусть другие держат стену щитов.'),
      ],
    },
  ];

  // Same geometry as the game panel: 740 x 590 canvas, origin at the centre, y up.
  const W = 740, H = 590, TRUNK_BASE = -140, TRUNK_TOP = 180;
  const need = (tier) => (tier === 4 ? 15 : tier * 3);
  const gateY = (tier) => TRUNK_BASE + (TRUNK_TOP - TRUNK_BASE) * need(tier) / 15;
  const X = (x) => x + W / 2, Y = (y) => H / 2 - y;
  const NS = 'http://www.w3.org/2000/svg';

  function el(name, attrs, parent) {
    const e = document.createElementNS(NS, name);
    for (const k in attrs) e.setAttribute(k, attrs[k]);
    if (parent) parent.appendChild(e);
    return e;
  }

  function mount(host) {
    host.innerHTML = '';
    const tabs = document.createElement('div');
    tabs.className = 'tabs';
    tabs.setAttribute('role', 'tablist');
    const body = document.createElement('div');
    body.className = 'explorer-body';
    const svg = el('svg', { viewBox: `0 0 ${W} ${H}`, class: 'tree', role: 'img' });
    const card = document.createElement('aside');
    card.className = 'card';
    card.setAttribute('aria-live', 'polite');
    body.append(svg, card);
    host.append(tabs, body);

    let current = 0, pinned = null;

    BRANCHES.forEach((b, i) => {
      const btn = document.createElement('button');
      btn.className = 'tab';
      btn.setAttribute('role', 'tab');
      btn.innerHTML = `<img src="${ROOT}icons/branch_${b.id}.svg" alt=""><span>${t(b.name)}</span>`;
      btn.onclick = () => { current = i; pinned = null; draw(); };
      tabs.appendChild(btn);
    });

    function showCard(node) {
      const b = BRANCHES[current];
      if (!node) {
        card.innerHTML = `<div class="card-icon perk"><img src="${ROOT}icons/branch_${b.id}.svg" alt=""></div>
          <h3>${t(b.name)}</h3><p class="kind">${t(UI.branch)}</p><p class="lore">${t(b.lore)}</p><p class="hint">${t(UI.help)}</p>`;
        return;
      }
      const isPerk = !!node.tier || !!node.ability, isKey = node.tier === 4;
      const kind = node.ability ? t(UI.ability) : isKey ? t(UI.keystone) : isPerk ? UI.perk[LANG](node.tier) : t(UI.mastery);
      const effect = isPerk ? t(node.effect) + (node.ranks > 1 ? ' ' + UI.ranks[LANG](node.ranks) : '') : UI.masteryEffect[LANG](t(node.name));
      const extra = node.ability ? t(UI.abilityRule) : isKey ? t(UI.keystoneRule) : isPerk ? UI.unlock[LANG](node.tier * 3) : t(UI.rankGate);
      card.innerHTML = `<div class="card-icon ${isPerk ? 'perk' : ''}"><img src="${ROOT}icons/${node.id}.svg" alt=""></div>
        <h3>${t(node.name)}</h3><p class="kind">${kind} · ${t(b.name)}</p>
        ${isPerk && !(node.ranks > 1) ? '' : '<div class="pips">' + '<i></i>'.repeat(isPerk ? node.ranks : 5) + '</div>'}
        <p class="effect">${effect}</p>${isPerk ? `<p class="lore">${t(node.lore)}</p>` : ''}<p class="hint">${extra}</p>`;
    }

    function node(g, n, x, y, size, isPerk) {
      const grp = el('g', { class: 'node', tabindex: 0, transform: `translate(${X(x)},${Y(y)})` }, g);
      el('image', { href: `${ROOT}icons/ring_${isPerk ? 'perk' : 'mastery'}.svg`, x: -(size + 14) / 2, y: -(size + 14) / 2, width: size + 14, height: size + 14, class: 'ring' }, grp);
      el('image', { href: `${ROOT}icons/frame_${isPerk ? 'perk' : 'mastery'}.svg`, x: -size / 2, y: -size / 2, width: size, height: size }, grp);
      const s = size * 0.62;
      el('image', { href: `${ROOT}icons/${n.id}.svg`, x: -s / 2, y: -s / 2, width: s, height: s }, grp);
      const label = el('text', { y: n.tier === 4 ? -size / 2 - 12 : size / 2 + 24, class: 'label' }, grp);
      label.textContent = t(n.name);
      const enter = () => showCard(n);
      const leave = () => showCard(pinned);
      grp.addEventListener('mouseenter', enter);
      grp.addEventListener('focus', enter);
      grp.addEventListener('mouseleave', leave);
      grp.addEventListener('blur', leave);
      grp.addEventListener('click', () => { pinned = n; showCard(n); });
    }

    function draw() {
      [...tabs.children].forEach((c, i) => c.setAttribute('aria-selected', i === current));
      svg.innerHTML = '';
      const b = BRANCHES[current];
      const g = el('g', {}, svg);
      const ms = b.masteries;
      const spacing = ms.length > 1 ? Math.min(150, 600 / (ms.length - 1)) : 0;
      const roots = ms.map((_, i) => {
        const x = (i - (ms.length - 1) / 2) * spacing;
        return [x, -220 + 45 * (x / 330) * (x / 330)];
      });
      roots.forEach(([x, y]) => el('line', { x1: X(0), y1: Y(TRUNK_BASE), x2: X(x), y2: Y(y), class: 'root' }, g));
      el('line', { x1: X(0), y1: Y(TRUNK_BASE), x2: X(0), y2: Y(TRUNK_TOP), class: 'trunk' }, g);
      el('line', { x1: X(0), y1: Y(TRUNK_BASE), x2: X(0), y2: Y(TRUNK_TOP), class: 'sap' }, g);
      const pos = {};
      for (let tier = 1; tier <= 4; tier++) {
        const ps = b.perks.filter((p) => p.tier === tier), y = gateY(tier);
        ps.forEach((p, i) => { pos[p.id] = tier === 4 ? [0, y + 46] : [(i ? 1 : -1) * (tier === 2 ? 245 : 130), y]; });
        ps.forEach((p) => el('line', { x1: X(0), y1: Y(y), x2: X(pos[p.id][0]), y2: Y(pos[p.id][1]), class: 'branchline' }, g));
        el('rect', { x: X(-13), y: Y(y) - 3, width: 26, height: 6, class: 'gate' }, g);
        const lbl = el('text', { x: X(tier === 4 ? 30 : -26), y: Y(y + (tier === 4 ? -14 : 14)) + 5, class: 'gate-label' }, g);
        lbl.textContent = need(tier);
      }
      // The active ability grows outward from the tier 1 perks.
      const firstPerk = b.perks.find((p) => p.tier === 1), ab = [-322, gateY(1)];
      el('line', { x1: X(pos[firstPerk.id][0]), y1: Y(pos[firstPerk.id][1]), x2: X(ab[0]), y2: Y(ab[1]), class: 'root' }, g);
      node(g, b.ability, ab[0], ab[1], 56, true);
      ms.forEach((m, i) => node(g, m, roots[i][0], roots[i][1], 60, false));
      b.perks.forEach((p) => node(g, p, pos[p.id][0], pos[p.id][1], p.tier === 4 ? 78 : 64, true));
      showCard(null);
    }

    draw();
  }

  document.querySelectorAll('[data-explorer]').forEach(mount);
})();
