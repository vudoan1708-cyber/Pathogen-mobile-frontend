using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// A capturable organ objective on the map.
    /// - Virus team attacks it to capture (reduce HP to 0) → shifts human health negatively
    /// - Immune team protects it — if the timer expires without capture, immune team gets an advantage
    /// - At 50% HP, a Neutrophil Berserker spawns and attacks both sides
    /// - Each objective phase lasts 4-5 minutes
    /// </summary>
    public class OrganObjectiveEntity : Entity
    {
        [Header("Organ Identity")]
        public int organIndex; // Index into GameManager.organs[]
        public string organName;
        public float healthShiftOnCapture;

        [Header("Phase Timer")]
        public float phaseDuration = 270f; // 4.5 minutes default
        public float phaseTimer;
        public bool phaseActive;

        [Header("Neutrophil Spawn")]
        public bool neutrophilSpawned;
        public float neutrophilHealthThreshold = 0.5f;

        [Header("Immune Advantage on Timeout")]
        public float immuneTimeoutHealthBoost = 3f;
        public float immuneTimeoutDamageBuffDuration = 60f;

        // Events
        public event System.Action<OrganObjectiveEntity> OnOrganCaptured;
        public event System.Action<OrganObjectiveEntity> OnPhaseTimeout;
        public event System.Action<OrganObjectiveEntity> OnNeutrophilSpawn;

        protected override void Awake()
        {
            base.Awake();
            entityType = EntityType.Objective;
        }

        public void StartPhase(int index)
        {
            if (GameManager.Instance == null) return;
            var organs = GameManager.Instance.organs;
            if (index < 0 || index >= organs.Length) return;

            organIndex = index;
            organName = organs[index].name;
            healthShiftOnCapture = organs[index].healthShift;
            entityName = organName;
            gameObject.name = organName;

            // Set HP based on organ importance (higher value = more HP)
            maxHealth = 800f + healthShiftOnCapture * 40f;
            currentHealth = maxHealth;

            phaseTimer = phaseDuration;
            phaseActive = true;
            neutrophilSpawned = false;

            // Update health bar label
            InvokeHealthChanged();
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead || !phaseActive) return;

            // Phase timer countdown
            phaseTimer -= Time.deltaTime;
            if (phaseTimer <= 0f)
            {
                // Immune team wins the phase — virus failed to capture in time
                OnPhaseExpired();
                return;
            }

            // Neutrophil spawn at 50% HP
            if (!neutrophilSpawned && currentHealth <= maxHealth * neutrophilHealthThreshold)
            {
                neutrophilSpawned = true;
                SpawnNeutrophilBerserker();
            }
        }

        /// <summary>
        /// Only the virus team can damage organs. Immune team defends, not attacks.
        /// </summary>
        public new float TakeDamage(float amount, bool isMagic, Entity source, SkillVisuals visuals = null)
        {
            if (source == null || source.team != Team.Virus) return 0f;
            return base.TakeDamage(amount, isMagic, source, visuals);
        }

        protected override void Die(Entity killer)
        {
            phaseActive = false;

            // Organ captured by virus team — shift human health
            if (GameManager.Instance != null)
                GameManager.Instance.ShiftHumanHealth(healthShiftOnCapture, Team.Virus);

            OnOrganCaptured?.Invoke(this);

            // Visual: shrink and darken
            StartCoroutine(CapturedVisualCoroutine());
        }

        private void OnPhaseExpired()
        {
            phaseActive = false;

            // Immune team advantage — small health boost
            if (GameManager.Instance != null)
                GameManager.Instance.ShiftHumanHealth(immuneTimeoutHealthBoost, Team.Immune);

            OnPhaseTimeout?.Invoke(this);

            // Restore organ to full health (it survived)
            currentHealth = maxHealth;
            InvokeHealthChanged();
        }

        private void SpawnNeutrophilBerserker()
        {
            OnNeutrophilSpawn?.Invoke(this);

            // Get organ-specific defender data
            var organs = GameManager.Instance.organs;
            var organ = (organIndex >= 0 && organIndex < organs.Length) ? organs[organIndex] : null;

            string creatureName = organ != null ? organ.defenderName : "Neutrophil Berserker";
            float hp = organ != null ? organ.defenderHealth : 1200f;
            float dmg = organ != null ? organ.defenderDamage : 90f;
            float spd = organ != null ? organ.defenderSpeed : 4f;
            Color color = organ != null ? organ.defenderColor : new Color(1f, 0.6f, 0f);

            Vector3 spawnPos = transform.position + new Vector3(
                Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));

            var creatureGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            creatureGO.name = creatureName;
            creatureGO.transform.position = spawnPos;
            creatureGO.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            creatureGO.GetComponent<Renderer>().material.color = color;

            var rb = creatureGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var berserker = creatureGO.AddComponent<NeutrophilBerserker>();
            berserker.entityName = creatureName;
            berserker.maxHealth = hp;
            berserker.currentHealth = hp;
            berserker.attackDamage = dmg;
            berserker.attackSpeed = 1.2f;
            berserker.attackRange = 3f;
            berserker.moveSpeed = spd;
            berserker.armor = 25f;
            berserker.magicResist = 25f;
            berserker.roamCenter = transform.position;

            var hbar = creatureGO.AddComponent<FloatingHealthBar>();
            hbar.heightOffset = 1.5f;
            hbar.barWidth = 1.2f;

            creatureGO.AddComponent<TargetHighlight>();
        }

        private System.Collections.IEnumerator CapturedVisualCoroutine()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer == null) yield break;

            float t = 0f;
            Color startColor = renderer.material.color;
            Color endColor = new Color(0.2f, 0.05f, 0.05f);
            Vector3 startScale = transform.localScale;

            while (t < 1f)
            {
                t += Time.deltaTime;
                renderer.material.color = Color.Lerp(startColor, endColor, t);
                transform.localScale = Vector3.Lerp(startScale, startScale * 0.6f, t);
                yield return null;
            }
        }

        /// <summary>
        /// Get remaining phase time as formatted string.
        /// </summary>
        public string GetTimerString()
        {
            int minutes = Mathf.FloorToInt(phaseTimer / 60f);
            int seconds = Mathf.FloorToInt(phaseTimer % 60f);
            return $"{minutes}:{seconds:D2}";
        }
    }
}
