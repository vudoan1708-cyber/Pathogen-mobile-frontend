using UnityEngine;

namespace Pathogen
{
    public class TargetHighlight : MonoBehaviour
    {
        private GameObject highlightRing;
        private Renderer entityRenderer;
        private Color originalColor;
        private float ringDiameter;

        private static Mesh ringMesh;

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
            if (ringMesh == null)
                ringMesh = GenerateRingMesh(0.42f, 0.5f, 48);

            ringDiameter = 2.5f;
            if (entityRenderer != null)
            {
                var bounds = entityRenderer.bounds;
                ringDiameter = Mathf.Max(bounds.size.x, bounds.size.z) + 1f;
            }

            highlightRing = new GameObject("HighlightRing");
            highlightRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            highlightRing.transform.localScale = Vector3.one * ringDiameter;

            var filter = highlightRing.AddComponent<MeshFilter>();
            filter.sharedMesh = ringMesh;

            var renderer = highlightRing.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            renderer.material.color = new Color(1f, 0.75f, 0.3f, 0.55f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        void LateUpdate()
        {
            if (highlightRing != null && highlightRing.activeSelf)
            {
                highlightRing.transform.position = new Vector3(
                    transform.position.x, 0.05f, transform.position.z);
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            if (highlightRing != null)
                highlightRing.SetActive(highlighted);

            if (entityRenderer != null)
                entityRenderer.material.color = highlighted ? originalColor * 1.2f : originalColor;
        }

        void OnDestroy()
        {
            if (highlightRing != null)
                Destroy(highlightRing);
        }

        private static Mesh GenerateRingMesh(float innerRadius, float outerRadius, int segments)
        {
            var mesh = new Mesh();
            var vertices = new Vector3[segments * 2];
            var triangles = new int[segments * 6];

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                vertices[i * 2] = new Vector3(cos * innerRadius, sin * innerRadius, 0f);
                vertices[i * 2 + 1] = new Vector3(cos * outerRadius, sin * outerRadius, 0f);

                int ti = i * 6;
                int curr = i * 2;
                int next = ((i + 1) % segments) * 2;

                triangles[ti] = curr;
                triangles[ti + 1] = next;
                triangles[ti + 2] = curr + 1;
                triangles[ti + 3] = next;
                triangles[ti + 4] = next + 1;
                triangles[ti + 5] = curr + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
