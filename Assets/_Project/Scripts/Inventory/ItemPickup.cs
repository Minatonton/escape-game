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
        [SerializeField, TextArea(2, 6)] private string pickupDescription;
        [SerializeField] private bool pickedUp = false;

        protected override void OnInteract()
        {
            if (pickedUp) return;
            if (itemData == null) return;

            bool added = InventoryManager.Instance.AddItem(itemData);
            if (added)
            {
                pickedUp = true;
                string msg = string.IsNullOrEmpty(pickupDescription)
                    ? $"「{itemData.displayName}」を手に入れた！"
                    : $"{pickupDescription}\n\n「{itemData.displayName}」を手に入れた！";
                ExaminePanel.Show(msg, 6f);
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
