using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeGame.UI
{
    /// <summary>調査テキスト・メッセージを表示するパネル</summary>
    public class ExaminePanel : MonoBehaviour
    {
        private static ExaminePanel _instance;

        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private float autoHideDuration = 4f;

        private Coroutine _hideCoroutine;

        void Awake()
        {
            _instance = this;
            panel.SetActive(false);
        }

        public static void Show(string message)
        {
            if (_instance == null) return;
            _instance.ShowMessage(null, message, _instance.autoHideDuration);
        }

        public static void Show(string message, float duration)
        {
            if (_instance == null) return;
            _instance.ShowMessage(null, message, duration);
        }

        /// <summary>タイトル付きで表示（ストーリーログ・長文用、自動で表示時間を伸ばす）</summary>
        public static void ShowWithTitle(string title, string body)
        {
            if (_instance == null) return;
            _instance.ShowMessage(title, body, Mathf.Max(_instance.autoHideDuration, 10f));
        }

        public static void Hide()
        {
            _instance?.HidePanel();
        }

        private void ShowMessage(string title, string body, float duration)
        {
            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);

            if (titleText != null)
            {
                titleText.text = title ?? "";
                titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
            }
            if (messageText != null) messageText.text = body;
            panel.SetActive(true);
            _hideCoroutine = StartCoroutine(AutoHide(duration));
        }

        private IEnumerator AutoHide(float duration)
        {
            yield return new WaitForSeconds(duration);
            HidePanel();
        }

        private void HidePanel()
        {
            panel.SetActive(false);
        }

        public void OnCloseButtonClicked()
        {
            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);
            HidePanel();
        }
    }
}
