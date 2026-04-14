using UnityEngine;

namespace Pathogen
{
    public class SkillAimIndicator : MonoBehaviour
    {
        // Line renderer kept only for attack-button aim (not skill indicators)
        private LineRenderer aimLine;

        // Skill indicator shapes — all use Bio-Pulse shader
        private GameObject rectangleIndicator;
        private GameObject coneIndicator;
        private GameObject discIndicator;
        private GameObject rangeRing;

        private Material rectangleMaterial;
        private Material coneMaterial;
        private Material discMaterial;
        private Material rangeRingMaterial;

        private bool isActive;

        // Cached meshes
        private static Mesh quadMesh;
        private static Mesh discMesh;
        private static Mesh ringMesh;
        private float cachedConeAngle = -1f;
        private Mesh cachedConeMesh;

        // Colors
        private static readonly Color indicatorColor = new Color(0.3f, 0.5f, 0.8f, 1f);
        private static readonly Color cancelColor = new Color(1f, 0.2f, 0.2f, 1f);

        // Line renderer colors (attack aim only)
        public Color lineColor = new Color(0.3f, 0.5f, 0.8f, 0.7f);
        private static readonly Color outOfRangeColor = new Color(1f, 0.3f, 0.3f, 0.3f);

        private static Shader bioPulseShader;

        void Awake()
        {
            bioPulseShader = Shader.Find("Pathogen/BioPulse");
            if (bioPulseShader == null)
                bioPulseShader = Shader.Find("Universal Render Pipeline/Unlit");

            CreateAimLine();
            CreateRectangleIndicator();
            CreateConeIndicator();
            CreateDiscIndicator();
            CreateRangeRing();
            Hide();
        }

        // ─── SHARED MATERIAL SETUP ──────────────────────────────────────

        private static Material CreateBioPulseMaterial(Color color,
            float fillAlpha = 0.06f, float edgeAlpha = 0.7f)
        {
            var mat = new Material(bioPulseShader);
            mat.SetColor("_Color", color);
            mat.SetFloat("_FillAlpha", fillAlpha);
            mat.SetFloat("_EdgeAlpha", edgeAlpha);
            mat.renderQueue = 3010;
            return mat;
        }

        private static void SetMaterialColor(Material mat, Color color)
        {
            if (mat != null) mat.SetColor("_Color", color);
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

        private void CreateRectangleIndicator()
        {
            if (quadMesh == null) quadMesh = GenerateSubdividedQuad(8, 20);

            rectangleIndicator = new GameObject("RectangleIndicator");
            var filter = rectangleIndicator.AddComponent<MeshFilter>();
            filter.sharedMesh = quadMesh;

            rectangleMaterial = CreateBioPulseMaterial(indicatorColor);
            var renderer = rectangleIndicator.AddComponent<MeshRenderer>();
            renderer.material = rectangleMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            rectangleIndicator.SetActive(false);
        }

        private void CreateConeIndicator()
        {
            coneIndicator = new GameObject("ConeIndicator");
            coneIndicator.AddComponent<MeshFilter>();

            coneMaterial = CreateBioPulseMaterial(indicatorColor);
            var renderer = coneIndicator.AddComponent<MeshRenderer>();
            renderer.material = coneMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            coneIndicator.SetActive(false);
        }

        private void CreateDiscIndicator()
        {
            if (discMesh == null) discMesh = GenerateDiscMesh(48, 6);

            discIndicator = new GameObject("DiscIndicator");
            var filter = discIndicator.AddComponent<MeshFilter>();
            filter.sharedMesh = discMesh;

            discMaterial = CreateBioPulseMaterial(indicatorColor);
            var renderer = discIndicator.AddComponent<MeshRenderer>();
            renderer.material = discMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            discIndicator.SetActive(false);
        }

        private void CreateRangeRing()
        {
            if (ringMesh == null) ringMesh = GenerateRingMesh(0.47f, 0.5f, 64);

            rangeRing = new GameObject("RangeRing");
            var filter = rangeRing.AddComponent<MeshFilter>();
            filter.sharedMesh = ringMesh;

            rangeRingMaterial = CreateBioPulseMaterial(indicatorColor, 0f, 0.35f);
            rangeRingMaterial.SetFloat("_PulseIntensity", 0f);
            var renderer = rangeRing.AddComponent<MeshRenderer>();
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

            Vector3 start = new Vector3(origin.x, 0.05f, origin.z);
            Vector3 end = start + direction.normalized * range;
            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, end);
        }

        public void ShowRectangle(Vector3 origin, Vector3 direction, float range, float width)
        {
            isActive = true;
            HideAllShapes();
            rectangleIndicator.SetActive(true);

            Vector3 dir = direction.normalized;
            rectangleIndicator.transform.position = new Vector3(origin.x, 0.05f, origin.z);
            rectangleIndicator.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            rectangleIndicator.transform.localScale = new Vector3(width, 1f, range);
        }

        public void ShowCone(Vector3 origin, Vector3 direction, float range, float angle)
        {
            isActive = true;
            HideAllShapes();
            coneIndicator.SetActive(true);

            if (!Mathf.Approximately(angle, cachedConeAngle))
            {
                cachedConeMesh = GenerateSubdividedCone(angle, 32, 5);
                cachedConeAngle = angle;
            }
            coneIndicator.GetComponent<MeshFilter>().sharedMesh = cachedConeMesh;

            Vector3 dir = direction.normalized;
            coneIndicator.transform.position = new Vector3(origin.x, 0.05f, origin.z);
            coneIndicator.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            coneIndicator.transform.localScale = new Vector3(range, 1f, range);
        }

        public void ShowAOECircle(Vector3 position, float radius)
        {
            isActive = true;
            HideAllShapes();
            discIndicator.SetActive(true);

            discIndicator.transform.position = new Vector3(position.x, 0.05f, position.z);
            discIndicator.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);
        }

        public void ShowDirectionWithAOE(Vector3 origin, Vector3 direction, float range, float aoeRadius)
        {
            isActive = true;
            HideAllShapes();
            aimLine.gameObject.SetActive(true);
            discIndicator.SetActive(true);

            Vector3 start = new Vector3(origin.x, 0.05f, origin.z);
            Vector3 end = start + direction.normalized * range;
            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, end);

            discIndicator.transform.position = new Vector3(end.x, 0.05f, end.z);
            discIndicator.transform.localScale = new Vector3(aoeRadius * 2f, 1f, aoeRadius * 2f);
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

        // ─── UNIFIED SHOW BY SKILL ──────────────────────────────────────

        public void ShowSkillIndicator(SkillDefinition def, Vector3 origin, Vector3 direction)
        {
            float range = def.GetEffectiveRange();

            switch (def.indicatorShape)
            {
                case IndicatorShape.Line:
                    ShowRectangle(origin, direction, range, 0.4f);
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

        private void HideAllShapes()
        {
            if (aimLine != null) aimLine.gameObject.SetActive(false);
            if (rectangleIndicator != null) rectangleIndicator.SetActive(false);
            if (coneIndicator != null) coneIndicator.SetActive(false);
            if (discIndicator != null) discIndicator.SetActive(false);
        }

        public void Hide()
        {
            isActive = false;
            HideAllShapes();
            HideRangeRing();
        }

        public void SetCancelTint(bool cancelling)
        {
            Color color = cancelling ? cancelColor : indicatorColor;
            SetMaterialColor(rectangleMaterial, color);
            SetMaterialColor(coneMaterial, color);
            SetMaterialColor(discMaterial, color);
            if (aimLine != null)
                aimLine.material.color = cancelling
                    ? new Color(1f, 0.2f, 0.2f, 0.7f) : lineColor;
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
            if (rectangleIndicator != null) Destroy(rectangleIndicator);
            if (coneIndicator != null) Destroy(coneIndicator);
            if (discIndicator != null) Destroy(discIndicator);
            if (rangeRing != null) Destroy(rangeRing);
        }

        // ─── MESH GENERATION ────────────────────────────────────────────

        private static Mesh GenerateSubdividedQuad(int segX, int segZ)
        {
            int vertCount = (segX + 1) * (segZ + 1);
            var vertices = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];
            var normals = new Vector3[vertCount];

            for (int z = 0; z <= segZ; z++)
            {
                for (int x = 0; x <= segX; x++)
                {
                    int idx = z * (segX + 1) + x;
                    float u = (float)x / segX;
                    float v = (float)z / segZ;

                    vertices[idx] = new Vector3(u - 0.5f, 0f, v);
                    normals[idx] = Vector3.up;

                    float edgeDist = Mathf.Min(u, 1f - u, v, 1f - v) * 2f;
                    uvs[idx] = new Vector2(Mathf.Clamp01(edgeDist), 0f);
                }
            }

            int triCount = segX * segZ * 6;
            var triangles = new int[triCount];
            int ti = 0;
            for (int z = 0; z < segZ; z++)
            {
                for (int x = 0; x < segX; x++)
                {
                    int bl = z * (segX + 1) + x;
                    int br = bl + 1;
                    int tl = bl + (segX + 1);
                    int tr = tl + 1;
                    triangles[ti++] = bl;
                    triangles[ti++] = tl;
                    triangles[ti++] = br;
                    triangles[ti++] = br;
                    triangles[ti++] = tl;
                    triangles[ti++] = tr;
                }
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uvs;
            return mesh;
        }

        private static Mesh GenerateSubdividedCone(float angleDegrees, int segments, int rings)
        {
            float halfAngle = angleDegrees * 0.5f * Mathf.Deg2Rad;
            int vertsPerRing = segments + 1;
            int vertCount = 1 + (rings + 1) * vertsPerRing;
            var vertices = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];

            // Center vertex
            vertices[0] = Vector3.zero;
            uvs[0] = new Vector2(1f, 0f);

            // Ring vertices
            for (int r = 0; r <= rings; r++)
            {
                float radius = (float)(r + 1) / (rings + 1);
                for (int i = 0; i <= segments; i++)
                {
                    int idx = 1 + r * vertsPerRing + i;
                    float t = (float)i / segments;
                    float angle = Mathf.Lerp(-halfAngle, halfAngle, t);

                    vertices[idx] = new Vector3(
                        Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);

                    float radialEdge = 1f - radius;
                    float angularEdge = Mathf.Min(t, 1f - t) * 2f;
                    uvs[idx] = new Vector2(Mathf.Min(radialEdge, angularEdge), 0f);
                }
            }

            // Triangles: center fan + ring strips
            int triCount = segments * 3 + rings * segments * 6;
            var triangles = new int[triCount];
            int ti = 0;

            // Center fan to first ring
            for (int i = 0; i < segments; i++)
            {
                triangles[ti++] = 0;
                triangles[ti++] = 1 + i;
                triangles[ti++] = 1 + i + 1;
            }

            // Ring-to-ring strips
            for (int r = 0; r < rings; r++)
            {
                int ring0 = 1 + r * vertsPerRing;
                int ring1 = 1 + (r + 1) * vertsPerRing;
                for (int i = 0; i < segments; i++)
                {
                    triangles[ti++] = ring0 + i;
                    triangles[ti++] = ring1 + i;
                    triangles[ti++] = ring0 + i + 1;
                    triangles[ti++] = ring0 + i + 1;
                    triangles[ti++] = ring1 + i;
                    triangles[ti++] = ring1 + i + 1;
                }
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Mesh GenerateDiscMesh(int segments, int rings)
        {
            float maxRadius = 0.5f;
            int vertCount = 1 + (rings + 1) * segments;
            var vertices = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];

            // Center
            vertices[0] = Vector3.zero;
            uvs[0] = new Vector2(1f, 0f);

            for (int r = 0; r <= rings; r++)
            {
                float radius = (float)(r + 1) / (rings + 1);
                for (int i = 0; i < segments; i++)
                {
                    int idx = 1 + r * segments + i;
                    float angle = (float)i / segments * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * radius * maxRadius;
                    float z = Mathf.Sin(angle) * radius * maxRadius;

                    vertices[idx] = new Vector3(x, 0f, z);
                    uvs[idx] = new Vector2(1f - radius, 0f);
                }
            }

            int triCount = segments * 3 + rings * segments * 6;
            var triangles = new int[triCount];
            int ti = 0;

            // Center fan (reversed winding for upward normals)
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles[ti++] = 0;
                triangles[ti++] = 1 + next;
                triangles[ti++] = 1 + i;
            }

            // Ring strips (reversed winding for upward normals)
            for (int r = 0; r < rings; r++)
            {
                int ring0 = 1 + r * segments;
                int ring1 = 1 + (r + 1) * segments;
                for (int i = 0; i < segments; i++)
                {
                    int next = (i + 1) % segments;
                    triangles[ti++] = ring0 + i;
                    triangles[ti++] = ring0 + next;
                    triangles[ti++] = ring1 + i;
                    triangles[ti++] = ring0 + next;
                    triangles[ti++] = ring1 + next;
                    triangles[ti++] = ring1 + i;
                }
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Mesh GenerateRingMesh(float innerRadius, float outerRadius, int segments)
        {
            var mesh = new Mesh();
            var vertices = new Vector3[segments * 2];
            var uvs = new Vector2[segments * 2];
            var triangles = new int[segments * 6];

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                vertices[i * 2] = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
                vertices[i * 2 + 1] = new Vector3(cos * outerRadius, 0f, sin * outerRadius);

                // All ring vertices are edge (UV.x = 0)
                uvs[i * 2] = new Vector2(0.02f, 0f);
                uvs[i * 2 + 1] = new Vector2(0f, 0f);

                int ti = i * 6;
                int curr = i * 2;
                int next = ((i + 1) % segments) * 2;

                triangles[ti] = curr;
                triangles[ti + 1] = curr + 1;
                triangles[ti + 2] = next;
                triangles[ti + 3] = next;
                triangles[ti + 4] = curr + 1;
                triangles[ti + 5] = next + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
