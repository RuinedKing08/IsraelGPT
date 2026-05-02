using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Runtime Story Graph")]
public class RuntimeStoryGraph : ScriptableObject
{
    public string startNodeId = "start";
    public List<RuntimeStoryNode> nodes = new List<RuntimeStoryNode>();

    public RuntimeStoryNode GetNode(string id)
    {
        return nodes.Find(node => node.id == id);
    }
}

