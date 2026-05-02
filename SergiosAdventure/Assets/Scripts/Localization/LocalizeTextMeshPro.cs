using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizeTextMeshPro : MonoBehaviour
{
    const string DefaultTable = "StringTable";

    [SerializeField] LocalizedString stringReference = new LocalizedString
    {
        TableReference = DefaultTable
    };

    TextMeshProUGUI textComponent;
    AsyncOperationHandle<string>? activeHandle;
    bool waitingForInitialization;

    public string TableCollection
    {
        get => stringReference.TableReference;
        set => stringReference.TableReference = string.IsNullOrWhiteSpace(value) ? DefaultTable : value.Trim();
    }

    public string EntryKey
    {
        get => stringReference.TableEntryReference;
        set => stringReference.TableEntryReference = value;
    }

    public void Configure(string tableCollection, string entryKey)
    {
        TableCollection = tableCollection;
        EntryKey = entryKey;
        Refresh();
    }

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        Refresh();
        stringReference.StringChanged += OnStringChanged;
    }

    void OnDisable()
    {
        stringReference.StringChanged -= OnStringChanged;
        if (activeHandle.HasValue)
        {
            if (activeHandle.Value.IsValid())
            {
                activeHandle.Value.Completed -= OnCompleted;
            }

            activeHandle = null;
        }

        if (waitingForInitialization && LocalizationSettings.HasSettings)
        {
            LocalizationSettings.InitializationOperation.Completed -= HandleInitializationCompleted;
            waitingForInitialization = false;
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            Refresh();
        }
    }

    void Refresh()
    {
        if (this == null)
        {
            return;
        }

        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        if (activeHandle.HasValue)
        {
            if (activeHandle.Value.IsValid())
            {
                activeHandle.Value.Completed -= OnCompleted;
            }

            activeHandle = null;
        }

        if (string.IsNullOrWhiteSpace(EntryKey))
        {
            return;
        }

        if (!LocalizationSettings.HasSettings)
        {
            return;
        }

        if (!LocalizationSettings.InitializationOperation.IsDone)
        {
            if (!waitingForInitialization)
            {
                LocalizationSettings.InitializationOperation.Completed += HandleInitializationCompleted;
                waitingForInitialization = true;
            }

            return;
        }

        if (LocalizationSettings.SelectedLocale == null)
        {
            return;
        }

        AsyncOperationHandle<string> handle = stringReference.GetLocalizedStringAsync();
        activeHandle = handle;
        handle.Completed += OnCompleted;
    }

    void OnCompleted(AsyncOperationHandle<string> handle)
    {
        if (this != null && textComponent != null && handle.Status == AsyncOperationStatus.Succeeded)
        {
            textComponent.text = handle.Result;
        }
    }

    void OnStringChanged(string localizedString)
    {
        if (this != null && textComponent != null)
        {
            textComponent.text = localizedString;
        }
    }

    void HandleInitializationCompleted(AsyncOperationHandle<LocalizationSettings> _)
    {
        if (this == null || !LocalizationSettings.HasSettings)
        {
            return;
        }

        LocalizationSettings.InitializationOperation.Completed -= HandleInitializationCompleted;
        waitingForInitialization = false;
        Refresh();
    }
}
