using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Neutrophil Berserker — a neutral creature that spawns when an organ objective
    /// drops below 50% HP. Attacks the nearest entity regardless of team.
    ///
    /// Neutrophils are the most abundant white blood cells in the human body.
    /// They're aggressive first responders known for causing collateral damage
    /// to host tissue — hence why they attack both virus and immune teams.
    ///
    /// Killing one grants shared gold and XP to the killing team.
    /// </summary>
    public class NeutrophilBerserker : Entity
    {
        [Header("Berserker Settings")]
        public float aggroRange = 10f;
        public Vector3 roamCenter;
        public float roamRadius = 8f;

        [Header("Enrage")]
        public float enrageHealthPercent = 0.3f;
        public float enrageDamageMultiplier = 1.5f;
        public float enrageSpeedMultiplier = 1.3f;
        private bool isEnraged;
        private float baseAttackDamage;
        private float baseMoveSpeed;

        private Vector3 roamTarget;
        private float retargetTimer;

        protected override void Awake()
        {
            base.Awake();
            entityType = EntityType.Objective; // Grants gold/XP on kill
            team = Team.Virus; // Set to virus so immune can target, but we override targeting
        }

        protected override void Start()
        {
            base.Start();
            baseAttackDamage = attackDamage;
            baseMoveSpeed = moveSpeed;
            PickRoamTarget();
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead) return;

            // Enrage at low HP
            if (!isEnraged && currentHealth <= maxHealth * enrageHealthPercent)
                Enrage();

            retargetTimer -= Time.deltaTime;
            if (retargetTimer <= 0f)
            {
                FindNearestTarget();
                retargetTimer = 0.5f; // Re-check every 0.5s
            }

            if (currentTarget != null && !currentTarget.IsDead)
                ChaseAndAttack();
            else
                Roam();
        }

        /// <summary>
        /// Find the nearest entity of ANY team within aggro range.
        /// Neutrophils attack everything indiscriminately.
        /// </summary>
        private void FindNearestTarget()
        {
            if (GameManager.Instance == null) return;

            Entity nearest = null;
            float nearestDist = aggroRange * aggroRange;

            // Search both teams
            var virusEntities = GameManager.Instance.GetEntitiesInRange(
                transform.position, aggroRange, Team.Virus);
            var immuneEntities = GameManager.Instance.GetEntitiesInRange(
                transform.position, aggroRange, Team.Immune);

            foreach (var e in virusEntities)
            {
                if (e == this || e.IsDead) continue;
                float dist = (e.transform.position - transform.position).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = e;
                }
            }

            foreach (var e in immuneEntities)
            {
                if (e == this || e.IsDead) continue;
                float dist = (e.transform.position - transform.position).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = e;
                }
            }

            currentTarget = nearest;
        }

        private void ChaseAndAttack()
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dist > attackRange)
            {
                Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
                dir.y = 0f;
                transform.position += dir * moveSpeed * Time.deltaTime;
                FaceDirection(dir);
            }
            else
            {
                // Attack — Neutrophils bypass team checks, damage everyone
                if (CanAttack())
                {
                    attackCooldown = 1f / attackSpeed;
                    currentTarget.TakeDamage(attackDamage, false, this);
                    FaceDirection((currentTarget.transform.position - transform.position).normalized);
                }
            }
        }

        private void Roam()
        {
            float dist = Vector3.Distance(transform.position, roamTarget);
            if (dist < 1.5f)
                PickRoamTarget();

            Vector3 dir = (roamTarget - transform.position).normalized;
            dir.y = 0f;
            transform.position += dir * moveSpeed * 0.5f * Time.deltaTime;
            FaceDirection(dir);
        }

        private void PickRoamTarget()
        {
            Vector2 offset = Random.insideUnitCircle * roamRadius;
            roamTarget = roamCenter + new Vector3(offset.x, 0f, offset.y);
        }

        private void Enrage()
        {
            isEnraged = true;
            attackDamage = baseAttackDamage * enrageDamageMultiplier;
            moveSpeed = baseMoveSpeed * enrageSpeedMultiplier;

            // Visual: turn bright orange-red
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = new Color(1f, 0.2f, 0f);
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
            // Grant gold/XP to the killing team via shared distribution
            if (GameManager.Instance != null && killer != null)
            {
                GameManager.Instance.DistributeGold(transform.position, killer.team, 200f);
                GameManager.Instance.DistributeXP(transform.position, killer.team, 150f);
            }

            InvokeDeath();
            Destroy(gameObject, 0.1f);
        }
    }
}
