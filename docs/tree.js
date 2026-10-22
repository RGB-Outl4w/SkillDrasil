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
    masteryEffect: { en: (s) => `+20 ${s} skill levels per rank (5 ranks, up to level 100).`, ru: (s) => `+20 к уровню навыка «${s}» за ранг (5 рангов, до уровня 100).` },
    unlock: { en: (n) => `Unlocks after ${n} points spent in this branch.`, ru: (n) => `Открывается после ${n} очков, потраченных в этой ветви.` },
    rankGate: { en: 'Rank R needs 2×(R−1) points already spent in this branch.', ru: 'Ранг R требует 2×(R−1) уже потраченных в ветви очков.' },
    help: { en: 'Hover or tap a node to inspect it.', ru: 'Наведите или нажмите на узел, чтобы изучить его.' },
  };

  const M = (id, en, ru) => ({ id, name: { en, ru } });
  const P = (id, tier, en, ru, effEn, effRu, loreEn, loreRu) =>
    ({ id, tier, name: { en, ru }, effect: { en: effEn, ru: effRu }, lore: { en: loreEn, ru: loreRu } });

  const BRANCHES = [
    {
      id: 'warrior', name: { en: 'Warrior', ru: 'Воин' },
      lore: { en: 'Steel, shield and the will to stand.', ru: 'Сталь, щит и воля выстоять.' },
      masteries: [M('swords', 'Swords', 'Мечи'), M('axes', 'Axes', 'Топоры'), M('clubs', 'Clubs', 'Дубины'),
        M('polearms', 'Polearms', 'Древковое оружие'), M('spears', 'Spears', 'Копья'), M('unarmed', 'Fists', 'Без оружия'),
        M('blocking', 'Blocking', 'Блокирование')],
      perks: [
        P('shieldwall', 1, 'Shield-Wall', 'Стена щитов', 'Blocking costs 25% less stamina.', 'Блок расходует на 25% меньше выносливости.', 'Lock shields. Hold the line.', 'Сомкнуть щиты. Держать строй.'),
        P('berserkgang', 2, 'Berserkgang', 'Берсеркганг', 'Below 30% health: +20% melee damage.', 'При здоровье ниже 30%: +20% к урону в ближнем бою.', "The bear-shirt's fury wakes when blood runs low.", 'Ярость медвежьей шкуры просыпается, когда течёт кровь.'),
        P('riposte', 3, 'Riposte', 'Ответный удар', 'Parries are 30% stronger.', 'Парирование на 30% сильнее.', 'Turn their strength against them.', 'Обрати их силу против них.'),
      ],
    },
    {
      id: 'hunter', name: { en: 'Hunter', ru: 'Охотник' },
      lore: { en: 'Patience, silence and the clean kill.', ru: 'Терпение, тишина и точный удар.' },
      masteries: [M('bows', 'Bows', 'Луки'), M('crossbows', 'Crossbows', 'Арбалеты'), M('knives', 'Knives', 'Ножи'), M('sneak', 'Sneak', 'Скрытность')],
      perks: [
        P('silentdeath', 1, 'Silent Death', 'Тихая смерть', 'Sneak attacks deal 50% more bonus damage.', 'Скрытные атаки наносят на 50% больше бонусного урона.', 'Strike before they know you are there.', 'Бей, прежде чем тебя заметят.'),
        P('steadyaim', 2, 'Steady Aim', 'Твёрдая рука', '+15% bow and crossbow damage while crouched.', '+15% к урону луков и арбалетов, пока вы пригнулись.', 'Breathe. Kneel. Loose.', 'Вдох. Колено. Выстрел.'),
        P('fletcher', 3, "Fletcher's Luck", 'Бережливый стрелок', '25% chance to not use up ammo.', 'Шанс 25% не потратить боеприпас.', 'A good hunter wastes nothing.', 'Хороший охотник ничего не тратит зря.'),
      ],
    },
    {
      id: 'seidr', name: { en: 'Seiðr', ru: 'Сейд' },
      lore: { en: 'The eitr-craft of the völva.', ru: 'Колдовство эйтра, искусство вёльвы.' },
      masteries: [M('elementalmagic', 'Elemental Magic', 'Магия стихий'), M('bloodmagic', 'Blood Magic', 'Магия крови')],
      perks: [
        P('eitrwell', 1, 'Eitr Well', 'Источник эйтра', '+25% eitr regeneration.', '+25% к восстановлению эйтра.', 'Drink deep from the well beneath the roots.', 'Пей из колодца под корнями.'),
        P('focusedmind', 2, 'Focused Mind', 'Ясный разум', 'All eitr costs 20% less.', 'Все затраты эйтра на 20% меньше.', 'A quiet mind spends less of itself.', 'Спокойный разум тратит меньше себя.'),
        P('arcanesurge', 3, 'Arcane Surge', 'Всплеск сейда', '+15% elemental and blood magic damage.', '+15% к урону магией стихий и крови.', 'Let the eitr burn bright.', 'Пусть эйтр горит ярко.'),
      ],
    },
    {
      id: 'wanderer', name: { en: 'Wanderer', ru: 'Странник' },
      lore: { en: 'Wind at your heels, sky beneath your feet.', ru: 'Ветер за спиной, небо под ногами.' },
      masteries: [M('run', 'Run', 'Бег'), M('jump', 'Jump', 'Прыжок'), M('swim', 'Swim', 'Плавание'), M('dodge', 'Dodge', 'Уклонение'), M('ride', 'Riding', 'Верховая езда')],
      perks: [
        P('fleetfoot', 1, 'Fleetfoot', 'Быстроногий', 'Running costs 20% less stamina.', 'Бег расходует на 20% меньше выносливости.', 'The road is long. Your stride is longer.', 'Дорога длинна. Твой шаг длиннее.'),
        P('secondwind', 2, 'Second Wind', 'Второе дыхание', 'Dodging costs 25% less stamina.', 'Уклонение расходует на 25% меньше выносливости.', 'One more breath. One more step.', 'Ещё один вдох. Ещё один шаг.'),
        P('featherfall', 3, 'Featherfall', 'Лёгкое падение', 'Take 50% less fall damage.', 'Урон от падения на 50% меньше.', "Drift down like a leaf from Yggdrasil's crown.", 'Опустись, как лист с кроны Иггдрасиля.'),
      ],
    },
    {
      id: 'artisan', name: { en: 'Artisan', ru: 'Ремесленник' },
      lore: { en: 'Hands that shape the world.', ru: 'Руки, что создают мир.' },
      masteries: [M('pickaxes', 'Pickaxes', 'Кирки'), M('woodcutting', 'Wood Cutting', 'Рубка древесины'), M('fishing', 'Fishing', 'Рыбалка'),
        M('cooking', 'Cooking', 'Кулинария'), M('farming', 'Farming', 'Земледелие'), M('crafting', 'Crafting', 'Ремесло')],
      perks: [
        P('deeppockets', 1, 'Deep Pockets', 'Бездонные карманы', '+50 carry weight.', '+50 к переносимому весу.', 'Carry the world on your back.', 'Неси мир на своих плечах.'),
        P('buildersease', 2, "Builder's Ease", 'Рука мастера', 'Building and repairing cost 30% less stamina.', 'Строительство и ремонт расходуют на 30% меньше выносливости.', 'The hammer remembers the work.', 'Молот помнит работу.'),
        P('hearthkeeper', 3, 'Hearthkeeper', 'Хранитель очага', 'Food lasts 25% longer.', 'Еда действует на 25% дольше.', 'A full belly lasts the long night.', 'Сытый желудок переживёт долгую ночь.'),
      ],
    },
  ];

  // Same geometry as the game panel: 740 x 590 canvas, origin at the centre, y up.
  const W = 740, H = 590, TRUNK_BASE = -120, TRUNK_TOP = 140;
  const gateY = (tier) => TRUNK_BASE + (TRUNK_TOP - TRUNK_BASE) * tier / 3;
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
      const isPerk = !!node.tier;
      const kind = isPerk ? UI.perk[LANG](node.tier) : t(UI.mastery);
      const effect = isPerk ? t(node.effect) : UI.masteryEffect[LANG](t(node.name));
      const extra = isPerk ? UI.unlock[LANG](node.tier * 3) : t(UI.rankGate);
      card.innerHTML = `<div class="card-icon ${isPerk ? 'perk' : ''}"><img src="${ROOT}icons/${node.id}.svg" alt=""></div>
        <h3>${t(node.name)}</h3><p class="kind">${kind} · ${t(b.name)}</p>
        ${isPerk ? '' : '<div class="pips">' + '<i></i>'.repeat(5) + '</div>'}
        <p class="effect">${effect}</p>${isPerk ? `<p class="lore">${t(node.lore)}</p>` : ''}<p class="hint">${extra}</p>`;
    }

    function node(g, n, x, y, size, isPerk) {
      const grp = el('g', { class: 'node', tabindex: 0, transform: `translate(${X(x)},${Y(y)})` }, g);
      el('image', { href: `${ROOT}icons/ring_${isPerk ? 'perk' : 'mastery'}.svg`, x: -(size + 14) / 2, y: -(size + 14) / 2, width: size + 14, height: size + 14, class: 'ring' }, grp);
      el('image', { href: `${ROOT}icons/frame_${isPerk ? 'perk' : 'mastery'}.svg`, x: -size / 2, y: -size / 2, width: size, height: size }, grp);
      const s = size * 0.62;
      el('image', { href: `${ROOT}icons/${n.id}.svg`, x: -s / 2, y: -s / 2, width: s, height: s }, grp);
      const label = el('text', { y: size / 2 + (isPerk ? 22 : 24), class: 'label' }, grp);
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
      const spacing = ms.length > 1 ? Math.min(150, 660 / (ms.length - 1)) : 0;
      const roots = ms.map((_, i) => {
        const x = (i - (ms.length - 1) / 2) * spacing;
        return [x, -200 + 45 * (x / 330) * (x / 330)];
      });
      roots.forEach(([x, y]) => el('line', { x1: X(0), y1: Y(TRUNK_BASE), x2: X(x), y2: Y(y), class: 'root' }, g));
      el('line', { x1: X(0), y1: Y(TRUNK_BASE), x2: X(0), y2: Y(TRUNK_TOP), class: 'trunk' }, g);
      el('line', { x1: X(0), y1: Y(TRUNK_BASE), x2: X(0), y2: Y(TRUNK_TOP), class: 'sap' }, g);
      const perkPos = [[-215, gateY(1)], [215, gateY(2)], [0, 218]];
      for (let tier = 1; tier <= 3; tier++) {
        el('line', { x1: X(0), y1: Y(gateY(tier)), x2: X(perkPos[tier - 1][0]), y2: Y(perkPos[tier - 1][1]), class: 'branchline' }, g);
        el('rect', { x: X(-13), y: Y(gateY(tier)) - 3, width: 26, height: 6, class: 'gate' }, g);
        const lbl = el('text', { x: X(tier === 2 ? -32 : 32), y: Y(gateY(tier) + (tier === 3 ? -12 : 0)) + 5, class: 'gate-label' }, g);
        lbl.textContent = tier * 3;
      }
      ms.forEach((m, i) => node(g, m, roots[i][0], roots[i][1], 64, false));
      b.perks.forEach((p, i) => node(g, p, perkPos[i][0], perkPos[i][1], 78, true));
      showCard(null);
    }

    draw();
  }

  document.querySelectorAll('[data-explorer]').forEach(mount);
})();
