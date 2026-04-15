using UnityEngine;

namespace Pathogen
{
    public static class ChampionRoster
    {
        public static ChampionDefinition[] GetAll()
        {
            return new ChampionDefinition[]
            {
                Pathobyte(),
                Immunix()
            };
        }

        public static ChampionDefinition Get(string name)
        {
            var all = GetAll();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].championName == name)
                    return all[i];
            }
            return all[0];
        }

        // ─── VIRUS CHAMPIONS ────────────────────────────────────────────

        public static ChampionDefinition Pathobyte()
        {
            return new ChampionDefinition
            {
                championName = "Pathobyte",
                color = new Color(0.9f, 0.2f, 0.3f),
                maxHealth = 500f,
                maxMana = 250f,
                attackDamage = 50f,
                attackSpeed = 1f,
                attackRange = 2.5f,
                moveSpeed = 3.5f,
                armor = 15f,
                magicResist = 12f,
                healthRegen = 2f,
                manaRegen = 4f,
                skills = new SkillDefinition[]
                {
                    new SkillDefinition
                    {
                        skillName = "Toxic Spit",
                        description = "Fire a toxic projectile that damages the first enemy hit.",
                        type = SkillType.Projectile,
                        baseDamage = 150f, cooldown = 4f, manaCost = 40f,
                        range = 12f, projectileSpeed = 14f, isMagicDamage = true,
                        piercing = ProjectilePiercing.StopOnFirst,
                        indicatorShape = IndicatorShape.Rectangle, skillWidth = 1.2f,
                        visuals = new SkillVisuals
                        {
                            primaryColor = new Color(0.7f, 0f, 0.9f),
                            scale = 0.35f,
                            hasTrail = true,
                            trailColor = new Color(0.5f, 0f, 0.6f, 0.6f),
                            trailWidth = 0.12f,
                            particleColor = new Color(0.6f, 0f, 0.8f),
                            particleCount = 5, particleSize = 0.1f, particleForce = 4f,
                            aimColor = new Color(0.7f, 0.2f, 1f, 0.6f)
                        }
                    },
                    new SkillDefinition
                    {
                        skillName = "Viral Surge",
                        description = "Dash forward, dealing damage to enemies at the destination.",
                        type = SkillType.Dash,
                        baseDamage = 60f, cooldown = 10f, manaCost = 50f,
                        dashDistance = 7f, dashSpeed = 22f, isMagicDamage = false,
                        indicatorShape = IndicatorShape.Line,
                        visuals = new SkillVisuals
                        {
                            primaryColor = new Color(0.9f, 0.2f, 0.1f),
                            hasTrail = true,
                            trailColor = new Color(0.8f, 0.1f, 0f, 0.5f),
                            trailWidth = 0.2f,
                            particleColor = new Color(1f, 0.4f, 0.1f),
                            particleCount = 6, particleSize = 0.14f, particleForce = 5f,
                            aimColor = new Color(1f, 0.3f, 0.1f, 0.6f)
                        }
                    },
                    new SkillDefinition
                    {
                        skillName = "Infection Aura",
                        description = "Release a burst of infection, damaging all nearby enemies.",
                        type = SkillType.AreaOfEffect,
                        baseDamage = 100f, cooldown = 12f, manaCost = 60f,
                        range = 5f, aoeRadius = 2.5f, isMagicDamage = true,
                        indicatorShape = IndicatorShape.Circle,
                        visuals = new SkillVisuals
                        {
                            primaryColor = new Color(0.4f, 0.8f, 0f),
                            particleColor = new Color(0.5f, 0.9f, 0.1f),
                            particleCount = 8, particleSize = 0.1f, particleForce = 3f,
                            aimColor = new Color(0.4f, 0.9f, 0.2f, 0.5f)
                        }
                    },
                    new SkillDefinition
                    {
                        skillName = "Mutation",
                        description = "Mutate your cells — temporarily boosting attack and speed.",
                        type = SkillType.SelfBuff,
                        cooldown = 60f, manaCost = 100f, isMagicDamage = false,
                        indicatorShape = IndicatorShape.None,
                        buffAttackDamage = 25f, buffMoveSpeed = 3f, buffDuration = 8f,
                        visuals = new SkillVisuals
                        {
                            primaryColor = new Color(1f, 0.3f, 0f),
                            particleColor = new Color(1f, 0.5f, 0f),
                            particleCount = 10, particleSize = 0.08f, particleForce = 2f
                        }
                    }
                }
            };
        }

        // ─── IMMUNE CHAMPIONS ───────────────────────────────────────────

        public static ChampionDefinition Immunix()
        {
            return new ChampionDefinition
            {
                championName = "Immunix",
                color = new Color(0.2f, 0.6f, 1f),
                maxHealth = 500f,
                maxMana = 250f,
                attackDamage = 50f,
                attackSpeed = 1f,
                attackRange = 2.5f,
                moveSpeed = 3.5f,
                armor = 15f,
                magicResist = 12f,
                healthRegen = 2f,
                manaRegen = 4f,
                skills = new SkillDefinition[]
                {
                    new SkillDefinition
                    {
                        skillName = "Antibody Shot",
                        description = "Fire an antibody projectile that damages the first enemy hit.",
                        type = SkillType.Projectile,
                        baseDamage = 150f, cooldown = 4f, manaCost = 35f,
                        range = 13f, projectileSpeed = 16f, isMagicDamage = true,
                        piercing = ProjectilePiercing.PierceMinions,
                        indicatorShape = IndicatorShape.Rectangle, skillWidth = 1f,
                        visuals = new SkillVisuals
                        {
                            primaryColor = new Color(0.2f, 0.7f, 1f),
                            scale = 0.3f,
                            hasTrail = true,
                            trailColor = new Color(0.1f, 0.5f, 0.9f, 0.6f),
                            trailWidth = 0.1f,
                            particleColor = new Color(0.3f, 0.8f, 1f),
                            particleCount = 4, particleSize = 0.1f, particleForce = 3.5f,
                            aimColor = new Color(0.2f, 0.7f, 1f, 0.6f)
                        }
                    },
                    new SkillDefinition
                    {
                        skillName = "Rapid Response",
                        description = "Dash toward the threat, dealing damage on arrival.",
                        type = SkillType.Dash,
                        baseDamage = 55f, cooldown = 10f, manaCost = 45f,
                        dashDistance = 6f, dashSpeed = 20f, isMagicDamage = false,
                        indicatorShape = IndicatorShape.Line,
                        visuals = new SkillVisuals
                        {
                            primaryColor = new Color(0f, 0.9f, 0.5f),
                            hasTrail = true,
                            trailColor = new Color(0f, 0.7f, 0.4f, 0.5f),
                            trailWidth = 0.18f,
                            particleColor = new Color(0.2f, 1f, 0.6f),
                            particleCount = 5, particleSize = 0.12f, particleForce = 4f,
                            aimColor = new Color(0f, 0.9f, 0.5f, 0.6f)
                        }
                    },
                    new SkillDefinition
                    {
                        skillName = "Purify Wave",
                        description = "Emit a purifying wave that damages nearby enemies.",
                        type = SkillType.AreaOfEffect,
                        baseDamage = 90f, cooldown = 12f, manaCost = 55f,
                        range = 5f, aoeRadius = 2.5f, isMagicDamage = true,
                        indicatorShape = IndicatorShape.Circle,
                        visuals = new SkillVisuals
                        {
                            primaryColor = new Color(0.3f, 0.5f, 1f),
                            particleColor = new Color(0.4f, 0.6f, 1f),
                            particleCount = 7, particleSize = 0.1f, particleForce = 3f,
                            aimColor = new Color(0.3f, 0.5f, 1f, 0.5f)
                        }
                    },
                    new SkillDefinition
                    {
                        skillName = "Immune Response",
                        description = "Activate immune overdrive — boost attack and movement.",
                        type = SkillType.SelfBuff,
                        cooldown = 60f, manaCost = 100f, isMagicDamage = false,
                        indicatorShape = IndicatorShape.None,
                        buffAttackDamage = 20f, buffMoveSpeed = 4f, buffDuration = 8f,
                        visuals = new SkillVisuals
                        {
                            primaryColor = new Color(1f, 1f, 0.3f),
                            particleColor = new Color(1f, 0.9f, 0.4f),
                            particleCount = 10, particleSize = 0.08f, particleForce = 2f
                        }
                    }
                }
            };
        }
    }
}
