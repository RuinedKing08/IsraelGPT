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
            return;
        }

        string graphPath = AssetDatabase.GetAssetPath(selectedObject);

        if (string.IsNullOrEmpty(graphPath))
        {
            return;
        }

        AdventureGraph editorGraph = GraphDatabase.LoadGraph<AdventureGraph>(graphPath);

        if (editorGraph == null)
        {
            return;
        }

        RuntimeStoryGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeStoryGraph>();
        runtimeGraph.nodes.Clear();

        foreach (RuntimeStoryNode node in editorGraph.runtimeNodes)
        {
            runtimeGraph.nodes.Add(node);
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Runtime Story Graph",
            "RuntimeStoryGraph",
            "asset",
            "Choose where to save the exported runtime graph."
        );

        if (string.IsNullOrEmpty(path))
            return;

        AssetDatabase.CreateAsset(runtimeGraph, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Runtime story graph exported successfully.");
    }
}