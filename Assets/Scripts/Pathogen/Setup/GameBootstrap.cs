using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

        /// <summary>
        /// Single font used by all UI text in the game. Created once here at startup.
        /// </summary>
        public static Font UIFont { get; private set; }
#if UNITY_EDITOR
        public static bool IsMobile => UnityEngine.Device.Application.isMobilePlatform;
#else
        public static bool IsMobile => Application.isMobilePlatform;
#endif

        private Champion playerChampion;
        private Champion aiChampion;

        void Awake()
        {
            // Lock to landscape on mobile (no-op on desktop)
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;

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
            DestroyImmediate(ground.GetComponent<BoxCollider>());
            ground.isStatic = true;

            var lanePath = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lanePath.name = "LanePath";
            lanePath.transform.position = new Vector3(0f, 0.01f, 0f);
            lanePath.transform.localScale = new Vector3(laneLength, 0.05f, 4f);
            lanePath.GetComponent<Renderer>().material.color = new Color(0.35f, 0.25f, 0.30f);
            DestroyImmediate(lanePath.GetComponent<BoxCollider>());
            lanePath.isStatic = true;

            var virusSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            virusSide.name = "VirusSideMarker";
            virusSide.transform.position = new Vector3(-laneLength * 0.25f, 0.02f, 0f);
            virusSide.transform.localScale = new Vector3(laneLength * 0.5f, 0.03f, groundWidth - 2f);
            virusSide.GetComponent<Renderer>().material.color = new Color(0.3f, 0.15f, 0.15f, 0.5f);
            DestroyImmediate(virusSide.GetComponent<BoxCollider>());
            virusSide.isStatic = true;

            var immuneSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            immuneSide.name = "ImmuneSideMarker";
            immuneSide.transform.position = new Vector3(laneLength * 0.25f, 0.02f, 0f);
            immuneSide.transform.localScale = new Vector3(laneLength * 0.5f, 0.03f, groundWidth - 2f);
            immuneSide.GetComponent<Renderer>().material.color = new Color(0.15f, 0.2f, 0.3f, 0.5f);
            DestroyImmediate(immuneSide.GetComponent<BoxCollider>());
            immuneSide.isStatic = true;
        }

        // ─── STRUCTURES ─────────────────────────────────────────────────

        private void SetupStructures()
        {
            float half = laneLength * 0.5f;

            // Virus structures (Infection Nodes)
            CreateStructure("InfectionNode_1", Team.Virus, new Vector3(-half * 0.4f, 0f, -3f),
                           1500f, 80f, 10f, new Color(0.8f, 0.15f, 0.15f));
            CreateStructure("InfectionNode_2", Team.Virus, new Vector3(-half * 0.75f, 0f, -3f),
                           2000f, 100f, 12f, new Color(0.6f, 0.1f, 0.1f));

            // Immune structures (Sentinels)
            CreateStructure("Sentinel_1", Team.Immune, new Vector3(half * 0.4f, 0f, -3f),
                           1500f, 80f, 10f, new Color(0.15f, 0.4f, 0.8f));
            CreateStructure("Sentinel_2", Team.Immune, new Vector3(half * 0.75f, 0f, -3f),
                           2000f, 100f, 12f, new Color(0.1f, 0.3f, 0.6f));
        }

        private void CreateStructure(string name, Team team, Vector3 position,
                                     float health, float damage, float attackRange, Color color)
        {
            var structGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            structGO.name = name;
            structGO.transform.position = position;
            structGO.transform.localScale = new Vector3(1.2f, 3f, 1.2f);
            structGO.GetComponent<Renderer>().material.color = color;

            // Trigger collider so minions can walk through (structures detect via range, not physics)
            structGO.GetComponent<BoxCollider>().isTrigger = true;

            var rb = structGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Solid child collider for blocking movement
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

            var playerCtrl = PlayerController.Create(playerChampion.gameObject);

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
            camCtrl.mapFlipped = playerChampion.transform.position.x > 0f;
        }

        // ─── UI ─────────────────────────────────────────────────────────

        private void SetupUI()
        {
            // Canvas
            var canvasGO = new GameObject("HUDCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvas.pixelPerfect = true;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100;
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

            // ── Champion Stats (world-space, follows each champion) ──
            CreateChampionWorldStats(playerChampion);
            CreateChampionWorldStats(aiChampion);

            // Bio-currency (top left of screen HUD)
            hud.bioCurrencyText = CreateUIText(canvasGO.transform, "BioText",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f),
                new Vector2(150f, 25f), "0", 16, new Color(1f, 0.85f, 0.2f));

            // ── Skill Buttons ──
            var skillNames = new string[] { "Q", "W", "E", "R" };
            hud.skillButtons = new Button[4];
            hud.skillCooldownTexts = new Text[4];
            var playerInputRef = playerChampion.GetComponent<PlayerController>();

            float arcCenterX = -(AutoAttackButton.MarginRight + AutoAttackButton.BigButtonSize * 0.5f);
            float arcCenterY = AutoAttackButton.MarginBottom + AutoAttackButton.BigButtonSize * 0.5f;
            float arcRadius = AutoAttackButton.BigButtonSize * 0.5f + AutoAttackButton.SmallButtonSize
                + AutoAttackButton.ButtonGap + 80f;
            float[] arcAngles = { 180f, 150f, 120f, 90f };

            for (int i = 0; i < 4; i++)
            {
                Vector2 pos;
                if (IsMobile)
                {
                    float rad = arcAngles[i] * Mathf.Deg2Rad;
                    pos = new Vector2(
                        arcCenterX + Mathf.Cos(rad) * arcRadius,
                        arcCenterY + Mathf.Sin(rad) * arcRadius);
                }
                else
                {
                    pos = new Vector2(-133f + i * 89f, 50f);
                }

                var btnGO = CreateButton(canvasGO.transform, $"SkillBtn_{skillNames[i]}",
                    new Vector2(IsMobile ? 1f : 0.5f, 0f), pos,
                    IsMobile ? 110f : 65f, new Color(0.3f, 0.3f, 0.3f, 0.9f), skillNames[i], IsMobile, IsMobile);

                var btn = btnGO.AddComponent<Button>();
                hud.skillButtons[i] = btn;

                var skillBtn = btnGO.AddComponent<SkillButton>();
                skillBtn.skillIndex = i;
                skillBtn.playerController = playerInputRef;

                hud.skillCooldownTexts[i] = btnGO.GetComponentInChildren<Text>();
            }

            // ── Shop Button ──
            var shopBtnGO = CreateUIImage(canvasGO.transform, "ShopButton",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f),
                new Vector2(80f, 35f), new Color(0.6f, 0.5f, 0.1f, 0.9f));
            hud.shopButton = shopBtnGO.AddComponent<Button>();
            CreateUIText(shopBtnGO.transform, "ShopLabel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(80f, 35f), "MUTATE", 12, Color.white);

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
            joystick.handle = joystickHandle.GetComponent<RectTransform>();
            joystick.handleRange = 55f;

            var playerInput = playerChampion.GetComponent<PlayerController>();
            if (playerInput != null) playerInput.moveJoystick = joystick;

            // ── Auto-Attack Buttons (mobile only) ──
            if (IsMobile)
            {
                float attackButtonMarginRight = AutoAttackButton.MarginRight;
                float attackButtonMarginBottom = AutoAttackButton.MarginBottom;
                float bigBtn = AutoAttackButton.BigButtonSize;
                float smallBtn = AutoAttackButton.SmallButtonSize;
                float btnGap = AutoAttackButton.ButtonGap;

                Color btnFill = new Color(0.3f, 0.5f, 0.8f, 0.3f);

                var champBtn = CreateButton(canvasGO.transform, "ChampionAttackBtn",
                    new Vector2(1f, 0f),
                    new Vector2(-(attackButtonMarginRight + bigBtn * 0.5f), attackButtonMarginBottom + bigBtn * 0.5f),
                    bigBtn, btnFill, "ATK", true, true);
                var champAtk = champBtn.AddComponent<AutoAttackButton>();
                champAtk.targetType = AttackTargetType.Champion;
                champAtk.playerController = playerInput;
                champAtk.activeRing = CreateActiveRing(champBtn.transform, bigBtn);
                champAtk.content = champBtn.transform.Find("Label").gameObject;

                var minionBtn = CreateButton(canvasGO.transform, "MinionAttackBtn",
                    new Vector2(1f, 0f),
                    new Vector2(-(attackButtonMarginRight + bigBtn + btnGap + smallBtn * 0.5f), attackButtonMarginBottom + bigBtn * 0.5f),
                    smallBtn, btnFill, "M", true, true);
                var minionAtk = minionBtn.AddComponent<AutoAttackButton>();
                minionAtk.targetType = AttackTargetType.Minion;
                minionAtk.playerController = playerInput;
                minionAtk.activeRing = CreateActiveRing(minionBtn.transform, smallBtn);
                minionAtk.content = minionBtn.transform.Find("Label").gameObject;

                float structY = attackButtonMarginBottom + bigBtn + btnGap + smallBtn * 0.5f;
                var structBtn = CreateButton(canvasGO.transform, "StructureAttackBtn",
                    new Vector2(1f, 0f),
                    new Vector2(-(attackButtonMarginRight + bigBtn * 0.5f), structY),
                    smallBtn, btnFill, "S", true, true);
                var structAtk = structBtn.AddComponent<AutoAttackButton>();
                structAtk.targetType = AttackTargetType.Structure;
                structAtk.playerController = playerInput;
                structAtk.activeRing = CreateActiveRing(structBtn.transform, smallBtn);
                structAtk.content = structBtn.transform.Find("Label").gameObject;

                float cancelY = structY + smallBtn * 0.5f + 120f + smallBtn * 0.3f;
                var cancelBtn = CreateButton(canvasGO.transform, "SkillCancelBtn",
                    new Vector2(1f, 0f),
                    new Vector2(-(attackButtonMarginRight + bigBtn * 0.5f), cancelY),
                    smallBtn * 0.75f, btnFill, "X", true, true);

                var mobileCtrl = playerInput as MobilePlayerController;
                if (mobileCtrl != null)
                    mobileCtrl.SetCancelButton(cancelBtn);
            }

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

            // ── Death Overlay + Respawn Countdown ──
            // Full-screen semi-transparent black overlay
            var respawnPanel = new GameObject("RespawnPanel", typeof(RectTransform));
            respawnPanel.transform.SetParent(canvasGO.transform, false);
            var rpRT = respawnPanel.GetComponent<RectTransform>();
            rpRT.anchorMin = Vector2.zero;
            rpRT.anchorMax = Vector2.one;
            rpRT.offsetMin = Vector2.zero;
            rpRT.offsetMax = Vector2.zero;
            var rpImg = respawnPanel.AddComponent<Image>();
            rpImg.color = new Color(0f, 0f, 0f, 0.5f);
            respawnPanel.SetActive(false);
            hud.respawnPanel = respawnPanel;

            // Respawn text — centered on screen, below the health bar area
            hud.respawnCountdownText = CreateUIText(respawnPanel.transform, "RespawnText",
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero,
                new Vector2(400f, 50f), "", 24, new Color(1f, 0.4f, 0.4f));
            hud.respawnCountdownText.fontStyle = FontStyle.Bold;
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

        private GameObject CreateButton(Transform parent, string name,
            Vector2 anchor, Vector2 anchoredPos, float size, Color color, string label,
            bool circular = true, bool showBorder = false)
        {
            var go = CreateUIImage(parent, name, anchor, anchor, anchoredPos,
                new Vector2(size, size), color);
            go.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            if (circular) MakeCircular(go);

            if (showBorder)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.25f);
                outline.effectDistance = new Vector2(2f, 2f);
            }

            go.AddComponent<ButtonPressFeedback>();

            CreateUIText(go.transform, "Label",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(size, size), label, (int)(size * 0.25f), Color.white);

            return go;
        }

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

        private Sprite ringSprite;

        private Sprite GetRingSprite()
        {
            if (ringSprite != null) return ringSprite;

            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size * 0.5f;
            float innerRadius = radius - 8f;
            Color clear = new Color(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                    tex.SetPixel(x, y, dist <= radius && dist >= innerRadius ? Color.white : clear);
                }
            }

            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return ringSprite;
        }

        private GameObject CreateActiveRing(Transform parent, float buttonSize)
        {
            float ringSize = buttonSize + AutoAttackButton.ActiveRingPadding * 2f;
            var go = new GameObject("ActiveRing", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(ringSize, ringSize);

            var img = go.AddComponent<Image>();
            img.sprite = GetRingSprite();
            img.color = Color.white;
            img.raycastTarget = false;

            go.SetActive(false);
            return go;
        }
        private ChampionStats CreateChampionWorldStats(Champion champ)
        {
            var container = new GameObject("ChampionStats");
            container.transform.SetParent(champ.transform, false);
            float statsY = champ.championHeight + 1.2f;
            container.transform.localPosition = new Vector3(0f, statsY, 0f);

            var canvas = container.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 50;
            var rt = container.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1.5f, 0.8f);
            rt.localScale = Vector3.one * 0.015f;

            var cg = container.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Level circle with XP ring stroke (left side, vertically centered)
            float lvlSize = 40f;
            float barWidth = 80f;
            float groupGap = 6f;
            float groupWidth = lvlSize + groupGap + barWidth;
            float groupStartX = -groupWidth * 0.5f;

            var lvlContainer = CreateUIImage(container.transform, "LevelContainer",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(groupStartX + lvlSize * 0.5f, 0f),
                new Vector2(lvlSize, lvlSize), new Color(0.1f, 0.1f, 0.15f, 0.8f));
            lvlContainer.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            if (circleSprite == null) circleSprite = CreateCircleSprite(64);
            lvlContainer.GetComponent<Image>().sprite = circleSprite;

            var xpRingGO = CreateUIImage(lvlContainer.transform, "XPRing",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(lvlSize + 9f, lvlSize + 9f), new Color(0.3f, 0.8f, 1f, 0.9f));
            xpRingGO.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            var xpRingImg = xpRingGO.GetComponent<Image>();
            xpRingImg.type = Image.Type.Filled;
            xpRingImg.fillMethod = Image.FillMethod.Radial360;
            xpRingImg.fillOrigin = (int)Image.Origin360.Top;
            xpRingImg.fillAmount = 0f;
            xpRingImg.raycastTarget = false;
            xpRingImg.sprite = GetRingSprite();

            var lvlText = CreateUIText(lvlContainer.transform, "LevelText",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(lvlSize, lvlSize), "1", 23, Color.white);
            lvlText.fontStyle = FontStyle.Bold;
            lvlText.alignment = TextAnchor.MiddleCenter;

            float healthBarHeight = 12f;
            float manaBarHeight = 12f;
            float barGap = 1f;
            float barsTopY = (healthBarHeight + barGap + manaBarHeight) * 0.5f;
            float barsLeftX = groupStartX + lvlSize + groupGap;

            // Health bar (top)
            var healthBG = CreateUIImage(container.transform, "HealthBarBG",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(barsLeftX, barsTopY),
                new Vector2(barWidth, healthBarHeight), new Color(0.2f, 0.2f, 0.2f, 0.9f));
            healthBG.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            var healthTrailGO = CreateUIImage(healthBG.transform, "HealthTrail",
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero,
                Vector2.zero, new Color(1f, 0.2f, 0.1f, 0.6f));

            var healthFillGO = CreateUIImage(healthBG.transform, "HealthFill",
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero,
                Vector2.zero, new Color(0.2f, 0.8f, 0.2f));

            // Mana bar (below health)
            var manaBG = CreateUIImage(container.transform, "ManaBarBG",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(barsLeftX, barsTopY - healthBarHeight - barGap),
                new Vector2(barWidth, manaBarHeight), new Color(0.1f, 0.1f, 0.2f, 0.9f));
            manaBG.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            var manaTrailGO = CreateUIImage(manaBG.transform, "ManaTrail",
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero,
                Vector2.zero, new Color(1f, 0.2f, 0.1f, 0.6f));

            var manaFillGO = CreateUIImage(manaBG.transform, "ManaFill",
                new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero,
                Vector2.zero, new Color(0.3f, 0.4f, 0.9f));

            var stats = container.AddComponent<ChampionStats>();
            stats.champion = champ;
            stats.healthFill = healthFillGO.GetComponent<RectTransform>();
            stats.healthTrail = healthTrailGO.GetComponent<RectTransform>();
            stats.manaFill = manaFillGO.GetComponent<RectTransform>();
            stats.manaTrail = manaTrailGO.GetComponent<RectTransform>();
            stats.levelText = lvlText;
            stats.xpRing = xpRingImg;
            champ.Stats = stats;

            return stats;
        }
    }
}
