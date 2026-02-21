using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EscapeGame.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private float fadeDuration = 0.5f;

        private Image _fadeImage;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // フェード用の全画面Imageを作成
            var canvas = new GameObject("FadeCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            DontDestroyOnLoad(canvas.gameObject);

            var imgGo = new GameObject("FadeImage");
            imgGo.transform.SetParent(canvas.transform, false);
            _fadeImage = imgGo.AddComponent<Image>();
            _fadeImage.color = new Color(0, 0, 0, 0);
            var rt = imgGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadWithFade(sceneName));
        }

        private IEnumerator LoadWithFade(string sceneName)
        {
            // フェードアウト
            yield return StartCoroutine(Fade(0f, 1f));

            var op = SceneManager.LoadSceneAsync(sceneName);
            yield return op;

            // フェードイン
            yield return StartCoroutine(Fade(1f, 0f));
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(from, to, elapsed / fadeDuration);
                _fadeImage.color = new Color(0, 0, 0, a);
                yield return null;
            }
            _fadeImage.color = new Color(0, 0, 0, to);
        }
    }
}
