using UnityEngine;

namespace Pathogen
{
    public class DamageFlash : MonoBehaviour
    {
        private Renderer[] renderers;
        private Color[] originalColors;
        private float flashTimer;
        private const float FlashDuration = 0.1f;
        private static readonly Color flashColor = Color.white;

        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                originalColors[i] = renderers[i].material.color;
        }

        public void Flash()
        {
            flashTimer = FlashDuration;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].material.color = flashColor;
            }
        }

        void Update()
        {
            if (flashTimer <= 0f) return;

            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
                RestoreColors();
        }

        private void RestoreColors()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].material.color = originalColors[i];
            }
        }

        public void RefreshOriginalColors()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                originalColors[i] = renderers[i].material.color;
        }
    }
}
