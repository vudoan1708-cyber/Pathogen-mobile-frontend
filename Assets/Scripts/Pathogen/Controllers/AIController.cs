using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// AI for the enemy champion. Strategic aggression based on health comparison,
    /// disengages after taking consecutive hits, farms minions, and pushes down the lane.
    /// </summary>
    public class AIController : MonoBehaviour
    {
        public Champion champion;

        [Header("AI Settings")]
        public float aggroRange = 10f;
        public float retreatHealthPercent = 0.25f;
        public float skillUseRange = 8f;

        [Header("Lane Patrol")]
        public Vector3[] patrolPoints;
        public int currentPatrolIndex;

        [Header("Hit Tracking")]
        public int maxConsecutiveHits = 4;

        private enum AIState { Patrol, Farm, Fight, Retreat, Kite }
        private AIState state = AIState.Patrol;
        private Entity currentTarget;
        private float skillCooldownTimer;

        // Hit tracking — disengage after taking too many hits in a row
        private int consecutiveHitsTaken;
        private float hitResetTimer;
        private const float HitResetDelay = 2f;

        // Kite state — back off briefly after max hits
        private float kiteTimer;
        private const float KiteDuration = 1.5f;

        void Start()
        {
            champion = GetComponent<Champion>();
        }

        void Update()
        {
            if (champion == null || champion.IsDead || champion.isRespawning) return;
            if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;

            // Tick down timers
            skillCooldownTimer -= Time.deltaTime;

            if (hitResetTimer > 0f)
            {
                hitResetTimer -= Time.deltaTime;
                if (hitResetTimer <= 0f)
                    consecutiveHitsTaken = 0;
            }

            if (kiteTimer > 0f)
                kiteTimer -= Time.deltaTime;

            UpdateState();
            ExecuteState();
        }

        public void OnHitReceived()
        {
            consecutiveHitsTaken++;
            hitResetTimer = HitResetDelay;
        }

        private void UpdateState()
        {
            float healthPercent = champion.currentHealth / champion.maxHealth;

            // Forced retreat at critical health
            if (healthPercent < retreatHealthPercent)
            {
                state = AIState.Retreat;
                return;
            }

            // Disengage after taking too many consecutive hits
            if (consecutiveHitsTaken >= maxConsecutiveHits && state == AIState.Fight)
            {
                state = AIState.Kite;
                kiteTimer = KiteDuration;
                consecutiveHitsTaken = 0;
                return;
            }

            // Currently kiting — stay in kite until timer expires
            if (state == AIState.Kite && kiteTimer > 0f)
                return;

            var enemyChamp = FindEnemyChampion();

            if (enemyChamp != null)
            {
                float myHP = champion.currentHealth / champion.maxHealth;
                float enemyHP = enemyChamp.currentHealth / enemyChamp.maxHealth;

                if (myHP < enemyHP - 0.15f)
                {
                    // Significantly lower health — play cautious, farm instead if possible
                    var minion = FindEnemyMinion();
                    if (minion != null)
                    {
                        currentTarget = minion;
                        state = AIState.Farm;
                        return;
                    }
                    // No minions, just keep distance
                    state = AIState.Retreat;
                    return;
                }

                // Health is comparable or we're healthier — fight
                currentTarget = enemyChamp;
                state = AIState.Fight;
                return;
            }

            var nearestMinion = FindEnemyMinion();
            if (nearestMinion != null)
            {
                currentTarget = nearestMinion;
                state = AIState.Farm;
                return;
            }

            state = AIState.Patrol;
        }

        private Entity FindEnemyChampion()
        {
            if (GameManager.Instance == null) return null;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, aggroRange, champion.team == Team.Virus ? Team.Immune : Team.Virus);

            foreach (var e in enemies)
            {
                if (!e.IsDead && e.entityType == EntityType.Champion)
                    return e;
            }
            return null;
        }

        private Entity FindEnemyMinion()
        {
            if (GameManager.Instance == null) return null;
            Team enemyTeam = champion.team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, aggroRange, enemyTeam);

            Entity best = null;
            float bestDist = float.MaxValue;
            foreach (var e in enemies)
            {
                if (e.IsDead || e.entityType != EntityType.Minion) continue;
                float dist = (e.transform.position - transform.position).sqrMagnitude;
                if (dist < bestDist) { bestDist = dist; best = e; }
            }
            return best;
        }

        private void ExecuteState()
        {
            switch (state)
            {
                case AIState.Patrol: Patrol(); break;
                case AIState.Farm: FarmMinion(); break;
                case AIState.Fight: FightTarget(); break;
                case AIState.Retreat: Retreat(); break;
                case AIState.Kite: Kite(); break;
            }
        }

        private void Patrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            Vector3 target = patrolPoints[currentPatrolIndex];
            MoveToward(target);

            if (Vector3.Distance(transform.position, target) < 2f)
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        private void FarmMinion()
        {
            if (currentTarget == null || currentTarget.IsDead)
            {
                state = AIState.Patrol;
                return;
            }

            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dist > champion.attackRange)
                MoveToward(currentTarget.transform.position);
            else
            {
                champion.PerformAutoAttack(currentTarget);
                FaceTarget(currentTarget.transform.position);
            }
        }

        private void FightTarget()
        {
            if (currentTarget == null || currentTarget.IsDead)
            {
                state = AIState.Patrol;
                return;
            }

            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dist < skillUseRange && skillCooldownTimer <= 0f)
            {
                TryUseSkill();
                skillCooldownTimer = 1.5f;
            }

            // Chase enemy champion — especially if their health is lower
            if (dist > champion.attackRange)
                MoveToward(currentTarget.transform.position);
            else
            {
                champion.PerformAutoAttack(currentTarget);
                FaceTarget(currentTarget.transform.position);
            }
        }

        private void Retreat()
        {
            MoveToward(champion.spawnPoint);

            float healthPercent = champion.currentHealth / champion.maxHealth;
            if (healthPercent > 0.5f)
            {
                consecutiveHitsTaken = 0;
                state = AIState.Patrol;
            }
        }

        private void Kite()
        {
            // Back away from the current threat while still attacking if in range
            if (currentTarget != null && !currentTarget.IsDead)
            {
                Vector3 awayDir = (transform.position - currentTarget.transform.position).normalized;
                awayDir.y = 0f;
                MoveInDirection(awayDir);

                float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (dist <= champion.attackRange && champion.CanAttack())
                {
                    champion.PerformAutoAttack(currentTarget);
                    FaceTarget(currentTarget.transform.position);
                }
            }
            else
            {
                // Threat gone, resume normal behavior
                kiteTimer = 0f;
            }
        }

        private void TryUseSkill()
        {
            if (currentTarget == null) return;

            Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
            dir.y = 0f;

            for (int i = 0; i < champion.skills.Length; i++)
            {
                if (champion.skills[i] != null && champion.skills[i].IsReady)
                {
                    champion.UseSkill(i, dir, currentTarget.transform.position);
                    break;
                }
            }
        }

        private void MoveToward(Vector3 target)
        {
            if (champion.isDashing) return;

            Vector3 dir = (target - transform.position).normalized;
            dir.y = 0f;
            MoveInDirection(dir);
            FaceTarget(target);
        }

        private void MoveInDirection(Vector3 dir)
        {
            if (champion.isDashing) return;
            dir = ChampionSteerAroundObstacles(dir);
            transform.position += dir * champion.moveSpeed * Time.deltaTime;
        }

        private Vector3 ChampionSteerAroundObstacles(Vector3 desiredDir)
        {
            if (GameManager.Instance == null) return desiredDir;

            Vector3 steer = Vector3.zero;
            var nearby = GameManager.Instance.GetEntitiesInRange(transform.position, 3f);

            foreach (var e in nearby)
            {
                if (e == champion) continue;

                bool isStructure = e.entityType == EntityType.Structure;
                bool isChampion = e.entityType == EntityType.Champion;
                if (!isStructure && !isChampion) continue;

                Vector3 toOther = e.transform.position - transform.position;
                toOther.y = 0f;
                float dist = toOther.magnitude;
                if (dist < 0.01f) continue;

                float ahead = Vector3.Dot(desiredDir, toOther.normalized);
                if (ahead < 0.3f) continue;

                float range = isStructure ? 3f : 2.5f;
                float strength = (1f - dist / range) * (isStructure ? 2f : 1.5f);
                if (strength <= 0f) continue;

                float cross = desiredDir.x * toOther.z - desiredDir.z * toOther.x;
                Vector3 perpendicular = cross >= 0f
                    ? new Vector3(-desiredDir.z, 0f, desiredDir.x)
                    : new Vector3(desiredDir.z, 0f, -desiredDir.x);

                steer += perpendicular * strength;
            }

            if (steer.sqrMagnitude < 0.001f) return desiredDir;
            return (desiredDir + steer).normalized;
        }

        private void FaceTarget(Vector3 target)
        {
            Vector3 dir = (target - transform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }
    }
}
