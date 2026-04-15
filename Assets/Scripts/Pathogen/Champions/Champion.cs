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
        public int maxLevel = 12;
        public float currentXP;
        public float xpToNextLevel = 100f;
        public float xpScalePerLevel = 50f;
        public int pendingSkillPoints;

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

        [Header("Recall")]
        public bool isRecalling;
        public float recallTimer;
        public const float RecallDuration = 8f;

        private Color originalColor;

        // Ultimate rank-up level gates (unlock at 4, upgrade at 8 and 12)
        public static readonly int[] UltimateLevelGates = { 4, 8, 12 };

        // Events
        public event Action<int> OnLevelUp;
        public event Action<int> OnSkillPointsChanged;    // Fires with current pendingSkillPoints
        public event Action<float> OnBioCurrencyChanged;
        public event Action<float, float> OnManaChanged;
        public event Action OnRespawn;
        public event Action<float> OnRespawnTimerChanged; // Fires each second with remaining time
        public event Action OnRecallStarted;
        public event Action<float> OnRecallProgress;      // Fires with normalized progress 0..1
        public event Action OnRecallCompleted;
        public event Action OnRecallCancelled;

        public void InvokeManaChanged() => OnManaChanged?.Invoke(currentMana, maxMana);

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

            if (isRecalling)
            {
                TickRecallChannel();
                return;
            }

            RegenerateMana();
            TickSkillCooldowns();
            TickBuffDuration();
        }

        private void RegenerateMana()
        {
            if (currentMana >= maxMana) return;
            currentMana = Mathf.Min(maxMana, currentMana + manaRegen * Time.deltaTime);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        private void TickSkillCooldowns()
        {
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i] != null)
                    skills[i].UpdateCooldown(Time.deltaTime);
            }
        }

        private void TickBuffDuration()
        {
            if (!isBuffed) return;
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
                RemoveBuff();
        }

        private void TickRecallChannel()
        {
            recallTimer -= Time.deltaTime;
            OnRecallProgress?.Invoke(1f - recallTimer / RecallDuration);

            if (recallTimer <= 0f)
                CompleteRecall();
        }

        public void InitializeSkills(SkillDefinition[] definitions)
        {
            for (int i = 0; i < 4 && i < definitions.Length; i++)
                skills[i] = new Skill(definitions[i]);

            // Grant first skill point — player chooses which skill to unlock
            pendingSkillPoints = 1;
            OnSkillPointsChanged?.Invoke(pendingSkillPoints);
        }

        public bool UseSkill(int index, Vector3 direction, Vector3 targetPosition)
        {
            if (index < 0 || index >= skills.Length) return false;
            var skill = skills[index];
            if (skill == null || !skill.IsReady) return false;
            if (currentMana < skill.definition.manaCost) return false;

            if (isRecalling) CancelRecall();

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
                    PerformAOE(skill, targetPosition);
                    break;

                case SkillType.SelfBuff:
                    ApplyBuff(skill);
                    break;

                case SkillType.Target:
                    PerformTargeted(skill, targetPosition);
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
            // Copy: TakeDamage can kill entities and modify the source list
            var snapshot = enemies.ToArray();
            foreach (var enemy in snapshot)
            {
                if (enemy != null && !enemy.IsDead && enemy.entityType != EntityType.Structure)
                    enemy.TakeDamage(skill.GetDamage(), skill.definition.isMagicDamage, this, vis);
            }
        }

        private void PerformAOE(Skill skill, Vector3 targetPosition)
        {
            // Use target position if provided, otherwise self-centered
            Vector3 center = targetPosition.sqrMagnitude > 0.01f ? targetPosition : transform.position;
            center.y = transform.position.y;

            var vis = skill.definition.visuals;
            Team enemyTeam = team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                center, skill.definition.aoeRadius, enemyTeam);

            var snapshot = enemies.ToArray();
            foreach (var enemy in snapshot)
            {
                if (enemy != null && !enemy.IsDead && enemy.entityType != EntityType.Structure)
                    enemy.TakeDamage(skill.GetDamage(), skill.definition.isMagicDamage, this, vis);
            }

            StartCoroutine(AOEVisualCoroutine(skill, center));
        }

        private System.Collections.IEnumerator AOEVisualCoroutine(Skill skill, Vector3 center)
        {
            var def = skill.definition;
            var vis = def.visuals;

            var visual = new GameObject("AOEVisual");
            visual.transform.position = new Vector3(center.x, 0.05f, center.z);

            float diameter = def.aoeRadius * 2f;
            visual.transform.localScale = new Vector3(diameter, diameter, diameter);

            var filter = visual.AddComponent<MeshFilter>();
            filter.sharedMesh = SkillAimIndicator.SharedDiscMesh;

            var bioPulseShader = Shader.Find("Pathogen/BioPulse");
            if (bioPulseShader == null)
                bioPulseShader = Shader.Find("Universal Render Pipeline/Unlit");

            var mat = new Material(bioPulseShader);
            mat.SetColor("_Color", vis.primaryColor);
            mat.SetFloat("_FillAlpha", 0.12f);
            mat.SetFloat("_EdgeAlpha", 0.85f);
            mat.SetFloat("_PulseSpeed", 2.5f);
            mat.SetFloat("_PulseIntensity", 0.3f);
            mat.renderQueue = 3010;

            var renderer = visual.AddComponent<MeshRenderer>();
            renderer.material = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            yield return new WaitForSeconds(0.5f);
            Destroy(visual);
        }

        private void PerformTargeted(Skill skill, Vector3 targetPosition)
        {
            var def = skill.definition;
            var vis = def.visuals;
            Team enemyTeam = team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(targetPosition, 2f, enemyTeam);

            Entity closest = null;
            float closestDist = float.MaxValue;
            foreach (var enemy in enemies)
            {
                if (enemy.IsDead || enemy.entityType == EntityType.Structure) continue;
                float dist = (enemy.transform.position - targetPosition).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
                closest.TakeDamage(skill.GetDamage(), def.isMagicDamage, this, vis);
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

            // Grant a skill rank-up point (free, player chooses which skill)
            pendingSkillPoints++;
            OnSkillPointsChanged?.Invoke(pendingSkillPoints);

            InvokeHealthChanged();
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnLevelUp?.Invoke(level);
        }

        /// <summary>
        /// Can the player put a skill point into this slot right now?
        /// </summary>
        public bool CanRankUpSkill(int index)
        {
            if (pendingSkillPoints <= 0) return false;
            var skill = skills[index];
            if (skill == null) return false;
            if (skill.skillLevel >= Skill.MaxSkillRank) return false;

            // Ultimate (index 3): gated to specific champion levels
            if (index == 3)
            {
                int requiredLevel = skill.skillLevel < UltimateLevelGates.Length
                    ? UltimateLevelGates[skill.skillLevel]
                    : int.MaxValue;
                if (level < requiredLevel) return false;
            }

            return true;
        }

        /// <summary>
        /// Spend one pending skill point to unlock / rank up a skill.
        /// Returns true on success.
        /// </summary>
        public bool AllocateSkillPoint(int index)
        {
            if (!CanRankUpSkill(index)) return false;

            var skill = skills[index];

            if (!skill.IsUnlocked)
                skill.tier = MutationTier.Base;

            skill.skillLevel++;
            pendingSkillPoints--;
            OnSkillPointsChanged?.Invoke(pendingSkillPoints);
            return true;
        }

        // ─── RECALL ──────────────────────────────────────────────────────

        public void StartRecall()
        {
            if (IsDead || isRespawning || isRecalling || isDashing) return;

            isRecalling = true;
            recallTimer = RecallDuration;
            currentTarget = null;
            OnRecallStarted?.Invoke();
        }

        public void CancelRecall()
        {
            if (!isRecalling) return;

            isRecalling = false;
            recallTimer = 0f;
            OnRecallCancelled?.Invoke();
        }

        private void CompleteRecall()
        {
            isRecalling = false;
            recallTimer = 0f;

            transform.position = spawnPoint;
            currentTarget = null;
            isDashing = false;

            OnRecallCompleted?.Invoke();
        }

        // ─── ECONOMY ────────────────────────────────────────────────────

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

            if (isRecalling) CancelRecall();
            if (isBuffed) RemoveBuff();

            isDashing = false;
            currentTarget = null;
            attackCooldown = 0f;

            // Reset all skill cooldowns
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i] != null)
                    skills[i].currentCooldown = 0f;
            }

            // Start respawn countdown — quadratic scaling: round(4.75 + 0.25 × level²)
            isRespawning = true;
            respawnTimer = Mathf.Round(4.75f + 0.25f * level * level);
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
