using UnityEngine;
using UnityEngine.InputSystem;

namespace Pathogen
{
    public class CameraController : MonoBehaviour
    {
        public Transform target;
        public bool mapFlipped;

        [Header("Edge Pan (Desktop)")]
        public float edgePanSpeed = 20f;
        public float edgeThreshold = 5f;

        [Header("Terrain Bounds")]
        public float minX = -50f;
        public float maxX = 50f;
        public float minZ = -15f;
        public float maxZ = 15f;

        public VirtualJoystick joystick;
        public bool followChampion;

        // Camera settings — Wild Rift-style angled top-down, orthographic
        private readonly Vector3 baseOffset = new Vector3(-3f, 20f, -7f);
        private readonly Vector3 baseLookOffset = new Vector3(0f, 0f, 3f);
        private float orthoSize = 6f;

        private Camera cam;
        private Vector3 focusPoint;
        private bool hasCentered;
        private Vector3 offset;
        private Vector3 lookOffset;
        private bool flipApplied;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
                cam = gameObject.AddComponent<Camera>();

            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
        }

        private void ApplyFlipOnce()
        {
            if (flipApplied) return;
            flipApplied = true;

            if (mapFlipped)
            {
                offset = new Vector3(-baseOffset.x, baseOffset.y, -baseOffset.z);
                lookOffset = new Vector3(-baseLookOffset.x, baseLookOffset.y, -baseLookOffset.z);
            }
            else
            {
                offset = baseOffset;
                lookOffset = baseLookOffset;
            }
        }

        void LateUpdate()
        {
            ApplyFlipOnce();
            if (target == null) return;

            if (joystick == null)
                joystick = FindAnyObjectByType<VirtualJoystick>();

            bool joystickActive = joystick != null && joystick.IsDragging;
            followChampion = GameBootstrap.IsMobile || joystickActive;

            if (followChampion)
                UpdateFollowCamera();
            else
                UpdateDesktopCamera();

            ClampFocusPoint();

            transform.position = focusPoint + offset;
            Vector3 lookAt = focusPoint + lookOffset;
            transform.rotation = Quaternion.LookRotation(lookAt - transform.position);
        }

        private void UpdateFollowCamera()
        {
            if (joystick != null && joystick.IsRightTouchActive)
            {
                Vector2 delta = joystick.RightTouchDelta;
                float flip = mapFlipped ? -1f : 1f;
                focusPoint += new Vector3(delta.x * flip, 0f, delta.y * flip);
            }
            else if (joystick != null && joystick.RightTouchReleased)
            {
                focusPoint = target.position;
            }
            else
            {
                focusPoint = target.position;
            }
        }

        private void UpdateDesktopCamera()
        {
            if (!hasCentered)
            {
                focusPoint = target.position;
                hasCentered = true;
            }

            HandleEdgePan();

            var kb = Keyboard.current;
            if (kb != null && kb.spaceKey.wasPressedThisFrame)
                focusPoint = target.position;
        }

        private void HandleEdgePan()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
                return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.isPressed || mouse.rightButton.isPressed)
                return;

            Vector2 mousePos = mouse.position.ReadValue();
            Vector3 pan = Vector3.zero;

            float screenW = Screen.width;
            float screenH = Screen.height;

            if (mousePos.x < edgeThreshold)
                pan.x -= 1f;
            else if (mousePos.x > screenW - edgeThreshold)
                pan.x += 1f;

            if (mousePos.y < edgeThreshold)
                pan.z -= 1f;
            else if (mousePos.y > screenH - edgeThreshold)
                pan.z += 1f;

            if (pan.sqrMagnitude > 0.01f)
            {
                if (mapFlipped) { pan.x = -pan.x; pan.z = -pan.z; }
                focusPoint += pan.normalized * edgePanSpeed * Time.deltaTime;
            }
        }

        private void ClampFocusPoint()
        {
            focusPoint.x = Mathf.Clamp(focusPoint.x, minX, maxX);
            focusPoint.z = Mathf.Clamp(focusPoint.z, minZ, maxZ);
        }

        public void Recenter()
        {
            if (target != null)
                focusPoint = target.position;
        }
    }
}
