using UnityEngine;
using EscapeGame.Data;
using EscapeGame.Save;
using EscapeGame.UI;

namespace EscapeGame.Room
{
    /// <summary>ストーリーログを表示し、収集フラグを立てるオブジェクト</summary>
    public class StoryLogObject : ExaminableObject
    {
        [SerializeField] private StoryLogData logData;

        protected override void OnInteract()
        {
            if (logData == null) return;

            string title = string.IsNullOrEmpty(logData.speakerName)
                ? $"【{logData.title}】"
                : $"【{logData.title}】  —  {logData.speakerName}";

            ExaminePanel.ShowWithTitle(title, logData.bodyText);
            SaveManager.Instance?.SetFlag($"log_{logData.logId}", true);
        }
    }
}
