using UnityEngine;
using UnityEngine.UI;

namespace Pathogen
{
    public class ChampionStats : MonoBehaviour
    {
        public Champion champion;
        public RectTransform healthFill;
        public RectTransform healthTrail;
        public RectTransform manaFill;
        public RectTransform manaTrail;
        public Text levelText;
        public Image xpRing;

        private float healthTarget;
        private float healthDisplayed;
        private float healthTrailValue;

        private float manaTarget;
        private float manaDisplayed;
        private float manaTrailValue;

        private float xpDisplayed;
        private const float FillSpeed = 60f;
        private const float TrailSpeed = 20f;
        private const float XpFillSpeed = 3f;

        void Start()
        {
            if (champion == null) return;

            float hp = champion.currentHealth / champion.maxHealth;
            healthTarget = hp;
            healthDisplayed = hp;
            healthTrailValue = hp;
            SetFill(healthFill, hp);
            HideTrail(healthTrail);

            float mp = champion.currentMana / champion.maxMana;
            manaTarget = mp;
            manaDisplayed = mp;
            manaTrailValue = mp;
            SetFill(manaFill, mp);
            HideTrail(manaTrail);

            if (levelText != null)
                levelText.text = champion.level.ToString();

            if (xpRing != null)
                xpRing.fillAmount = champion.currentXP / champion.xpToNextLevel;

            champion.OnHealthChanged += (cur, max) => healthTarget = cur / max;
            champion.OnManaChanged += (cur, max) => manaTarget = cur / max;
            champion.OnLevelUp += (lvl) =>
            {
                if (levelText != null) levelText.text = lvl.ToString();
                xpDisplayed = 0f;
            };
            champion.OnDeath += (_) => SetVisible(false);
            champion.OnRespawn += () => SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(visible);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            AnimateBar(ref healthDisplayed, ref healthTrailValue, healthTarget, healthFill, healthTrail, dt);
            AnimateBar(ref manaDisplayed, ref manaTrailValue, manaTarget, manaFill, manaTrail, dt);

            if (xpRing != null && champion != null)
            {
                float xpTarget = champion.currentXP / champion.xpToNextLevel;
                xpDisplayed = Mathf.MoveTowards(xpDisplayed, xpTarget, XpFillSpeed * dt);
                xpRing.fillAmount = xpDisplayed;
            }
        }

        void LateUpdate()
        {
            if (Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;
        }

        private void AnimateBar(ref float displayed, ref float trail, float target,
            RectTransform fill, RectTransform trailRT, float dt)
        {
            if (fill == null) return;

            if (Mathf.Abs(displayed - target) > 0.001f)
            {
                displayed = Mathf.MoveTowards(displayed, target, FillSpeed * dt);
                SetFill(fill, displayed);
            }

            if (trailRT == null) return;

            if (Mathf.Abs(trail - target) > 0.001f)
            {
                trail = Mathf.MoveTowards(trail, target, TrailSpeed * dt);
                UpdateTrail(trailRT, displayed, trail);
            }
            else
            {
                HideTrail(trailRT);
            }
        }

        private void SetFill(RectTransform rt, float pct)
        {
            if (rt == null) return;
            pct = Mathf.Clamp01(pct);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(pct, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void UpdateTrail(RectTransform rt, float displayed, float trail)
        {
            if (rt == null) return;

            float lo = Mathf.Min(displayed, trail);
            float hi = Mathf.Max(displayed, trail);

            rt.anchorMin = new Vector2(Mathf.Clamp01(lo), 0f);
            rt.anchorMax = new Vector2(Mathf.Clamp01(hi), 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = rt.GetComponent<Image>();
            if (img != null)
                img.color = trail > displayed
                    ? new Color(1f, 0.2f, 0.1f, 0.6f)
                    : new Color(0.2f, 1f, 0.3f, 0.6f);
        }

        private void HideTrail(RectTransform rt)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
