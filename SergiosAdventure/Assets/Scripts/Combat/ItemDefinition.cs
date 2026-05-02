using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Item Definition", fileName = "NewItemDefinition")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] string itemId = "item";
    [SerializeField] string displayName = "Item";
    [Min(0)]
    [SerializeField] int healAmount = 5;

    public string ItemId => string.IsNullOrWhiteSpace(itemId) ? name : itemId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public int HealAmount => Mathf.Max(0, healAmount);
}
