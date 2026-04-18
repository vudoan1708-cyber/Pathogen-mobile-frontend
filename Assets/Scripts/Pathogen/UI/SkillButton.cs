using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

namespace Pathogen
{
    /// <summary>
    /// Mobile: mini joystick — drag offset from button center maps to skill aim direction.
    ///   Quick tap = smart target. Hold+drag = aim, release fires. Beyond boundary or X = cancel.
    /// Desktop: click only enters aim mode. Drag and release do nothing — second click fires via PlayerController.
    ///
    /// Also owns the "+" upgrade button and flash overlay for this skill slot.
    /// </summary>
    public class SkillButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public int skillIndex;
        public PlayerController playerController;
        public float cancelBoundary = 350f;

        [Header("Level-Up UI")]
        public GameObject upgradeButton;    // The gold "+" overlay (child of this GO)
        public Image flashOverlay;          // White flash overlay (child of this GO)
        public Image cooldownOverlay;       // Radial sweep overlay (shader-driven)
        public Material cooldownMaterial;   // Per-button material instance for _Progress

        private const float DragThresholdSq = 100f; // 10px radius — matches AutoAttackButton

        private bool isPressed;
        private bool hasDragged;
        private Vector2 pressPosition;
        private Vector3 lastAimDirection;
        private bool IsMobile => GameBootstrap.IsMobile;

        // Cached upgrade button refs
        private RectTransform upgradeBtnRT;
        private CanvasGroup upgradeBtnCG;

        /// <summary>
        /// Init in Start (not Awake) because GameBootstrap assigns upgradeButton
        /// and flashOverlay after AddComponent — they're null at Awake time.
        /// </summary>
        void Start()
        {
            if (upgradeButton != null)
            {
                upgradeBtnRT = upgradeButton.GetComponent<RectTransform>();
                upgradeBtnCG = upgradeButton.GetComponent<CanvasGroup>();

                var btn = upgradeButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(OnUpgradeClicked);

                    // Disable navigation so EventSystem doesn't auto-select neighbours
                    btn.navigation = new Navigation { mode = Navigation.Mode.None };
                }

                upgradeButton.SetActive(false);
            }

            if (flashOverlay != null)
                flashOverlay.color = Color.clear;

            CreateCooldownOverlay();
        }

        // ─── COOLDOWN OVERLAY ───────────────────────────────────────────

        private void CreateCooldownOverlay()
        {
            var cdShader = ShaderLibrary.Instance.uiSkillCooldown;
            if (cdShader == null) return;

            cooldownMaterial = new Material(cdShader);
            cooldownMaterial.SetFloat("_Progress", 1f);

            var rt = GetComponent<RectTransform>();
            float size = rt != null ? rt.sizeDelta.x : 100f;

            var cdGO = new GameObject("CooldownOverlay", typeof(RectTransform));
            cdGO.transform.SetParent(transform, false);
            var cdRT = cdGO.GetComponent<RectTransform>();
            cdRT.anchorMin = new Vector2(0.5f, 0.5f);
            cdRT.anchorMax = new Vector2(0.5f, 0.5f);
            cdRT.pivot = new Vector2(0.5f, 0.5f);
            cdRT.anchoredPosition = Vector2.zero;
            cdRT.sizeDelta = new Vector2(size, size);

            cooldownOverlay = cdGO.AddComponent<Image>();
            cooldownOverlay.sprite = GetComponent<Image>()?.sprite; // reuse circle sprite from skill button
            cooldownOverlay.material = cooldownMaterial;
            cooldownOverlay.color = Color.white;
            cooldownOverlay.raycastTarget = false;
        }

        // ─── SKILL AIM INPUT ────────────────────────────────────────────

        void Update()
        {
            UpdateCooldownOverlay();

            if (!isPressed || !hasDragged || !IsMobile) return;
            playerController.OnMobileSkillAimUpdate(lastAimDirection);
        }

        private void UpdateCooldownOverlay()
        {
            if (cooldownMaterial == null || playerController == null) return;
            var champion = playerController.champion;
            if (champion == null || skillIndex >= champion.skills.Length) return;

            var skill = champion.skills[skillIndex];
            if (skill == null || !skill.IsUnlocked || skill.currentCooldown <= 0f)
            {
                cooldownMaterial.SetFloat("_Progress", 1f);
                return;
            }

            float total = skill.GetCooldown(champion.cooldownReduction);
            float progress = 1f - Mathf.Clamp01(skill.currentCooldown / total);
            cooldownMaterial.SetFloat("_Progress", progress);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (playerController == null || playerController.champion == null) return;
            if (playerController.champion.IsDead) return;

            isPressed = true;
            hasDragged = false;
            pressPosition = eventData.position;
            AutoAttackButton.ClearActive();
            playerController.StartAiming(skillIndex, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isPressed || !IsMobile) return;

            Vector2 offset = eventData.position - pressPosition;
            if (offset.sqrMagnitude > DragThresholdSq)
            {
                hasDragged = true;
                // Preserve aim ratio (0–1) as magnitude so AOE skills can control distance
                float ratio = Mathf.Clamp01(offset.magnitude / cancelBoundary);
                lastAimDirection = new Vector3(offset.x, 0f, offset.y).normalized * ratio;
                playerController.OnMobileSkillAimUpdate(lastAimDirection);
            }

            bool overCancel = playerController.IsTouchOverCancelButton(eventData.position);
            playerController.SetAimCancelTint(overCancel);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            isPressed = false;

            if (!IsMobile) return;

            if (playerController.IsTouchOverCancelButton(eventData.position))
            {
                playerController.CancelAiming();
                return;
            }

            Vector2 offset = eventData.position - pressPosition;
            if (offset.sqrMagnitude > DragThresholdSq)
            {
                float ratio = Mathf.Clamp01(offset.magnitude / cancelBoundary);
                Vector3 aimDirection = new Vector3(offset.x, 0f, offset.y).normalized * ratio;
                playerController.OnMobileSkillAimRelease(aimDirection);
            }
            else
            {
                playerController.OnMobileSkillAimRelease(Vector3.zero);
            }
        }

        // ─── UPGRADE BUTTON ─────────────────────────────────────────────

        /// <summary>Show the "+" button with a pop-in overshoot animation.</summary>
        public void ShowUpgrade()
        {
            if (upgradeButton == null) return;
            upgradeButton.SetActive(true);
            StartCoroutine(PopInUpgrade());
        }

        /// <summary>Immediately hide the "+" button.</summary>
        public void HideUpgrade()
        {
            if (upgradeButton == null) return;
            upgradeButton.SetActive(false);
        }

        private IEnumerator PopInUpgrade()
        {
            if (upgradeBtnRT == null) yield break;

            float elapsed = 0f;
            const float duration = 0.22f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Overshoot: 0 → 1.15 → 1
                float scale = t < 0.65f
                    ? Mathf.Lerp(0f, 1.15f, t / 0.65f)
                    : Mathf.Lerp(1.15f, 1f, (t - 0.65f) / 0.35f);

                upgradeBtnRT.localScale = Vector3.one * scale;
                if (upgradeBtnCG != null)
                    upgradeBtnCG.alpha = Mathf.Clamp01(t * 3f);

                yield return null;
            }

            upgradeBtnRT.localScale = Vector3.one;
            if (upgradeBtnCG != null) upgradeBtnCG.alpha = 1f;
        }

        private void OnUpgradeClicked()
        {
            var champion = playerController != null ? playerController.champion : null;
            if (champion == null) return;
            if (!champion.AllocateSkillPoint(skillIndex)) return;

            StartCoroutine(FlashOnUpgrade());
            // SkillLevelUpUI.Refresh() is driven by Champion.OnSkillPointsChanged
        }

        /// <summary>White flash + scale punch on the skill button after allocating a point.</summary>
        private IEnumerator FlashOnUpgrade()
        {
            if (flashOverlay == null) yield break;

            var rt = GetComponent<RectTransform>();
            Vector3 origScale = rt.localScale;

            float elapsed = 0f;
            const float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // White flash fading out
                flashOverlay.color = new Color(1f, 1f, 1f, (1f - t) * 0.7f);

                // Scale punch: smooth sine bump 1 → 1.15 → 1
                float punch = 1f + Mathf.Sin(t * Mathf.PI) * 0.15f;
                rt.localScale = origScale * punch;

                yield return null;
            }

            flashOverlay.color = Color.clear;
            rt.localScale = origScale;
        }
    }
}
