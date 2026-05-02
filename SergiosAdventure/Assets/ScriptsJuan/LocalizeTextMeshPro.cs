using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LocalizeTextMeshPro : MonoBehaviour
{
    [SerializeField] private LocalizedString stringReference;
    private TextMeshProUGUI textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        stringReference.GetLocalizedStringAsync().Completed += OnCompleted;

        stringReference.StringChanged += OnStringChanged;
    }

    private void OnCompleted(AsyncOperationHandle<string> handle)
    {
        textComponent.text = handle.Result;
    }

    private void OnStringChanged(string localizedString)
    {
        textComponent.text = localizedString;
    }
}
