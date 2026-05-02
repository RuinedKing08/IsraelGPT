using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class RegistroController : MonoBehaviour
{
    const string AdventureSceneName = "Adventure";
    const string RootCanvasName = "Registro Canvas";
    const string BackdropName = "Registro Backdrop";
    const string PanelName = "Registro Panel";
    const string TitleName = "Registro Title";
    const string BodyName = "Registro Body";
    const string HintName = "Registro Hint";

    [SerializeField] Canvas rootCanvas;
    [SerializeField] RectTransform backdrop;
    [SerializeField] RectTransform panel;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI bodyText;
    [SerializeField] TextMeshProUGUI hintText;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_FontAsset fontAsset;

    bool isTransitioning;

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
        ResolveReferences();
        EnsureBuilt();
    }

    void OnEnable()
    {
        ResolveReferences();
        EnsureBuilt();

        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(HandleSubmit);
        }
    }

    void Start()
    {
        PlayerSession.ClearCustomStats();

        if (Application.isPlaying && nameInputField != null)
        {
            StartCoroutine(FocusInputFieldNextFrame());
        }
    }

    void Update()
    {
        if (!Application.isPlaying || nameInputField == null || isTransitioning || !nameInputField.isFocused || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            HandleSubmit(nameInputField.text);
        }
    }

    void OnDisable()
    {
        if (nameInputField != null)
        {
            nameInputField.onSubmit.RemoveListener(HandleSubmit);
        }
    }

    void ResolveReferences()
    {
        if (nameInputField == null)
        {
            nameInputField = GetComponent<TMP_InputField>();
        }
    }

    void EnsureBuilt()
    {
        bool createdSomething = false;
        ResolveReferences();

        if (fontAsset == null)
        {
            fontAsset = SceneUiBuilder.ResolveFontAsset();
        }

        if (rootCanvas == null)
        {
            rootCanvas = FindCanvas();
        }

        rootCanvas = SceneUiBuilder.EnsureCanvas(transform, rootCanvas, RootCanvasName, 0, ref createdSomething);
        backdrop = SceneUiBuilder.EnsureRect(backdrop, rootCanvas.transform, BackdropName,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        SceneUiBuilder.EnsureImage(backdrop.gameObject, new Color(0.05f, 0.08f, 0.11f, 1f), ref createdSomething);

        panel = SceneUiBuilder.EnsureRect(panel, rootCanvas.transform, PanelName,
            new Vector2(0.24f, 0.18f), new Vector2(0.76f, 0.82f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);
        SceneUiBuilder.EnsureImage(panel.gameObject, new Color(0.08f, 0.13f, 0.16f, 0.96f), ref createdSomething);

        titleText = SceneUiBuilder.EnsureText(titleText, panel, TitleName, "Escribe tu nombre", 48f, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 0.76f), Vector2.one, new Vector2(40f, -48f), new Vector2(-40f, -16f), fontAsset, ref createdSomething);
        bodyText = SceneUiBuilder.EnsureText(bodyText, panel, BodyName,
            "La mazmorra recordara cada decision. Ingresa el nombre con el que enfrentaras la aventura.", 26f, FontStyles.Normal, TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.72f), Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);
        hintText = SceneUiBuilder.EnsureText(hintText, panel, HintName, "Presiona Enter para continuar", 22f, FontStyles.Italic, TextAlignmentOptions.Center,
            new Vector2(0.15f, 0.18f), new Vector2(0.85f, 0.28f), Vector2.zero, Vector2.zero, fontAsset, ref createdSomething);

        if (nameInputField != null)
        {
            RectTransform inputRect = nameInputField.GetComponent<RectTransform>();
            inputRect.SetParent(panel, false);
            inputRect.anchorMin = new Vector2(0.16f, 0.34f);
            inputRect.anchorMax = new Vector2(0.84f, 0.48f);
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            inputRect.localScale = Vector3.one;
            inputRect.localRotation = Quaternion.identity;

            Image inputBackground = nameInputField.GetComponent<Image>();
            if (inputBackground != null)
            {
                inputBackground.color = new Color(0.94f, 0.95f, 0.93f, 1f);
            }

            nameInputField.lineType = TMP_InputField.LineType.SingleLine;
            nameInputField.contentType = TMP_InputField.ContentType.Standard;

            TextMeshProUGUI inputText = nameInputField.textComponent as TextMeshProUGUI;
            if (inputText != null)
            {
                inputText.fontSize = 32f;
                inputText.alignment = TextAlignmentOptions.MidlineLeft;
                inputText.color = new Color(0.12f, 0.16f, 0.18f, 1f);
                if (fontAsset != null)
                {
                    inputText.font = fontAsset;
                }
            }

            TextMeshProUGUI placeholder = nameInputField.placeholder as TextMeshProUGUI;
            if (placeholder != null)
            {
                placeholder.fontSize = 30f;
                placeholder.fontStyle = FontStyles.Italic;
                placeholder.color = new Color(0.35f, 0.4f, 0.44f, 0.85f);
                if (fontAsset != null)
                {
                    placeholder.font = fontAsset;
                }

                SceneUiBuilder.EnsureLocalizer(placeholder, "REGISTER_PLACEHOLDER", ref createdSomething);
            }
        }

        SceneUiBuilder.EnsureLocalizer(titleText, "REGISTER_TITLE", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(bodyText, "REGISTER_BODY", ref createdSomething);
        SceneUiBuilder.EnsureLocalizer(hintText, "REGISTER_HINT", ref createdSomething);

        SceneUiBuilder.EnsureEventSystem(transform, ref createdSomething);
        DeactivateLegacyChildren();
        SceneUiBuilder.MarkSceneDirty(transform, createdSomething);
    }

    IEnumerator FocusInputFieldNextFrame()
    {
        yield return null;
        FocusInputField();
    }

    void HandleSubmit(string enteredName)
    {
        if (isTransitioning)
        {
            return;
        }

        string trimmedName = enteredName.Trim();
        if (trimmedName.Length == 0)
        {
            FocusInputField();
            return;
        }

        isTransitioning = true;
        PlayerSession.SetPlayerName(trimmedName);
        SceneManager.LoadScene(AdventureSceneName);
    }

    void FocusInputField()
    {
        if (nameInputField == null)
        {
            return;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(nameInputField.gameObject);
        }

        nameInputField.Select();
        nameInputField.ActivateInputField();
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
