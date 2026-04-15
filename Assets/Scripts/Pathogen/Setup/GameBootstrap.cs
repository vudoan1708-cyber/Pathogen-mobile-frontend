using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

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
        public static TMP_FontAsset UIFont { get; private set; }
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

            UIFont = TMP_Settings.defaultFontAsset;

            SetupGameManager();
            SetupLighting();
            SetupGround();
            SetupStructures();
            SetupChampions();
            SetupBases();
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
                           1500f, 80f, new Color(0.8f, 0.15f, 0.15f));
            CreateStructure("InfectionNode_2", Team.Virus, new Vector3(-half * 0.75f, 0f, -3f),
                           2000f, 100f, new Color(0.6f, 0.1f, 0.1f));

            // Immune structures (Sentinels)
            CreateStructure("Sentinel_1", Team.Immune, new Vector3(half * 0.4f, 0f, -3f),
                           1500f, 80f, new Color(0.15f, 0.4f, 0.8f));
            CreateStructure("Sentinel_2", Team.Immune, new Vector3(half * 0.75f, 0f, -3f),
                           2000f, 100f, new Color(0.1f, 0.3f, 0.6f));
        }

        private void CreateStructure(string name, Team team, Vector3 position,
                                     float health, float damage, Color color)
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
            structure.attackSpeed = 0.88f;
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

            // --- PLAYER ---
            var playerDef = ChampionRoster.Get("Immunix");
            var playerPos = new Vector3(half * 0.85f, 0.5f, 0f);
            playerChampion = CreateChampion(playerDef, Team.Immune, playerPos);

            PlayerController.Create(playerChampion.gameObject);

            var cc = playerChampion.gameObject.AddComponent<CharacterController>();
            cc.height = 1f;
            cc.radius = 0.4f;
            cc.center = Vector3.zero;

            // --- AI ---
            var aiDef = ChampionRoster.Get("Pathobyte");
            var aiPos = new Vector3(-half * 0.85f, 0.5f, 0f);
            aiChampion = CreateChampion(aiDef, Team.Virus, aiPos);

            var aiCtrl = aiChampion.gameObject.AddComponent<AIController>();
            aiCtrl.patrolPoints = new Vector3[]
            {
                new Vector3(-half * 0.85f, 0.5f, 0f),
                new Vector3(-half * 0.5f, 0.5f, 0f),
                new Vector3(-half * 0.2f, 0.5f, 0f),
                new Vector3(0f, 0.5f, 0f),
            };
        }

        private Champion CreateChampion(ChampionDefinition def, Team team, Vector3 position)
        {
            var champGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            champGO.name = def.championName;
            champGO.transform.position = position;
            champGO.transform.localScale = Vector3.one;
            champGO.GetComponent<Renderer>().material.color = def.color;

            var rb = champGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var champ = champGO.AddComponent<Champion>();
            champ.team = team;
            champ.entityName = def.championName;
            champ.championName = def.championName;
            champ.maxHealth = def.maxHealth;
            champ.currentHealth = def.maxHealth;
            champ.maxMana = def.maxMana;
            champ.currentMana = def.maxMana;
            champ.attackDamage = def.attackDamage;
            champ.attackSpeed = def.attackSpeed;
            champ.attackRange = def.attackRange;
            champ.moveSpeed = def.moveSpeed;
            champ.armor = def.armor;
            champ.magicResist = def.magicResist;
            champ.healthRegen = def.healthRegen;
            champ.manaRegen = def.manaRegen;
            champ.sightRange = def.sightRange;
            champ.healthPerLevel = def.healthPerLevel;
            champ.manaPerLevel = def.manaPerLevel;
            champ.attackDamagePerLevel = def.attackDamagePerLevel;
            champ.armorPerLevel = def.armorPerLevel;
            champ.magicResistPerLevel = def.magicResistPerLevel;
            champ.spawnPoint = position;
            champ.InitializeSkills(def.skills);

            champGO.AddComponent<TargetHighlight>();

            return champ;
        }

        // ─── BASES ──────────────────────────────────────────────────────

        private void SetupBases()
        {
            float half = laneLength * 0.5f;

            CreateBase("VirusBase", Team.Virus, new Vector3(-half * 0.85f, 0f, 0f));
            CreateBase("ImmuneBase", Team.Immune, new Vector3(half * 0.85f, 0f, 0f));
        }

        private void CreateBase(string name, Team team, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;

            var teamBase = go.AddComponent<Base>();
            teamBase.team = team;
            teamBase.range = 5f;
            teamBase.Show();
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
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
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

            // ── Human Health Bar ──
            HumanHealthBar.Create(canvasGO.transform);

            // ── Champion Stats (world-space, follows each champion) ──
            playerChampion.Stats = ChampionStats.Create(playerChampion.transform, playerChampion, playerChampion.championHeight);
            aiChampion.Stats = ChampionStats.Create(aiChampion.transform, aiChampion, aiChampion.championHeight);

            // Bio-currency (top left of screen HUD)
            hud.bioCurrencyText = CreateUIText(canvasGO.transform, "BioText",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f),
                new Vector2(150f, 25f), "0", 16, new Color(1f, 0.85f, 0.2f));

            // ── Skill Buttons (each with inline "+" upgrade button + flash overlay) ──
            var skillNames = new string[] { "Q", "W", "E", "R" };
            hud.skillButtons = new Button[4];
            hud.skillCooldownTexts = new TextMeshProUGUI[4];
            var playerInputRef = playerChampion.GetComponent<PlayerController>();
            var skillBtnComponents = new SkillButton[4];

            float arcCenterX = -(AutoAttackButton.MarginRight + AutoAttackButton.BigButtonSize * 0.5f);
            float arcCenterY = AutoAttackButton.MarginBottom + AutoAttackButton.BigButtonSize * 0.5f;
            float arcRadius = AutoAttackButton.BigButtonSize * 0.5f + AutoAttackButton.SmallButtonSize
                + AutoAttackButton.ButtonGap + 120f;
            float[] arcAngles = { 180f, 150f, 120f, 90f };

            float skillSize = IsMobile ? 126f : 84f;
            float upgSize = IsMobile ? 63f : 53f;
            var woodenMat = new Material(Shader.Find("Pathogen/UIGoldButton"));

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
                    skillSize, new Color(0.3f, 0.3f, 0.3f, 0.9f), skillNames[i], IsMobile, IsMobile);

                var btn = btnGO.AddComponent<Button>();
                hud.skillButtons[i] = btn;

                var skillBtn = btnGO.AddComponent<SkillButton>();
                skillBtn.skillIndex = i;
                skillBtn.playerController = playerInputRef;

                hud.skillCooldownTexts[i] = btnGO.GetComponentInChildren<TextMeshProUGUI>();

                // ── Flash overlay (full-size circle over the skill button, invisible by default)
                var flashGO = new GameObject("FlashOverlay", typeof(RectTransform));
                flashGO.transform.SetParent(btnGO.transform, false);
                var flashRT = flashGO.GetComponent<RectTransform>();
                flashRT.anchorMin = new Vector2(0.5f, 0.5f);
                flashRT.anchorMax = new Vector2(0.5f, 0.5f);
                flashRT.pivot = new Vector2(0.5f, 0.5f);
                flashRT.anchoredPosition = Vector2.zero;
                flashRT.sizeDelta = new Vector2(skillSize, skillSize);
                var flashImg = flashGO.AddComponent<Image>();
                flashImg.sprite = circleSprite;
                flashImg.color = Color.clear;
                flashImg.raycastTarget = false;
                skillBtn.flashOverlay = flashImg;

                // ── "+" upgrade button (gold shader, diagonal outward from skill button)
                var upgGO = new GameObject($"UpgradeBtn_{skillNames[i]}", typeof(RectTransform));
                upgGO.transform.SetParent(btnGO.transform, false);
                var upgRT = upgGO.GetComponent<RectTransform>();
                upgRT.anchorMin = new Vector2(0.5f, 0.5f);
                upgRT.anchorMax = new Vector2(0.5f, 0.5f);
                upgRT.pivot = new Vector2(0.5f, 0.5f);
                upgRT.sizeDelta = new Vector2(upgSize, upgSize);

                // Position at -45° (upper-left diagonal) from skill button centre
                float upgOffset = skillSize * 0.78f + 10f;
                upgRT.anchoredPosition = new Vector2(-0.707f, 0.707f) * upgOffset;

                var upgImg = upgGO.AddComponent<Image>();
                upgImg.material = woodenMat;      // shader handles rounded-rect shape + wooden gradient
                upgImg.color = Color.white;

                upgGO.AddComponent<CanvasGroup>();
                upgGO.AddComponent<Button>();

                var plusText = CreateUIText(upgGO.transform, "PlusLabel",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                    new Vector2(upgSize, upgSize), "+", (int)(upgSize * 0.55f), Color.black);
                plusText.fontStyle = FontStyles.Bold;

                skillBtn.upgradeButton = upgGO;
                skillBtnComponents[i] = skillBtn;
            }

            // ── Skill Level-Up coordinator (stagger animation + event wiring)
            var levelUpUI = canvasGO.AddComponent<SkillLevelUpUI>();
            levelUpUI.champion = playerChampion;
            levelUpUI.skillButtons = skillBtnComponents;
            hud.skillLevelUpUI = levelUpUI;

            // ── Shop Button ──
            var shopBtnGO = CreateUIImage(canvasGO.transform, "ShopButton",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f),
                new Vector2(80f, 35f), new Color(0.6f, 0.5f, 0.1f, 0.9f));
            hud.shopButton = shopBtnGO.AddComponent<Button>();
            CreateUIText(shopBtnGO.transform, "ShopLabel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(80f, 35f), "MUTATE", 12, Color.white);

            // ── Recall ──
            if (IsMobile)
            {
                float recallBtnSize = 126f;
                var recallBtnGO = CreateButton(canvasGO.transform, "RecallButton",
                    new Vector2(0.5f, 0f), new Vector2(-10f, 100f),
                    recallBtnSize, new Color(0.2f, 0.5f, 0.8f, 0.7f), "B", true, true);
                hud.recallButton = recallBtnGO.AddComponent<Button>();

                var recallBar = CreateUIImage(canvasGO.transform, "RecallProgressBar",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f),
                    new Vector2(200f, 16f), new Color(0.15f, 0.15f, 0.2f, 0.85f));
                recallBar.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                hud.recallProgressBar = recallBar;

                var recallFill = CreateUIImage(recallBar.transform, "RecallFill",
                    new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero,
                    new Vector2(200f, 0f), new Color(0.3f, 0.7f, 1f, 0.9f));
                var recallFillImg = recallFill.GetComponent<Image>();
                recallFillImg.type = Image.Type.Filled;
                recallFillImg.fillMethod = Image.FillMethod.Horizontal;
                recallFillImg.fillAmount = 0f;
                var fillRT = recallFill.GetComponent<RectTransform>();
                fillRT.anchorMin = Vector2.zero;
                fillRT.anchorMax = Vector2.one;
                fillRT.offsetMin = Vector2.zero;
                fillRT.offsetMax = Vector2.zero;
                fillRT.pivot = new Vector2(0.5f, 0.5f);
                hud.recallProgressFill = recallFillImg;

                hud.recallProgressText = CreateUIText(recallBar.transform, "RecallText",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 16f),
                    new Vector2(200f, 20f), "", 12, new Color(0.7f, 0.9f, 1f));

                recallBar.SetActive(false);
            }

            // ── Joystick (bottom left, circular) ──
            var joystickBG = CreateUIImage(canvasGO.transform, "JoystickBG",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(110f, 110f),
                new Vector2(239f, 239f), new Color(1f, 1f, 1f, 0.1f));
            joystickBG.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            MakeCircular(joystickBG);

            var joystickHandle = CreateUIImage(joystickBG.transform, "JoystickHandle",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(75f, 75f), new Color(1f, 1f, 1f, 0.45f));
            MakeCircular(joystickHandle);

            var joystick = joystickBG.AddComponent<VirtualJoystick>();
            joystick.handle = joystickHandle.GetComponent<RectTransform>();
            joystick.handleRange = 70f;

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

                float cancelY = structY + smallBtn * 0.5f + 200f + smallBtn * 0.3f;
                var cancelBtn = CreateButton(canvasGO.transform, "SkillCancelBtn",
                    new Vector2(1f, 0f),
                    new Vector2(-(attackButtonMarginRight + bigBtn * 0.5f), cancelY),
                    smallBtn, btnFill, "X", true, true);

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
            rpImg.raycastTarget = false;
            respawnPanel.SetActive(false);
            hud.respawnPanel = respawnPanel;

            // Respawn text — centered on screen, below the health bar area
            hud.respawnCountdownText = CreateUIText(respawnPanel.transform, "RespawnText",
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero,
                new Vector2(400f, 50f), "", 24, new Color(1f, 0.4f, 0.4f));
            hud.respawnCountdownText.fontStyle = FontStyles.Bold;
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

        private TextMeshProUGUI CreateUIText(Transform parent, string name,
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

            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAlignmentOptions.Center;
            txt.font = GameBootstrap.UIFont;
            txt.overflowMode = TextOverflowModes.Overflow;
            txt.textWrappingMode = TextWrappingModes.NoWrap;

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
                outline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.25f);
                outline.effectDistance = new Vector2(1f, 1f);
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
                circleSprite = CreateCircleSprite(256);

            var img = go.GetComponent<Image>();
            if (img != null)
                img.sprite = circleSprite;
        }

        public static Sprite CreateCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(radius - dist + 1f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private Sprite ringSprite;

        public Sprite GetRingSprite()
        {
            if (ringSprite != null) return ringSprite;

            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float outerRadius = center - 1f;
            float innerRadius = outerRadius - 8f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float outerAlpha = Mathf.Clamp01(outerRadius - dist + 1f);
                    float innerAlpha = Mathf.Clamp01(dist - innerRadius + 1f);
                    float alpha = Mathf.Min(outerAlpha, innerAlpha);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
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
            img.color = new Color(0.3f, 0.5f, 0.8f, 0.4f);
            img.raycastTarget = false;

            go.SetActive(false);
            return go;
        }
    }
}
