using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Pathogen
{
    /// <summary>
    /// Sets up the entire MOBA scene programmatically.
    /// Add this to an empty GameObject in SampleScene and hit Play.
    /// Creates: ground, lane, structures, champions, spawners, camera, and UI.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Map Settings")]
        public float laneLength = 80f;
        public float groundWidth = 30f;

        [Header("Camera Mode")]
        public CameraMode startingCameraMode = CameraMode.WideThirdPerson;

        /// <summary>
        /// Single font used by all UI text in the game. Created once here at startup.
        /// </summary>
        public static Font UIFont { get; private set; }

        private Champion playerChampion;
        private Champion aiChampion;

        void Awake()
        {
            UIFont = Font.CreateDynamicFontFromOSFont("Arial", 14);

            SetupGameManager();
            SetupLighting();
            SetupGround();
            SetupStructures();
            SetupChampions();
            SetupMinionSpawners();
            SetupCamera();
            SetupUI();
        }

        // ─── GAME MANAGER ───────────────────────────────────────────────

        private void SetupGameManager()
        {
            var gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
        }

        // ─── LIGHTING ───────────────────────────────────────────────────

        private void SetupLighting()
        {
            // Directional light
            var lightGO = new GameObject("DirectionalLight");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1.2f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // ─── GROUND ─────────────────────────────────────────────────────

        private void SetupGround()
        {
            // Main ground plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(laneLength + 20f, 1f, groundWidth);
            ground.GetComponent<Renderer>().material.color = new Color(0.25f, 0.18f, 0.22f);
            ground.isStatic = true;

            // Lane path (lighter strip down the center)
            var lanePath = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lanePath.name = "LanePath";
            lanePath.transform.position = new Vector3(0f, 0.01f, 0f);
            lanePath.transform.localScale = new Vector3(laneLength, 0.05f, 4f);
            lanePath.GetComponent<Renderer>().material.color = new Color(0.35f, 0.25f, 0.30f);
            lanePath.isStatic = true;

            // Virus side coloring (left half tint)
            var virusSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            virusSide.name = "VirusSideMarker";
            virusSide.transform.position = new Vector3(-laneLength * 0.25f, 0.02f, 0f);
            virusSide.transform.localScale = new Vector3(laneLength * 0.5f, 0.03f, groundWidth - 2f);
            virusSide.GetComponent<Renderer>().material.color = new Color(0.3f, 0.15f, 0.15f, 0.5f);
            virusSide.isStatic = true;

            // Immune side coloring
            var immuneSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            immuneSide.name = "ImmuneSideMarker";
            immuneSide.transform.position = new Vector3(laneLength * 0.25f, 0.02f, 0f);
            immuneSide.transform.localScale = new Vector3(laneLength * 0.5f, 0.03f, groundWidth - 2f);
            immuneSide.GetComponent<Renderer>().material.color = new Color(0.15f, 0.2f, 0.3f, 0.5f);
            immuneSide.isStatic = true;
        }

        // ─── STRUCTURES ─────────────────────────────────────────────────

        private void SetupStructures()
        {
            float half = laneLength * 0.5f;

            // Virus structures (Infection Nodes)
            CreateStructure("InfectionNode_1", Team.Virus, new Vector3(-half * 0.4f, 0f, 0f),
                           1500f, 80f, 10f, new Color(0.8f, 0.15f, 0.15f));
            CreateStructure("InfectionNode_2", Team.Virus, new Vector3(-half * 0.75f, 0f, 0f),
                           2000f, 100f, 12f, new Color(0.6f, 0.1f, 0.1f));

            // Immune structures (Sentinels)
            CreateStructure("Sentinel_1", Team.Immune, new Vector3(half * 0.4f, 0f, 0f),
                           1500f, 80f, 10f, new Color(0.15f, 0.4f, 0.8f));
            CreateStructure("Sentinel_2", Team.Immune, new Vector3(half * 0.75f, 0f, 0f),
                           2000f, 100f, 12f, new Color(0.1f, 0.3f, 0.6f));
        }

        private void CreateStructure(string name, Team team, Vector3 position,
                                     float health, float damage, float attackRange, Color color)
        {
            var structGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            structGO.name = name;
            structGO.transform.position = position;
            structGO.transform.localScale = new Vector3(2f, 3f, 2f);
            structGO.GetComponent<Renderer>().material.color = color;

            // Trigger collider so minions can walk through (structures detect via range, not physics)
            structGO.GetComponent<BoxCollider>().isTrigger = true;

            var rb = structGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var structure = structGO.AddComponent<Structure>();
            structure.team = team;
            structure.entityName = name;
            structure.maxHealth = health;
            structure.currentHealth = health;
            structure.attackDamage = damage;
            structure.attackRange = attackRange;
            structure.detectionRange = attackRange + 2f;
            structure.armor = 40f;
            structure.magicResist = 30f;

            var hbar = structGO.AddComponent<FloatingHealthBar>();
            hbar.heightOffset = 2.5f;
            hbar.barWidth = 1.5f;

            structGO.AddComponent<TargetHighlight>();
        }

        // ─── CHAMPIONS ──────────────────────────────────────────────────

        private void SetupChampions()
        {
            float half = laneLength * 0.5f;

            // Champions spawn at base (behind the last structure)
            // --- PLAYER: Immune team champion (right side base) ---
            playerChampion = CreateChampion(
                "Immunix", Team.Immune,
                new Vector3(half * 0.85f, 0.5f, 0f),
                new Color(0.2f, 0.6f, 1f),
                GetImmuneSkills());

            var playerCtrl = playerChampion.gameObject.AddComponent<PlayerController>();

            var cc = playerChampion.gameObject.AddComponent<CharacterController>();
            cc.height = 1f;
            cc.radius = 0.4f;
            cc.center = Vector3.zero;

            // --- AI: Virus team champion (left side base) ---
            aiChampion = CreateChampion(
                "Pathobyte", Team.Virus,
                new Vector3(-half * 0.85f, 0.5f, 0f),
                new Color(0.9f, 0.2f, 0.3f),
                GetVirusSkills());

            var aiCtrl = aiChampion.gameObject.AddComponent<AIController>();
            aiCtrl.patrolPoints = new Vector3[]
            {
                new Vector3(-half * 0.85f, 0.5f, 0f),
                new Vector3(-half * 0.5f, 0.5f, 0f),
                new Vector3(-half * 0.2f, 0.5f, 0f),
                new Vector3(0f, 0.5f, 0f),
            };
        }

        private Champion CreateChampion(string name, Team team, Vector3 position,
                                         Color color, SkillDefinition[] skills)
        {
            var champGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            champGO.name = name;
            champGO.transform.position = position;
            champGO.transform.localScale = new Vector3(1f, 1f, 1f);
            champGO.GetComponent<Renderer>().material.color = color;

            var rb = champGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var champ = champGO.AddComponent<Champion>();
            champ.team = team;
            champ.entityName = name;
            champ.championName = name;
            champ.maxHealth = 500f;
            champ.currentHealth = 500f;
            champ.maxMana = 250f;
            champ.currentMana = 250f;
            champ.attackDamage = 50f;
            champ.attackSpeed = 1f;
            champ.attackRange = 2.5f;
            champ.moveSpeed = 3.5f; // Slow at start — only improves through mutation purchases
            champ.armor = 15f;
            champ.magicResist = 12f;
            champ.healthRegen = 2f;
            champ.manaRegen = 4f;
            champ.spawnPoint = position;
            champ.InitializeSkills(skills);

            var hbar = champGO.AddComponent<FloatingHealthBar>();
            hbar.heightOffset = 1.2f;
            hbar.barWidth = 1f;

            champGO.AddComponent<TargetHighlight>();

            return champ;
        }

        private SkillDefinition[] GetVirusSkills()
        {
            return new SkillDefinition[]
            {
                new SkillDefinition
                {
                    skillName = "Toxic Spit",
                    description = "Fire a toxic projectile that damages the first enemy hit.",
                    type = SkillType.Projectile,
                    baseDamage = 150f, cooldown = 4f, manaCost = 40f,
                    range = 12f, projectileSpeed = 14f, isMagicDamage = true,
                    piercing = ProjectilePiercing.StopOnFirst,
                    visuals = new SkillVisuals
                    {
                        primaryColor = new Color(0.7f, 0f, 0.9f),
                        scale = 0.35f,
                        hasTrail = true,
                        trailColor = new Color(0.5f, 0f, 0.6f, 0.6f),
                        trailWidth = 0.12f,
                        particleColor = new Color(0.6f, 0f, 0.8f),
                        particleCount = 5, particleSize = 0.1f, particleForce = 4f,
                        aimColor = new Color(0.7f, 0.2f, 1f, 0.6f)
                    }
                },
                new SkillDefinition
                {
                    skillName = "Viral Surge",
                    description = "Dash forward, dealing damage to enemies at the destination.",
                    type = SkillType.Dash,
                    baseDamage = 60f, cooldown = 10f, manaCost = 50f,
                    dashDistance = 7f, dashSpeed = 22f, isMagicDamage = false,
                    visuals = new SkillVisuals
                    {
                        primaryColor = new Color(0.9f, 0.2f, 0.1f),
                        hasTrail = true,
                        trailColor = new Color(0.8f, 0.1f, 0f, 0.5f),
                        trailWidth = 0.2f,
                        particleColor = new Color(1f, 0.4f, 0.1f),
                        particleCount = 6, particleSize = 0.14f, particleForce = 5f,
                        aimColor = new Color(1f, 0.3f, 0.1f, 0.6f)
                    }
                },
                new SkillDefinition
                {
                    skillName = "Infection Aura",
                    description = "Release a burst of infection, damaging all nearby enemies.",
                    type = SkillType.AreaOfEffect,
                    baseDamage = 100f, cooldown = 12f, manaCost = 60f,
                    aoeRadius = 5f, isMagicDamage = true,
                    visuals = new SkillVisuals
                    {
                        primaryColor = new Color(0.4f, 0.8f, 0f),
                        particleColor = new Color(0.5f, 0.9f, 0.1f),
                        particleCount = 8, particleSize = 0.1f, particleForce = 3f,
                        aimColor = new Color(0.4f, 0.9f, 0.2f, 0.5f)
                    }
                },
                new SkillDefinition
                {
                    skillName = "Mutation",
                    description = "Mutate your cells — temporarily boosting attack and speed.",
                    type = SkillType.SelfBuff,
                    cooldown = 60f, manaCost = 100f, isMagicDamage = false,
                    buffAttackDamage = 25f, buffMoveSpeed = 3f, buffDuration = 8f,
                    visuals = new SkillVisuals
                    {
                        primaryColor = new Color(1f, 0.3f, 0f),
                        particleColor = new Color(1f, 0.5f, 0f),
                        particleCount = 10, particleSize = 0.08f, particleForce = 2f
                    }
                }
            };
        }

        private SkillDefinition[] GetImmuneSkills()
        {
            return new SkillDefinition[]
            {
                new SkillDefinition
                {
                    skillName = "Antibody Shot",
                    description = "Fire an antibody projectile that damages the first enemy hit.",
                    type = SkillType.Projectile,
                    baseDamage = 150f, cooldown = 4f, manaCost = 35f,
                    range = 13f, projectileSpeed = 16f, isMagicDamage = true,
                    piercing = ProjectilePiercing.PierceMinions,
                    visuals = new SkillVisuals
                    {
                        primaryColor = new Color(0.2f, 0.7f, 1f),
                        scale = 0.3f,
                        hasTrail = true,
                        trailColor = new Color(0.1f, 0.5f, 0.9f, 0.6f),
                        trailWidth = 0.1f,
                        particleColor = new Color(0.3f, 0.8f, 1f),
                        particleCount = 4, particleSize = 0.1f, particleForce = 3.5f,
                        aimColor = new Color(0.2f, 0.7f, 1f, 0.6f)
                    }
                },
                new SkillDefinition
                {
                    skillName = "Rapid Response",
                    description = "Dash toward the threat, dealing damage on arrival.",
                    type = SkillType.Dash,
                    baseDamage = 55f, cooldown = 10f, manaCost = 45f,
                    dashDistance = 6f, dashSpeed = 20f, isMagicDamage = false,
                    visuals = new SkillVisuals
                    {
                        primaryColor = new Color(0f, 0.9f, 0.5f),
                        hasTrail = true,
                        trailColor = new Color(0f, 0.7f, 0.4f, 0.5f),
                        trailWidth = 0.18f,
                        particleColor = new Color(0.2f, 1f, 0.6f),
                        particleCount = 5, particleSize = 0.12f, particleForce = 4f,
                        aimColor = new Color(0f, 0.9f, 0.5f, 0.6f)
                    }
                },
                new SkillDefinition
                {
                    skillName = "Purify Wave",
                    description = "Emit a purifying wave that damages nearby enemies.",
                    type = SkillType.AreaOfEffect,
                    baseDamage = 90f, cooldown = 12f, manaCost = 55f,
                    aoeRadius = 5f, isMagicDamage = true,
                    visuals = new SkillVisuals
                    {
                        primaryColor = new Color(0.3f, 0.5f, 1f),
                        particleColor = new Color(0.4f, 0.6f, 1f),
                        particleCount = 7, particleSize = 0.1f, particleForce = 3f,
                        aimColor = new Color(0.3f, 0.5f, 1f, 0.5f)
                    }
                },
                new SkillDefinition
                {
                    skillName = "Immune Response",
                    description = "Activate immune overdrive — boost attack and movement.",
                    type = SkillType.SelfBuff,
                    cooldown = 60f, manaCost = 100f, isMagicDamage = false,
                    buffAttackDamage = 20f, buffMoveSpeed = 4f, buffDuration = 8f,
                    visuals = new SkillVisuals
                    {
                        primaryColor = new Color(1f, 1f, 0.3f),
                        particleColor = new Color(1f, 0.9f, 0.4f),
                        particleCount = 10, particleSize = 0.08f, particleForce = 2f
                    }
                }
            };
        }

        // ─── MINION SPAWNERS ────────────────────────────────────────────

        private void SetupMinionSpawners()
        {
            float half = laneLength * 0.5f;

            // Virus minions: spawn behind virus structures, march right toward immune side
            var virusSpawner = new GameObject("VirusMinionSpawner");
            virusSpawner.transform.position = new Vector3(-half * 0.85f, 0.3f, 0f);
            var vs = virusSpawner.AddComponent<MinionSpawner>();
            vs.team = Team.Virus;
            vs.marchDirection = 1f; // March in +X direction
            vs.waypoints = new Vector3[]
            {
                new Vector3(-half * 0.5f, 0.3f, 0f),
                new Vector3(-half * 0.2f, 0.3f, 0f),
                new Vector3(0f, 0.3f, 0f),
                new Vector3(half * 0.2f, 0.3f, 0f),
                new Vector3(half * 0.5f, 0.3f, 0f),
                new Vector3(half * 0.85f, 0.3f, 0f),
            };

            // Immune minions: spawn behind immune structures, march left toward virus side
            var immuneSpawner = new GameObject("ImmuneMinionSpawner");
            immuneSpawner.transform.position = new Vector3(half * 0.85f, 0.3f, 0f);
            var imm = immuneSpawner.AddComponent<MinionSpawner>();
            imm.team = Team.Immune;
            imm.marchDirection = -1f; // March in -X direction
            imm.waypoints = new Vector3[]
            {
                new Vector3(half * 0.5f, 0.3f, 0f),
                new Vector3(half * 0.2f, 0.3f, 0f),
                new Vector3(0f, 0.3f, 0f),
                new Vector3(-half * 0.2f, 0.3f, 0f),
                new Vector3(-half * 0.5f, 0.3f, 0f),
                new Vector3(-half * 0.85f, 0.3f, 0f),
            };
        }

        // ─── CAMERA ─────────────────────────────────────────────────────

        private void SetupCamera()
        {
            // Remove default camera if exists
            var existingCam = Camera.main;
            if (existingCam != null)
                Destroy(existingCam.gameObject);

            var camGO = new GameObject("MOBACamera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.08f, 0.12f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 100f;
            camGO.AddComponent<AudioListener>();

            var camCtrl = camGO.AddComponent<CameraController>();
            camCtrl.target = playerChampion.transform;
            camCtrl.SetMode(startingCameraMode);
        }

        // ─── UI ─────────────────────────────────────────────────────────

        private void SetupUI()
        {
            // Canvas
            var canvasGO = new GameObject("HUDCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem — ensure it uses new Input System, not legacy
            var existingES = FindAnyObjectByType<EventSystem>();
            if (existingES == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<InputSystemUIInputModule>();
            }
            else
            {
                // Replace legacy StandaloneInputModule if present
                var legacy = existingES.GetComponent<StandaloneInputModule>();
                if (legacy != null) Destroy(legacy);
                if (existingES.GetComponent<InputSystemUIInputModule>() == null)
                    existingES.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            var hud = canvasGO.AddComponent<HUDManager>();
            hud.playerChampion = playerChampion;

            // ── Human Health Bar (top center) ──
            var humanHealthBG = CreateUIImage(canvasGO.transform, "HumanHealthBG",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f),
                new Vector2(400f, 30f), new Color(0.15f, 0.15f, 0.15f, 0.9f));

            var humanHealthFill = CreateUIImage(humanHealthBG.transform, "HumanHealthFill",
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f),
                new Vector2(400f, 30f), Color.yellow);
            humanHealthFill.GetComponent<Image>().type = Image.Type.Filled;
            humanHealthFill.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
            humanHealthFill.GetComponent<Image>().fillAmount = 0.5f;
            hud.humanHealthFill = humanHealthFill.GetComponent<Image>();

            var humanHealthText = CreateUIText(humanHealthBG.transform, "HumanHealthText",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(400f, 30f), "HOST: 50% — NORMAL", 14, Color.white);
            hud.humanHealthText = humanHealthText;

            // ── Player Health Bar (bottom center-left) ──
            var healthBG = CreateUIImage(canvasGO.transform, "HealthBarBG",
                new Vector2(0.35f, 0f), new Vector2(0.35f, 0f), new Vector2(0f, 60f),
                new Vector2(200f, 18f), new Color(0.3f, 0.1f, 0.1f, 0.9f));

            var healthFill = CreateUIImage(healthBG.transform, "HealthFill",
                new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero,
                new Vector2(200f, 18f), new Color(0.2f, 0.8f, 0.2f));
            healthFill.GetComponent<Image>().type = Image.Type.Filled;
            healthFill.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
            hud.healthBarFill = healthFill.GetComponent<Image>();

            // ── Player Mana Bar ──
            var manaBG = CreateUIImage(canvasGO.transform, "ManaBarBG",
                new Vector2(0.35f, 0f), new Vector2(0.35f, 0f), new Vector2(0f, 38f),
                new Vector2(200f, 14f), new Color(0.1f, 0.1f, 0.3f, 0.9f));

            var manaFill = CreateUIImage(manaBG.transform, "ManaFill",
                new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero,
                new Vector2(200f, 14f), new Color(0.3f, 0.4f, 0.9f));
            manaFill.GetComponent<Image>().type = Image.Type.Filled;
            manaFill.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
            hud.manaBarFill = manaFill.GetComponent<Image>();

            // ── Level + Bio-currency (top left) ──
            hud.levelText = CreateUIText(canvasGO.transform, "LevelText",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f),
                new Vector2(100f, 30f), "Lv.1", 18, Color.white);

            hud.bioCurrencyText = CreateUIText(canvasGO.transform, "BioText",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -50f),
                new Vector2(150f, 25f), "0", 16, new Color(1f, 0.85f, 0.2f));

            // ── Skill Buttons (bottom right) ──
            var skillNames = new string[] { "Q", "W", "E", "R" };
            hud.skillButtons = new Button[4];
            hud.skillCooldownTexts = new Text[4];

            for (int i = 0; i < 4; i++)
            {
                float xOffset = -260f + i * 65f;
                var btnGO = CreateUIImage(canvasGO.transform, $"SkillBtn_{skillNames[i]}",
                    new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(xOffset, 50f),
                    new Vector2(55f, 55f), new Color(0.3f, 0.3f, 0.3f, 0.9f));

                var btn = btnGO.AddComponent<Button>();
                hud.skillButtons[i] = btn;

                var txt = CreateUIText(btnGO.transform, "Label",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                    new Vector2(55f, 55f), skillNames[i], 14, Color.white);
                hud.skillCooldownTexts[i] = txt;
            }

            // ── Shop Button ──
            var shopBtnGO = CreateUIImage(canvasGO.transform, "ShopButton",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f),
                new Vector2(80f, 35f), new Color(0.6f, 0.5f, 0.1f, 0.9f));
            hud.shopButton = shopBtnGO.AddComponent<Button>();
            CreateUIText(shopBtnGO.transform, "ShopLabel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(80f, 35f), "MUTATE", 12, Color.white);

            // ── Camera Mode Button ──
            var camBtnGO = CreateUIImage(canvasGO.transform, "CamModeButton",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-110f, -20f),
                new Vector2(80f, 35f), new Color(0.3f, 0.3f, 0.5f, 0.9f));
            hud.cameraModeButton = camBtnGO.AddComponent<Button>();
            CreateUIText(camBtnGO.transform, "CamLabel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(80f, 35f), "CAM [C]", 11, Color.white);

            // ── Joystick (bottom left, circular) ──
            var joystickBG = CreateUIImage(canvasGO.transform, "JoystickBG",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(110f, 110f),
                new Vector2(160f, 160f), new Color(1f, 1f, 1f, 0.1f));
            MakeCircular(joystickBG);

            var joystickHandle = CreateUIImage(joystickBG.transform, "JoystickHandle",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(55f, 55f), new Color(1f, 1f, 1f, 0.45f));
            MakeCircular(joystickHandle);

            var joystick = joystickBG.AddComponent<VirtualJoystick>();
            joystick.background = joystickBG.GetComponent<RectTransform>();
            joystick.handle = joystickHandle.GetComponent<RectTransform>();
            joystick.handleRange = 55f;

            // Wire joystick to player controller
            var pc = playerChampion.GetComponent<PlayerController>();
            if (pc != null) pc.moveJoystick = joystick;

            // ── Shop Panel (hidden by default) ──
            var shopPanel = CreateUIImage(canvasGO.transform, "ShopPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(500f, 400f), new Color(0.1f, 0.1f, 0.15f, 0.95f));
            shopPanel.SetActive(false);
            hud.shopPanel = shopPanel;

            CreateUIText(shopPanel.transform, "ShopTitle",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -15f),
                new Vector2(300f, 40f), "MUTATION SHOP", 20, new Color(1f, 0.85f, 0.2f));

            CreateUIText(shopPanel.transform, "ShopHint",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(400f, 200f), "Skill upgrades coming soon.\nPress [B] to close.", 14, Color.gray);

            // ── Game Over Panel ──
            var gameOverPanel = CreateUIImage(canvasGO.transform, "GameOverPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(500f, 300f), new Color(0f, 0f, 0f, 0.9f));
            gameOverPanel.SetActive(false);
            hud.gameOverPanel = gameOverPanel;

            hud.gameOverText = CreateUIText(gameOverPanel.transform, "GameOverText",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(400f, 200f), "GAME OVER", 30, Color.white);
        }

        // ─── UI HELPERS ─────────────────────────────────────────────────

        private GameObject CreateUIImage(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos,
            Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            rt.pivot = anchorMin; // pivot matches anchor for easy positioning

            var img = go.AddComponent<Image>();
            img.color = color;

            return go;
        }

        private Text CreateUIText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos,
            Vector2 size, string text, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            rt.pivot = new Vector2(0.5f, 0.5f);

            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = GameBootstrap.UIFont;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;

            return txt;
        }
        // Cache the circle sprite so we only generate it once
        private static Sprite circleSprite;

        private void MakeCircular(GameObject go)
        {
            if (circleSprite == null)
                circleSprite = CreateCircleSprite(64);

            var img = go.GetComponent<Image>();
            if (img != null)
                img.sprite = circleSprite;
        }

        private static Sprite CreateCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size * 0.5f;
            Color clear = new Color(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                    tex.SetPixel(x, y, dist <= radius ? Color.white : clear);
                }
            }

            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
