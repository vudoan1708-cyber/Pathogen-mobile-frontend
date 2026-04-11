using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Simple world-space health bar rendered with GL lines.
    /// Attaches to any Entity and floats above it.
    /// </summary>
    public class FloatingHealthBar : MonoBehaviour
    {
        public float heightOffset = 1.5f;
        public float barWidth = 1f;
        public float barHeight = 0.1f;

        private Entity entity;
        private GameObject barBackground;
        private GameObject barFill;
        private Transform barFillTransform;

        void Start()
        {
            entity = GetComponent<Entity>();
            if (entity == null) return;

            CreateBar();
            entity.OnHealthChanged += UpdateBar;
        }

        void OnDestroy()
        {
            if (entity != null)
                entity.OnHealthChanged -= UpdateBar;
            if (barBackground != null) Destroy(barBackground);
        }

        void LateUpdate()
        {
            if (barBackground == null || entity == null) return;

            // Position above entity, face camera
            Vector3 pos = transform.position + Vector3.up * heightOffset;
            barBackground.transform.position = pos;

            if (Camera.main != null)
                barBackground.transform.rotation = Camera.main.transform.rotation;

            // Hide if dead
            barBackground.SetActive(!entity.IsDead);
        }

        private void CreateBar()
        {
            // Background (dark)
            barBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            barBackground.name = "HealthBarBG";
            barBackground.transform.localScale = new Vector3(barWidth, barHeight, 1f);
            DestroyImmediate(barBackground.GetComponent<MeshCollider>());

            var bgRenderer = barBackground.GetComponent<Renderer>();
            bgRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            bgRenderer.material.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            bgRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            bgRenderer.receiveShadows = false;

            // Fill (colored)
            barFill = GameObject.CreatePrimitive(PrimitiveType.Quad);
            barFill.name = "HealthBarFill";
            barFill.transform.SetParent(barBackground.transform, false);
            barFill.transform.localPosition = Vector3.forward * -0.001f;
            barFill.transform.localScale = Vector3.one;
            DestroyImmediate(barFill.GetComponent<MeshCollider>());

            var fillRenderer = barFill.GetComponent<Renderer>();
            fillRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            fillRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            fillRenderer.receiveShadows = false;

            Color fillColor = entity.team == Team.Virus
                ? new Color(0.9f, 0.2f, 0.2f)
                : new Color(0.2f, 0.7f, 0.3f);
            fillRenderer.material.color = fillColor;

            barFillTransform = barFill.transform;
        }

        private void UpdateBar(float current, float max)
        {
            if (barFillTransform == null) return;

            float pct = Mathf.Clamp01(current / max);

            // Scale fill bar and offset to align left edge
            barFillTransform.localScale = new Vector3(pct, 1f, 1f);
            barFillTransform.localPosition = new Vector3((pct - 1f) * 0.5f, 0f, -0.001f);
        }
    }
}
