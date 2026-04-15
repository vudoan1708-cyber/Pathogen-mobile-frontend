using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Pathogen
{
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

        public RectTransform[] ignoreRects;

        private RectTransform touchArea;
        private Canvas canvas;
        private Image touchAreaImage;
        private Image handleImage;
        private Vector2 leftTouchOrigin;
        private Vector2 rightTouchPrev;
        private bool rightWasActive;
        private int ignoredLeftTouchId = -1;

        void Start()
        {
            touchArea = GetComponent<RectTransform>();
            if (handle == null && transform.childCount > 0)
                handle = transform.GetChild(0).GetComponent<RectTransform>();

            canvas = GetComponentInParent<Canvas>();

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

        private void ProcessTouches(Touchscreen touchscreen)
        {
            bool leftFound = false;
            Vector2 leftPos = Vector2.zero;
            bool leftBegan = false;

            bool rightFound = false;
            Vector2 rightPos = Vector2.zero;
            bool rightBegan = false;

            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.isPressed)
                {
                    if (touch.touchId.ReadValue() == ignoredLeftTouchId)
                        ignoredLeftTouchId = -1;
                    continue;
                }

                Vector2 pos = touch.position.ReadValue();
                var phase = touch.phase.ReadValue();
                bool isLeft = pos.x < Screen.width * screenSplit;

                if (isLeft && !leftFound)
                {
                    int tid = touch.touchId.ReadValue();

                    if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        if (IsTouchOverIgnoredRect(pos))
                        {
                            ignoredLeftTouchId = tid;
                            continue;
                        }
                        leftBegan = true;
                    }
                    else if (tid == ignoredLeftTouchId)
                    {
                        continue;
                    }

                    leftFound = true;
                    leftPos = pos;
                }
                else if (!isLeft && !rightFound)
                {
                    if (phase == UnityEngine.InputSystem.TouchPhase.Began && !IsOverUI(touch.touchId.ReadValue()))
                    {
                        rightFound = true;
                        rightPos = pos;
                        rightBegan = true;
                    }
                    else if (rightWasActive)
                    {
                        rightFound = true;
                        rightPos = pos;
                    }
                }
            }

            // Left joystick
            if (leftFound)
            {
                if (leftBegan || !IsDragging)
                {
                    leftTouchOrigin = leftPos;
                    IsDragging = true;
                    Direction = Vector2.zero;
                    SetJoystickVisible(true);
                    MoveJoystickToScreen(leftPos);
                    if (handle != null) handle.anchoredPosition = Vector2.zero;
                }
                UpdateJoystickDirection(leftPos, leftTouchOrigin);
            }
            else if (IsDragging)
            {
                IsDragging = false;
                Direction = Vector2.zero;
                if (handle != null) handle.anchoredPosition = Vector2.zero;
                SetJoystickVisible(false);
            }

            // Right camera pan
            if (rightFound)
            {
                if (rightBegan)
                    rightTouchPrev = rightPos;

                IsRightTouchActive = true;
                RightTouchDelta = (rightPos - rightTouchPrev) * cameraPanSpeed;
                rightTouchPrev = rightPos;
            }
            else if (rightWasActive)
            {
                IsRightTouchActive = false;
                RightTouchDelta = Vector2.zero;
                RightTouchReleased = true;
            }

            rightWasActive = rightFound;
        }

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

        private void MoveJoystickToScreen(Vector2 screenPos)
        {
            if (touchArea == null || canvas == null) return;

            float scaleFactor = canvas.scaleFactor;
            touchArea.anchoredPosition = new Vector2(
                screenPos.x / scaleFactor,
                screenPos.y / scaleFactor);
        }

        private void SetJoystickVisible(bool visible)
        {
            if (touchAreaImage != null) touchAreaImage.enabled = visible;
            if (handleImage != null) handleImage.enabled = visible;
        }

        private bool IsTouchOverIgnoredRect(Vector2 screenPos)
        {
            if (ignoreRects == null) return false;
            for (int i = 0; i < ignoreRects.Length; i++)
            {
                if (ignoreRects[i] != null &&
                    RectTransformUtility.RectangleContainsScreenPoint(ignoreRects[i], screenPos, null))
                    return true;
            }
            return false;
        }

        private bool IsOverUI(int fingerId)
        {
            var es = EventSystem.current;
            return es != null && es.IsPointerOverGameObject(fingerId);
        }
    }
}
