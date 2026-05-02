using System.Collections.Generic;
using System.Text;
using SergiosAdventure.Game.Data;
using UnityEngine;

namespace SergiosAdventure.Game.Core
{
    public static class GameSession
    {
        private const int DefaultMaxHealth = 24;
        private const int DefaultBaseDamage = 6;

        private static readonly Dictionary<string, InventoryEntry> Inventory = new Dictionary<string, InventoryEntry>();

        public static string PlayerName { get; private set; } = string.Empty;

        public static int MaxHealth { get; private set; } = DefaultMaxHealth;

        public static int CurrentHealth { get; private set; } = DefaultMaxHealth;

        public static int BaseDamage { get; private set; } = DefaultBaseDamage;

        public static bool HasActiveRun => !string.IsNullOrWhiteSpace(PlayerName);

        public static void StartNewRun(string playerName)
        {
            PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Aventurero" : playerName.Trim();
            MaxHealth = DefaultMaxHealth;
            CurrentHealth = DefaultMaxHealth;
            BaseDamage = DefaultBaseDamage;
            Inventory.Clear();
        }

        public static void ResetRun()
        {
            PlayerName = string.Empty;
            MaxHealth = DefaultMaxHealth;
            CurrentHealth = DefaultMaxHealth;
            BaseDamage = DefaultBaseDamage;
            Inventory.Clear();
        }

        public static int ApplyDamage(int amount)
        {
            var applied = Mathf.Max(0, amount);
            CurrentHealth = Mathf.Max(0, CurrentHealth - applied);
            return applied;
        }

        public static int ApplyHealing(int amount)
        {
            var target = Mathf.Max(0, amount);
            var previous = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + target, 0, MaxHealth);
            return CurrentHealth - previous;
        }

        public static int ApplyHealthDelta(int amount)
        {
            var previous = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
            return CurrentHealth - previous;
        }

        public static int ApplyDamageDelta(int amount)
        {
            BaseDamage = Mathf.Max(1, BaseDamage + amount);
            return BaseDamage;
        }

        public static void AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0)
            {
                return;
            }

            if (Inventory.TryGetValue(item.ItemId, out var entry))
            {
                entry.Count += amount;
                return;
            }

            Inventory[item.ItemId] = new InventoryEntry(item, amount);
        }

        public static bool ConsumeItem(ItemData item)
        {
            if (item == null || !Inventory.TryGetValue(item.ItemId, out var entry) || entry.Count <= 0)
            {
                return false;
            }

            entry.Count--;
            if (entry.Count <= 0)
            {
                Inventory.Remove(item.ItemId);
            }

            return true;
        }

        public static int GetItemCount(ItemData item)
        {
            if (item == null || !Inventory.TryGetValue(item.ItemId, out var entry))
            {
                return 0;
            }

            return entry.Count;
        }

        public static List<ItemData> GetHealingItems()
        {
            var items = new List<ItemData>();

            foreach (var pair in Inventory)
            {
                if (pair.Value.Item != null && pair.Value.Item.HealAmount > 0 && pair.Value.Count > 0)
                {
                    items.Add(pair.Value.Item);
                }
            }

            return items;
        }

        public static string GetInventorySummary()
        {
            if (Inventory.Count == 0)
            {
                return "Sin objetos";
            }

            var builder = new StringBuilder();

            foreach (var pair in Inventory)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(pair.Value.Item.DisplayName);
                builder.Append(" x");
                builder.Append(pair.Value.Count);
            }

            return builder.ToString();
        }

        private sealed class InventoryEntry
        {
            public InventoryEntry(ItemData item, int count)
            {
                Item = item;
                Count = count;
            }

            public ItemData Item { get; }

            public int Count { get; set; }
        }
    }
}
