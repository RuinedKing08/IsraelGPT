using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

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

    private Stack<string> decisionTacker;

    public Player(string name, int initialHealth, int initialAttack, int initialBravery)
    {
        playerName = name;
        health = initialHealth;
        attack = initialAttack;
        bravery = initialBravery;
        decisionTacker = new Stack<string>();
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
    public void PushDecision(string decisionId)
    {
        decisionTacker.Push(decisionId);
    }
    public string PopLastDecision()
    {
        return decisionTacker.Count > 0 ? decisionTacker.Pop() : null;
    }
    public string PeekLastDecision()
    {
        return decisionTacker.Count > 0 ? decisionTacker.Peek() : null;
    }

    public Stack<string> GetDecisionTracker()
    {
        return new Stack<string>(decisionTacker);
    }

    public string GetName() => playerName;
    public int GetHealth() => health;
    public int GetAttack() => attack;
    public int GetBravery() => bravery;
    public bool IsAlive() => health > 0;

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
}
