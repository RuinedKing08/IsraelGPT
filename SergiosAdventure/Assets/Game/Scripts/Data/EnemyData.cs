using System.Collections.Generic;
using UnityEngine;

namespace SergiosAdventure.Game.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "SergiosAdventure/Enemy Data")]
    public sealed class EnemyData : ScriptableObject
    {
        [SerializeField] private string enemyName = "Enemigo";

        [SerializeField] private int maxHealth = 10;

        [SerializeField] private int damage = 2;

        [SerializeField] private List<EnemyDropEntry> drops = new List<EnemyDropEntry>();

        public string EnemyName
        {
            get => enemyName;
            set => enemyName = value;
        }

        public int MaxHealth
        {
            get => maxHealth;
            set => maxHealth = value;
        }

        public int Damage
        {
            get => damage;
            set => damage = value;
        }

        public List<EnemyDropEntry> Drops
        {
            get => drops;
            set => drops = value;
        }
    }
}
