using System;
using SergiosAdventure.Game.Story;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SergiosAdventure.Game.UI
{
    public sealed class DialogueView : MonoBehaviour
    {
        [Serializable]
        public sealed class ChoiceButtonView
        {
            public Button Button;
            public TMP_Text Label;
        }

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private ChoiceButtonView[] choiceButtons;
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private TMP_Text endingTitleText;
        [SerializeField] private TMP_Text endingBodyText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        public void ShowDialogueNode(StoryNodeData node, string status, Action<int> onChoiceSelected)
        {
            HideEnding();
            ApplyText(node.Title, node.Body, status);
            PopulateChoices(node.Choices, onChoiceSelected);
        }

        public void ShowCombatNode(StoryNodeData node, string status, Action onStartCombat)
        {
            HideEnding();
            ApplyText(node.Title, node.Body, status);

            for (var index = 0; index < choiceButtons.Length; index++)
            {
                var view = choiceButtons[index];
                var isPrimary = index == 0;
                view.Button.gameObject.SetActive(isPrimary);
                view.Button.onClick.RemoveAllListeners();

                if (!isPrimary)
                {
                    continue;
                }

                view.Label.text = "Enfrentar al guardian";
                view.Button.onClick.AddListener(() => onStartCombat?.Invoke());
            }

            FocusFirstVisibleChoice();
        }

        public void ShowEnding(StoryNodeData node, string status, string summary, Action onRestart, Action onBackToMenu)
        {
            ApplyText(node.Title, node.Body, status);

            foreach (var view in choiceButtons)
            {
                view.Button.onClick.RemoveAllListeners();
                view.Button.gameObject.SetActive(false);
            }

            endingPanel.SetActive(true);
            endingTitleText.text = node.EndingType switch
            {
                EndingType.Good => "Final bueno",
                EndingType.Neutral => "Final neutral",
                EndingType.Bad => "Final malo",
                _ => "Fin",
            };
            endingBodyText.text = summary;

            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => onRestart?.Invoke());

            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => onBackToMenu?.Invoke());

            FocusButton(restartButton);
        }

        public void HideEnding()
        {
            if (endingPanel != null)
            {
                endingPanel.SetActive(false);
            }
        }

        private void ApplyText(string title, string body, string status)
        {
            titleText.text = title;
            bodyText.text = body;
            statusText.text = status;
        }

        private void PopulateChoices(System.Collections.Generic.IReadOnlyList<StoryChoiceData> choices, Action<int> onChoiceSelected)
        {
            for (var index = 0; index < choiceButtons.Length; index++)
            {
                var view = choiceButtons[index];
                var isVisible = choices != null && index < choices.Count && choices[index] != null;
                view.Button.gameObject.SetActive(isVisible);
                view.Button.onClick.RemoveAllListeners();

                if (!isVisible)
                {
                    continue;
                }

                view.Label.text = choices[index].Label;
                var capturedIndex = index;
                view.Button.onClick.AddListener(() => onChoiceSelected?.Invoke(capturedIndex));
            }

            FocusFirstVisibleChoice();
        }

        private void FocusFirstVisibleChoice()
        {
            foreach (var view in choiceButtons)
            {
                if (view.Button.gameObject.activeInHierarchy)
                {
                    FocusButton(view.Button);
                    return;
                }
            }
        }

        private static void FocusButton(Button button)
        {
            if (button == null || EventSystem.current == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
