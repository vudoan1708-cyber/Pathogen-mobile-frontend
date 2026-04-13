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

        public const float MarginRight = 80f;
        public const float MarginBottom = 30f;
        public const float BigButtonSize = 165f;
        public const float SmallButtonSize = 135f;
        public const float ButtonGap = 24f;
        public const float ActiveRingPadding = 12f;
        public const float MaxDragPixels = 150f;

        private static AutoAttackButton currentActive;
        private static GameObject crosshairIcon;

        private bool isPressed;
        private Vector2 pressPosition;
        private bool hasDragged;
        private Vector3 lastAimDirection;
        private float lastReach;

        void Update()
        {
            if (!isPressed || !hasDragged) return;
            playerController.OnAttackButtonAim(targetType, lastAimDirection);
            UpdateCrosshairPosition(lastAimDirection, lastReach);
        }

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
                lastReach = Mathf.Clamp01(offset.magnitude / MaxDragPixels);
                lastAimDirection = new Vector3(offset.x, 0f, offset.y).normalized * lastReach;
                playerController.OnAttackButtonAim(targetType, lastAimDirection);
                UpdateCrosshairPosition(lastAimDirection, lastReach);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            isPressed = false;

            HideCrosshair();
            playerController.ShowAttackRange(false);

            if (hasDragged)
            {
                Vector2 offset = eventData.position - pressPosition;
                if (offset.sqrMagnitude > 100f)
                {
                    float reach = Mathf.Clamp01(offset.magnitude / MaxDragPixels);
                    Vector3 aimDirection = new Vector3(offset.x, 0f, offset.y).normalized * reach;
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

        private static CameraController cachedCamera;

        private void UpdateCrosshairPosition(Vector3 aimDirection, float reach)
        {
            if (playerController == null || playerController.champion == null) return;

            if (cachedCamera == null)
                cachedCamera = FindAnyObjectByType<CameraController>();
            if (cachedCamera != null && cachedCamera.mapFlipped)
                aimDirection = new Vector3(-aimDirection.x, 0f, -aimDirection.z);

            EnsureCrosshairExists();
            crosshairIcon.SetActive(true);

            Entity snapped = playerController.GetAttackButtonTarget();
            if (snapped != null && !snapped.IsDead)
            {
                crosshairIcon.transform.position = new Vector3(
                    snapped.transform.position.x, 0.05f, snapped.transform.position.z);
            }
            else
            {
                float range = playerController.champion.attackRange * 1.5f;
                Vector3 dir = aimDirection.normalized;
                Vector3 worldPos = playerController.transform.position + dir * range * reach;
                crosshairIcon.transform.position = new Vector3(worldPos.x, 0.05f, worldPos.z);
            }
        }

        private static void HideCrosshair()
        {
            if (crosshairIcon != null) crosshairIcon.SetActive(false);
        }

        private static void EnsureCrosshairExists()
        {
            if (crosshairIcon != null) return;

            crosshairIcon = new GameObject("AttackCrosshair");
            var sr = crosshairIcon.AddComponent<SpriteRenderer>();

            var tex = Resources.Load<Texture2D>("Sprites/crosshair");
            if (tex != null)
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);
            sr.color = new Color(1f, 1f, 1f, 0.85f);
            sr.sortingOrder = 100;
            crosshairIcon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            crosshairIcon.transform.localScale = Vector3.one * 0.15f;
            crosshairIcon.SetActive(false);
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
            HideCrosshair();
        }
    }
}
