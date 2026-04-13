using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Pathogen
{
    public class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Vector3 originalScale;
        private Color originalColor;
        private Image image;
        private float bounceTimer;
        private bool isPressed;

        private static readonly Color pressedColor = new Color(0.3f, 0.5f, 0.8f, 0.45f);

        void Awake()
        {
            originalScale = transform.localScale;
            image = GetComponent<Image>();
            if (image != null)
                originalColor = image.color;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            bounceTimer = 0.08f;
            transform.localScale = originalScale * 0.9f;
            if (image != null)
                image.color = pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            bounceTimer = 0.1f;
            transform.localScale = originalScale * 1.05f;
        }

        void Update()
        {
            if (bounceTimer > 0f)
            {
                bounceTimer -= Time.deltaTime;
                if (bounceTimer <= 0f)
                {
                    transform.localScale = originalScale;
                    if (!isPressed && image != null)
                        image.color = originalColor;
                }
            }
        }
    }
}
