using System;
using System.Collections.Generic;
using SergiosAdventure.Game.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SergiosAdventure.Game.UI
{
    public sealed class CombatView : MonoBehaviour
    {
        [Serializable]
        public sealed class ItemButtonView
        {
            public Button Button;
            public TMP_Text Label;
        }

        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text logText;
        [SerializeField] private TMP_Text playerStatsText;
        [SerializeField] private TMP_Text enemyStatsText;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button itemsButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text continueButtonText;
        [SerializeField] private GameObject actionRow;
        [SerializeField] private GameObject itemPanel;
        [SerializeField] private Button itemBackButton;
        [SerializeField] private ItemButtonView[] itemButtons;

        public void Show()
        {
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        public void SetHeader(string title, string playerStats, string enemyStats, string log)
        {
            titleText.text = title;
            playerStatsText.text = playerStats;
            enemyStatsText.text = enemyStats;
            logText.text = log;
        }

        public void ShowActionState(Action onAttack, Action onItems)
        {
            actionRow.SetActive(true);
            itemPanel.SetActive(false);
            continueButton.gameObject.SetActive(false);

            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(() => onAttack?.Invoke());

            itemsButton.onClick.RemoveAllListeners();
            itemsButton.onClick.AddListener(() => onItems?.Invoke());

            FocusButton(attackButton);
        }

        public void ShowItems(IReadOnlyList<ItemData> items, Func<ItemData, string> itemLabelFactory, Action<int> onUseItem, Action onBack)
        {
            actionRow.SetActive(false);
            itemPanel.SetActive(true);
            continueButton.gameObject.SetActive(false);

            for (var index = 0; index < itemButtons.Length; index++)
            {
                var view = itemButtons[index];
                var isVisible = items != null && index < items.Count && items[index] != null;
                view.Button.gameObject.SetActive(isVisible);
                view.Button.onClick.RemoveAllListeners();

                if (!isVisible)
                {
                    continue;
                }

                view.Label.text = itemLabelFactory(items[index]);
                var capturedIndex = index;
                view.Button.onClick.AddListener(() => onUseItem?.Invoke(capturedIndex));
            }

            itemBackButton.onClick.RemoveAllListeners();
            itemBackButton.onClick.AddListener(() => onBack?.Invoke());

            foreach (var view in itemButtons)
            {
                if (view.Button.gameObject.activeInHierarchy)
                {
                    FocusButton(view.Button);
                    return;
                }
            }

            FocusButton(itemBackButton);
        }

        public void ShowContinue(string buttonLabel, Action onContinue)
        {
            actionRow.SetActive(false);
            itemPanel.SetActive(false);
            continueButton.gameObject.SetActive(true);
            continueButtonText.text = buttonLabel;
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => onContinue?.Invoke());
            FocusButton(continueButton);
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
