# PATHOGEN — Game Design Document

**The Body is the Battlefield**

A 5v5 mobile 3D MOBA set inside a human body. The Virus team invades and drains the host. The Immune team defends and flushes the infection out. The win condition is a central Human Health Bar — an abstract tug-of-war gauge that replaces the traditional destructible Nexus found in other MOBAs. This is PATHOGEN's core innovation: the health bar IS the Nexus.

---

## Table of Contents

1. [Core Concept & USPs](#1-core-concept--usps)
2. [Win Conditions](#2-win-conditions)
3. [Map Layout](#3-map-layout)
4. [Lanes & Roles](#4-lanes--roles)
5. [Structures — Dark Sentinels & Sentinels](#5-structures--dark-sentinels--sentinels)
6. [Supply Lines](#6-supply-lines)
7. [Organs — 11 Immune-Side Objectives](#7-organs--11-immune-side-objectives)
8. [Hives — 7 Virus-Side Objectives](#8-hives--7-virus-side-objectives)
9. [Organ & Hive Defenders](#9-organ--hive-defenders)
10. [Defender Conversion Mechanic](#10-defender-conversion-mechanic)
11. [Slayer Mechanic — Deteriorating & Festering Phases](#11-slayer-mechanic--deteriorating--festering-phases)
12. [The Heart — Virus Main Objective](#12-the-heart--virus-main-objective)
13. [The Infection Site — Immune Main Objective](#13-the-infection-site--immune-main-objective)
14. [Surge System](#14-surge-system)
15. [Hypothalamus — Neutral Boss](#15-hypothalamus--neutral-boss)
16. [Game Phases](#16-game-phases)
17. [Missions — Phase 0 Only](#17-missions--phase-0-only)
18. [Body Conditions — Comeback Mechanic](#18-body-conditions--comeback-mechanic)
19. [Minions & Frenzied Minions](#19-minions--frenzied-minions)
20. [Neutral Jungle Camps](#20-neutral-jungle-camps)
21. [Economy](#21-economy)
22. [Mutation Shop & Tier Upgrades](#22-mutation-shop--tier-upgrades)
23. [Leveling & Gene Points](#23-leveling--gene-points)
24. [Champion Roster — Virus Team](#24-champion-roster--virus-team)
25. [Champion Roster — Immune Team](#25-champion-roster--immune-team)
26. [Stat Glossary](#26-stat-glossary)
27. [Strategy Guide](#27-strategy-guide)
28. [Post-MVP — Adaptive Biomes](#28-post-mvp--adaptive-biomes)

---

## 1. Core Concept & USPs

PATHOGEN is not a League of Legends reskin with a biology theme. It is a fundamentally different MOBA built around a single innovation: the Human Health Bar replaces the Nexus as the win condition.

**What makes PATHOGEN different from every other MOBA:**

- **The health bar IS the Nexus.** There is no destructible base. No building to push toward. The health bar starts at 50% and both teams push it in opposite directions through strategic actions: champion kills, structure destruction, organ captures, Hive destruction, and main objective Surges. When it hits 0% (virus wins) or 100% (immune wins), the game ends.
- **Asymmetric team goals with symmetric aggression.** The virus team attacks organs (immune territory) to drain the host. The immune team attacks Hives (virus territory) to flush the infection out. Both teams are aggressors pushing into enemy territory. Neither team is passive.
- **Timer-driven organ phasing.** Objectives appear on a fixed schedule (not tied to health %), giving both teams predictable escalation windows. No "lose to win" paradoxes.
- **Supply lines from structures to defenders.** Pushing structures mechanically weakens organ and Hive defenders through severed biological connections. Lane wins cascade into jungle objective advantages.
- **Defender conversion, not killing.** Objective defenders cannot die — only be converted to the enemy team through a last-hit system. This creates territorial skirmish moments before every major objective fight.
- **Slayer mechanic.** The champion who last-hits a defender carries a personal aura and becomes a marked target. Their team gets temporary buffs and health bar damage. The enemy team must hunt and kill the slayer to reverse the capture. Every objective becomes a two-phase event: win the contest, then survive as the slayer.
- **Surge system for main objectives.** Capturing the Heart or Infection Site triggers a team-wide Surge that ticks the health bar toward victory. The enemy can reverse the Surge by ACEing the Surging team, creating the game's climactic moments.
- **Bases are sanctuaries, not targets.** Both bases are invulnerable. They serve as spawn points, recall destinations, and shop access. Minions that reach the enemy base on open lanes deal small health bar damage directly — the health bar replaces the Nexus in every mechanical sense.

---

## 2. Win Conditions

The Human Health Bar starts at 50%. Both teams push it in opposite directions.

**Virus team wins when:** Health bar reaches 0%.

**Immune team wins when:** Health bar reaches 100%.

**Alternative win — Surge ACE:** If a Surging team (having captured the Heart or Infection Site) fully ACEs the non-Surging team, the game ends immediately in the Surging team's favor.

There is no other win condition. No destructible bases. No Nexus equivalent. The health bar is the only target, and it can only be affected through champion-meaningful actions.

---

## 3. Map Layout

**Shape:** Symmetric hexagon. Equal jungle area on both sides. Equal lane lengths. Equal camp count. Both roamers have identical pathing, clear speeds, and gank distances. No geometric advantage for either team.

**Axis:** Outside-to-inside. Virus base on the left (skin surface — the infection entry point). Immune core on the right (body's deep interior). The virus team pushes right (inward). The immune team pushes left (outward, flushing the infection).

**Content asymmetry:** The virus jungle (left half) contains 7 Hives plus the Infection Site. The immune jungle (right half) contains 11 organs plus the Heart. The Hypothalamus neutral boss spawns near map center at Phase 3.

**Jungle structure:** Upper jungle sits between the Cortex and Pulmonary lanes. Lower jungle sits between the Pulmonary and Visceral lanes. The jungle physically separates lanes — cross-lane ganks require traversing the jungle, making rotation decisions meaningful.

---

## 4. Lanes & Roles

Three lanes connect the virus base (left) to the immune core (right). Minions spawn at each base and walk their lane, engaging enemy minions and structures.

| Lane | Position | Role | Body Region |
|------|----------|------|-------------|
| Cortex | Top | Solo | Head / nervous system |
| Pulmonary | Mid | Solo | Chest / respiratory |
| Visceral | Bot | Duo (ADC + Support) | Abdomen / digestive |

**The Roamer (Jungler):** Farms neutral camps in the jungle, ganks lanes through entrance corridors, contests jungle organ and Hive objectives, and guards the main objectives (Heart, Infection Site). No lane, no minions, no structures of their own. Income comes from camps, assists, and objective rewards.

**First Blood bonus:** The first champion kill of the match grants +200 Bio and +100 XP to the killer. This encourages early gank attempts and gives the roamer a meaningful early reward.

---

## 5. Structures — Dark Sentinels & Sentinels

Both teams have defensive structures along each lane. Virus structures are **Dark Sentinels** (D). Immune structures are **Sentinels** (S). They behave identically.

**Per lane:** 2 Dark Sentinels (virus side) + 2 Sentinels (immune side) = 4 per lane, 12 total.

**Structure behavior:**
- First-in-range targeting: whichever enemy entity enters range first gets targeted
- Champion aggro override: if an enemy champion attacks an allied champion within range, the structure switches to that enemy immediately
- Homing projectiles: 80–100 damage per shot, 0.88 attacks/second, 7–9 range
- Escalating true damage: after 3 consecutive shots on the same enemy champion, the structure deals true damage (bypasses all armor/MR), shown by a white projectile
- Range indicator: yellow-orange ring at the structure's base when any champion approaches within 3 units of attack range
- HP: 1,500–2,000
- Structures must be destroyed in order — you cannot skip the outer to hit the inner

**Destruction reward:** 5% health bar shift toward your team + 150 Bio + 100 XP (shared among nearby allies)

---

## 6. Supply Lines

Structures feed biological connections to nearby objective defenders, empowering them.

**Sentinels** (immune structures) buff the 2 nearest organ defenders in the same jungle quadrant. **Dark Sentinels** (virus structures) buff the 2 nearest Hive defenders in the same jungle quadrant.

Each surviving connected structure grants its nearby defender: **+16% bonus HP, +16% bonus damage, +16% bonus damage reduction.**

Two structures alive and connected = +32% total bonus stats on the defender.

**Special case — main objectives:** The Cardiac Sentinel (Heart's defender) is fed by ALL 6 Sentinels globally. The Infection Guardian (Infection Site's defender) is fed by ALL 6 Dark Sentinels globally.

**When a defender is converted** to the enemy team, supply lines from the 2 nearest friendly structures to that defender are immediately severed. The structures still function (shoot, have HP, grant 5% on death) but no longer empower the defender.

**Supply lines reconnect** when the defender reverts to neutral (after slayer death or re-conversion).

**Structure destruction permanently** removes that supply line source for the rest of the match.

---

## 7. Organs — 11 Immune-Side Objectives

Organs sit in the immune jungle (right half of the map) at roughly anatomically correct vertical positions. They are the virus team's attack targets. Each organ is guarded by a unique defender creature.

Organs are dormant until their phase begins. Dormant organs cannot be damaged, but their defenders are always awake and patrol their alcove.

**Organ placement:** Brain sits near the inner Sentinel (S2) on the Cortex lane — close to lane but off it in an alcove. Other organs are distributed through the immune jungle between lanes. Intestines sit in the lower immune jungle near the Visceral lane.

| Organ | Health Shift | Deteriorating Window | Team Effect (virus captures) |
|-------|-------------|---------------------|------------------------------|
| Skin | 3% | 1:00 | +5 armor, +5 MR |
| Lymph Vessels | 4% | 1:15 | +3% movement speed |
| Stomach | 5% | 1:30 | +10 Bio/sec team income |
| Intestines | 5% | 1:30 | +5% healing received |
| Thymus | 6% | 1:45 | +5% ability damage |
| Spleen | 6% | 1:45 | +8% attack speed |
| Kidneys | 7% | 2:00 | +5% cooldown reduction |
| Liver | 8% | 2:15 | +5% damage reduction |
| Bone Marrow | 8% | 2:15 | +8% max HP |
| Lungs | 10% | 2:30 | +15 flat AD, +15 flat AP |
| Brain | 15% | 3:00 | +10% all damage |

**Total potential virus health shift from organs:** 77%

All 11 effects are unique — no two organs grant the same type of buff.

---

## 8. Hives — 7 Virus-Side Objectives

Hives sit in the virus jungle (left half of the map). They are viral replication colonies that the immune team attacks. Each Hive is guarded by a unique defender creature.

**No passive buffs.** Hives do not grant the virus team any bonuses at game start. All buffs must be earned through captures. Both teams start at perfect parity.

**Hive placement:** Replication Hive sits in the upper-left jungle (the immune team's "Brain equivalent" — highest value, requires deliberate rotation). Dormancy Hive sits in the lower-left jungle. Remaining Hives are distributed through the virus jungle between lanes.

| Hive | Health Shift | Festering Window | Team Effect (immune captures) |
|------|-------------|-----------------|-------------------------------|
| Catalyst Hive | 5% | 1:30 | +3 mana regen/sec |
| Resistance Hive | 6% | 1:45 | +10% tenacity (CC reduction) |
| Carrier Hive | 6% | 1:45 | +8 armor pen, +8 magic pen |
| Toxin Hive | 7% | 2:00 | +3 HP regen/sec |
| Mutation Hive | 8% | 2:15 | +15% structure damage |
| Dormancy Hive | 8% | 2:15 | +200 shield every 30s |
| Replication Hive | 10% | 2:45 | +5% crit chance |

**Total potential immune health shift from Hives:** 50%

The 27% gap versus organs (77% vs 50%) is balanced by objective count asymmetry: the immune team needs 7 successful captures versus the virus team's 11. Each Hive capture is individually worth more and has a longer Festering window. Both teams also get 30% from structures (6 × 5%) and variable amounts from champion kills.

All 7 Hive effects are unique and do not repeat any organ effect — 18 distinct buffs across all objectives.

---

## 9. Organ & Hive Defenders

Every organ and Hive has a neutral defender creature that patrols its alcove. Defenders **cannot be killed — only converted.** This is a core design principle.

**Neutral behavior (default):** The defender sits at its objective alcove and attacks whoever attacks it first, like Dragon or Baron in League. It does not aggro on passing champions who don't engage it. Patrol radius: approximately 6 units.

**Objectives are tanky until their defender falls.** While the defender is active and aligned with the defending team, the objective itself has massive damage reduction. Converting the defender makes the objective squishy and vulnerable.

**Defender enrage:** All defenders enrage at 30% of their conversion bar — gaining +50% damage and +30% movement speed, turning bright red.

| Organ | Defender Creature | Style |
|-------|-------------------|-------|
| Brain (15%) | Microglial Guardian | Tanky, high damage |
| Lungs (10%) | Alveolar Warden | Balanced, space control |
| Liver (8%) | Kupffer Brute | Slow, heavy-hitting |
| Bone Marrow (8%) | Osteoclast Titan | Massive HP, slow |
| Kidneys (7%) | Mesangial Crusher | Mid-range, methodical |
| Spleen (6%) | Splenic Ravager | Fast, aggressive |
| Thymus (6%) | Thymic Instructor | Balanced |
| Stomach (5%) | Gastric Acidborn | Slow, extreme damage |
| Intestines (5%) | Gut Flora Golem | Moderate, bacterial |
| Lymph Vessels (4%) | Lymphatic Patrol | Very fast, lower HP |
| Skin (3%) | Langerhans Sentry | Weakest defender |

Hive defenders follow the same behavioral rules with virus-themed creature designs.

---

## 10. Defender Conversion Mechanic

Both teams damage a defender to bring its conversion bar down. Whichever team deals the last hit converts it. The conversion bar takes roughly 30 seconds of focused attack from one champion.

**Virus-converted organ defender:** Stays in place at its alcove. Becomes territorially aggressive toward immune champions — auto-attacks any immune player who enters its patrol radius. Ignores virus champions. The virus player who landed the last hit receives:
- A personal **Corruption Aura**: +20% organ damage from virus attacks while the slayer is within 8 units of that specific organ
- Gold, XP, and a stacking buff
- The aura affects ONLY that organ — other organs are unaffected

**Immune re-conversion:** The immune team damages the corrupted defender and lands the last hit. The defender becomes a temporary immune ally for 60 seconds — attacking virus champions, emitting a **Protection Aura** (-30% organ damage while the immune slayer is within 8 units). After 60 seconds, reverts to neutral.

**Same mechanic mirrors for Hive defenders** with Purification Aura (+20% Hive damage) and Fortification Aura (-30% Hive damage).

**When a defender is converted:** Supply lines from the 2 nearest friendly structures are severed. The converted defender operates at base stats only plus its conversion aura.

**When the slayer dies within the allowed timeframe:** The objective resets to full HP. The defender auto-reverts to neutral (no manual re-conversion needed). The health bar damage dealt during the Deteriorating/Festering phase stops immediately.

---

## 11. Slayer Mechanic — Deteriorating & Festering Phases

Every non-main objective capture creates a two-phase event: win the defender contest, then survive as the marked slayer.

**Phase 1 — Contest:** Both teams fight over the defender's last hit. The team that converts the defender gains control of the objective.

**Phase 2 — Deteriorating (organs) / Festering (Hives):** The slayer is marked. Their team gains the objective's team-wide buff. The health bar ticks toward the slayer's team at a rate that deals the full percentage over the objective's window duration. The enemy team must hunt and kill the slayer to reverse the capture.

**Binary outcome:**
- **Slayer survives the full window:** The objective is permanently destroyed. Removed from the map forever. The health bar damage is locked in. The team-wide buff expires (it was tied to the phase duration, not permanent).
- **Slayer is killed within the window:** The objective resets to full HP with its defender returned to neutral. The health bar damage dealt so far stops (no further ticking). The buff ends immediately for the whole team.

**Timeframe scales with organ/Hive value:** Lower-value objectives have shorter windows (easier to lock in, but also easier for the enemy to respond quickly). Higher-value objectives have longer windows (more total damage but more time for the enemy to organize a hunt).

**Multiple slayers can be active simultaneously.** A team that captures 3 objectives at once has 3 marked champions. The enemy must triage: which slayer is the highest priority target? A single champion can hold multiple slayer marks, making them an extremely high-value kill target.

---

## 12. The Heart — Virus Main Objective

The Heart sits deep in the immune jungle, near the immune core. It is always visible from minute 1 and attackable at all times — but extremely well-defended early on.

**Three layers of defense:**

**Layer 1 — Raw stats:** 10,000 HP, 150 armor, 150 MR, 50 HP/s regen when not in combat.

**Layer 2 — The Cardiac Sentinel:** Always active from minute 1. Fast, relentless attacks. Threat-based aggro (targets whichever virus champion has dealt the most damage to the Heart). Killing the Cardiac Sentinel grants 2,000 Bio + 600 XP (shared). Respawns after 3 minutes. Enrages at 30% HP.

**Layer 3 — Scaling defenses:** The Heart's effective defenses scale inversely with host health. At 50% (game start) = full defenses. At 10% = nearly paper. Each 10% below 50% reduces resistances by 15% and regen by 10 HP/s.

**Capturing the Heart** (depleting its HP to 0, then channeling 5 seconds) triggers the **Corruption Surge** for the virus team. See [Surge System](#14-surge-system).

---

## 13. The Infection Site — Immune Main Objective

The Infection Site sits deep in the virus jungle at equivalent depth to the Heart. It is the original infection wound — the breach point where the virus first entered the body. The immune team attacks and purifies it.

**Mirrored defenses:** 10,000 HP, 150 armor, 150 MR, 50 HP/s regen. The Infection Guardian patrols it permanently, fed by all 6 Dark Sentinels. Defense scaling mirrors the Heart but in the opposite direction (weakens as health rises toward 100%).

**Capturing the Infection Site** triggers the **Purification Surge** for the immune team.

---

## 14. Surge System

The Surge is PATHOGEN's climax mechanic. When a team captures their main objective, the entire team becomes empowered with a Surge that ticks the health bar toward victory.

**Mechanics:**
- Surge lasts 3 minutes of real time
- The health bar moves toward the capturing team's win condition at a rate calibrated to close the remaining distance in exactly 3 minutes if all 5 champions stay alive and actively engaged
- Per-second rate = (remaining_health_distance / 180) for the full team
- Each champion contributes 1/5 of the total rate
- Champions must be **actively engaged** (dealt/taken damage, used abilities, contested objectives in the last 5 seconds) for their contribution to count
- Champions in base or standing idle do not contribute — this prevents "capture and hide" strategies

**Surge interaction states:**

| State | Scenario | Result |
|-------|----------|--------|
| 1 | No Surge active, one team ACEs the other | Standard 40-second pressure window. No auto-win. |
| 2 | Surging team ACEs non-Surging team | **Immediate win** for the Surging team. |
| 3 | Non-Surging team ACEs the Surging team | Surge ends. Main objective **resets** to full HP with all defenses restored. The team that ACEd can rush the enemy's reset objective or start their own Surge by capturing their own main objective during the respawn window. |
| 4 | Both teams Surging, one ACEs the other | Surviving team's Surge continues. ACEd team's Surge ends and their objective resets. |
| 5 | Surging team partially killed (not full ACE) | Surge continues at reduced rate (fewer active champions). |

**Why State 2 creates climax:** The Surging team has a clear incentive to engage and fight (ACE = instant win). The non-Surging team has a clear incentive to prevent the full ACE (losing even 4 players is survivable if 1 stays alive). The 1v5 hero moment — one player surviving to prevent the ACE and save the game — is the most dramatic possible outcome in any MOBA.

**Biology:** The Surge represents active viral dominance (Corruption Surge) or active immune response (Purification Surge). Champions must be actively fighting for the effect to propagate — passive existence doesn't affect the body.

---

## 15. Hypothalamus — Neutral Boss

The Hypothalamus spawns near map center at Phase 3 (11:00). It is the first team-wide objective that demands coordinated response.

**Reward:** 8% health bar shift toward the capturing team + 25% ultimate CDR for the entire team for 3 minutes.

**Behavior:** Neutral boss, attacks both teams. Standard MOBA boss fight — both teams contest the area and try to secure the last hit.

---

## 16. Game Phases

Phases are timer-based. Both teams know exactly when each objective goes live. The health bar drives Body Conditions bonuses only, not objective timing.

| Phase | Time | What Phases In |
|-------|------|---------------|
| 0 | 0:00–3:00 | Nothing. Pure farming. Mission resources spawn in lanes. |
| 1 | 3:00–7:00 | Skin (3%), Lymph Vessels (4%) |
| 2 | 7:00–11:00 | Stomach (5%), Intestines (5%), Thymus (6%) |
| 3 | 11:00–15:00 | Spleen (6%), Kidneys (7%), Lungs (10%), Hypothalamus boss (8%) |
| 4 | 15:00–20:00 | Liver (8%), Bone Marrow (8%), Brain (15%) |
| 5 | 20:00+ | Endgame. All organs have phased in. Main objectives are the primary focus. |

Hives are targetable from minute 1 (they don't phase in) but their defenders are extremely strong early — attempting a Hive at level 1 is suicide. As champions level and gear up, Hive attempts become viable around Phase 2–3.

---

## 17. Missions — Phase 0 Only

During Phase 0 (0:00–3:00), mission resources spawn in each lane's clash zone alongside minion waves. One resource per wave. Both laners compete for them. Collecting one takes a 1-second animation that roots the player briefly, making them vulnerable.

**Completion:** Collect 8 to complete. Binary outcome — you have the aura or you don't.

**Mission auras (permanent for rest of game):**
- **Virus mission aura:** nearby organ defenders within 8 units take 10% more damage from all sources
- **Immune mission aura:** nearby organ defenders within 8 units gain +15 armor and +15 MR

Auras are presence-based — only active when the player is physically near the organ. Leave the radius, lose the effect. Die, lose it until you return.

**Cascade effects (utility only, no combat stats):**
- Cortex completion: brief minimap pings of enemy champions in the zone
- Pulmonary completion: +1 HP/s out-of-combat regen for the team
- Visceral completion: +2 Bio per minion kill in that lane

Mission spawns stop globally when Phase 1 begins at 3:00.

---

## 18. Body Conditions — Comeback Mechanic

The host's condition changes based on health %, granting team-wide damage modifiers:

| Health Range | Condition | Effect |
|-------------|-----------|--------|
| 0–20% | Critical | Virus +20% damage, Immune -20% damage |
| 20–40% | Sick | Virus +10% damage, Immune -10% damage |
| 40–60% | Normal | No bonuses — balanced fight |
| 60–80% | Recovering | Immune +10% damage, Virus -10% damage |
| 80–100% | Healthy | Immune +20% damage, Virus -20% damage |

This creates natural friction against snowballs. A team pushing the health bar toward an extreme faces progressively stronger opponents. The +20% damage swing at the extremes means every fight in Critical/Healthy zones is volatile — the "losing" team hits harder, creating comeback potential.

---

## 19. Minions & Frenzied Minions

**Standard minion waves:** Spawn every 30 seconds at each base. 6 minions per wave (3 melee, 3 ranged). 1 siege minion added after 15 minutes. Minions walk their lane, engage enemy minions and structures, and never enter the jungle.

**Minion purpose:** Lane economy (25 Bio + 40 XP per last-hit), structure pressure (minions damage Dark Sentinels and Sentinels), and late-game health bar pressure on open lanes.

**Frenzied Minions:** When BOTH enemy structures on a lane are destroyed, the attacking team's minions on that lane become Frenzied Minions — larger, more HP, more damage. Frenzied Minions that reach the enemy base perimeter deal direct health bar damage:
- Basic Frenzied Minion: 0.05% per minion reaching base
- Frenzied Siege Minion: 0.15% per siege minion reaching base

Over 10 minutes of a fully open lane, this amounts to roughly 8% health bar shift — meaningful but not game-dominating. Three open lanes would scale to approximately 24%.

**Minions never enter the jungle.** Converted defenders in jungle alcoves never interact with minions. Clean separation between lane gameplay and jungle gameplay.

---

## 20. Neutral Jungle Camps

The roamer's primary income source during Phase 0 and throughout the game. Camps respawn every 90 seconds.

| Camp | Location | Buff Granted (90s duration) |
|------|----------|-----------------------------|
| Red Blood Cell (RBC) | Virus-side jungle (×2) | +15% attack damage |
| White Blood Cell (WBC) | Immune-side jungle (×2) | +8 armor, +8 MR |
| Platelet | Center jungle (×2) | Temporary shield (~100 damage absorbed, 60s) |

All camps are clearable by a solo roamer at level 2–3. They provide Bio, XP, and temporary combat buffs.

---

## 21. Economy

**Bio-currency sources:**

| Source | Bio | XP |
|--------|-----|-----|
| Minion kill | 25 | 40 |
| Champion kill | 300 | 200 |
| Structure destruction | 150 | 100 |
| Organ/Hive defender conversion | 2,000 | 600 |
| Cardiac Sentinel / Infection Guardian kill | 2,000 | 600 |
| Passive income | 2/sec | — |
| First Blood (first kill of match) | +200 bonus | +100 bonus |

**Shared gold/XP:** When multiple teammates are nearby (within 12 units) during a kill, gold and XP are split evenly.

---

## 22. Mutation Shop & Tier Upgrades

Champions don't buy items. They spend Bio on champion-wide tier upgrades through the Mutation Shop. Skill rank-ups are free (come with leveling).

**Tier progression:**

```
TIER 1 (free) → POTENCY (1,200 Bio) → ALPHA or OMEGA (2,000 Bio) → APEX (3,600 Bio)
```

| Tier | Cost | Unlock Condition | Bonus |
|------|------|-----------------|-------|
| Tier 1 | Free | All skills at Rank 1 | Skills usable, +0.3 move speed per skill |
| Potency | 1,200 | All 4 skills at Rank 2 | +20% skill damage, +5% omnivamp, +0.5 move speed |
| Alpha (Branch A) | 2,000 | Potency purchased | +15% crit damage, +10% crit chance, Bleed on-hit |
| Omega (Branch B) | 2,000 | Potency purchased | +10% damage reflection, +20 MR, heal on cast |
| Apex | 3,600 | All 4 skills at Rank 3 (level 12) | +10% all scaling, +25 AD, +25 AP |

**Branch switching:** Reversible but costly. Switch from Alpha to Omega costs 2,000 Bio again. If past the branch with Apex, must also re-buy Apex (total switch: 5,600 Bio).

---

## 23. Leveling & Gene Points

**12-level system.** Max level is 12 (4 skills × 3 ranks). Each level-up grants 1 skill rank-up slot (choose which skill) + 1 Gene Point (free stat choice).

**Gene Point paths:**

| Path | HP | AD | AP | Mana | Armor | MR |
|------|-----|-----|-----|------|-------|-----|
| Endurance (Tank) | +60 | — | — | — | +3 | +2 |
| Aggression (AD) | +25 | +5 | — | — | +2 | — |
| Adaptation (AP) | +25 | — | +5 | +15 | — | +2 |

**XP requirements:**

| Level | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 |
|-------|---|---|---|---|---|---|---|---|----|----|-----|
| XP needed | 100 | 150 | 200 | 250 | 300 | 350 | 400 | 450 | 500 | 550 | 600 |
| Cumulative | 100 | 250 | 450 | 700 | 1,000 | 1,350 | 1,750 | 2,200 | 2,700 | 3,250 | 3,850 |

**Respawn formula:** `round(4.75 + 0.25 × level²)` — scales quadratically.

| Level | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 |
|-------|---|---|---|---|---|---|---|---|---|----|----|-----|
| Respawn | 5s | 6s | 7s | 9s | 11s | 14s | 17s | 21s | 25s | 30s | 35s | 41s |

---

## 24. Champion Roster — Virus Team

### Necrova (Assassin / Infiltrator)

**Flavor:** The Flesh Eater — Necrotizing Fasciitis Strain

**Passive — Tissue Decay:** All attack skills (1, 2, 3) apply 1 Decay stack on hit (lasts 6s, max 5 stacks). Champion kills decay 0.3% human health over 5s.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Tissue Melt | Projectile | 120 magic dmg, 4s CD, 40 mana | Dissolves 30% of target's armor over 3s. Applies 1 Decay stack. |
| 2 | Subdermal Tunnel | Dash | 60 magic dmg, 10s CD, 50 mana | Burrow through terrain, untargetable during dash. Exit deals AoE + 1 Decay stack. |
| 3 | Septic Bloom | AOE | 100 magic dmg, 12s CD, 60 mana | Mark area — erupts after 1.2s. 25% slow for 2s + 1 Decay stack. |
| 4 | Total Necrosis | Self-Buff | 50 AP + 25 AD/stack, 60s CD, 100 mana | Consume all Decay stacks. Enraged for 5s — true damage. Heals 25% of damage dealt. |

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| Tissue Melt | +18 dmg (→138), -0.5s CD, +3% omnivamp | +20 dmg (→158), -0.5s CD, omnivamp → 6% |
| Subdermal Tunnel | +10 AoE (→70), -1s CD, +0.5 dash | +10 AoE (→80), -1s CD, +0.5 dash |
| Septic Bloom | +15 dmg (→115), -1s CD, +0.2s slow | +15 dmg (→130), -1s CD, +0.2s slow |
| Total Necrosis | +8 AP/stack (→58), +4 AD (→29), -5s CD | +7 AP (→65), +4 AD (→33), -5s CD, heal → 30% |

### PrionMind (Mage / Controller)

**Flavor:** The Thought Corrupter — Prion-class Pathogen

**Passive — Misfolded Conversion:** Converts 2% of max mana into AP. Every 3rd ability cast on same target applies Confusion (rooted 1.5s).

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Misfolded Cascade | Projectile | 150 magic dmg, 4s CD, 40 mana | Chains up to 3 enemies. Each bounce +12% damage. |
| 2 | Neural Hijack | Target | No dmg, 10s CD, 50 mana | Reverses enemy movement controls for 1.2s. |
| 3 | Plaque Wall | AOE | No dmg, 12s CD, 55 mana | Protein wall for 3s. Blocks projectiles, 30% slow through. |
| 4 | Cognitive Collapse | AOE | 250 magic dmg, 60s CD, 100 mana | Scramble abilities in area for 3s. +8% remaining HP/s when Confused. |

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| Misfolded Cascade | +22 dmg (→172), -0.5s CD, bounce → +14% | +26 dmg (→198), -0.5s CD, bounce → +16% |
| Neural Hijack | +0.2s reverse (→1.4s), -1s CD | +0.2s reverse (→1.6s), -1s CD |
| Plaque Wall | +0.5s wall (→3.5s), -1s CD | +0.5s wall (→4s), -1s CD |
| Cognitive Collapse | +30 dmg (→280), -5s CD, +0.2s scramble | +30 dmg (→310), -5s CD, +0.3s scramble |

### Sporaxis (Tank / Bruiser)

**Flavor:** The Living Colony — Fungal Overgrowth

**Passive — Mycelial Network:** Converts 1.5% of max HP into AD. +2 armor and +1 MR per nearby allied minion (max 4 stacks).

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Mycelial Slam | AOE | 80 phys dmg, 8s CD, 40 mana | Ground pound. First enemy rooted 1s. |
| 2 | Spore Shield | Self-Buff | 15% max HP shield, 10s CD, 50 mana | Shield 4s. On break: toxic cloud, allies +10 MR for 3s. |
| 3 | Parasitic Latch | Target | No dmg, 14s CD, 60 mana | Attach to enemy 1.5s — drain 10% of their AD and armor for 5s. |
| 4 | Colony Eruption | Self-Buff | 40 magic dmg/clone, 60s CD, 100 mana | Split into 3 mini-clones (25% stats) for 5s. Explode on death. |

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| Mycelial Slam | +12 dmg (→92), -0.5s CD, +0.1s root | +13 dmg (→105), -0.5s CD, +0.1s root |
| Spore Shield | +2% shield (→17%), -1s CD, MR → +12 | +2% shield (→19%), -1s CD, MR → +15 |
| Parasitic Latch | -1s CD, +0.1s attach, drain → 12% | -1s CD, +0.1s attach, drain → 14% |
| Colony Eruption | +6 dmg (→46), -5s CD, stats → 28% | +6 dmg (→52), -5s CD, stats → 30% |

### Vector9 (Marksman / Spread)

**Flavor:** The Patient Zero — Airborne Contagion

**Passive — Contagion Index:** Auto-attacks mark unique enemies as Infected (6s). Each Infected target grants +4% attack speed (max 4 stacks = +16%). With 3+ Infected targets, human health drops 0.1%/s.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Viral Volley | Projectile | 100 phys dmg, 4s CD, 35 mana | Applies 1 Infection stack. At 4 stacks: burst 60 bonus magic + spread. |
| 2 | Mutation Drift | Dash | 10s CD, 45 mana | Dash leaving particle trail 2s. Enemies crossing gain 1 Infection. |
| 3 | Incubation Zone | AOE | 20 magic dmg, 14s CD, 50 mana | Spore ward. Explodes on contact: AoE + vision 3s. |
| 4 | Pandemic Protocol | Self-Buff | 60s CD, 100 mana | Mark all visible enemies 5s. 100% Infection transfer + +15% crit. |

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| Viral Volley | +15 dmg (→115), -0.5s CD, burst → 70 | +15 dmg (→130), -0.5s CD, burst → 80 |
| Mutation Drift | -1s CD, +0.5 dash, +0.5s trail | -1s CD, +0.5 dash, +0.5s trail |
| Incubation Zone | +5 dmg (→25), -1s CD, +1s vision | +5 dmg (→30), -1s CD, +1s vision |
| Pandemic Protocol | -5s CD, +0.5s mark, crit → +18% | -5s CD, +0.5s mark, crit → +20% |

---

## 25. Champion Roster — Immune Team

### Phagorath (Tank / Bruiser)

**Flavor:** The Devourer — Macrophage Titan

**Passive — Phagocytosis:** Minion kills grant +3 max HP permanently (max 80 stacks = +240 HP). Champion kills grant +12 max HP (no cap). Converts 2% of max HP into AD.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Engulf | Target | 1/6 max HP + 3% target HP, 6s CD, 40 mana | Suppress 1.2s. Regen 2% max HP during suppress. |
| 2 | Cytoplasm Crush | AOE | 70 phys dmg, 14s CD, 50 mana | Barrier zone — allies inside take 20% reduced damage 3s. |
| 3 | Chemotaxis Charge | Dash | 60 phys dmg, 10s CD, 45 mana | Dash through enemies. First hit slowed 40% 2s, gain +8 armor 4s. |
| 4 | Immune Cascade | AOE | 120 magic dmg, 60s CD, 100 mana | Push enemies out, 30% slow 2s. Heal allies 10% max HP. |

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| Engulf | -1s CD (→5s), target HP → 3.5%, +0.1s suppress, regen → 2.5% | -1s CD (→4s), target HP → 4%, +0.1s suppress, regen → 3% |
| Cytoplasm Crush | +10 dmg (→80), -1s CD, reduction → 22% | +10 dmg (→90), -1s CD, reduction → 25% |
| Chemotaxis Charge | +10 dmg (→70), -1s CD, armor → +10 | +10 dmg (→80), -1s CD, armor → +12 |
| Immune Cascade | +15 dmg (→135), -5s CD, heal → 12% | +15 dmg (→150), -5s CD, heal → 14% |

### KyllexT (Assassin / Hunter)

**Flavor:** The Precision Strike — Killer T-Cell

**Passive — Adaptive Memory:** First time KyllexT damages each unique enemy, they are Tagged. Tagged champions take +10% damage from KyllexT permanently. Champion kills restore 0.2% human health.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Antigen Lock | Target | +20 true dmg/hit, 8s CD, 40 mana | Mark 4s — next 3 autos deal +20 true damage + 30% slow 1s. |
| 2 | Rapid Response | Dash | 55 phys dmg, 10s CD, 45 mana | Dash through enemy. Applies Exposed (+8% damage taken 3s). |
| 3 | Perforin Burst | AOE | 3% target max HP, 12s CD, 55 mana | Cone of enzymes dealing 3% target max HP as magic damage over 3s. |
| 4 | Adaptive Recall | Self-Buff | 60s CD, 100 mana | Passive: +3 AD per unique kill. Active: +30% AS vs Exposed 4s. |

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| Antigen Lock | +5 true dmg (→+25), -0.5s CD, +0.1s slow | +5 true dmg (→+30), -0.5s CD, +0.1s slow |
| Rapid Response | +8 dmg (→63), -1s CD, Exposed → +9% | +8 dmg (→71), -1s CD, Exposed → +10% |
| Perforin Burst | +0.5% HP (→3.5%), -1s CD | +0.5% HP (→4%), -1s CD |
| Adaptive Recall | -5s CD, AS → +35%, AD/kill → +4 | -5s CD, AS → +40%, AD/kill → +5 |

### SeraB (Support / Healer)

**Flavor:** The Antibody Weaver — B-Cell Architect

**Passive — Antibody Synthesis:** Converts 2% of max mana into AP. Healing or shielding an ally restores 0.1% human health (8s CD). Allies within 6 units regen +0.5 HP/s.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Antibody Tether | Target | 3 HP/s regen, 10s CD, 40 mana | Link to ally 10s — both regen. SeraB absorbs 20% of ally damage. |
| 2 | Plasma Infusion | Target | 80 HP heal, 8s CD, 50 mana | Heal over 3s. Overhealing → shield (max 40 HP, 4s). |
| 3 | Opsonize | Target | No dmg, 12s CD, 55 mana | Tag enemy — +10% damage from all sources, revealed 3s. |
| 4 | Herd Immunity | AOE | 60s CD, 100 mana | Allies share collective HP pool for 2s. Damage split evenly. |

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| Antibody Tether | +1 HP/s (→4), -1s CD, absorb → 23% | +1 HP/s (→5), -1s CD, absorb → 25%, link → 12s |
| Plasma Infusion | +12 heal (→92), -0.5s CD, shield → 48 | +13 heal (→105), -0.5s CD, shield → 55 |
| Opsonize | -1s CD, +0.5s reveal, amp → +11% | -1s CD, +0.5s reveal, amp → +12% |
| Herd Immunity | -5s CD, +0.3s pool (→2.3s) | -5s CD, +0.2s pool (→2.5s) |

### Pyrexia (Mage / Area Denial)

**Flavor:** The Fever Engine — Inflammatory Response

**Passive — Fever Response:** All magic damage applies a stack. Every 3rd stack applies Burn (1.5% damage dealt over 2s). At 3 stacks, target takes +6% magic damage. Ability hits on Burnt enemies recover 0.05% human health.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Fever Spike | Projectile | 80 magic dmg, 4s CD, 35 mana | 15% stacking slow. At 4 stacks: silenced 1s. |
| 2 | Inflammation Zone | AOE | 20 magic dmg/s, 14s CD, 55 mana | Burning area 4s. Allies: +10% AS. Enemies: 20 dmg/s. |
| 3 | Thermal Barrier | AOE | No dmg, 14s CD, 50 mana | Heat wall 3s — blocks vision, deflects first projectile. |
| 4 | Hyperthermia | AOE | 1.5% max HP/s, 60s CD, 100 mana | Map-wide: all virus burn 1.5% HP/s for 8s. Below 30% host HP: 3%/s. |

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| Fever Spike | +12 dmg (→92), -0.5s CD, slow → 17% | +13 dmg (→105), -0.5s CD, slow → 19%, silence → 1.1s |
| Inflammation Zone | +4 dmg/s (→24), -1s CD, AS → +12% | +4 dmg/s (→28), -1s CD, AS → +15% |
| Thermal Barrier | -1s CD, +0.5s wall (→3.5s) | -1s CD, +0.5s wall (→4s) |
| Hyperthermia | +0.25% HP/s (→1.75%), -5s CD, +1s (→9s) | +0.25% HP/s (→2.0%), -5s CD, +1s (→10s) |

---

## 26. Stat Glossary

| Stat | Description |
|------|-------------|
| Attack Damage (AD) | Physical damage per auto-attack |
| Ability Power (AP) | Scales magic ability damage |
| Armor | Reduces physical damage: reduction = armor / (armor + 100) |
| Magic Resist (MR) | Reduces magic damage: reduction = MR / (MR + 100) |
| Omnivamp | Heal for X% of all damage dealt |
| Damage Reflection | Reflect X% of damage taken back to attacker |
| Crit Chance | Chance for auto-attacks to deal 150% damage |
| Cooldown Reduction (CDR) | Reduces skill cooldowns by X% |
| Health Regen | HP recovered per second |
| Mana Regen | Mana recovered per second |
| Move Speed | Units moved per second |
| Attack Speed | Auto-attacks per second |
| Tenacity | Reduces crowd control duration by X% |
| Armor/Magic Penetration | Ignores X flat armor/MR on the target |

---

## 27. Strategy Guide

### Virus Team

**Phase 0 (0:00–3:00):** Farm minions for Bio and XP. Compete for mission resources. Roamer clears RBC and Platelet camps for AD buff and shield. Look for early gank opportunities — First Blood gives +200 Bio.

**Phase 1–2 (3:00–11:00):** Contest early organs (Skin, Lymph Vessels) for small health shifts and team buffs. Push Sentinels for 5% health shift each + severed supply lines to organ defenders. The Visceral lane becomes a hotspot in Phase 2 when Stomach and Intestines appear simultaneously.

**Phase 3 (11:00–15:00):** Contest the Hypothalamus for 8% swing + ult CDR. Target Lungs (10%) as the highest-value organ available. Defend Hives from immune invasion — losing a Hive gives the immune team health shift + team buff.

**Phase 4 (15:00–20:00):** Brain (15%) is the game-warping objective. Assess Heart viability — how many Sentinels are down? If 4+ are destroyed, the Cardiac Sentinel is severely weakened.

**Phase 5 (20:00+):** Decide: grind remaining organs to push health toward 0%, or attempt the Heart. Remember that capturing the Heart triggers a Surge, not an instant win — you still need to ACE the enemy during Surge for the decisive victory.

### Immune Team

**Phase 0 (0:00–3:00):** Mirror the virus team's farming. Complete missions for defender-buffing auras. Roamer clears WBC camps for defensive buffs.

**Phase 1–2 (3:00–11:00):** Defend organs by killing virus slayers during Deteriorating phases. Push Dark Sentinels for 5% health shift + severed supply lines to Hive defenders. Begin Hive invasion when power spikes allow.

**Phase 3 (11:00–15:00):** Contest Hypothalamus. Invade virus jungle for Hive captures — each Hive destroyed gives health shift + unique team buff. Defend Lungs (10%) as the highest-priority organ.

**Phase 4 (15:00–20:00):** Defend Brain (15%). Assess Infection Site viability — how many Dark Sentinels are down? Continue Hive destruction for permanent virus debuffs.

**Phase 5 (20:00+):** Push for the Infection Site when enough Dark Sentinels are destroyed. The Purification Surge mirrors the Corruption Surge — ACE the virus team during your Surge for the decisive win.

### Economy Strategy

- Never miss minion kills — Bio is the only currency for tier upgrades
- Rush Potency (1,200 Bio) once all skills hit Rank 2
- Choose Alpha (carry/assassin) vs Omega (tank/support) based on matchup
- Champion kills are worth 300 Bio (12× a minion) but split among nearby allies
- Objective defender conversions give 2,000 Bio — the biggest single reward in the game
- Solo farming gives full gold; grouping splits income but creates pressure

---

## 28. Post-MVP — Adaptive Biomes

**Status: Deferred to Season 2.**

Biomes are dynamic map modifiers tied to the host's personality, age, and medical conditions. The base game's 11 organs, 7 Hives, Body Conditions system, 6 game phases, Surge mechanic, and defender conversion already create enormous match-to-match variety. Adding biomes multiplies QA burden without proportional player value in early game lifecycle.

**Planned biomes:**

| Biome | Host Condition | Key Effects |
|-------|---------------|-------------|
| The Cortisol Inferno | Chronic Anxiety + Depression | Unstable terrain, -30% healing, panic waves, fog shift |
| The Synaptic Storm | Child Prodigy / Hyperactive | +40% move speed, lanes rewire, phantasms, dopamine burst |
| The Iron Current | Elderly / Cardiovascular | Slow current, plaque walls, heart pulses, -15% immune base |
| The Radiant Wellspring | Athlete / Peak Condition | Immune favoured, WBC patrols, O2 regen, stealth meta |

**Implementation:** Ranked = procedurally generated hosts. Casual/custom = virus team selects infection type, determining host profile. Seasonal rotation of biome subsets into ranked pool.

---

*PATHOGEN — Every Cell Counts*
