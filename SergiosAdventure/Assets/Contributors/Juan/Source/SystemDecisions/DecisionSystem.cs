using System.Collections.Generic;

public class DecisionSystem
{
    private Dictionary<string, DecisionNodeSO> nodes;
    private GameState state;

    public DecisionNodeSO CurrentNode { get; private set; }

    public DecisionSystem(List<DecisionNodeSO> nodeList, GameState gameState)
    {
        nodes = new Dictionary<string, DecisionNodeSO>();

        foreach (DecisionNodeSO node in nodeList)
        {
            if (!nodes.ContainsKey(node.id))
            {
                nodes.Add(node.id, node);
            }
        }

        state = gameState;
    }

    public void Start(string startId)
    {
        CurrentNode = nodes[startId];
    }

    public List<ChoiceData> GetAvailableChoices()
    {
        List<ChoiceData> availableChoices = new List<ChoiceData>();

        foreach (ChoiceData choice in CurrentNode.choices)
        {
            bool isValid = true;

            foreach (ConditionSO condition in choice.conditions)
            {
                if (!condition.Evaluate(state))
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                availableChoices.Add(choice);
            }
        }

        return availableChoices;
    }

    public void Choose(ChoiceData choice)
    {
        foreach (EffectSO effect in choice.effects)
        {
            effect.Apply(state);
        }

        CurrentNode = nodes[choice.nextNodeId];
    }
}
