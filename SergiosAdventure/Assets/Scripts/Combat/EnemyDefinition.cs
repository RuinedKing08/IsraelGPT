using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Enemy Definition", fileName = "NewEnemyDefinition")]
public class EnemyDefinition : ScriptableObject
{
    [SerializeField] string enemyName = "Enemy";
    [Min(1)]
    [SerializeField] int maxHealth = 10;
    [Min(0)]
    [SerializeField] int damage = 2;
    [SerializeField] List<EnemyDropEntry> drops = new List<EnemyDropEntry>();

    public string EnemyName => string.IsNullOrWhiteSpace(enemyName) ? name : enemyName;
    public int MaxHealth => Mathf.Max(1, maxHealth);
    public int Damage => Mathf.Max(0, damage);
    public IReadOnlyList<EnemyDropEntry> Drops => drops;
}
