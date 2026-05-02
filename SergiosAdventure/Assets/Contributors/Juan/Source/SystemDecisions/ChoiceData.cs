using System;
using System.Collections.Generic;

[Serializable]
public class ChoiceData
{
    public string textKey;
    public string nextNodeId;

    public List<ConditionSO> conditions = new List<ConditionSO>();
    public List<EffectSO> effects = new List<EffectSO>();
}
