using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class StoryGameManager : MonoBehaviour
{
    const string DefaultStoryGraphPath = "Story/MainStoryGraph";
    const string DefaultPlayerProfilePath = "Combat/Profiles/DefaultPlayerCombatProfile";
    const string EnemyResourceRoot = "Combat/Enemies/";
    const string ItemResourceRoot = "Combat/Items/";
    const string MainMenuSceneName = "SampleScene";

    const string StoryCanvasName = "Story Canvas";
    const string BackdropName = "Story Backdrop";
    const string StoryPanelName = "Dialogue Panel";
    const string TitleName = "Dialogue Title";
    const string BodyName = "Dialogue Body";
    const string ChoiceRootName = "Choice Buttons";
    const string ChoiceTemplateName = "Choice Button Template";
    const string HudPanelName = "Player HUD";
    const string HudNameName = "Player Name";
    const string HudStatsName = "Player Stats";
    const string EndingPanelName = "Ending Panel";
    const string EndingTitleName = "Ending Title";
    const string EndingBodyName = "Ending Body";
    const string EndingActionsName = "Ending Actions";
    const string RestartButtonName = "Restart Button";
    const string MenuButtonName = "Menu Button";

    [Header("Combat Bridge")]
    [SerializeField] CombatEncounterController encounterController;
    [SerializeField] CombatPanelUI combatUI;
    [SerializeField] EnemyDefinition debugEnemy;

    [Header("Story Runtime")]
    [SerializeField] RuntimeStoryGraph storyGraph;
    [SerializeField] Player player;
    [SerializeField] PlayerCombatProfile playerProfile;

    [Header("Story UI")]
    [SerializeField] Canvas storyCanvas;
    [SerializeField] RectTransform backdrop;
    [SerializeField] RectTransform storyPanel;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI bodyText;
    [SerializeField] RectTransform choicesRoot;
    [SerializeField] Button choiceTemplate;
    [SerializeField] RectTransform hudPanel;
    [SerializeField] TextMeshProUGUI hudNameText;
    [SerializeField] TextMeshProUGUI hudStatsText;
    [SerializeField] RectTransform endingPanel;
    [SerializeField] TextMeshProUGUI endingTitleText;
    [SerializeField] TextMeshProUGUI endingBodyText;
    [SerializeField] RectTransform endingActionsRoot;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuButton;
    [SerializeField] TMP_FontAsset fontAsset;

    readonly List<Button> spawnedChoiceButtons = new List<Button>();

    RuntimeStoryNode currentNode;
    bool waitingForCombat;
    bool runtimeInitialized;

    void Reset()
    {
        QueueEditorRebuild();
    }

    void OnValidate()
    {
        QueueEditorRebuild();
    }

    void Awake()
    {
        EnsureBuilt();
        ResolveReferences();

        if (Application.isPlaying)
        {
            RegisterCallbacks();
            PreparePlayModeView();
            InitializeRuntimeState();
        }
        else
        {
            PrepareEditorPreview();
        }
    }

    void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        BeginAdventure();
    }

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            RegisterCallbacks();
        }
    }

    void OnDisable()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        UnregisterCallbacks();
    }

    void OnDestroy()
    {
        if (Application.isPlaying)
        {
            UnregisterCallbacks();
        }
    }

    public void EnsureBuilt()
    {
        bool createdSomething = false;
        ResolveReferences();

        if (fontAsset == null)
        {
            fontAsset = SceneUiBuilder.ResolveFontAsset();
        }

        storyCanvas = storyCanvas != null ? storyCanvas : FindCanvas();
        if (storyCanvas == null || storyCanvas.name != StoryCanvasName)
        {
            storyCanvas = SceneUiBuilder.EnsureCanvas(transform, storyCanvas, StoryCanvasName, 0, ref createdSomething);
        }

        backdrop = SceneUiBuilder.EnsureRect(backdrop, storyCanvas.transform, BackdropName,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        SceneUiBuilder.EnsureImage(backdrop.gameObject, new Color(0.04f, 0.07f, 0.1f, 1f), ref createdSomething);

        hudPanel = SceneUiBuilder.EnsureRect(hudPanel, storyCanvas.transform, HudPanelName,
            new Vector2(0.03f, 0.72f), new Vector2(0.29f, 0.95f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        SceneUiBuilder.EnsureImage(hudPanel.gameObject, new Color(0.06f, 0.12f, 0.16f, 0.92f), ref createdSomething);
        hudNameText = SceneUiBuilder.EnsureText(hudNameText, hudPanel, HudNameName, "Aventurero", 28f, FontStyles.Bold, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0.58f), Vector2.one, new Vector2(28f, -28f), new Vector2(-28f, -18f), fontAsset, ref createdSomething);
        hudStatsText = SceneUiBuilder.EnsureText(hudStatsText, hudPanel, HudStatsName, "Vida 0/0", 22f, FontStyles.Normal, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0f), new Vector2(1f, 0.64f), new Vector2(28f, 22f), new Vector2(-28f, -24f), fontAsset, ref createdSomething);

        storyPanel = SceneUiBuilder.EnsureRect(storyPanel, storyCanvas.transform, StoryPanelName,
            new Vector2(0.24f, 0.08f), new Vector2(0.95f, 0.9f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        SceneUiBuilder.EnsureImage(storyPanel.gameObject, new Color(0.08f, 0.11f, 0.14f, 0.95f), ref createdSomething);

        titleText = SceneUiBuilder.EnsureText(titleText, storyPanel, TitleName, "Camara del umbral", 42f, FontStyles.Bold, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0.78f), Vector2.one, new Vector2(48f, -42f), new Vector2(-48f, -24f), fontAsset, ref createdSomething);
        bodyText = SceneUiBuilder.EnsureText(bodyText, storyPanel, BodyName,
            "Una brisa fria recorre la mazmorra mientras las antorchas tiemblan sobre la piedra.", 28f, FontStyles.Normal, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0.30f), new Vector2(1f, 0.78f), new Vector2(48f, 20f), new Vector2(-48f, -18f), fontAsset, ref createdSomething);

        choicesRoot = SceneUiBuilder.EnsureVerticalLayoutRoot(choicesRoot, storyPanel, ChoiceRootName,
            new Vector2(0f, 0.06f), new Vector2(1f, 0.24f), new Vector2(48f, 0f), new Vector2(-48f, 0f), 18f, ref createdSomething);
        choiceTemplate = SceneUiBuilder.EnsureButton(choiceTemplate, choicesRoot, ChoiceTemplateName, "Explorar el pasillo",
            new Vector2(0f, 0f), Vector2.one, Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);
        SceneUiBuilder.EnsureLayoutElement(choiceTemplate.gameObject, 92f, ref createdSomething);

        endingPanel = SceneUiBuilder.EnsureRect(endingPanel, storyCanvas.transform, EndingPanelName,
            new Vector2(0.28f, 0.18f), new Vector2(0.92f, 0.82f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        SceneUiBuilder.EnsureImage(endingPanel.gameObject, new Color(0.06f, 0.1f, 0.12f, 0.97f), ref createdSomething);
        endingTitleText = SceneUiBuilder.EnsureText(endingTitleText, endingPanel, EndingTitleName, "Final alcanzado", 44f, FontStyles.Bold, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0.74f), Vector2.one, new Vector2(52f, -46f), new Vector2(-52f, -22f), fontAsset, ref createdSomething);
        endingBodyText = SceneUiBuilder.EnsureText(endingBodyText, endingPanel, EndingBodyName,
            "Has sobrevivido a la mazmorra, pero el precio de tus decisiones quedara contigo.", 28f, FontStyles.Normal, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0.28f), new Vector2(1f, 0.72f), new Vector2(52f, 18f), new Vector2(-52f, -18f), fontAsset, ref createdSomething);
        endingActionsRoot = SceneUiBuilder.EnsureHorizontalLayoutRoot(endingActionsRoot, endingPanel, EndingActionsName,
            new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.2f), Vector2.zero, Vector2.zero, 24f, ref createdSomething);
        restartButton = SceneUiBuilder.EnsureButton(restartButton, endingActionsRoot, RestartButtonName, "Reiniciar",
            new Vector2(0f, 0f), Vector2.one, Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);
        SceneUiBuilder.EnsureLayoutElement(restartButton.gameObject, 82f, ref createdSomething);
        menuButton = SceneUiBuilder.EnsureButton(menuButton, endingActionsRoot, MenuButtonName, "Menu principal",
            new Vector2(0f, 0f), Vector2.one, Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);
        SceneUiBuilder.EnsureLayoutElement(menuButton.gameObject, 82f, ref createdSomething);

        SceneUiBuilder.EnsureLocalizer(restartButton.GetComponentInChildren<TextMeshProUGUI>(true), "ENDING_RESTART", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(menuButton.GetComponentInChildren<TextMeshProUGUI>(true), "ENDING_MENU", ref createdSomething);

        if (!Application.isPlaying)
        {
            PrepareEditorPreview();
        }

        SceneUiBuilder.MarkSceneDirty(transform, createdSomething);
    }

    void PrepareEditorPreview()
    {
        if (choiceTemplate != null)
        {
            choiceTemplate.gameObject.SetActive(true);
        }

        if (endingPanel != null)
        {
            endingPanel.gameObject.SetActive(true);
        }
    }

    void PreparePlayModeView()
    {
        if (choiceTemplate != null)
        {
            choiceTemplate.gameObject.SetActive(false);
        }

        SetEndingVisible(false);
        combatUI?.SetDebugPanelVisible(false);
    }

    void EnsureEditorHierarchy()
    {
        if (!gameObject.scene.IsValid())
        {
            return;
        }

        EnsureBuilt();
    }

#if UNITY_EDITOR
    void QueueEditorRebuild()
    {
        if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || !gameObject.scene.IsValid())
        {
            return;
        }

        EditorApplication.delayCall -= HandleEditorRebuild;
        EditorApplication.delayCall += HandleEditorRebuild;
    }

    void HandleEditorRebuild()
    {
        EditorApplication.delayCall -= HandleEditorRebuild;

        if (this == null || Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || !gameObject.scene.IsValid())
        {
            return;
        }

        EnsureBuilt();
    }
#endif

    void ResolveReferences()
    {
        if (encounterController == null)
        {
            encounterController = GetComponent<CombatEncounterController>();
        }

        if (combatUI == null)
        {
            combatUI = GetComponent<CombatPanelUI>();
        }

        if (player == null)
        {
            player = GetComponent<Player>();
        }

        if (player == null && Application.isPlaying)
        {
            player = gameObject.AddComponent<Player>();
        }

        if (storyGraph == null && Application.isPlaying)
        {
            storyGraph = Resources.Load<RuntimeStoryGraph>(DefaultStoryGraphPath);
            if (storyGraph == null)
            {
                storyGraph = Resources.LoadAll<RuntimeStoryGraph>(string.Empty).FirstOrDefault();
            }
        }

        if (playerProfile == null && Application.isPlaying)
        {
            playerProfile = Resources.Load<PlayerCombatProfile>(DefaultPlayerProfilePath);
        }
    }

    void RegisterCallbacks()
    {
        if (encounterController != null)
        {
            encounterController.OnEncounterFinished -= HandleCombatFinished;
            encounterController.OnEncounterFinished += HandleCombatFinished;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartAdventure);
            restartButton.onClick.AddListener(RestartAdventure);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(ReturnToMenu);
            menuButton.onClick.AddListener(ReturnToMenu);
        }

        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    void UnregisterCallbacks()
    {
        if (encounterController != null)
        {
            encounterController.OnEncounterFinished -= HandleCombatFinished;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartAdventure);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(ReturnToMenu);
        }

        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    void InitializeRuntimeState()
    {
        if (runtimeInitialized)
        {
            return;
        }

        ResolveReferences();

        if (playerProfile != null)
        {
            CombatSession.ResetFromProfile(playerProfile);
        }
        else
        {
            CombatSession.ResetFromProfile(null);
        }

        if (PlayerSession.HasCustomStats)
        {
            CombatSession.ConfigurePlayerStats(PlayerSession.InitialHealth, PlayerSession.InitialDamage);
        }

        if (player == null)
        {
            player = gameObject.AddComponent<Player>();
        }

        int initialHealth = CombatSession.MaxHealth;
        int initialDamage = CombatSession.BaseDamage;
        int initialBravery = PlayerSession.HasCustomStats ? PlayerSession.InitialBravery : 0;
        player.Initialize(PlayerDisplayName, initialHealth, initialDamage, initialBravery);

        runtimeInitialized = true;
    }

    void BeginAdventure()
    {
        if (storyGraph == null)
        {
            Debug.LogError("StoryGameManager could not find a RuntimeStoryGraph to load.");
            return;
        }

        SetEndingVisible(false);
        LoadNode(storyGraph.GetStartNode()?.id ?? storyGraph.startNodeId);
    }

    void LoadNode(string nodeId)
    {
        if (storyGraph == null || string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        RuntimeStoryNode nextNode = storyGraph.GetNode(nodeId);
        if (nextNode == null)
        {
            Debug.LogError($"Story node not found: {nodeId}");
            return;
        }

        currentNode = nextNode;
        waitingForCombat = false;
        SetEndingVisible(false);

        titleText.text = RuntimeLocalization.GetText(currentNode.titleKey, currentNode.title);
        bodyText.text = RuntimeLocalization.GetText(currentNode.bodyKey, currentNode.body, PlayerDisplayName);
        RefreshHud();

        switch (currentNode.type)
        {
            case NodeType.Combat:
                RebuildChoiceButtons(Array.Empty<RuntimeStoryChoice>());
                StartCombatNode(currentNode);
                break;
            case NodeType.Ending:
                ShowEnding(currentNode);
                break;
            default:
                RebuildChoiceButtons(currentNode.choices);
                break;
        }
    }

    void RebuildChoiceButtons(IReadOnlyList<RuntimeStoryChoice> choices)
    {
        foreach (Button spawnedButton in spawnedChoiceButtons)
        {
            if (spawnedButton != null)
            {
                DestroyImmediateSafe(spawnedButton.gameObject);
            }
        }

        spawnedChoiceButtons.Clear();

        if (choiceTemplate == null)
        {
            return;
        }

        choiceTemplate.gameObject.SetActive(!Application.isPlaying);

        if (choices == null)
        {
            return;
        }

        foreach (RuntimeStoryChoice choice in choices)
        {
            if (choice == null)
            {
                continue;
            }

            Button button = Application.isPlaying
                ? Instantiate(choiceTemplate, choicesRoot)
                : Instantiate(choiceTemplate, choicesRoot, false);

            button.name = $"Choice - {choice.nextNodeId}";
            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectChoice(choice));

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = RuntimeLocalization.GetText(choice.textKey, choice.text);
            }

            spawnedChoiceButtons.Add(button);
        }
    }

    void SelectChoice(RuntimeStoryChoice choice)
    {
        if (choice == null || currentNode == null)
        {
            return;
        }

        ApplyChoiceEffects(choice);
        LoadNode(choice.nextNodeId);
    }

    void ApplyChoiceEffects(RuntimeStoryChoice choice)
    {
        string decisionLabel = !string.IsNullOrWhiteSpace(choice.textKey)
            ? RuntimeLocalization.GetText(choice.textKey, choice.text)
            : choice.text;

        player?.PushDecision($"{currentNode.id}:{decisionLabel}");

        if (choice.healthChange < 0)
        {
            player?.TakeDamage(-choice.healthChange);
            CombatSession.ApplyDamage(-choice.healthChange);
        }
        else if (choice.healthChange > 0)
        {
            player?.Heal(choice.healthChange);
            CombatSession.ApplyHealing(choice.healthChange);
        }

        if (choice.damageChange != 0)
        {
            player?.ModifyStat(StatType.Attack, choice.damageChange);
            CombatSession.AddDamageBonus(choice.damageChange);
        }

        if (choice.courageChange != 0)
        {
            player?.ModifyStat(StatType.Bravery, choice.courageChange);
        }

        if (!string.IsNullOrWhiteSpace(choice.grantedItemId))
        {
            ItemDefinition item = ResolveItem(choice.grantedItemId);
            if (item != null)
            {
                CombatSession.AddItem(item);
            }
        }

        SyncPlayerRuntimeStats();
        RefreshHud();
    }

    void StartCombatNode(RuntimeStoryNode node)
    {
        if (encounterController == null)
        {
            ContinueFromCombat(false);
            return;
        }

        EnemyDefinition enemy = ResolveEnemy(node.enemyId);
        if (enemy == null)
        {
            Debug.LogError($"Enemy not found for combat node '{node.id}': {node.enemyId}");
            ContinueFromCombat(false);
            return;
        }

        waitingForCombat = true;
        encounterController.BeginEncounter(enemy);
    }

    void HandleCombatFinished(CombatResult result)
    {
        if (!waitingForCombat || currentNode == null || result == null)
        {
            return;
        }

        waitingForCombat = false;
        SyncPlayerRuntimeStats();
        RefreshHud();
        ContinueFromCombat(result.Outcome == CombatOutcome.Victory);
    }

    void ContinueFromCombat(bool victory)
    {
        if (currentNode == null)
        {
            return;
        }

        string nextNodeId = victory ? currentNode.victoryNodeId : currentNode.defeatNodeId;
        if (!string.IsNullOrWhiteSpace(nextNodeId))
        {
            LoadNode(nextNodeId);
            return;
        }

        int routeIndex = victory ? 0 : 1;
        if (currentNode.choices != null && routeIndex < currentNode.choices.Count)
        {
            LoadNode(currentNode.choices[routeIndex].nextNodeId);
        }
    }

    void ShowEnding(RuntimeStoryNode node)
    {
        titleText.text = RuntimeLocalization.GetText(node.titleKey, node.title);
        bodyText.text = RuntimeLocalization.GetText(node.bodyKey, node.body, PlayerDisplayName);
        endingTitleText.text = titleText.text;
        endingBodyText.text = bodyText.text;
        RebuildChoiceButtons(Array.Empty<RuntimeStoryChoice>());
        SetEndingVisible(true);
        RefreshHud();
    }

    void SetEndingVisible(bool visible)
    {
        if (endingPanel != null)
        {
            endingPanel.gameObject.SetActive(visible || !Application.isPlaying);
        }
    }

    void RestartAdventure()
    {
        runtimeInitialized = false;
        InitializeRuntimeState();
        BeginAdventure();
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }

    void HandleLocaleChanged(Locale _)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (currentNode != null)
        {
            LoadNode(currentNode.id);
        }
        else
        {
            RefreshHud();
        }
    }

    void RefreshHud()
    {
        if (hudNameText == null || hudStatsText == null)
        {
            return;
        }

        string playerName = PlayerDisplayName;
        int currentHealth = CombatSession.CurrentHealth;
        int maxHealth = CombatSession.MaxHealth;
        int damage = CombatSession.BaseDamage;
        int bravery = player != null ? player.GetBravery() : 0;

        hudNameText.text = RuntimeLocalization.GetText("HUD_NAME", "Aventurero: {0}", playerName);

        IReadOnlyList<CombatItemStack> inventory = CombatSession.GetInventorySnapshot();
        string inventoryLabel = inventory.Count == 0
            ? RuntimeLocalization.GetText("HUD_INVENTORY_EMPTY", "Inventario: vacio")
            : $"{RuntimeLocalization.GetText("HUD_INVENTORY", "Inventario")}: {string.Join(", ", inventory.Select(item => $"{item.Item.DisplayName} x{item.Quantity}"))}";

        hudStatsText.text = string.Join("\n", new[]
        {
            RuntimeLocalization.GetText("HUD_HEALTH", "Vida: {0}/{1}", currentHealth, maxHealth),
            RuntimeLocalization.GetText("HUD_DAMAGE", "Dano: {0}", damage),
            RuntimeLocalization.GetText("HUD_BRAVERY", "Valentia: {0}", bravery),
            inventoryLabel
        });
    }

    void SyncPlayerRuntimeStats()
    {
        if (player == null)
        {
            return;
        }

        int healthDelta = CombatSession.CurrentHealth - player.GetHealth();
        if (healthDelta != 0)
        {
            player.ModifyStat(StatType.Health, healthDelta);
        }

        int damageDelta = CombatSession.BaseDamage - player.GetAttack();
        if (damageDelta != 0)
        {
            player.ModifyStat(StatType.Attack, damageDelta);
        }
    }

    EnemyDefinition ResolveEnemy(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
        {
            return debugEnemy;
        }

        string resourcePath = enemyId.Contains("/") ? enemyId : EnemyResourceRoot + enemyId;
        EnemyDefinition enemy = Resources.Load<EnemyDefinition>(resourcePath);
        if (enemy != null)
        {
            return enemy;
        }

        return debugEnemy;
    }

    ItemDefinition ResolveItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        string resourcePath = itemId.Contains("/") ? itemId : ItemResourceRoot + itemId;
        ItemDefinition item = Resources.Load<ItemDefinition>(resourcePath);
        if (item != null)
        {
            return item;
        }

        return Resources.LoadAll<ItemDefinition>(ItemResourceRoot)
            .FirstOrDefault(candidate => candidate != null && (candidate.name == itemId || candidate.ItemId == itemId));
    }

    Canvas FindCanvas()
    {
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas != null && canvas.name == StoryCanvasName)
            {
                return canvas;
            }
        }

        return null;
    }

    void DestroyImmediateSafe(UnityEngine.Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    string PlayerDisplayName => string.IsNullOrWhiteSpace(PlayerSession.PlayerName) ? "Aventurero" : PlayerSession.PlayerName;
}
