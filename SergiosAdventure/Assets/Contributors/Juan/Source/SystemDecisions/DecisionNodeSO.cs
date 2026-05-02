using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Narrative/Decision Node")]
public class DecisionNodeSO : ScriptableObject
{
    public string id;
    public string textKey;
    public List<ChoiceData> choices = new List<ChoiceData>();
}