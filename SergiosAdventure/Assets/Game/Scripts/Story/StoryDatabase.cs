using System.Collections.Generic;
using UnityEngine;

namespace SergiosAdventure.Game.Story
{
    [CreateAssetMenu(fileName = "StoryDatabase", menuName = "SergiosAdventure/Story Database")]
    public sealed class StoryDatabase : ScriptableObject
    {
        [SerializeField] private List<StoryNodeData> nodes = new List<StoryNodeData>();

        public IReadOnlyList<StoryNodeData> Nodes => nodes;

        public List<StoryNodeData> MutableNodes => nodes;

        public StoryNodeData GetNode(string nodeId)
        {
            for (var index = 0; index < nodes.Count; index++)
            {
                if (nodes[index] != null && nodes[index].Id == nodeId)
                {
                    return nodes[index];
                }
            }

            return null;
        }
    }
}
