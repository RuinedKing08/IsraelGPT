using UnityEngine;

public abstract class EffectSO : ScriptableObject
{
    public abstract void Apply(GameState state);
}
