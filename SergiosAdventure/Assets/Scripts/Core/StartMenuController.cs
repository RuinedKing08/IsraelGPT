using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class StartMenuController : MonoBehaviour
{
    const string RegistroSceneName = "Registro";

    const string RootCanvasName = "Main Menu Canvas";
    const string BackdropName = "Menu Backdrop";
    const string PanelName = "Main Menu Panel";
    const string TitleName = "Menu Title";
    const string SubtitleName = "Menu Subtitle";
    const string StartButtonName = "Start Button";
    const string QuitButtonName = "Quit Button";
    const string LanguageLabelName = "Language Label";
    const string LanguageRowName = "Language Row";
    const string SpanishButtonName = "Spanish Button";
    const string EnglishButtonName = "English Button";
    const string CreditsLabelName = "Credits Label";

    [SerializeField] Canvas rootCanvas;
    [SerializeField] RectTransform backdrop;
    [SerializeField] RectTransform panel;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI subtitleText;
    [SerializeField] Button startButton;
    [SerializeField] Button quitButton;
    [SerializeField] TextMeshProUGUI languageLabelText;
    [SerializeField] RectTransform languageRow;
    [SerializeField] Button spanishButton;
    [SerializeField] Button englishButton;
    [SerializeField] TextMeshProUGUI creditsText;
    [SerializeField] TMP_FontAsset fontAsset;

    bool listenersRegistered;

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
        RegisterListeners();
    }

    void Start()
    {
        UpdateLocaleVisuals();
    }

    void OnEnable()
    {
        EnsureBuilt();
        RegisterListeners();
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    void OnDisable()
    {
        UnregisterListeners();
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    public void OpenRegistro()
    {
        SceneManager.LoadScene(RegistroSceneName);
    }

    void EnsureBuilt()
    {
        bool createdSomething = false;

        if (fontAsset == null)
        {
            fontAsset = SceneUiBuilder.ResolveFontAsset();
        }

        if (rootCanvas == null)
        {
            rootCanvas = FindCanvas();
        }

        rootCanvas = SceneUiBuilder.EnsureCanvas(transform, rootCanvas, RootCanvasName, 0, ref createdSomething);
        DeactivateLegacyChildren();

        backdrop = SceneUiBuilder.EnsureRect(backdrop, rootCanvas.transform, BackdropName,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        SceneUiBuilder.EnsureImage(backdrop.gameObject, new Color(0.04f, 0.08f, 0.11f, 1f), ref createdSomething);

        panel = SceneUiBuilder.EnsureRect(panel, rootCanvas.transform, PanelName,
            new Vector2(0.24f, 0.12f), new Vector2(0.76f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        SceneUiBuilder.EnsureImage(panel.gameObject, new Color(0.08f, 0.14f, 0.17f, 0.94f), ref createdSomething);

        titleText = SceneUiBuilder.EnsureText(titleText, panel, TitleName, "La Mazmorra de TLS", 54f, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 0.78f), Vector2.one, new Vector2(42f, -52f), new Vector2(-42f, -16f), fontAsset, ref createdSomething);
        subtitleText = SceneUiBuilder.EnsureText(subtitleText, panel, SubtitleName,
            "Una aventura de texto donde cada decision empuja la historia hacia un final distinto.", 28f, FontStyles.Normal, TextAlignmentOptions.Center,
            new Vector2(0f, 0.62f), new Vector2(1f, 0.78f), new Vector2(54f, 10f), new Vector2(-54f, -12f), fontAsset, ref createdSomething);

        startButton = SceneUiBuilder.EnsureButton(startButton, panel, StartButtonName, "Iniciar aventura",
            new Vector2(0.22f, 0.42f), new Vector2(0.78f, 0.54f), Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);
        quitButton = SceneUiBuilder.EnsureButton(quitButton, panel, QuitButtonName, "Salir",
            new Vector2(0.22f, 0.28f), new Vector2(0.78f, 0.38f), Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);

        languageLabelText = SceneUiBuilder.EnsureText(languageLabelText, panel, LanguageLabelName, "Idioma", 24f, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0.2f, 0.19f), new Vector2(0.8f, 0.26f), Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);
        languageRow = SceneUiBuilder.EnsureHorizontalLayoutRoot(languageRow, panel, LanguageRowName,
            new Vector2(0.2f, 0.09f), new Vector2(0.8f, 0.18f), Vector2.zero, Vector2.zero, 18f, ref createdSomething);

        spanishButton = SceneUiBuilder.EnsureButton(spanishButton, languageRow, SpanishButtonName, "ES",
            new Vector2(0f, 0f), Vector2.one, Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);
        SceneUiBuilder.EnsureLayoutElement(spanishButton.gameObject, 74f, ref createdSomething);
        englishButton = SceneUiBuilder.EnsureButton(englishButton, languageRow, EnglishButtonName, "EN",
            new Vector2(0f, 0f), Vector2.one, Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);
        SceneUiBuilder.EnsureLayoutElement(englishButton.gameObject, 74f, ref createdSomething);

        creditsText = SceneUiBuilder.EnsureText(creditsText, panel, CreditsLabelName,
            "Sergio, Mateo, Juan, Milton y Oscar", 22f, FontStyles.Italic, TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.01f), new Vector2(0.92f, 0.08f), Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);

        SceneUiBuilder.EnsureLocalizer(titleText, "MENU_TITLE", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(subtitleText, "MENU_SUBTITLE", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(startButton.GetComponentInChildren<TextMeshProUGUI>(true), "MENU_START", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(quitButton.GetComponentInChildren<TextMeshProUGUI>(true), "MENU_QUIT", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(languageLabelText, "MENU_LANGUAGE", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(spanishButton.GetComponentInChildren<TextMeshProUGUI>(true), "MENU_LANG_ES", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(englishButton.GetComponentInChildren<TextMeshProUGUI>(true), "MENU_LANG_EN", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(creditsText, "MENU_CREDITS", ref createdSomething);

        SceneUiBuilder.MarkSceneDirty(transform, createdSomething);
    }

    void RegisterListeners()
    {
        if (listenersRegistered || startButton == null || quitButton == null || spanishButton == null || englishButton == null)
        {
            return;
        }

        startButton.onClick.AddListener(OpenRegistro);
        quitButton.onClick.AddListener(QuitGame);
        spanishButton.onClick.AddListener(SetSpanish);
        englishButton.onClick.AddListener(SetEnglish);
        listenersRegistered = true;
    }

    void UnregisterListeners()
    {
        if (!listenersRegistered)
        {
            return;
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OpenRegistro);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }

        if (spanishButton != null)
        {
            spanishButton.onClick.RemoveListener(SetSpanish);
        }

        if (englishButton != null)
        {
            englishButton.onClick.RemoveListener(SetEnglish);
        }

        listenersRegistered = false;
    }

    void SetSpanish()
    {
        RuntimeLocalization.TrySelectLocale("es");
        UpdateLocaleVisuals();
    }

    void SetEnglish()
    {
        RuntimeLocalization.TrySelectLocale("en");
        UpdateLocaleVisuals();
    }

    void QuitGame()
    {
        Application.Quit();
    }

    void HandleLocaleChanged(Locale _)
    {
        UpdateLocaleVisuals();
    }

    void UpdateLocaleVisuals()
    {
        string localeCode = RuntimeLocalization.GetSelectedLocaleCode();
        ApplySelectionState(spanishButton, localeCode == "es");
        ApplySelectionState(englishButton, localeCode == "en");
    }

    void ApplySelectionState(Button button, bool selected)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = selected
                ? new Color(0.7f, 0.84f, 0.55f, 1f)
                : new Color(0.18f, 0.29f, 0.33f, 0.95f);
        }

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.color = selected ? new Color(0.11f, 0.17f, 0.1f, 1f) : Color.white;
        }
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

    Canvas FindCanvas()
    {
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas != null && canvas.name == RootCanvasName)
            {
                return canvas;
            }
        }

        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas != null)
            {
                return canvas;
            }
        }

        return null;
    }

    void DeactivateLegacyChildren()
    {
        if (rootCanvas == null)
        {
            return;
        }

        foreach (Transform child in rootCanvas.transform)
        {
            bool keep = child.name == BackdropName || child.name == PanelName;
            if (!keep)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
