using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Narrative/Ending")]
public class EndingSO : ScriptableObject
{
    public string id;
    public string textKey;
    public List<ConditionSO> conditions = new List<ConditionSO>();
}
