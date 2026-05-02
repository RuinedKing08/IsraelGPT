using UnityEngine;

[System.Serializable]
public class RuntimeStoryChoice
{
    public string textKey;
    public string text;
    public string nextNodeId;
    public int healthChange;
    public int damageChange;
    public int courageChange;
    public string grantedItemId;
    public string flagToSet;
}
