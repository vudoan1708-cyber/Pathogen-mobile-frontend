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
        public Image healthBarFill;
        public Image manaBarFill;
        public Text levelText;
        public Text bioCurrencyText;
        public Text humanHealthText;
        public Image humanHealthFill;
        public Button[] skillButtons;
        public Text[] skillCooldownTexts;
        public Button shopButton;
        public GameObject shopPanel;
        public Text gameOverText;
        public GameObject gameOverPanel;
        public Button cameraModeButton;

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
                playerChampion.OnHealthChanged += UpdateHealthBar;
                playerChampion.OnManaChanged += UpdateManaBar;
                playerChampion.OnLevelUp += UpdateLevel;
                playerChampion.OnBioCurrencyChanged += UpdateBioCurrency;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHumanHealthChanged += UpdateHumanHealth;
                GameManager.Instance.OnGameOver += ShowGameOver;
            }

            // Skill buttons
            if (skillButtons != null)
            {
                for (int i = 0; i < skillButtons.Length; i++)
                {
                    int index = i;
                    skillButtons[i].onClick.AddListener(() => OnSkillPressed(index));
                }
            }

            // Shop button
            if (shopButton != null)
                shopButton.onClick.AddListener(ToggleShop);

            // Camera mode button
            if (cameraModeButton != null)
                cameraModeButton.onClick.AddListener(CycleCameraMode);

            if (shopPanel != null)
                shopPanel.SetActive(false);
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            // Initial UI update
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
                if (kb.cKey.wasPressedThisFrame)
                    CycleCameraMode();
            }
#endif
        }

        private void UpdateAll()
        {
            if (playerChampion == null) return;
            UpdateHealthBar(playerChampion.currentHealth, playerChampion.maxHealth);
            UpdateManaBar(playerChampion.currentMana, playerChampion.maxMana);
            UpdateLevel(playerChampion.level);
            UpdateBioCurrency(playerChampion.bioCurrency);
            if (GameManager.Instance != null)
                UpdateHumanHealth(GameManager.Instance.HumanHealth);
        }

        private void UpdateHealthBar(float current, float max)
        {
            if (healthBarFill != null)
                healthBarFill.fillAmount = current / max;
        }

        private void UpdateManaBar(float current, float max)
        {
            if (manaBarFill != null)
                manaBarFill.fillAmount = current / max;
        }

        private void UpdateLevel(int level)
        {
            if (levelText != null)
                levelText.text = "Lv." + level;
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

        private void OnSkillPressed(int index)
        {
            var pc = playerChampion.GetComponent<PlayerController>();
            if (pc != null)
                pc.OnSkillButtonPressed(index);
        }

        private void ToggleShop()
        {
            if (shopPanel != null)
                shopPanel.SetActive(!shopPanel.activeSelf);
        }

        private void CycleCameraMode()
        {
            if (cameraController != null)
                cameraController.CycleMode();
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
    }
}
