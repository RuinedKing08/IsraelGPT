using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Runtime Story Graph")]
public class RuntimeStoryGraph : ScriptableObject
{
    public string startNodeId = "start";
    public List<RuntimeStoryNode> nodes = new List<RuntimeStoryNode>();

    Dictionary<string, RuntimeStoryNode> nodeLookup;

    public RuntimeStoryNode GetNode(string id)
    {
        EnsureLookup();
        return !string.IsNullOrWhiteSpace(id) && nodeLookup.TryGetValue(id, out RuntimeStoryNode node)
            ? node
            : null;
    }

    void EnsureLookup()
    {
        if (nodeLookup != null)
        {
            return;
        }

        nodeLookup = new Dictionary<string, RuntimeStoryNode>(StringComparer.Ordinal);
        foreach (RuntimeStoryNode node in nodes)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.id))
            {
                continue;
            }

            nodeLookup[node.id] = node;
        }
    }

    void OnValidate()
    {
        nodeLookup = null;
    }
}
