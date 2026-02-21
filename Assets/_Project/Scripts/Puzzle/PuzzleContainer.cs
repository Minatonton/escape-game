using UnityEngine;
using EscapeGame.Data;
using EscapeGame.UI;

namespace EscapeGame.Room
{
    /// <summary>クリックでパズルUIを開くオブジェクト</summary>
    public class PuzzleContainer : InteractableObject
    {
        [SerializeField] private Puzzle.PuzzleBase puzzle;
        [SerializeField, TextArea] private string examineText = "何かを入力できるようだ。";

        protected override void OnInteract()
        {
            if (puzzle == null) return;

            if (puzzle.IsSolved)
            {
                ExaminePanel.Show("このパズルはすでに解かれている。");
                return;
            }

            ExaminePanel.Show(examineText);
            PuzzleUIPanel.Instance?.Open(puzzle);
        }

        protected override void OnItemUsed(ItemData item)
        {
            ExaminePanel.Show($"「{item.displayName}」をここに使っても何も起こらなかった。");
        }
    }
}
