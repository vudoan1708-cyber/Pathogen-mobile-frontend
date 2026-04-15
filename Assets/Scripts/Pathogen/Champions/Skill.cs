using UnityEngine;
using System;

namespace Pathogen
{
    public enum SkillType
    {
        Projectile,
        Dash,
        AreaOfEffect,
        SelfBuff,
        Target
    }

    public enum IndicatorShape
    {
        Line,           // Thin directional line (narrow projectiles, dashes)
        Rectangle,      // Wide skillshot rectangle showing projectile width
        Cone,           // Fan/sector shape (cleave, breath attacks)
        Circle,         // AoE circle placed at target location
        None            // No indicator (self-buff, passives)
    }

    public enum ProjectilePiercing
    {
        StopOnFirst,
        PierceMinions,
        PierceAll
    }

    public enum StatusType
    {
        Slow,
        Stun,
        Silence,
        Bleed,
        Burn,
        HealOverTime,
        ArmorShred
    }

    public enum MutationTier
    {
        Locked,
        Base,
        Potency,
        Alpha,
        Omega,
        Apex
    }

    // ─── VISUALS (universal — works for any skill type) ─────────────

    [Serializable]
    public class SkillVisuals
    {
        public Color primaryColor = new Color(0.8f, 0.4f, 1f);
        public Color secondaryColor = Color.white;
        public float scale = 0.4f;

        // Trail (projectile, dash)
        public bool hasTrail;
        public Color trailColor = new Color(1f, 1f, 1f, 0.5f);
        public float trailWidth = 0.1f;

        // Hit/cast particles
        public Color particleColor = new Color(1f, 0.8f, 0.2f);
        public int particleCount = 4;
        public float particleSize = 0.12f;
        public float particleForce = 3f;
        public float particleLifetime = 0.6f;

        // Aim indicator
        public Color aimColor = new Color(1f, 1f, 0.3f, 0.7f);
        public float aimWidth = 0.15f;
    }

    // ─── STATUS EFFECTS (modular — any skill can apply any combo) ───

    [Serializable]
    public class SkillStatusEffect
    {
        public StatusType type;
        public float value;    // Slow: 0.5 = 50% slow. Bleed: DPS. Stun: unused (binary).
        public float duration;
    }

    // ─── SKILL DEFINITION ───────────────────────────────────────────

    [Serializable]
    public class SkillDefinition
    {
        public string skillName = "Skill";
        public string description = "";
        public SkillType type = SkillType.Projectile;
        public float baseDamage = 60f;
        public float cooldown = 8f;
        public float manaCost = 40f;
        public float range = 10f;
        public bool isMagicDamage = true;
        public bool rootWhileChanneling = false;
        public float maxChannelTime = 0f;
        public bool rootOnFire = false;           // Champion stops movement when skill fires
        public bool faceSkillDirection = false;    // Champion rotates to face the skill direction on fire
        public bool freeMoveDuringFire = true;     // Champion can move freely during and after firing

        // Projectile behavior
        public float projectileSpeed = 15f;
        public ProjectilePiercing piercing = ProjectilePiercing.StopOnFirst;

        // Dash behavior
        public float dashDistance = 6f;
        public float dashSpeed = 20f;

        // AOE behavior
        public float aoeRadius = 2.5f;

        // Indicator shape
        public IndicatorShape indicatorShape = IndicatorShape.Line;
        public float skillWidth = 1f;
        public float coneAngle = 60f;

        // Buff behavior
        public float buffAttackDamage = 15f;
        public float buffMoveSpeed = 3f;
        public float buffDuration = 6f;

        // Visuals — how this skill looks (universal across all types)
        public SkillVisuals visuals = new SkillVisuals();

        public float GetEffectiveRange()
        {
            return type == SkillType.Dash ? dashDistance : range;
        }

        // Status effects — what this skill applies on hit (any combination)
        public SkillStatusEffect[] statusEffects = Array.Empty<SkillStatusEffect>();
    }

    // ─── SKILL RUNTIME ──────────────────────────────────────────────

    public class Skill
    {
        public SkillDefinition definition;
        public MutationTier tier = MutationTier.Locked;
        public float currentCooldown;

        // Skill-point leveling (separate from MutationTier / Bio upgrades)
        public int skillLevel;                          // 0 = locked, 1+ = unlocked ranks
        public const int MaxSkillRank = 3;              // All skills cap at rank 3 (4 × 3 = 12 levels)
        private const float DamagePerRank = 0.15f;      // +15 % base damage per rank above 1
        private const float CdrPerRank = 0.05f;         // +5 % CDR per rank above 1 (doubles after branch)

        public bool IsReady => tier != MutationTier.Locked && currentCooldown <= 0f;
        public bool IsUnlocked => tier != MutationTier.Locked;

        public static readonly int[] TierCosts = { 0, 0, 1200, 2000, 2000, 3600 };

        // Mutation bonuses
        public float bonusDamagePercent;
        public float bonusOmnivamp;
        public float bonusCritDamage;
        public bool hasBleed;
        public float bonusDamageReflect;
        public float bonusMagicResist;
        public bool hasHealOnCast;
        public float bonusAllScaling;

        public Skill(SkillDefinition def)
        {
            definition = def;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (currentCooldown > 0f)
                currentCooldown -= deltaTime;
        }

        public float GetDamage()
        {
            float levelBonus = Mathf.Max(0, skillLevel - 1) * DamagePerRank;
            return definition.baseDamage * (1f + levelBonus + bonusDamagePercent + bonusAllScaling);
        }

        public float GetCooldown(float cdrPercent)
        {
            // CDR per rank doubles once a branch (Alpha/Omega) is chosen
            float cdrMultiplier = (tier >= MutationTier.Alpha) ? 2f : 1f;
            float levelCdr = Mathf.Max(0, skillLevel - 1) * CdrPerRank * cdrMultiplier;
            return definition.cooldown * (1f - Mathf.Clamp01(cdrPercent + levelCdr));
        }

        public void StartCooldown(float cdrPercent)
        {
            currentCooldown = GetCooldown(cdrPercent);
        }

        public int GetNextUpgradeCost()
        {
            int idx = GetNextTierIndex();
            if (idx < 0 || idx >= TierCosts.Length) return -1;
            return TierCosts[idx];
        }

        public bool NextUpgradeIsBranch() => tier == MutationTier.Potency;

        public bool Upgrade(Champion champion, bool alphaPath = true)
        {
            int cost = GetNextUpgradeCost();
            if (cost < 0 || champion.bioCurrency < cost) return false;

            champion.SpendBioCurrency(cost);

            switch (tier)
            {
                case MutationTier.Locked:
                    tier = MutationTier.Base;
                    champion.moveSpeed += 0.3f;
                    break;

                case MutationTier.Base:
                    tier = MutationTier.Potency;
                    bonusDamagePercent += 0.20f;
                    champion.omnivamp += 0.05f;
                    bonusOmnivamp += 0.05f;
                    champion.moveSpeed += 0.5f;
                    break;

                case MutationTier.Potency:
                    if (alphaPath)
                    {
                        tier = MutationTier.Alpha;
                        bonusCritDamage += 0.15f;
                        champion.critChance += 0.10f;
                        hasBleed = true;
                        champion.moveSpeed += 0.4f;
                    }
                    else
                    {
                        tier = MutationTier.Omega;
                        bonusDamageReflect += 0.10f;
                        champion.damageReflection += 0.10f;
                        bonusMagicResist += 20f;
                        champion.magicResist += 20f;
                        hasHealOnCast = true;
                        champion.moveSpeed += 0.4f;
                    }
                    break;

                case MutationTier.Alpha:
                case MutationTier.Omega:
                    tier = MutationTier.Apex;
                    bonusAllScaling += 0.10f;
                    champion.attackDamage += 25f;
                    champion.abilityPower += 25f;
                    champion.moveSpeed += 0.6f;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Reverses the current branch (Alpha↔Omega). If at Apex, reverts to Potency
        /// and requires re-buying the new branch + Apex (total: branch + apex cost).
        /// Champion must have the full amount upfront.
        /// Returns total cost charged, or -1 if cannot afford or not on a branch.
        /// </summary>
        public int ReverseBranch(Champion champion)
        {
            bool isAlpha = tier == MutationTier.Alpha;
            bool isOmega = tier == MutationTier.Omega;
            bool isApex = tier == MutationTier.Apex;

            if (!isAlpha && !isOmega && !isApex) return -1;

            int branchCost = TierCosts[3]; // 2000
            int apexCost = TierCosts[5];   // 3600
            int totalCost = isApex ? branchCost + apexCost : branchCost;

            if (champion.bioCurrency < totalCost) return -1;

            // If at Apex, revert Apex bonuses first
            if (isApex)
            {
                bonusAllScaling -= 0.10f;
                champion.attackDamage -= 25f;
                champion.abilityPower -= 25f;
                champion.moveSpeed -= 0.6f;
                isAlpha = hasBleed;
                isOmega = hasHealOnCast;
            }

            // Undo current branch
            if (isAlpha)
            {
                bonusCritDamage -= 0.15f;
                champion.critChance -= 0.10f;
                hasBleed = false;
                champion.moveSpeed -= 0.4f;
            }
            else if (isOmega)
            {
                bonusDamageReflect -= 0.10f;
                champion.damageReflection -= 0.10f;
                bonusMagicResist -= 20f;
                champion.magicResist -= 20f;
                hasHealOnCast = false;
                champion.moveSpeed -= 0.4f;
            }

            champion.SpendBioCurrency(totalCost);

            // Apply opposite branch
            if (isAlpha)
            {
                tier = MutationTier.Omega;
                bonusDamageReflect += 0.10f;
                champion.damageReflection += 0.10f;
                bonusMagicResist += 20f;
                champion.magicResist += 20f;
                hasHealOnCast = true;
                champion.moveSpeed += 0.4f;
            }
            else
            {
                tier = MutationTier.Alpha;
                bonusCritDamage += 0.15f;
                champion.critChance += 0.10f;
                hasBleed = true;
                champion.moveSpeed += 0.4f;
            }

            // If was Apex, re-apply Apex on the new branch
            if (isApex)
            {
                tier = MutationTier.Apex;
                bonusAllScaling += 0.10f;
                champion.attackDamage += 25f;
                champion.abilityPower += 25f;
                champion.moveSpeed += 0.6f;
            }

            return totalCost;
        }

        private int GetNextTierIndex()
        {
            switch (tier)
            {
                case MutationTier.Locked: return 1;
                case MutationTier.Base: return 2;
                case MutationTier.Potency: return 3;
                case MutationTier.Alpha: return 5;
                case MutationTier.Omega: return 5;
                default: return -1;
            }
        }
    }
}
