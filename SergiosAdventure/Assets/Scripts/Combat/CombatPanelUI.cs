using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class CombatPanelUI : MonoBehaviour
{
    const string CanvasName = "Combat Canvas";
    const string DebugPanelName = "Debug Panel";
    const string DebugTitleName = "Debug Title";
    const string DebugStatusName = "Debug Status";
    const string StartDebugButtonName = "Start Debug Button";
    const string ResetDebugButtonName = "Reset Debug Button";
    const string EncounterPanelName = "Combat Panel";
    const string PlayerHeaderName = "Player Header";
    const string EnemyHeaderName = "Enemy Header";
    const string CombatLogName = "Combat Log";
    const string ActionButtonsRootName = "Action Buttons";
    const string ActionButtonTemplateName = "Action Button Template";
    const string ItemMenuName = "Item Menu";
    const string ItemMenuTitleName = "Item Menu Title";
    const string ItemButtonsRootName = "Item Buttons";
    const string ItemButtonTemplateName = "Item Button Template";
    const string ItemBackButtonName = "Item Back Button";

    public readonly struct ButtonModel
    {
        public ButtonModel(string label, UnityAction callback, bool interactable = true)
        {
            Label = label;
            Callback = callback;
            Interactable = interactable;
        }

        public string Label { get; }
        public UnityAction Callback { get; }
        public bool Interactable { get; }
    }

    [SerializeField] Canvas rootCanvas;
    [SerializeField] RectTransform debugPanel;
    [SerializeField] TMP_Text debugStatusText;
    [SerializeField] Button debugEncounterButton;
    [SerializeField] Button debugResetButton;
    [SerializeField] RectTransform encounterPanel;
    [SerializeField] TMP_Text playerHeaderText;
    [SerializeField] TMP_Text enemyHeaderText;
    [SerializeField] TMP_Text logText;
    [SerializeField] RectTransform actionButtonsRoot;
    [SerializeField] Button actionButtonTemplate;
    [SerializeField] RectTransform itemMenuRoot;
    [SerializeField] TMP_Text itemMenuTitleText;
    [SerializeField] RectTransform itemButtonsRoot;
    [SerializeField] Button itemButtonTemplate;
    [SerializeField] Button itemBackButton;
    [SerializeField] TMP_FontAsset fontAsset;

    readonly List<Button> spawnedActionButtons = new List<Button>();
    readonly List<Button> spawnedItemButtons = new List<Button>();

    void Reset()
    {
        EnsureEditorHierarchy();
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            EnsureEditorHierarchy();
        }
    }

    void Awake()
    {
        EnsureBuilt();

        if (Application.isPlaying)
        {
            EnsureEventSystem();
            PreparePlayModeView();
            SetEncounterVisible(false);
        }
        else
        {
            PrepareEditorView();
        }
    }

    public void EnsureBuilt()
    {
        bool createdSomething = false;

        TryAssignExistingReferences();

        if (fontAsset == null)
        {
            fontAsset = ResolveFontAsset();
        }

        rootCanvas = EnsureCanvas(rootCanvas, ref createdSomething);
        debugPanel = EnsureRect(debugPanel, rootCanvas.transform, DebugPanelName,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(360f, 220f), ref createdSomething);
        EnsureImage(debugPanel.gameObject, new Color(0.07f, 0.09f, 0.13f, 0.86f), ref createdSomething);

        EnsureText(debugPanel, DebugTitleName, "Combat Debug", 28, FontStyles.Bold, TextAlignmentOptions.TopLeft,
            new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.95f), Vector2.zero, Vector2.zero, ref createdSomething);
        debugStatusText = EnsureText(debugPanel, DebugStatusName, "Inicializando combate...", 20, FontStyles.Normal, TextAlignmentOptions.TopLeft,
            new Vector2(0.06f, 0.30f), new Vector2(0.94f, 0.75f), Vector2.zero, Vector2.zero, ref createdSomething);

        debugEncounterButton = EnsureButton(debugPanel, StartDebugButtonName, "Iniciar combate",
            new Vector2(0.06f, 0.07f), new Vector2(0.46f, 0.23f), Vector2.zero, Vector2.zero, ref createdSomething);
        debugResetButton = EnsureButton(debugPanel, ResetDebugButtonName, "Resetear estado",
            new Vector2(0.54f, 0.07f), new Vector2(0.94f, 0.23f), Vector2.zero, Vector2.zero, ref createdSomething);

        encounterPanel = EnsureRect(encounterPanel, rootCanvas.transform, EncounterPanelName,
            new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.47f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        EnsureImage(encounterPanel.gameObject, new Color(0.08f, 0.08f, 0.1f, 0.92f), ref createdSomething);

        playerHeaderText = EnsureText(encounterPanel, PlayerHeaderName, "Jugador\nHP 0/0", 24, FontStyles.Bold, TextAlignmentOptions.TopLeft,
            new Vector2(0.04f, 0.80f), new Vector2(0.44f, 0.95f), Vector2.zero, Vector2.zero, ref createdSomething);
        enemyHeaderText = EnsureText(encounterPanel, EnemyHeaderName, "Sin enemigo\n--", 24, FontStyles.Bold, TextAlignmentOptions.TopRight,
            new Vector2(0.56f, 0.80f), new Vector2(0.96f, 0.95f), Vector2.zero, Vector2.zero, ref createdSomething);
        logText = EnsureText(encounterPanel, CombatLogName, "Sin acciones por ahora.", 24, FontStyles.Normal, TextAlignmentOptions.TopLeft,
            new Vector2(0.04f, 0.38f), new Vector2(0.96f, 0.74f), Vector2.zero, Vector2.zero, ref createdSomething);

        actionButtonsRoot = EnsureVerticalLayoutRoot(actionButtonsRoot, encounterPanel, ActionButtonsRootName,
            new Vector2(0.04f, 0.08f), new Vector2(0.46f, 0.30f), Vector2.zero, Vector2.zero, ref createdSomething);
        actionButtonTemplate = EnsureTemplateButton(actionButtonsRoot, actionButtonTemplate, ActionButtonTemplateName, "Atacar", ref createdSomething);

        itemMenuRoot = EnsureRect(itemMenuRoot, encounterPanel, ItemMenuName,
            new Vector2(0.54f, 0.05f), new Vector2(0.96f, 0.33f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        EnsureImage(itemMenuRoot.gameObject, new Color(0.12f, 0.14f, 0.18f, 0.95f), ref createdSomething);
        itemMenuTitleText = EnsureText(itemMenuRoot, ItemMenuTitleName, "Items", 22, FontStyles.Bold, TextAlignmentOptions.TopLeft,
            new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.95f), Vector2.zero, Vector2.zero, ref createdSomething);
        itemButtonsRoot = EnsureVerticalLayoutRoot(itemButtonsRoot, itemMenuRoot, ItemButtonsRootName,
            new Vector2(0.06f, 0.24f), new Vector2(0.94f, 0.74f), Vector2.zero, Vector2.zero, ref createdSomething);
        itemButtonTemplate = EnsureTemplateButton(itemButtonsRoot, itemButtonTemplate, ItemButtonTemplateName, "Pocion pequena x1 (+6 HP)", ref createdSomething);
        itemBackButton = EnsureButton(itemMenuRoot, ItemBackButtonName, "Volver",
            new Vector2(0.20f, 0.06f), new Vector2(0.80f, 0.18f), Vector2.zero, Vector2.zero, ref createdSomething);

        if (!Application.isPlaying)
        {
            EnsureEventSystem();
            PrepareEditorView();
        }

#if UNITY_EDITOR
        if (createdSomething && gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }

    public void SetEncounterVisible(bool visible)
    {
        EnsureBuilt();
        encounterPanel.gameObject.SetActive(visible || !Application.isPlaying);

        if (!visible && Application.isPlaying)
        {
            SetItemMenuVisible(false);
            ClearButtons(spawnedActionButtons);
            ClearButtons(spawnedItemButtons);
            SetButton(itemBackButton, "Volver", null, false);
        }
    }

    public void SetItemMenuVisible(bool visible)
    {
        EnsureBuilt();
        itemMenuRoot.gameObject.SetActive(visible || !Application.isPlaying);
    }

    public void SetPlayerStats(string playerName, int currentHealth, int maxHealth)
    {
        EnsureBuilt();
        string resolvedName = string.IsNullOrWhiteSpace(playerName) ? "Jugador" : playerName;
        playerHeaderText.text = $"{resolvedName}\nHP {Mathf.Max(0, currentHealth)}/{Mathf.Max(0, maxHealth)}";
    }

    public void SetEnemyStats(string enemyName, int currentHealth, int maxHealth)
    {
        EnsureBuilt();
        if (maxHealth <= 0)
        {
            enemyHeaderText.text = "Sin enemigo\n--";
            return;
        }

        string resolvedName = string.IsNullOrWhiteSpace(enemyName) ? "Enemigo" : enemyName;
        enemyHeaderText.text = $"{resolvedName}\nHP {Mathf.Max(0, currentHealth)}/{Mathf.Max(0, maxHealth)}";
    }

    public void SetCombatLog(IReadOnlyList<string> lines)
    {
        EnsureBuilt();

        if (lines == null || lines.Count == 0)
        {
            logText.text = "Sin acciones por ahora.";
            return;
        }

        int visibleCount = Mathf.Min(lines.Count, 6);
        int startIndex = lines.Count - visibleCount;
        var visibleLines = new List<string>(visibleCount);
        for (int i = startIndex; i < lines.Count; i++)
        {
            visibleLines.Add(lines[i]);
        }

        logText.text = string.Join("\n", visibleLines);
    }

    public void SetActionButtons(IReadOnlyList<ButtonModel> buttons)
    {
        EnsureBuilt();
        SetItemMenuVisible(false);
        RebuildButtons(actionButtonsRoot, actionButtonTemplate, spawnedActionButtons, buttons);
    }

    public void SetItemButtons(string title, IReadOnlyList<ButtonModel> buttons, UnityAction backAction)
    {
        EnsureBuilt();
        itemMenuTitleText.text = string.IsNullOrWhiteSpace(title) ? "Items" : title;
        RebuildButtons(itemButtonsRoot, itemButtonTemplate, spawnedItemButtons, buttons);
        SetButton(itemBackButton, "Volver", backAction, true);
        SetItemMenuVisible(true);
    }

    public void ConfigureDebugButtons(string encounterLabel, UnityAction encounterAction, string resetLabel, UnityAction resetAction)
    {
        EnsureBuilt();
        SetButton(debugEncounterButton, encounterLabel, encounterAction, encounterAction != null);
        SetButton(debugResetButton, resetLabel, resetAction, resetAction != null);
    }

    public void SetDebugButtonsInteractable(bool encounterInteractable, bool resetInteractable)
    {
        EnsureBuilt();
        debugEncounterButton.interactable = encounterInteractable;
        debugResetButton.interactable = resetInteractable;
    }

    public void SetDebugStatus(string status)
    {
        EnsureBuilt();
        debugStatusText.text = string.IsNullOrWhiteSpace(status) ? "Sin estado." : status;
    }

    void EnsureEditorHierarchy()
    {
        if (!gameObject.scene.IsValid())
        {
            return;
        }

        EnsureBuilt();
    }

    void PrepareEditorView()
    {
        if (encounterPanel != null)
        {
            encounterPanel.gameObject.SetActive(true);
        }

        if (itemMenuRoot != null)
        {
            itemMenuRoot.gameObject.SetActive(true);
        }

        if (actionButtonTemplate != null)
        {
            actionButtonTemplate.gameObject.SetActive(true);
        }

        if (itemButtonTemplate != null)
        {
            itemButtonTemplate.gameObject.SetActive(true);
        }
    }

    void PreparePlayModeView()
    {
        if (actionButtonTemplate != null)
        {
            actionButtonTemplate.gameObject.SetActive(false);
        }

        if (itemButtonTemplate != null)
        {
            itemButtonTemplate.gameObject.SetActive(false);
        }

        if (itemMenuRoot != null)
        {
            itemMenuRoot.gameObject.SetActive(false);
        }
    }

    void TryAssignExistingReferences()
    {
        if (rootCanvas == null)
        {
            rootCanvas = FindChildComponent<Canvas>(transform, CanvasName);
        }

        Transform canvasTransform = rootCanvas != null ? rootCanvas.transform : null;
        if (canvasTransform == null)
        {
            return;
        }

        debugPanel = debugPanel != null ? debugPanel : FindChildComponent<RectTransform>(canvasTransform, DebugPanelName);
        debugStatusText = debugStatusText != null ? debugStatusText : FindNestedText(debugPanel, DebugStatusName);
        debugEncounterButton = debugEncounterButton != null ? debugEncounterButton : FindNestedButton(debugPanel, StartDebugButtonName);
        debugResetButton = debugResetButton != null ? debugResetButton : FindNestedButton(debugPanel, ResetDebugButtonName);

        encounterPanel = encounterPanel != null ? encounterPanel : FindChildComponent<RectTransform>(canvasTransform, EncounterPanelName);
        playerHeaderText = playerHeaderText != null ? playerHeaderText : FindNestedText(encounterPanel, PlayerHeaderName);
        enemyHeaderText = enemyHeaderText != null ? enemyHeaderText : FindNestedText(encounterPanel, EnemyHeaderName);
        logText = logText != null ? logText : FindNestedText(encounterPanel, CombatLogName);
        actionButtonsRoot = actionButtonsRoot != null ? actionButtonsRoot : FindNestedRect(encounterPanel, ActionButtonsRootName);
        actionButtonTemplate = actionButtonTemplate != null ? actionButtonTemplate : FindNestedButton(actionButtonsRoot, ActionButtonTemplateName);
        itemMenuRoot = itemMenuRoot != null ? itemMenuRoot : FindNestedRect(encounterPanel, ItemMenuName);
        itemMenuTitleText = itemMenuTitleText != null ? itemMenuTitleText : FindNestedText(itemMenuRoot, ItemMenuTitleName);
        itemButtonsRoot = itemButtonsRoot != null ? itemButtonsRoot : FindNestedRect(itemMenuRoot, ItemButtonsRootName);
        itemButtonTemplate = itemButtonTemplate != null ? itemButtonTemplate : FindNestedButton(itemButtonsRoot, ItemButtonTemplateName);
        itemBackButton = itemBackButton != null ? itemBackButton : FindNestedButton(itemMenuRoot, ItemBackButtonName);
    }

    void RebuildButtons(RectTransform container, Button template, List<Button> spawnedButtons, IReadOnlyList<ButtonModel> buttons)
    {
        ClearButtons(spawnedButtons);

        if (!Application.isPlaying || container == null || template == null || buttons == null)
        {
            return;
        }

        template.gameObject.SetActive(false);

        foreach (ButtonModel buttonModel in buttons)
        {
            Button createdButton = Instantiate(template, container);
            createdButton.gameObject.name = $"{template.gameObject.name} - {buttonModel.Label}";
            createdButton.gameObject.SetActive(true);
            SetButton(createdButton, buttonModel.Label, buttonModel.Callback, buttonModel.Interactable && buttonModel.Callback != null);
            spawnedButtons.Add(createdButton);
        }
    }

    void ClearButtons(List<Button> spawnedButtons)
    {
        for (int i = spawnedButtons.Count - 1; i >= 0; i--)
        {
            Button button = spawnedButtons[i];
            if (button == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(button.gameObject);
            }
            else
            {
                DestroyImmediate(button.gameObject);
            }
        }

        spawnedButtons.Clear();
    }

    void SetButton(Button button, string label, UnityAction callback, bool interactable)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text labelText = button.GetComponentInChildren<TMP_Text>(true);
        if (labelText != null)
        {
            labelText.text = label;
        }

        button.onClick.RemoveAllListeners();
        if (callback != null)
        {
            button.onClick.AddListener(callback);
        }

        button.interactable = interactable;
    }

    Canvas EnsureCanvas(Canvas currentCanvas, ref bool createdSomething)
    {
        if (currentCanvas != null)
        {
            return currentCanvas;
        }

        GameObject canvasObject = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        createdSomething = true;
        return canvas;
    }

    RectTransform EnsureRect(RectTransform currentRect, Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, ref bool createdSomething)
    {
        if (currentRect != null)
        {
            return currentRect;
        }

        Transform existingChild = parent.Find(objectName);
        if (existingChild != null)
        {
            RectTransform existingRect = existingChild.GetComponent<RectTransform>();
            if (existingRect != null)
            {
                return existingRect;
            }
        }

        GameObject rectObject = new GameObject(objectName, typeof(RectTransform));
        rectObject.transform.SetParent(parent, false);
        RectTransform rectTransform = rectObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        createdSomething = true;
        return rectTransform;
    }

    TMP_Text EnsureText(Transform parent, string objectName, string defaultText, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, ref bool createdSomething)
    {
        RectTransform rectTransform = EnsureRect(null, parent, objectName, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), anchoredPosition, sizeDelta, ref createdSomething);
        TextMeshProUGUI textComponent = rectTransform.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            textComponent = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = defaultText;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.alignment = alignment;
            textComponent.color = Color.white;
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            createdSomething = true;
        }

        if (textComponent.font == null && fontAsset != null)
        {
            textComponent.font = fontAsset;
        }

        return textComponent;
    }

    RectTransform EnsureVerticalLayoutRoot(RectTransform currentRoot, Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, ref bool createdSomething)
    {
        RectTransform rectTransform = EnsureRect(currentRoot, parent, objectName, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), anchoredPosition, sizeDelta, ref createdSomething);
        VerticalLayoutGroup layoutGroup = rectTransform.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.spacing = 8f;
            createdSomething = true;
        }

        return rectTransform;
    }

    Button EnsureButton(Transform parent, string objectName, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, ref bool createdSomething)
    {
        RectTransform rectTransform = EnsureRect(null, parent, objectName, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), anchoredPosition, sizeDelta, ref createdSomething);
        EnsureImage(rectTransform.gameObject, new Color(0.19f, 0.30f, 0.43f, 1f), ref createdSomething);

        Button button = rectTransform.GetComponent<Button>();
        if (button == null)
        {
            button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = rectTransform.GetComponent<Image>();
            createdSomething = true;
        }

        EnsureText(rectTransform, "Label", label, 20, FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, ref createdSomething);

        return button;
    }

    Button EnsureTemplateButton(Transform parent, Button currentButton, string objectName, string label, ref bool createdSomething)
    {
        if (currentButton != null)
        {
            return currentButton;
        }

        Button button = EnsureButton(parent, objectName, label, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, 44f), ref createdSomething);
        LayoutElement layoutElement = button.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = button.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 44f;
            createdSomething = true;
        }

        return button;
    }

    void EnsureImage(GameObject target, Color color, ref bool createdSomething)
    {
        Image image = target.GetComponent<Image>();
        if (image == null)
        {
            image = target.AddComponent<Image>();
            image.color = color;
            createdSomething = true;
        }
    }

    TMP_FontAsset ResolveFontAsset()
    {
        TMP_FontAsset resolvedFont = TMP_Settings.defaultFontAsset;
        if (resolvedFont != null)
        {
            return resolvedFont;
        }

        return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();

#if UNITY_EDITOR
        if (!Application.isPlaying && gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }

    static T FindChildComponent<T>(Transform parent, string childName) where T : Component
    {
        if (parent == null)
        {
            return null;
        }

        Transform child = parent.Find(childName);
        return child != null ? child.GetComponent<T>() : null;
    }

    static TMP_Text FindNestedText(Transform parent, string childName)
    {
        return FindChildComponent<TMP_Text>(parent, childName);
    }

    static Button FindNestedButton(Transform parent, string childName)
    {
        return FindChildComponent<Button>(parent, childName);
    }

    static RectTransform FindNestedRect(Transform parent, string childName)
    {
        return FindChildComponent<RectTransform>(parent, childName);
    }
}
