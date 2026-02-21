using UnityEngine;
using EscapeGame.UI;

namespace EscapeGame.Room
{
    /// <summary>解錠時にアイテムを出現させる金庫・引き出し等</summary>
    public class SafeObject : MonoBehaviour
    {
        [SerializeField] private GameObject itemToReveal;
        [SerializeField, TextArea] private string unlockMessage = "開いた！中に何かある。";

        public void Unlock()
        {
            if (itemToReveal != null)
                itemToReveal.SetActive(true);
            ExaminePanel.Show(unlockMessage);
        }
    }
}
