using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Spawns waves of 6 minions: 3 front row, 3 back row.
    /// Front and back rows are ~4-5 units apart with organic noise.
    /// Minions walk toward the enemy side and stop when they see an attackable target.
    /// </summary>
    public class MinionSpawner : MonoBehaviour
    {
        [Header("Spawning")]
        public Team team;
        public float waveInterval = 30f;

        [Header("Formation")]
        public int frontRow = 3;
        public int backRow = 3;
        public float rowGap = 4.5f;       // Distance between front and back row
        public float lateralSpread = 1.5f; // Spread across Z axis within a row
        public float noise = 0.6f;         // Random offset for organic feel

        [Header("Minion Stats")]
        public float minionHealth = 300f;
        public float minionDamage = 10f;
        public float minionSpeed = 4f;
        public float minionArmor = 5f;
        public float minionAttackRange = 2f;
        public float minionAttackSpeed = 0.5f; // 2s between attacks

        [Header("Waypoints")]
        public Vector3[] waypoints;

        /// <summary>
        /// Direction the wave marches (1 = positive X, -1 = negative X).
        /// Set by GameBootstrap based on team.
        /// </summary>
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
                SpawnWave();
            }
        }

        private void SpawnWave()
        {
            Vector3 origin = transform.position;

            // Front row (closer to enemy)
            for (int i = 0; i < frontRow; i++)
            {
                float z = GetRowZ(i, frontRow);
                float xNoise = Random.Range(-noise * 0.5f, noise * 0.5f);
                float zNoise = Random.Range(-noise, noise);
                Vector3 pos = origin + new Vector3(xNoise, 0f, z + zNoise);
                SpawnMinion(pos);
            }

            // Back row (further from enemy, behind front row)
            for (int i = 0; i < backRow; i++)
            {
                float z = GetRowZ(i, backRow);
                float xNoise = Random.Range(-noise * 0.5f, noise * 0.5f);
                float zNoise = Random.Range(-noise, noise);
                float backOffset = -marchDirection * (rowGap + Random.Range(-0.5f, 0.5f));
                Vector3 pos = origin + new Vector3(backOffset + xNoise, 0f, z + zNoise);
                SpawnMinion(pos);
            }
        }

        /// <summary>
        /// Distribute minions across the Z axis within a row.
        /// </summary>
        private float GetRowZ(int index, int total)
        {
            if (total <= 1) return 0f;
            float span = (total - 1) * lateralSpread;
            return -span * 0.5f + index * lateralSpread;
        }

        private void SpawnMinion(Vector3 position)
        {
            var minionGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            minionGO.name = team == Team.Virus ? "VirusMinion" : "ImmuneMinion";
            minionGO.transform.position = position;
            minionGO.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            var renderer = minionGO.GetComponent<Renderer>();
            renderer.material.color = team == Team.Virus
                ? new Color(0.7f, 0.2f, 0.2f)
                : new Color(0.2f, 0.5f, 0.8f);

            // Use trigger collider so minions don't physically block each other or structures
            // Tall trigger collider — Y is ignored for projectile hits (XZ detection only)
            var boxCollider = minionGO.GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(1f, 15f, 1f);

            // Kinematic Rigidbody required for trigger detection with projectiles
            // Movement still uses transform.position — kinematic doesn't interfere
            var rb = minionGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var minion = minionGO.AddComponent<Minion>();
            minion.team = team;
            minion.entityName = minionGO.name;
            minion.maxHealth = minionHealth;
            minion.currentHealth = minionHealth;
            minion.attackDamage = minionDamage;
            minion.moveSpeed = minionSpeed + Random.Range(-0.3f, 0.3f); // Slight speed variance
            minion.armor = minionArmor;
            minion.attackRange = minionAttackRange;
            minion.attackSpeed = minionAttackSpeed;
            minion.waypoints = waypoints;

            var healthBar = minionGO.AddComponent<FloatingHealthBar>();
            healthBar.heightOffset = 0.8f;
            healthBar.barWidth = 0.5f;

            minionGO.AddComponent<TargetHighlight>();
        }
    }
}
