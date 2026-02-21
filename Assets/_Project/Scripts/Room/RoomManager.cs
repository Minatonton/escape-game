using UnityEngine;
using EscapeGame.Audio;
using EscapeGame.Core;
using EscapeGame.Data;

namespace EscapeGame.Room
{
    /// <summary>現在の部屋を管理するクラス（各部屋シーンに配置）</summary>
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [SerializeField] private RoomData roomData;

        public RoomData CurrentRoom => roomData;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ChangeState(GameState.Playing);

            if (roomData != null && roomData.bgmClip != null)
                AudioManager.Instance?.PlayBGM(roomData.bgmClip);
        }
    }
}
