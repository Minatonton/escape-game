using UnityEngine;
using EscapeGame.Data;
using EscapeGame.UI;

namespace EscapeGame.Room
{
    /// <summary>クリックで調査テキストを表示するオブジェクト</summary>
    public class ExaminableObject : InteractableObject
    {
        [SerializeField, TextArea] private string examineText = "特に変わったものは見当たらない。";

        protected override void OnInteract()
        {
            ExaminePanel.Show(examineText);
        }

        protected override void OnItemUsed(ItemData item)
        {
            ExaminePanel.Show($"「{item.displayName}」をここに使っても何も起こらなかった。");
        }
    }
}
