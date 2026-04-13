using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Pathogen
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Create(GameObject go)
        {
            if (GameBootstrap.IsMobile)
                return go.AddComponent<MobilePlayerController>();
            return go.AddComponent<DesktopPlayerController>();
        }

        public Champion champion;
        public VirtualJoystick moveJoystick;

        [Header("Attack")]
        public float attackRange = 10f;

        // Movement
        protected Vector3 moveTarget;
        protected bool isMovingToTarget;
        protected Entity attackTarget;
        protected bool isChasing;

        // Skill aiming
        protected int aimingSkillIndex = -1;
        protected SkillAimIndicator aimIndicator;
        protected bool skillFiredThisFrame;

        // Attack target circle
        private GameObject attackCircle;

        // Range indicator
        private GameObject rangeIndicator;
        private Renderer rangeIndicatorRenderer;
        private static readonly Color rangeInRange = new Color(0.3f, 0.5f, 0.8f, 0.25f);
        private static readonly Color rangeOutOfRange = new Color(1f, 0.3f, 0.3f, 0.3f);
        private float rangeFlashTimer;
        private bool rangeHeldOpen;

        // Attack button targeting
        protected Entity attackButtonTarget;
        protected AttackTargetType? activeAttackTargetType;
        protected bool isLockedOn;

        public Entity GetAttackButtonTarget() => attackButtonTarget;

        private Entity currentHighlightedEntity;
        private TargetHighlight currentHighlightedRing;

        protected CharacterController characterController;

        protected virtual void Start()
        {
            champion = GetComponent<Champion>();
            characterController = GetComponent<CharacterController>();
            aimIndicator = gameObject.AddComponent<SkillAimIndicator>();
            CreateAttackCircle();
            CreateRangeIndicator();

            if (champion != null)
                champion.OnRespawn += ResetControllerState;
        }

        protected virtual void ResetControllerState()
        {
            isMovingToTarget = false;
            isChasing = false;
            attackTarget = null;
            attackButtonTarget = null;
            activeAttackTargetType = null;
            if (currentHighlightedRing != null)
                currentHighlightedRing.SetHighlighted(false);
            currentHighlightedEntity = null;
            currentHighlightedRing = null;
            AutoAttackButton.ClearActive();
            CancelAiming();
        }

        protected virtual void Update()
        {
            if (champion == null || champion.IsDead || champion.isRespawning)
            {
                if (aimIndicator != null) aimIndicator.Hide();
                return;
            }
            if (GameManager.Instance != null && !GameManager.Instance.GameActive) return;

            skillFiredThisFrame = false;
            HandleInput();
            ExecuteMovement();
            ExecuteAutoAttack();
            UpdateAttackCircle();
            UpdateTargetHighlight();
            UpdateRangeFlash();
        }

        protected virtual void HandleInput() { }

        // ─── SKILL AIMING (shared) ──────────────────────────────────────

        public virtual void StartAiming(int skillIndex, bool fromButtonClick)
        {
            if (champion == null || champion.IsDead) return;
            if (skillIndex >= champion.skills.Length) return;
            var skill = champion.skills[skillIndex];
            if (skill == null || !skill.IsReady) return;
            if (champion.currentMana < skill.definition.manaCost) return;

            if (skill.definition.type == SkillType.SelfBuff)
            {
                champion.UseSkill(skillIndex, transform.forward, Vector3.zero);
                return;
            }

            ShowAttackRange(true, 0.3f);
            aimingSkillIndex = skillIndex;
        }

        public virtual void CancelAiming()
        {
            aimingSkillIndex = -1;
            if (aimIndicator != null)
            {
                aimIndicator.SetCancelTint(false);
                aimIndicator.Hide();
            }
        }

        public void SetAimCancelTint(bool cancelling)
        {
            if (aimIndicator != null) aimIndicator.SetCancelTint(cancelling);
        }

        protected Vector3 GetSmartAimTarget(SkillDefinition def)
        {
            if (GameManager.Instance == null)
                return transform.position + transform.forward * def.range;

            Team enemyTeam = champion.team == Team.Virus ? Team.Immune : Team.Virus;
            float searchRange = Mathf.Max(def.range, champion.sightRange);
            var shared = GameManager.Instance.GetEntitiesInRange(transform.position, searchRange, enemyTeam);

            int count = shared.Count;
            if (count == 0)
                return transform.position + transform.forward * def.range;

            var enemies = new Entity[count];
            shared.CopyTo(enemies);

            float rangeSquared = def.range * def.range;

            // 1. Champion with lowest health in skill range
            Entity inRangeChamp = null;
            float inRangeLowestHP = float.MaxValue;

            // 2. Any champion (even out of skill range) — aim towards them
            Entity anyChamp = null;
            float anyChampLowestHP = float.MaxValue;

            // 3. Nearest minion
            Entity nearestMinion = null;
            float closestMinionDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.IsDead) continue;

                float distSq = (enemy.transform.position - transform.position).sqrMagnitude;

                if (enemy.entityType == EntityType.Champion)
                {
                    if (distSq <= rangeSquared && enemy.currentHealth < inRangeLowestHP)
                    {
                        inRangeLowestHP = enemy.currentHealth;
                        inRangeChamp = enemy;
                    }
                    if (enemy.currentHealth < anyChampLowestHP)
                    {
                        anyChampLowestHP = enemy.currentHealth;
                        anyChamp = enemy;
                    }
                }
                else if (enemy.entityType == EntityType.Minion && distSq < closestMinionDist)
                {
                    closestMinionDist = distSq;
                    nearestMinion = enemy;
                }
            }

            if (inRangeChamp != null) return inRangeChamp.transform.position;
            if (anyChamp != null) return anyChamp.transform.position;
            if (nearestMinion != null) return nearestMinion.transform.position;

            // 4. Facing direction
            return transform.position + transform.forward * def.range;
        }

        // ─── MOBILE SKILL STUBS (overridden by MobilePlayerController) ──

        public virtual void OnMobileSkillAimUpdate(Vector3 aimDirection) { }
        public virtual void OnMobileSkillAimRelease(Vector3 aimDirection) { }
        public virtual bool IsTouchOverCancelButton(Vector2 screenPosition) => false;

        // ─── ATTACK BUTTONS (overridden by MobilePlayerController) ──────

        public virtual void OnAttackButtonAutoTarget(AttackTargetType targetType) { }
        public virtual void OnAttackButtonAim(AttackTargetType targetType, Vector3 aimDirection) { }
        public virtual void OnAttackButtonFire(AttackTargetType targetType, Vector3 aimDirection) { }

        // ─── VISUALS ────────────────────────────────────────────────────

        private void CreateAttackCircle()
        {
            attackCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            attackCircle.name = "AttackTargetCircle";
            attackCircle.transform.localScale = new Vector3(1.8f, 0.02f, 1.8f);
            DestroyImmediate(attackCircle.GetComponent<CapsuleCollider>());
            var r = attackCircle.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            r.material.color = new Color(0.85f, 0.75f, 0.25f, 0.7f);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            attackCircle.SetActive(false);
        }

        private void UpdateAttackCircle()
        {
            if (attackCircle == null) return;
            if (isChasing && attackTarget != null && !attackTarget.IsDead)
            {
                attackCircle.SetActive(true);
                attackCircle.transform.position = new Vector3(
                    attackTarget.transform.position.x, 0.03f, attackTarget.transform.position.z);
            }
            else
            {
                attackCircle.SetActive(false);
            }
        }

        private void UpdateTargetHighlight()
        {
            Entity target = (isChasing || (isLockedOn && attackTarget != null && !attackTarget.IsDead))
                ? attackTarget : null;

            if (target == currentHighlightedEntity) return;

            if (currentHighlightedRing != null)
                currentHighlightedRing.SetHighlighted(false);

            currentHighlightedEntity = target;

            if (target != null && !target.IsDead)
            {
                currentHighlightedRing = target.GetComponent<TargetHighlight>();
                if (currentHighlightedRing != null)
                    currentHighlightedRing.SetHighlighted(true);
            }
            else
            {
                currentHighlightedRing = null;
            }
        }

        private void CreateRangeIndicator()
        {
            rangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rangeIndicator.name = "RangeIndicator";
            rangeIndicator.transform.localScale = new Vector3(1f, 0.01f, 1f);
            DestroyImmediate(rangeIndicator.GetComponent<CapsuleCollider>());
            rangeIndicatorRenderer = rangeIndicator.GetComponent<Renderer>();
            rangeIndicatorRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            rangeIndicatorRenderer.material.color = rangeInRange;
            rangeIndicatorRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rangeIndicator.SetActive(false);
        }

        /// <summary>
        /// Shows or hides the attack range indicator.
        /// When timer > 0, the range auto-hides after that duration.
        /// When timer == 0, the range stays until explicitly hidden with show=false.
        /// </summary>
        public void ShowAttackRange(bool show, float timer = 0f)
        {
            if (rangeIndicator == null || champion == null) return;

            if (show)
            {
                float range = champion.attackRange * 2f;
                rangeIndicator.transform.localScale = new Vector3(range, 0.01f, range);
                rangeIndicator.SetActive(true);
                UpdateRangeIndicatorColor();

                if (timer > 0f)
                {
                    rangeHeldOpen = false;
                    rangeFlashTimer = timer;
                }
                else
                {
                    rangeHeldOpen = true;
                    rangeFlashTimer = 0f;
                }
            }
            else
            {
                rangeHeldOpen = false;
                if (rangeFlashTimer <= 0f)
                    rangeIndicator.SetActive(false);
            }
        }

        private void UpdateRangeFlash()
        {
            if (rangeFlashTimer > 0f)
            {
                rangeFlashTimer -= Time.deltaTime;
                if (rangeFlashTimer <= 0f && !rangeHeldOpen)
                    rangeIndicator.SetActive(false);
            }
        }

        private void UpdateRangeIndicatorColor()
        {
            if (rangeIndicator == null || !rangeIndicator.activeSelf) return;
            rangeIndicator.transform.position = new Vector3(
                transform.position.x, 0.02f, transform.position.z);
            bool hasTarget = GameManager.Instance != null &&
                GameManager.Instance.GetNearestEnemy(transform.position, champion.attackRange, champion.team) != null;
            rangeIndicatorRenderer.material.color = hasTarget ? rangeInRange : rangeOutOfRange;
        }

        // ─── EXECUTION (shared) ─────────────────────────────────────────

        protected void ExecuteMovement()
        {
            if (champion.isDashing) return;

            if (isChasing && attackTarget != null && !attackTarget.IsDead)
            {
                float dist = Vector3.Distance(transform.position, attackTarget.transform.position);
                if (dist > champion.attackRange)
                {
                    Vector3 dir = (attackTarget.transform.position - transform.position).normalized;
                    dir.y = 0f;
                    MoveInDirection(dir);
                }
                return;
            }

            if (isMovingToTarget)
            {
                Vector3 toTarget = moveTarget - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.5f)
                    MoveInDirection(toTarget.normalized);
                else
                    isMovingToTarget = false;
            }
        }

        protected void ExecuteAutoAttack()
        {
            bool targetLost = attackTarget == null || attackTarget.IsDead;

            if (isLockedOn && !targetLost)
            {
                float sightDist = Vector3.Distance(transform.position, attackTarget.transform.position);
                if (sightDist > champion.sightRange)
                {
                    isLockedOn = false;
                    attackTarget = null;
                    isChasing = false;
                    return;
                }
            }

            if (isChasing && !targetLost)
            {
                if (!isLockedOn && activeAttackTargetType.HasValue)
                {
                    EntityType preferred = TargetTypeToEntityType(activeAttackTargetType.Value);
                    if (attackTarget.entityType != preferred)
                    {
                        Entity better = FindNearestOfType(activeAttackTargetType.Value);
                        if (better != null)
                        {
                            attackTarget = better;
                            attackButtonTarget = better;
                        }
                    }
                }

                if (champion.CanAttack())
                {
                    float dist = Vector3.Distance(transform.position, attackTarget.transform.position);
                    if (dist <= champion.attackRange)
                    {
                        champion.PerformAutoAttack(attackTarget);
                        FaceTarget(attackTarget.transform.position);
                    }
                }
                return;
            }

            if (isChasing && targetLost)
            {
                attackTarget = null;
                isChasing = false;
                isLockedOn = false;

                if (GameManager.Instance != null && champion != null)
                {
                    var next = GameManager.Instance.GetNearestEnemy(
                        transform.position, champion.sightRange, champion.team);
                    if (next != null)
                    {
                        attackTarget = next;
                        isChasing = true;
                    }
                }
            }
        }

        // ─── HELPERS ────────────────────────────────────────────────────

        protected void MoveInDirection(Vector3 direction)
        {
            direction = ChampionSteerAroundObstacles(direction);
            transform.position += direction * champion.moveSpeed * Time.deltaTime;
            FaceTarget(transform.position + direction);
        }

        protected Vector3 ChampionSteerAroundObstacles(Vector3 desiredDir)
        {
            if (GameManager.Instance == null || champion == null) return desiredDir;

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

        protected void FaceTarget(Vector3 target)
        {
            Vector3 dir = (target - transform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 12f);
        }

        protected Entity RaycastForEntity(Vector2 screenPos)
        {
            if (Camera.main == null) return null;
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            var hits = Physics.RaycastAll(ray, 200f, ~0, QueryTriggerInteraction.Ignore);
            if (hits.Length == 0) return null;

            Entity bestEnemy = null;
            float bestEnemyDist = float.MaxValue;
            Entity fallback = null;
            Team myTeam = champion != null ? champion.team : Team.Immune;

            foreach (var hit in hits)
            {
                var entity = hit.collider.GetComponentInParent<Entity>();
                if (entity == null || entity.IsDead) continue;
                if (entity.team != myTeam)
                {
                    float priority = hit.distance;
                    if (entity.entityType == EntityType.Champion) priority *= 0.5f;
                    if (priority < bestEnemyDist)
                    {
                        bestEnemyDist = priority;
                        bestEnemy = entity;
                    }
                }
                else if (fallback == null)
                {
                    fallback = entity;
                }
            }
            return bestEnemy ?? fallback;
        }

        protected Vector3 GetMouseWorldPosition(Vector2 screenPos)
        {
            if (Camera.main == null) return transform.position;
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);
            return ray.GetPoint(20f);
        }

        protected bool IsPointerOverUI()
        {
            var es = EventSystem.current;
            return es != null && es.IsPointerOverGameObject();
        }

        protected static EntityType TargetTypeToEntityType(AttackTargetType targetType)
        {
            switch (targetType)
            {
                case AttackTargetType.Minion: return EntityType.Minion;
                case AttackTargetType.Champion: return EntityType.Champion;
                case AttackTargetType.Structure: return EntityType.Structure;
                default: return EntityType.Minion;
            }
        }

        protected Entity FindNearestOfType(AttackTargetType targetType, bool fallbackToAny = false,
            float searchRange = 0f)
        {
            if (searchRange <= 0f) searchRange = champion.sightRange;
            Team enemyTeam = champion.team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, searchRange, enemyTeam);

            EntityType filter = TargetTypeToEntityType(targetType);
            Entity nearest = null;
            Entity nearestAny = null;
            float closestDist = float.MaxValue;
            float closestAnyDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                float dist = (enemy.transform.position - transform.position).sqrMagnitude;
                if (enemy.entityType == filter && dist < closestDist)
                {
                    closestDist = dist;
                    nearest = enemy;
                }
                if (fallbackToAny && dist < closestAnyDist)
                {
                    closestAnyDist = dist;
                    nearestAny = enemy;
                }
            }
            return nearest ?? nearestAny;
        }

        protected Entity FindNearestInDirection(AttackTargetType targetType, Vector3 aimDirection,
            bool fallbackToAny = false, float searchRange = 0f, float snapRadius = 4f)
        {
            if (searchRange <= 0f) searchRange = champion.sightRange;
            Team enemyTeam = champion.team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, searchRange, enemyTeam);

            float reach = aimDirection.magnitude;
            Vector3 dir = reach > 0.01f ? aimDirection / reach : transform.forward;
            Vector3 aimEndpoint = transform.position + dir * searchRange * Mathf.Max(reach, 0.3f);

            EntityType filter = TargetTypeToEntityType(targetType);
            Entity best = null;
            Entity bestAny = null;
            float bestDist = snapRadius * snapRadius;
            float bestAnyDist = snapRadius * snapRadius;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                float distToAim = (enemy.transform.position - aimEndpoint).sqrMagnitude;
                if (enemy.entityType == filter && distToAim < bestDist)
                {
                    bestDist = distToAim;
                    best = enemy;
                }
                if (fallbackToAny && distToAim < bestAnyDist)
                {
                    bestAnyDist = distToAim;
                    bestAny = enemy;
                }
            }
            return best ?? bestAny;
        }
    }
}
