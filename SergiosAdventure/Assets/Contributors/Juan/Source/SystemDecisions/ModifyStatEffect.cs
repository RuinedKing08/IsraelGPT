using UnityEngine;

[CreateAssetMenu(menuName = "Narrative/Effect/Modify Stat")]
public class ModifyStatEffect : EffectSO
{
    public string statName;
    public int amount;

    public override void Apply(GameState state)
    {
        if (!state.Stats.ContainsKey(statName))
        {
            state.Stats[statName] = 0;
        }

        state.Stats[statName] += amount;
    }
}