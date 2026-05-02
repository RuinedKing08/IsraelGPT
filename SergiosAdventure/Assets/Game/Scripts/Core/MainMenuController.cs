using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SergiosAdventure.Game.Core
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button closeCreditsButton;
        [SerializeField] private GameObject creditsPanel;

        private void Awake()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OpenRegistro);
            }

            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(OpenCredits);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(ExitGame);
            }

            if (closeCreditsButton != null)
            {
                closeCreditsButton.onClick.AddListener(CloseCredits);
            }
        }

        private void Start()
        {
            GameSession.ResetRun();
            CloseCredits();
            Focus(startButton);
        }

        public void OpenRegistro()
        {
            SceneManager.LoadScene("Registro");
        }

        public void OpenCredits()
        {
            if (creditsPanel == null)
            {
                return;
            }

            creditsPanel.SetActive(true);
            Focus(closeCreditsButton);
        }

        public void CloseCredits()
        {
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }

            Focus(startButton);
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void Focus(Button button)
        {
            if (button == null || EventSystem.current == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
