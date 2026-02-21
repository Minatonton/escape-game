using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeGame.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Cutscene,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        public event Action<GameState> OnStateChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void StartNewGame()
        {
            ChangeState(GameState.Playing);
            SceneManager.LoadScene("R001_WakeRoom");
        }

        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
