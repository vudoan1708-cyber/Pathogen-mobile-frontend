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
| Organ: Heart | **100%** |
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

Each champion has 4 skills. Each skill can be evolved through mutation tiers, up to 4 tiers per skill:

### Upgrade Path

```
LOCKED → BASE (free) → POTENCY (1200 Bio) → ALPHA or OMEGA (2000 Bio) → APEX (3600 Bio)
```

### Tier Details

| Tier | Cost | Effect |
|------|------|--------|
| **Base** | Free | Unlocks the skill |
| **Potency** | 1200 | +10–25% skill attributes (varies per skill — e.g. armor skill gets +15% armor, damage skill gets +20% damage), +5% omnivamp |
| **Alpha** (Branch A) | 2000 | Offensive: +15% crit damage, +10% crit chance, Bleed on-hit |
| **Omega** (Branch B) | 2000 | Defensive: +10% damage reflection, +20 magic resist, heal on cast |
| **Apex** | 3600 | Final evolution: +10% all scaling, +25 AD, +25 AP |

### Branch Unlocking

- **Alpha/Omega branches** appear when all 4 skills reach **Tier 2 (Potency)**.
- **Apex** becomes available when all 4 skills reach **Tier 3 (Alpha or Omega)** — champion must be at least **level 12**.
- **Branches are reversible:** You can switch from Alpha to Omega (or vice versa) by paying the branch cost again (2000 Bio). If champion is already past the branch (e.g. level 12 with Apex), they must also re-buy Apex (3600 Bio) — total cost to switch: **5600 Bio**.

### Genome Slots (Future)
Additional passive slots:
- **Parasite Link** — On-damage: steal 2% of target's current stats for 5s
- **Regen Membrane** — Absorb first damage every 12s, +10% movement speed while active

---

## Lane Structures

Both teams have defensive structures — **Infection Nodes** (Virus) and **Sentinels** (Immune). They behave identically:

- **First-in-range targeting:** Whichever enemy entity enters the structure's range first gets targeted. If minions arrive first, the structure fires at minions until they're cleared before moving to champions.
- **Champion aggro override:** If an enemy champion attacks an allied champion within the structure's range, the structure immediately breaks minion targeting and switches to that enemy champion.
- **Escalating true damage (champions only):** After **3 consecutive shots** on the same enemy champion, the structure deals **true damage** (bypasses all armor and magic resistance), shown by a white projectile. This escalation does not apply to minions.
- **Homing projectiles:** Structure shots lock onto their target and cannot be dodged.
- **Range ring:** A yellow-orange ring appears at the structure's base when any champion approaches within 3 units of its attack range.

**Structure stats:** High HP (1500–2000), 80–100 damage per shot, 7–9 attack range, 0.88 attacks/second. Destroying one shifts human health by 5% and grants 150 Bio + 100 XP (shared among nearby allies).

---

## Organ Objectives

Organs are the key strategic objectives. The **virus team attacks** organs to capture them (depleting their HP to 0). The **immune team protects** organs — preventing the virus team from capturing them within the time limit. This is asymmetric: the immune team doesn't "capture" organs, they defend them while the virus team commits resources to take them down.

### Objective Phase Timing
- Each organ phase lasts **4–5 minutes**
- If the virus team **captures** (destroys) the organ → human health shifts by the organ's value
- If the timer **expires** without capture → immune team gets a **+3% health boost** and the organ resets to full HP
- The immune team's goal is to run down the clock while denying the virus team

### Organ Defenders (Neutral Creatures)

When an organ's HP drops below **50%**, a unique **defender creature** spawns and attacks **both teams** indiscriminately. Each organ spawns a different creature with unique stats and personality, themed to the organ's biological function. Killing the defender grants **2000 Bio + 600 XP** (shared among nearby allies).

| Organ | Shift | Defender Creature | Creature Style |
|-------|-------|-------------------|----------------|
| **Brain** | 15% | **Microglial Guardian** | Tanky, high damage. The brain's resident immune cell. |
| **Heart** | 100% | **Cardiac Sentinel** | Fast, relentless. Matches the heart's rhythm with rapid attacks. |
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
- **Brain and Heart** (15%+) are game-deciding objectives with the strongest defenders — contesting them can swing the entire match
- **Mid-tier organs** (Lungs, Liver, Bone Marrow) offer consistent value with manageable defenders — prioritise these for steady progress
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

## Champion Skills & Passives

Each champion has 1 passive and 4 active skills. Skills are numbered 1–4 (mobile controls). Each skill has 3 ranks — each rank increases the skill's base values by **+15%** (damage, healing, shield, buff). Tier milestones (Potency, Alpha/Omega, Apex) add champion-wide bonuses on top.

### Skill Rank Scaling

| Rank | Base Multiplier | Example (150 base damage) |
|------|----------------|---------------------------|
| 1 | 1.0× | 150 |
| 2 | 1.15× | 172 |
| 3 | 1.32× | 198 |

CC durations gain **+0.2s per rank**. Dash skills gain **+0.5 distance per rank**.

---

### Necrova (Virus — Assassin)

**Passive: Tissue Decay** — All of Necrova's attack skills (1, 2, 3) apply 1 **Decay** stack on hit (lasts 6s, max 5 stacks). Champion kills decay **0.3% human health** over 5s.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Tissue Melt | Projectile | 120 magic dmg, 4s CD, 40 mana | Deals damage and dissolves **30% of target's armor** over 3s. Applies 1 Decay stack. Rank 2: +3% omnivamp. Rank 3: +6% omnivamp. |
| 2 | Subdermal Tunnel | Dash | 60 magic dmg, 10s CD, 50 mana | Burrow through terrain, **untargetable** during dash (6 distance). Exit deals AoE damage and applies 1 Decay stack. |
| 3 | Septic Bloom | AOE | 100 magic dmg, 12s CD, 60 mana | Mark an area — after **1.2s** erupts with toxin. **25% slow** for 2s, applies 1 Decay stack. |
| 4 | Total Necrosis | Target | 50 true ability dmg/stack, 60s CD, 100 mana | Consume all Decay stacks on enemies in range. Become **Enraged** and deal more **50 base AP** and **25 base attack damage / stack** as true damage for **5 seconds**. Heals Necrova for **25%** of total damage dealt. |

---

### PrionMind (Virus — Mage)

**Passive: Misfolded Conversion** — Converts **2% of max mana into AP** (recalculates on mana changes). Every 3rd ability cast on the same target applies **Confusion** (rooted for 1.5s).

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Misfolded Cascade | Projectile | 150 magic dmg, 4s CD, 40 mana | Skillshot that **chains** up to 3 enemies. Each bounce deals **+12%** more than the last. |
| 2 | Neural Hijack | Target | No dmg, 10s CD, 50 mana | **Reverses** enemy movement controls for 1.2s. Pure disruption. Rank scaling: +0.2s per rank. |
| 3 | Plaque Wall | AOE | No dmg, 12s CD, 55 mana | Create a protein wall for 3s. Blocks projectiles, enemies passing through are **30% slowed** for 1.5s. |
| 4 | Cognitive Collapse | AOE | 80 magic dmg + 30/s, 60s CD, 100 mana | Scramble enemy abilities in area for 3s (abilities fire in random directions). Deals **250 damage** on cast + **8% of remainning health / second** when Confused. |

---

### Sporaxis (Virus — Tank)

**Passive: Mycelial Network** — Converts **1.5% of max HP into AD**. Gains **+2 armor and +1 MR** per nearby allied minion (max 4 stacks, 8 unit range). Taking damage gives nearby allied champions +8% move speed for 2s.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Mycelial Slam | AOE | 80 phys dmg, 8s CD, 40 mana | Ground pound. First enemy hit is **rooted 1s**. |
| 2 | Spore Shield | Self-Buff | Shield: 15% max HP, 10s CD, 50 mana | Shield for 4s. When broken or expired, releases toxic cloud granting nearby allies **+10 MR** for 3s. |
| 3 | Parasitic Latch | Target | No dmg, 14s CD, 60 mana | Attach to enemy for 1.5s — **drain 10% of their AD and armor**, adding to Sporaxis for 5s. |
| 4 | Colony Eruption | Self-Buff | 40 magic dmg/clone death, 60s CD, 100 mana | Split into **3 mini-clones** (25% of Sporaxis's stats) for 5s. Clones attack Sporaxis's target. On death each clone **explodes** for 40 magic AOE damage each. Clones inherit Mycelial Network armor stacks. |

---

### Vector9 (Virus — Marksman)

**Passive: Contagion Index** — Auto-attacks mark unique enemies as **Infected** (6s). Each Infected target grants Vector9 **+4% attack speed** (max 4 stacks = +16%). With 3+ Infected targets, human health drops **0.1%/s**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Viral Volley | Projectile | 100 phys dmg, 4s CD, 35 mana | Applies 1 **Infection** stack. At 4 stacks on same target: burst **60 bonus magic damage** and spread 1 stack to nearest enemy. |
| 2 | Mutation Drift | Dash | 55 phys dmg, 10s CD, 45 mana | Dash leaving particle trail for 2s. Enemies crossing gain 1 Infection stack. |
| 3 | Incubation Zone | AOE | 20 magic dmg, 14s CD, 50 mana | Place a spore ward. Enemy champion steps on will explode it + grants vision of area for 3s. |
| 4 | Pandemic Protocol | Self-Buff | No dmg, 60s CD, 100 mana | Mark all visible enemies for 5s. Auto-attacks have **100% Infection transfer** (spreads to nearest unmarked enemy) and gain **+15% crit chance**. |

---

### Phagorath (Immune — Tank)

**Passive: Phagocytosis** — Minion kills grant **+3 max HP permanently** (max 80 stacks = +240 HP). Champion kills grant **+12 max HP permanently** (no cap). Converts **2% of max HP into AD**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Engulf | Target | 1/6 max HP + 3% target max HP as phys dmg, 6s CD (-1s per skill upgrade), 40 mana | Grab and **suppress** enemy for 1.2s, dealing 1/6 max HP as physical damage + 3% target max HP. During suppress, Phagorath regenerates **2% max HP**. |
| 2 | Cytoplasm Crush | AOE | 70 phys dmg, 14s CD, 50 mana | Slam area for 70 physical damage creating a barrier zone - allies inside take **20% reduced damage** for 3s. |
| 3 | Chemotaxis Charge | Dash | 60 phys dmg, 40% slow if hit for 2s, 10s CD, 45 mana | Dash through enemies. First enemy hit is slowed, gain **+8 armor** for 4s. |
| 4 | Immune Cascade | AOE | No dmg, 60s CD, 100 mana | Push all enemy champions outward, **30% slow** for 2s. Heal nearby allies **15% max HP**. Cleanse enemy zones. |

---

### KyllexT (Immune — Assassin)

**Passive: Adaptive Memory** — First time KyllexT damages each unique enemy champion, they are **Tagged**. Tagged champions take **+10% damage** from KyllexT permanently. Champion kills restore **0.2% human health**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Antigen Lock | Target | +20 true dmg/hit, 8s CD, 40 mana | Mark target for 4s — next 3 auto-attacks deal **+20 bonus true damage** each and apply **30% slow** for 1s. |
| 2 | Rapid Response | Dash | 55 phys dmg, 10s CD, 45 mana | Dash through enemy, dealing damage and applying **Exposed** (+8% damage taken from all sources for 3s). |
| 3 | Perforin Burst | AOE | 3% target max HP, 12s CD, 55 mana | Cone of cytotoxic enzymes dealing **3% target max HP** as magic damage over 3s. |
| 4 | Adaptive Recall | Self-Buff | 60s CD, 100 mana | **Passive:** +3 AD per unique enemy type killed (stacks separately per type). **Active:** +30% attack speed against Exposed targets for 4s. |

---

### SeraB (Immune — Support)

**Passive: Antibody Synthesis** — Converts **2% of max mana into AP**. Healing or shielding an ally restores **0.1% human health** (8s cooldown). Allies within 6 units regenerate **+0.5 HP/s**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Antibody Tether | Target | 3 HP/s regen, 10s CD, 40 mana | Link to ally for 5s — both regenerate. SeraB absorbs **20% of lethal damage** to tethered ally. |
| 2 | Plasma Infusion | Target | 80 HP heal, 8s CD, 50 mana | Heal ally over 3s. Overhealing converts to **shield** (max 40 HP, 4s duration). |
| 3 | Opsonize | Target | No dmg, 12s CD, 55 mana | Tag enemy — **+10% damage from all sources**, revealed for 3s. |
| 4 | Herd Immunity | AOE | No dmg, 60s CD, 100 mana | Allies within range share a **collective HP pool** for 2s. Damage to any ally is split evenly among all allies in the zone. |

---

### Pyrexia (Immune — Mage)

**Passive: Fever Response** — All magic damage applies **Burn** (1.5% of damage dealt over 2s, stacks up to 3×). At 3 Burn stacks, target takes **+6% magic damage** from all sources. Ability damage on enemies recovers **0.05% human health per enemy hit**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Fever Spike | Projectile | 150 magic dmg, 4s CD, 35 mana | Heat bolt applying **15% stacking slow**. At 4 stacks: target is **silenced 1s**. |
| 2 | Inflammation Zone | AOE | 20 magic dmg/s, 14s CD, 55 mana | Burning area for 4s. Allies: **+10% attack speed**. Enemies: damage ticks + **-20% healing**. |
| 3 | Thermal Barrier | AOE | No dmg, 14s CD, 50 mana | Wall of heat haze for 3s — blocks vision, **deflects** the first projectile that hits it. |
| 4 | Hyperthermia | AOE | 1.5% max HP/s, 60s CD, 100 mana | Map-wide: all virus champions burn for **1.5% max HP/s** for 8s. Below 30% host HP: also damages allies at 0.5%/s. |

---

## Leveling & Gene Points

### 12-Level System

Max level is **12** (4 skills × 3 ranks). Each level-up requires XP and grants:
1. **1 skill rank-up slot** — choose which skill to upgrade (costs Bio-currency)
2. **1 Gene Point** — free stat choice (immediate, no Bio cost)
3. **Skill base stat increase** — the ranked-up skill gets +15% stronger

### Gene Point Paths

| Path | HP | AD | AP | Mana | Armor | MR |
|------|-----|-----|-----|------|-------|-----|
| **Endurance** (Tank) | +60 | — | — | — | +3 | +2 |
| **Aggression** (AD) | +25 | +5 | — | — | +2 | — |
| **Adaptation** (AP) | +25 | — | +5 | +15 | — | +2 |

### XP Requirements

| Level | XP Required | Cumulative |
|-------|-------------|------------|
| 2 | 100 | 100 |
| 3 | 150 | 250 |
| 4 | 200 | 450 |
| 5 | 250 | 700 |
| 6 | 300 | 1,000 |
| 7 | 350 | 1,350 |
| 8 | 400 | 1,750 |
| 9 | 450 | 2,200 |
| 10 | 500 | 2,700 |
| 11 | 550 | 3,250 |
| 12 | 600 | 3,850 |

### Tier Milestones

Tier upgrades unlock when all 4 skills reach the same rank. Still costs Bio-currency to purchase.

| Milestone | Trigger | Cost | Champion-Wide Bonus |
|---|---|---|---|
| **Tier 1** | All skills Rank 1 | Free | Skills usable, +0.3 move speed per skill |
| **Potency** | All skills Rank 2 | 1,200/skill | +20% skill damage, +5% omnivamp, +0.5 MS |
| **Alpha** | All skills Rank 2 + branch | 2,000/skill | +15% crit dmg, +10% crit chance, Bleed on-hit |
| **Omega** | All skills Rank 2 + branch | 2,000/skill | +10% dmg reflect, +20 MR, heal on cast |
| **Apex** | All skills Rank 3 | 3,600/skill | +10% all scaling, +25 AD, +25 AP |

### Respawn Time

`5s + (level × 1s)` — Level 1: 6s, Level 6: 11s, Level 12: 17s.

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
3. **Rush Potency (Tier 2)** on your primary skill — +10–25% skill attributes and +0.5 move speed.
4. **Choose Alpha vs Omega wisely** — Alpha for carry/assassin playstyle, Omega for tank/support. This choice is reversible but costly.
5. **Save for Apex (3600 Bio)** if you're snowballing — +10% all scaling, +25 AD, +25 AP.
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

## Level-Up System

**Max Level:** 12 (4 skills × 3 ranks)

Stat gains come from **Gene Points** (chosen per level-up) — see [Leveling & Gene Points](#leveling--gene-points) section.

**XP to level:** `100 + (level - 1) × 50`
**Respawn time:** `5s + (level × 1s)`

---

*PATHOGEN — Every Cell Counts*
