using UnityEngine;
using UnityEngine.UI;

namespace Pathogen
{
    /// <summary>
    /// World-space floating text that drifts upward and fades out.
    /// Uses a mini Canvas + UI Text for reliable font rendering.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        private Text uiText;
        private CanvasGroup canvasGroup;
        private Color startColor;
        private float duration;
        private float elapsed;
        private Vector3 velocity;

        public static void Spawn(Vector3 position, string text, Color color, float duration = 0.9f)
        {
            var go = new GameObject("FloatingText");
            go.transform.position = position;

            // World-space canvas sized for a single text element
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 200;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(2f, 0.5f);
            rt.localScale = Vector3.one * 0.02f;

            var cg = go.AddComponent<CanvasGroup>();

            // Text element
            var textGO = new GameObject("Label", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);

            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            var uiText = textGO.AddComponent<Text>();
            uiText.text = text;
            uiText.fontSize = 24;
            uiText.color = color;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.font = GameBootstrap.UIFont;
            uiText.fontStyle = FontStyle.Bold;
            uiText.horizontalOverflow = HorizontalWrapMode.Overflow;

            // FloatingText behaviour
            var ft = go.AddComponent<FloatingText>();
            ft.uiText = uiText;
            ft.canvasGroup = cg;
            ft.startColor = color;
            ft.duration = duration;
            ft.velocity = new Vector3(Random.Range(-0.3f, 0.3f), 1.5f, 0f);
        }

        void Update()
        {
            elapsed += Time.deltaTime;

            transform.position += velocity * Time.deltaTime;

            // Face camera
            if (Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;

            // Fade via CanvasGroup alpha
            float alpha = Mathf.Clamp01(1f - elapsed / duration);
            canvasGroup.alpha = alpha;

            if (elapsed >= duration)
                Destroy(gameObject);
        }
    }
}
