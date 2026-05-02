using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CombatLogicTests
{
    [Test]
    public void ApplyDamage_NeverDropsBelowZero()
    {
        Assert.AreEqual(0, CombatMath.ApplyDamage(3, 10));
    }

    [Test]
    public void ApplyHealing_ClampsToMaximumHealth()
    {
        Assert.AreEqual(20, CombatMath.ApplyHealing(17, 20, 9));
    }

    [Test]
    public void RollDrops_UsesIndependentProbabilities()
    {
        ItemDefinition potion = ScriptableObject.CreateInstance<ItemDefinition>();
        SetField(potion, "itemId", "potion");
        SetField(potion, "displayName", "Pocion");
        SetField(potion, "healAmount", 5);

        EnemyDropEntry guaranteedDrop = new EnemyDropEntry();
        SetField(guaranteedDrop, "item", potion);
        SetField(guaranteedDrop, "dropChance", 1f);

        EnemyDropEntry failedDrop = new EnemyDropEntry();
        SetField(failedDrop, "item", potion);
        SetField(failedDrop, "dropChance", 0.25f);

        Queue<float> rolls = new Queue<float>(new[] { 0.1f, 0.8f });
        List<CombatItemStack> drops = CombatMath.RollDrops(new[] { guaranteedDrop, failedDrop }, () => rolls.Dequeue());

        Assert.AreEqual(1, drops.Count);
        Assert.AreEqual("potion", drops[0].Item.ItemId);
        Assert.AreEqual(1, drops[0].Quantity);
    }

    [Test]
    public void CombatSession_ResetsFromProfile_AndConsumesHealingItems()
    {
        ItemDefinition potion = ScriptableObject.CreateInstance<ItemDefinition>();
        SetField(potion, "itemId", "small_potion");
        SetField(potion, "displayName", "Pocion pequena");
        SetField(potion, "healAmount", 6);

        PlayerCombatProfile profile = ScriptableObject.CreateInstance<PlayerCombatProfile>();
        SetField(profile, "maxHealth", 20);
        SetField(profile, "baseDamage", 5);
        SetField(profile, "startingItems", new List<CombatItemStack> { new CombatItemStack(potion, 2) });

        CombatSession.ResetFromProfile(profile);
        CombatSession.ApplyDamage(9);

        bool usedItem = CombatSession.TryUseHealingItem(potion, out int healedAmount);

        Assert.IsTrue(usedItem);
        Assert.AreEqual(6, healedAmount);
        Assert.AreEqual(17, CombatSession.CurrentHealth);
        Assert.AreEqual(1, CombatSession.GetItemCount(potion));
    }

    static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"No se encontro el campo {fieldName}.");
        field.SetValue(target, value);
    }
}
