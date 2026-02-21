using System;
using System.Collections.Generic;

namespace EscapeGame.Save
{
    [Serializable]
    public class SaveData
    {
        public string saveId;
        public string savedAt;          // DateTime.ToString("O")
        public string currentRoomId;
        public string currentChapter;
        public List<string> collectedItemIds = new List<string>();
        public List<string> solvedPuzzleIds = new List<string>();
        public List<string> viewedLogIds = new List<string>();
        public List<FlagEntry> flags = new List<FlagEntry>();
        public float playtime;
        public int hintUsedCount;

        [Serializable]
        public class FlagEntry
        {
            public string key;
            public bool value;
        }
    }
}
