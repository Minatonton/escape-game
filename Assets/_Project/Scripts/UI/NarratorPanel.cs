using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeGame.UI
{
    /// <summary>部屋入室時の独白・ナレーションを画面上部に自動フェード表示するパネル</summary>
    public class NarratorPanel : MonoBehaviour
    {
        public static NarratorPanel Instance { get; private set; }
        public static float DisplayDuration => Instance != null ? Instance._displayDuration : 8f;

        [SerializeField] private GameObject panel;
        [SerializeField] private Text narratorText;
        [SerializeField] private float _displayDuration = 8f;

        private Coroutine _hideCoroutine;

        void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
        }

        public static void Show(string message)
        {
            Instance?.ShowMessage(message);
        }

        private void ShowMessage(string message)
        {
            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
            if (narratorText != null) narratorText.text = message;
            if (panel != null) panel.SetActive(true);
            _hideCoroutine = StartCoroutine(AutoHide());
        }

        private IEnumerator AutoHide()
        {
            yield return new WaitForSeconds(_displayDuration);
            if (panel != null) panel.SetActive(false);
        }
    }
}
