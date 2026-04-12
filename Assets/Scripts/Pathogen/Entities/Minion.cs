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
    public class Minion : Entity
    {
        [Header("Minion Settings")]
        public Vector3[] waypoints;
        public int currentWaypointIndex;
        public float waypointReachDistance = 1.5f;

        [Header("Aggro")]
        public float sightRange = 7f;
        public float championLeashTime = 2f;

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

            // Champion leash: can't reach for 2s → drop aggro
            if (aggroLockedOnChampion && aggroTarget != null)
            {
                float dist = Vector3.Distance(transform.position, aggroTarget.transform.position);
                if (dist > attackRange)
                {
                    championAggroTimer += Time.deltaTime;
                    if (championAggroTimer >= championLeashTime)
                    {
                        aggroTarget = null;
                        aggroLockedOnChampion = false;
                    }
                }
                else
                {
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

            float dist = Vector3.Distance(transform.position, enemyChampion.transform.position);
            if (dist > sightRange) return;

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
                PerformAutoAttack(aggroTarget);
                FaceDirection(aggroTarget.transform.position - transform.position);
            }
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
                // Mostly push along X (lane axis), minimal Z to stay on-lane
                push.z *= 0.3f;
                transform.position += push.normalized * separationForce * Time.deltaTime;
            }
        }

        private void AssignCombatOffset()
        {
            // Offset along X (lane axis), minimal Z to prevent diagonal drift
            float xOffset = Random.Range(-2f, 2f);
            float zOffset = Random.Range(-0.5f, 0.5f);
            combatOffset = new Vector3(xOffset, 0f, zOffset);
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
            transform.position += dir * moveSpeed * Time.deltaTime;
            FaceDirection(dir);
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
