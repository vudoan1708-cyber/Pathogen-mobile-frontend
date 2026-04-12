using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Pathogen
{
    /// <summary>
    /// Main HUD for the player: health/mana bars, bio-currency, level,
    /// skill buttons, human health display, and shop button.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("References")]
        public Champion playerChampion;

        [Header("UI Elements")]
        public Text bioCurrencyText;
        public Text humanHealthText;
        public Image humanHealthFill;
        public Button[] skillButtons;
        public Text[] skillCooldownTexts;
        public Button shopButton;
        public GameObject shopPanel;
        public Text gameOverText;
        public GameObject gameOverPanel;
        public Text respawnCountdownText;
        public GameObject respawnPanel;

        // Skill button colors
        private readonly Color readyColor = new Color(0.3f, 0.8f, 0.3f);
        private readonly Color cooldownColor = new Color(0.4f, 0.4f, 0.4f);
        private readonly Color lockedColor = new Color(0.2f, 0.2f, 0.2f);

        private CameraController cameraController;

        void Start()
        {
            cameraController = FindAnyObjectByType<CameraController>();

            if (playerChampion != null)
            {
                playerChampion.OnBioCurrencyChanged += UpdateBioCurrency;
                playerChampion.OnRespawnTimerChanged += ShowRespawnCountdown;
                playerChampion.OnRespawn += OnPlayerRespawn;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHumanHealthChanged += UpdateHumanHealth;
                GameManager.Instance.OnGameOver += ShowGameOver;
            }

            // Skill buttons
            // Skill button interactions are handled by SkillButton component (tap, hold+drag)
            // which works for mouse, touch, and any input device

            // Shop button
            if (shopButton != null)
                shopButton.onClick.AddListener(ToggleShop);

            if (shopPanel != null)
                shopPanel.SetActive(false);
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            if (respawnPanel != null)
                respawnPanel.SetActive(false);

            UpdateAll();
        }

        void Update()
        {
            UpdateSkillButtons();

#if UNITY_EDITOR
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.bKey.wasPressedThisFrame)
                    ToggleShop();
            }
#endif
        }

        private void UpdateAll()
        {
            if (playerChampion == null) return;
            UpdateBioCurrency(playerChampion.bioCurrency);
            if (GameManager.Instance != null)
                UpdateHumanHealth(GameManager.Instance.HumanHealth);
        }

        private void UpdateBioCurrency(float amount)
        {
            if (bioCurrencyText != null)
                bioCurrencyText.text = Mathf.FloorToInt(amount).ToString();
        }

        private void UpdateHumanHealth(float health)
        {
            if (humanHealthText != null)
            {
                string condition = GameManager.Instance != null
                    ? GameManager.Instance.CurrentCondition.ToString().ToUpper()
                    : "NORMAL";
                humanHealthText.text = $"HOST: {health:F0}% — {condition}";
            }

            if (humanHealthFill != null)
            {
                humanHealthFill.fillAmount = health / 100f;
                // Color gradient: red(0%) -> yellow(50%) -> green(100%)
                humanHealthFill.color = Color.Lerp(
                    Color.Lerp(Color.red, Color.yellow, health / 50f),
                    Color.Lerp(Color.yellow, Color.green, (health - 50f) / 50f),
                    health > 50f ? 1f : 0f);
            }
        }

        private void UpdateSkillButtons()
        {
            if (playerChampion == null || skillButtons == null) return;

            for (int i = 0; i < skillButtons.Length && i < playerChampion.skills.Length; i++)
            {
                var skill = playerChampion.skills[i];
                if (skill == null) continue;

                var img = skillButtons[i].GetComponent<Image>();

                if (!skill.IsUnlocked)
                {
                    img.color = lockedColor;
                    if (skillCooldownTexts != null && i < skillCooldownTexts.Length)
                        skillCooldownTexts[i].text = "LOCKED";
                }
                else if (skill.currentCooldown > 0f)
                {
                    img.color = cooldownColor;
                    if (skillCooldownTexts != null && i < skillCooldownTexts.Length)
                        skillCooldownTexts[i].text = Mathf.CeilToInt(skill.currentCooldown).ToString();
                }
                else
                {
                    img.color = readyColor;
                    if (skillCooldownTexts != null && i < skillCooldownTexts.Length)
                        skillCooldownTexts[i].text = skill.definition.skillName;
                }
            }
        }


        private void ToggleShop()
        {
            if (shopPanel != null)
                shopPanel.SetActive(!shopPanel.activeSelf);
        }

        private void ShowGameOver(Team winner)
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                if (winner == Team.Virus)
                    gameOverText.text = "HOST DECEASED\nVIRUS VICTORY";
                else
                    gameOverText.text = "FULL RECOVERY\nIMMUNE VICTORY";
            }
        }

        private void ShowRespawnCountdown(float remaining)
        {
            if (respawnPanel != null)
                respawnPanel.SetActive(true);
            if (respawnCountdownText != null)
                respawnCountdownText.text = $"RESPAWNING IN {Mathf.CeilToInt(remaining)}";
        }

        private void OnPlayerRespawn()
        {
            if (respawnPanel != null)
                respawnPanel.SetActive(false);

            // Camera auto-moves to base
            if (cameraController != null)
                cameraController.Recenter();
        }
    }
}
