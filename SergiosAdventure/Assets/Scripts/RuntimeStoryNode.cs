using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimeStoryNode
{
    public string id;
    public string title;
    [TextArea] public string body;
    public NodeType type;
    public string enemyId;
    public List<RuntimeStoryChoice> choices = new List<RuntimeStoryChoice>();
}
