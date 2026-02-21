using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EscapeGame.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const int SlotCount = 3;
        private const string AutoSaveId = "autosave";
        private const byte XorKey = 0x5A;

        private SaveData _currentSave;
        private float _playtimeAccum;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _currentSave = new SaveData();
        }

        void Update()
        {
            _playtimeAccum += Time.deltaTime;
        }

        private string GetPath(string slotId)
        {
            var dir = Path.Combine(Application.persistentDataPath, "saves");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"{slotId}.json");
        }

        public void Save(int slot)
        {
            _currentSave.saveId = slot.ToString();
            _currentSave.savedAt = DateTime.Now.ToString("O");
            _currentSave.playtime += _playtimeAccum;
            _playtimeAccum = 0f;
            WriteFile(GetPath(slot.ToString()), _currentSave);
            Debug.Log($"[SaveManager] Saved to slot {slot}");
        }

        public void AutoSave()
        {
            _currentSave.saveId = AutoSaveId;
            _currentSave.savedAt = DateTime.Now.ToString("O");
            _currentSave.playtime += _playtimeAccum;
            _playtimeAccum = 0f;
            WriteFile(GetPath(AutoSaveId), _currentSave);
            Debug.Log("[SaveManager] Auto-saved");
        }

        public SaveData Load(int slot)
        {
            var data = ReadFile(GetPath(slot.ToString()));
            if (data != null) _currentSave = data;
            return data;
        }

        private void WriteFile(string path, SaveData data)
        {
            var json = JsonUtility.ToJson(data, true);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= XorKey;
            File.WriteAllBytes(path, bytes);
        }

        private SaveData ReadFile(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                var bytes = File.ReadAllBytes(path);
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] ^= XorKey;
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load: {e.Message}");
                return null;
            }
        }

        // ゲームフラグ操作
        public void SetFlag(string key, bool value)
        {
            var entry = _currentSave.flags.Find(f => f.key == key);
            if (entry != null)
                entry.value = value;
            else
                _currentSave.flags.Add(new SaveData.FlagEntry { key = key, value = value });
        }

        public bool GetFlag(string key)
        {
            var entry = _currentSave.flags.Find(f => f.key == key);
            return entry != null && entry.value;
        }

        public void SetPuzzleSolved(string puzzleId)
        {
            if (!_currentSave.solvedPuzzleIds.Contains(puzzleId))
                _currentSave.solvedPuzzleIds.Add(puzzleId);
        }

        public bool IsPuzzleSolved(string puzzleId)
        {
            return _currentSave.solvedPuzzleIds.Contains(puzzleId);
        }
    }
}
