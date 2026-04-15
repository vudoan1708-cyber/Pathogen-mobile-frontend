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
        public float structureRange = 6f;

        [Header("Escalating Damage")]
        public int trueDamageThreshold = 3;
        public float trueDamageMultiplier = 1.5f;

        [Header("Projectile")]
        public float projectileSpeed = 12f;

        private Entity attackTarget;
        private Entity lastTarget;
        private int consecutiveHits;
        private float structureAttackTimer;
        private bool focusingMinions;

        // Enemy champion that attacked our ally champion — prioritised while in range
        private Entity aggroChampion;

        // Range ring visualization
        private GameObject rangeRing;
        private Material rangeRingMaterial;
        private const float RangeRingRevealBuffer = 3f;
        private static readonly Color rangeRingSafe = new Color(0.3f, 0.5f, 0.8f, 1f);
        private static readonly Color rangeRingDanger = new Color(1f, 0.25f, 0.15f, 1f);
        private bool lastDangerState;

        private static Mesh structureRangeMesh;

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
                Vector3.Distance(transform.position, aggroChampion.transform.position) > structureRange))
            {
                aggroChampion = null;
            }

            FindTarget();

            if (attackTarget != null && !attackTarget.IsDead && structureAttackTimer <= 0f)
            {
                float dist = Vector3.Distance(transform.position, attackTarget.transform.position);
                if (dist <= structureRange)
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
            if (aggroChampion != null) return;

            float dist = Vector3.Distance(transform.position, enemyChampion.transform.position);
            if (dist <= structureRange)
                aggroChampion = enemyChampion;
        }

        private void FindTarget()
        {
            if (GameManager.Instance == null) return;

            // Keep current target while alive and in range
            if (attackTarget != null && !attackTarget.IsDead)
            {
                float dist = Vector3.Distance(transform.position, attackTarget.transform.position);
                if (dist <= structureRange)
                {
                    // Aggro override: enemy champion attacked ally while we're on minions
                    if (focusingMinions && aggroChampion != null && !aggroChampion.IsDead)
                    {
                        float agroDist = Vector3.Distance(transform.position, aggroChampion.transform.position);
                        if (agroDist <= structureRange)
                        {
                            attackTarget = aggroChampion;
                            focusingMinions = false;
                            return;
                        }
                        aggroChampion = null;
                    }
                    return;
                }
            }

            Team enemyTeam = team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, structureRange, enemyTeam);

            if (enemies.Count == 0)
            {
                attackTarget = null;
                focusingMinions = false;
                return;
            }

            // Aggro champion takes priority
            if (aggroChampion != null && !aggroChampion.IsDead)
            {
                float agroDist = Vector3.Distance(transform.position, aggroChampion.transform.position);
                if (agroDist <= structureRange)
                {
                    attackTarget = aggroChampion;
                    focusingMinions = false;
                    return;
                }
                aggroChampion = null;
            }

            Entity nearestMinion = null;
            Entity nearestChampion = null;
            float closestMinionDist = float.MaxValue;
            float closestChampDist = float.MaxValue;

            foreach (var entity in enemies)
            {
                if (entity.IsDead) continue;
                float dist = Vector3.Distance(transform.position, entity.transform.position);
                if (entity.entityType == EntityType.Minion && dist < closestMinionDist)
                    { closestMinionDist = dist; nearestMinion = entity; }
                if (entity.entityType == EntityType.Champion && dist < closestChampDist)
                    { closestChampDist = dist; nearestChampion = entity; }
            }

            // If already focusing minions, stay on minions until none left
            if (focusingMinions)
            {
                if (nearestMinion != null)
                    { attackTarget = nearestMinion; return; }
                focusingMinions = false;
            }

            // First entity in range determines focus
            if (nearestMinion != null && nearestChampion != null)
            {
                if (closestMinionDist <= closestChampDist)
                    { attackTarget = nearestMinion; focusingMinions = true; }
                else
                    { attackTarget = nearestChampion; focusingMinions = false; }
            }
            else if (nearestMinion != null)
            {
                attackTarget = nearestMinion;
                focusingMinions = true;
            }
            else
            {
                attackTarget = nearestChampion;
                focusingMinions = false;
            }
        }

        // ─── RANGE RING ────────────────────────────────────────────────

        private void CreateRangeRing()
        {
            if (structureRangeMesh == null)
                structureRangeMesh = GenerateDiscMesh(48, 6);

            rangeRing = new GameObject("StructureRangeRing");
            rangeRing.transform.position = new Vector3(
                transform.position.x, 0.05f, transform.position.z);

            float diameter = structureRange * 2f;
            rangeRing.transform.localScale = new Vector3(diameter, 1f, diameter);

            var filter = rangeRing.AddComponent<MeshFilter>();
            filter.sharedMesh = structureRangeMesh;

            var shader = Shader.Find("Pathogen/BioPulse");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");

            rangeRingMaterial = new Material(shader);
            rangeRingMaterial.SetColor("_Color", rangeRingSafe);
            rangeRingMaterial.SetFloat("_FillAlpha", 0.03f);
            rangeRingMaterial.SetFloat("_EdgeAlpha", 0.4f);
            rangeRingMaterial.SetFloat("_PulseSpeed", 0.5f);
            rangeRingMaterial.SetFloat("_PulseIntensity", 0.1f);
            rangeRingMaterial.renderQueue = 3010;

            var renderer = rangeRing.AddComponent<MeshRenderer>();
            renderer.material = rangeRingMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            rangeRing.SetActive(false);
        }

        private void UpdateRangeRing()
        {
            if (rangeRing == null || GameManager.Instance == null) return;

            // Only show range ring on ENEMY structures when the player champion is near
            float revealRange = structureRange + RangeRingRevealBuffer;
            bool showRing = false;

            var nearbyEntities = GameManager.Instance.GetEntitiesInRange(transform.position, revealRange);
            foreach (var entity in nearbyEntities)
            {
                if (entity.IsDead || entity.entityType != EntityType.Champion) continue;
                // Show ring only to enemy champions (player near hostile structure)
                if (entity.team != team && entity.GetComponent<PlayerController>() != null)
                {
                    showRing = true;
                    break;
                }
            }

            rangeRing.SetActive(showRing);

            if (showRing && rangeRingMaterial != null)
            {
                bool danger = attackTarget != null
                    && attackTarget.entityType == EntityType.Champion
                    && !attackTarget.IsDead;

                if (danger != lastDangerState)
                {
                    lastDangerState = danger;
                    rangeRingMaterial.SetColor("_Color", danger ? rangeRingDanger : rangeRingSafe);
                    rangeRingMaterial.SetFloat("_FillAlpha", danger ? 0.06f : 0.03f);
                    rangeRingMaterial.SetFloat("_EdgeAlpha", danger ? 0.6f : 0.4f);
                    rangeRingMaterial.SetFloat("_PulseSpeed", danger ? 1.2f : 0.5f);
                    rangeRingMaterial.SetFloat("_PulseIntensity", danger ? 0.22f : 0.1f);
                }
            }
        }

        // ─── MESH GENERATION ──────────────────────────────────────────

        private static Mesh GenerateDiscMesh(int segments, int rings)
        {
            float maxRadius = 0.5f;
            int vertCount = 1 + (rings + 1) * segments;
            var vertices = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];

            vertices[0] = Vector3.zero;
            uvs[0] = new Vector2(1f, 0f);

            for (int r = 0; r <= rings; r++)
            {
                float radius = (float)(r + 1) / (rings + 1);
                for (int i = 0; i < segments; i++)
                {
                    int idx = 1 + r * segments + i;
                    float angle = (float)i / segments * Mathf.PI * 2f;
                    vertices[idx] = new Vector3(
                        Mathf.Cos(angle) * radius * maxRadius,
                        0f,
                        Mathf.Sin(angle) * radius * maxRadius);
                    uvs[idx] = new Vector2(1f - radius, 0f);
                }
            }

            int triCount = segments * 3 + rings * segments * 6;
            var triangles = new int[triCount];
            int ti = 0;

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles[ti++] = 0;
                triangles[ti++] = 1 + next;
                triangles[ti++] = 1 + i;
            }

            for (int r = 0; r < rings; r++)
            {
                int ring0 = 1 + r * segments;
                int ring1 = 1 + (r + 1) * segments;
                for (int i = 0; i < segments; i++)
                {
                    int next = (i + 1) % segments;
                    triangles[ti++] = ring0 + i;
                    triangles[ti++] = ring0 + next;
                    triangles[ti++] = ring1 + i;
                    triangles[ti++] = ring0 + next;
                    triangles[ti++] = ring1 + next;
                    triangles[ti++] = ring1 + i;
                }
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
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
