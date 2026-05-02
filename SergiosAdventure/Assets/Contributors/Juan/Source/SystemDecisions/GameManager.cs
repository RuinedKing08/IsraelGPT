using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<DecisionNodeSO> nodes;
    public List<EndingSO> endings;

    private DecisionSystem decisionSystem;
    private EndingManager endingManager;
    private GameState state;

    void Start()
    {
        state = new GameState();

        decisionSystem = new DecisionSystem(nodes, state);
        endingManager = new EndingManager(endings);

        decisionSystem.Start("node1");

        RunGame();
    }

    void RunGame()
    {
        for (int i = 0; i < 10; i++)
        {
            Debug.Log("Node: " + decisionSystem.CurrentNode.id);

            List<ChoiceData> choices = decisionSystem.GetAvailableChoices();

            if (choices.Count == 0)
            {
                break;
            }

            int index = Random.Range(0, choices.Count);
            decisionSystem.Choose(choices[index]);
        }

        EndingSO ending = endingManager.GetEnding(state);

        if (ending != null)
        {
            Debug.Log("FINAL: " + ending.id);
        }
        else
        {
            Debug.Log("No se llegó a ningún final");
        }
    }
}