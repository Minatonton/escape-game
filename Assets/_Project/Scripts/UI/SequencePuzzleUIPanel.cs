using UnityEngine;
using UnityEngine.UI;
using EscapeGame.Puzzle;

namespace EscapeGame.UI
{
    /// <summary>順序入力パズルUIパネル</summary>
    public class SequencePuzzleUIPanel : MonoBehaviour
    {
        public static SequencePuzzleUIPanel Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private Text statusText;

        private SequencePuzzle _currentPuzzle;

        void Awake()
        {
            Instance = this;
            panel.SetActive(false);
        }

        public void Open(PuzzleBase puzzle)
        {
            _currentPuzzle = puzzle as SequencePuzzle;
            if (_currentPuzzle == null) return;

            _currentPuzzle.ResetInput();
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
            if (statusText == null) return;
            if (_currentPuzzle == null)
            {
                statusText.text = "";
                return;
            }
            var input = _currentPuzzle.CurrentInput;
            statusText.text = input.Count == 0
                ? "ボタンを正しい順序で押せ"
                : "入力: " + string.Join(" → ", input);
        }

        public void OnButtonClicked(int id)
        {
            if (_currentPuzzle == null) return;
            _currentPuzzle.PressButton(id);
            if (_currentPuzzle.IsSolved)
                Close();
        }

        public void OnCloseButtonClicked()
        {
            Close();
        }
    }
}
