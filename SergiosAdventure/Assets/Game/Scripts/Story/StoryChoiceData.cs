using System;
using SergiosAdventure.Game.Data;
using UnityEngine;

namespace SergiosAdventure.Game.Story
{
    [Serializable]
    public sealed class StoryChoiceData
    {
        [SerializeField] private string label = "Continuar";

        [SerializeField] private string nextNodeId = string.Empty;

        [SerializeField] private int healthDelta = 0;

        [SerializeField] private int damageDelta = 0;

        [SerializeField] private ItemData grantedItem;

        public string Label
        {
            get => label;
            set => label = value;
        }

        public string NextNodeId
        {
            get => nextNodeId;
            set => nextNodeId = value;
        }

        public int HealthDelta
        {
            get => healthDelta;
            set => healthDelta = value;
        }

        public int DamageDelta
        {
            get => damageDelta;
            set => damageDelta = value;
        }

        public ItemData GrantedItem
        {
            get => grantedItem;
            set => grantedItem = value;
        }
    }
}
