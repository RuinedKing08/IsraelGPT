using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GayManager : MonoBehaviour
{
    [SerializeField] private RuntimeStoryGraph storyGraph;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private TMP_Text choiceText1;
    [SerializeField] private TMP_Text choiceText2;

    private RuntimeStoryNode currentNode;
    //private Player player;

    private void Start()
    {
        //player = new Player("Jugador");
        LoadNode(storyGraph.startNodeId);
    }

    private void LoadNode(string nodeId)
    {
        currentNode = storyGraph.GetNode(nodeId);

        if (currentNode == null)
        {
            Debug.LogError("Node not found: " + nodeId);
            return;
        }

        titleText.text = currentNode.title;
        bodyText.text = currentNode.body;
        //statsText.text = $"HP: {player.Health} | DMG: {player.Damage}";

        SetupChoiceButton(choiceButton1, choiceText1, 0);
        SetupChoiceButton(choiceButton2, choiceText2, 1);
    }

    private void SetupChoiceButton(Button button, TMP_Text buttonText, int index)
    {
        bool hasChoice = index < currentNode.choices.Count;
        button.gameObject.SetActive(hasChoice);

        if (!hasChoice)
            return;

        RuntimeStoryChoice choice = currentNode.choices[index];
        buttonText.text = choice.text;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectChoice(choice));
    }

    private void SelectChoice(RuntimeStoryChoice choice)
    {
        //player.TakeDamage(-choice.healthChange);
        //player.IncreaseDamage(choice.damageChange);
        //player.IncreaseCourage(choice.courageChange);

        LoadNode(choice.nextNodeId);
    }
}
