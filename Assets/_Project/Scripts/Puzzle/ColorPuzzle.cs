using System.Collections.Generic;
using UnityEngine;
using EscapeGame.UI;

namespace EscapeGame.Puzzle
{
    /// <summary>色順序パズル（色を正しい順序で選択する）</summary>
    public class ColorPuzzle : PuzzleBase
    {
        [SerializeField] private string[] colorSequence;

        private readonly List<string> _input = new List<string>();

        public IReadOnlyList<string> CurrentInput => _input;

        public void SelectColor(string color)
        {
            if (IsSolved) return;
            if (colorSequence == null || _input.Count >= colorSequence.Length) return;

            int idx = _input.Count;
            _input.Add(color);

            if (color != colorSequence[idx])
            {
                _input.Clear();
                OnWrongAnswer();
                ColorPuzzleUIPanel.Instance?.RefreshDisplay();
                return;
            }

            ColorPuzzleUIPanel.Instance?.RefreshDisplay();

            if (_input.Count == colorSequence.Length)
                TrySolve();
        }

        public void ResetInput()
        {
            _input.Clear();
        }

        protected override bool ValidateSolution()
        {
            if (colorSequence == null || _input.Count != colorSequence.Length) return false;
            for (int i = 0; i < colorSequence.Length; i++)
                if (_input[i] != colorSequence[i]) return false;
            return true;
        }
    }
}
