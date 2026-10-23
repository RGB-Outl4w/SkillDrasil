# Source of the SkillDrasil icon set. Writes assets/icons/*.svg.
# Icons are white line-art (the game tints them per node state); black parts stay black under tint
# and are used as cut lines. Frames are baked in colour. Run: python tools/icons.py, then tools/render-icons.ps1.
import io
import os

F = 'fill="#fff" stroke="none"'
K = 'fill="#000" stroke="none"'

ICONS = {
    # Warrior masteries
    'swords': f'<polygon points="53,11 50,22 26,46 18,38 42,14" {F}/>'
              '<path d="M13 35 L29 51" stroke-width="4.5"/><path d="M21 43 L11 53" stroke-width="5"/>'
              f'<circle cx="9.5" cy="54.5" r="3.5" {F}/>',
    'axes': '<path d="M16 58 L40 8" stroke-width="4.5"/>'
            f'<path d="M34 16 C44 10 54 14 57 24 C54 34 46 36 40 36 C41 30 39 24 34 22 Z" {F}/>',
    'clubs': f'<path d="M12 54 L32 32" stroke-width="5"/><circle cx="39" cy="25" r="11" {F}/>'
             '<path d="M39 9 V14 M52 12 L48 16 M55 25 H50 M52 38 L48 34 M26 12 L30 16 M23 25 H28"/>',
    'polearms': '<path d="M8 60 L40 28" stroke-width="3.5"/>'
                f'<path d="M36 24 L46 14 L58 4 L54 18 L44 28 Z" {F}/>'
                '<path d="M40 28 L32 22 C30 16 34 12 38 14" stroke-width="3"/>'
                '<path d="M14 54 L18 58" stroke-width="4"/>',
    'spears': '<path d="M10 56 L42 24"/>'
              f'<path d="M40 26 C42 14 50 8 57 7 C56 14 50 22 38 24 Z" {F}/><path d="M36 22 L44 30" stroke-width="3"/>',
    'unarmed': f'<path d="M14 26 C14 20 20 18 24 20 C26 16 32 16 34 20 C36 16 42 16 44 21 C48 18 54 21 52 28 V40 C52 48 46 52 38 52 H26 C18 52 14 46 14 40 Z" {F}/>'
               '<path d="M24 20 V30 M34 20 V30 M44 22 V30" stroke="#000" stroke-width="2.5"/>'
               '<path d="M14 36 C20 34 28 36 30 42" stroke="#000" stroke-width="2.5"/>'
               f'<rect x="24" y="52" width="18" height="8" rx="2" {F}/>',
    'blocking': f'<circle cx="32" cy="32" r="23" stroke-width="4"/><circle cx="32" cy="32" r="6" {F}/>'
                '<path d="M32 9 V26 M32 38 V55 M9 32 H26 M38 32 H55" stroke-width="2.5"/>',
    # Hunter masteries
    'bows': '<path d="M22 8 Q54 32 22 56" stroke-width="4"/><path d="M22 8 V56" stroke-width="1.5"/>'
            f'<path d="M10 32 H48" stroke-width="3"/><polygon points="56,32 46,27 46,37" {F}/>'
            '<path d="M10 32 L5 27 M10 32 L5 37 M15 32 L10 27 M15 32 L10 37" stroke-width="2.5"/>',
    'crossbows': '<path d="M32 18 V58" stroke-width="6"/><path d="M8 26 Q32 10 56 26" stroke-width="4"/>'
                 '<path d="M8 26 L32 34 L56 26" stroke-width="1.5"/><path d="M32 34 V10" stroke-width="2.5"/>'
                 f'<polygon points="32,4 37,12 27,12" {F}/>',
    'knives': f'<polygon points="32,5 38,16 36,38 28,38 26,16" {F}/><path d="M21 41 H43" stroke-width="4"/>'
              f'<path d="M32 41 V53" stroke-width="5.5"/><circle cx="32" cy="57" r="3.5" {F}/>',
    'sneak': '<path d="M10 22 Q32 0 54 22"/><path d="M8 36 Q32 16 56 36 Q32 54 8 36 Z"/>'
             f'<circle cx="32" cy="36" r="7" {F}/>',
    # Seidr masteries
    'elementalmagic': '<path d="M32 6 C40 18 50 24 48 40 A16 16 0 0 1 16 40 C15 30 22 25 25 16 C29 24 33 24 32 6 Z"/>'
                      f'<path d="M32 30 C36 36 40 40 38 46 A6 6 0 0 1 26 46 C25 41 29 38 32 30 Z" {F}/>',
    'bloodmagic': '<path d="M32 6 C40 20 50 30 50 41 A18 18 0 0 1 14 41 C14 30 24 20 32 6 Z"/>'
                  '<path d="M32 26 V52 M32 36 L24 28 M32 36 L40 28"/>',
    # Wanderer masteries
    'run': '<path d="M24 10 H36 V36 L52 42 V52 H22 Z"/><path d="M22 46 H52" stroke-width="2.5"/>'
           '<path d="M6 22 H16 M4 31 H16 M6 40 H16" stroke-width="3"/>',
    'jump': '<polyline points="16,30 32,14 48,30" stroke-width="4.5"/><polyline points="16,44 32,28 48,44" stroke-width="4.5"/>'
            '<path d="M10 55 H54" stroke-width="3"/>',
    'swim': '<path d="M6 22 q7 -7 13 0 t13 0 t13 0 t13 0"/><path d="M6 34 q7 -7 13 0 t13 0 t13 0 t13 0"/>'
            '<path d="M6 46 q7 -7 13 0 t13 0 t13 0 t13 0"/>',
    'dodge': f'<path d="M44 48 A18 18 0 1 1 50 26" stroke-width="4"/><polygon points="56,22 46,20 52,30" {F}/>'
             '<path d="M8 50 H22 M12 57 H28" stroke-width="3"/>',
    'ride': '<path d="M18 12 V32 A14 14 0 0 0 46 32 V12" stroke-width="8"/>'
            f'<circle cx="18" cy="22" r="1.8" {K}/><circle cx="46" cy="22" r="1.8" {K}/>'
            f'<circle cx="21" cy="38" r="1.8" {K}/><circle cx="43" cy="38" r="1.8" {K}/>',
    # Artisan masteries
    'pickaxes': '<path d="M8 24 Q32 4 56 24" stroke-width="5.5"/><path d="M32 14 V58" stroke-width="4.5"/>',
    'woodcutting': '<path d="M14 6 V58 M34 6 V58" stroke-width="3.5"/><path d="M14 58 H34" stroke-width="3.5"/>'
                   '<path d="M20 18 C22 24 22 30 20 36" stroke-width="2"/>'
                   f'<path d="M56 18 L36 34" stroke-width="4"/><polygon points="30,26 40,28 38,40 28,38" {F}/>'
                   '<path d="M44 46 L50 44 M46 52 L54 54 M40 54 L42 60" stroke-width="2.5"/>',
    'fishing': '<circle cx="40" cy="8" r="3.5" stroke-width="3"/><path d="M40 12 V40 A11 11 0 1 1 18 40 L23 34" stroke-width="4"/>'
               f'<polygon points="24,30 26,40 17,36" {F}/>',
    'cooking': '<path d="M10 26 H54" stroke-width="4"/><path d="M14 26 L18 46 Q32 54 46 46 L50 26"/>'
               '<path d="M20 50 L16 58 M44 50 L48 58" stroke-width="3"/>'
               '<path d="M24 20 q-5 -5 0 -10 M32 20 q-5 -5 0 -12 M40 20 q-5 -5 0 -10" stroke-width="2.5"/>',
    'farming': '<path d="M32 58 V12" stroke-width="3"/>'
               f'<g {F}>'
               '<ellipse cx="26" cy="20" rx="3.5" ry="7" transform="rotate(-30 26 20)"/><ellipse cx="38" cy="20" rx="3.5" ry="7" transform="rotate(30 38 20)"/>'
               '<ellipse cx="26" cy="32" rx="3.5" ry="7" transform="rotate(-30 26 32)"/><ellipse cx="38" cy="32" rx="3.5" ry="7" transform="rotate(30 38 32)"/>'
               '<ellipse cx="26" cy="44" rx="3.5" ry="7" transform="rotate(-30 26 44)"/><ellipse cx="38" cy="44" rx="3.5" ry="7" transform="rotate(30 38 44)"/>'
               '<ellipse cx="32" cy="9" rx="3.5" ry="6"/></g>',
    'crafting': f'<path d="M8 24 H54 V30 C54 34 50 36 46 36 L42 44 H48 V52 H16 V44 H22 L18 34 C12 34 8 30 8 24 Z" {F}/>'
                '<path d="M30 14 L26 6 M38 14 L42 6 M34 12 V4" stroke-width="2.5"/>',
    # Warrior perks
    'shieldwall': f'<circle cx="16" cy="36" r="13" {F}/><circle cx="48" cy="36" r="13" {F}/>'
                  '<circle cx="32" cy="32" r="15" fill="#fff" stroke="#000" stroke-width="3"/>'
                  f'<circle cx="32" cy="32" r="4" {K}/>',
    'berserkgang': '<path d="M16 8 Q26 32 14 56" stroke-width="5"/><path d="M30 6 Q40 32 28 58" stroke-width="5"/>'
                   '<path d="M44 8 Q54 32 42 56" stroke-width="5"/>',
    'riposte': '<path d="M10 54 L50 14" stroke-width="4.5"/><path d="M54 54 L14 14" stroke-width="4.5"/>'
               '<path d="M12 46 L18 52 M52 46 L46 52" stroke-width="4"/>'
               '<polygon points="32,20 35,29 44,32 35,35 32,44 29,35 20,32 29,29" fill="#fff" stroke="#000" stroke-width="2"/>',
    # Hunter perks
    'silentdeath': f'<path d="M22 6 A16 16 0 1 0 40 30 A12 12 0 1 1 22 6 Z" {F}/>'
                   f'<polygon points="58,26 50,34 34,52 30,48 48,30" {F}/>'
                   '<path d="M30 42 L40 52"/><path d="M33 51 L26 58" stroke-width="4"/>',
    'steadyaim': '<circle cx="32" cy="32" r="20"/><circle cx="32" cy="32" r="9" stroke-width="3"/>'
                 f'<path d="M32 4 V16 M32 48 V60 M4 32 H16 M48 32 H60"/><circle cx="32" cy="32" r="2.5" {F}/>',
    'fletcher': f'<path d="M28 54 V14" stroke-width="3"/><polygon points="28,5 35,17 21,17" {F}/>'
                '<path d="M28 48 L20 56 M28 48 L36 56 M28 41 L20 49 M28 41 L36 49" stroke-width="2.5"/>'
                f'<polygon points="48,14 50,21 57,23 50,25 48,32 46,25 39,23 46,21" {F}/>',
    # Seidr perks
    'eitrwell': f'<path d="M32 4 C36 12 41 17 41 22 A9 9 0 0 1 23 22 C23 17 28 12 32 4 Z" {F}/>'
                '<ellipse cx="32" cy="40" rx="20" ry="6"/><path d="M12 40 V52 Q32 62 52 52 V40"/>'
                '<path d="M22 47 V56 M32 48 V58 M42 47 V56" stroke-width="2"/>',
    'focusedmind': '<path d="M32 6 L58 54 H6 Z"/><path d="M17 40 Q32 26 47 40 Q32 52 17 40 Z" stroke-width="3"/>'
                   f'<circle cx="32" cy="40" r="4.5" {F}/>',
    'arcanesurge': f'<polygon points="32,4 37,22 54,10 42,27 60,32 42,37 54,54 37,42 32,60 27,42 10,54 22,37 4,32 22,27 10,10 27,22" {F}/>'
                   f'<circle cx="32" cy="32" r="7" {K}/><circle cx="32" cy="32" r="3.5" {F}/>',
    # Wanderer perks
    'fleetfoot': '<path d="M28 12 H40 V36 L56 42 V52 H26 Z"/>'
                 '<path d="M26 18 Q12 14 4 18 Q14 22 26 24 M26 26 Q14 26 8 30 Q16 33 26 32" stroke-width="2.5"/>',
    'secondwind': '<path d="M6 22 H38 A7 7 0 1 0 31 15"/><path d="M6 33 H48 A8 8 0 1 1 41 41"/>'
                  '<path d="M6 44 H28 A6 6 0 1 1 23 50"/>',
    'featherfall': '<path d="M50 8 Q22 14 14 54 Q42 46 50 8 Z" stroke-width="3"/><path d="M14 54 L48 12" stroke-width="2.5"/>'
                   '<path d="M22 40 L18 32 M28 33 L25 24 M35 25 L33 17 M27 46 L36 45 M33 39 L42 37 M39 31 L47 28" stroke-width="2"/>',
    # Artisan perks
    'deeppockets': '<path d="M22 22 C10 34 10 56 32 56 C54 56 54 34 42 22 Z"/><path d="M20 22 H44" stroke-width="4"/>'
                   f'<path d="M26 16 L22 8 M38 16 L42 8 M32 16 V6" stroke-width="2.5"/><circle cx="32" cy="40" r="7" {F}/>',
    'buildersease': f'<rect x="12" y="10" width="34" height="13" rx="2" {F}/><rect x="26" y="23" width="7" height="34" rx="2" {F}/>'
                    f'<polygon points="51,32 53,38 59,40 53,42 51,48 49,42 43,40 49,38" {F}/>',
    'hearthkeeper': f'<path d="M32 8 C38 18 44 22 42 32 A10 10 0 0 1 22 32 C21 26 25 22 27 16 C29 22 32 22 32 8 Z" {F}/>'
                    f'<g {F}><rect x="6" y="46" width="14" height="9" rx="4"/><rect x="22" y="44" width="20" height="11" rx="5"/>'
                    '<rect x="44" y="46" width="14" height="9" rx="4"/></g>'
                    '<path d="M14 42 L24 36 M50 42 L40 36" stroke-width="3"/>',
    # Branch tabs
    # 0.3.2 active abilities
    'warcry': f'<path d="M8 40 C8 28 18 22 30 26 L44 14 V52 L30 40 C22 44 8 50 8 40 Z" {F}/>'
              '<path d="M50 22 C54 26 54 38 50 42 M56 16 C62 24 62 40 56 48" stroke-width="3"/>',
    'huntersmark': '<circle cx="34" cy="34" r="20" stroke-width="3"/><circle cx="34" cy="34" r="9" stroke-width="3"/>'
                   '<path d="M26 26 L42 42 M42 26 L26 42" stroke-width="4"/>'
                   '<path d="M34 4 V12 M34 56 V64 M4 34 H12 M56 34 H64" stroke-width="3"/>',
    'volvaschant': f'<path d="M32 58 V24" stroke-width="4"/><circle cx="32" cy="16" r="7" {F}/>'
                   '<path d="M18 10 C12 18 12 30 18 38 M46 10 C52 18 52 30 46 38" stroke-width="3"/>'
                   '<path d="M10 4 C2 16 2 32 10 44 M54 4 C62 16 62 32 54 44" stroke-width="2" opacity="0.6"/>',
    'galestep': f'<path d="M30 12 H42 V34 L58 40 V50 H28 Z" {F}/>'
                '<path d="M4 22 H20 M8 32 H22 M2 42 H22 M10 52 H24" stroke-width="3"/>',
    'tirelesstoil': '<path d="M10 44 L38 16" stroke-width="4.5"/><rect x="32" y="4" width="18" height="11" rx="2" transform="rotate(45 41 9)" fill="#fff" stroke="none"/>'
                    '<path d="M54 44 L28 18" stroke-width="4"/><path d="M18 22 Q30 8 44 14" stroke-width="4.5"/>'
                    '<path d="M18 54 C18 48 24 46 32 54 C40 62 46 60 46 54 C46 48 40 46 32 54 C24 62 18 60 18 54 Z" stroke-width="2.5"/>',
    # 0.3 perks
    'ironhide': '<path d="M18 10 L32 6 L46 10 L54 22 L48 26 V56 H16 V26 L10 22 Z"/>'
                '<path d="M22 30 H42 M22 38 H42 M22 46 H42" stroke-width="2.5" stroke-dasharray="3 3"/>',
    'battlefervor': f'<path d="M14 50 L46 18" stroke-width="4.5"/><path d="M10 46 L18 54" stroke-width="4"/>'
                    f'<path d="M44 34 C50 26 56 24 54 14 C50 20 46 18 46 10 C40 16 38 24 44 34 Z" {F}/>',
    'unshaken': f'<path d="M32 58 V30" stroke-width="5"/><circle cx="32" cy="22" r="15" {F}/>'
                '<path d="M32 50 C24 54 16 54 10 58 M32 50 C40 54 48 54 54 58" stroke-width="3.5"/>',
    'bearshirt': f'<ellipse cx="32" cy="42" rx="14" ry="12" {F}/>'
                 f'<g {F}><ellipse cx="14" cy="24" rx="5" ry="7"/><ellipse cx="25" cy="15" rx="5" ry="7"/>'
                 '<ellipse cx="39" cy="15" rx="5" ry="7"/><ellipse cx="50" cy="24" rx="5" ry="7"/></g>',
    'shadowstep': f'<g {F}><path d="M18 34 C14 26 18 14 24 14 C30 14 30 26 26 34 Z"/><circle cx="22" cy="40" r="4"/>'
                  '<path d="M40 50 C36 42 40 30 46 30 C52 30 52 42 48 50 Z" opacity="0.55"/><circle cx="44" cy="56" r="4" opacity="0.55"/></g>',
    'keeneye': '<path d="M6 32 Q32 10 58 32 Q32 54 6 32 Z"/>'
               f'<circle cx="32" cy="32" r="9" {F}/><circle cx="32" cy="32" r="3.5" {K}/><path d="M32 4 V12 M32 52 V60" stroke-width="3"/>',
    'ghost': f'<path d="M16 50 V28 C16 14 24 6 32 6 C40 6 48 14 48 28 V50 Z" {F}/>'
             f'<path d="M22 30 C22 22 26 18 32 18 C38 18 42 22 42 30 V36 H22 Z" {K}/>'
             f'<circle cx="27" cy="29" r="2.5" {F}/><circle cx="37" cy="29" r="2.5" {F}/>'
             '<path d="M12 54 H24 M28 58 H40 M44 54 H54" stroke-width="3" stroke-dasharray="4 3"/>',
    'lonewolf': f'<path d="M12 10 L22 24 L32 20 L42 24 L52 10 L50 32 C50 44 42 54 32 58 C22 54 14 44 14 32 Z" {F}/>'
                f'<path d="M22 34 L28 36 M42 34 L36 36" stroke="#000" stroke-width="3"/><path d="M28 46 L32 50 L36 46" stroke="#000" stroke-width="2.5"/>',
    'vigor': f'<path d="M32 56 C10 40 6 28 12 18 C18 8 30 12 32 20 C34 12 46 8 52 18 C58 28 54 40 32 56 Z" {F}/>'
             '<path d="M32 22 V44 M32 30 L24 24 M32 30 L40 24" stroke="#000" stroke-width="3"/>',
    'galdr': '<path d="M8 32 Q14 20 20 32 T32 32 T44 32 T56 32" stroke-width="3"/>'
             '<path d="M8 44 Q14 32 20 44 T32 44 T44 44 T56 44" stroke-width="2" opacity="0.6"/>'
             '<path d="M26 8 V22 M26 8 L34 14 L26 18" stroke-width="3"/>',
    'runeward': '<path d="M32 6 L54 14 V30 C54 44 44 54 32 58 C20 54 10 44 10 30 V14 Z"/>'
                '<path d="M32 18 V46 M32 26 L24 34 M32 26 L40 34" stroke-width="3.5"/>',
    'volvasoath': f'<path d="M32 60 V22" stroke-width="4"/><circle cx="32" cy="14" r="8"/>'
                  f'<circle cx="32" cy="14" r="3" {F}/><path d="M22 24 C26 28 38 28 42 24" stroke-width="3"/>'
                  '<path d="M18 8 L22 12 M46 8 L42 12 M14 18 H20 M44 18 H50" stroke-width="2.5"/>',
    'sealborn': f'<path d="M10 40 C12 26 30 20 44 24 C52 26 56 22 58 18 C58 28 54 34 48 36 C40 40 24 44 10 40 Z" {F}/>'
                '<path d="M6 52 q6 -6 12 0 t12 0 t12 0 t12 0" stroke-width="3"/>',
    'springstep': '<path d="M20 56 L44 50 L20 44 L44 38 L20 32 L44 26" stroke-width="3.5"/>'
                  f'<path d="M32 22 V6" stroke-width="3.5"/><polygon points="32,2 39,12 25,12" {F}/>',
    'windrunner': '<path d="M6 20 H34 C42 20 44 12 38 10" stroke-width="3.5"/>'
                  '<path d="M6 32 H46 C56 32 58 42 50 44" stroke-width="3.5"/>'
                  '<path d="M6 44 H28 C36 44 38 52 32 54" stroke-width="3.5"/>'
                  f'<polygon points="52,14 56,20 60,14 56,8" {F}/>',
    'endlessroad': '<path d="M26 8 L6 58 M38 8 L58 58" stroke-width="3.5"/>'
                   '<path d="M32 14 V20 M32 28 V36 M32 44 V54" stroke-width="3.5"/><circle cx="32" cy="6" r="3"/>',
    'quickstudy': f'<path d="M32 16 C24 10 14 10 8 12 V52 C14 50 24 50 32 56 C40 50 50 50 56 52 V12 C50 10 40 10 32 16 Z"/>'
                  '<path d="M32 16 V56" stroke-width="2.5"/><path d="M14 22 H26 M14 30 H26 M38 22 H50 M38 30 H50" stroke-width="2"/>',
    'minersgrit': '<path d="M8 22 Q28 6 48 22" stroke-width="5"/><path d="M28 14 V58" stroke-width="4.5"/>'
                  f'<polygon points="50,34 52,40 58,42 52,44 50,50 48,44 42,42 48,40" {F}/>',
    'hardworker': f'<path d="M14 54 V40 C14 32 20 28 26 30 L30 22 C32 18 38 18 40 22 L40 30 C48 30 52 36 50 44 L46 54 Z" {F}/>'
                  f'<path d="M48 6 C52 12 54 14 54 17 A4 4 0 0 1 46 17 C46 14 46 12 48 6 Z" {F}/>',
    'homesteader': '<path d="M6 34 L32 12 L58 34" stroke-width="4"/><path d="M12 30 V56 H52 V30"/>'
                   f'<rect x="27" y="40" width="10" height="16" {F}/><path d="M20 12 L14 4 M44 12 L50 4" stroke-width="3"/>',
    'branch_warrior': f'<path d="M12 36 A20 20 0 0 1 52 36 Z" {F}/><path d="M8 37 H56" stroke-width="4"/>'
                      '<path d="M32 37 V54" stroke-width="5"/><path d="M14 40 Q14 52 24 54 M50 40 Q50 52 40 54"/>',
    'branch_hunter': '<path d="M32 58 V30"/>'
                     '<path d="M32 30 C24 26 18 18 18 6 M20 18 L10 14 M22 24 L12 26"/>'
                     '<path d="M32 30 C40 26 46 18 46 6 M44 18 L54 14 M42 24 L52 26"/>',
    'branch_seidr': '<g stroke-width="3"><polygon points="32,8 44,30 20,30"/><polygon points="22,28 34,50 10,50"/>'
                    '<polygon points="42,28 54,50 30,50"/></g>',
    'branch_wanderer': '<circle cx="32" cy="32" r="24" stroke-width="3"/>'
                       f'<polygon points="32,6 37,27 58,32 37,37 32,58 27,37 6,32 27,27" {F}/><circle cx="32" cy="32" r="4" {K}/>',
    'branch_artisan': '<path d="M12 52 L44 20" stroke-width="4.5"/>'
                      f'<rect x="36" y="6" width="22" height="12" rx="2" transform="rotate(45 47 12)" {F}/>'
                      f'<path d="M52 52 L20 20" stroke-width="4.5"/><path d="M14 26 C10 18 14 10 22 8 L28 16 Z" {F}/>',
    # Rune of Forgetting item, rank pip
    'rune': '<path d="M18 58 C10 44 12 20 22 10 C30 4 40 6 46 14 C54 26 54 46 46 58 Z"/>'
            '<path d="M32 16 V50 M32 22 L42 30 M32 22 L22 30 M32 34 L42 42 M32 34 L22 42"/>',
    # Baked-colour art for places the game shows untinted: inventory item, HUD status effect
    'item_rune': '<path d="M18 58 C10 44 12 20 22 10 C30 4 40 6 46 14 C54 26 54 46 46 58 Z" fill="#5b5a60" stroke="#2a2a2e" stroke-width="3"/>'
                 '<path d="M20 54 C14 42 16 22 24 14" stroke="#8a8990" stroke-width="2"/>'
                 '<path d="M32 16 V50 M32 22 L42 30 M32 22 L22 30 M32 34 L42 42 M32 34 L22 42" stroke="#ffb347" stroke-width="3.5"/>',
    'se_severed': '<path d="M32 6 V30" stroke="#c9a46a" stroke-width="6"/>'
                  '<path d="M32 36 C30 44 22 48 12 52 M32 36 V58 M32 36 C34 44 42 48 52 52" stroke="#c9a46a" stroke-width="4"/>'
                  '<path d="M14 40 L50 26" stroke="#e04a3a" stroke-width="5"/>',
    'pip': f'<polygon points="32,6 58,32 32,58 6,32" {F}/>',
    # Node frames (baked colour) and selection glow rings (white, tinted)
    'frame_mastery': '<defs><linearGradient id="g" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#3a322a"/>'
                     '<stop offset="1" stop-color="#1c1814"/></linearGradient></defs>'
                     '<rect x="3" y="3" width="58" height="58" rx="9" fill="url(#g)" stroke="#a9813f" stroke-width="3"/>'
                     '<rect x="8" y="8" width="48" height="48" rx="6" stroke="#5e4a2e" stroke-width="1.5"/>',
    'frame_perk': '<defs><radialGradient id="g" cx="0.5" cy="0.4" r="0.6"><stop offset="0" stop-color="#3e352b"/>'
                  '<stop offset="1" stop-color="#191511"/></radialGradient></defs>'
                  '<circle cx="32" cy="32" r="29" fill="url(#g)" stroke="#a9813f" stroke-width="3"/>'
                  '<circle cx="32" cy="32" r="24" stroke="#5e4a2e" stroke-width="1.5"/>',
    'ring_mastery': '<defs><filter id="b" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur stdDeviation="2"/></filter></defs>'
                    '<rect x="5" y="5" width="54" height="54" rx="10" stroke-width="4" filter="url(#b)"/>'
                    '<rect x="5" y="5" width="54" height="54" rx="10" stroke-width="2"/>',
    'ring_perk': '<defs><filter id="b" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur stdDeviation="2"/></filter></defs>'
                 '<circle cx="32" cy="32" r="28" stroke-width="4" filter="url(#b)"/><circle cx="32" cy="32" r="28" stroke-width="2"/>',
}

HEAD = ('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" fill="none" stroke="#fff" '
        'stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round">\n')

if __name__ == '__main__':
    out = os.path.join(os.path.dirname(__file__), '..', 'assets', 'icons')
    os.makedirs(out, exist_ok=True)
    for name, body in ICONS.items():
        with io.open(os.path.join(out, name + '.svg'), 'w', encoding='utf-8', newline='\n') as f:
            f.write(HEAD + body + '\n</svg>\n')
    # Website copies: same art, gold baked in (the game tints at runtime, the page can't tint an <img>).
    web = os.path.join(os.path.dirname(__file__), '..', 'docs', 'icons')
    os.makedirs(web, exist_ok=True)
    for name, body in ICONS.items():
        with io.open(os.path.join(web, name + '.svg'), 'w', encoding='utf-8', newline='\n') as f:
            f.write((HEAD + body + '\n</svg>\n').replace('#fff', '#f2b24a'))
    print(len(ICONS), 'svgs')
