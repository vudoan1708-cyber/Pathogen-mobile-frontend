# PATHOGEN — The Body is the Battlefield

> A MOBA where the map is the human body. Play as viruses or the immune system in a battle for the host's survival.

---

## Game Overview

PATHOGEN is a mobile 3D MOBA set inside a human body. Two teams battle: **Viruses** try to destroy the host, while the **Immune System** fights to flush all viruses out and restore full health. The map dynamically changes based on the host's deteriorating or improving condition.

### Win Conditions

| Team | Goal | Victory Trigger |
|------|------|-----------------|
| **Virus** | Destroy organs, spread infection, drain the host | Human Health reaches **0%** |
| **Immune** | Push viruses out, complete healing missions, restore health | Human Health reaches **100%** |

The Human Health Bar always starts at **50%** — perfectly balanced. Every strategic action shifts it.

---

## Teams

### Virus Team (Attackers)
The virus team's infection type determines the host profile — choosing your pathogen is choosing your battleground. Viruses specialize in destruction, infiltration, and corruption.

**Team Goal:** Destroy organs and complete infection missions to drain human health to 0%.

**Playstyle:** Aggressive. Push into the body, target organs, spread infection. If you reach the heart and stop it, you win fast. Otherwise, grind the host down through attrition.

### Immune Team (Defenders)
The immune system fights to protect the host. They push viruses out by completing purification missions and defending vital organs.

**Team Goal:** Complete healing objectives and push viruses out to restore human health to 100%.

**Playstyle:** Defensive-into-offensive. Protect organs early, build advantages through farming, then push the viruses out systematically.

---

## Human Health System

The central mechanic. Starts at 50% and shifts based on both teams' actions.

### What Shifts Health

| Action | Health Shift |
|--------|-------------|
| Champion kill (early, <10 takedowns) | 1% per kill |
| Champion kill (after 10 takedowns) | **3% per kill** (scales up) |
| Infection Node / Sentinel destroyed | 5% |
| Organ: Skin (Epidermis) | 3% |
| Organ: Lymph Vessels | 4% |
| Organ: Stomach | 5% |
| Organ: Intestines | 5% |
| Organ: Spleen | 6% |
| Organ: Thymus | 6% |
| Organ: Kidneys | 7% |
| Organ: Liver | 8% |
| Organ: Bone Marrow | 8% |
| Organ: Lungs | 10% |
| Organ: Heart | **15%** |
| Organ: Brain (Cerebrum) | **15%** |
| Minion farming | No direct shift (economy only) |

Champion kill health shift **scales with takedowns**: starts at 1% per kill, increases to 3% after a team reaches 10 total takedowns. This makes late-game fights much more impactful.

### Body Conditions

The host's condition changes based on health %, granting team-wide bonuses:

| Health Range | Condition | Effect |
|-------------|-----------|--------|
| 0–20% | **Critical** | Virus: +20% damage / Immune: -20% damage |
| 20–40% | **Sick** | Virus: +10% damage / Immune: -10% damage |
| 40–60% | **Normal** | No bonuses — balanced fight |
| 60–80% | **Recovering** | Immune: +10% damage / Virus: -10% damage |
| 80–100% | **Healthy** | Immune: +20% damage / Virus: -20% damage |

This creates a **snowball mechanic** — the winning team gets progressively stronger, but the losing team can fight back by targeting key objectives.

---

## Game Phases

The game progresses through dynamic phases tied to the host's health:

### Phase 1 — Incubation (50% HP)
*The Calm Before the Storm*

Both teams farm lane minions, establish vision, and begin first skill upgrades. Lane missions are revealed. Lymph Node camps grant bonus Bio and team-wide temporary buffs.

### Phase 2 — Symptoms Manifest (35–65% HP)
*The Body Notices*

The host starts reacting. Winning team gets map advantages — symptom events or oxygen boosts. Organ Siege points appear. Capture a vital organ to swing health bar by 5% and gain a team buff for 2 minutes.

### Phase 3 — Crisis Point (20–35% or 65–80% HP)
*The Tipping Point*

Map transforms dramatically. Terrain crumbles or reinforcements spawn. The Hypothalamus — a neutral boss — appears. Defeating it grants 8% health swing and 3-minute team-wide ult CDR.

### Phase 4 — Critical Condition (5–20% or 80–95% HP)
*Last Stand*

Losing team gains Survival Instinct — 15% bonus damage and tenacity. Map advantage is massive but fights are harder. Core objectives become vulnerable.

### Phase 5 — Endgame (0–5% or 95–100% HP)
*Flatline or Full Recovery*

All lanes converge. 60-second final siege. Losing team gets 30% Miraculous Rally buff. Push past threshold to force the final act.

---

## Economy & Farming

### Bio-Currency
The game's currency, earned through farming. **Minion kills do not affect human health** — they are purely for personal economy and leveling.

| Source | Bio-Currency | XP |
|--------|-------------|-----|
| Minion kill | 25 | 40 |
| Champion kill | 300 | 200 |
| Infection Node / Sentinel destroyed | 150 | 100 |
| Passive income | 2/sec | — |

### Shared Gold & XP
When multiple teammates are nearby (within 12 units) during a kill, the gold and XP are **split evenly** among all nearby allied champions. This encourages teamwork but means solo farming is more gold-efficient per player. Strategic choice: group for safety or solo for economy.

### What Bio-Currency Buys
Unlike traditional MOBAs, you don't buy items. Instead, you **upgrade your skills** through the Mutation Shop.

---

## Mutation Shop (Skill Upgrade System)

Each champion has 4 skills. Each skill can be evolved through mutation tiers:

### Upgrade Path

```
LOCKED → BASE (free) → POTENCY (300 Bio) → ALPHA or OMEGA (500 Bio) → APEX (900 Bio)
```

### Tier Details

| Tier | Cost | Effect |
|------|------|--------|
| **Base** | Free | Unlocks the skill |
| **Potency** | 300 | +25% base damage/healing, +5% omnivamp |
| **Alpha** (Branch A) | 500 | Offensive: +15% crit damage, +10% crit chance, Bleed on-hit |
| **Omega** (Branch B) | 500 | Defensive: +10% damage reflection, +20 magic resist, heal on cast |
| **Apex** | 900 | Final evolution: +40% all scaling, +15 AD, +15 AP |

**Key Decision:** At Tier 3, you must choose Alpha (offensive) or Omega (defensive). This choice is permanent and defines your build identity.

### Genome Slots (Future)
Additional passive slots:
- **Parasite Link** — On-damage: steal 2% of target's current stats for 5s
- **Regen Membrane** — Absorb first damage every 12s, +10% movement speed while active

---

## Lane Structures

### Infection Nodes (Virus Team)
Defensive structures that protect the virus team's territory. Attack enemies that enter their range. After **3 consecutive shots** on the same target, they deal **true damage** (bypasses all armor and magic resistance), shown by a white beam.

### Sentinels (Immune Team)
The immune system's defensive outposts. Same escalating damage mechanic — 3 consecutive hits trigger true damage. Prioritize attacking minions first, but will switch to champions if no minions are present.

**Structure stats:** High HP (1500–2000), 80–100 damage per shot, 10–12 attack range. Destroying one shifts human health by 5% and grants 150 Bio + 100 XP (shared among nearby allies).

---

## Organ Objectives

Organs are the key strategic objectives. The **virus team attacks** organs to capture them (depleting their HP to 0). The **immune team protects** organs — preventing the virus team from capturing them within the time limit. This is asymmetric: the immune team doesn't "capture" organs, they defend them (similar to how in LoL, one team takes Dragon while the other contests it).

### Objective Phase Timing
- Each organ phase lasts **4–5 minutes**
- If the virus team **captures** (destroys) the organ → human health shifts by the organ's value
- If the timer **expires** without capture → immune team gets a **+3% health boost** and the organ resets to full HP
- The immune team's goal is to run down the clock while denying the virus team

### Organ Defenders (Neutral Creatures)

When an organ's HP drops below **50%**, a unique **defender creature** spawns and attacks **both teams** indiscriminately — like different Dragon types in LoL. Each organ spawns a different creature with unique stats and personality, themed to the organ's biological function. Killing the defender grants **200 Bio + 150 XP** (shared among nearby allies).

| Organ | Shift | Defender Creature | Creature Style |
|-------|-------|-------------------|----------------|
| **Brain** | 15% | **Microglial Guardian** | Tanky, high damage. The brain's resident immune cell. |
| **Heart** | 15% | **Cardiac Sentinel** | Fast, relentless. Matches the heart's rhythm with rapid attacks. |
| **Lungs** | 10% | **Alveolar Warden** | Balanced. Controls space around the objective. |
| **Liver** | 8% | **Kupffer Brute** | Slow, heavy-hitting. Liver's specialized macrophage. |
| **Bone Marrow** | 8% | **Osteoclast Titan** | Massive HP, slow. A bone-resorbing giant. |
| **Kidneys** | 7% | **Mesangial Crusher** | Mid-range stats. Filters threats methodically. |
| **Spleen** | 6% | **Splenic Ravager** | Fast and aggressive. Blood-fueled berserker. |
| **Thymus** | 6% | **Thymic Instructor** | Balanced. Trains immune responses even in death. |
| **Stomach** | 5% | **Gastric Acidborn** | Slow but extremely high damage. Acid-based attacks. |
| **Intestines** | 5% | **Gut Flora Golem** | Moderate. Made of beneficial bacteria colonies. |
| **Lymph Vessels** | 4% | **Lymphatic Patrol** | Very fast, lower HP. A swift interceptor. |
| **Skin** | 3% | **Langerhans Sentry** | Weakest defender. The skin's first-line scout. |

### Defender Enrage
All defenders **enrage at 30% HP** — gaining +50% damage and +30% movement speed, turning bright red. This makes the final stretch of an organ fight dangerous for both teams.

### Strategic Notes
- **Brain and Heart** (15% each) are the "Barons" of Pathogen — game-deciding objectives with the strongest defenders
- **Mid-tier organs** (Lungs, Liver, Bone Marrow) are the "Dragons" — consistent value, manageable defenders
- **Low-tier organs** (Skin, Lymph Vessels) are early-game objectives — easy to contest but low reward
- The immune team should stall and poke during objective phases — every second the virus wastes is a win
- The virus team should burst the organ quickly before the defender spawns, or coordinate to handle both organ + defender

---

## Champion Roster

### Virus Team

| Champion | Role | Flavor |
|----------|------|--------|
| **Necrova** | Assassin / Infiltrator | The Flesh Eater — Necrotizing Fasciitis Strain |
| **PrionMind** | Mage / Controller | The Thought Corrupter — Prion-class Pathogen |
| **Sporaxis** | Tank / Bruiser | The Living Colony — Fungal Overgrowth |
| **Vector9** | Marksman / Spread | The Patient Zero — Airborne Contagion |

### Immune Team

| Champion | Role | Flavor |
|----------|------|--------|
| **Phagorath** | Tank / Guardian | The Devourer — Macrophage Titan |
| **KyllexT** | Assassin / Hunter | The Precision Strike — Killer T-Cell |
| **SeraB** | Support / Healer | The Antibody Weaver — B-Cell Architect |
| **Pyrexia** | Mage / Area Denial | The Fever Engine — Inflammatory Response |

---

## Champion Skills (Detailed)

### Necrova (Virus — Assassin)
- **Q: Tissue Melt** — Dissolves target armor over 3s. Upgrades add omnivamp.
- **W: Subdermal Tunnel** — Burrow through terrain, becoming untargetable. Exit deals AoE.
- **E: Septic Bloom** — Mark an area — after 2s it erupts with toxin, slowing and applying Decay.
- **R: Total Necrosis** — Consume all Decay stacks in range for massive true damage. Heals for 40% dealt.

### PrionMind (Virus — Mage)
- **Q: Misfolded Cascade** — Skillshot that chains between enemies, +15% dmg per bounce.
- **W: Neural Hijack** — Reverse an enemy's movement controls for 1.5s.
- **E: Plaque Wall** — Create a protein wall. Blocks projectiles, slows enemies.
- **R: Cognitive Collapse** — Scramble all enemy abilities in area for 4s. Fog expands.

### Sporaxis (Virus — Tank)
- **Q: Mycelial Slam** — Ground pound — roots snare first enemy. Upgrades add damage reflection.
- **W: Spore Shield** — Decaying shield. When broken, releases toxic cloud granting allies MR.
- **E: Parasitic Latch** — Attach to enemy for 2s — drain their stats, add to yours.
- **R: Colony Eruption** — Explode into 3 mini-clones with 30% of your stats for 6s.

### Vector9 (Virus — Marksman)
- **Q: Viral Volley** — Rapid-fire applying Infection stacks. At 5, burst + spread.
- **W: Mutation Drift** — Dash leaving airborne particle trail. Enemies crossing gain 1 Infection.
- **E: Incubation Zone** — Place spore ward. Explodes after 8s for vision + AoE Infection.
- **R: Pandemic Protocol** — Mark all visible enemies. 100% transfer rate + crit for 6s.

### Phagorath (Immune — Tank)
- **Q: Engulf** — Grab and suppress enemy for 1.5s. Upgrades add HP regen per second.
- **W: Cytoplasm Shield** — AoE barrier — allies inside take 25% reduced damage.
- **E: Chemotaxis Charge** — Dash to nearest infected ally, cleanse 1 debuff, gain armor.
- **R: Immune Cascade** — Push all virus champs outward (slowed 10%), heal allies 20% max HP, cleanse zone.

### KyllexT (Immune — Assassin)
- **Q: Antigen Lock** — Mark target — next 3 attacks deal bonus true damage + 50% slow.
- **W: Rapid Response** — Dash through enemy, apply Exposed (+10% dmg taken 3s).
- **E: Perforin Burst** — Cone of cytotoxic enzymes. % max HP damage over 4s.
- **R: Adaptive Memory** — Passive: +5 ATK per kill per type. Active: 50% AS on Exposed targets 5s.

### SeraB (Immune — Support)
- **Q: Antibody Tether** — Link to ally — both regen. Absorb 30% of their lethal damage.
- **W: Plasma Infusion** — Heal over 3s. Overhealing converts to shield. Upgrades add omnivamp.
- **E: Opsonize** — Tag enemy — +15% damage from all sources, revealed 4s.
- **R: Herd Immunity** — Allies within 250 units share collective HP pool for 2.25s.

### Pyrexia (Immune — Mage)
- **Q: Fever Spike** — Heat bolt with stacking slow. At 5 stacks: silenced 1.5s.
- **W: Inflammation Zone** — Burning area: allies gain AS, enemies take ticks + reduced healing.
- **E: Thermal Barrier** — Wall of heat haze — blocks vision, deflects projectiles.
- **R: Hyperthermia** — Map-wide virus burn 2% HP/s for 10s. Below 30% host HP: also damages allies.

---

## Curveball Events (Fairness System)

Every event gives symmetric or complementary effects to both teams:

| Event | Virus Effect | Immune Effect | Design |
|-------|-------------|---------------|--------|
| **Joy Surge** | -10% speed | +15% healing | Speed tax vs sustain |
| **Fear Wave** | +10% damage | +10% armor | Offense vs defense |
| **Anger Flare** | +15% dmg, -10% def | +15% dmg, -10% def | Symmetric glass-cannon |
| **Antibiotic Strike** | Dodge AoE zones | Same zones hit bacteria | Both dodge, mobile champs shine |
| **REM Sleep** | 50% CDR, -15% dmg | 50% CDR, -15% dmg | Symmetric spell-spam |
| **Microbiome Shift** | Jungle +50% Bio, harder | Same | Economy event, not combat |
| **Adrenaline Spike** | +20% speed | +20% speed | Universal mobility |
| **Hypothermia** | -25% AS, +10% ability | Same | Shifts to ability-based play |

---

## Adaptive Biomes

The virus team's infection type determines the host profile:

### The Cortisol Inferno
**Host:** Chronic Anxiety + Depression
- Volcanic eruptions reshape lanes every 3 min
- Fog of war shifts unpredictably
- Tags: Unstable Terrain, -30% Healing, Panic Waves, Fog Shift

### The Synaptic Storm
**Host:** Child Prodigy / Hyperactive
- +40% move speed for all
- Lanes constantly rewire
- Tags: +40% Move Speed, Lane Rewire, Phantasms, Dopamine Burst

### The Iron Current
**Host:** Elderly / Cardiovascular
- Sluggish bloodstream with arterial plaque chokepoints
- Heart rhythm dictates game pace
- Tags: Slow Current, Plaque Walls, Heart Pulses, -15% Immune Base

### The Radiant Wellspring
**Host:** Athlete / Peak Condition
- Pristine battlefield favoring Immune
- WBC patrols as neutral allies
- Tags: Immune Favoured, WBC Patrols, O2 Regen, Stealth Meta

---

## Strategy Tips

### Virus Team Strategy
1. **Early game:** Focus on farming minions for Bio-currency. Don't fight the immune champion head-on until you have skill upgrades.
2. **Destroy Sentinels:** Taking out Sentinels shifts health -5% each and opens the map. Beware: after 3 shots, Sentinels deal true damage.
3. **Target mid-tier organs:** Lungs (10%), Liver (8%), Bone Marrow (8%) offer consistent swings without heavy defense.
4. **Snowball:** Once below 40% health, your +10% damage bonus kicks in. After 10 takedowns, each champion kill shifts 3% instead of 1%.
5. **Heart/Brain rush:** Worth 15% each — game-ending if captured, but heavily contested.

### Immune Team Strategy
1. **Protect Sentinels:** Your defensive structures are critical. Losing them swings health -5% each.
2. **Farm efficiently:** Build Bio-currency for skill upgrades. Each mutation tier also grants move speed — you start slow (3.5) and need upgrades to roam.
3. **Capture healing organs:** Prioritize Lungs (+10%), Liver (+8%), Bone Marrow (+8%) for steady recovery.
4. **Comeback mechanic:** Even when behind, the Survival Instinct buff at Critical condition (+15% damage) can turn fights.
5. **Deny takedowns:** After 10 kills, enemy champion kills shift 3% — avoid feeding late-game kills.

### Economy Strategy
1. **Never miss minion kills** — Bio-currency is your only way to upgrade skills and gain move speed.
2. **Prioritize Q unlock first** — it's free and gives you immediate lane pressure + 0.3 move speed.
3. **Rush Potency (Tier 2)** on your primary skill — +25% damage and +0.5 move speed.
4. **Choose Alpha vs Omega wisely** — Alpha for carry/assassin playstyle, Omega for tank/support.
5. **Save for Apex (900 Bio)** if you're snowballing — +40% all scaling and +0.6 move speed.
6. **Champion kills are worth 300 Bio** — 12x more than a minion kill, but gold is shared if teammates are nearby (within 12 units). Fight when you have an advantage.
7. **Solo farm vs group:** Solo farming gives full gold. Grouping splits gold but is safer. Balance risk vs reward.

---

## Stat Glossary

| Stat | Description |
|------|-------------|
| **Attack Damage (AD)** | Physical damage per auto-attack |
| **Ability Power (AP)** | Scales magic ability damage |
| **Armor** | Reduces physical damage: `reduction = armor / (armor + 100)` |
| **Magic Resist (MR)** | Reduces magic damage: `reduction = MR / (MR + 100)` |
| **Omnivamp** | Heal for X% of all damage dealt |
| **Damage Reflection** | Reflect X% of damage taken back to attacker |
| **Crit Chance** | Chance for auto-attacks to deal 150% damage |
| **Cooldown Reduction (CDR)** | Reduces skill cooldowns by X% |
| **Health Regen** | HP recovered per second |
| **Mana Regen** | Mana recovered per second |
| **Move Speed** | Units moved per second |
| **Attack Speed** | Auto-attacks per second |

---

## Level-Up Stat Gains (Per Level)

| Stat | Gain Per Level |
|------|---------------|
| Max Health | +50 |
| Max Mana | +25 |
| Attack Damage | +3 |
| Armor | +2 |
| Magic Resist | +1.5 |

**Max Level:** 18
**XP to level:** `100 + (level - 1) × 50`

---

*PATHOGEN — Every Cell Counts*
