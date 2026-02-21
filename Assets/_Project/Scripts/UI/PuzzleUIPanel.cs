using UnityEngine;
using UnityEngine.UI;
using EscapeGame.Puzzle;

namespace EscapeGame.UI
{
    /// <summary>数字コード入力UIパネル</summary>
    public class PuzzleUIPanel : MonoBehaviour
    {
        public static PuzzleUIPanel Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private Text displayText;
        [SerializeField] private Text titleText;

        private NumericCodePuzzle _currentPuzzle;

        void Awake()
        {
            Instance = this;
            panel.SetActive(false);
        }

        public void Open(PuzzleBase puzzle)
        {
            _currentPuzzle = puzzle as NumericCodePuzzle;
            if (_currentPuzzle == null) return;

            _currentPuzzle.ClearInput();
            RefreshDisplay();
            panel.SetActive(true);
        }

        public void Close()
        {
            panel.SetActive(false);
            _currentPuzzle = null;
        }

        public void RefreshDisplay()
        {
            if (_currentPuzzle == null) return;
            string input = _currentPuzzle.CurrentInput;
            int remaining = _currentPuzzle.DigitCount - input.Length;
            displayText.text = input + new string('_', remaining);
        }

        public void OnDigitButtonClicked(int digit)
        {
            if (_currentPuzzle == null) return;
            _currentPuzzle.AppendDigit(digit);
            RefreshDisplay();

            if (_currentPuzzle.IsSolved)
                Close();
        }

        public void OnDeleteButtonClicked()
        {
            _currentPuzzle?.DeleteLastDigit();
            RefreshDisplay();
        }

        public void OnClearButtonClicked()
        {
            _currentPuzzle?.ClearInput();
            RefreshDisplay();
        }

        public void OnCloseButtonClicked()
        {
            Close();
        }
    }
}
