using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Player Combat Profile", fileName = "PlayerCombatProfile")]
public class PlayerCombatProfile : ScriptableObject
{
    [Min(1)]
    [SerializeField] int maxHealth = 20;
    [Min(1)]
    [SerializeField] int baseDamage = 4;
    [SerializeField] List<CombatItemStack> startingItems = new List<CombatItemStack>();

    public int MaxHealth => Mathf.Max(1, maxHealth);
    public int BaseDamage => Mathf.Max(1, baseDamage);
    public IReadOnlyList<CombatItemStack> StartingItems => startingItems;
}
