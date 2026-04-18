using UnityEngine;
using System;
using System.Collections.Generic;

namespace Pathogen
{
    public enum Team { Virus, Immune }

    public enum BodyCondition
    {
        Critical,   // 0-20%  - Virus advantage
        Sick,       // 20-40% - Slight virus advantage
        Normal,     // 40-60% - Balanced
        Recovering, // 60-80% - Slight immune advantage
        Healthy     // 80-100% - Immune advantage
    }

    /// <summary>
    /// Organ objectives that can be captured. Each has a different health shift value.
    /// </summary>
    [Serializable]
    public class OrganObjective
    {
        public string name;
        public float healthShift;
        public string description;

        // Defender creature that spawns at 50% organ HP
        public string defenderName;
        public float defenderHealth;
        public float defenderDamage;
        public float defenderSpeed;
        public Color defenderColor;
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Human Health")]
        [SerializeField] private float humanHealth = 50f;
        public float HumanHealth => humanHealth;
        public float HumanHealthNormalized => humanHealth / 100f;
        public BodyCondition CurrentCondition => GetCondition(humanHealth);

        [Header("Game State")]
        public bool GameActive { get; private set; } = true;
        public Team? WinningTeam { get; private set; }
        public Team playerTeam = Team.Virus;

        [Header("Health Shift - Champion Kills")]
        public float baseChampionKillShift = 1f;
        public float scaledChampionKillShift = 3f;
        public int takedownsToScale = 10;

        [Header("Health Shift - Structures")]
        public float structureDestroyShift = 5f;

        [Header("Shared Gold")]
        public float goldShareRange = 12f; // Nearby teammates share gold

        [Header("Condition Bonuses")]
        public float strongBonus = 0.20f;
        public float mildBonus = 0.10f;

        [Header("Economy")]
        public float passiveGoldRate = 2f;
        public float passiveGoldTimer;

        [Header("Organ Objectives")]
        public OrganObjective[] organs;

        // Takedown tracking per team
        private int virusTakedowns;
        private int immuneTakedowns;

        // Entity tracking
        private readonly List<Entity> allEntities = new List<Entity>();

        // Events
        public event Action<float> OnHumanHealthChanged;
        public event Action<BodyCondition> OnConditionChanged;
        public event Action<Team> OnGameOver;

        private BodyCondition lastCondition;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            lastCondition = CurrentCondition;

            InitializeOrgans();
        }

        void Update()
        {
            if (!GameActive) return;

            passiveGoldTimer += Time.deltaTime;
            if (passiveGoldTimer >= 1f)
            {
                passiveGoldTimer -= 1f;
                foreach (var entity in allEntities)
                {
                    var champ = entity as Champion;
                    if (champ != null && !champ.IsDead)
                        champ.AddGold(passiveGoldRate);
                }
            }
        }

        private void InitializeOrgans()
        {
            organs = new OrganObjective[]
            {
                new OrganObjective { name = "Brain (Cerebrum)", healthShift = 15f,
                    description = "Command center. Capturing devastates cognitive function.",
                    defenderName = "Microglial Guardian", defenderHealth = 1800f, defenderDamage = 120f,
                    defenderSpeed = 3.5f, defenderColor = new Color(0.9f, 0.9f, 1f) },

                new OrganObjective { name = "Heart", healthShift = 15f,
                    description = "The pump. Capturing causes rapid flatline.",
                    defenderName = "Cardiac Sentinel", defenderHealth = 2000f, defenderDamage = 100f,
                    defenderSpeed = 5f, defenderColor = new Color(0.8f, 0.1f, 0.2f) },

                new OrganObjective { name = "Lungs", healthShift = 10f,
                    description = "Oxygen supply. Capture cuts breathing capacity.",
                    defenderName = "Alveolar Warden", defenderHealth = 1400f, defenderDamage = 85f,
                    defenderSpeed = 4f, defenderColor = new Color(0.6f, 0.8f, 1f) },

                new OrganObjective { name = "Liver", healthShift = 8f,
                    description = "Detox center. Shifts toxin processing.",
                    defenderName = "Kupffer Brute", defenderHealth = 1500f, defenderDamage = 95f,
                    defenderSpeed = 3f, defenderColor = new Color(0.5f, 0.3f, 0.15f) },

                new OrganObjective { name = "Kidneys", healthShift = 7f,
                    description = "Filtration. Affects waste and toxin flushing.",
                    defenderName = "Mesangial Crusher", defenderHealth = 1300f, defenderDamage = 90f,
                    defenderSpeed = 3.5f, defenderColor = new Color(0.6f, 0.3f, 0.4f) },

                new OrganObjective { name = "Stomach", healthShift = 5f,
                    description = "Digestion. Controls nutrient absorption.",
                    defenderName = "Gastric Acidborn", defenderHealth = 1000f, defenderDamage = 110f,
                    defenderSpeed = 2.5f, defenderColor = new Color(0.7f, 0.9f, 0.2f) },

                new OrganObjective { name = "Intestines", healthShift = 5f,
                    description = "The gut. Home of the microbiome.",
                    defenderName = "Gut Flora Golem", defenderHealth = 1100f, defenderDamage = 70f,
                    defenderSpeed = 3f, defenderColor = new Color(0.4f, 0.7f, 0.3f) },

                new OrganObjective { name = "Spleen", healthShift = 6f,
                    description = "Blood filter and immune cell storage.",
                    defenderName = "Splenic Ravager", defenderHealth = 1200f, defenderDamage = 100f,
                    defenderSpeed = 4.5f, defenderColor = new Color(0.5f, 0.1f, 0.3f) },

                new OrganObjective { name = "Bone Marrow", healthShift = 8f,
                    description = "Cell factory. Produces blood and immune cells.",
                    defenderName = "Osteoclast Titan", defenderHealth = 1600f, defenderDamage = 80f,
                    defenderSpeed = 2.5f, defenderColor = new Color(0.9f, 0.85f, 0.7f) },

                new OrganObjective { name = "Thymus", healthShift = 6f,
                    description = "T-cell training ground. Affects adaptive immunity.",
                    defenderName = "Thymic Instructor", defenderHealth = 1100f, defenderDamage = 95f,
                    defenderSpeed = 4f, defenderColor = new Color(0.7f, 0.5f, 0.9f) },

                new OrganObjective { name = "Lymph Vessels", healthShift = 4f,
                    description = "Transport network for immune response speed.",
                    defenderName = "Lymphatic Patrol", defenderHealth = 900f, defenderDamage = 75f,
                    defenderSpeed = 5.5f, defenderColor = new Color(0.8f, 1f, 0.8f) },

                new OrganObjective { name = "Skin (Epidermis)", healthShift = 3f,
                    description = "Outer barrier. First line of defense.",
                    defenderName = "Langerhans Sentry", defenderHealth = 800f, defenderDamage = 65f,
                    defenderSpeed = 4f, defenderColor = new Color(0.9f, 0.75f, 0.6f) },
            };
        }

        // ─── ENTITY TRACKING ────────────────────────────────────────────

        public void RegisterEntity(Entity entity)
        {
            if (!allEntities.Contains(entity))
                allEntities.Add(entity);
        }

        public void UnregisterEntity(Entity entity)
        {
            allEntities.Remove(entity);
        }

        private static readonly List<Entity> sharedEntityBuffer = new List<Entity>(32);

        public List<Entity> GetEntitiesInRange(Vector3 position, float range, Team? teamFilter = null)
        {
            sharedEntityBuffer.Clear();
            float rangeSqr = range * range;
            for (int i = allEntities.Count - 1; i >= 0; i--)
            {
                var e = allEntities[i];
                if (e == null || e.IsDead) continue;
                if (teamFilter.HasValue && e.team != teamFilter.Value) continue;
                if ((e.transform.position - position).sqrMagnitude <= rangeSqr)
                    sharedEntityBuffer.Add(e);
            }
            return sharedEntityBuffer;
        }

        public Entity GetNearestEnemy(Vector3 position, float range, Team myTeam)
        {
            Entity nearest = null;
            float nearestDist = range * range;
            Team enemyTeam = myTeam == Team.Virus ? Team.Immune : Team.Virus;

            for (int i = allEntities.Count - 1; i >= 0; i--)
            {
                var e = allEntities[i];
                if (e == null || e.IsDead || e.team != enemyTeam) continue;
                float dist = (e.transform.position - position).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = e;
                }
            }
            return nearest;
        }

        // ─── HEALTH SHIFT ───────────────────────────────────────────────

        public void ShiftHumanHealth(float amount, Team causedBy)
        {
            if (!GameActive) return;

            float shift = causedBy == Team.Virus ? -Mathf.Abs(amount) : Mathf.Abs(amount);
            humanHealth = Mathf.Clamp(humanHealth + shift, 0f, 100f);

            OnHumanHealthChanged?.Invoke(humanHealth);

            var newCondition = CurrentCondition;
            if (newCondition != lastCondition)
            {
                lastCondition = newCondition;
                OnConditionChanged?.Invoke(newCondition);
            }

            if (humanHealth <= 0f)
            {
                GameActive = false;
                WinningTeam = Team.Virus;
                OnGameOver?.Invoke(Team.Virus);
            }
            else if (humanHealth >= 100f)
            {
                GameActive = false;
                WinningTeam = Team.Immune;
                OnGameOver?.Invoke(Team.Immune);
            }
        }

        /// <summary>
        /// Get champion kill health shift. Starts at 1%, scales to 3% after 10 takedowns.
        /// </summary>
        public float GetChampionKillShift(Team killerTeam)
        {
            int takedowns = killerTeam == Team.Virus ? virusTakedowns : immuneTakedowns;
            if (takedowns >= takedownsToScale)
                return scaledChampionKillShift;
            return baseChampionKillShift;
        }

        public void RecordChampionTakedown(Team killerTeam)
        {
            if (killerTeam == Team.Virus)
                virusTakedowns++;
            else
                immuneTakedowns++;
        }

        /// <summary>
        /// Get the health shift for capturing a specific organ by index.
        /// </summary>
        public float GetOrganCaptureShift(int organIndex)
        {
            if (organs == null || organIndex < 0 || organIndex >= organs.Length)
                return 5f;
            return organs[organIndex].healthShift;
        }

        // ─── SHARED GOLD ────────────────────────────────────────────────

        /// <summary>
        /// Distribute gold from a kill to nearby teammates. If multiple champions
        /// of the same team are nearby, gold is split evenly among them.
        /// </summary>
        public void DistributeGold(Vector3 killPosition, Team team, float totalGold)
        {
            var nearbyChampions = new List<Champion>();
            float rangeSqr = goldShareRange * goldShareRange;

            foreach (var entity in allEntities)
            {
                var champ = entity as Champion;
                if (champ == null || champ.IsDead || champ.team != team) continue;
                if ((champ.transform.position - killPosition).sqrMagnitude <= rangeSqr)
                    nearbyChampions.Add(champ);
            }

            if (nearbyChampions.Count == 0) return;

            float goldPerChampion = totalGold / nearbyChampions.Count;
            foreach (var champ in nearbyChampions)
                champ.AddGold(goldPerChampion);
        }

        /// <summary>
        /// Distribute XP from a kill to nearby teammates.
        /// </summary>
        public void DistributeXP(Vector3 killPosition, Team team, float totalXP, bool showText = true)
        {
            var nearbyChampions = new List<Champion>();
            float rangeSqr = goldShareRange * goldShareRange;

            foreach (var entity in allEntities)
            {
                var champ = entity as Champion;
                if (champ == null || champ.IsDead || champ.team != team) continue;
                if ((champ.transform.position - killPosition).sqrMagnitude <= rangeSqr)
                    nearbyChampions.Add(champ);
            }

            if (nearbyChampions.Count == 0) return;

            float xpPerChampion = totalXP / nearbyChampions.Count;
            foreach (var champ in nearbyChampions)
            {
                champ.AddXP(xpPerChampion);
                if (showText)
                {
                    float textY = champ.championHeight + 2.5f;
                    FloatingText.Spawn(
                        champ.transform.position + Vector3.up * textY,
                        $"+{Mathf.FloorToInt(xpPerChampion)} XP",
                        new Color(0.4f, 0.7f, 1f));
                }
            }
        }

        // ─── CONDITION BONUSES ──────────────────────────────────────────

        public float GetTeamDamageMultiplier(Team team)
        {
            switch (CurrentCondition)
            {
                case BodyCondition.Critical:
                    return team == Team.Virus ? 1f + strongBonus : 1f - strongBonus;
                case BodyCondition.Sick:
                    return team == Team.Virus ? 1f + mildBonus : 1f - mildBonus;
                case BodyCondition.Recovering:
                    return team == Team.Immune ? 1f + mildBonus : 1f - mildBonus;
                case BodyCondition.Healthy:
                    return team == Team.Immune ? 1f + strongBonus : 1f - strongBonus;
                default:
                    return 1f;
            }
        }

        private static BodyCondition GetCondition(float health)
        {
            if (health < 20f) return BodyCondition.Critical;
            if (health < 40f) return BodyCondition.Sick;
            if (health < 60f) return BodyCondition.Normal;
            if (health < 80f) return BodyCondition.Recovering;
            return BodyCondition.Healthy;
        }
    }
}
