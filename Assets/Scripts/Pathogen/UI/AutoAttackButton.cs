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
        public const float BigButtonSize = 180f;
        public const float SmallButtonSize = 147f;
        public const float ButtonGap = 27f;
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
        private static Material crosshairMaterial;

        private static readonly Color crosshairBlue = new Color(0.3f, 0.5f, 0.9f, 0.5f);
        private static readonly Color crosshairSnapped = new Color(0.3f, 0.5f, 0.9f, 0.7f);
        private static readonly Color crosshairOutOfRange = new Color(0.9f, 0.25f, 0.2f, 0.5f);

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
            bool hasTarget = snapped != null && !snapped.IsDead;

            if (hasTarget)
            {
                crosshairIcon.transform.position = new Vector3(
                    snapped.transform.position.x, 0.05f, snapped.transform.position.z);
                crosshairMaterial.SetColor("_Color", crosshairSnapped);
            }
            else
            {
                float range = playerController.champion.attackRange * 1.5f;
                Vector3 dir = aimDirection.normalized;
                Vector3 worldPos = playerController.transform.position + dir * range * reach;
                crosshairIcon.transform.position = new Vector3(worldPos.x, 0.05f, worldPos.z);
                crosshairMaterial.SetColor("_Color", crosshairOutOfRange);
            }
        }

        private static void HideCrosshair()
        {
            if (crosshairIcon != null) crosshairIcon.SetActive(false);
        }

        private static void EnsureCrosshairExists()
        {
            if (crosshairIcon != null) return;

            crosshairMaterial = new Material(ShaderLibrary.Instance.crosshair);
            crosshairMaterial.SetColor("_Color", crosshairBlue);

            crosshairIcon = GameObject.CreatePrimitive(PrimitiveType.Quad);
            crosshairIcon.name = "AttackCrosshair";
            Object.DestroyImmediate(crosshairIcon.GetComponent<MeshCollider>());

            var renderer = crosshairIcon.GetComponent<Renderer>();
            renderer.material = crosshairMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            crosshairIcon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            crosshairIcon.transform.localScale = Vector3.one * 1.5f;
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
