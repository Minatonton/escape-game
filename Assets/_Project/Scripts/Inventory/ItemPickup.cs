using UnityEngine;
using EscapeGame.Data;
using EscapeGame.Room;
using EscapeGame.UI;

namespace EscapeGame.Inventory
{
    /// <summary>クリックするとインベントリにアイテムを追加するオブジェクト</summary>
    public class ItemPickup : InteractableObject
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private bool pickedUp = false;

        protected override void OnInteract()
        {
            if (pickedUp) return;
            if (itemData == null) return;

            bool added = InventoryManager.Instance.AddItem(itemData);
            if (added)
            {
                pickedUp = true;
                ExaminePanel.Show($"「{itemData.displayName}」を手に入れた！");
                gameObject.SetActive(false);
            }
            else
            {
                ExaminePanel.Show("これ以上アイテムを持てない。");
            }
        }

        protected override void OnItemUsed(ItemData item) { }
    }
}
