using System;
using System.Collections.Generic;
using UnityEngine;

public static class CombatMath
{
    public static int ApplyDamage(int currentHealth, int damage)
    {
        return Mathf.Max(0, currentHealth - Mathf.Max(0, damage));
    }

    public static int ApplyHealing(int currentHealth, int maxHealth, int healAmount)
    {
        return Mathf.Clamp(currentHealth + Mathf.Max(0, healAmount), 0, Mathf.Max(0, maxHealth));
    }

    public static List<CombatItemStack> RollDrops(IEnumerable<EnemyDropEntry> drops, Func<float> rollProvider)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        var items = new Dictionary<string, ItemDefinition>(StringComparer.Ordinal);
        Func<float> safeRollProvider = rollProvider ?? (() => UnityEngine.Random.value);

        if (drops == null)
        {
            return new List<CombatItemStack>();
        }

        foreach (EnemyDropEntry drop in drops)
        {
            if (drop == null || drop.Item == null)
            {
                continue;
            }

            float roll = Mathf.Clamp01(safeRollProvider());
            if (roll > drop.DropChance)
            {
                continue;
            }

            string key = drop.Item.ItemId;
            items[key] = drop.Item;
            counts[key] = counts.TryGetValue(key, out int currentCount) ? currentCount + 1 : 1;
        }

        var rewards = new List<CombatItemStack>();
        foreach ((string key, int count) in counts)
        {
            rewards.Add(new CombatItemStack(items[key], count));
        }

        return rewards;
    }
}
