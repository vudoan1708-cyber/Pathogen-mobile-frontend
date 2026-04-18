using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Lane minion with LoL-style aggro rules:
    /// - Prioritizes enemy minions first, then structures, then champions
    /// - Aggros on first enemy in sight range
    /// - If chasing a champion and can't attack for 2s, drops aggro and resumes lane
    /// - Re-aggros on a champion if that champion hits an allied champion nearby
    /// - Stays on-lane (Z clamped to prevent diagonal drift)
    /// </summary>
    public enum MinionType { Melee, Ranged, ArmoredMelee, Buffed }

    public class Minion : Entity
    {
        [Header("Minion Settings")]
        public MinionType minionType;
        public int formationIndex;
        public Minion waveAhead;
        public Vector3[] waypoints;
        public int currentWaypointIndex;
        public float waypointReachDistance = 1.5f;

        [Header("Aggro")]
        public float sightRange = 7f;
        public float championLeashTime = 1.5f;

        [Header("Spacing")]
        public float separationRadius = 1.2f;
        public float separationForce = 2f;
        public float laneZClamp = 3f; // Max Z drift from lane center (Z=0)

        private Entity aggroTarget;
        private bool initialized;
        private float championAggroTimer;
        private bool aggroLockedOnChampion;
        private Vector3 combatOffset;

        protected override void Awake()
        {
            entityType = EntityType.Minion;
        }

        protected override void Start()
        {
            if (currentHealth <= 0f)
                currentHealth = maxHealth;

            initialized = true;
            base.Start();
        }

        protected override void Update()
        {
            if (!initialized || IsDead) return;

            if (attackCooldown > 0f)
                attackCooldown -= Time.deltaTime;

            UpdateAggro();

            if (aggroTarget != null && !aggroTarget.IsDead)
                HandleCombat();
            else
                FollowWaypoints();

            ClampToLane();
        }

        private void UpdateAggro()
        {
            if (GameManager.Instance == null) return;

            // Drop dead or destroyed targets — immediately scan for next
            if (aggroTarget != null && (aggroTarget.IsDead || aggroTarget == null))
            {
                aggroTarget = null;
                aggroLockedOnChampion = false;
                // Immediately try to find the next target in sight
                aggroTarget = FindPriorityTarget();
                if (aggroTarget != null)
                    AssignCombatOffset();
                return;
            }

            // Champion leash: hard 1.5s cap — always ticking while chasing a champion
            if (aggroLockedOnChampion && aggroTarget != null)
            {
                championAggroTimer += Time.deltaTime;
                if (championAggroTimer >= championLeashTime)
                {
                    aggroTarget = null;
                    aggroLockedOnChampion = false;
                    championAggroTimer = 0f;
                }
                return;
            }

            // No target — scan for enemies
            if (aggroTarget == null)
            {
                aggroTarget = FindPriorityTarget();
                if (aggroTarget != null)
                    AssignCombatOffset();
            }
        }

        private Entity FindPriorityTarget()
        {
            Team enemyTeam = team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, sightRange, enemyTeam);

            Entity bestMinion = null;
            Entity bestStructure = null;
            Entity bestChampion = null;
            float closestMinion = float.MaxValue;
            float closestStructure = float.MaxValue;
            float closestChampion = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                float dist = (enemy.transform.position - transform.position).sqrMagnitude;

                switch (enemy.entityType)
                {
                    case EntityType.Minion:
                        if (dist < closestMinion) { closestMinion = dist; bestMinion = enemy; }
                        break;
                    case EntityType.Structure:
                        if (dist < closestStructure) { closestStructure = dist; bestStructure = enemy; }
                        break;
                    case EntityType.Champion:
                        if (dist < closestChampion) { closestChampion = dist; bestChampion = enemy; }
                        break;
                }
            }

            if (bestMinion != null) return bestMinion;
            if (bestStructure != null) return bestStructure;

            if (bestChampion != null)
            {
                aggroLockedOnChampion = true;
                championAggroTimer = 0f;
                return bestChampion;
            }

            return null;
        }

        public void OnAllyChampionAttacked(Entity enemyChampion)
        {
            if (IsDead || enemyChampion == null || enemyChampion.IsDead) return;
            if (enemyChampion.team == team) return;

            // Only aggro if enemy champion is within sight range + small buffer
            float dist = Vector3.Distance(transform.position, enemyChampion.transform.position);
            if (dist > sightRange + 1.5f) return;

            aggroTarget = enemyChampion;
            aggroLockedOnChampion = true;
            championAggroTimer = 0f;
            AssignCombatOffset();
        }

        private void HandleCombat()
        {
            Vector3 targetPos = aggroTarget.transform.position + combatOffset;
            float dist = Vector3.Distance(transform.position, aggroTarget.transform.position);

            if (dist > attackRange)
            {
                MoveToward(targetPos);
                ApplySeparation();
            }
            else
            {
                ApplySeparation();

                if (minionType == MinionType.Ranged)
                    FireRangedAttack(aggroTarget);
                else
                    PerformAutoAttack(aggroTarget);

                FaceDirection(aggroTarget.transform.position - transform.position);
            }
        }

        // PROTOTYPE: replace with 3D animated attacks per minion type
        private void FireRangedAttack(Entity target)
        {
            if (target == null || target.IsDead || !CanAttack()) return;
            attackCooldown = 1f / attackSpeed;

            Vector3 direction = (target.transform.position - transform.position).normalized;
            direction.y = 0f;

            Color color = team == Team.Virus
                ? new Color(1f, 0.4f, 0.3f) : new Color(0.4f, 0.7f, 1f);

            var vis = new SkillVisuals
            {
                primaryColor = color, scale = 0.15f,
                hasTrail = true,
                trailColor = new Color(color.r, color.g, color.b, 0.4f),
                trailWidth = 0.04f,
                particleCount = 2, particleSize = 0.06f,
                particleForce = 1.5f, particleLifetime = 0.3f
            };

            var projGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projGO.name = "MinionProjectile";
            projGO.transform.position = transform.position + direction * 0.5f + Vector3.up * 0.3f;
            projGO.transform.localScale = Vector3.one * vis.scale;
            projGO.GetComponent<Renderer>().material.color = vis.primaryColor;

            var proj = projGO.AddComponent<Projectile>();
            proj.Initialize(this, direction, attackDamage, 6f, attackRange,
                false, ProjectilePiercing.StopOnFirst, vis);

            DestroyImmediate(projGO.GetComponent<SphereCollider>());
            var col = projGO.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var rb = projGO.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

        private void ApplySeparation()
        {
            if (GameManager.Instance == null) return;

            var nearby = GameManager.Instance.GetEntitiesInRange(transform.position, separationRadius, team);
            Vector3 push = Vector3.zero;

            foreach (var e in nearby)
            {
                if (e == this || e.entityType != EntityType.Minion) continue;
                Vector3 away = transform.position - e.transform.position;
                away.y = 0f;
                float d = away.magnitude;
                if (d < 0.01f)
                    away = new Vector3(Random.Range(-1f, 1f), 0f, 0f); // Push along X only
                else
                    away /= d;
                push += away * (1f - d / separationRadius);
            }

            if (push.sqrMagnitude > 0.01f)
            {
                push.z *= 0.3f;
                MoveBy(push.normalized * separationForce * Time.deltaTime);
            }
        }

        private void AssignCombatOffset()
        {
            bool isFrontRow = minionType == MinionType.Melee || minionType == MinionType.ArmoredMelee;
            // Ranged sit 3 units behind melee — far enough to stay separate,
            // close enough that structures (range 4) can still target them.
            float xOffset = isFrontRow ? Random.Range(-0.8f, 0.8f) : -2f + Random.Range(-0.5f, 0.5f);

            // Melee cluster tight (spread 1.5) so all stay within attack range.
            // Ranged spread wider (2.5) since they have 7-unit range.
            float spread = isFrontRow ? 1.5f : 2.5f;
            int count = 3;
            int idx = formationIndex < count ? formationIndex : formationIndex - count;
            float zBase = -(count - 1) * spread * 0.5f + idx * spread;

            combatOffset = new Vector3(xOffset, 0f, zBase + Random.Range(-0.3f, 0.3f));
        }

        private void FollowWaypoints()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Vector3 target = waypoints[currentWaypointIndex];
            MoveToward(target);

            if (Vector3.Distance(transform.position, target) < waypointReachDistance)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                    currentWaypointIndex = waypoints.Length - 1;
            }
        }

        /// <summary>
        /// Prevent minions from drifting too far off the lane center (Z=0).
        /// </summary>
        private void ClampToLane()
        {
            var pos = transform.position;
            pos.z = Mathf.Clamp(pos.z, -laneZClamp, laneZClamp);
            transform.position = pos;
        }

        private void MoveToward(Vector3 target)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;

            dir.Normalize();
            dir = SteerAroundObstacles(dir);

            float speed = moveSpeed;
            if (waveAhead != null && !waveAhead.IsDead)
            {
                float gap = Vector3.Distance(transform.position, waveAhead.transform.position);
                if (gap > 4f)
                    speed = moveSpeed * Mathf.Lerp(1f, 1.6f, (gap - 4f) / 4f);
            }

            transform.position += dir * speed * Time.deltaTime;
            FaceDirection(dir);
        }

        private Vector3 SteerAroundObstacles(Vector3 desiredDir)
        {
            if (GameManager.Instance == null) return desiredDir;

            Vector3 steer = Vector3.zero;
            var nearby = GameManager.Instance.GetEntitiesInRange(transform.position, 3f);

            foreach (var e in nearby)
            {
                if (e == this || e.entityType == EntityType.Objective) continue;
                if (e.entityType == EntityType.Minion && e.team != team) continue;

                // Only steer around allied minions that are in combat (stationary)
                if (e.entityType == EntityType.Minion)
                {
                    var other = e as Minion;
                    if (other != null && other.aggroTarget == null) continue;
                }

                Vector3 toOther = e.transform.position - transform.position;
                toOther.y = 0f;
                float dist = toOther.magnitude;
                if (dist < 0.01f) continue;

                float ahead = Vector3.Dot(desiredDir, toOther.normalized);
                if (ahead < 0.3f) continue;

                float strength = 0f;
                switch (e.entityType)
                {
                    case EntityType.Minion: strength = (1f - dist / 2f) * 0.8f; break;
                    case EntityType.Champion: strength = (1f - dist / 2.5f) * 1.5f; break;
                    case EntityType.Structure: strength = (1f - dist / 3f) * 2f; break;
                }
                if (strength <= 0f) continue;

                // Always nudge in Z (sideways), never backward along the lane
                float side = toOther.z > transform.position.z ? -1f : 1f;
                steer.z += side * strength;
            }

            // Pull back toward lane center (Z=0) when no obstacle is pushing
            float zDrift = Mathf.Abs(transform.position.z);
            if (zDrift > 0.5f && steer.sqrMagnitude < 0.01f)
                steer.z += -Mathf.Sign(transform.position.z) * zDrift * 0.5f;

            if (steer.sqrMagnitude < 0.001f) return desiredDir;
            return (desiredDir + steer).normalized;
        }

        private void FaceDirection(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }

        protected override void Die(Entity killer)
        {
            base.Die(killer);
            Destroy(gameObject, 0.1f);
        }
    }
}
