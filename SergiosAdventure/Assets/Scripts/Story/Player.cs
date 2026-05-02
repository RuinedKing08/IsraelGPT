using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Health,
    Attack,
    Bravery
}

public class Player : MonoBehaviour
{
    private string playerName;
    private int health;
    private int attack;
    private int bravery;

    private Stack<string> decisionTracker = new Stack<string>();
    private List<Item> inventory = new List<Item>();

    private void Awake()
    {
        EnsureCollections();
    }

    public void Initialize(string name, int initialHealth, int initialAttack, int initialBravery)
    {
        playerName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
        health = Mathf.Max(0, initialHealth);
        attack = Mathf.Max(0, initialAttack);
        bravery = initialBravery;
        decisionTracker.Clear();
        inventory.Clear();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        health -= amount;
        if (health < 0) health = 0;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        health += amount;
    }

    public void ModifyStat(StatType stat, int delta)
    {
        switch (stat)
        {
            case StatType.Health:
                health += delta;
                if (health < 0) health = 0;
                break;
            case StatType.Attack:
                attack += delta;
                if (attack < 0) attack = 0;
                break;
            case StatType.Bravery:
                bravery += delta;
                break;
        }
    }

    public void AddItem(Item item)
    {
        EnsureCollections();
        if (item == null) return;
        inventory.Add(item);
    }

    public void RemoveItem(Item item)
    {
        EnsureCollections();
        if (item != null && inventory.Contains(item))
        {
            inventory.Remove(item);
        }
    }

    public void PushDecision(string decisionId)
    {
        EnsureCollections();
        if (!string.IsNullOrWhiteSpace(decisionId))
        {
            decisionTracker.Push(decisionId);
        }
    }

    public string PopLastDecision()
    {
        EnsureCollections();
        return decisionTracker.Count > 0 ? decisionTracker.Pop() : null;
    }

    public string PeekLastDecision()
    {
        EnsureCollections();
        return decisionTracker.Count > 0 ? decisionTracker.Peek() : null;
    }

    public Stack<string> GetDecisionTracker()
    {
        EnsureCollections();
        return new Stack<string>(decisionTracker);
    }

    public string GetName() => playerName;
    public int GetHealth() => health;
    public int GetAttack() => attack;
    public int GetBravery() => bravery;
    public bool IsAlive() => health > 0;
    public int GetInventoryCount()
    {
        EnsureCollections();
        return inventory.Count;
    }

    public bool HasItem(Item item)
    {
        EnsureCollections();
        return inventory.Contains(item);
    }

    public int GetStatValue(StatType stat)
    {
        switch (stat)
        {
            case StatType.Health: return health;
            case StatType.Attack: return attack;
            case StatType.Bravery: return bravery;
            default: return 0;
        }
    }

    public Item GetItemAtIndex(int index)
    {
        EnsureCollections();
        if (index >= 0 && index < inventory.Count)
        {
            return inventory[index];
        }

        return null;
    }

    public List<Item> GetAllItems()
    {
        EnsureCollections();
        return new List<Item>(inventory);
    }

    private void EnsureCollections()
    {
        decisionTracker ??= new Stack<string>();
        inventory ??= new List<Item>();
    }
}
