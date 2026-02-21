using UnityEngine;

namespace EscapeGame.Data
{
    public enum ItemCategory
    {
        Key,
        Tool,
        Information,
        Material
    }

    [CreateAssetMenu(menuName = "EscapeGame/Item", fileName = "New Item")]
    public class ItemData : ScriptableObject
    {
        public string itemId;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public ItemCategory category;
        public bool isConsumable;

        // 合成レシピ（このアイテムが生成される場合）
        public ItemData combineIngredientA;
        public ItemData combineIngredientB;
    }
}
