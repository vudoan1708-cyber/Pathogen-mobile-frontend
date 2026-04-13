using UnityEngine;
using System;

namespace Pathogen
{
    public class Champion : Entity
    {
        [Header("Champion Identity")]
        public string championName = "Champion";
        public float sightRange = 12f;
        public float championHeight = 1f;

        [Header("Mana")]
        public float maxMana = 200f;
        public float currentMana;
        public float manaRegen = 3f;

        [Header("Leveling")]
        public int level = 1;
        public int maxLevel = 18;
        public float currentXP;
        public float xpToNextLevel = 100f;
        public float xpScalePerLevel = 50f;

        [Header("Level-Up Stat Gains")]
        public float healthPerLevel = 50f;
        public float manaPerLevel = 25f;
        public float attackDamagePerLevel = 3f;
        public float armorPerLevel = 2f;
        public float magicResistPerLevel = 1.5f;

        [Header("Economy")]
        public float bioCurrency;
        public float bioPerMinionKill = 25f;
        public float bioPerChampionKill = 300f;
        public float xpPerMinionKill = 40f;
        public float xpPerChampionKill = 200f;

        [Header("Skills")]
        public Skill[] skills = new Skill[4];

        public ChampionStats Stats { get; set; }

        [Header("State")]
        public bool isDashing;
        public bool isBuffed;
        public float buffTimer;
        public float buffedAttackDamage;
        public float buffedMoveSpeed;

        [Header("Respawn")]
        public float respawnTime = 8f;
        public float respawnTimer;
        public Vector3 spawnPoint;
        public bool isRespawning;

        private Color originalColor;

        // Events
        public event Action<int> OnLevelUp;
        public event Action<float> OnBioCurrencyChanged;
        public event Action<float, float> OnManaChanged;
        public event Action OnRespawn;
        public event Action<float> OnRespawnTimerChanged; // Fires each second with remaining time

        protected override void Awake()
        {
            base.Awake();
            entityType = EntityType.Champion;
            currentMana = maxMana;

            var r = GetComponent<Renderer>();
            if (r != null) originalColor = r.material.color;
        }

        protected override void Update()
        {
            if (isRespawning)
            {
                UpdateRespawnTimer();
                return;
            }

            base.Update();
            if (IsDead) return;

            // Mana regen
            if (currentMana < maxMana)
            {
                currentMana = Mathf.Min(maxMana, currentMana + manaRegen * Time.deltaTime);
                OnManaChanged?.Invoke(currentMana, maxMana);
            }

            // Skill cooldowns
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i] != null)
                    skills[i].UpdateCooldown(Time.deltaTime);
            }

            // Buff timer
            if (isBuffed)
            {
                buffTimer -= Time.deltaTime;
                if (buffTimer <= 0f)
                    RemoveBuff();
            }
        }

        public void InitializeSkills(SkillDefinition[] definitions)
        {
            for (int i = 0; i < 4 && i < definitions.Length; i++)
            {
                skills[i] = new Skill(definitions[i]);
                // Unlock Q (skill 0) at game start for free
                if (i == 0)
                    skills[i].tier = MutationTier.Base;
            }
        }

        public bool UseSkill(int index, Vector3 direction, Vector3 targetPosition)
        {
            if (index < 0 || index >= skills.Length) return false;
            var skill = skills[index];
            if (skill == null || !skill.IsReady) return false;
            if (currentMana < skill.definition.manaCost) return false;

            currentMana -= skill.definition.manaCost;
            OnManaChanged?.Invoke(currentMana, maxMana);
            skill.StartCooldown(cooldownReduction);

            // Heal on cast (Omega mutation bonus)
            if (skill.hasHealOnCast)
                Heal(maxHealth * 0.03f);

            ExecuteSkill(skill, direction, targetPosition);
            return true;
        }

        private void ExecuteSkill(Skill skill, Vector3 direction, Vector3 targetPosition)
        {
            var def = skill.definition;

            switch (def.type)
            {
                case SkillType.Projectile:
                    SpawnProjectile(skill, direction);
                    break;

                case SkillType.Dash:
                    StartDash(skill, direction);
                    break;

                case SkillType.AreaOfEffect:
                    PerformAOE(skill);
                    break;

                case SkillType.SelfBuff:
                    ApplyBuff(skill);
                    break;
            }
        }

        private void SpawnProjectile(Skill skill, Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
                direction = transform.forward;

            direction.y = 0f;
            direction.Normalize();

            var def = skill.definition;
            var vis = def.visuals;

            var projGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projGO.name = "SkillProjectile";
            projGO.transform.position = transform.position + direction * 1.5f + Vector3.up * 0.5f;
            projGO.transform.localScale = Vector3.one * vis.scale;

            var renderer = projGO.GetComponent<Renderer>();
            renderer.material.color = vis.primaryColor;

            var proj = projGO.AddComponent<Projectile>();
            proj.Initialize(this, direction, skill.GetDamage(), def.projectileSpeed,
                           def.range, def.isMagicDamage, def.piercing, vis);

            DestroyImmediate(projGO.GetComponent<SphereCollider>());
            var col = projGO.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1f;

            var rb = projGO.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

        private void StartDash(Skill skill, Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
                direction = transform.forward;

            direction.y = 0f;
            direction.Normalize();
            isDashing = true;

            var dashTarget = transform.position + direction * skill.definition.dashDistance;
            StartCoroutine(DashCoroutine(dashTarget, skill));
        }

        private System.Collections.IEnumerator DashCoroutine(Vector3 target, Skill skill)
        {
            float elapsed = 0f;
            float duration = skill.definition.dashDistance / skill.definition.dashSpeed;
            Vector3 start = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            transform.position = target;
            isDashing = false;

            var vis = skill.definition.visuals;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, 3f, team == Team.Virus ? Team.Immune : Team.Virus);
            foreach (var enemy in enemies)
            {
                if (enemy.entityType != EntityType.Structure)
                    enemy.TakeDamage(skill.GetDamage(), skill.definition.isMagicDamage, this, vis);
            }
        }

        private void PerformAOE(Skill skill)
        {
            var vis = skill.definition.visuals;
            Team enemyTeam = team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, skill.definition.aoeRadius, enemyTeam);

            foreach (var enemy in enemies)
            {
                if (enemy.entityType != EntityType.Structure)
                    enemy.TakeDamage(skill.GetDamage(), skill.definition.isMagicDamage, this, vis);
            }

            StartCoroutine(AOEVisualCoroutine(skill));
        }

        private System.Collections.IEnumerator AOEVisualCoroutine(Skill skill)
        {
            var def = skill.definition;
            var vis = def.visuals;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "AOEVisual";
            visual.transform.position = transform.position;
            visual.transform.localScale = Vector3.one * def.aoeRadius * 2f;

            DestroyImmediate(visual.GetComponent<SphereCollider>());

            var renderer = visual.GetComponent<Renderer>();
            Color color = new Color(vis.primaryColor.r, vis.primaryColor.g, vis.primaryColor.b, 0.3f);
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.SetFloat("_Surface", 1);
            renderer.material.color = color;

            yield return new WaitForSeconds(0.5f);
            Destroy(visual);
        }

        private void ApplyBuff(Skill skill)
        {
            isBuffed = true;
            buffTimer = skill.definition.buffDuration;
            buffedAttackDamage = skill.definition.buffAttackDamage;
            buffedMoveSpeed = skill.definition.buffMoveSpeed;
            attackDamage += buffedAttackDamage;
            moveSpeed += buffedMoveSpeed;
        }

        private void RemoveBuff()
        {
            isBuffed = false;
            attackDamage -= buffedAttackDamage;
            moveSpeed -= buffedMoveSpeed;
            buffedAttackDamage = 0f;
            buffedMoveSpeed = 0f;
        }

        public void AddXP(float amount)
        {
            if (level >= maxLevel) return;

            currentXP += amount;
            while (currentXP >= xpToNextLevel && level < maxLevel)
            {
                currentXP -= xpToNextLevel;
                LevelUp();
            }
        }

        private void LevelUp()
        {
            level++;
            maxHealth += healthPerLevel;
            currentHealth += healthPerLevel;
            maxMana += manaPerLevel;
            currentMana += manaPerLevel;
            attackDamage += attackDamagePerLevel;
            armor += armorPerLevel;
            magicResist += magicResistPerLevel;
            xpToNextLevel = 100f + (level - 1) * xpScalePerLevel;

            InvokeHealthChanged();
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnLevelUp?.Invoke(level);
        }

        public void AddGold(float amount)
        {
            bioCurrency += amount;
            OnBioCurrencyChanged?.Invoke(bioCurrency);
        }

        public void SpendBioCurrency(float amount)
        {
            bioCurrency -= amount;
            OnBioCurrencyChanged?.Invoke(bioCurrency);
        }

        public void GrantKillRewards(Entity killed)
        {
            if (killed.entityType == EntityType.Minion)
            {
                AddGold(bioPerMinionKill);
                AddXP(xpPerMinionKill);
            }
            else if (killed.entityType == EntityType.Champion)
            {
                AddGold(bioPerChampionKill);
                AddXP(xpPerChampionKill);
            }
        }

        protected override void Die(Entity killer)
        {
            base.Die(killer);

            if (isBuffed) RemoveBuff();

            // Clear all combat state
            isDashing = false;
            currentTarget = null;
            attackCooldown = 0f;

            // Reset all skill cooldowns
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i] != null)
                    skills[i].currentCooldown = 0f;
            }

            // Start respawn countdown
            isRespawning = true;
            respawnTimer = respawnTime + (level * 0.5f);
            lastRespawnSecond = -1;

            var deathRenderer = GetComponent<Renderer>();
            if (deathRenderer != null)
                deathRenderer.material.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            GetComponent<Collider>().enabled = false;
        }

        private int lastRespawnSecond;

        private void UpdateRespawnTimer()
        {
            respawnTimer -= Time.deltaTime;

            // Fire event each whole second for countdown display
            int currentSecond = Mathf.CeilToInt(respawnTimer);
            if (currentSecond != lastRespawnSecond && currentSecond >= 0)
            {
                lastRespawnSecond = currentSecond;
                OnRespawnTimerChanged?.Invoke(respawnTimer);
            }

            if (respawnTimer <= 0f)
                Respawn();
        }

        private void Respawn()
        {
            isRespawning = false;

            // Restore health and mana
            currentHealth = maxHealth;
            currentMana = maxMana;

            // Reset position to base
            transform.position = spawnPoint;
            transform.rotation = Quaternion.identity;

            // Clear movement/combat state so champion starts idle
            isDashing = false;
            currentTarget = null;
            attackCooldown = 0f;

            // Show champion
            var r = GetComponent<Renderer>();
            if (r != null) r.material.color = originalColor;
            GetComponent<Collider>().enabled = true;

            InvokeHealthChanged();
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnRespawn?.Invoke();
        }
    }
}
