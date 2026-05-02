using UnityEngine;

namespace SergiosAdventure.Game.Data
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "SergiosAdventure/Item Data")]
    public sealed class ItemData : ScriptableObject
    {
        [SerializeField] private string itemId = "item";

        [SerializeField] private string displayName = "Objeto";

        [SerializeField] [TextArea(2, 4)] private string description = "Descripcion";

        [SerializeField] private int healAmount = 0;

        public string ItemId
        {
            get => itemId;
            set => itemId = value;
        }

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        public string Description
        {
            get => description;
            set => description = value;
        }

        public int HealAmount
        {
            get => healAmount;
            set => healAmount = value;
        }
    }
}
