using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Pathogen
{
    public enum AttackTargetType { Minion, Champion, Structure }

    public class AutoAttackButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public AttackTargetType targetType;
        public PlayerController playerController;
        public GameObject activeRing;
        public GameObject content;

        public const float Margin = 30f;
        public const float BigButtonSize = 110f;
        public const float SmallButtonSize = 90f;
        public const float ButtonGap = 24f;
        public const float ActiveRingPadding = 12f;

        private static AutoAttackButton currentActive;

        private bool isPressed;
        private Vector2 pressPosition;
        private bool hasDragged;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (playerController == null || playerController.champion == null) return;
            if (playerController.champion.IsDead) return;

            isPressed = true;
            hasDragged = false;
            pressPosition = eventData.position;

            SetActive(this);
            playerController.ShowAttackRange(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isPressed) return;

            Vector2 offset = eventData.position - pressPosition;
            if (offset.sqrMagnitude > 100f)
            {
                hasDragged = true;
                Vector3 aimDirection = new Vector3(offset.x, 0f, offset.y).normalized;
                playerController.OnAttackButtonAim(targetType, aimDirection);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            isPressed = false;

            playerController.ShowAttackRange(false);

            if (hasDragged)
            {
                Vector2 offset = eventData.position - pressPosition;
                if (offset.sqrMagnitude > 100f)
                {
                    Vector3 aimDirection = new Vector3(offset.x, 0f, offset.y).normalized;
                    playerController.OnAttackButtonFire(targetType, aimDirection);
                }
                else
                {
                    playerController.OnAttackButtonAutoTarget(targetType);
                }
            }
            else
            {
                playerController.OnAttackButtonAutoTarget(targetType);
            }
        }

        private static void SetActive(AutoAttackButton button)
        {
            if (currentActive != null && currentActive != button && currentActive.activeRing != null)
                currentActive.activeRing.SetActive(false);

            currentActive = button;
            if (button.activeRing != null)
                button.activeRing.SetActive(true);
        }

        public static void ClearActive()
        {
            if (currentActive != null && currentActive.activeRing != null)
                currentActive.activeRing.SetActive(false);
            currentActive = null;
        }
    }
}
