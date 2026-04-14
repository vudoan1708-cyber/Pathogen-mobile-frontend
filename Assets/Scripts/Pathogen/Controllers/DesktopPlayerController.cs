using UnityEngine;
using UnityEngine.InputSystem;

namespace Pathogen
{
    public class DesktopPlayerController : PlayerController
    {
        [Header("Cursor")]
        public Texture2D defaultCursor;
        public Texture2D attackCursor;

        private Entity hoveredEntity;
        private TargetHighlight hoveredHighlight;
        private bool aimStartedByClick;
        private int aimStartFrame;
        private bool mouseOverCancel;

        protected override void Start()
        {
            base.Start();
            GenerateCursors();
        }

        protected override void ResetControllerState()
        {
            base.ResetControllerState();
            ClearHover();
        }

        protected override void HandleInput()
        {
            HandleSkillAiming();

            if (aimingSkillIndex < 0)
                HandleHover();
            else
            {
                ClearHover();
                UpdateCancelButtonHover();
            }

            HandleMouseCommands();
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

                if (aimStartedByClick && Time.frameCount > aimStartFrame)
                {
                    var mouse = Mouse.current;
                    if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                    {
                        if (IsPointerOverUI())
                            CancelAiming();
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

        public override void StartAiming(int skillIndex, bool fromButtonClick)
        {
            base.StartAiming(skillIndex, fromButtonClick);
            if (aimingSkillIndex < 0) return;

            aimStartedByClick = fromButtonClick;
            aimStartFrame = Time.frameCount;

            var def = champion.skills[skillIndex].definition;
            Vector3 defaultPos = GetSmartAimTarget(def);
            Vector3 dir = defaultPos - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;

            if (aimIndicator != null)
                aimIndicator.ShowSkillIndicator(def, transform.position, dir);
        }

        private void UpdateAimIndicator()
        {
            var mouse = Mouse.current;
            if (mouse == null || aimIndicator == null) return;

            Vector3 mouseWorld = GetMouseWorldPosition(mouse.position.ReadValue());
            Vector3 direction = mouseWorld - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) direction = transform.forward;

            var def = champion.skills[aimingSkillIndex].definition;
            aimIndicator.ShowSkillIndicator(def, transform.position, direction);
            aimIndicator.ShowRangeRing(transform.position, def.GetEffectiveRange());
        }

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

            skillFiredThisFrame = true;
            CancelAiming();
        }

        private void UpdateCancelButtonHover()
        {
            mouseOverCancel = false;
        }

        // ─── MOUSE COMMANDS ─────────────────────────────────────────────

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
                    attackTarget = clickedEntity;
                    isChasing = true;
                    isMovingToTarget = false;
                    ShowAttackRange(true, 0.3f);
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
