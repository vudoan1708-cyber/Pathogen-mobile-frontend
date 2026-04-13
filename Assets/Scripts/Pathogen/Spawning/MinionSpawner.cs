using UnityEngine;

namespace Pathogen
{
    public class MinionSpawner : MonoBehaviour
    {
        [Header("Spawning")]
        public Team team;
        public float waveInterval = 30f;

        [Header("Formation")]
        public int meleeCount = 3;
        public int rangedCount = 3;

        [Header("Melee Minion Stats")]
        public float meleeHealth = 300f;
        public float meleeDamage = 10f;
        public float meleeSpeed = 2.5f;
        public float meleeArmor = 5f;
        public float meleeAttackRange = 2f;
        public float meleeAttackSpeed = 1.25f;

        [Header("Ranged Minion Stats")]
        public float rangedHealth = 200f;
        public float rangedDamage = 15f;
        public float rangedSpeed = 2f;
        public float rangedArmor = 3f;
        public float rangedAttackRange = 7f;
        public float rangedAttackSpeed = 1.0f;

        [Header("Waypoints")]
        public Vector3[] waypoints;

        public float marchDirection = 1f;

        private float waveTimer;

        void Start()
        {
            waveTimer = 5f;
        }

        void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;

            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f)
            {
                waveTimer = waveInterval;
                StartCoroutine(SpawnWave());
            }
        }

        private System.Collections.IEnumerator SpawnWave()
        {
            Vector3 origin = transform.position;
            int index = 0;
            Minion waveAhead = null;

            for (int i = 0; i < meleeCount; i++)
            {
                waveAhead = SpawnMinion(origin, MinionType.Melee, index++, waveAhead);
                yield return new WaitForSeconds(0.25f);
            }

            for (int i = 0; i < rangedCount; i++)
            {
                waveAhead = SpawnMinion(origin, MinionType.Ranged, index++, waveAhead);
                yield return new WaitForSeconds(0.25f);
            }
        }

        private Minion SpawnMinion(Vector3 position, MinionType type, int formationIndex, Minion waveAhead)
        {
            bool isRanged = type == MinionType.Ranged;

            var minionGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            minionGO.transform.position = position;
            minionGO.name = $"{team}{type}Minion";
            minionGO.transform.localScale = isRanged
                ? new Vector3(0.35f, 0.35f, 0.35f)
                : new Vector3(0.45f, 0.45f, 0.45f);

            var renderer = minionGO.GetComponent<Renderer>();
            renderer.material.color = isRanged
                ? (team == Team.Virus ? new Color(0.85f, 0.3f, 0.3f) : new Color(0.3f, 0.6f, 0.9f))
                : (team == Team.Virus ? new Color(0.7f, 0.2f, 0.2f) : new Color(0.2f, 0.5f, 0.8f));

            var boxCollider = minionGO.GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(1f, 15f, 1f);

            var rb = minionGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var solidCollider = new GameObject("SolidCollider");
            solidCollider.transform.SetParent(minionGO.transform, false);
            var solidSphere = solidCollider.AddComponent<SphereCollider>();
            solidSphere.radius = 0.5f;

            var minion = minionGO.AddComponent<Minion>();
            minion.team = team;
            minion.entityName = minionGO.name;
            minion.minionType = type;
            minion.formationIndex = formationIndex;
            minion.waveAhead = waveAhead;
            minion.waypoints = waypoints;

            if (isRanged)
            {
                minion.maxHealth = rangedHealth;
                minion.currentHealth = rangedHealth;
                minion.attackDamage = rangedDamage;
                minion.moveSpeed = rangedSpeed;
                minion.armor = rangedArmor;
                minion.attackRange = rangedAttackRange;
                minion.attackSpeed = rangedAttackSpeed;
            }
            else
            {
                minion.maxHealth = meleeHealth;
                minion.currentHealth = meleeHealth;
                minion.attackDamage = meleeDamage;
                minion.moveSpeed = meleeSpeed;
                minion.armor = meleeArmor;
                minion.attackRange = meleeAttackRange;
                minion.attackSpeed = meleeAttackSpeed;
            }

            var healthBar = minionGO.AddComponent<FloatingHealthBar>();
            healthBar.heightOffset = 0.8f;
            healthBar.barWidth = 0.5f;

            minionGO.AddComponent<TargetHighlight>();

            return minion;
        }
    }
}
