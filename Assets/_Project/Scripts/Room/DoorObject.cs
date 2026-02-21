using UnityEngine;
using EscapeGame.Data;
using EscapeGame.UI;

namespace EscapeGame.Room
{
    /// <summary>別の部屋へ遷移するドアオブジェクト</summary>
    public class DoorObject : InteractableObject
    {
        [SerializeField] private string targetSceneName;
        [SerializeField] private bool isLocked = true;
        [SerializeField, TextArea] private string lockedMessage = "ドアはロックされている。";
        [SerializeField, TextArea] private string openMessage = "ドアが開いた！";

        protected override void OnInteract()
        {
            if (isLocked)
            {
                ExaminePanel.Show(lockedMessage);
                return;
            }

            ExaminePanel.Show(openMessage);

            if (!string.IsNullOrEmpty(targetSceneName))
                Core.SceneLoader.Instance?.LoadScene(targetSceneName);
        }

        protected override void OnItemUsed(ItemData item)
        {
            ExaminePanel.Show($"「{item.displayName}」を使っても開かない。");
        }

        /// <summary>パズルクリア時などに呼び出してドアを解錠する</summary>
        public void Unlock()
        {
            isLocked = false;
            ExaminePanel.Show("カチャリ――ロックが解除された！");
        }
    }
}
