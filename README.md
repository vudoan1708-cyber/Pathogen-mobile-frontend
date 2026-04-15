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
| Cyst / Sentinel destroyed | 5% |
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
| Cyst / Sentinel destroyed | 150 | 100 |
| Passive income | 2/sec | — |

### Shared Gold & XP
When multiple teammates are nearby (within 12 units) during a kill, the gold and XP are **split evenly** among all nearby allied champions. This encourages teamwork but means solo farming is more gold-efficient per player. Strategic choice: group for safety or solo for economy.

### What Bio-Currency Buys
Unlike traditional MOBAs, you don't buy items. Instead, you spend Bio-currency on **champion-wide tier upgrades** through the Mutation Shop. Skill rank-ups are free — they come with leveling.

---

## Mutation Shop (Tier Upgrade System)

Each level-up grants a **free skill rank slot** — choose which of your 4 skills to rank up. Bio-currency is spent on **champion-wide tier upgrades** purchased from the Mutation Shop:

### Tier Upgrade Path

```
TIER 1 (free) → POTENCY (1,200 Bio) → ALPHA or OMEGA (2,000 Bio) → APEX (3,600 Bio)
```

### Tier Details

| Tier | Cost | Unlock Condition | Champion-Wide Bonus |
|------|------|-----------------|---------------------|
| **Tier 1** | Free | All skills at Rank 1 | Skills usable, +0.3 move speed per skill |
| **Potency** | 1,200 | All 4 skills at Rank 2 | +20% skill damage, +5% omnivamp, +0.5 move speed |
| **Alpha** (Branch A) | 2,000 | Potency purchased | +15% crit damage, +10% crit chance, Bleed on-hit |
| **Omega** (Branch B) | 2,000 | Potency purchased | +10% damage reflection, +20 MR, heal on cast |
| **Apex** | 3,600 | All 4 skills at Rank 3 (level 12) | +10% all scaling, +25 AD, +25 AP |

### Branch Rules

- **Alpha/Omega** unlocks after purchasing Potency. Choose one branch.
- **Apex** becomes available when all 4 skills reach Rank 3 — champion must be at least **level 12**.
- **Branches are reversible:** You can switch from Alpha to Omega (or vice versa) by paying the branch cost again (2,000 Bio). If champion is already past the branch (e.g. level 12 with Apex), they must also re-buy Apex (3,600 Bio) — total cost to switch: **5,600 Bio**.

### Genome Slots (Future)
Additional passive slots:
- **Parasite Link** — On-damage: steal 2% of target's current stats for 5s
- **Regen Membrane** — Absorb first damage every 12s, +10% movement speed while active

---

## Lane Structures

Both teams have defensive structures — **Cysts** (Virus) and **Sentinels** (Immune). They behave identically:

- **First-in-range targeting:** Whichever enemy entity enters the structure's range first gets targeted. If minions arrive first, the structure fires at minions until they're cleared before moving to champions.
- **Champion aggro override:** If an enemy champion attacks an allied champion within the structure's range, the structure immediately breaks minion targeting and switches to that enemy champion.
- **Escalating true damage (champions only):** After **3 consecutive shots** on the same enemy champion, the structure deals **true damage** (bypasses all armor and magic resistance), shown by a white projectile. This escalation does not apply to minions.
- **Homing projectiles:** Structure shots lock onto their target and cannot be dodged.
- **Range ring:** A yellow-orange ring appears at the structure's base when any champion approaches within 3 units of its attack range.

**Structure stats:** High HP (1500–2000), 80–100 damage per shot, 4 attack range, 0.88 attacks/second. Destroying one shifts human health by 5% and grants 150 Bio + 100 XP (shared among nearby allies).

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
| **Phagorath** | Tank / Bruiser | The Devourer — Macrophage Titan |
| **KyllexT** | Assassin / Hunter | The Precision Strike — Killer T-Cell |
| **SeraB** | Support / Healer | The Antibody Weaver — B-Cell Architect |
| **Pyrexia** | Mage / Area Denial | The Fever Engine — Inflammatory Response |

---

## Champion Skills & Passives

Each champion has 1 passive and 4 active skills. Skills are numbered 1–4 (mobile controls). Each skill has 3 ranks — rank-ups are **free** (granted on level-up). Champion-wide tier bonuses (Potency, Alpha/Omega, Apex) are purchased with Bio-currency from the Mutation Shop.

### Skill Rank Scaling

Each rank-up improves the skill's primary stats (damage, cooldown, duration) and may unlock bonus effects at higher ranks. See individual champion sections below for exact per-rank values.

**General scaling guidelines:**
- **Damage skills:** +10–20 flat damage per rank (~15% of base)
- **Cooldowns:** -0.5s (short CDs) to -5s (ultimates) per rank
- **CC durations:** +0.1–0.2s per rank (conservative — CC is powerful)
- **Dash distance:** +0.5 per rank
- **% effects:** +1–3% per rank

---

### Necrova (Virus — Assassin)

**Passive: Tissue Decay** — All of Necrova's attack skills (1, 2, 3) apply 1 **Decay** stack on hit (lasts 6s, max 5 stacks). Champion kills decay **0.3% human health** over 5s.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Tissue Melt | Projectile | 120 magic dmg, 4s CD, 40 mana | Deals damage and dissolves **30% of target's armor** over 3s. Applies 1 Decay stack. Rank 2: +3% omnivamp. Rank 3: +6% omnivamp. |
| 2 | Subdermal Tunnel | Dash | 60 magic dmg, 10s CD, 50 mana | Burrow through terrain, **untargetable** during dash. Exit deals AoE damage and applies 1 Decay stack. |
| 3 | Septic Bloom | AOE | 100 magic dmg, 12s CD, 60 mana | Mark an area — after **1.2s** erupts with toxin. **25% slow** for 2s, applies 1 Decay stack. |
| 4 | Total Necrosis | Self-Buff | 50 AP + 25 AD/stack, 60s CD, 100 mana | Consume all Decay stacks on enemies in range. Become **Enraged** — deal **50 AP + 25 AD per stack** as true damage for **5 seconds**. Heals **25%** of damage dealt. |

#### Rank-Up Details

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| **Tissue Melt** | +18 dmg (→138), -0.5s CD (→3.5s), gains +3% omnivamp | +20 dmg (→158), -0.5s CD (→3.0s), omnivamp → 6% |
| **Subdermal Tunnel** | +10 AoE dmg (→70), -1s CD (→9s), +0.5 dash distance | +10 AoE dmg (→80), -1s CD (→8s), +0.5 dash distance |
| **Septic Bloom** | +15 dmg (→115), -1s CD (→11s), +0.2s slow (→2.2s) | +15 dmg (→130), -1s CD (→10s), +0.2s slow (→2.4s) |
| **Total Necrosis** | +8 AP/stack (→58), +4 AD/stack (→29), -5s CD (→55s) | +7 AP/stack (→65), +4 AD/stack (→33), -5s CD (→50s), heal → 30% |

---

### PrionMind (Virus — Mage)

**Passive: Misfolded Conversion** — Converts **2% of max mana into AP** (recalculates on mana changes). Every 3rd ability cast on the same target applies **Confusion** (rooted for 1.5s).

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Misfolded Cascade | Projectile | 150 magic dmg, 4s CD, 40 mana | Skillshot that **chains** up to 3 enemies. Each bounce deals **+12%** more than the last. |
| 2 | Neural Hijack | Target | No dmg, 10s CD, 50 mana | **Reverses** enemy movement controls for 1.2s. Pure disruption. +0.2s per rank. |
| 3 | Plaque Wall | AOE | No dmg, 12s CD, 55 mana | Create a protein wall for 3s. Blocks projectiles, enemies passing through are **30% slowed**. |
| 4 | Cognitive Collapse | AOE | 250 magic dmg, 60s CD, 100 mana | Scramble enemy abilities in area for 3s (abilities fire in random directions). Deals **250 magic damage** on cast + **8% of remaining health / second** when Confused. |

#### Rank-Up Details

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| **Misfolded Cascade** | +22 dmg (→172), -0.5s CD (→3.5s), bounce dmg → +14% | +26 dmg (→198), -0.5s CD (→3.0s), bounce dmg → +16% |
| **Neural Hijack** | +0.2s reverse (→1.4s), -1s CD (→9s) | +0.2s reverse (→1.6s), -1s CD (→8s) |
| **Plaque Wall** | +0.5s wall (→3.5s), -1s CD (→11s), +0.2s slow duration | +0.5s wall (→4s), -1s CD (→10s), +0.2s slow duration |
| **Cognitive Collapse** | +30 dmg (→280), -5s CD (→55s), +0.2s scramble (→3.2s) | +30 dmg (→310), -5s CD (→50s), +0.3s scramble (→3.5s) |

---

### Sporaxis (Virus — Tank)

**Passive: Mycelial Network** — Converts **1.5% of max HP into AD**. Gains **+2 armor and +1 MR** per nearby allied minion (max 4 stacks).

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Mycelial Slam | AOE | 80 phys dmg, 8s CD, 40 mana | Ground pound. First enemy hit is **rooted 1s**. |
| 2 | Spore Shield | Self-Buff | Shield: 15% max HP, 10s CD, 50 mana | Shield for 4s. When broken or expired, releases toxic cloud granting nearby allies **+10 MR** for 3s. |
| 3 | Parasitic Latch | Target | No dmg, 14s CD, 60 mana | Shoot a branch in a direction and attach to enemy for 1.5s if hit — **drain 10% of their AD and armor** for 5s. |
| 4 | Colony Eruption | Self-Buff | 40 magic dmg/clone death, 60s CD, 100 mana | Split into **3 mini-clones** (25% of Sporaxis's stats) for 5s. Clones attack Sporaxis's target. On death each clone **explodes** for 40 magic AoE damage. Clones inherit Mycelial Network armor stacks. |

#### Rank-Up Details

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| **Mycelial Slam** | +12 dmg (→92), -0.5s CD (→7.5s), +0.1s root (→1.1s) | +13 dmg (→105), -0.5s CD (→7s), +0.1s root (→1.2s) |
| **Spore Shield** | +2% shield (→17%), -1s CD (→9s), +0.5s duration (→4.5s), MR → +12 | +2% shield (→19%), -1s CD (→8s), +0.5s duration (→5s), MR → +15 |
| **Parasitic Latch** | -1s CD (→13s), +0.1s attach (→1.6s), drain → 12% | -1s CD (→12s), +0.1s attach (→1.7s), drain → 14% |
| **Colony Eruption** | +6 dmg/clone (→46), -5s CD (→55s), +0.5s clones (→5.5s), stats → 28% | +6 dmg/clone (→52), -5s CD (→50s), +0.5s clones (→6s), stats → 30% |

---

### Vector9 (Virus — Marksman)

**Passive: Contagion Index** — Auto-attacks mark unique enemies as **Infected** (6s). Each Infected target grants Vector9 **+4% attack speed** (max 4 stacks = +16%). With 3+ Infected targets, human health drops **0.1%/s**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Viral Volley | Projectile | 100 phys dmg, 4s CD, 35 mana | Applies 1 **Infection** stack. At 4 stacks on same target: burst **60 bonus magic damage** and spread 1 stack to nearest enemy. |
| 2 | Mutation Drift | Dash | 10s CD, 45 mana | Dash leaving particle trail for 2s. Enemies crossing gain 1 Infection stack. |
| 3 | Incubation Zone | AOE | 20 magic dmg, 14s CD, 50 mana | Place a spore ward. Enemy champion steps on will explode it for 20 AoE magic damage + grants vision of area for 3s. |
| 4 | Pandemic Protocol | Self-Buff | 60s CD, 100 mana | Mark all visible enemies for 5s. Auto-attacks have **100% Infection transfer** (spreads to nearest unmarked enemy) and gain **+15% crit chance**. |

#### Rank-Up Details

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| **Viral Volley** | +15 dmg (→115), -0.5s CD (→3.5s), burst → 70 | +15 dmg (→130), -0.5s CD (→3.0s), burst → 80 |
| **Mutation Drift** | -1s CD (→9s), +0.5 dash distance, +0.5s trail (→2.5s) | -1s CD (→8s), +0.5 dash distance, +0.5s trail (→3s) |
| **Incubation Zone** | +5 dmg (→25), -1s CD (→13s), +1s vision (→4s) | +5 dmg (→30), -1s CD (→12s), +1s vision (→5s) |
| **Pandemic Protocol** | -5s CD (→55s), +0.5s mark (→5.5s), crit → +18% | -5s CD (→50s), +0.5s mark (→6s), crit → +20% |

---

### Phagorath (Immune — Tank)

**Passive: Phagocytosis** — Minion kills grant **+3 max HP permanently** (max 80 stacks = +240 HP). Champion kills grant **+12 max HP permanently** (no cap). Converts **2% of max HP into AD**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Engulf | Target | 1/6 max HP + 3% target max HP as phys dmg, 6s CD, 40 mana | Grab and **suppress** enemy for 1.2s, dealing 1/6 max HP as physical damage + 3% target max HP. During suppress, Phagorath regenerates **2% max HP**. |
| 2 | Cytoplasm Crush | AOE | 70 phys dmg, 14s CD, 50 mana | Slam area for 70 physical damage creating a barrier zone — allies inside take **20% reduced damage** for 3s. |
| 3 | Chemotaxis Charge | Dash | 60 phys dmg, 40% slow 2s, 10s CD, 45 mana | Dash through enemies. First enemy hit is slowed 40% for 2s, gain **+8 armor** for 4s. |
| 4 | Immune Cascade | AOE | 120 magic dmg, 60s CD, 100 mana | Push enemies outward dealing 120 magic damage, **30% slow** for 2s. Heal allies **10% max HP**. Cleanse zones. |

#### Rank-Up Details

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| **Engulf** | -1s CD (→5s), target HP → 3.5%, +0.1s suppress (→1.3s), regen → 2.5% | -1s CD (→4s), target HP → 4%, +0.1s suppress (→1.4s), regen → 3% |
| **Cytoplasm Crush** | +10 dmg (→80), -1s CD (→13s), dmg reduction → 22% | +10 dmg (→90), -1s CD (→12s), dmg reduction → 25% |
| **Chemotaxis Charge** | +10 dmg (→70), -1s CD (→9s), +0.5 dash, armor → +10, +0.1s slow (→2.1s) | +10 dmg (→80), -1s CD (→8s), +0.5 dash, armor → +12, +0.1s slow (→2.2s) |
| **Immune Cascade** | +15 dmg (→135), -5s CD (→55s), heal → 12%, +0.2s slow (→2.2s) | +15 dmg (→150), -5s CD (→50s), heal → 14%, +0.2s slow (→2.4s) |

---

### KyllexT (Immune — Assassin)

**Passive: Adaptive Memory** — First time KyllexT damages each unique enemy champion, they are **Tagged**. Tagged champions take **+10% damage** from KyllexT permanently. Champion kills restore **0.2% human health**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Antigen Lock | Target | +20 true dmg/hit, 8s CD, 40 mana | Mark target for 4s — next 3 auto-attacks deal **+20 bonus true damage** each and apply **30% slow** for 1s. |
| 2 | Rapid Response | Dash | 55 phys dmg, 10s CD, 45 mana | Dash through enemy, dealing damage and applying **Exposed** (+8% damage taken from all sources for 3s). |
| 3 | Perforin Burst | AOE | 3% target max HP, 12s CD, 55 mana | Cone of cytotoxic enzymes dealing **3% target max HP** as magic damage over 3s. |
| 4 | Adaptive Recall | Self-Buff | 60s CD, 100 mana | **Passive:** +3 AD per unique enemy type killed (stacks separately per type). **Active:** +30% attack speed against Exposed targets for 4s. |

#### Rank-Up Details

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| **Antigen Lock** | +5 true dmg/hit (→+25), -0.5s CD (→7.5s), +0.1s slow (→1.1s) | +5 true dmg/hit (→+30), -0.5s CD (→7s), +0.1s slow (→1.2s) |
| **Rapid Response** | +8 dmg (→63), -1s CD (→9s), +0.5 dash, Exposed → +9% | +8 dmg (→71), -1s CD (→8s), +0.5 dash, Exposed → +10% |
| **Perforin Burst** | +0.5% target HP (→3.5%), -1s CD (→11s) | +0.5% target HP (→4%), -1s CD (→10s) |
| **Adaptive Recall** | -5s CD (→55s), AS → +35%, AD/kill → +4 | -5s CD (→50s), AS → +40%, AD/kill → +5, duration → 4.5s |

---

### SeraB (Immune — Support)

**Passive: Antibody Synthesis** — Converts **2% of max mana into AP**. Healing or shielding an ally restores **0.1% human health** (8s cooldown). Allies within 6 units regenerate **+0.5 HP/s**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Antibody Tether | Target | 3 HP/s regen, 10s CD, 40 mana | Link to ally for 10s — both regenerate 3 HP/s. SeraB absorbs **20% of damage** to tethered ally. |
| 2 | Plasma Infusion | Target | 80 HP heal, 8s CD, 50 mana | Heal ally 80 HP over 3s. Overhealing converts to **shield** (max 40 HP, 4s duration). |
| 3 | Opsonize | Target | No dmg, 12s CD, 55 mana | Tag enemy — **+10% damage from all sources**, revealed for 3s. |
| 4 | Herd Immunity | AOE | 60s CD, 100 mana | Allies within range share a **collective HP pool** for 2s. Damage to any ally is split evenly among all allies in the zone. |

#### Rank-Up Details

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| **Antibody Tether** | +1 HP/s (→4), -1s CD (→9s), absorb → 23% | +1 HP/s (→5), -1s CD (→8s), absorb → 25%, link → 12s |
| **Plasma Infusion** | +12 heal (→92), -0.5s CD (→7.5s), shield → 48 | +13 heal (→105), -0.5s CD (→7s), shield → 55, shield duration → 5s |
| **Opsonize** | -1s CD (→11s), +0.5s reveal (→3.5s), dmg amp → +11% | -1s CD (→10s), +0.5s reveal (→4s), dmg amp → +12% |
| **Herd Immunity** | -5s CD (→55s), +0.3s pool (→2.3s) | -5s CD (→50s), +0.2s pool (→2.5s) |

---

### Pyrexia (Immune — Mage)

**Passive: Fever Response** — All magic damage applies a stack on an enemy, on every 3rd stack applies **Burn** (1.5% of damage dealt over 2s). Additionally, at 3 stacks, target takes **+6% magic damage** from all sources. Ability hits on Burnt enemies recover **0.05% human health**.

| # | Name | Type | Base Stats | Description |
|---|------|------|-----------|-------------|
| 1 | Fever Spike | Projectile | 80 magic dmg, 4s CD, 35 mana | Heat bolt applying **15% stacking slow**. At 4 stacks: target is **silenced 1s**. |
| 2 | Inflammation Zone | AOE | 20 magic dmg/s, 14s CD, 55 mana | Burning area for 4s. Allies: **+10% attack speed**. Enemies: 20 magic damage per second. |
| 3 | Thermal Barrier | AOE | No dmg, 14s CD, 50 mana | Wall of heat haze for 3s — blocks vision, **deflects** the first projectile that hits it. |
| 4 | Hyperthermia | AOE | 1.5% max HP/s, 60s CD, 100 mana | Map-wide: all virus champions burn for **1.5% max HP/s** for 8s. Below 30% host HP: increases to **3%/s**. |

#### Rank-Up Details

| Skill | Rank 1 → 2 | Rank 2 → 3 |
|-------|-----------|-----------|
| **Fever Spike** | +12 dmg (→92), -0.5s CD (→3.5s), slow → 17% per stack | +13 dmg (→105), -0.5s CD (→3.0s), slow → 19% per stack, silence → 1.1s |
| **Inflammation Zone** | +4 dmg/s (→24), -1s CD (→13s), +0.5s zone (→4.5s), AS → +12% | +4 dmg/s (→28), -1s CD (→12s), +0.5s zone (→5s), AS → +15% |
| **Thermal Barrier** | -1s CD (→13s), +0.5s wall (→3.5s) | -1s CD (→12s), +0.5s wall (→4s) |
| **Hyperthermia** | +0.25% HP/s (→1.75%), -5s CD (→55s), +1s duration (→9s) | +0.25% HP/s (→2.0%), -5s CD (→50s), +1s duration (→10s) |

---

## Leveling & Gene Points

### 12-Level System

Max level is **12** (4 skills × 3 ranks). Each level-up requires XP and grants:
1. **1 skill rank-up slot** — choose which skill to rank up (**free**, no Bio cost)
2. **1 Gene Point** — free stat choice (immediate, no Bio cost)

Bio-currency is spent on champion-wide tier upgrades from the Mutation Shop — see [Mutation Shop](#mutation-shop-tier-upgrade-system) section.

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

### Respawn Time

`round(4.75 + 0.25 × level²)` — scales quadratically so late-game deaths are severely punishing.

| Level | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 |
|-------|---|---|---|---|---|---|---|---|---|----|----|-----|
| **Respawn** | 5s | 6s | 7s | 9s | 11s | 14s | 17s | 21s | 25s | 30s | 35s | 41s |

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
1. **Early game:** Focus on farming minions for Bio-currency and XP. Don't fight the immune champion head-on until you have skill ranks.
2. **Destroy Sentinels:** Taking out Sentinels shifts health -5% each and opens the map. Beware: after 3 shots, Sentinels deal true damage.
3. **Target mid-tier organs:** Lungs (10%), Liver (8%), Bone Marrow (8%) offer consistent swings without heavy defense.
4. **Snowball:** Once below 40% health, your +10% damage bonus kicks in. After 10 takedowns, each champion kill shifts 3% instead of 1%.
5. **Heart/Brain rush:** Worth 15% each — game-ending if captured, but heavily contested.

### Immune Team Strategy
1. **Protect Sentinels:** Your defensive structures are critical. Losing them swings health -5% each.
2. **Farm efficiently:** Build Bio-currency for tier upgrades. Each tier milestone also grants move speed — you start slow (3.5) and need upgrades to roam.
3. **Capture healing organs:** Prioritize Lungs (+10%), Liver (+8%), Bone Marrow (+8%) for steady recovery.
4. **Comeback mechanic:** Even when behind, the Survival Instinct buff at Critical condition (+15% damage) can turn fights.
5. **Deny takedowns:** After 10 kills, enemy champion kills shift 3% — avoid feeding late-game kills.

### Economy Strategy
1. **Never miss minion kills** — Bio-currency is your only way to buy tier upgrades and Genome Slots.
2. **Level up fast** — skill rank-ups are free but require XP. Each rank-up makes your skills meaningfully stronger.
3. **Rush Potency (1,200 Bio)** once all skills hit Rank 2 — +20% skill damage, +5% omnivamp, +0.5 move speed.
4. **Choose Alpha vs Omega wisely** — Alpha for carry/assassin playstyle, Omega for tank/support. This choice is reversible but costly.
5. **Save for Apex (3,600 Bio)** if you're snowballing — +10% all scaling, +25 AD, +25 AP.
6. **Champion kills are worth 300 Bio** — 12x more than a minion kill, but gold is shared 80 / 20 if teammates are nearby (within 12 units). Gold is shared equally if teammates contributing to slaying the enemies. Fight when you have an advantage.
7. **Solo farm vs group:** Solo farming gives full gold. Grouping creates pressure to the enemy laner but gold will be shared equally team wise (unless you are a Support).

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
**Respawn time:** `round(4.75 + 0.25 × level²)` — Level 1: 5s, Level 6: 14s, Level 12: 41s

---

*PATHOGEN — Every Cell Counts*
