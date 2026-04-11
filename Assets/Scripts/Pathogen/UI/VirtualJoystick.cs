using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pathogen
{
    /// <summary>
    /// Virtual joystick for mobile touch controls.
    /// Circular background and handle, with a generous dead zone to prevent
    /// accidental drift when tapping the center.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        public RectTransform background;
        public RectTransform handle;

        [Header("Settings")]
        public float handleRange = 55f;
        public float deadZone = 0.25f; // Requires significant directional input before moving

        public Vector2 Direction { get; private set; }
        public bool IsDragging { get; private set; }

        private Canvas canvas;
        private Camera uiCamera;
        private Vector2 pointerDownPos;

        void Start()
        {
            if (background == null)
                background = GetComponent<RectTransform>();
            if (handle == null && transform.childCount > 0)
                handle = transform.GetChild(0).GetComponent<RectTransform>();

            canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                uiCamera = canvas.worldCamera;

            // Make both images circular using the Mask approach isn't available
            // programmatically without sprites — instead we'll note this for the
            // GameBootstrap which creates the actual visual shapes
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsDragging = true;
            // Record where the finger went down — don't snap handle on tap
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, uiCamera, out pointerDownPos);
            // Don't move handle yet — wait for actual drag
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, uiCamera, out localPoint);

            // Normalize relative to background radius
            float bgRadius = background.sizeDelta.x * 0.5f;
            Vector2 normalized = localPoint / bgRadius;
            normalized = Vector2.ClampMagnitude(normalized, 1f);

            // Dead zone — must drag past threshold before registering direction
            if (normalized.magnitude < deadZone)
            {
                Direction = Vector2.zero;
                if (handle != null)
                    handle.anchoredPosition = Vector2.zero;
                return;
            }

            // Remap from dead zone edge to 1.0
            float remapped = (normalized.magnitude - deadZone) / (1f - deadZone);
            Direction = normalized.normalized * remapped;

            if (handle != null)
                handle.anchoredPosition = normalized * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsDragging = false;
            Direction = Vector2.zero;

            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
        }
    }
}
