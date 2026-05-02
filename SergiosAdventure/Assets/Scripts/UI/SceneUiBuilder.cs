using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public static class SceneUiBuilder
{
    public static TMP_FontAsset ResolveFontAsset()
    {
        if (TMP_Settings.defaultFontAsset != null)
        {
            return TMP_Settings.defaultFontAsset;
        }

        foreach (TMP_FontAsset fontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            if (fontAsset != null)
            {
                return fontAsset;
            }
        }

        return null;
    }

    public static Canvas EnsureCanvas(Transform owner, Canvas existingCanvas, string name, int sortOrder, ref bool createdSomething)
    {
        Canvas canvas = existingCanvas;
        if (canvas == null)
        {
            canvas = FindChildComponent<Canvas>(owner, name);
        }

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(owner, false);
            canvas = canvasObject.GetComponent<Canvas>();
            createdSomething = true;
        }

        canvas.gameObject.layer = LayerMask.NameToLayer("UI");
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    public static RectTransform EnsureRect(RectTransform existingRect, Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, ref bool createdSomething)
    {
        RectTransform rect = existingRect;
        if (rect == null)
        {
            rect = FindChildComponent<RectTransform>(parent, name);
        }

        if (rect == null)
        {
            GameObject rectObject = new GameObject(name, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            rect = rectObject.GetComponent<RectTransform>();
            createdSomething = true;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        return rect;
    }

    public static Image EnsureImage(GameObject target, Color color, ref bool createdSomething)
    {
        Image image = target.GetComponent<Image>();
        if (image == null)
        {
            image = target.AddComponent<Image>();
            createdSomething = true;
        }

        image.color = color;
        image.raycastTarget = true;
        return image;
    }

    public static TextMeshProUGUI EnsureText(TextMeshProUGUI existingText, Transform parent, string name, string fallbackText,
        float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TMP_FontAsset fontAsset, ref bool createdSomething)
    {
        TextMeshProUGUI text = existingText;
        if (text == null)
        {
            text = FindChildComponent<TextMeshProUGUI>(parent, name);
        }

        if (text == null)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            text = textObject.GetComponent<TextMeshProUGUI>();
            createdSomething = true;
        }

        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        text.text = fallbackText;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.color = Color.white;
        text.raycastTarget = false;
        if (fontAsset != null)
        {
            text.font = fontAsset;
        }

        return text;
    }

    public static Button EnsureButton(Button existingButton, Transform parent, string name, string fallbackLabel,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TMP_FontAsset fontAsset, ref bool createdSomething)
    {
        Button button = existingButton;
        if (button == null)
        {
            button = FindChildComponent<Button>(parent, name);
        }

        if (button == null)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            button = buttonObject.GetComponent<Button>();
            createdSomething = true;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        Image image = EnsureImage(button.gameObject, new Color(0.18f, 0.29f, 0.33f, 0.95f), ref createdSomething);
        image.type = Image.Type.Sliced;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.18f, 0.29f, 0.33f, 0.95f);
        colors.highlightedColor = new Color(0.27f, 0.42f, 0.47f, 1f);
        colors.pressedColor = new Color(0.13f, 0.2f, 0.24f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.22f, 0.22f, 0.22f, 0.5f);
        button.colors = colors;

        TextMeshProUGUI label = EnsureText(button.GetComponentInChildren<TextMeshProUGUI>(true), button.transform, "Label", fallbackLabel,
            30f, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(16f, 12f), new Vector2(-16f, -12f), fontAsset, ref createdSomething);
        label.text = fallbackLabel;
        label.raycastTarget = false;
        label.enableAutoSizing = true;
        label.fontSizeMin = 22f;
        label.fontSizeMax = 32f;

        return button;
    }

    public static RectTransform EnsureVerticalLayoutRoot(RectTransform existingRoot, Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float spacing, ref bool createdSomething)
    {
        RectTransform root = EnsureRect(existingRoot, parent, name, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);

        VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            createdSomething = true;
        }

        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(0, 0, 0, 0);

        ContentSizeFitter fitter = root.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = root.gameObject.AddComponent<ContentSizeFitter>();
            createdSomething = true;
        }

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        return root;
    }

    public static RectTransform EnsureHorizontalLayoutRoot(RectTransform existingRoot, Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float spacing, ref bool createdSomething)
    {
        RectTransform root = EnsureRect(existingRoot, parent, name, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, ref createdSomething);

        HorizontalLayoutGroup layout = root.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            createdSomething = true;
        }

        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(0, 0, 0, 0);

        return root;
    }

    public static LayoutElement EnsureLayoutElement(GameObject target, float preferredHeight, ref bool createdSomething)
    {
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = target.AddComponent<LayoutElement>();
            createdSomething = true;
        }

        layoutElement.preferredHeight = preferredHeight;
        layoutElement.minHeight = preferredHeight;
        return layoutElement;
    }

    public static void EnsureLocalizer(TextMeshProUGUI text, string entryKey, ref bool createdSomething)
    {
        if (text == null)
        {
            return;
        }

        LocalizeTextMeshPro localizer = text.GetComponent<LocalizeTextMeshPro>();
        if (localizer == null)
        {
            localizer = text.gameObject.AddComponent<LocalizeTextMeshPro>();
            createdSomething = true;
        }

        localizer.Configure("StringTable", entryKey);
    }

    public static EventSystem EnsureEventSystem(Transform owner, ref bool createdSomething)
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (eventSystem != null)
        {
            return eventSystem;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystemObject.transform.SetParent(owner, false);
        createdSomething = true;
        return eventSystemObject.GetComponent<EventSystem>();
    }

    public static void MarkSceneDirty(Transform owner, bool createdSomething)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && createdSomething && owner != null && owner.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(owner.gameObject.scene);
        }
#endif
    }

    static T FindChildComponent<T>(Transform root, string name) where T : Component
    {
        if (root == null)
        {
            return null;
        }

        foreach (T component in root.GetComponentsInChildren<T>(true))
        {
            if (component != null && component.name == name)
            {
                return component;
            }
        }

        return null;
    }
}
