using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Team base zone placed at spawn points. Regenerates 25% of max health
    /// and mana every 0.4 seconds for all allied champions within range.
    /// </summary>
    public class Base : MonoBehaviour
    {
        public Team team;
        public float range = 5f;

        private const float TickInterval = 0.4f;
        private const float RegenPercent = 0.25f;

        private float tickTimer;

        public void Show()
        {
            Color baseColor = team == Team.Virus
                ? new Color(0.5f, 0.1f, 0.1f, 0.4f)
                : new Color(0.1f, 0.2f, 0.5f, 0.4f);

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            platform.name = "BasePlatform";
            platform.transform.SetParent(transform, false);
            platform.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            platform.transform.localScale = new Vector3(range * 2f, 0.1f, range * 2f);
            platform.GetComponent<Renderer>().material.color = baseColor;
            platform.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Object.DestroyImmediate(platform.GetComponent<CapsuleCollider>());
            platform.isStatic = true;

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "BasePillar";
            pillar.transform.SetParent(transform, false);
            pillar.transform.localPosition = new Vector3(0f, 1f, 0f);
            pillar.transform.localScale = new Vector3(0.8f, 2f, 0.8f);
            Color pillarColor = team == Team.Virus
                ? new Color(0.7f, 0.15f, 0.15f)
                : new Color(0.15f, 0.3f, 0.7f);
            pillar.GetComponent<Renderer>().material.color = pillarColor;
            Object.DestroyImmediate(pillar.GetComponent<CapsuleCollider>());
            pillar.isStatic = true;
        }

        void Update()
        {
            tickTimer -= Time.deltaTime;
            if (tickTimer > 0f) return;

            tickTimer = TickInterval;
            Regenerate();
        }

        private void Regenerate()
        {
            if (GameManager.Instance == null) return;

            var nearby = GameManager.Instance.GetEntitiesInRange(transform.position, range, team);

            foreach (var entity in nearby)
            {
                if (entity.IsDead || entity.entityType != EntityType.Champion) continue;

                var champion = entity as Champion;
                if (champion == null || champion.isRespawning) continue;

                if (champion.currentHealth < champion.maxHealth)
                    champion.Heal(champion.maxHealth * RegenPercent);

                if (champion.currentMana < champion.maxMana)
                {
                    champion.currentMana = Mathf.Min(champion.maxMana,
                        champion.currentMana + champion.maxMana * RegenPercent);
                    champion.InvokeManaChanged();
                }
            }
        }
    }
}
