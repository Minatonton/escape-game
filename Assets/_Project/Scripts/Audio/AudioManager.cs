using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource[] sePool;
        [SerializeField] private float fadeDuration = 1f;

        // SEクリップをIDで参照するための辞書
        [System.Serializable]
        public struct SEEntry
        {
            public string id;
            public AudioClip clip;
        }
        [SerializeField] private SEEntry[] seEntries;
        private Dictionary<string, AudioClip> _seDict;

        private int _seIndex;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _seDict = new Dictionary<string, AudioClip>();
            foreach (var entry in seEntries)
            {
                if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
                    _seDict[entry.id] = entry.clip;
            }
        }

        public void PlayBGM(AudioClip clip, float fade = 1f)
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;
            StartCoroutine(CrossFadeBGM(clip, fade));
        }

        private IEnumerator CrossFadeBGM(AudioClip newClip, float duration)
        {
            float startVolume = bgmSource.volume;

            // フェードアウト
            float elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration * 0.5f));
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.clip = newClip;
            bgmSource.Play();

            // フェードイン
            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, startVolume, elapsed / (duration * 0.5f));
                yield return null;
            }
            bgmSource.volume = startVolume;
        }

        public void PlaySE(string seId)
        {
            if (!_seDict.TryGetValue(seId, out var clip)) return;
            PlaySEClip(clip);
        }

        public void PlaySEClip(AudioClip clip)
        {
            if (sePool == null || sePool.Length == 0) return;
            var source = sePool[_seIndex % sePool.Length];
            _seIndex++;
            source.PlayOneShot(clip);
        }

        public void SetBGMVolume(float volume)
        {
            if (bgmSource != null) bgmSource.volume = Mathf.Clamp01(volume);
        }

        public void SetSEVolume(float volume)
        {
            foreach (var s in sePool)
                if (s != null) s.volume = Mathf.Clamp01(volume);
        }
    }
}
