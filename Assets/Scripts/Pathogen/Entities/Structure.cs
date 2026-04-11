using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Defensive lane structure:
    /// - Virus team: Infection Nodes
    /// - Immune team: Sentinels
    /// After 3 consecutive shots on the same target, deals true damage
    /// (bypasses armor and magic resistance). Visual changes to white beam.
    /// </summary>
    public class Structure : Entity
    {
        [Header("Structure Settings")]
        public float detectionRange = 12f;
        public float attackCooldownTime = 1.5f;

        [Header("Target Priority")]
        public bool prioritizeMinions = true;

        [Header("Escalating Damage")]
        public int trueDamageThreshold = 3;
        public float trueDamageMultiplier = 1.5f;

        private Entity attackTarget;
        private Entity lastTarget;
        private int consecutiveHits;
        private float structureAttackTimer;

        protected override void Awake()
        {
            base.Awake();
            entityType = EntityType.Structure;
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead) return;

            structureAttackTimer -= Time.deltaTime;

            FindTarget();

            if (attackTarget != null && !attackTarget.IsDead && structureAttackTimer <= 0f)
            {
                float dist = Vector3.Distance(transform.position, attackTarget.transform.position);
                if (dist <= attackRange)
                {
                    // Track consecutive hits on the same target
                    if (attackTarget == lastTarget)
                        consecutiveHits++;
                    else
                    {
                        consecutiveHits = 1;
                        lastTarget = attackTarget;
                    }

                    if (consecutiveHits >= trueDamageThreshold)
                        attackTarget.TakeRawDamage(attackDamage * trueDamageMultiplier);
                    else
                        attackTarget.TakeDamage(attackDamage, false, this);

                    structureAttackTimer = attackCooldownTime;

                    // Line color: white for true damage, team color otherwise
                    Color lineColor;
                    if (consecutiveHits >= trueDamageThreshold)
                        lineColor = Color.white;
                    else
                        lineColor = team == Team.Virus ? Color.red : Color.cyan;

                    bool isTrueDmg = consecutiveHits >= trueDamageThreshold;
                    StartCoroutine(AttackVisualCoroutine(attackTarget.transform.position, lineColor, isTrueDmg));
                }
            }
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

            Entity bestMinion = null;
            Entity bestChampion = null;
            float closestMinionDist = float.MaxValue;
            float closestChampDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                float dist = Vector3.Distance(transform.position, enemy.transform.position);

                if (enemy.entityType == EntityType.Minion && dist < closestMinionDist)
                {
                    closestMinionDist = dist;
                    bestMinion = enemy;
                }
                else if (enemy.entityType == EntityType.Champion && dist < closestChampDist)
                {
                    closestChampDist = dist;
                    bestChampion = enemy;
                }
            }

            attackTarget = (prioritizeMinions && bestMinion != null) ? bestMinion : bestChampion;
            if (attackTarget == null)
                attackTarget = bestMinion;
        }

        private System.Collections.IEnumerator AttackVisualCoroutine(Vector3 targetPos, Color color, bool isTrueDmg)
        {
            var lineObj = new GameObject("StructureAttackLine");
            var line = lineObj.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, transform.position + Vector3.up * 2f);
            line.SetPosition(1, targetPos + Vector3.up * 0.5f);
            line.startWidth = isTrueDmg ? 0.3f : 0.15f;
            line.endWidth = isTrueDmg ? 0.15f : 0.05f;
            line.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            line.material.color = color;

            yield return new WaitForSeconds(0.15f);
            Destroy(lineObj);
        }

        protected override void Die(Entity killer)
        {
            base.Die(killer);
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
