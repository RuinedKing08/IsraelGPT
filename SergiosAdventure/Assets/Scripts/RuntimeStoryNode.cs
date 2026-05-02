using System.Collections.Generic;
using UnityEngine;
using static TreeEditor.TreeEditorHelper;

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
