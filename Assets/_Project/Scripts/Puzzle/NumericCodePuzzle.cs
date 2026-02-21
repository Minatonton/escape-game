using UnityEngine;
using EscapeGame.Data;
using EscapeGame.UI;

namespace EscapeGame.Puzzle
{
    /// <summary>数字コード入力パズル（4〜6桁）</summary>
    public class NumericCodePuzzle : PuzzleBase
    {
        [SerializeField] private PuzzleData puzzleData;
        [SerializeField] private int digitCount = 4;

        private string _currentInput = "";

        public int DigitCount => digitCount;
        public string CurrentInput => _currentInput;

        public void AppendDigit(int digit)
        {
            if (IsSolved) return;
            if (_currentInput.Length >= digitCount) return;
            _currentInput += digit.ToString();

            if (_currentInput.Length == digitCount)
                TrySolve();
        }

        public void ClearInput()
        {
            _currentInput = "";
        }

        public void DeleteLastDigit()
        {
            if (_currentInput.Length > 0)
                _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
        }

        protected override bool ValidateSolution()
        {
            if (puzzleData == null || puzzleData.answers == null) return false;
            foreach (var answer in puzzleData.answers)
            {
                if (_currentInput == answer) return true;
            }
            return false;
        }

        protected override void OnWrongAnswer()
        {
            base.OnWrongAnswer();
            _currentInput = "";
            PuzzleUIPanel.Instance?.RefreshDisplay();
        }
    }
}
