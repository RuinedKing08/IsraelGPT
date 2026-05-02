using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RegistroController : MonoBehaviour
{
    const string AdventureSceneName = "Adventure";

    [Header("Optional Stat Inputs")]
    [SerializeField] TMP_InputField healthInputField;
    [SerializeField] TMP_InputField damageInputField;
    [SerializeField] TMP_InputField braveryInputField;
    [SerializeField] int defaultHealth = 20;
    [SerializeField] int defaultDamage = 4;
    [SerializeField] int defaultBravery;

    TMP_InputField nameInputField;
    bool isTransitioning;

    void Awake()
    {
        nameInputField = GetComponent<TMP_InputField>();

        if (nameInputField == null)
        {
            Debug.LogError("RegistroController requires a TMP_InputField on the same GameObject.", this);
        }
    }

    void OnEnable()
    {
        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(HandleSubmit);
        }
    }

    void Start()
    {
        if (nameInputField != null)
        {
            StartCoroutine(FocusInputFieldNextFrame());
        }
    }

    void Update()
    {
        if (nameInputField == null || isTransitioning || !nameInputField.isFocused || Keyboard.current == null)
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
        PlayerSession.SetPlayerStats(
            ReadStat(healthInputField, defaultHealth, 1),
            ReadStat(damageInputField, defaultDamage, 1),
            ReadStat(braveryInputField, defaultBravery, 0));
        SceneManager.LoadScene(AdventureSceneName);
    }

    int ReadStat(TMP_InputField inputField, int fallbackValue, int minimumValue)
    {
        if (inputField == null || string.IsNullOrWhiteSpace(inputField.text))
        {
            return Mathf.Max(minimumValue, fallbackValue);
        }

        return int.TryParse(inputField.text.Trim(), out int value)
            ? Mathf.Max(minimumValue, value)
            : Mathf.Max(minimumValue, fallbackValue);
    }

    void FocusInputField()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(nameInputField.gameObject);
        }

        nameInputField.Select();
        nameInputField.ActivateInputField();
    }
}
