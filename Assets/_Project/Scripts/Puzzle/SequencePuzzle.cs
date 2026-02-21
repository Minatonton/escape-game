using System.Collections.Generic;
using UnityEngine;
using EscapeGame.UI;

namespace EscapeGame.Puzzle
{
    /// <summary>順序入力パズル（ボタンを正しい順序で押す）</summary>
    public class SequencePuzzle : PuzzleBase
    {
        [SerializeField] private int[] correctSequence;

        private readonly List<int> _input = new List<int>();

        public IReadOnlyList<int> CurrentInput => _input;

        public void PressButton(int id)
        {
            if (IsSolved) return;
            if (correctSequence == null || _input.Count >= correctSequence.Length) return;

            int idx = _input.Count;
            _input.Add(id);

            if (id != correctSequence[idx])
            {
                _input.Clear();
                OnWrongAnswer();
                SequencePuzzleUIPanel.Instance?.RefreshDisplay();
                return;
            }

            SequencePuzzleUIPanel.Instance?.RefreshDisplay();

            if (_input.Count == correctSequence.Length)
                TrySolve();
        }

        public void ResetInput()
        {
            _input.Clear();
        }

        protected override bool ValidateSolution()
        {
            if (correctSequence == null || _input.Count != correctSequence.Length) return false;
            for (int i = 0; i < correctSequence.Length; i++)
                if (_input[i] != correctSequence[i]) return false;
            return true;
        }
    }
}
