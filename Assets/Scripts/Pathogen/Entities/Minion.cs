using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Lane minion with LoL-style aggro rules:
    /// - Prioritizes enemy minions first, then structures
    /// - Attacks enemy champions only if they're the first thing encountered
    /// - If chasing a champion and can't attack for 2s, drops aggro and resumes lane
    /// - Re-aggros on a champion only if that champion hits an allied champion nearby
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

        private Entity aggroTarget;
        private bool initialized;
        private float championAggroTimer;
        private bool aggroLockedOnChampion;
        private Vector3 combatOffset; // Unique offset so minions don't stack

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
        }

        private void UpdateAggro()
        {
            if (GameManager.Instance == null) return;

            // Drop dead or destroyed targets
            if (aggroTarget != null && (aggroTarget.IsDead || aggroTarget == null))
            {
                aggroTarget = null;
                aggroLockedOnChampion = false;
            }

            // Champion leash: if locked on a champion and can't reach them for 2s, drop aggro
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
                    championAggroTimer = 0f; // Reset timer when in attack range
                }
                return; // Don't re-scan while locked on champion
            }

            // Scan for targets with priority: minions > structures > champions
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

            // Priority: minions first, then structures, then champions
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

        /// <summary>
        /// Called by GameManager when an enemy champion hits an allied champion nearby.
        /// Forces this minion to switch aggro to that enemy champion.
        /// </summary>
        public void OnAllyChampionAttacked(Entity enemyChampion)
        {
            if (IsDead || enemyChampion == null || enemyChampion.IsDead) return;
            if (enemyChampion.team == team) return;

            float dist = Vector3.Distance(transform.position, enemyChampion.transform.position);
            if (dist > sightRange) return;

            aggroTarget = enemyChampion;
            aggroLockedOnChampion = true;
            championAggroTimer = 0f;
        }

        private void HandleCombat()
        {
            // Move toward target position + unique offset to avoid stacking
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
                float dist = away.magnitude;
                if (dist < 0.01f)
                    away = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                else
                    away /= dist; // normalize
                push += away * (1f - dist / separationRadius);
            }

            if (push.sqrMagnitude > 0.01f)
                transform.position += push.normalized * separationForce * Time.deltaTime;
        }

        private void AssignCombatOffset()
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(1.5f, 2.5f);
            combatOffset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
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
