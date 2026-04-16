using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Continuously resizes this RectTransform to Screen.safeArea.
    /// Add as a full-screen child of the canvas and parent all HUD elements to it
    /// to avoid notches, home indicators, and rounded corners on mobile devices.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastResolution;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (Screen.width == 0 || Screen.height == 0) return;

            Rect safe = Screen.safeArea;
            lastSafeArea = safe;
            lastResolution = new Vector2Int(Screen.width, Screen.height);

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
