using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pathogen
{
    public class ChampionStats : MonoBehaviour
    {
        public Champion champion;
        public TextMeshProUGUI levelText;
        public Image xpRing;

        // Shader-driven materials (created by factory)
        public Material healthMaterial;
        public Material manaMaterial;

        private float healthTarget;
        private float lastHealthPct;
        private float trailAlpha;
        private float trailDelayTimer;
        private float xpDisplayed;

        private const float TrailFadeSpeed = 4f;    // 0.25s
        private const float TrailDelay = 0.12f;
        private const float XpFillSpeed = 3f;
        private const float HpPerTick = 100f;

        void Start()
        {
            if (champion == null) return;

            float hp = champion.currentHealth / champion.maxHealth;
            healthTarget = hp;
            lastHealthPct = hp;

            if (xpRing != null)
                xpRing.fillAmount = champion.currentXP / champion.xpToNextLevel;

            champion.OnHealthChanged += OnHealthChanged;
            champion.OnManaChanged += OnManaChanged;
            champion.OnLevelUp += OnLevelUp;
            champion.OnDeath += _ => SetVisible(false);
            champion.OnRespawn += () => SetVisible(true);
        }

        void Update()
        {
            // Damage/heal trail fade
            if (healthMaterial != null && trailAlpha > 0f)
            {
                if (trailDelayTimer > 0f)
                    trailDelayTimer -= Time.deltaTime;
                else
                {
                    trailAlpha -= TrailFadeSpeed * Time.deltaTime;
                    if (trailAlpha <= 0f) trailAlpha = 0f;
                }
                healthMaterial.SetFloat("_TrailAlpha", trailAlpha);
            }

            // XP ring
            if (xpRing != null && champion != null)
            {
                float xpTarget = champion.currentXP / champion.xpToNextLevel;
                xpDisplayed = Mathf.MoveTowards(xpDisplayed, xpTarget, XpFillSpeed * Time.deltaTime);
                xpRing.fillAmount = xpDisplayed;
            }
        }

        void LateUpdate()
        {
            if (Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;
        }

        private void OnHealthChanged(float current, float max)
        {
            if (healthMaterial == null) return;

            float pct = max > 0 ? Mathf.Clamp01(current / max) : 0f;
            float delta = pct - lastHealthPct;

            if (Mathf.Abs(delta) > 0.001f)
            {
                healthMaterial.SetFloat("_TrailPct", lastHealthPct);
                healthMaterial.SetFloat("_TrailSign", delta < 0 ? -1f : 1f);
                trailAlpha = 1f;
                trailDelayTimer = TrailDelay;
            }

            lastHealthPct = pct;
            healthMaterial.SetFloat("_FillPct", pct);

            // Update tick spacing when max HP changes (level-ups)
            float tickUV = max > 0 ? HpPerTick / max : 0.1f;
            healthMaterial.SetFloat("_TickSpacing", tickUV);
        }

        private void OnManaChanged(float current, float max)
        {
            if (manaMaterial == null) return;
            float pct = max > 0 ? Mathf.Clamp01(current / max) : 0f;
            manaMaterial.SetFloat("_FillPct", pct);
        }

        private void OnLevelUp(int lvl)
        {
            if (levelText != null) levelText.text = lvl.ToString();
            xpDisplayed = 0f;
        }

        private void SetVisible(bool visible)
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(visible);
        }

        // ─── FACTORY ────────────────────────────────────────────────────

        public static ChampionStats Create(Transform champTransform, Champion champ, float champHeight)
        {
            var container = new GameObject("ChampionStats");
            container.transform.SetParent(champTransform, false);
            container.transform.localPosition = new Vector3(0f, champHeight + 1.2f, 0f);

            var canvas = container.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 50;
            var rt = container.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1.5f, 0.8f);
            rt.localScale = Vector3.one * 0.015f;

            var cg = container.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Layout constants
            float lvlSize = 45f;
            float barWidth = 90f;
            float groupGap = 6f;
            float groupWidth = lvlSize + groupGap + barWidth;
            float groupStartX = -groupWidth * 0.5f;
            float healthBarHeight = 17f;
            float manaBarHeight = 17f;
            float barGap = 1f;
            float barsTopY = (healthBarHeight + barGap + manaBarHeight) * 0.5f;
            float barsLeftX = groupStartX + lvlSize + groupGap;

            // ── Level circle + XP ring ──
            var lvlContainer = CreateImg(container.transform, "LevelContainer",
                new Vector2(0.5f, 0.5f), new Vector2(groupStartX + lvlSize * 0.5f, 0f),
                new Vector2(lvlSize, lvlSize), new Color(0.1f, 0.1f, 0.15f, 0.8f));
            lvlContainer.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            var circleSprite = GameBootstrap.CreateCircleSprite(256);
            lvlContainer.GetComponent<Image>().sprite = circleSprite;

            CreateXPRingBackground(lvlContainer.transform, lvlSize);
            var xpRingImg = CreateXPRingFill(lvlContainer.transform, lvlSize);

            var lvlText = CreateLabel(lvlContainer.transform, "LevelText",
                new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(lvlSize, lvlSize), "1", 20, Color.white);
            lvlText.fontStyle = FontStyles.Bold;

            // ── Health bar (shader-driven) ──
            var barShader = Shader.Find("Pathogen/UIEntityBarCanvas");

            var hpMat = new Material(barShader);
            Color hpColor = champ.team == Team.Virus
                ? new Color(0.85f, 0.18f, 0.18f)
                : new Color(0.2f, 0.72f, 0.3f);
            hpMat.SetColor("_FillColor", hpColor);
            hpMat.SetFloat("_FillPct", 1f);
            hpMat.SetFloat("_TrailPct", 1f);
            hpMat.SetFloat("_TrailAlpha", 0f);
            float tickUV = champ.maxHealth > 0 ? HpPerTick / champ.maxHealth : 0.1f;
            hpMat.SetFloat("_TickSpacing", tickUV);

            var healthBarGO = CreateImg(container.transform, "HealthBar",
                new Vector2(0.5f, 0.5f), new Vector2(barsLeftX, barsTopY),
                new Vector2(barWidth, healthBarHeight), Color.white);
            healthBarGO.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
            healthBarGO.GetComponent<Image>().material = hpMat;

            // ── Mana bar (shader-driven, no ticks/trail) ──
            var manaMat = new Material(barShader);
            manaMat.SetColor("_FillColor", new Color(0.25f, 0.4f, 0.9f));
            manaMat.SetColor("_BGColor", new Color(0.1f, 0.1f, 0.18f, 0.85f));
            manaMat.SetFloat("_FillPct", 1f);
            manaMat.SetFloat("_TrailAlpha", 0f);
            manaMat.SetFloat("_TickSpacing", 0f);

            var manaBarGO = CreateImg(container.transform, "ManaBar",
                new Vector2(0.5f, 0.5f),
                new Vector2(barsLeftX, barsTopY - healthBarHeight - barGap),
                new Vector2(barWidth, manaBarHeight), Color.white);
            manaBarGO.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
            manaBarGO.GetComponent<Image>().material = manaMat;

            // ── Wire component ──
            var stats = container.AddComponent<ChampionStats>();
            stats.champion = champ;
            stats.healthMaterial = hpMat;
            stats.manaMaterial = manaMat;
            stats.levelText = lvlText;
            stats.xpRing = xpRingImg;

            return stats;
        }

        // ─── HELPERS ────────────────────────────────────────────────────

        private static GameObject CreateImg(Transform parent, string name,
            Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor;
            r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            go.AddComponent<Image>().color = color;
            return go;
        }

        private static Sprite cachedRingSprite;

        private static Sprite GetOrCreateRingSprite()
        {
            if (cachedRingSprite != null) return cachedRingSprite;

            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float outerRadius = center - 1f;
            float innerRadius = outerRadius - 24f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float outerAlpha = Mathf.Clamp01(outerRadius - dist + 1f);
                    float innerAlpha = Mathf.Clamp01(dist - innerRadius + 1f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Min(outerAlpha, innerAlpha)));
                }
            }

            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            cachedRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return cachedRingSprite;
        }

        private static void CreateXPRingBackground(Transform parent, float lvlSize)
        {
            var ringSprite = GetOrCreateRingSprite();
            var go = CreateImg(parent, "XPRingBG",
                new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(lvlSize + 8f, lvlSize + 8f), new Color(0.35f, 0.35f, 0.4f, 0.5f));
            go.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            var img = go.GetComponent<Image>();
            img.sprite = ringSprite;
            img.raycastTarget = false;
        }

        private static Image CreateXPRingFill(Transform parent, float lvlSize)
        {
            var ringSprite = GetOrCreateRingSprite();
            var go = CreateImg(parent, "XPRing",
                new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(lvlSize + 8f, lvlSize + 8f), new Color(0.3f, 0.8f, 1f, 0.9f));
            go.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            var img = go.GetComponent<Image>();
            img.sprite = ringSprite;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillOrigin = (int)Image.Origin360.Top;
            img.fillAmount = 0f;
            img.raycastTarget = false;
            return img;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name,
            Vector2 anchor, Vector2 pos, Vector2 size,
            string text, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchor;
            r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = pos;
            r.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = GameBootstrap.UIFont;
            return tmp;
        }
    }
}
