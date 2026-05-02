using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SergiosAdventure.Game.Core
{
    public sealed class RegistroController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text feedbackText;

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(Submit);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(BackToMenu);
            }

            if (nameInput != null)
            {
                nameInput.onSubmit.AddListener(HandleSubmittedText);
            }
        }

        private void Start()
        {
            if (feedbackText != null)
            {
                feedbackText.text = string.Empty;
            }

            if (nameInput != null)
            {
                nameInput.text = string.Empty;
                nameInput.lineType = TMP_InputField.LineType.SingleLine;
                nameInput.ActivateInputField();
                nameInput.Select();
            }

            Focus(nameInput != null ? nameInput.gameObject : continueButton != null ? continueButton.gameObject : null);
        }

        public void Submit()
        {
            if (nameInput == null)
            {
                return;
            }

            var playerName = nameInput.text.Trim();
            if (string.IsNullOrWhiteSpace(playerName))
            {
                if (feedbackText != null)
                {
                    feedbackText.text = "Escribe un nombre para continuar.";
                }

                nameInput.ActivateInputField();
                nameInput.Select();
                return;
            }

            GameSession.StartNewRun(playerName);
            SceneManager.LoadScene("Adventure");
        }

        public void BackToMenu()
        {
            SceneManager.LoadScene("SampleScene");
        }

        private static void Focus(GameObject target)
        {
            if (target == null || EventSystem.current == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(target);
        }

        private void HandleSubmittedText(string _)
        {
            Submit();
        }
    }
}
