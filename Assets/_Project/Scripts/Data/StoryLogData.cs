using UnityEngine;

namespace EscapeGame.Data
{
    [CreateAssetMenu(menuName = "EscapeGame/StoryLog", fileName = "New StoryLog")]
    public class StoryLogData : ScriptableObject
    {
        public string logId;
        public string title;
        public string speakerName;
        [TextArea(4, 12)] public string bodyText;
    }
}
