using UnityEngine;
using System;

namespace Pathogen
{
    public enum EntityType { Champion, Minion, Structure, Objective }

    public class Entity : MonoBehaviour
    {
        [Header("Identity")]
        public Team team;
        public EntityType entityType;
        public string entityName = "Entity";

        [Header("Base Stats")]
        public float maxHealth = 500f;
        public float currentHealth;
        public float attackDamage = 50f;
        public float abilityPower = 0f;
        public float attackSpeed = 1f;
        public float attackRange = 3f;
        public float moveSpeed = 5f;
        public float armor = 10f;
        public float magicResist = 10f;

        [Header("Regen")]
        public float healthRegen = 0f; // 0 by default — only champions regen

        [Header("Combat State")]
        public Entity currentTarget;
        public float attackCooldown;
        public bool IsDead => currentHealth <= 0f;

        // Upgrade-based passives
        public float omnivamp;
        public float damageReflection;
        public float critChance;
        public float critMultiplier = 1.5f;
        public float cooldownReduction;

        // Events
        public event Action<Entity> OnDeath;
        public event Action<float, float> OnHealthChanged;

        // Protected helpers so subclasses can invoke events
        protected void InvokeHealthChanged() => OnHealthChanged?.Invoke(currentHealth, maxHealth);
        protected void InvokeDeath() => OnDeath?.Invoke(this);

        protected virtual void Awake()
        {
            // Don't set currentHealth here — let the spawner set both maxHealth and currentHealth
            // before Start() runs. If currentHealth is 0 (unset), default to maxHealth.
        }

        protected virtual void Start()
        {
            // If spawner didn't set currentHealth, default to maxHealth
            if (currentHealth <= 0f)
                currentHealth = maxHealth;

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterEntity(this);

            // Fire initial health event so health bars display correctly
            InvokeHealthChanged();
        }

        protected virtual void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterEntity(this);
        }

        protected virtual void Update()
        {
            if (IsDead) return;

            // Health regen (only if healthRegen > 0)
            if (healthRegen > 0f && currentHealth < maxHealth)
            {
                currentHealth = Mathf.Min(maxHealth, currentHealth + healthRegen * Time.deltaTime);
                InvokeHealthChanged();
            }

            // Attack cooldown
            if (attackCooldown > 0f)
                attackCooldown -= Time.deltaTime;
        }

        public float TakeDamage(float amount, bool isMagic, Entity source, SkillVisuals visuals = null)
        {
            if (IsDead) return 0f;

            // Apply team condition multiplier
            float multiplier = 1f;
            if (source != null && GameManager.Instance != null)
                multiplier = GameManager.Instance.GetTeamDamageMultiplier(source.team);

            float rawDamage = amount * multiplier;

            // Armor/MR reduction
            float resist = isMagic ? magicResist : armor;
            float reduction = resist / (resist + 100f);
            float finalDamage = rawDamage * (1f - reduction);

            // Crit (for physical)
            if (!isMagic && source != null && source.critChance > 0f)
            {
                if (UnityEngine.Random.value < source.critChance)
                    finalDamage *= source.critMultiplier;
            }

            currentHealth = Mathf.Max(0f, currentHealth - finalDamage);
            InvokeHealthChanged();

            // Only spawn hit particles for skill damage (visuals provided), not auto-attacks
            if (visuals != null)
                HitEffect.Spawn(transform.position, isMagic, visuals);

            // If an enemy champion hits a champion, alert nearby allied minions
            if (entityType == EntityType.Champion && source != null
                && source.entityType == EntityType.Champion && GameManager.Instance != null)
            {
                var nearbyAllies = GameManager.Instance.GetEntitiesInRange(transform.position, 10f, team);
                foreach (var ally in nearbyAllies)
                {
                    var minion = ally as Minion;
                    if (minion != null)
                        minion.OnAllyChampionAttacked(source);
                }
            }

            // Damage reflection
            if (damageReflection > 0f && source != null && !source.IsDead)
                source.TakeRawDamage(finalDamage * damageReflection);

            // Omnivamp for source
            if (source != null && source.omnivamp > 0f)
                source.Heal(finalDamage * source.omnivamp);

            if (currentHealth <= 0f)
                Die(source);

            return finalDamage;
        }

        public void TakeRawDamage(float amount)
        {
            if (IsDead) return;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            InvokeHealthChanged();
            if (currentHealth <= 0f)
                Die(null);
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            InvokeHealthChanged();
        }

        protected virtual void Die(Entity killer)
        {
            InvokeDeath();

            if (GameManager.Instance == null || killer == null) return;

            var gm = GameManager.Instance;

            // Health shift — minion kills don't affect human health
            float shift = 0f;
            switch (entityType)
            {
                case EntityType.Champion:
                    shift = gm.GetChampionKillShift(killer.team);
                    gm.RecordChampionTakedown(killer.team);
                    break;
                case EntityType.Structure:
                    shift = gm.structureDestroyShift;
                    break;
            }
            if (shift > 0f)
                gm.ShiftHumanHealth(shift, killer.team);

            // Shared gold & XP for all kills (minions, champions, structures)
            float gold = 0f;
            float xp = 0f;
            switch (entityType)
            {
                case EntityType.Minion: gold = 25f; xp = 40f; break;
                case EntityType.Champion: gold = 300f; xp = 200f; break;
                case EntityType.Structure: gold = 150f; xp = 100f; break;
            }
            if (gold > 0f)
            {
                gm.DistributeGold(transform.position, killer.team, gold);
                FloatingText.Spawn(
                    transform.position,
                    $"+{Mathf.FloorToInt(gold)}",
                    new Color(1f, 0.85f, 0.2f));
            }
            if (xp > 0f)
                gm.DistributeXP(transform.position, killer.team, xp);
        }

        public bool CanAttack()
        {
            return !IsDead && attackCooldown <= 0f;
        }

        public void PerformAutoAttack(Entity target)
        {
            if (target == null || target.IsDead || !CanAttack()) return;

            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist > attackRange) return;

            attackCooldown = 1f / attackSpeed;
            target.TakeDamage(attackDamage, false, this);
        }
    }

    /// <summary>
    /// Spawns hit particles on damage. Accepts optional SkillVisuals for per-skill look.
    /// Falls back to gold (physical) / purple (magic) when no visuals provided.
    /// </summary>
    public static class HitEffect
    {
        public static void Spawn(Vector3 position, bool isMagic, SkillVisuals visuals = null)
        {
            Color color = visuals != null ? visuals.particleColor
                : (isMagic ? new Color(0.6f, 0.3f, 1f) : new Color(1f, 0.8f, 0.2f));
            int count = visuals != null ? visuals.particleCount : 4;
            float size = visuals != null ? visuals.particleSize : 0.12f;
            float force = visuals != null ? visuals.particleForce : 3f;
            float lifetime = visuals != null ? visuals.particleLifetime : 0.6f;

            for (int i = 0; i < count; i++)
            {
                var particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                particle.name = "HitParticle";
                particle.transform.position = position + Vector3.up * 0.5f;
                particle.transform.localScale = Vector3.one * size;

                UnityEngine.Object.Destroy(particle.GetComponent<BoxCollider>());

                var renderer = particle.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                renderer.material.color = color;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                var rb = particle.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.mass = 0.1f;
                Vector3 dir = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(0.5f, 1.5f),
                    UnityEngine.Random.Range(-1f, 1f));
                rb.AddForce(dir * force, ForceMode.Impulse);

                UnityEngine.Object.Destroy(particle, lifetime);
            }
        }
    }
}
