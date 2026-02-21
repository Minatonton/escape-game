using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Audio;
using EscapeGame.Save;
using EscapeGame.UI;

namespace EscapeGame.Puzzle
{
    /// <summary>全パズルの基底クラス</summary>
    public abstract class PuzzleBase : MonoBehaviour
    {
        [SerializeField] protected string puzzleId;
        [SerializeField] private UnityEvent onSolved;

        public bool IsSolved { get; private set; }

        protected abstract bool ValidateSolution();

        protected void TrySolve()
        {
            if (IsSolved) return;

            if (ValidateSolution())
            {
                IsSolved = true;
                SaveManager.Instance?.SetPuzzleSolved(puzzleId);
                onSolved?.Invoke();
                OnCorrectAnswer();
            }
            else
            {
                OnWrongAnswer();
            }
        }

        protected virtual void OnCorrectAnswer()
        {
            AudioManager.Instance?.PlaySE("correct");
        }

        protected virtual void OnWrongAnswer()
        {
            AudioManager.Instance?.PlaySE("wrong");
            ExaminePanel.Show("違う……もう一度考えよう。");
        }
    }
}
