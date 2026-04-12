using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Simple AI for the enemy champion. Farms minions, fights enemy champion,
    /// and pushes down the lane.
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

        private enum AIState { Patrol, Farm, Fight, Retreat }
        private AIState state = AIState.Patrol;
        private Entity currentTarget;
        private float skillCooldownTimer;

        void Start()
        {
            champion = GetComponent<Champion>();
        }

        void Update()
        {
            if (champion == null || champion.IsDead || champion.isRespawning) return;
            if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;

            UpdateState();
            ExecuteState();
            skillCooldownTimer -= Time.deltaTime;
        }

        private void UpdateState()
        {
            float healthPercent = champion.currentHealth / champion.maxHealth;

            if (healthPercent < retreatHealthPercent && state != AIState.Retreat)
            {
                state = AIState.Retreat;
                return;
            }

            var enemyChamp = GameManager.Instance.GetNearestEnemy(
                transform.position, aggroRange, champion.team);

            if (enemyChamp != null && enemyChamp.entityType == EntityType.Champion)
            {
                currentTarget = enemyChamp;
                state = AIState.Fight;
                return;
            }

            var nearestMinion = GameManager.Instance.GetNearestEnemy(
                transform.position, aggroRange, champion.team);

            if (nearestMinion != null)
            {
                currentTarget = nearestMinion;
                state = AIState.Farm;
                return;
            }

            state = AIState.Patrol;
        }

        private void ExecuteState()
        {
            switch (state)
            {
                case AIState.Patrol: Patrol(); break;
                case AIState.Farm: FarmMinion(); break;
                case AIState.Fight: FightTarget(); break;
                case AIState.Retreat: Retreat(); break;
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
                state = AIState.Patrol;
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
            dir = ChampionSteerAroundObstacles(dir);
            transform.position += dir * champion.moveSpeed * Time.deltaTime;
            FaceTarget(target);
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
