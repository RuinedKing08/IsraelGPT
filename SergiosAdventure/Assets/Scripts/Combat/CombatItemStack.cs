using System;
using UnityEngine;

[Serializable]
public class CombatItemStack
{
    [SerializeField] ItemDefinition item;
    [Min(1)]
    [SerializeField] int quantity = 1;

    public ItemDefinition Item => item;
    public int Quantity => Mathf.Max(0, quantity);

    public CombatItemStack()
    {
    }

    public CombatItemStack(ItemDefinition item, int quantity)
    {
        this.item = item;
        this.quantity = Mathf.Max(1, quantity);
    }
}
