using UnityEngine;
using UnityEngine.UI;
using EscapeGame.Puzzle;

namespace EscapeGame.UI
{
    /// <summary>色順序パズルUIパネル</summary>
    public class ColorPuzzleUIPanel : MonoBehaviour
    {
        public static ColorPuzzleUIPanel Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private Text statusText;

        private ColorPuzzle _currentPuzzle;

        void Awake()
        {
            Instance = this;
            panel.SetActive(false);
        }

        public void Open(PuzzleBase puzzle)
        {
            _currentPuzzle = puzzle as ColorPuzzle;
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
                ? "色を正しい順序で選べ"
                : "選択: " + string.Join(" → ", input);
        }

        public void OnColorClicked(string color)
        {
            if (_currentPuzzle == null) return;
            _currentPuzzle.SelectColor(color);
            if (_currentPuzzle.IsSolved)
                Close();
        }

        public void OnCloseButtonClicked()
        {
            Close();
        }
    }
}
