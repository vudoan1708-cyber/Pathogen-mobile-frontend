using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// World-space health (+ optional mana) bar using the Pathogen/UIEntityBar shader.
    /// Features: rounded rect, border, 100 HP tick marks, damage trail (red fade),
    /// heal-over-time glow (light green), and dark background.
    /// </summary>
    public class FloatingHealthBar : MonoBehaviour
    {
        public float heightOffset = 1.5f;
        public float barWidth = 1f;
        public float barHeight = 0.1f;
        public bool showManaBar;
        public bool showTrailAnimation;     // Champions only
        public bool showTickMarks;          // Champions only — 100 HP membrane ticks

        private const float HpPerTick = 100f;
        private const float TrailFadeSpeed = 4f;  // 0.25s fade
        private const float TrailDelay = 0.1f;

        private Entity entity;

        // Health bar
        private GameObject healthBarGO;
        private Material healthMat;
        private float healthDisplayed;
        private float healthTrailPct;
        private float healthTrailSign;  // -1 damage, +1 heal
        private float trailAlpha;
        private float trailDelayTimer;
        private float lastHealthPct;

        // Mana bar (champion only)
        private GameObject manaBarGO;
        private Material manaMat;

        private static Mesh quadMesh;

        void Start()
        {
            entity = GetComponent<Entity>();
            if (entity == null) return;

            if (quadMesh == null) quadMesh = CreateQuadMesh();

            CreateHealthBar();

            var champion = entity as Champion;
            if (champion != null || showManaBar)
                CreateManaBar();

            float pct = entity.maxHealth > 0 ? entity.currentHealth / entity.maxHealth : 1f;
            healthDisplayed = pct;
            healthTrailPct = pct;
            lastHealthPct = pct;

            entity.OnHealthChanged += OnHealthChanged;
        }

        void OnDestroy()
        {
            if (entity != null)
                entity.OnHealthChanged -= OnHealthChanged;
            if (healthBarGO != null) Destroy(healthBarGO);
            if (manaBarGO != null) Destroy(manaBarGO);
        }

        void Update()
        {
            if (entity == null || healthBarGO == null) return;

            if (showTrailAnimation && trailAlpha > 0f)
            {
                if (trailDelayTimer > 0f)
                    trailDelayTimer -= Time.deltaTime;
                else
                {
                    trailAlpha -= TrailFadeSpeed * Time.deltaTime;
                    if (trailAlpha <= 0f)
                    {
                        trailAlpha = 0f;
                        healthTrailPct = healthDisplayed;
                    }
                }
                healthMat.SetFloat("_TrailAlpha", trailAlpha);
            }

            // Update mana if present
            var champion = entity as Champion;
            if (manaMat != null && champion != null)
            {
                float manaPct = champion.maxMana > 0 ? champion.currentMana / champion.maxMana : 0f;
                manaMat.SetFloat("_FillPct", manaPct);
            }
        }

        void LateUpdate()
        {
            if (healthBarGO == null || entity == null) return;

            Vector3 pos = transform.position + Vector3.up * heightOffset;
            healthBarGO.transform.position = pos;

            if (manaBarGO != null)
                manaBarGO.transform.position = pos - Vector3.up * (barHeight + 0.015f);

            if (Camera.main != null)
            {
                healthBarGO.transform.rotation = Camera.main.transform.rotation;
                if (manaBarGO != null)
                    manaBarGO.transform.rotation = Camera.main.transform.rotation;
            }

            bool visible = !entity.IsDead;
            healthBarGO.SetActive(visible);
            if (manaBarGO != null) manaBarGO.SetActive(visible);
        }

        // ─── EVENTS ─────────────────────────────────────────────────────

        private void OnHealthChanged(float current, float max)
        {
            float pct = max > 0 ? Mathf.Clamp01(current / max) : 0f;

            float delta = pct - lastHealthPct;

            if (showTrailAnimation && Mathf.Abs(delta) > 0.001f)
            {
                healthTrailPct = lastHealthPct;

                if (delta < 0f)
                {
                    healthTrailSign = -1f;
                    trailAlpha = 1f;
                    trailDelayTimer = TrailDelay;
                }
                else
                {
                    healthTrailSign = 1f;
                    trailAlpha = 0.8f;
                    trailDelayTimer = 0.1f;
                }

                healthMat.SetFloat("_TrailPct", healthTrailPct);
                healthMat.SetFloat("_TrailSign", healthTrailSign);
            }

            healthDisplayed = pct;
            lastHealthPct = pct;
            healthMat.SetFloat("_FillPct", pct);

            if (showTickMarks)
            {
                float tickUV = max > 0 ? HpPerTick / max : 0.1f;
                healthMat.SetFloat("_TickSpacing", tickUV);
            }
        }

        // ─── CREATION ───────────────────────────────────────────────────

        private void CreateHealthBar()
        {
            var shader = Shader.Find("Pathogen/UIEntityBar");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");

            healthMat = new Material(shader);

            float pct = entity.maxHealth > 0 ? entity.currentHealth / entity.maxHealth : 1f;
            float tickUV = showTickMarks && entity.maxHealth > 0 ? HpPerTick / entity.maxHealth : 0f;

            Color fillColor = entity.team == Team.Virus
                ? new Color(0.85f, 0.18f, 0.18f)
                : new Color(0.2f, 0.72f, 0.3f);

            healthMat.SetFloat("_FillPct", pct);
            healthMat.SetFloat("_TrailPct", pct);
            healthMat.SetFloat("_TrailSign", 0f);
            healthMat.SetFloat("_TrailAlpha", 0f);
            healthMat.SetColor("_FillColor", fillColor);
            healthMat.SetFloat("_TickSpacing", tickUV);

            healthBarGO = CreateBarQuad("HealthBar", healthMat, barWidth, barHeight);
        }

        private void CreateManaBar()
        {
            var shader = Shader.Find("Pathogen/UIEntityBar");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");

            manaMat = new Material(shader);

            var champion = entity as Champion;
            float manaPct = champion != null && champion.maxMana > 0
                ? champion.currentMana / champion.maxMana : 1f;

            manaMat.SetFloat("_FillPct", manaPct);
            manaMat.SetFloat("_TrailPct", manaPct);
            manaMat.SetFloat("_TrailAlpha", 0f);
            manaMat.SetColor("_FillColor", new Color(0.25f, 0.4f, 0.9f));
            manaMat.SetColor("_BorderColor", new Color(0.2f, 0.25f, 0.45f, 0.9f));
            manaMat.SetFloat("_TickSpacing", 0f); // no ticks on mana

            float manaHeight = barHeight * 0.7f;
            manaBarGO = CreateBarQuad("ManaBar", manaMat, barWidth, manaHeight);
        }

        private GameObject CreateBarQuad(string name, Material mat, float width, float height)
        {
            var go = new GameObject(name);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = quadMesh;
            go.transform.localScale = new Vector3(width, height, 1f);

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.material = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            return go;
        }

        private static Mesh CreateQuadMesh()
        {
            var mesh = new Mesh { name = "BarQuad" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3( 0.5f, -0.5f, 0f),
                new Vector3( 0.5f,  0.5f, 0f),
                new Vector3(-0.5f,  0.5f, 0f)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
