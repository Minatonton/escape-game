using UnityEngine;

namespace EscapeGame.Data
{
    public enum PuzzleType
    {
        NumericCode,
        Sequence,
        Color,
        Symbol
    }

    [System.Serializable]
    public class HintData
    {
        public int level;
        [TextArea] public string hintText;
    }

    [CreateAssetMenu(menuName = "EscapeGame/Puzzle", fileName = "New Puzzle")]
    public class PuzzleData : ScriptableObject
    {
        public string puzzleId;
        public PuzzleType type;
        public string[] answers;
        public HintData[] hints;
    }
}
