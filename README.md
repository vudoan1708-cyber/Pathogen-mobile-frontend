# PATHOGEN — Game Design Document

**The Body is the Battlefield**

A 5v5 mobile 3D MOBA set inside a human body. The Virus team invades and drains the host. The Immune team defends and flushes the infection out. The win condition is a central Human Health Bar — an abstract tug-of-war gauge that replaces the traditional destructible Nexus found in other MOBAs.

---

## Table of Contents

1. [Core Concept & USPs](#1-core-concept--usps)
2. [Win Conditions](#2-win-conditions)
3. [Map Layout](#3-map-layout)
4. [Lanes & Roles](#4-lanes--roles)
5. [Structures — Dark Sentinels & Sentinels](#5-structures--dark-sentinels--sentinels)
6. [Supply Lines](#6-supply-lines)
7. [Organs — 8 Immune-Side Objectives](#7-organs--8-immune-side-objectives)
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

**What makes PATHOGEN different:**

- **The health bar IS the Nexus.** Starts at 50%. Reaches 0% (virus wins) or 100% (immune wins).
- **Asymmetric objectives, symmetric aggression.** Virus attacks organs. Immune attacks Hives.
- **No passive buffs at game start.** Perfect parity.
- **Supply lines from structures to defenders.** Lane wins weaken enemy objective defenders.
- **Defender conversion, not killing.** Defenders can only be converted through a last-hit system.
- **Slayer mechanic.** Last-hit champion carries team buff and becomes a marked hunt target.
- **Surge system.** Main objective capture triggers team-wide Surge. ACEing during Surge = immediate win.
- **Bases are sanctuaries, not targets.** Frenzied Minions on open lanes deal health bar damage directly.
- **Wards earned through missions, not purchased.**

---

## 2. Win Conditions

The Human Health Bar starts at 50%.

**Virus wins when:** Health bar reaches 0%.

**Immune wins when:** Health bar reaches 100%.

**Surge ACE win:** If a Surging team fully ACEs the non-Surging team, the game ends immediately.

---

## 3. Map Layout

**Shape:** Symmetric rectangle. Equal jungle, lane lengths, and camp counts on both sides.

**Axis:** Outside-to-inside. Virus base left (skin surface). Immune core right (body interior). Virus pushes right. Immune pushes left.

**Content asymmetry:** Virus jungle (left) = 7 Hives + Infection Site. Immune jungle (right) = 8 organs + Heart.

**Jungle zones:** Upper jungle between Cortex and Pulmonary lanes. Lower jungle between Pulmonary and Visceral lanes.

**Main objective placement:** Heart and Infection Site sit deep in the lower jungle (between Pulmonary and Visceral) on their respective sides. 8–10 seconds of committed jungle pathing required. Prevents casual attempts.

**Hypothalamus:** Spawns in the upper jungle near map center. Opposite side from main objectives, forcing teams to split attention.

**High-value objective placement:** Highest-value organs and Hives sit in off-lane alcoves just behind the inner (2nd tier) structure on their nearest lane, angled slightly toward the top or bottom edge of the map. Brain (15%) in an alcove above S2 on Cortex. Replication Hive (10%) in an alcove above D2 on Cortex. Lungs (10%) near S2 Pulmonary. Mutation Hive (8%) near D2 Pulmonary. Liver (8%) near S2 Visceral. Dormancy Hive (8%) near D2 Visceral.

**Mid and low-value objectives:** Distributed through jungle between lanes.

---

## 4. Lanes & Roles

| Lane | Position | Role | Body Region |
|------|----------|------|-------------|
| Cortex | Top | Solo | Head / nervous system |
| Pulmonary | Mid | Solo | Chest / respiratory |
| Visceral | Bot | Duo (ADC + Support) | Abdomen / digestive |

**The Roamer (Jungler):** Farms neutral camps, ganks lanes, contests jungle objectives, guards main objectives.

**First Blood:** First champion kill grants +200 Bio and +100 XP.

---

## 5. Structures — Dark Sentinels & Sentinels

Virus structures = **Dark Sentinels** (D). Immune structures = **Sentinels** (S). Identical behavior.

**Per lane:** 2 Dark Sentinels + 2 Sentinels = 4 per lane, 12 total.

**Behavior:** First-in-range targeting. Champion aggro override. Homing projectiles (80–100 dmg, 0.88 atk/s). Escalating true damage after 3 consecutive shots on same champion. HP: 1,500–2,000. Must be destroyed in order.

**Destruction:** 5% health bar shift + 150 Bio + 100 XP (shared nearby).

---

## 6. Supply Lines

Structures feed biological connections to nearby defenders.

**Sentinels** buff the 2 nearest organ defenders. **Dark Sentinels** buff the 2 nearest Hive defenders. Each surviving structure grants: **+16% bonus HP, damage, and DR** to connected defenders.

**Main objectives:** Cardiac Sentinel fed by ALL 6 Sentinels globally. Infection Guardian fed by ALL 6 Dark Sentinels globally.

**On conversion:** Supply lines from 2 nearest structures severed. Reconnect when defender reverts neutral. Structure destruction permanently removes the source.

---

## 7. Organs — 8 Immune-Side Objectives

Organs sit in the immune jungle. Virus team's attack targets. Dormant until their phase — defenders patrol even while dormant.

| Organ | Health Shift | Deteriorating Window | Team Effect (virus captures) |
|-------|-------------|---------------------|------------------------------|
| Intestines | 5% | 1:30 | +5% healing received |
| Thymus | 6% | 1:45 | +5% ability damage |
| Spleen | 6% | 1:45 | +8% attack speed |
| Kidneys | 7% | 2:00 | +5% cooldown reduction |
| Liver | 8% | 2:15 | +5% damage reduction |
| Bone Marrow | 8% | 2:15 | +8% max HP |
| Lungs | 10% | 2:30 | +15 flat AD, +15 flat AP |
| Brain | 15% | 3:00 | +10% all damage |

**Total virus health shift from organs:** 65%.

---

## 8. Hives — 7 Virus-Side Objectives

Hives sit in the virus jungle. Immune team's attack targets. No passive buffs — Hives grant nothing at game start.

| Hive | Health Shift | Festering Window | Team Effect (immune captures) |
|------|-------------|-----------------|-------------------------------|
| Catalyst Hive | 5% | 1:30 | +3 mana regen/sec |
| Resistance Hive | 6% | 1:45 | +10% tenacity (CC reduction) |
| Carrier Hive | 6% | 1:45 | +8 armor pen, +8 magic pen |
| Toxin Hive | 7% | 2:00 | +3 HP regen/sec |
| Mutation Hive | 8% | 2:15 | +15% structure damage |
| Dormancy Hive | 8% | 2:15 | +200 shield every 30s |
| Replication Hive | 10% | 2:45 | +5% crit chance |

**Total immune health shift from Hives:** 50%.

---

## 9. Organ & Hive Defenders

Every organ and Hive has a neutral defender. **Cannot be killed — only converted.**

**Neutral:** Attacks whoever hits it first. ~6-unit patrol radius.

**Objectives are tanky until defender falls.** Once converted, the objective becomes squishy.

**Enrage:** 30% conversion bar — +50% damage, +30% MS, turns red.

| Organ | Defender | Style |
|-------|---------|-------|
| Brain (15%) | Microglial Guardian | Tanky, high damage |
| Lungs (10%) | Alveolar Warden | Balanced, space control |
| Liver (8%) | Kupffer Brute | Slow, heavy-hitting |
| Bone Marrow (8%) | Osteoclast Titan | Massive HP, slow |
| Kidneys (7%) | Mesangial Crusher | Mid-range, methodical |
| Spleen (6%) | Splenic Ravager | Fast, aggressive |
| Thymus (6%) | Thymic Instructor | Balanced |
| Intestines (5%) | Gut Flora Golem | Moderate, bacterial |

Hive defenders follow identical rules with virus-themed designs.

---

## 10. Defender Conversion Mechanic

Both teams damage defender. Last hit converts it. ~30 seconds focused attack.

**Virus-converted organ defender:** Stays in alcove. Attacks immune champions in patrol radius. Ignores virus. Last-hitter gets:
- Personal **Corruption Aura**: +20% organ damage within 8 units (that organ only)
- Gold, XP, stacking buff

**Immune re-conversion:** Last hit on corrupted defender. 60-second immune ally with **Protection Aura** (-30% organ damage). Then reverts neutral.

**Hive mirrors:** Purification Aura (+20% Hive damage) / Fortification Aura (-30% Hive damage).

**On slayer death:** Objective resets to full, defender auto-reverts neutral.

---

## 11. Slayer Mechanic — Deteriorating & Festering Phases

Every non-main objective capture = two-phase event.

**Phase 1 — Contest:** Fight for defender last hit.

**Phase 2 — Deteriorating (organs) / Festering (Hives):** Slayer marked. Team gets buff. Health bar ticks. Enemy hunts slayer.

**Outcomes:**
- **Slayer survives full window:** Objective permanently destroyed. Health shift locked. Buff expires.
- **Slayer killed:** Objective resets, defender reverts neutral. Health damage stops. Buff ends.

Multiple simultaneous slayers allowed — one champion can carry multiple marks.

---

## 12. The Heart — Virus Main Objective

Deep in immune-side lower jungle, between Pulmonary and Visceral lanes. 8–10 seconds of jungle pathing. Always visible. Attackable from minute 1.

**Defense layers:**
- **Stats:** 10,000 HP, 150 armor, 150 MR, 50 HP/s regen
- **Cardiac Sentinel:** Always active. Threat-based aggro. 2,000 Bio + 600 XP on kill. 3-min respawn. Enrages at 30%.
- **Scaling:** Weakens as host health drops. Each 10% below 50% = -15% resistances, -10 HP/s regen.

**Capture:** HP to 0 → 5-second channel → **Corruption Surge**.

---

## 13. The Infection Site — Immune Main Objective

Mirrors Heart in virus-side lower jungle. The original infection wound.

**Mirrored defenses:** 10,000 HP, 150/150 resistances. Infection Guardian fed by all 6 Dark Sentinels. Weakens as health rises toward 100%.

**Capture triggers Purification Surge.**

---

## 14. Surge System

Capturing a main objective triggers team-wide Surge.

**Mechanics:**
- 3 minutes real time
- Rate = remaining_health_distance / 180 (closes the gap in exactly 3 min with full team)
- Each of 5 champions contributes 1/5 of rate
- Must be **actively engaged** (damage dealt/taken, abilities used in last 5s) — idle or in-base = no contribution

**States:**

| # | Scenario | Result |
|---|----------|--------|
| 1 | No Surge, ACE | 40s pressure window. No auto-win. |
| 2 | **Surging team ACEs non-Surging** | **Immediate win.** |
| 3 | Non-Surging ACEs Surging | Surge ends. Objective resets. Can capture own objective for reverse Surge. |
| 4 | Both Surging, one ACEs other | Surviving team wins (same as State 2). |
| 5 | Partial kill of Surging team | Surge continues at reduced rate. |

**The 1v5 MVP moment:** One surviving non-Surging champion prevents the instant-win.

---

## 15. Hypothalamus — Neutral Boss

Upper jungle (between Cortex and Pulmonary), near map center. Phase 3 (11:00).

**Reward:** 8% health shift + 25% ult CDR team-wide for 3 minutes.

---

## 16. Game Phases

Timer-based.

| Phase | Time | Objectives |
|-------|------|-----------|
| 0 | 0:00–3:00 | None. Farming + missions. |
| 1 | 3:00–7:00 | Intestines (5%), Thymus (6%) |
| 2 | 7:00–11:00 | Spleen (6%), Kidneys (7%) |
| 3 | 11:00–15:00 | Bone Marrow (8%), Lungs (10%), Hypothalamus (8%) |
| 4 | 15:00–20:00 | Liver (8%), Brain (15%) |
| 5 | 20:00+ | Endgame. Main objective focus. |

Hives targetable from minute 1 but defenders are strong early.

---

## 17. Missions — Phase 0 Only

During Phase 0 (0:00–3:00), resources spawn in lane clash zones and jungle camps. Each lane and the jungle has its own mission with a unique reward. First to collect 8 wins at full value. The opponent can still complete for 60% effectiveness. Hard cutoff at 4:00.

### Cortex Mission — TEAM-WIDE — Wards

The **only source of wards in the game**.

- **Winning team:** 2 wards per player
- **Losing team:** 1 ward per player

Wards: visible, 3 hits to destroy, permanent until destroyed or replaced. 120-second refresh per charge. Max active = charge count.

### Pulmonary Mission — INDIVIDUAL — HP Regen

- **Winning laner:** +2 HP/s out-of-combat regen
- **Losing laner:** +1 HP/s

### Visceral Mission — INDIVIDUAL (Duo) — Bio Income

Both ADC and support collect independently.

- **Winning duo:** +3 Bio per minion kill in their lane (each)
- **Losing duo:** +1 Bio per minion kill (each)

### Roamer Mission — INDIVIDUAL — Jungle Movement Speed

Roamer collects from jungle camps instead of lane resources. Each camp cleared during Phase 0 drops one collectible. Both roamers compete over the same camps. First to collect 5 wins.

- **Winning roamer:** +5% movement speed in jungle territory
- **Losing roamer:** +3% movement speed in jungle

---

## 18. Body Conditions — Comeback Mechanic

| Health Range | Condition | Effect |
|-------------|-----------|--------|
| 0–20% | Critical | Virus +20% dmg, Immune -20% |
| 20–40% | Sick | Virus +10%, Immune -10% |
| 40–60% | Normal | No bonuses |
| 60–80% | Recovering | Immune +10%, Virus -10% |
| 80–100% | Healthy | Immune +20%, Virus -20% |

---

## 19. Minions & Frenzied Minions

**Waves:** 6 every 30s (3 melee, 3 ranged). +1 siege after 15 min. Lane-only. Never enter jungle.

**Frenzied Minions:** Both enemy structures on a lane destroyed = Frenzied upgrade. Larger, more HP/damage. Health bar damage at enemy base:
- Basic Frenzied: 0.05% per minion
- Frenzied Siege: 0.15% per minion

~8% shift over 10 min of one open lane. Three open = ~24%.

---

## 20. Neutral Jungle Camps

Respawn every 90 seconds.

| Camp | Location | Buff (90s) |
|------|----------|-----------|
| RBC | Virus jungle (x2) | +15% AD |
| WBC | Immune jungle (x2) | +8 armor, +8 MR |
| Platelet | Center (x2) | Shield (~100 dmg, 60s) |

---

## 21. Economy

| Source | Bio | XP |
|--------|-----|-----|
| Minion kill | 25 | 40 |
| Champion kill | 300 | 200 |
| Structure destroyed | 150 | 100 |
| Defender conversion | 2,000 | 600 |
| Cardiac/Infection Guardian kill | 2,000 | 600 |
| Passive income | 2/sec | — |
| First Blood | +200 | +100 |

Shared when teammates within 12 units.

---

## 22. Mutation Shop & Tier Upgrades

No items. Bio spent on tier upgrades.

```
TIER 1 (free) → POTENCY (1,200) → ALPHA or OMEGA (2,000) → APEX (3,600)
```

| Tier | Cost | Condition | Bonus |
|------|------|-----------|-------|
| Tier 1 | Free | All skills Rank 1 | Skills usable, +0.3 MS/skill |
| Potency | 1,200 | All Rank 2 | +20% skill dmg, +5% omnivamp, +0.5 MS |
| Alpha | 2,000 | Potency | +15% crit dmg, +10% crit, Bleed |
| Omega | 2,000 | Potency | +10% reflect, +20 MR, heal on cast |
| Apex | 3,600 | All Rank 3 | +10% scaling, +25 AD, +25 AP |

Switch branches: 2,000 Bio (+3,600 if Apex = 5,600 total).

---

## 23. Leveling & Gene Points

12-level max. Each level: 1 skill rank-up + 1 Gene Point.

| Path | HP | AD | AP | Mana | Armor | MR |
|------|-----|-----|-----|------|-------|-----|
| Endurance | +60 | — | — | — | +3 | +2 |
| Aggression | +25 | +5 | — | — | +2 | — |
| Adaptation | +25 | — | +5 | +15 | — | +2 |

| Level | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 |
|-------|---|---|---|---|---|---|---|---|----|----|-----|
| Total XP | 100 | 250 | 450 | 700 | 1K | 1.35K | 1.75K | 2.2K | 2.7K | 3.25K | 3.85K |

**Respawn:** `round(4.75 + 0.25 × level²)` — 5s at lvl 1, 41s at lvl 12.

---

## 24. Champion Roster — Virus Team

### Necrova — Assassin / Infiltrator

**The Flesh Eater — Necrotizing Fasciitis.** Passive: attack skills apply Decay stacks (6s, max 5). Kills decay 0.3% health over 5s.

| # | Name | Type | Stats | Description |
|---|------|------|-------|-------------|
| 1 | Tissue Melt | Projectile | 120 magic, 4s, 40 mana | Dissolve 30% armor 3s. +1 Decay. |
| 2 | Subdermal Tunnel | Dash | 60 magic, 10s, 50 mana | Untargetable burrow. AoE exit +1 Decay. |
| 3 | Septic Bloom | AOE | 100 magic, 12s, 60 mana | Erupts 1.2s. 25% slow 2s +1 Decay. |
| 4 | Total Necrosis | Buff | 50AP+25AD/stack, 60s, 100 mana | Consume Decay. True dmg 5s. Heal 25%. |

### PrionMind — Mage / Controller

**The Thought Corrupter — Prion-class.** Passive: 2% mana→AP. Every 3rd cast on same target: root 1.5s.

| # | Name | Type | Stats | Description |
|---|------|------|-------|-------------|
| 1 | Misfolded Cascade | Projectile | 150 magic, 4s, 40 mana | Chains 3. Each bounce +12%. |
| 2 | Neural Hijack | Target | 10s, 50 mana | Reverse controls 1.2s. |
| 3 | Plaque Wall | AOE | 12s, 55 mana | Protein wall 3s. Blocks, 30% slow. |
| 4 | Cognitive Collapse | AOE | 250 magic, 60s, 100 mana | Scramble abilities 3s. +8% HP/s. |

### Sporaxis — Tank / Bruiser

**The Living Colony — Fungal Overgrowth.** Passive: 1.5% HP→AD. +2 armor/+1 MR per nearby minion (max 4).

| # | Name | Type | Stats | Description |
|---|------|------|-------|-------------|
| 1 | Mycelial Slam | AOE | 80 phys, 8s, 40 mana | Ground pound. Root 1s. |
| 2 | Spore Shield | Buff | 15% HP shield, 10s, 50 mana | 4s. On break: +10 MR cloud 3s. |
| 3 | Parasitic Latch | Target | 14s, 60 mana | Attach 1.5s. Drain 10% AD+armor 5s. |
| 4 | Colony Eruption | Buff | 40 magic/clone, 60s, 100 mana | 3 clones (25% stats) 5s. Explode. |

### Vector9 — Marksman / Spread

**The Patient Zero — Airborne Contagion.** Passive: autos mark Infected (6s). +4% AS/target (max +16%). 3+ infected: 0.1% health drain/s.

| # | Name | Type | Stats | Description |
|---|------|------|-------|-------------|
| 1 | Viral Volley | Projectile | 100 phys, 4s, 35 mana | +1 Infection. At 4: burst 60 magic+spread. |
| 2 | Mutation Drift | Dash | 10s, 45 mana | Trail 2s. Crossing +1 Infection. |
| 3 | Incubation Zone | AOE | 20 magic, 14s, 50 mana | Spore ward. Explodes: AoE+vision 3s. |
| 4 | Pandemic Protocol | Buff | 60s, 100 mana | Mark all 5s. 100% transfer+15% crit. |

---

## 25. Champion Roster — Immune Team

### Phagorath — Tank / Bruiser

**The Devourer — Macrophage Titan.** Passive: minion kills +3 HP (max 80=+240). Champ kills +12 HP. 2% HP→AD.

| # | Name | Type | Stats | Description |
|---|------|------|-------|-------------|
| 1 | Engulf | Target | 1/6 HP+3% target, 6s, 40 mana | Suppress 1.2s. Regen 2% HP. |
| 2 | Cytoplasm Crush | AOE | 70 phys, 14s, 50 mana | Barrier: allies -20% dmg 3s. |
| 3 | Chemotaxis Charge | Dash | 60 phys, 10s, 45 mana | 40% slow 2s, +8 armor 4s. |
| 4 | Immune Cascade | AOE | 120 magic, 60s, 100 mana | Push out, 30% slow. Heal 10% HP. |

### KyllexT — Assassin / Hunter

**The Precision Strike — Killer T-Cell.** Passive: first dmg to each enemy = Tagged (+10% from KyllexT). Kills +0.2% health.

| # | Name | Type | Stats | Description |
|---|------|------|-------|-------------|
| 1 | Antigen Lock | Target | +20 true/hit, 8s, 40 mana | Mark 4s. 3 autos +true+30% slow. |
| 2 | Rapid Response | Dash | 55 phys, 10s, 45 mana | Exposed +8% dmg taken 3s. |
| 3 | Perforin Burst | AOE | 3% target HP, 12s, 55 mana | Cone: magic over 3s. |
| 4 | Adaptive Recall | Buff | 60s, 100 mana | +3 AD/unique kill. Active: +30% AS 4s. |

### SeraB — Support / Healer

**The Antibody Weaver — B-Cell Architect.** Passive: 2% mana→AP. Heal/shield restores 0.1% health (8s CD). Nearby allies +0.5 HP/s.

| # | Name | Type | Stats | Description |
|---|------|------|-------|-------------|
| 1 | Antibody Tether | Target | 3 HP/s, 10s, 40 mana | Link 10s. Both regen. Absorb 20%. |
| 2 | Plasma Infusion | Target | 80 heal, 8s, 50 mana | 3s heal. Overheal→shield 40 HP. |
| 3 | Opsonize | Target | 12s, 55 mana | +10% dmg from all, revealed 3s. |
| 4 | Herd Immunity | AOE | 60s, 100 mana | Shared HP pool 2s. Dmg split. |

### Pyrexia — Mage / Area Denial

**The Fever Engine — Inflammatory Response.** Passive: magic stacks. 3rd: Burn 1.5% over 2s +6% magic dmg. Burns recover 0.05% health.

| # | Name | Type | Stats | Description |
|---|------|------|-------|-------------|
| 1 | Fever Spike | Projectile | 80 magic, 4s, 35 mana | 15% stacking slow. At 4: silence 1s. |
| 2 | Inflammation Zone | AOE | 20/s, 14s, 55 mana | 4s. Allies +10% AS. Enemies 20 dmg/s. |
| 3 | Thermal Barrier | AOE | 14s, 50 mana | Heat wall 3s. Blocks, deflects 1st. |
| 4 | Hyperthermia | AOE | 1.5% HP/s, 60s, 100 mana | Map-wide 8s. Below 30%: 3%/s. |

---

## 26. Stat Glossary

| Stat | Description |
|------|-------------|
| AD | Physical auto-attack damage |
| AP | Magic ability scaling |
| Armor | Phys reduction: armor/(armor+100) |
| MR | Magic reduction: MR/(MR+100) |
| Omnivamp | Heal X% of all damage dealt |
| Damage Reflection | Reflect X% taken |
| Crit Chance | 150% auto-attack chance |
| CDR | Skill cooldown reduction |
| HP/Mana Regen | Per-second recovery |
| Move Speed | Units/second |
| Attack Speed | Attacks/second |
| Tenacity | CC duration reduction |
| Armor/Magic Pen | Ignores X flat resistance |

---

## 27. Strategy Guide

### Virus Team

**Phase 0:** Farm. Compete for missions. Roamer contests camps for +5% jungle MS.

**Phase 1–2:** Contest early organs. Push Sentinels (5% each + supply line severance). Defend Hives.

**Phase 3:** Hypothalamus (8% + ult CDR). Target Lungs (10%). Protect Hives.

**Phase 4:** Brain (15%). Assess Heart viability by Sentinel count.

**Phase 5:** Heart Surge or grind to 0%. ACE during Surge = win.

### Immune Team

**Phase 0:** Win Cortex mission for full ward advantage. Roamer secures camps.

**Phase 1–2:** Defend organs (hunt slayers). Push Dark Sentinels. Begin Hive invasion.

**Phase 3:** Hypothalamus. Hive captures. Defend Lungs.

**Phase 4:** Defend Brain. Assess Infection Site by Dark Sentinel count.

**Phase 5:** Infection Site Surge or grind to 100%. ACE during Surge = win.

---

## 28. Post-MVP — Adaptive Biomes

**Deferred to Season 2.**

| Biome | Condition | Effects |
|-------|-----------|---------|
| Cortisol Inferno | Anxiety | -30% healing, panic waves |
| Synaptic Storm | Hyperactive | +40% MS, lane rewire |
| Iron Current | Elderly | Plaque walls, heart pulses |
| Radiant Wellspring | Athlete | WBC patrols, stealth meta |

---

*PATHOGEN — Every Cell Counts*