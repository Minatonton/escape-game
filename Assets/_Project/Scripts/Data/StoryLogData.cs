using UnityEngine;

namespace EscapeGame.Data
{
    [CreateAssetMenu(menuName = "EscapeGame/StoryLog", fileName = "New StoryLog")]
    public class StoryLogData : ScriptableObject
    {
        public string logId;
        public string title;
        [TextArea] public string bodyText;
    }
}
