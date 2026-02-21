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
            _instance.ShowMessage(message);
        }

        public static void Hide()
        {
            _instance?.HidePanel();
        }

        private void ShowMessage(string message)
        {
            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);

            messageText.text = message;
            panel.SetActive(true);
            _hideCoroutine = StartCoroutine(AutoHide());
        }

        private IEnumerator AutoHide()
        {
            yield return new WaitForSeconds(autoHideDuration);
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
