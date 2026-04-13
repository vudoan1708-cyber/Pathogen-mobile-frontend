using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pathogen
{
    /// <summary>
    /// Human health bar with gradient fill (red→yellow→green),
    /// animated transitions, and condition labels.
    /// </summary>
    public class HumanHealthBar : MonoBehaviour
    {
        [Header("References")]
        public Image gradientFill;
        public Image animatedTrail; // Trails behind the fill to show recent change
        public TextMeshProUGUI percentText;
        public TextMeshProUGUI conditionText;
        public TextMeshProUGUI deathLabel;
        public TextMeshProUGUI healthyLabel;

        [Header("Animation")]
        public float animSpeed = 2f;

        private float displayedHealth = 50f;
        private float trailHealth = 50f;
        private float targetHealth = 50f;
        private RectTransform fillRT;
        private RectTransform trailRT;
        private float barWidth;

        void Start()
        {
            if (gradientFill != null)
                fillRT = gradientFill.GetComponent<RectTransform>();
            if (animatedTrail != null)
                trailRT = animatedTrail.GetComponent<RectTransform>();
            if (fillRT != null)
                barWidth = fillRT.transform.parent.GetComponent<RectTransform>().sizeDelta.x;

            if (GameManager.Instance != null)
                GameManager.Instance.OnHumanHealthChanged += OnHealthChanged;

            UpdateDisplay(50f);
        }

        void Update()
        {
            // Animate fill toward target
            if (Mathf.Abs(displayedHealth - targetHealth) > 0.1f)
            {
                displayedHealth = Mathf.MoveTowards(displayedHealth, targetHealth, animSpeed * Time.deltaTime * 30f);
                UpdateFillPosition(displayedHealth);
            }

            // Trail follows slower — shows the "impact" of the shift
            if (Mathf.Abs(trailHealth - targetHealth) > 0.1f)
            {
                trailHealth = Mathf.MoveTowards(trailHealth, targetHealth, animSpeed * Time.deltaTime * 10f);
                UpdateTrailPosition(trailHealth);
            }
        }

        private void OnHealthChanged(float health)
        {
            targetHealth = health;
            UpdateLabels(health);
        }

        private void UpdateDisplay(float health)
        {
            targetHealth = health;
            displayedHealth = health;
            trailHealth = health;
            UpdateFillPosition(health);
            UpdateTrailPosition(health);
            UpdateLabels(health);
        }

        private void UpdateFillPosition(float health)
        {
            if (fillRT == null) return;
            float pct = Mathf.Clamp01(health / 100f);
            fillRT.anchorMax = new Vector2(pct, 1f);
        }

        private void UpdateTrailPosition(float health)
        {
            if (trailRT == null) return;
            float pct = Mathf.Clamp01(health / 100f);
            // Trail shows between displayed and target
            float displayPct = Mathf.Clamp01(displayedHealth / 100f);
            float trailPct = Mathf.Clamp01(trailHealth / 100f);

            if (trailPct > displayPct)
            {
                // Health decreased — trail shows the lost amount in red
                trailRT.anchorMin = new Vector2(displayPct, 0f);
                trailRT.anchorMax = new Vector2(trailPct, 1f);
                trailRT.GetComponent<Image>().color = new Color(1f, 0.2f, 0.1f, 0.6f);
            }
            else
            {
                // Health increased — trail shows the gained amount in green
                trailRT.anchorMin = new Vector2(trailPct, 0f);
                trailRT.anchorMax = new Vector2(displayPct, 1f);
                trailRT.GetComponent<Image>().color = new Color(0.2f, 1f, 0.3f, 0.6f);
            }
        }

        private void UpdateLabels(float health)
        {
            if (percentText != null)
                percentText.text = $"{Mathf.RoundToInt(health)}%";

            if (conditionText != null)
            {
                var condition = GameManager.Instance != null
                    ? GameManager.Instance.CurrentCondition : BodyCondition.Normal;

                switch (condition)
                {
                    case BodyCondition.Critical:
                        conditionText.text = "CRITICAL \u2014 ORGAN FAILURE";
                        conditionText.color = new Color(1f, 0.2f, 0.2f);
                        break;
                    case BodyCondition.Sick:
                        conditionText.text = "WORSENING \u2014 SPREADING SYMPTOMS";
                        conditionText.color = new Color(1f, 0.6f, 0.2f);
                        break;
                    case BodyCondition.Normal:
                        conditionText.text = "STABLE \u2014 MILD SYMPTOMS";
                        conditionText.color = new Color(1f, 0.9f, 0.3f);
                        break;
                    case BodyCondition.Recovering:
                        conditionText.text = "IMPROVING \u2014 RECOVERY PHASE";
                        conditionText.color = new Color(0.5f, 1f, 0.4f);
                        break;
                    case BodyCondition.Healthy:
                        conditionText.text = "STRONG \u2014 NEAR FULL HEALTH";
                        conditionText.color = new Color(0.3f, 1f, 0.3f);
                        break;
                }
            }
        }

        /// <summary>
        /// Creates the gradient texture: red → yellow → light green → green.
        /// </summary>
        public static Texture2D CreateGradientTexture(int width)
        {
            var tex = new Texture2D(width, 1, TextureFormat.RGBA32, false);
            for (int x = 0; x < width; x++)
            {
                float t = (float)x / (width - 1);
                Color color;
                if (t < 0.33f)
                    color = Color.Lerp(new Color(0.9f, 0.1f, 0.1f), new Color(1f, 0.85f, 0.1f), t / 0.33f);
                else if (t < 0.66f)
                    color = Color.Lerp(new Color(1f, 0.85f, 0.1f), new Color(0.5f, 0.9f, 0.2f), (t - 0.33f) / 0.33f);
                else
                    color = Color.Lerp(new Color(0.5f, 0.9f, 0.2f), new Color(0.2f, 0.85f, 0.2f), (t - 0.66f) / 0.34f);
                tex.SetPixel(x, 0, color);
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }
    }
}
