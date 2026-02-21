using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Data;
using EscapeGame.Inventory;
using EscapeGame.UI;

namespace EscapeGame.Room
{
    /// <summary>別の部屋へ遷移するドアオブジェクト</summary>
    public class DoorObject : InteractableObject
    {
        [SerializeField] private string targetSceneName;
        [SerializeField] private bool isLocked = true;
        [SerializeField] private bool triggerEnding = false;
        [SerializeField] private ItemData requiredKey;
        [SerializeField, TextArea] private string lockedMessage = "ドアはロックされている。";
        [SerializeField, TextArea] private string openMessage = "ドアが開いた！";
        [SerializeField] private UnityEvent onUnlocked;

        protected override void OnInteract()
        {
            if (isLocked)
            {
                ExaminePanel.Show(lockedMessage);
                return;
            }

            ExaminePanel.Show(openMessage);

            if (triggerEnding)
                Core.GameManager.Instance?.TriggerEnding();
            else if (!string.IsNullOrEmpty(targetSceneName))
                Core.SceneLoader.Instance?.LoadScene(targetSceneName);
        }

        protected override void OnItemUsed(ItemData item)
        {
            if (requiredKey != null && item == requiredKey)
            {
                InventoryManager.Instance.RemoveItem(item.itemId);
                Unlock();
            }
            else
            {
                ExaminePanel.Show($"「{item.displayName}」を使っても開かない。");
            }
        }

        /// <summary>パズルクリア時などに呼び出してドアを解錠する</summary>
        public void Unlock()
        {
            isLocked = false;
            ExaminePanel.Show("カチャリ――ロックが解除された！");
            onUnlocked?.Invoke();
        }
    }
}
