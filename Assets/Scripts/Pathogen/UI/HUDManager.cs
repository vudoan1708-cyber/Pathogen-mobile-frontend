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
