using UnityEngine;
using UnityEngine.UI;

namespace Pathogen
{
    public class MobilePlayerController : PlayerController
    {
        private GameObject skillCancelButton;
        private RectTransform skillCancelButtonRect;

        protected override void Start()
        {
            base.Start();
        }

        protected override void ResetControllerState()
        {
            base.ResetControllerState();
            HideCancelButton();
        }

        protected override void HandleInput()
        {
            HandleMobileMovement();
        }

        private CameraController cameraController;

        private void HandleMobileMovement()
        {
            if (moveJoystick == null || !moveJoystick.IsDragging) return;

            if (cameraController == null)
                cameraController = FindAnyObjectByType<CameraController>();

            Vector2 input = moveJoystick.Direction;
            if (cameraController != null && cameraController.mapFlipped)
                input = -input;

            Vector3 dir = new Vector3(input.x, 0f, input.y);

            if (dir.sqrMagnitude > 0.01f)
            {
                dir.Normalize();
                MoveInDirection(dir);
                isMovingToTarget = false;
                isChasing = false;
            }
        }

        // ─── SKILL AIMING ───────────────────────────────────────────────

        public override void StartAiming(int skillIndex, bool fromButtonClick)
        {
            base.StartAiming(skillIndex, fromButtonClick);
            if (aimingSkillIndex < 0) return;

            ShowCancelButton();

            var def = champion.skills[skillIndex].definition;
            Vector3 defaultPos = GetSmartAimTarget(def);
            Vector3 dir = defaultPos - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;

            if (aimIndicator != null)
            {
                switch (def.type)
                {
                    case SkillType.Projectile:
                        aimIndicator.ShowDirectionLine(transform.position, dir.normalized, def.range);
                        break;
                    case SkillType.Dash:
                        aimIndicator.ShowDirectionLine(transform.position, dir.normalized, def.dashDistance);
                        break;
                    case SkillType.AreaOfEffect:
                        var aoePos = transform.position + Vector3.ClampMagnitude(dir, def.range);
                        aoePos.y = 0.05f;
                        aimIndicator.ShowAOECircle(aoePos, def.aoeRadius);
                        break;
                }
            }
        }

        public override void CancelAiming()
        {
            base.CancelAiming();
            HideCancelButton();
        }

        public override void OnMobileSkillAimUpdate(Vector3 aimDirection)
        {
            if (aimingSkillIndex < 0 || aimIndicator == null) return;

            if (cameraController == null)
                cameraController = FindAnyObjectByType<CameraController>();
            if (cameraController != null && cameraController.mapFlipped)
                aimDirection = new Vector3(-aimDirection.x, 0f, -aimDirection.z);

            var def = champion.skills[aimingSkillIndex].definition;

            switch (def.type)
            {
                case SkillType.Projectile:
                    aimIndicator.ShowDirectionLine(transform.position, aimDirection, def.range);
                    break;
                case SkillType.Dash:
                    aimIndicator.ShowDirectionLine(transform.position, aimDirection, def.dashDistance);
                    break;
                case SkillType.AreaOfEffect:
                    var aoePos = transform.position + aimDirection * def.range;
                    aoePos.y = 0.05f;
                    aimIndicator.ShowAOECircle(aoePos, def.aoeRadius);
                    break;
            }
        }

        public override void OnMobileSkillAimRelease(Vector3 aimDirection)
        {
            if (aimingSkillIndex < 0) return;

            if (cameraController == null)
                cameraController = FindAnyObjectByType<CameraController>();
            if (cameraController != null && cameraController.mapFlipped && aimDirection.sqrMagnitude > 0.01f)
                aimDirection = new Vector3(-aimDirection.x, 0f, -aimDirection.z);

            var def = champion.skills[aimingSkillIndex].definition;
            Vector3 direction;

            if (aimDirection.sqrMagnitude > 0.01f)
            {
                direction = aimDirection;
            }
            else
            {
                Vector3 targetPos = GetSmartAimTarget(def);
                direction = targetPos - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude < 0.01f) direction = transform.forward;
            }

            champion.UseSkill(aimingSkillIndex, direction.normalized, Vector3.zero);
            CancelAiming();
        }

        // ─── CANCEL BUTTON ──────────────────────────────────────────────

        public void SetCancelButton(GameObject button)
        {
            skillCancelButton = button;
            skillCancelButtonRect = button.GetComponent<RectTransform>();
            skillCancelButton.SetActive(false);
        }

        private void ShowCancelButton()
        {
            if (skillCancelButton != null) skillCancelButton.SetActive(true);
        }

        private void HideCancelButton()
        {
            if (skillCancelButton != null) skillCancelButton.SetActive(false);
        }

        public override bool IsTouchOverCancelButton(Vector2 screenPosition)
        {
            if (skillCancelButtonRect == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(
                skillCancelButtonRect, screenPosition, null);
        }

        // ─── ATTACK BUTTONS ─────────────────────────────────────────────

        private float AttackChaseRange => champion.attackRange * 1.5f;

        public override void OnAttackButtonAutoTarget(AttackTargetType targetType)
        {
            if (champion == null || champion.IsDead) return;
            if (GameManager.Instance == null) return;

            ShowAttackRange(true, 0.5f);
            activeAttackTargetType = targetType;

            // Locked target has priority, else find nearest of type
            Entity target = isLockedOn && attackTarget != null && !attackTarget.IsDead
                ? attackTarget
                : FindNearestOfType(targetType, true, AttackChaseRange);
            if (target == null) return;

            bool isMoving = moveJoystick != null && moveJoystick.IsDragging;
            float dist = Vector3.Distance(transform.position, target.transform.position);

            if (isMoving)
            {
                // Kiting: attack if in range, face target regardless
                if (dist <= champion.attackRange && champion.CanAttack())
                    champion.PerformAutoAttack(target);
                FaceTarget(target.transform.position);
            }
            else
            {
                // Stationary: chase and attack
                attackTarget = target;
                attackButtonTarget = target;
                isChasing = true;
                isMovingToTarget = false;
            }
        }

        public override void OnAttackButtonAim(AttackTargetType targetType, Vector3 aimDirection)
        {
            if (champion == null || champion.IsDead) return;
            if (GameManager.Instance == null) return;

            ShowAttackRange(true);

            if (cameraController == null)
                cameraController = FindAnyObjectByType<CameraController>();
            if (cameraController != null && cameraController.mapFlipped)
                aimDirection = new Vector3(-aimDirection.x, 0f, -aimDirection.z);

            Entity best = FindNearestInDirection(targetType, aimDirection,
                true, AttackChaseRange);
            attackButtonTarget = best;

            bool hasTarget = best != null && !best.IsDead;
            if (aimIndicator != null)
            {
                if (hasTarget)
                {
                    Vector3 toTarget = best.transform.position - transform.position;
                    toTarget.y = 0f;
                    aimIndicator.ShowDirectionLine(transform.position, toTarget.normalized, toTarget.magnitude);
                }
                else
                {
                    float reach = aimDirection.magnitude;
                    aimIndicator.ShowDirectionLine(transform.position, aimDirection.normalized,
                        AttackChaseRange * Mathf.Max(reach, 0.1f));
                }
                aimIndicator.SetOutOfRange(!hasTarget);
            }
        }

        public override void OnAttackButtonFire(AttackTargetType targetType, Vector3 aimDirection)
        {
            if (champion == null || champion.IsDead) return;

            if (cameraController == null)
                cameraController = FindAnyObjectByType<CameraController>();
            if (cameraController != null && cameraController.mapFlipped)
                aimDirection = new Vector3(-aimDirection.x, 0f, -aimDirection.z);

            if (aimIndicator != null) aimIndicator.Hide();
            ShowAttackRange(true, 0.5f);

            Entity target = attackButtonTarget;
            if (target == null || target.IsDead)
                target = FindNearestInDirection(targetType, aimDirection,
                    true, AttackChaseRange);

            if (target != null)
            {
                activeAttackTargetType = targetType;
                attackTarget = target;
                attackButtonTarget = target;
                isLockedOn = true;
                isChasing = true;
                isMovingToTarget = false;
            }
            else
            {
                attackTarget = null;
                attackButtonTarget = null;
                isLockedOn = false;
                isChasing = false;
                activeAttackTargetType = null;
            }
            isMovingToTarget = false;
        }
    }
}
