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
                    ? "全ての記録を手に入れた。\n\n私の名前も、過去も、ARIAに封じられていた。\nでも——真実だけは、取り戻した。\n\nいつか、自分を取り戻す日が来るだろうか。\n\n─── 被験者 #0047、脱出確認。"
                    : "光の中へ駆け出した。\n\n記憶はない。名前もない。\nでも、生きている。\n\nそれだけで、今は十分だ。";
        }

        public void OnMenuButtonClicked()
        {
            Core.GameManager.Instance?.ReturnToMainMenu();
        }
    }
}
