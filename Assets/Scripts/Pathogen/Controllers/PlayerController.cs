using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Pathogen
{
    /// <summary>
    /// LoL-style player controls:
    /// - Left-click ground: move to position
    /// - Left-click enemy: move toward and auto-attack
    /// - Hover enemy: highlight + sword cursor
    /// - Hold Q/W/E/R: show aim indicator, champion stops, release to cast
    /// - Release over X button or right-click/Escape: cancel aiming
    /// - Mobile: joystick + skill buttons with drag-to-cancel
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public Champion champion;
        public VirtualJoystick moveJoystick;

        [Header("Attack")]
        public float attackRange = 10f;

        [Header("Cursor")]
        public Texture2D defaultCursor;
        public Texture2D attackCursor;

        // Movement
        private Vector3 moveTarget;
        private bool isMovingToTarget;
        private Entity attackTarget;
        private bool isChasing;

        // Hover
        private Entity hoveredEntity;
        private TargetHighlight hoveredHighlight;

        // Skill aiming
        private int aimingSkillIndex = -1;
        private SkillAimIndicator aimIndicator;
        private bool aimStartedByClick;
        private int aimStartFrame;
        private bool skillFiredThisFrame; // True = button click (needs second click to fire), False = keyboard

        // Cancel button (X) — appears during skill aiming
        private GameObject skillCancelButton;
        private RectTransform skillCancelButtonRect;
        private bool mouseOverCancel;

        // Attack target ground circle
        private GameObject attackCircle;

        // Range indicator circle around champion
        private GameObject rangeIndicator;
        private Renderer rangeIndicatorRenderer;
        private static readonly Color rangeInRange = new Color(0.3f, 0.6f, 1f, 0.15f);
        private static readonly Color rangeOutOfRange = new Color(1f, 0.3f, 0.3f, 0.15f);

        // Attack button targeting
        private Entity attackButtonTarget;
        private AttackTargetType? activeAttackTargetType;
        private GameObject crosshairIndicator;

        private CharacterController characterController;

        void Start()
        {
            champion = GetComponent<Champion>();
            characterController = GetComponent<CharacterController>();
            aimIndicator = gameObject.AddComponent<SkillAimIndicator>();
            GenerateCursors();
            CreateCancelButton();
            CreateAttackCircle();
            CreateCrosshairIndicator();
            CreateRangeIndicator();

            if (champion != null)
                champion.OnRespawn += ResetControllerState;
        }

        private void ResetControllerState()
        {
            isMovingToTarget = false;
            isChasing = false;
            attackTarget = null;
            attackButtonTarget = null;
            activeAttackTargetType = null;
            HideCrosshair();
            CancelAiming();
            ClearHover();
        }

        void Update()
        {
            if (champion == null || champion.IsDead || champion.isRespawning)
            {
                if (aimIndicator != null) aimIndicator.Hide();
                HideCancelButton();
                return;
            }
            if (GameManager.Instance != null && !GameManager.Instance.GameActive) return;

            skillFiredThisFrame = false;
            HandleSkillAiming();

            // Hover only when not aiming
            if (aimingSkillIndex < 0)
                HandleHover();
            else
            {
                ClearHover();
                UpdateCancelButtonHover();
            }

            // Mouse move/attack clicks work even while aiming (reposition while holding skill key)
            HandleMouseCommands();

            // Movement and mobile input always execute — champion keeps walking while aiming
            HandleMobileInput();
            ExecuteMovement();
            ExecuteAutoAttack();
            UpdateAttackCircle();
        }

        // ─── HOVER & CURSOR ─────────────────────────────────────────────

        private void HandleHover()
        {
            if (IsPointerOverUI())
            {
                ClearHover();
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null) return;

            Entity newHovered = RaycastForEntity(mouse.position.ReadValue());

            if (hoveredEntity != newHovered)
            {
                if (hoveredHighlight != null)
                    hoveredHighlight.SetHighlighted(false);

                hoveredEntity = newHovered;

                if (hoveredEntity != null && hoveredEntity.team != champion.team && !hoveredEntity.IsDead)
                {
                    hoveredHighlight = hoveredEntity.GetComponent<TargetHighlight>();
                    if (hoveredHighlight != null)
                        hoveredHighlight.SetHighlighted(true);

                    Cursor.SetCursor(attackCursor, new Vector2(16, 16), CursorMode.Auto);
                }
                else
                {
                    hoveredEntity = null;
                    hoveredHighlight = null;
                    Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
                }
            }
        }

        private void ClearHover()
        {
            if (hoveredHighlight != null)
                hoveredHighlight.SetHighlighted(false);
            hoveredEntity = null;
            hoveredHighlight = null;
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }

        // ─── SKILL AIMING ───────────────────────────────────────────────

        private void HandleSkillAiming()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // Start aiming on key press
            if (aimingSkillIndex < 0)
            {
                if (kb.qKey.wasPressedThisFrame) StartAiming(0, false);
                else if (kb.wKey.wasPressedThisFrame) StartAiming(1, false);
                else if (kb.eKey.wasPressedThisFrame) StartAiming(2, false);
                else if (kb.rKey.wasPressedThisFrame) StartAiming(3, false);
            }

            if (aimingSkillIndex >= 0)
            {
                UpdateAimIndicator();

                // Keyboard-initiated: key release fires
                if (!aimStartedByClick)
                {
                    bool released = false;
                    switch (aimingSkillIndex)
                    {
                        case 0: released = kb.qKey.wasReleasedThisFrame; break;
                        case 1: released = kb.wKey.wasReleasedThisFrame; break;
                        case 2: released = kb.eKey.wasReleasedThisFrame; break;
                        case 3: released = kb.rKey.wasReleasedThisFrame; break;
                    }

                    if (released)
                    {
                        if (mouseOverCancel)
                            CancelAiming();
                        else
                            CastAimedSkill();
                    }
                }

                // Button-click-initiated: second click fires, re-click on UI cancels
                if (aimStartedByClick && Time.frameCount > aimStartFrame)
                {
                    var mouse = Mouse.current;
                    if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                    {
                        if (IsPointerOverUI())
                            CancelAiming(); // Clicking button again or any UI = cancel
                        else
                            CastAimedSkill();
                    }
                }

                var cancelMouse = Mouse.current;
                if ((cancelMouse != null && cancelMouse.rightButton.wasPressedThisFrame) ||
                    kb.escapeKey.wasPressedThisFrame)
                    CancelAiming();
            }
        }

        public void StartAiming(int skillIndex, bool fromButtonClick)
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

            aimingSkillIndex = skillIndex;
            aimStartedByClick = fromButtonClick;
            aimStartFrame = Time.frameCount;

            // X cancel button only shown on mobile
            if (GameBootstrap.IsMobile)
                ShowCancelButton();

            var def = skill.definition;
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

        private Vector3 GetSmartAimTarget(SkillDefinition def)
        {
            Vector3 fallback = transform.position + transform.forward * def.range * 0.5f;
            if (GameManager.Instance == null) return fallback;

            Team enemyTeam = champion.team == Team.Virus ? Team.Immune : Team.Virus;
            var enemiesInRange = GameManager.Instance.GetEntitiesInRange(transform.position, def.range, enemyTeam);

            if (enemiesInRange.Count == 0) return fallback;
            if (enemiesInRange.Count == 1 && !enemiesInRange[0].IsDead)
                return enemiesInRange[0].transform.position;

            Entity nearestChampion = null;
            Entity nearestMinion = null;
            float closestChampDist = float.MaxValue;
            float closestMinionDist = float.MaxValue;

            foreach (var enemy in enemiesInRange)
            {
                if (enemy.IsDead) continue;
                float dist = (enemy.transform.position - transform.position).sqrMagnitude;

                if (enemy.entityType == EntityType.Champion && dist < closestChampDist)
                    { closestChampDist = dist; nearestChampion = enemy; }
                else if (enemy.entityType == EntityType.Minion && dist < closestMinionDist)
                    { closestMinionDist = dist; nearestMinion = enemy; }
            }

            if (nearestChampion != null) return nearestChampion.transform.position;
            if (nearestMinion != null) return nearestMinion.transform.position;
            return fallback;
        }

        private void UpdateAimIndicator()
        {
            var mouse = Mouse.current;
            if (mouse == null || aimIndicator == null) return;

            Vector3 mouseWorld = GetMouseWorldPosition(mouse.position.ReadValue());
            Vector3 direction = mouseWorld - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                direction = transform.forward;

            var skill = champion.skills[aimingSkillIndex];
            var def = skill.definition;

            switch (def.type)
            {
                case SkillType.Projectile:
                    aimIndicator.ShowDirectionLine(
                        transform.position, direction.normalized, def.range);
                    break;

                case SkillType.Dash:
                    aimIndicator.ShowDirectionLine(
                        transform.position, direction.normalized, def.dashDistance);
                    break;

                case SkillType.AreaOfEffect:
                    Vector3 aoePos = transform.position + Vector3.ClampMagnitude(direction, def.range);
                    aoePos.y = 0.05f;
                    aimIndicator.ShowAOECircle(aoePos, def.aoeRadius);
                    break;
            }

            // Don't rotate champion during aiming — let movement rotation handle facing
        }

        /// <summary>Desktop only — fires skill toward current mouse position.</summary>
        private void CastAimedSkill()
        {
            var mouse = Mouse.current;
            if (mouse == null) { CancelAiming(); return; }

            var def = champion.skills[aimingSkillIndex].definition;

            Vector3 mouseWorld = GetMouseWorldPosition(mouse.position.ReadValue());
            Vector3 direction = mouseWorld - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) direction = transform.forward;

            if (def.faceSkillDirection)
                transform.rotation = Quaternion.LookRotation(direction.normalized);

            champion.UseSkill(aimingSkillIndex, direction.normalized, Vector3.zero);

            if (def.rootOnFire)
            {
                isMovingToTarget = false;
                isChasing = false;
            }

            // Prevent the fire-click from also being consumed as a move command
            skillFiredThisFrame = true;

            CancelAiming();
        }

        public void CancelAiming()
        {
            aimingSkillIndex = -1;
            mouseOverCancel = false;
            if (aimIndicator != null)
                aimIndicator.Hide();
            HideCancelButton();
        }

        // ─── MOBILE SKILL INTERACTION ────────────────────────────────────


        /// <summary>Mobile drag — direction comes from skill button joystick offset.</summary>
        public void OnMobileSkillAimUpdate(Vector3 aimDirection)
        {
            if (aimingSkillIndex < 0 || aimIndicator == null) return;

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

        /// <summary>Mobile release — Vector3.zero means quick tap (smart target).</summary>
        public void OnMobileSkillAimRelease(Vector3 aimDirection)
        {
            if (aimingSkillIndex < 0) return;

            var def = champion.skills[aimingSkillIndex].definition;
            Vector3 direction;

            if (aimDirection.sqrMagnitude > 0.01f)
            {
                direction = aimDirection;
            }
            else
            {
                // Quick tap — smart target
                Vector3 targetPos = GetSmartAimTarget(def);
                direction = targetPos - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude < 0.01f) direction = transform.forward;
            }

            champion.UseSkill(aimingSkillIndex, direction.normalized, Vector3.zero);
            CancelAiming();
        }

        // ─── CANCEL BUTTON (X) ──────────────────────────────────────────

        private void CreateCancelButton()
        {
            // Find the HUD canvas
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            skillCancelButton = new GameObject("AimCancelButton", typeof(RectTransform));
            skillCancelButton.transform.SetParent(canvas.transform, false);

            skillCancelButtonRect = skillCancelButton.GetComponent<RectTransform>();
            // Positioned above the skill buttons (bottom-right)
            skillCancelButtonRect.anchorMin = new Vector2(1f, 0f);
            skillCancelButtonRect.anchorMax = new Vector2(1f, 0f);
            skillCancelButtonRect.anchoredPosition = new Vector2(-140f, 95f);
            skillCancelButtonRect.sizeDelta = new Vector2(50f, 50f);
            skillCancelButtonRect.pivot = new Vector2(0.5f, 0.5f);

            var bgImage = skillCancelButton.AddComponent<Image>();
            bgImage.color = new Color(0.6f, 0.1f, 0.1f, 0.85f);

            // X label
            var labelGO = new GameObject("XLabel", typeof(RectTransform));
            labelGO.transform.SetParent(skillCancelButton.transform, false);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.sizeDelta = Vector2.zero;
            labelRT.anchoredPosition = Vector2.zero;

            var label = labelGO.AddComponent<Text>();
            label.text = "X";
            label.fontSize = 28;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.font = GameBootstrap.UIFont;

            skillCancelButton.SetActive(false);
        }

        private void ShowCancelButton()
        {
            if (skillCancelButton != null)
                skillCancelButton.SetActive(true);
        }

        private void HideCancelButton()
        {
            if (skillCancelButton != null)
                skillCancelButton.SetActive(false);
            mouseOverCancel = false;
        }

        private void UpdateCancelButtonHover()
        {
            if (skillCancelButtonRect == null || skillCancelButton == null) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 screenPos = mouse.position.ReadValue();
            mouseOverCancel = RectTransformUtility.RectangleContainsScreenPoint(
                skillCancelButtonRect, screenPos, null);

            var img = skillCancelButton.GetComponent<Image>();
            if (img != null)
                img.color = mouseOverCancel
                    ? new Color(1f, 0.2f, 0.2f, 1f)
                    : new Color(0.6f, 0.1f, 0.1f, 0.85f);
        }

        public bool IsTouchOverCancelButton(Vector2 screenPosition)
        {
            if (skillCancelButtonRect == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(
                skillCancelButtonRect, screenPosition, null);
        }

        // ─── ATTACK TARGET CIRCLE ────────────────────────────────────────

        private void CreateAttackCircle()
        {
            attackCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            attackCircle.name = "AttackTargetCircle";
            attackCircle.transform.localScale = new Vector3(1.8f, 0.02f, 1.8f);
            DestroyImmediate(attackCircle.GetComponent<CapsuleCollider>());

            var renderer = attackCircle.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            // Slightly darker yellow than the hover indicator
            renderer.material.color = new Color(0.85f, 0.75f, 0.25f, 0.7f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            attackCircle.SetActive(false);
        }

        private void UpdateAttackCircle()
        {
            if (attackCircle == null) return;

            if (isChasing && attackTarget != null && !attackTarget.IsDead)
            {
                attackCircle.SetActive(true);
                // Pin to ground level like the hover ring
                attackCircle.transform.position = new Vector3(
                    attackTarget.transform.position.x, 0.03f, attackTarget.transform.position.z);
            }
            else
            {
                attackCircle.SetActive(false);
            }
        }

        // ─── RANGE INDICATOR ─────────────────────────────────────────────

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

        public void ShowAttackRange(bool show)
        {
            if (rangeIndicator == null || champion == null) return;

            if (show)
            {
                float range = champion.attackRange * 2f;
                rangeIndicator.transform.localScale = new Vector3(range, 0.01f, range);
                rangeIndicator.SetActive(true);
                UpdateRangeIndicatorColor();
            }
            else
            {
                rangeIndicator.SetActive(false);
                HideCrosshair();
            }
        }

        private void UpdateRangeIndicatorColor()
        {
            if (rangeIndicator == null || !rangeIndicator.activeSelf) return;

            rangeIndicator.transform.position = new Vector3(
                transform.position.x, 0.02f, transform.position.z);

            bool hasTarget = GameManager.Instance != null &&
                GameManager.Instance.GetNearestEnemy(
                    transform.position, champion.attackRange, champion.team) != null;

            rangeIndicatorRenderer.material.color = hasTarget ? rangeInRange : rangeOutOfRange;
        }

        // ─── CROSSHAIR INDICATOR ─────────────────────────────────────────

        private void CreateCrosshairIndicator()
        {
            crosshairIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crosshairIndicator.name = "CrosshairIndicator";
            crosshairIndicator.transform.localScale = new Vector3(2.2f, 0.02f, 2.2f);
            DestroyImmediate(crosshairIndicator.GetComponent<CapsuleCollider>());

            var renderer = crosshairIndicator.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            renderer.material.color = new Color(1f, 0.3f, 0.3f, 0.6f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            crosshairIndicator.SetActive(false);
        }

        private void ShowCrosshair(Entity target)
        {
            if (crosshairIndicator == null) return;

            if (target != null && !target.IsDead)
            {
                crosshairIndicator.SetActive(true);
                crosshairIndicator.transform.position = new Vector3(
                    target.transform.position.x, 0.04f, target.transform.position.z);
            }
            else
            {
                crosshairIndicator.SetActive(false);
            }
        }

        private void HideCrosshair()
        {
            if (crosshairIndicator != null)
                crosshairIndicator.SetActive(false);
        }

        // ─── ATTACK BUTTONS (mobile) ────────────────────────────────────

        public void OnAttackButtonAutoTarget(AttackTargetType targetType)
        {
            if (champion == null || champion.IsDead) return;
            if (GameManager.Instance == null) return;

            activeAttackTargetType = targetType;
            Entity target = FindNearestOfType(targetType, true);
            if (target == null) return;

            attackTarget = target;
            attackButtonTarget = target;
            isChasing = true;
            isMovingToTarget = false;
        }

        public void OnAttackButtonAim(AttackTargetType targetType, Vector3 aimDirection)
        {
            if (champion == null || champion.IsDead) return;
            if (GameManager.Instance == null) return;

            Entity best = FindNearestInDirection(targetType, aimDirection);
            attackButtonTarget = best;
            ShowCrosshair(best);

            if (best == null)
            {
                attackTarget = null;
                isChasing = false;
                activeAttackTargetType = null;
            }

            UpdateRangeIndicatorColor();
        }

        public void OnAttackButtonFire(AttackTargetType targetType, Vector3 aimDirection)
        {
            if (champion == null || champion.IsDead) return;

            HideCrosshair();

            Entity target = FindNearestInDirection(targetType, aimDirection);
            if (target == null) return;

            activeAttackTargetType = targetType;
            attackTarget = target;
            attackButtonTarget = target;
            isChasing = true;
            isMovingToTarget = false;
        }

        private Entity FindNearestOfType(AttackTargetType targetType, bool fallbackToAny = false)
        {
            Team enemyTeam = champion.team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, champion.sightRange, enemyTeam);

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
            return nearest != null ? nearest : nearestAny;
        }

        private Entity FindNearestInDirection(AttackTargetType targetType, Vector3 aimDirection,
            bool fallbackToAny = false)
        {
            Team enemyTeam = champion.team == Team.Virus ? Team.Immune : Team.Virus;
            var enemies = GameManager.Instance.GetEntitiesInRange(
                transform.position, champion.sightRange, enemyTeam);

            EntityType filter = TargetTypeToEntityType(targetType);
            Entity best = null;
            Entity bestAny = null;
            float bestScore = -1f;
            float bestAnyScore = -1f;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                toEnemy.y = 0f;
                float dot = Vector3.Dot(aimDirection, toEnemy);
                if (enemy.entityType == filter && dot > bestScore)
                {
                    bestScore = dot;
                    best = enemy;
                }
                if (fallbackToAny && dot > bestAnyScore)
                {
                    bestAnyScore = dot;
                    bestAny = enemy;
                }
            }
            return best != null ? best : bestAny;
        }

        private static EntityType TargetTypeToEntityType(AttackTargetType targetType)
        {
            switch (targetType)
            {
                case AttackTargetType.Minion: return EntityType.Minion;
                case AttackTargetType.Champion: return EntityType.Champion;
                case AttackTargetType.Structure: return EntityType.Structure;
                default: return EntityType.Minion;
            }
        }

        // ─── MOUSE COMMANDS (MOVE / ATTACK) ─────────────────────────────

        private bool IsPointerOverUI()
        {
            var es = EventSystem.current;
            return es != null && es.IsPointerOverGameObject();
        }

        private void HandleMouseCommands()
        {
            if (IsPointerOverUI()) return;
            if (skillFiredThisFrame) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector2 screenPos = mouse.position.ReadValue();
                Entity clickedEntity = RaycastForEntity(screenPos);

                if (clickedEntity != null && clickedEntity.team != champion.team && !clickedEntity.IsDead)
                {
                    float dist = Vector3.Distance(transform.position, clickedEntity.transform.position);
                    if (dist <= attackRange)
                    {
                        attackTarget = clickedEntity;
                        isChasing = true;
                        isMovingToTarget = false;
                    }
                }
                else
                {
                    Vector3 worldPos = GetMouseWorldPosition(screenPos);
                    moveTarget = worldPos;
                    moveTarget.y = transform.position.y;
                    isMovingToTarget = true;
                    isChasing = false;
                    attackTarget = null;
                }
            }
        }

        // ─── MOBILE INPUT ───────────────────────────────────────────────

        private void HandleMobileInput()
        {
            // Joystick blocked during aiming (handled by caller)
            if (moveJoystick == null || !moveJoystick.IsDragging) return;

            Vector2 input = moveJoystick.Direction;
            Vector3 dir = new Vector3(input.x, 0f, input.y);

            if (dir.sqrMagnitude > 0.01f)
            {
                dir.Normalize();
                MoveInDirection(dir);
                isMovingToTarget = false;
                isChasing = false;
            }
        }

        // ─── EXECUTION ──────────────────────────────────────────────────

        private void ExecuteMovement()
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

        private void ExecuteAutoAttack()
        {
            // Check if current target is gone (dead or destroyed)
            bool targetLost = attackTarget == null || attackTarget.IsDead;

            if (isChasing && !targetLost)
            {
                // Re-evaluate: if chasing a fallback target, switch to preferred type when nearby
                if (activeAttackTargetType.HasValue)
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

            // Auto-retarget: target died or was destroyed — find next visible enemy
            if (isChasing && targetLost)
            {
                attackTarget = null;
                isChasing = false;

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

        private void MoveInDirection(Vector3 direction)
        {
            if (characterController != null)
                characterController.Move(direction * champion.moveSpeed * Time.deltaTime);
            else
                transform.position += direction * champion.moveSpeed * Time.deltaTime;

            FaceTarget(transform.position + direction);
        }

        private void FaceTarget(Vector3 target)
        {
            Vector3 dir = (target - transform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 12f);
        }

        private Entity RaycastForEntity(Vector2 screenPos)
        {
            if (Camera.main == null) return null;

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            var hits = Physics.RaycastAll(ray, 200f);
            if (hits.Length == 0) return null;

            // Prioritize: enemy champions > enemy minions > enemy structures > allies
            Entity bestEnemy = null;
            float bestEnemyDist = float.MaxValue;
            Entity fallback = null;

            Team myTeam = champion != null ? champion.team : Team.Immune;

            foreach (var hit in hits)
            {
                var entity = hit.collider.GetComponent<Entity>();
                if (entity == null || entity.IsDead) continue;

                if (entity.team != myTeam)
                {
                    // Weight champions higher — use a lower artificial distance
                    float priority = hit.distance;
                    if (entity.entityType == EntityType.Champion)
                        priority *= 0.5f;

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

        private Vector3 GetMouseWorldPosition(Vector2 screenPos)
        {
            if (Camera.main == null) return transform.position;
            Ray ray = Camera.main.ScreenPointToRay(screenPos);

            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);

            return ray.GetPoint(20f);
        }

        // ─── CURSOR GENERATION ──────────────────────────────────────────

        private void GenerateCursors()
        {
            defaultCursor = null;

            attackCursor = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color red = new Color(0.9f, 0.2f, 0.1f, 1f);
            Color darkRed = new Color(0.6f, 0.1f, 0.05f, 1f);

            var pixels = attackCursor.GetPixels();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
            attackCursor.SetPixels(pixels);

            for (int y = 4; y < 24; y++)
            {
                attackCursor.SetPixel(15, y, red);
                attackCursor.SetPixel(16, y, red);
            }
            for (int x = 10; x < 22; x++)
            {
                attackCursor.SetPixel(x, 10, darkRed);
                attackCursor.SetPixel(x, 11, darkRed);
            }
            for (int y = 0; y < 6; y++)
            {
                attackCursor.SetPixel(15, y, darkRed);
                attackCursor.SetPixel(16, y, darkRed);
            }
            attackCursor.SetPixel(15, 24, red);
            attackCursor.SetPixel(16, 24, red);
            attackCursor.SetPixel(15, 25, red);
            attackCursor.SetPixel(16, 25, red);

            attackCursor.Apply();
            attackCursor.filterMode = FilterMode.Point;
        }
    }
}
