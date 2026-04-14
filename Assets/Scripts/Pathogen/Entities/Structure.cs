using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Defensive lane structure. Fires poison projectiles at enemies in range.
    /// Prioritises enemy champion that attacked an allied champion, but only
    /// while that champion remains within attack range.
    /// </summary>
    public class Structure : Entity
    {
        [Header("Structure Settings")]
        public float detectionRange = 5f;

        [Header("Escalating Damage")]
        public int trueDamageThreshold = 3;
        public float trueDamageMultiplier = 1.5f;

        [Header("Projectile")]
        public float projectileSpeed = 12f;

        private Entity attackTarget;
        private Entity lastTarget;
        private int consecutiveHits;
        private float structureAttackTimer;

        // Enemy champion that attacked our ally champion — prioritised while in range
        private Entity aggroChampion;

        // Range ring visualization
        private GameObject rangeRing;
        private Renderer rangeRingRenderer;
        private const float RangeRingRevealBuffer = 3f;
        private static readonly Color rangeRingColor = new Color(1f, 0.7f, 0.15f, 0.25f);

        protected override void Awake()
        {
            base.Awake();
            entityType = EntityType.Structure;
        }

        protected override void Start()
        {
            base.Start();
            CreateRangeRing();
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead) return;

            structureAttackTimer -= Time.deltaTime;

            // Clear aggro if champion is gone
            if (aggroChampion != null && (aggroChampion.IsDead ||
                Vector3.Distance(transform.position, aggroChampion.transform.position) > attackRange))
            {
                aggroChampion = null;
            }

            FindTarget();

            if (attackTarget != null && !attackTarget.IsDead && structureAttackTimer <= 0f)
            {
                float dist = Vector3.Distance(transform.position, attackTarget.transform.position);
                if (dist <= attackRange)
                {
                    if (attackTarget == lastTarget)
                        consecutiveHits++;
                    else
                    {
                        consecutiveHits = 1;
                        lastTarget = attackTarget;
                    }

                    // True damage escalation only applies to champions
                    bool isChampion = attackTarget.entityType == EntityType.Champion;
                    bool isTrueDmg = isChampion && consecutiveHits >= trueDamageThreshold;
                    float damage = isTrueDmg
                        ? attackDamage * trueDamageMultiplier
                        : attackDamage;

                    SpawnPoisonProjectile(attackTarget, damage, isTrueDmg);
                    structureAttackTimer = 1f / attackSpeed;
                }
            }

            UpdateRangeRing();
        }

        /// <summary>
        /// Called when an enemy champion attacks an allied champion near this structure.
        /// Only sets aggro if the enemy is within attack range.
        /// </summary>
        public void OnAllyChampionAttackedBy(Entity enemyChampion)
        {
            if (enemyChampion == null || enemyChampion.IsDead) return;
            if (enemyChampion.team == team) return;

            float dist = Vector3.Distance(transform.position, enemyChampion.transform.position);
            if (dist <= attackRange)
                aggroChampion = enemyChampion;
        }

        private void FindTarget()
        {
            if (GameManager.Instance == null) return;

            Team enemyTeam = team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, detectionRange, enemyTeam);

            if (enemies.Count == 0)
            {
                attackTarget = null;
                return;
            }

            // Priority 1: Aggro champion still in range
            if (aggroChampion != null && !aggroChampion.IsDead)
            {
                float agroDist = Vector3.Distance(transform.position, aggroChampion.transform.position);
                if (agroDist <= attackRange)
                {
                    attackTarget = aggroChampion;
                    return;
                }
                aggroChampion = null;
            }

            // Target nearest enemy — first in range gets hit
            Entity nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }

            attackTarget = nearest;
        }

        // ─── RANGE RING ────────────────────────────────────────────────

        private void CreateRangeRing()
        {
            rangeRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rangeRing.name = "StructureRangeRing";
            DestroyImmediate(rangeRing.GetComponent<CapsuleCollider>());
            rangeRing.transform.SetParent(transform, false);
            rangeRing.transform.localPosition = new Vector3(0f, -transform.localScale.y * 0.5f + 0.03f, 0f);

            float diameter = attackRange * 2f;
            rangeRing.transform.localScale = new Vector3(
                diameter / transform.localScale.x,
                0.01f / transform.localScale.y,
                diameter / transform.localScale.z);

            rangeRingRenderer = rangeRing.GetComponent<Renderer>();
            rangeRingRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            rangeRingRenderer.material.color = rangeRingColor;
            rangeRingRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rangeRing.SetActive(false);
        }

        private void UpdateRangeRing()
        {
            if (rangeRing == null || GameManager.Instance == null) return;

            float revealRange = attackRange + RangeRingRevealBuffer;
            bool showRing = false;

            // Check all teams — show ring when any champion approaches
            var nearby = GameManager.Instance.GetEntitiesInRange(transform.position, revealRange);
            foreach (var e in nearby)
            {
                if (e.IsDead || e.entityType != EntityType.Champion) continue;
                showRing = true;
                break;
            }

            rangeRing.SetActive(showRing);
        }

        // ─── PROJECTILE ────────────────────────────────────────────────

        private void SpawnPoisonProjectile(Entity target, float damage, bool isTrueDmg)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 2f;

            Color poisonColor = isTrueDmg
                ? Color.white
                : new Color(0.3f, 0.85f, 0.1f, 0.9f);
            Color trailColor = isTrueDmg
                ? new Color(1f, 1f, 1f, 0.6f)
                : new Color(0.2f, 0.7f, 0.05f, 0.5f);

            var projGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projGO.name = "PoisonSpit";
            projGO.transform.position = spawnPos;
            projGO.transform.localScale = Vector3.one * (isTrueDmg ? 0.8f : 0.6f);

            // No collider needed — homing projectile hits via distance check
            DestroyImmediate(projGO.GetComponent<SphereCollider>());

            var renderer = projGO.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            renderer.material.color = poisonColor;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var trail = projGO.AddComponent<TrailRenderer>();
            trail.startWidth = isTrueDmg ? 0.6f : 0.45f;
            trail.endWidth = 0.02f;
            trail.time = 0.4f;
            trail.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            trail.startColor = trailColor;
            trail.endColor = new Color(trailColor.r * 0.5f, trailColor.g * 0.3f, 0f, 0f);
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var proj = projGO.AddComponent<StructureProjectile>();
            proj.Initialize(this, target, damage, projectileSpeed, isTrueDmg);
        }

        // ─── DEATH ─────────────────────────────────────────────────────

        protected override void Die(Entity killer)
        {
            base.Die(killer);
            if (rangeRing != null) rangeRing.SetActive(false);
            StartCoroutine(DestroyVisualCoroutine());
        }

        private System.Collections.IEnumerator DestroyVisualCoroutine()
        {
            float t = 0f;
            Vector3 startScale = transform.localScale;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
