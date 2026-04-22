using UnityEngine;

namespace Pathogen
{
    public static class StructureFactory
    {
        private const float OuterFromBase = 24f;
        private const float InnerFromBase = 16f;
        private const float PulmonaryWallZ = 10f;

        private const float OuterHealth = 1500f;
        private const float InnerHealth = 2000f;
        private const float OuterDamage = 80f;
        private const float InnerDamage = 100f;
        private const float AttackSpeed = 0.88f;
        private const float Armor = 40f;
        private const float MagicResist = 30f;

        private static readonly Color VirusOuterColor = new Color(0.8f, 0.15f, 0.15f);
        private static readonly Color VirusInnerColor = new Color(0.6f, 0.1f, 0.1f);
        private static readonly Color ImmuneOuterColor = new Color(0.15f, 0.4f, 0.8f);
        private static readonly Color ImmuneInnerColor = new Color(0.1f, 0.3f, 0.6f);

        private static readonly Vector3 StructureScale = new Vector3(1.2f, 7f, 1.2f);
        private const float HealthBarHeight = 5.5f;
        private const float HealthBarWidth = 1.5f;

        public static void BuildAll(float laneLength)
        {
            float half = laneLength * 0.5f;
            BuildPulmonary(half);
        }

        private static void BuildPulmonary(float half)
        {
            CreateStructure("DarkSentinel_1", Team.Virus,
                new Vector3(-half + OuterFromBase, 0f, +PulmonaryWallZ),
                OuterHealth, OuterDamage, VirusOuterColor);
            CreateStructure("DarkSentinel_2", Team.Virus,
                new Vector3(-half + InnerFromBase, 0f, -PulmonaryWallZ),
                InnerHealth, InnerDamage, VirusInnerColor);

            CreateStructure("Sentinel_1", Team.Immune,
                new Vector3(half - OuterFromBase, 0f, -PulmonaryWallZ),
                OuterHealth, OuterDamage, ImmuneOuterColor);
            CreateStructure("Sentinel_2", Team.Immune,
                new Vector3(half - InnerFromBase, 0f, +PulmonaryWallZ),
                InnerHealth, InnerDamage, ImmuneInnerColor);
        }

        private static void CreateStructure(string name, Team team, Vector3 position,
                                            float health, float damage, Color color)
        {
            var structGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            structGO.name = name;
            structGO.transform.position = position;
            structGO.transform.localScale = StructureScale;
            structGO.GetComponent<Renderer>().material.color = color;

            structGO.GetComponent<BoxCollider>().isTrigger = true;

            var rb = structGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var solidCollider = new GameObject("SolidCollider");
            solidCollider.transform.SetParent(structGO.transform, false);
            var solidBox = solidCollider.AddComponent<BoxCollider>();
            solidBox.size = Vector3.one;

            var structure = structGO.AddComponent<Structure>();
            structure.team = team;
            structure.entityName = name;
            structure.maxHealth = health;
            structure.currentHealth = health;
            structure.attackDamage = damage;
            structure.attackSpeed = AttackSpeed;
            structure.armor = Armor;
            structure.magicResist = MagicResist;

            var hbar = structGO.AddComponent<FloatingHealthBar>();
            hbar.heightOffset = HealthBarHeight;
            hbar.barWidth = HealthBarWidth;

            structGO.AddComponent<TargetHighlight>();
        }
    }
}
