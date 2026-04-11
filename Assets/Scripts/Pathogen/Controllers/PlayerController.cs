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

        // Cancel button (X) — appears during skill aiming
        private GameObject cancelButtonGO;
        private RectTransform cancelButtonRT;
        private bool mouseOverCancel;

        // Attack target ground circle
        private GameObject attackCircle;

        private CharacterController characterController;

        void Start()
        {
            champion = GetComponent<Champion>();
            characterController = GetComponent<CharacterController>();
            aimIndicator = gameObject.AddComponent<SkillAimIndicator>();
            GenerateCursors();
            CreateCancelButton();
            CreateAttackCircle();
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
                if (kb.qKey.wasPressedThisFrame) TryStartAiming(0);
                else if (kb.wKey.wasPressedThisFrame) TryStartAiming(1);
                else if (kb.eKey.wasPressedThisFrame) TryStartAiming(2);
                else if (kb.rKey.wasPressedThisFrame) TryStartAiming(3);
            }

            if (aimingSkillIndex >= 0)
            {
                UpdateAimIndicator();

                // Check for key release
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
                    // If mouse is over the X cancel button, cancel instead of cast
                    if (mouseOverCancel)
                        CancelAiming();
                    else
                        CastAimedSkill();
                }

                // Also cancel on right-click or Escape
                var mouse = Mouse.current;
                if ((mouse != null && mouse.rightButton.wasPressedThisFrame) ||
                    kb.escapeKey.wasPressedThisFrame)
                    CancelAiming();
            }
        }

        private void TryStartAiming(int skillIndex)
        {
            if (skillIndex >= champion.skills.Length) return;
            var skill = champion.skills[skillIndex];
            if (skill == null || !skill.IsReady) return;
            if (champion.currentMana < skill.definition.manaCost) return;

            // Self-buff skills cast immediately
            if (skill.definition.type == SkillType.SelfBuff)
            {
                champion.UseSkill(skillIndex, transform.forward, Vector3.zero);
                return;
            }

            aimingSkillIndex = skillIndex;

            // Champion keeps moving toward current destination while aiming
            ShowCancelButton();
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

        private void CastAimedSkill()
        {
            var mouse = Mouse.current;
            if (mouse == null) { CancelAiming(); return; }

            Vector3 mouseWorld = GetMouseWorldPosition(mouse.position.ReadValue());
            Vector3 direction = mouseWorld - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                direction = transform.forward;

            champion.UseSkill(aimingSkillIndex, direction.normalized, mouseWorld);
            CancelAiming();
        }

        private void CancelAiming()
        {
            aimingSkillIndex = -1;
            mouseOverCancel = false;
            if (aimIndicator != null)
                aimIndicator.Hide();
            HideCancelButton();
        }

        /// <summary>
        /// Called by mobile skill buttons. Enters aiming mode for aim skills,
        /// or instant-casts self-buffs.
        /// </summary>
        public void OnSkillButtonPressed(int skillIndex)
        {
            if (champion == null || champion.IsDead) return;

            if (skillIndex >= champion.skills.Length) return;
            var skill = champion.skills[skillIndex];
            if (skill == null || !skill.IsReady) return;

            if (skill.definition.type == SkillType.SelfBuff)
            {
                champion.UseSkill(skillIndex, transform.forward, Vector3.zero);
                return;
            }

            // Enter aiming mode on mobile too
            TryStartAiming(skillIndex);
        }

        /// <summary>
        /// Called when mobile skill aiming touch ends (finger released).
        /// If the touch position is over the cancel button, cancel. Otherwise cast.
        /// </summary>
        public void OnMobileSkillRelease(Vector2 screenPosition)
        {
            if (aimingSkillIndex < 0) return;

            if (IsTouchOverCancelButton(screenPosition))
            {
                CancelAiming();
            }
            else
            {
                // Aim toward touch position
                Vector3 worldPos = GetMouseWorldPosition(screenPosition);
                Vector3 direction = worldPos - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude < 0.01f)
                    direction = transform.forward;

                champion.UseSkill(aimingSkillIndex, direction.normalized, worldPos);
                CancelAiming();
            }
        }

        // ─── CANCEL BUTTON (X) ──────────────────────────────────────────

        private void CreateCancelButton()
        {
            // Find the HUD canvas
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            cancelButtonGO = new GameObject("AimCancelButton", typeof(RectTransform));
            cancelButtonGO.transform.SetParent(canvas.transform, false);

            cancelButtonRT = cancelButtonGO.GetComponent<RectTransform>();
            // Positioned above the skill buttons (bottom-right)
            cancelButtonRT.anchorMin = new Vector2(1f, 0f);
            cancelButtonRT.anchorMax = new Vector2(1f, 0f);
            cancelButtonRT.anchoredPosition = new Vector2(-140f, 95f);
            cancelButtonRT.sizeDelta = new Vector2(50f, 50f);
            cancelButtonRT.pivot = new Vector2(0.5f, 0.5f);

            var bgImage = cancelButtonGO.AddComponent<Image>();
            bgImage.color = new Color(0.6f, 0.1f, 0.1f, 0.85f);

            // X label
            var labelGO = new GameObject("XLabel", typeof(RectTransform));
            labelGO.transform.SetParent(cancelButtonGO.transform, false);
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

            cancelButtonGO.SetActive(false);
        }

        private void ShowCancelButton()
        {
            if (cancelButtonGO != null)
                cancelButtonGO.SetActive(true);
        }

        private void HideCancelButton()
        {
            if (cancelButtonGO != null)
                cancelButtonGO.SetActive(false);
            mouseOverCancel = false;
        }

        private void UpdateCancelButtonHover()
        {
            // Check if mouse is hovering over the cancel button
            if (cancelButtonRT == null) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 screenPos = mouse.position.ReadValue();
            mouseOverCancel = RectTransformUtility.RectangleContainsScreenPoint(
                cancelButtonRT, screenPos, null);
        }

        private bool IsTouchOverCancelButton(Vector2 screenPosition)
        {
            if (cancelButtonRT == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(
                cancelButtonRT, screenPosition, null);
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

        // ─── MOUSE COMMANDS (MOVE / ATTACK) ─────────────────────────────

        private bool IsPointerOverUI()
        {
            var es = EventSystem.current;
            return es != null && es.IsPointerOverGameObject();
        }

        private void HandleMouseCommands()
        {
            if (IsPointerOverUI()) return;

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
            if (!champion.CanAttack()) return;

            if (isChasing && attackTarget != null && !attackTarget.IsDead)
            {
                float dist = Vector3.Distance(transform.position, attackTarget.transform.position);
                if (dist <= champion.attackRange)
                {
                    champion.PerformAutoAttack(attackTarget);
                    FaceTarget(attackTarget.transform.position);
                }
                return;
            }

            if (attackTarget != null && (attackTarget.IsDead || attackTarget == null))
            {
                attackTarget = null;
                isChasing = false;
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
