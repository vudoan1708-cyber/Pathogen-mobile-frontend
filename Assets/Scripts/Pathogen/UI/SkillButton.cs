using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Pathogen
{
    /// <summary>
    /// Mobile: mini joystick — drag offset from button center maps to skill aim direction.
    ///   Quick tap = smart target. Hold+drag = aim, release fires. Beyond boundary or X = cancel.
    /// Desktop: click only enters aim mode. Drag and release do nothing — second click fires via PlayerController.
    /// </summary>
    public class SkillButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public int skillIndex;
        public PlayerController playerController;
        public float cancelBoundary = 350f;

        private const float DragThresholdSq = 100f; // 10px radius — matches AutoAttackButton

        private bool isPressed;
        private bool hasDragged;
        private Vector2 pressPosition;
        private Vector3 lastAimDirection;
        private bool IsMobile => GameBootstrap.IsMobile;

        void Update()
        {
            if (!isPressed || !hasDragged || !IsMobile) return;
            playerController.OnMobileSkillAimUpdate(lastAimDirection);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (playerController == null || playerController.champion == null) return;
            if (playerController.champion.IsDead) return;

            isPressed = true;
            hasDragged = false;
            pressPosition = eventData.position;
            AutoAttackButton.ClearActive();
            playerController.StartAiming(skillIndex, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isPressed || !IsMobile) return;

            Vector2 offset = eventData.position - pressPosition;
            if (offset.sqrMagnitude > DragThresholdSq)
            {
                hasDragged = true;
                lastAimDirection = new Vector3(offset.x, 0f, offset.y).normalized;
                playerController.OnMobileSkillAimUpdate(lastAimDirection);
            }

            bool overCancel = playerController.IsTouchOverCancelButton(eventData.position);
            playerController.SetAimCancelTint(overCancel);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            isPressed = false;

            if (!IsMobile) return;

            float dist = Vector2.Distance(eventData.position, pressPosition);

            if (playerController.IsTouchOverCancelButton(eventData.position))
            {
                playerController.CancelAiming();
                return;
            }

            Vector2 offset = eventData.position - pressPosition;
            if (offset.sqrMagnitude > DragThresholdSq)
            {
                Vector3 aimDirection = new Vector3(offset.x, 0f, offset.y).normalized;
                playerController.OnMobileSkillAimRelease(aimDirection);
            }
            else
            {
                playerController.OnMobileSkillAimRelease(Vector3.zero);
            }
        }
    }
}
