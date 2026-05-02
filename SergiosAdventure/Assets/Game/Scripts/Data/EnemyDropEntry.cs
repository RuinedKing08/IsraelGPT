using System;
using UnityEngine;

namespace SergiosAdventure.Game.Data
{
    [Serializable]
    public sealed class EnemyDropEntry
    {
        [SerializeField] private ItemData item;

        [SerializeField] [Range(0f, 1f)] private float dropChance = 1f;

        public ItemData Item
        {
            get => item;
            set => item = value;
        }

        public float DropChance
        {
            get => dropChance;
            set => dropChance = value;
        }
    }
}
