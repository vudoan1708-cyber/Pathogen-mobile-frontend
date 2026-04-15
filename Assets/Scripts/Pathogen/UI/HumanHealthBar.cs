using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pathogen
{
    /// <summary>
    /// Human health bar with shader-driven gradient fill (red→yellow→green),
    /// animated transitions, and condition labels.
    /// Call HumanHealthBar.Create() to build the full UI hierarchy.
    /// </summary>
    public class HumanHealthBar : MonoBehaviour
    {
        [Header("References")]
        public Image gradientFill;
        public Image animatedTrail;
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

        // ─── LAYOUT CONSTANTS ───────────────────────────────────────────

        private const float ContainerWidth = 650f;
        private const float ContainerHeight = 140f;
        private const float BarAnchorMin = 0.38f;     // vertical anchor bottom
        private const float BarAnchorMax = 0.78f;     // vertical anchor top

        private const int LabelFontSize = 22;
        private const int ConditionFontSize = 31;
        private const int PercentFontSize = 24;
        private const int SubtitleFontSize = 20;

        // ─── LIFECYCLE ──────────────────────────────────────────────────

        void Start()
        {
            if (gradientFill != null)
                fillRT = gradientFill.GetComponent<RectTransform>();
            if (animatedTrail != null)
                trailRT = animatedTrail.GetComponent<RectTransform>();

            if (GameManager.Instance != null)
                GameManager.Instance.OnHumanHealthChanged += OnHealthChanged;

            UpdateDisplay(50f);
        }

        void Update()
        {
            if (Mathf.Abs(displayedHealth - targetHealth) > 0.1f)
            {
                displayedHealth = Mathf.MoveTowards(displayedHealth, targetHealth, animSpeed * Time.deltaTime * 30f);
                UpdateFillPosition(displayedHealth);
            }

            if (Mathf.Abs(trailHealth - targetHealth) > 0.1f)
            {
                trailHealth = Mathf.MoveTowards(trailHealth, targetHealth, animSpeed * Time.deltaTime * 10f);
                UpdateTrailPosition(trailHealth);
            }
        }

        // ─── HEALTH UPDATES ─────────────────────────────────────────────

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
            float displayPct = Mathf.Clamp01(displayedHealth / 100f);
            float trailPct = Mathf.Clamp01(trailHealth / 100f);

            if (trailPct > displayPct)
            {
                trailRT.anchorMin = new Vector2(displayPct, 0f);
                trailRT.anchorMax = new Vector2(trailPct, 1f);
                trailRT.GetComponent<Image>().color = new Color(1f, 0.2f, 0.1f, 0.6f);
            }
            else
            {
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

        // ─── FACTORY ────────────────────────────────────────────────────

        /// <summary>
        /// Builds the complete human health bar UI hierarchy under the given canvas
        /// and returns the configured component. All layout values live here.
        /// </summary>
        public static HumanHealthBar Create(Transform canvasTransform)
        {
            // Container
            var container = new GameObject("HumanHealthContainer", typeof(RectTransform));
            container.transform.SetParent(canvasTransform, false);
            var rt = container.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -10f);
            rt.sizeDelta = new Vector2(ContainerWidth, ContainerHeight);

            var cg = container.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Dark background bar
            var barBG = CreateRect(container.transform, "BarBG");
            var barBGRT = barBG.GetComponent<RectTransform>();
            barBGRT.anchorMin = new Vector2(0f, BarAnchorMin);
            barBGRT.anchorMax = new Vector2(1f, BarAnchorMax);
            barBGRT.offsetMin = Vector2.zero;
            barBGRT.offsetMax = Vector2.zero;
            barBG.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 0.9f);

            // Gradient fill — shader handles red→yellow→green + organic noise + pulse
            var gradientMat = new Material(Shader.Find("Pathogen/UIHealthGradient"));

            var fillGO = CreateRect(barBG.transform, "GradientFill");
            var fillRt = fillGO.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0.5f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.material = gradientMat;
            fillImg.color = Color.white;

            // Animated trail
            var trailGO = CreateRect(barBG.transform, "Trail");
            var trailRt = trailGO.GetComponent<RectTransform>();
            trailRt.anchorMin = new Vector2(0.5f, 0f);
            trailRt.anchorMax = new Vector2(0.5f, 1f);
            trailRt.offsetMin = Vector2.zero;
            trailRt.offsetMax = Vector2.zero;
            var trailImg = trailGO.AddComponent<Image>();
            trailImg.color = new Color(1f, 0.2f, 0.1f, 0.5f);

            // Labels
            var death = CreateLabel(container.transform, "DeathLabel",
                new Vector2(0f, 0.28f), new Vector2(60f, 0f), new Vector2(160f, 30f),
                "DEATH 0%", LabelFontSize, new Color(1f, 0.3f, 0.3f));

            var healthy = CreateLabel(container.transform, "HealthyLabel",
                new Vector2(1f, 0.28f), new Vector2(-70f, 0f), new Vector2(200f, 30f),
                "HEALTHY 100%", LabelFontSize, new Color(0.3f, 1f, 0.3f));

            var condition = CreateLabel(container.transform, "ConditionText",
                new Vector2(0.5f, 0f), new Vector2(0f, -9f), new Vector2(550f, 45f),
                "STABLE \u2014 MILD SYMPTOMS", ConditionFontSize, new Color(1f, 0.9f, 0.3f));
            condition.fontStyle = FontStyles.Bold;

            // Percent text on the bar itself — black outline so it stands out on bright colours
            var percent = CreateLabel(barBG.transform, "PercentText",
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 32f),
                "50%", PercentFontSize, Color.white);
            percent.fontStyle = FontStyles.Bold;
            var outline = percent.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Wire component
            var hhBar = container.AddComponent<HumanHealthBar>();
            hhBar.gradientFill = fillImg;
            hhBar.animatedTrail = trailImg;
            hhBar.percentText = percent;
            hhBar.conditionText = condition;
            hhBar.deathLabel = death;
            hhBar.healthyLabel = healthy;

            return hhBar;
        }

        // ─── HELPERS ────────────────────────────────────────────────────

        private static GameObject CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name,
            Vector2 anchor, Vector2 pos, Vector2 size,
            string text, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var labelRT = go.GetComponent<RectTransform>();
            labelRT.anchorMin = anchor;
            labelRT.anchorMax = anchor;
            labelRT.pivot = new Vector2(0.5f, 0.5f);
            labelRT.anchoredPosition = pos;
            labelRT.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = GameBootstrap.UIFont;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }
    }
}
