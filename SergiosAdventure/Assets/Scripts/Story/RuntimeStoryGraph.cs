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

    public RuntimeStoryNode GetStartNode()
    {
        RuntimeStoryNode configuredStart = GetNode(startNodeId);
        if (configuredStart != null)
        {
            return configuredStart;
        }

        RuntimeStoryNode nodeNamedStart = GetNode("start");
        if (nodeNamedStart != null)
        {
            Debug.LogWarning($"Start node id '{startNodeId}' was not found. Falling back to node id 'start'.");
            return nodeNamedStart;
        }

        foreach (RuntimeStoryNode node in nodes)
        {
            if (node != null && !string.IsNullOrWhiteSpace(node.id))
            {
                Debug.LogWarning($"Start node id '{startNodeId}' was not found. Falling back to first node '{node.id}'.");
                return node;
            }
        }

        return null;
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
