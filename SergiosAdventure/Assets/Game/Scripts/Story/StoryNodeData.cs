using System;
using System.Collections.Generic;
using SergiosAdventure.Game.Data;
using UnityEngine;

namespace SergiosAdventure.Game.Story
{
    [Serializable]
    public sealed class StoryNodeData
    {
        [SerializeField] private string id = string.Empty;

        [SerializeField] private string title = "Titulo";

        [SerializeField] [TextArea(4, 8)] private string body = "Texto";

        [SerializeField] private StoryNodeType nodeType = StoryNodeType.Dialogue;

        [SerializeField] private List<StoryChoiceData> choices = new List<StoryChoiceData>();

        [SerializeField] private EnemyData enemy;

        [SerializeField] private string victoryNodeId = string.Empty;

        [SerializeField] private string defeatNodeId = string.Empty;

        [SerializeField] private EndingType endingType = EndingType.None;

        public string Id
        {
            get => id;
            set => id = value;
        }

        public string Title
        {
            get => title;
            set => title = value;
        }

        public string Body
        {
            get => body;
            set => body = value;
        }

        public StoryNodeType NodeType
        {
            get => nodeType;
            set => nodeType = value;
        }

        public List<StoryChoiceData> Choices
        {
            get => choices;
            set => choices = value;
        }

        public EnemyData Enemy
        {
            get => enemy;
            set => enemy = value;
        }

        public string VictoryNodeId
        {
            get => victoryNodeId;
            set => victoryNodeId = value;
        }

        public string DefeatNodeId
        {
            get => defeatNodeId;
            set => defeatNodeId = value;
        }

        public EndingType EndingType
        {
            get => endingType;
            set => endingType = value;
        }
    }
}
