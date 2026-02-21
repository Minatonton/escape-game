using UnityEngine;
using EscapeGame.Data;
using EscapeGame.UI;

namespace EscapeGame.Room
{
    /// <summary>クリックでパズルUIを開くオブジェクト（複数パズル種対応）</summary>
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

            if (puzzle is Puzzle.NumericCodePuzzle)
                PuzzleUIPanel.Instance?.Open(puzzle);
            else if (puzzle is Puzzle.SequencePuzzle)
                SequencePuzzleUIPanel.Instance?.Open(puzzle);
            else if (puzzle is Puzzle.ColorPuzzle)
                ColorPuzzleUIPanel.Instance?.Open(puzzle);
        }

        protected override void OnItemUsed(ItemData item)
        {
            ExaminePanel.Show($"「{item.displayName}」をここに使っても何も起こらなかった。");
        }
    }
}
