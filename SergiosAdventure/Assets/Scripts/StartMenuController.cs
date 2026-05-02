using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    const string StartButtonLabel = "Start";
    const string RegistroSceneName = "Registro";

    Button startButton;
    bool listenerRegistered;

    void Awake()
    {
        RegisterStartButton();
    }

    void OnEnable()
    {
        RegisterStartButton();
    }

    void OnDisable()
    {
        if (startButton != null && listenerRegistered)
        {
            startButton.onClick.RemoveListener(OpenRegistro);
            listenerRegistered = false;
        }
    }

    public void OpenRegistro()
    {
        SceneManager.LoadScene(RegistroSceneName);
    }

    void RegisterStartButton()
    {
        if (listenerRegistered)
        {
            return;
        }

        startButton = FindStartButton();
        if (startButton == null)
        {
            Debug.LogWarning("StartMenuController could not find the Start button in SampleScene.", this);
            return;
        }

        startButton.onClick.AddListener(OpenRegistro);
        listenerRegistered = true;
    }

    Button FindStartButton()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null && string.Equals(label.text.Trim(), StartButtonLabel, StringComparison.OrdinalIgnoreCase))
            {
                return button;
            }
        }

        return null;
    }
}
