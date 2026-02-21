using UnityEngine;
using UnityEngine.UI;

namespace EscapeGame.UI
{
    /// <summary>エンディング画面UI</summary>
    public class EndingUI : MonoBehaviour
    {
        [SerializeField] private Text endingText;
        [SerializeField] private bool isTrueEnding;

        void Start()
        {
            if (endingText != null)
                endingText.text = isTrueEnding
                    ? "おめでとう。\n真相究明 エンド！"
                    : "脱出成功！\nしかし、謎は残る…";
        }

        public void OnMenuButtonClicked()
        {
            Core.GameManager.Instance?.ReturnToMainMenu();
        }
    }
}
