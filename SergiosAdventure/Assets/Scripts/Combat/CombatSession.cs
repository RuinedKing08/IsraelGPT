using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CombatSession
{
    sealed class InventoryRecord
    {
        public InventoryRecord(ItemDefinition item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public ItemDefinition Item { get; }
        public int Quantity { get; set; }
    }

    const int DefaultMaxHealth = 20;
    const int DefaultBaseDamage = 4;

    static readonly Dictionary<string, InventoryRecord> inventory = new Dictionary<string, InventoryRecord>(StringComparer.Ordinal);

    static bool initialized;
    static int currentHealth = DefaultMaxHealth;
    static int maxHealth = DefaultMaxHealth;
    static int baseDamage = DefaultBaseDamage;
    static PlayerCombatProfile activeProfile;

    public static bool IsInitialized => initialized;
    public static int CurrentHealth => currentHealth;
    public static int MaxHealth => maxHealth;
    public static int BaseDamage => baseDamage;
    public static PlayerCombatProfile ActiveProfile => activeProfile;

    public static void Initialize(PlayerCombatProfile profile)
    {
        if (initialized)
        {
            return;
        }

        ResetFromProfile(profile);
    }

    public static void ResetFromProfile(PlayerCombatProfile profile)
    {
        activeProfile = profile;
        maxHealth = profile != null ? profile.MaxHealth : DefaultMaxHealth;
        baseDamage = profile != null ? profile.BaseDamage : DefaultBaseDamage;
        currentHealth = maxHealth;
        inventory.Clear();

        if (profile != null)
        {
            foreach (CombatItemStack itemStack in profile.StartingItems)
            {
                if (itemStack?.Item == null || itemStack.Quantity <= 0)
                {
                    continue;
                }

                AddItem(itemStack.Item, itemStack.Quantity);
            }
        }

        initialized = true;
    }

    public static int ApplyDamage(int damage)
    {
        currentHealth = CombatMath.ApplyDamage(currentHealth, damage);
        return currentHealth;
    }

    public static int GetItemCount(ItemDefinition item)
    {
        if (item == null)
        {
            return 0;
        }

        return inventory.TryGetValue(item.ItemId, out InventoryRecord record) ? record.Quantity : 0;
    }

    public static void AddItem(ItemDefinition item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            return;
        }

        string key = item.ItemId;
        if (inventory.TryGetValue(key, out InventoryRecord record))
        {
            record.Quantity += quantity;
            return;
        }

        inventory[key] = new InventoryRecord(item, quantity);
    }

    public static bool TryUseHealingItem(ItemDefinition item, out int healedAmount)
    {
        healedAmount = 0;
        if (item == null || !inventory.TryGetValue(item.ItemId, out InventoryRecord record) || record.Quantity <= 0)
        {
            return false;
        }

        int previousHealth = currentHealth;
        currentHealth = CombatMath.ApplyHealing(currentHealth, maxHealth, item.HealAmount);
        healedAmount = currentHealth - previousHealth;
        record.Quantity -= 1;

        if (record.Quantity <= 0)
        {
            inventory.Remove(item.ItemId);
        }

        return true;
    }

    public static List<CombatItemStack> GetUsableHealingItems()
    {
        return inventory.Values
            .Where(record => record.Item != null && record.Quantity > 0 && record.Item.HealAmount > 0)
            .OrderBy(record => record.Item.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(record => new CombatItemStack(record.Item, record.Quantity))
            .ToList();
    }

    public static List<CombatItemStack> GetInventorySnapshot()
    {
        return inventory.Values
            .Where(record => record.Item != null && record.Quantity > 0)
            .OrderBy(record => record.Item.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(record => new CombatItemStack(record.Item, record.Quantity))
            .ToList();
    }
}
