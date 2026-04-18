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
        public float projectileSpeed = 7f;

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

        // Aim indicator — shader-driven slime beam from spawn point to current target
        private LineRenderer aimLine;
        private Material aimLineMaterial;
        private const float AimLineTargetHeight = 0.5f;
        private const float AimLineWidth = 0.22f;

        private float ProjectileSpawnHeight => transform.localScale.y * 0.5f;

        // Team-specific slime look (identical gameplay, different feel)
        private static readonly Color VirusSlimeCore  = new Color(0.42f, 0.88f, 0.18f, 1f);
        private static readonly Color VirusSlimeEdge  = new Color(0.10f, 0.28f, 0.02f, 1f);
        private static readonly Color ImmuneSlimeCore = new Color(0.75f, 0.92f, 1.00f, 1f);
        private static readonly Color ImmuneSlimeEdge = new Color(0.22f, 0.48f, 0.80f, 1f);
        private const float VirusSlimeAmount  = 0.92f;
        private const float ImmuneSlimeAmount = 0.28f;

        protected override void Awake()
        {
            base.Awake();
            entityType = EntityType.Structure;
        }

        protected override void Start()
        {
            base.Start();
            CreateRangeRing();
            CreateAimLine();
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

            ResetConsecutiveHitsIfChampionLeftRange();
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
            UpdateAimLine();
        }

        private void ResetConsecutiveHitsIfChampionLeftRange()
        {
            if (lastTarget == null) return;
            if (lastTarget.entityType != EntityType.Champion) return;

            bool outOfRange = lastTarget.IsDead ||
                Vector3.Distance(transform.position, lastTarget.transform.position) > structureRange;

            if (outOfRange)
            {
                consecutiveHits = 0;
                lastTarget = null;
            }
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

            rangeRingMaterial = new Material(ShaderLibrary.Instance.bioPulse);
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

        // ─── AIM INDICATOR LINE ────────────────────────────────────────

        private void CreateAimLine()
        {
            var go = new GameObject("StructureAimLine");
            go.transform.SetParent(transform, false);

            aimLine = go.AddComponent<LineRenderer>();
            aimLine.positionCount = 2;
            aimLine.useWorldSpace = true;
            aimLine.startWidth = AimLineWidth;
            aimLine.endWidth = AimLineWidth * 0.55f;
            aimLine.numCapVertices = 4;
            aimLine.numCornerVertices = 2;
            aimLine.textureMode = LineTextureMode.Stretch;
            aimLine.alignment = LineAlignment.View;
            aimLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            aimLine.receiveShadows = false;

            aimLineMaterial = CreateSlimeBeamMaterial(isProjectileTrail: false);
            aimLine.material = aimLineMaterial;
            aimLine.enabled = false;
        }

        private void UpdateAimLine()
        {
            if (aimLine == null) return;

            bool hasTarget = attackTarget != null && !attackTarget.IsDead;
            bool inRange = hasTarget && Vector3.Distance(
                transform.position, attackTarget.transform.position) <= structureRange;

            if (!inRange)
            {
                if (aimLine.enabled) aimLine.enabled = false;
                return;
            }

            Vector3 origin = transform.position + Vector3.up * ProjectileSpawnHeight;
            Vector3 tip = attackTarget.transform.position + Vector3.up * AimLineTargetHeight;
            aimLine.SetPosition(0, origin);
            aimLine.SetPosition(1, tip);
            if (!aimLine.enabled) aimLine.enabled = true;
        }

        private Material CreateSlimeBeamMaterial(bool isProjectileTrail)
        {
            var mat = new Material(ShaderLibrary.Instance.slimeBeam);
            bool isVirus = team == Team.Virus;
            Color core = isVirus ? VirusSlimeCore : ImmuneSlimeCore;
            Color edge = isVirus ? VirusSlimeEdge : ImmuneSlimeEdge;
            float slime = isVirus ? VirusSlimeAmount : ImmuneSlimeAmount;

            mat.SetColor("_Color", core);
            mat.SetColor("_EdgeColor", edge);
            mat.SetFloat("_Slime", slime);
            mat.SetFloat("_FlowSpeed", isProjectileTrail ? 2.6f : 1.4f);
            mat.SetFloat("_PulseSpeed", 2.2f);
            mat.SetFloat("_Thickness", isProjectileTrail ? 0.78f : 0.68f);
            mat.SetFloat("_Alpha", isProjectileTrail ? 0.95f : 0.82f);
            return mat;
        }

        // ─── PROJECTILE ────────────────────────────────────────────────

        private void SpawnPoisonProjectile(Entity target, float damage, bool isTrueDmg)
        {
            Vector3 spawnPos = transform.position + Vector3.up * ProjectileSpawnHeight;
            bool isVirus = team == Team.Virus;

            Color teamColor = isVirus ? VirusSlimeCore : ImmuneSlimeCore;
            Color headColor = isTrueDmg ? Color.white : Color.Lerp(teamColor, Color.white, 0.55f);

            var projGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projGO.name = "StructureProjectile";
            projGO.transform.position = spawnPos;
            projGO.transform.localScale = Vector3.one * (isTrueDmg ? 0.55f : 0.42f);

            DestroyImmediate(projGO.GetComponent<SphereCollider>());

            var renderer = projGO.GetComponent<Renderer>();
            renderer.material = new Material(ShaderLibrary.Instance.urpUnlit);
            renderer.material.color = headColor;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            GameObject muzzleFlash = AttachMuzzleFlash(projGO.transform, isTrueDmg);

            var trail = projGO.AddComponent<TrailRenderer>();
            trail.startWidth = isTrueDmg ? 0.55f : 0.4f;
            trail.endWidth = 0.02f;
            trail.time = 0.45f;
            trail.textureMode = LineTextureMode.Stretch;
            trail.alignment = LineAlignment.View;
            trail.startColor = Color.white;
            trail.endColor = new Color(1f, 1f, 1f, 0f);
            trail.material = CreateSlimeBeamMaterial(isProjectileTrail: true);
            if (isTrueDmg)
            {
                trail.material.SetColor("_Color", Color.white);
                trail.material.SetColor("_EdgeColor", new Color(0.7f, 0.7f, 0.7f, 1f));
            }
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var proj = projGO.AddComponent<StructureProjectile>();
            proj.Initialize(this, target, damage, projectileSpeed, isTrueDmg, muzzleFlash);
        }

        private static GameObject AttachMuzzleFlash(Transform projectile, bool isTrueDmg)
        {
            var prefab = VFXLibrary.Instance != null ? VFXLibrary.Instance.structureMuzzleFlash : null;
            if (prefab == null) return null;

            var flash = Object.Instantiate(prefab, projectile);
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localRotation = Quaternion.identity;

            if (isTrueDmg)
            {
                var ps = flash.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.1f);
                }
            }
            return flash;
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
