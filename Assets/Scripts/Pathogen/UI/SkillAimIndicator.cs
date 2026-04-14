using UnityEngine;

namespace Pathogen
{
    public class SkillAimIndicator : MonoBehaviour
    {
        private LineRenderer aimLine;
        private GameObject aoeCircle;
        private GameObject rectangleIndicator;
        private GameObject coneIndicator;
        private GameObject rangeRing;
        private bool isActive;

        [Header("Visuals")]
        public Color lineColor = new Color(0.3f, 0.5f, 0.8f, 0.7f);
        public Color aoeColor = new Color(0.3f, 0.5f, 0.8f, 0.3f);
        private static readonly Color outOfRangeColor = new Color(1f, 0.3f, 0.3f, 0.3f);
        private static readonly Color cancelColor = new Color(1f, 0.2f, 0.2f, 0.3f);
        private static readonly Color rangeRingColor = new Color(0.3f, 0.5f, 0.8f, 0.3f);

        private Material rectangleMaterial;
        private Material coneMaterial;
        private Material rangeRingMaterial;

        private static Mesh rangeRingMesh;

        void Awake()
        {
            CreateAimLine();
            CreateAOECircle();
            CreateRectangleIndicator();
            CreateConeIndicator();
            CreateRangeRing();
            Hide();
        }

        // ─── CREATE VISUALS ─────────────────────────────────────────────

        private void CreateAimLine()
        {
            var lineGO = new GameObject("AimLine");
            aimLine = lineGO.AddComponent<LineRenderer>();
            aimLine.useWorldSpace = true;
            aimLine.positionCount = 2;
            aimLine.startWidth = 0.15f;
            aimLine.endWidth = 0.08f;
            aimLine.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            aimLine.material.color = lineColor;
            aimLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            aimLine.receiveShadows = false;
            lineGO.SetActive(false);
        }

        private void CreateAOECircle()
        {
            aoeCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            aoeCircle.name = "AOEIndicator";
            DestroyImmediate(aoeCircle.GetComponent<CapsuleCollider>());

            var renderer = aoeCircle.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            renderer.material.color = aoeColor;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            aoeCircle.SetActive(false);
        }

        private void CreateRectangleIndicator()
        {
            rectangleIndicator = new GameObject("RectangleIndicator");
            var filter = rectangleIndicator.AddComponent<MeshFilter>();
            filter.sharedMesh = GenerateQuadMesh();

            var renderer = rectangleIndicator.AddComponent<MeshRenderer>();
            rectangleMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            rectangleMaterial.color = aoeColor;
            renderer.material = rectangleMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            rectangleIndicator.SetActive(false);
        }

        private void CreateConeIndicator()
        {
            coneIndicator = new GameObject("ConeIndicator");
            coneIndicator.AddComponent<MeshFilter>();

            var renderer = coneIndicator.AddComponent<MeshRenderer>();
            coneMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            coneMaterial.color = aoeColor;
            renderer.material = coneMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            coneIndicator.SetActive(false);
        }

        private void CreateRangeRing()
        {
            if (rangeRingMesh == null)
                rangeRingMesh = GenerateRingMesh(0.48f, 0.5f, 64);

            rangeRing = new GameObject("RangeRing");
            rangeRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var filter = rangeRing.AddComponent<MeshFilter>();
            filter.sharedMesh = rangeRingMesh;

            var renderer = rangeRing.AddComponent<MeshRenderer>();
            rangeRingMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            rangeRingMaterial.color = rangeRingColor;
            renderer.material = rangeRingMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            rangeRing.SetActive(false);
        }

        // ─── SHOW METHODS ───────────────────────────────────────────────

        public void ShowDirectionLine(Vector3 origin, Vector3 direction, float range)
        {
            isActive = true;
            aimLine.gameObject.SetActive(true);
            aoeCircle.SetActive(false);
            rectangleIndicator.SetActive(false);
            coneIndicator.SetActive(false);

            Vector3 start = new Vector3(origin.x, 0.05f, origin.z);
            Vector3 end = start + direction.normalized * range;
            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, end);
        }

        public void ShowRectangle(Vector3 origin, Vector3 direction, float range, float width)
        {
            isActive = true;
            aimLine.gameObject.SetActive(false);
            aoeCircle.SetActive(false);
            rectangleIndicator.SetActive(true);
            coneIndicator.SetActive(false);

            Vector3 dir = direction.normalized;
            Vector3 pos = new Vector3(origin.x, 0.05f, origin.z);

            rectangleIndicator.transform.position = pos;
            rectangleIndicator.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            rectangleIndicator.transform.localScale = new Vector3(width, 1f, range);
        }

        public void ShowCone(Vector3 origin, Vector3 direction, float range, float angle)
        {
            isActive = true;
            aimLine.gameObject.SetActive(false);
            aoeCircle.SetActive(false);
            rectangleIndicator.SetActive(false);
            coneIndicator.SetActive(true);

            var filter = coneIndicator.GetComponent<MeshFilter>();
            filter.sharedMesh = GenerateConeMesh(angle, 32);

            Vector3 dir = direction.normalized;
            Vector3 pos = new Vector3(origin.x, 0.05f, origin.z);

            coneIndicator.transform.position = pos;
            coneIndicator.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            coneIndicator.transform.localScale = new Vector3(range, 1f, range);
        }

        public void ShowAOECircle(Vector3 position, float radius)
        {
            isActive = true;
            aimLine.gameObject.SetActive(false);
            aoeCircle.SetActive(true);
            rectangleIndicator.SetActive(false);
            coneIndicator.SetActive(false);

            aoeCircle.transform.position = position + Vector3.up * 0.05f;
            aoeCircle.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
        }

        public void ShowDirectionWithAOE(Vector3 origin, Vector3 direction, float range, float aoeRadius)
        {
            isActive = true;
            aimLine.gameObject.SetActive(true);
            aoeCircle.SetActive(true);
            rectangleIndicator.SetActive(false);
            coneIndicator.SetActive(false);

            Vector3 start = new Vector3(origin.x, 0.05f, origin.z);
            Vector3 end = start + direction.normalized * range;
            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, end);

            aoeCircle.transform.position = new Vector3(end.x, 0.05f, end.z);
            aoeCircle.transform.localScale = new Vector3(aoeRadius * 2f, 0.02f, aoeRadius * 2f);
        }

        public void ShowRangeRing(Vector3 origin, float range)
        {
            if (rangeRing == null) return;
            rangeRing.SetActive(true);
            rangeRing.transform.position = new Vector3(origin.x, 0.05f, origin.z);
            rangeRing.transform.localScale = Vector3.one * range * 2f;
        }

        public void HideRangeRing()
        {
            if (rangeRing != null) rangeRing.SetActive(false);
        }

        // ─── UNIFIED SHOW BY SHAPE ──────────────────────────────────────

        public void ShowSkillIndicator(SkillDefinition def, Vector3 origin, Vector3 direction)
        {
            float range = def.GetEffectiveRange();

            switch (def.indicatorShape)
            {
                case IndicatorShape.Line:
                    ShowDirectionLine(origin, direction, range);
                    break;

                case IndicatorShape.Rectangle:
                    ShowRectangle(origin, direction, range, def.skillWidth);
                    break;

                case IndicatorShape.Cone:
                    ShowCone(origin, direction, range, def.coneAngle);
                    break;

                case IndicatorShape.Circle:
                    var aoePos = origin + Vector3.ClampMagnitude(direction, def.range);
                    aoePos.y = 0.05f;
                    ShowAOECircle(aoePos, def.aoeRadius);
                    break;

                case IndicatorShape.None:
                    Hide();
                    break;
            }
        }

        // ─── STATE ──────────────────────────────────────────────────────

        public void Hide()
        {
            isActive = false;
            if (aimLine != null) aimLine.gameObject.SetActive(false);
            if (aoeCircle != null) aoeCircle.SetActive(false);
            if (rectangleIndicator != null) rectangleIndicator.SetActive(false);
            if (coneIndicator != null) coneIndicator.SetActive(false);
            HideRangeRing();
        }

        public void SetCancelTint(bool cancelling)
        {
            Color color = cancelling ? cancelColor : lineColor;
            Color fillColor = cancelling ? cancelColor : aoeColor;
            if (aimLine != null) aimLine.material.color = color;
            if (aoeCircle != null) aoeCircle.GetComponent<Renderer>().material.color = fillColor;
            if (rectangleMaterial != null) rectangleMaterial.color = fillColor;
            if (coneMaterial != null) coneMaterial.color = fillColor;
        }

        public void SetOutOfRange(bool outOfRange)
        {
            Color color = outOfRange ? outOfRangeColor : lineColor;
            if (aimLine != null) aimLine.material.color = color;
        }

        public bool IsActive => isActive;

        void OnDestroy()
        {
            if (aimLine != null) Destroy(aimLine.gameObject);
            if (aoeCircle != null) Destroy(aoeCircle);
            if (rectangleIndicator != null) Destroy(rectangleIndicator);
            if (coneIndicator != null) Destroy(coneIndicator);
            if (rangeRing != null) Destroy(rangeRing);
        }

        // ─── MESH GENERATION ────────────────────────────────────────────

        private static Mesh GenerateQuadMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, 0f, 0f),
                new Vector3(0.5f, 0f, 0f),
                new Vector3(0.5f, 0f, 1f),
                new Vector3(-0.5f, 0f, 1f)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            return mesh;
        }

        private static Mesh GenerateConeMesh(float angleDegrees, int segments)
        {
            var mesh = new Mesh();
            float halfAngle = angleDegrees * 0.5f * Mathf.Deg2Rad;
            int vertCount = segments + 2;
            var vertices = new Vector3[vertCount];
            var triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
                vertices[i + 1] = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            }

            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
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
