using UnityEngine;

namespace EscapeGame.UI
{
    /// <summary>メインメニュー画面のUI制御</summary>
    public class MainMenuUI : MonoBehaviour
    {
        public void OnNewGameClicked()
        {
            Core.GameManager.Instance?.StartNewGame();
        }

        public void OnLoadGameClicked()
        {
            // TODO: セーブスロット選択UI
            ExaminePanel.Show("ロード機能は近日実装予定です。");
        }

        public void OnOptionsClicked()
        {
            ExaminePanel.Show("オプション機能は近日実装予定です。");
        }

        public void OnQuitClicked()
        {
            Core.GameManager.Instance?.QuitGame();
        }
    }
}
