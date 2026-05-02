using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimeStoryNode
{
    [Header("Identity")]
    public string id;
    public string titleKey;
    public string title;
    public string bodyKey;
    [TextArea] public string body;
    public NodeType type;

    [Header("Combat")]
    public string enemyId;
    public string victoryNodeId;
    public string defeatNodeId;

    [Header("Ending")]
    public string endingId;
    public string endingName;

    [Header("Choices")]
    public List<RuntimeStoryChoice> choices = new List<RuntimeStoryChoice>();
}
