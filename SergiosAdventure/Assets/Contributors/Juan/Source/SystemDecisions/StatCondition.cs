using UnityEngine;

[CreateAssetMenu(menuName = "Narrative/Condition/Stat Condition")]
public class StatCondition : ConditionSO
{
    public string statName;
    public int requiredValue;

    public override bool Evaluate(GameState state)
    {
        int value;
        if (state.Stats.TryGetValue(statName, out value))
        {
            return value >= requiredValue;
        }

        return false;
    }
}