using System.Collections;
using UnityEngine;
using EscapeGame.UI;

namespace EscapeGame.Room
{
    /// <summary>部屋入室時に独白・ナレーション行を順番に表示する</summary>
    public class RoomNarrator : MonoBehaviour
    {
        [SerializeField, TextArea(2, 5)] private string[] lines;
        [SerializeField] private float initialDelay = 0.8f;
        [SerializeField] private float lineInterval  = 9f;

        void Start()
        {
            if (lines == null || lines.Length == 0) return;
            StartCoroutine(PlayLines());
        }

        private IEnumerator PlayLines()
        {
            yield return new WaitForSeconds(initialDelay);
            foreach (var line in lines)
            {
                NarratorPanel.Show(line);
                yield return new WaitForSeconds(lineInterval);
            }
        }
    }
}
