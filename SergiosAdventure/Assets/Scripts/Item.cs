using UnityEngine;

public class Item : MonoBehaviour
{
    public string Name { get; private set; }
    public Item(string name) { Name = name; }
}
