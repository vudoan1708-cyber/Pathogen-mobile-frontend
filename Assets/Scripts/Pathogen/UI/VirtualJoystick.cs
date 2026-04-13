using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Pathogen
{
    /// <summary>
    /// Dual-touch mobile controls:
    /// - LEFT side: movement joystick. Hidden until touched, appears at finger position.
    /// - RIGHT side: temporary camera pan. Drag to look around, release snaps back.
    /// Falls back to mouse input in editor/simulator when no touchscreen is available.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour
    {
        [Header("References")]
        public RectTransform handle;

        [Header("Settings")]
        public float handleRange = 55f;
        public float deadZone = 0.35f;
        public float screenSplit = 0.5f;

        [Header("Camera Pan")]
        public float cameraPanSpeed = 0.05f;

        public Vector2 Direction { get; private set; }
        public bool IsDragging { get; private set; }
        public bool IsRightTouchActive { get; private set; }
        public Vector2 RightTouchDelta { get; private set; }
        public bool RightTouchReleased { get; private set; }

        private RectTransform touchArea;
        private Canvas canvas;
        private Image touchAreaImage;
        private Image handleImage;
        private int leftFingerId = -1;
        private int rightFingerId = -1;
        private Vector2 leftTouchOrigin;
        private Vector2 rightTouchPrev;


        void Start()
        {
            touchArea = GetComponent<RectTransform>();
            if (handle == null && transform.childCount > 0)
                handle = transform.GetChild(0).GetComponent<RectTransform>();

            canvas = GetComponentInParent<Canvas>();

            // Cache Image components to toggle visibility without deactivating the GameObject
            touchAreaImage = GetComponent<Image>();
            if (handle != null) handleImage = handle.GetComponent<Image>();
            SetJoystickVisible(false);
        }

        void Update()
        {
            RightTouchDelta = Vector2.zero;
            RightTouchReleased = false;

            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
                ProcessTouches(touchscreen);
        }

        // ─── TOUCH INPUT ────────────────────────────────────────────────

        private void ProcessTouches(Touchscreen touchscreen)
        {
            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.isPressed) continue;

                int id = touch.touchId.ReadValue();
                Vector2 pos = touch.position.ReadValue();
                var phase = touch.phase.ReadValue();
                bool isLeft = pos.x < Screen.width * screenSplit;

                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (isLeft && leftFingerId < 0)
                    {
                        leftFingerId = id;
                        leftTouchOrigin = pos;
                        IsDragging = true;
                        Direction = Vector2.zero;
                        SetJoystickVisible(true);
                        MoveJoystickToScreen(pos);
                        if (handle != null) handle.anchoredPosition = Vector2.zero;
                    }
                    else if (!isLeft && rightFingerId < 0 && !IsOverUI(id))
                    {
                        // Only pan camera if touch is NOT on a UI button (skills, shop, etc.)
                        rightFingerId = id;
                        rightTouchPrev = pos;
                    }
                }

                if (id == leftFingerId)
                    UpdateJoystickDirection(pos, leftTouchOrigin);

                if (id == rightFingerId)
                {
                    IsRightTouchActive = true;
                    RightTouchDelta = (pos - rightTouchPrev) * cameraPanSpeed;
                    rightTouchPrev = pos;
                }
            }

            CheckTouchRelease(touchscreen);
        }

        private void CheckTouchRelease(Touchscreen touchscreen)
        {
            bool leftStillDown = false;
            bool rightStillDown = false;

            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.isPressed) continue;
                int id = touch.touchId.ReadValue();
                if (id == leftFingerId) leftStillDown = true;
                if (id == rightFingerId) rightStillDown = true;
            }

            if (!leftStillDown && leftFingerId >= 0)
            {
                leftFingerId = -1;
                ReleaseLeft();
            }

            if (!rightStillDown && rightFingerId >= 0)
            {
                rightFingerId = -1;
                ReleaseRight();
            }
        }

        // ─── SHARED ─────────────────────────────────────────────────────

        private void UpdateJoystickDirection(Vector2 currentPos, Vector2 origin)
        {
            Vector2 delta = currentPos - origin;
            float scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
            float rangePixels = handleRange * scaleFactor;

            Vector2 normalized = delta / rangePixels;
            normalized = Vector2.ClampMagnitude(normalized, 1f);

            if (normalized.magnitude < deadZone)
            {
                Direction = Vector2.zero;
                if (handle != null) handle.anchoredPosition = Vector2.zero;
                return;
            }

            float remapped = (normalized.magnitude - deadZone) / (1f - deadZone);
            Direction = normalized.normalized * remapped;

            if (handle != null)
                handle.anchoredPosition = normalized * handleRange;
        }

        private void ReleaseLeft()
        {
            IsDragging = false;
            Direction = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
            SetJoystickVisible(false);
        }

        private void ReleaseRight()
        {
            IsRightTouchActive = false;
            RightTouchDelta = Vector2.zero;
            RightTouchReleased = true;
        }

        private void MoveJoystickToScreen(Vector2 screenPos)
        {
            if (touchArea == null || canvas == null) return;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPos, cam, out Vector2 localPoint))
            {
                // Convert from canvas local space (pivot-centered) to bottom-left anchor
                touchArea.anchoredPosition = localPoint - canvasRect.rect.min;
            }
        }

        private void SetJoystickVisible(bool visible)
        {
            if (touchAreaImage != null) touchAreaImage.enabled = visible;
            if (handleImage != null) handleImage.enabled = visible;
        }

        private bool IsOverUI(int fingerId)
        {
            var es = EventSystem.current;
            return es != null && es.IsPointerOverGameObject(fingerId);
        }
    }
}
