using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

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
        public TextMeshProUGUI bioCurrencyText;
        public Button[] skillButtons;
        public TextMeshProUGUI[] skillCooldownTexts;
        public SkillLevelUpUI skillLevelUpUI;
        public Button shopButton;
        public GameObject shopPanel;
        public TextMeshProUGUI gameOverText;
        public GameObject gameOverPanel;
        public TextMeshProUGUI respawnCountdownText;
        public GameObject respawnPanel;
        public Button recallButton;
        public GameObject recallProgressBar;
        public Image recallProgressFill;
        public TextMeshProUGUI recallProgressText;

        // Skill button colors
        private readonly Color readyColor = new Color(0.3f, 0.8f, 0.3f);
        private readonly Color cooldownColor = new Color(0.4f, 0.4f, 0.4f);
        private readonly Color lockedColor = new Color(0.2f, 0.2f, 0.2f);
        private readonly Color noManaColor = new Color(0.35f, 0.35f, 0.35f);
        private readonly Color noManaTextColor = new Color(0.55f, 0.55f, 0.55f);

        private CameraController cameraController;

        void Start()
        {
            cameraController = FindAnyObjectByType<CameraController>();

            if (playerChampion != null)
            {
                playerChampion.OnBioCurrencyChanged += UpdateBioCurrency;
                playerChampion.OnRespawnTimerChanged += ShowRespawnCountdown;
                playerChampion.OnRespawn += OnPlayerRespawn;
                playerChampion.OnRecallStarted += ShowRecallProgress;
                playerChampion.OnRecallProgress += UpdateRecallProgress;
                playerChampion.OnRecallCompleted += HideRecallProgress;
                playerChampion.OnRecallCancelled += HideRecallProgress;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver += ShowGameOver;
            }

            // Skill buttons
            // Skill button interactions are handled by SkillButton component (tap, hold+drag)
            // which works for mouse, touch, and any input device

            // Shop button
            if (shopButton != null)
                shopButton.onClick.AddListener(ToggleShop);
            if (recallButton != null)
                recallButton.onClick.AddListener(OnRecallButtonPressed);

            if (shopPanel != null)
                shopPanel.SetActive(false);
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            if (respawnPanel != null)
                respawnPanel.SetActive(false);
            if (recallProgressBar != null)
                recallProgressBar.SetActive(false);

            UpdateAll();
        }

        void Update()
        {
            UpdateSkillButtons();

            var kb = Keyboard.current;
            if (kb != null && kb.bKey.wasPressedThisFrame)
                OnRecallButtonPressed();
        }

        private void UpdateAll()
        {
            if (playerChampion == null) return;
            UpdateBioCurrency(playerChampion.bioCurrency);
        }

        private void UpdateBioCurrency(float amount)
        {
            if (bioCurrencyText != null)
                bioCurrencyText.text = Mathf.FloorToInt(amount).ToString();
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
                else if (playerChampion.currentMana < skill.definition.manaCost)
                {
                    img.color = noManaColor;
                    if (skillCooldownTexts != null && i < skillCooldownTexts.Length)
                    {
                        skillCooldownTexts[i].text = skill.definition.skillName;
                        skillCooldownTexts[i].color = noManaTextColor;
                    }
                }
                else
                {
                    img.color = readyColor;
                    if (skillCooldownTexts != null && i < skillCooldownTexts.Length)
                    {
                        skillCooldownTexts[i].text = skill.definition.skillName;
                        skillCooldownTexts[i].color = Color.white;
                    }
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

            if (cameraController != null)
                cameraController.Recenter();
        }

        // ─── RECALL ─────────────────────────────────────────────────────

        private void OnRecallButtonPressed()
        {
            if (playerChampion == null || playerChampion.IsDead || playerChampion.isRespawning) return;

            if (playerChampion.isRecalling)
                playerChampion.CancelRecall();
            else
                playerChampion.StartRecall();
        }

        private void ShowRecallProgress()
        {
            if (recallProgressBar != null)
                recallProgressBar.SetActive(true);
            if (recallProgressFill != null)
                recallProgressFill.fillAmount = 0f;
            if (recallProgressText != null)
                recallProgressText.text = "RECALLING...";
        }

        private void UpdateRecallProgress(float normalizedProgress)
        {
            if (recallProgressFill != null)
                recallProgressFill.fillAmount = normalizedProgress;

            float remaining = Champion.RecallDuration * (1f - normalizedProgress);
            if (recallProgressText != null)
                recallProgressText.text = $"RECALLING... {remaining:F1}s";
        }

        private void HideRecallProgress()
        {
            if (recallProgressBar != null)
                recallProgressBar.SetActive(false);
        }
    }
}
