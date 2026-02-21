using UnityEngine;

namespace EscapeGame.Data
{
    [CreateAssetMenu(menuName = "EscapeGame/Room", fileName = "New Room")]
    public class RoomData : ScriptableObject
    {
        public string roomId;
        public string roomName;
        public string sceneAssetPath;
        public AudioClip bgmClip;
        public string[] requiredPuzzleIds;
        public string nextRoomId;
    }
}
