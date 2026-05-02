using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string itemName;

    public string Name => string.IsNullOrWhiteSpace(itemName) ? name : itemName;

    public void Initialize(string newName)
    {
        itemName = string.IsNullOrWhiteSpace(newName) ? name : newName.Trim();
    }
}
