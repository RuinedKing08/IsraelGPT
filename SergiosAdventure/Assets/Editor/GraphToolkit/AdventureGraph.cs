using UnityEngine;
using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace SergiosAdventure.GraphToolkit.Editor
{
    [Graph(AssetExtension)]
    [Serializable]
    public class AdventureGraph : Graph
    {
        public const string AssetExtension = "adventuregraph";

        [MenuItem("Assets/Create/Adventure/Adventure Graph")]
        public static void CreateAdventureGraph()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<AdventureGraph>();
        }
    }
}
