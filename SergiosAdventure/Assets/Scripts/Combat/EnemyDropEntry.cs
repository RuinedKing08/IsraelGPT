using System;
using UnityEngine;

[Serializable]
public class EnemyDropEntry
{
    [SerializeField] ItemDefinition item;
    [Range(0f, 1f)]
    [SerializeField] float dropChance = 1f;

    public ItemDefinition Item => item;
    public float DropChance => Mathf.Clamp01(dropChance);
}
