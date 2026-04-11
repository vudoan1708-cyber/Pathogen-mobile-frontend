using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Adds a hover highlight ring around an entity when the mouse hovers over it.
    /// Indicates the entity is an attackable target.
    /// </summary>
    public class TargetHighlight : MonoBehaviour
    {
        private GameObject highlightRing;
        private Renderer entityRenderer;
        private Color originalColor;
        private bool isHighlighted;

        void Start()
        {
            entityRenderer = GetComponent<Renderer>();
            if (entityRenderer != null)
                originalColor = entityRenderer.material.color;

            CreateHighlightRing();
            SetHighlighted(false);
        }

        private void CreateHighlightRing()
        {
            highlightRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            highlightRing.name = "HighlightRing";
            highlightRing.transform.SetParent(transform, false);
            highlightRing.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            highlightRing.transform.localScale = new Vector3(1.6f, 0.02f, 1.6f);

            DestroyImmediate(highlightRing.GetComponent<CapsuleCollider>());

            var ringRenderer = highlightRing.GetComponent<Renderer>();
            ringRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ringRenderer.receiveShadows = false;

            ringRenderer.material.color = new Color(1f, 0.95f, 0.5f, 0.7f);
        }

        public void SetHighlighted(bool highlighted)
        {
            isHighlighted = highlighted;
            if (highlightRing != null)
                highlightRing.SetActive(highlighted);

            // Slightly brighten the entity color on hover
            if (entityRenderer != null)
            {
                if (highlighted)
                    entityRenderer.material.color = originalColor * 1.4f;
                else
                    entityRenderer.material.color = originalColor;
            }
        }

        void OnDestroy()
        {
            if (highlightRing != null)
                Destroy(highlightRing);
        }
    }
}
