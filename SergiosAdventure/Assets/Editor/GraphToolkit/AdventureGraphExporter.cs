using UnityEditor;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using SergiosAdventure.GraphToolkit.Editor;

public static class AdventureGraphExporter
{
    [MenuItem("Tools/Adventure/Export Selected Graph To Runtime Graph")]
    public static void ExportSelectedGraph()
    {
        UnityEngine.Object selectedObject = Selection.activeObject;

        if (selectedObject == null)
        {
            Debug.LogWarning("Select an Adventure Graph asset before exporting.");
            return;
        }

        string graphPath = AssetDatabase.GetAssetPath(selectedObject);

        if (string.IsNullOrEmpty(graphPath))
        {
            Debug.LogWarning("The selected object is not a project asset.");
            return;
        }

        AdventureGraph editorGraph = GraphDatabase.LoadGraph<AdventureGraph>(graphPath);

        if (editorGraph == null)
        {
            Debug.LogWarning("The selected asset is not an Adventure Graph.");
            return;
        }

        RuntimeStoryGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeStoryGraph>();
        runtimeGraph.startNodeId = ResolveStartNodeId(editorGraph);
        runtimeGraph.nodes.Clear();

        foreach (RuntimeStoryNode node in editorGraph.runtimeNodes)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.id))
            {
                continue;
            }

            runtimeGraph.nodes.Add(CloneNode(node));
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Runtime Story Graph",
            "RuntimeStoryGraph",
            "asset",
            "Choose where to save the exported runtime graph."
        );

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        AssetDatabase.CreateAsset(runtimeGraph, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Runtime story graph exported successfully to {path}.");
    }

    private static string ResolveStartNodeId(AdventureGraph editorGraph)
    {
        foreach (RuntimeStoryNode node in editorGraph.runtimeNodes)
        {
            if (node != null && node.id == "start")
            {
                return node.id;
            }
        }

        return editorGraph.runtimeNodes.Count > 0 && editorGraph.runtimeNodes[0] != null
            ? editorGraph.runtimeNodes[0].id
            : "start";
    }

    private static RuntimeStoryNode CloneNode(RuntimeStoryNode source)
    {
        RuntimeStoryNode clone = new RuntimeStoryNode
        {
            id = source.id,
            title = source.title,
            body = source.body,
            type = source.type,
            enemyId = source.enemyId
        };

        foreach (RuntimeStoryChoice choice in source.choices)
        {
            if (choice == null)
            {
                continue;
            }

            clone.choices.Add(new RuntimeStoryChoice
            {
                text = choice.text,
                nextNodeId = choice.nextNodeId,
                healthChange = choice.healthChange,
                damageChange = choice.damageChange,
                courageChange = choice.courageChange,
                grantedItemId = choice.grantedItemId,
                flagToSet = choice.flagToSet
            });
        }

        return clone;
    }
}
