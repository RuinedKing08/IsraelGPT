using UnityEngine;

public abstract class ConditionSO : ScriptableObject
{
    public abstract bool Evaluate(GameState state);
}
