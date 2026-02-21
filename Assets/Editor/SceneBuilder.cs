// Assets/Editor/SceneBuilder.cs
// Unity の -executeMethod EscapeGame.Editor.SceneBuilder.BuildAll で呼び出す
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EscapeGame.Audio;
using EscapeGame.Core;
using EscapeGame.Data;
using EscapeGame.Inventory;
using EscapeGame.Puzzle;
using EscapeGame.Room;
using EscapeGame.Save;
using EscapeGame.UI;

namespace EscapeGame.Editor
{
    public static class SceneBuilder
    {
        // ---- パス定数 ----
        private const string SceneDir = "Assets/_Project/Scenes";
        private const string DataDir   = "Assets/_Project/Data";
        private const string PrefabDir = "Assets/_Project/Prefabs";

        // ---- メインエントリーポイント ----
        public static void BuildAll()
        {
            Debug.Log("[SceneBuilder] Start BuildAll");
            EnsureDirectories();
            CreatePuzzleData();
            CreateMainMenuScene();
            CreateR001Scene();
            AddScenesToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SceneBuilder] BuildAll complete");
        }

        // ---- ディレクトリ整備 ----
        private static void EnsureDirectories()
        {
            var dirs = new[]
            {
                SceneDir + "/Chapter1",
                DataDir + "/Puzzles",
                DataDir + "/Rooms",
                DataDir + "/Items",
                PrefabDir + "/UI",
            };
            foreach (var d in dirs)
            {
                if (!AssetDatabase.IsValidFolder(d))
                {
                    var parts = d.Split('/');
                    var cur = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var next = cur + "/" + parts[i];
                        if (!AssetDatabase.IsValidFolder(next))
                            AssetDatabase.CreateFolder(cur, parts[i]);
                        cur = next;
                    }
                }
            }
        }

        // ---- ScriptableObject アセット生成 ----
        private static PuzzleData _puzzleData;

        private static void CreatePuzzleData()
        {
            var path = DataDir + "/Puzzles/puzzle_r001_code.asset";
            _puzzleData = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            if (_puzzleData == null)
            {
                _puzzleData = ScriptableObject.CreateInstance<PuzzleData>();
                _puzzleData.puzzleId = "r001_code";
                _puzzleData.type = PuzzleType.NumericCode;
                _puzzleData.answers = new[] { "1234" };
                _puzzleData.hints = new[]
                {
                    new HintData { level = 1, hintText = "何か数字のヒントが部屋のどこかにあるはずだ。" },
                    new HintData { level = 2, hintText = "本棚のメモをよく見てみよう。" },
                    new HintData { level = 3, hintText = "「1234」を入力してみよう。" },
                };
                AssetDatabase.CreateAsset(_puzzleData, path);
            }
        }

        // ---- ItemSlot Prefab ----
        private static GameObject CreateItemSlotPrefab()
        {
            var prefabPath = PrefabDir + "/UI/ItemSlot.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return existing;

            var root = new GameObject("ItemSlot");
            var img = root.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f);
            var btn = root.AddComponent<Button>();
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            // 名前テキスト
            var textGo = new GameObject("Label");
            textGo.transform.SetParent(root.transform, false);
            var txt = textGo.AddComponent<Text>();
            txt.text = "Item";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 11;
            txt.color = Color.white;
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
            trt.anchoredPosition = Vector2.zero;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // ================================================================
        //  MainMenu シーン
        // ================================================================
        private static void CreateMainMenuScene()
        {
            var scenePath = SceneDir + "/MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- カメラ ----
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            camGo.AddComponent<AudioListener>();

            // ---- EventSystem ----
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            // ---- Managers ----
            BuildManagers();

            // ---- UI Canvas ----
            var canvas = BuildCanvas("MainMenuCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;

            // 背景
            CreatePanel(canvas.transform, "Background",
                Vector2.zero, new Vector2(1920, 1080),
                new Color(0.06f, 0.07f, 0.12f));

            // タイトル
            CreateText(canvas.transform, "TitleText", "ESCAPE",
                new Vector2(0, 300), new Vector2(600, 120),
                Color.white, 72, TextAnchor.MiddleCenter, bold: true);

            CreateText(canvas.transform, "SubTitleText", "〜 脱出ゲーム 〜",
                new Vector2(0, 220), new Vector2(400, 60),
                new Color(0.8f, 0.8f, 0.8f), 28, TextAnchor.MiddleCenter);

            // メニューボタン
            float btnY = 80f;
            float btnStep = 70f;

            var mainMenu = canvas.gameObject.AddComponent<MainMenuUI>();

            var btnNewGame = CreateButton(canvas.transform, "NewGameButton", "New Game",
                new Vector2(0, btnY), new Vector2(280, 55),
                new Color(0.2f, 0.6f, 0.9f));
            btnNewGame.onClick.AddListener(mainMenu.OnNewGameClicked);

            var btnLoad = CreateButton(canvas.transform, "LoadButton", "Load Game",
                new Vector2(0, btnY - btnStep), new Vector2(280, 55),
                new Color(0.3f, 0.3f, 0.4f));
            btnLoad.onClick.AddListener(mainMenu.OnLoadGameClicked);

            var btnOptions = CreateButton(canvas.transform, "OptionsButton", "Options",
                new Vector2(0, btnY - btnStep * 2), new Vector2(280, 55),
                new Color(0.3f, 0.3f, 0.4f));
            btnOptions.onClick.AddListener(mainMenu.OnOptionsClicked);

            var btnQuit = CreateButton(canvas.transform, "QuitButton", "Quit",
                new Vector2(0, btnY - btnStep * 3), new Vector2(280, 55),
                new Color(0.5f, 0.2f, 0.2f));
            btnQuit.onClick.AddListener(mainMenu.OnQuitClicked);

            // ExaminePanel（通知用）
            BuildExaminePanel(canvas.transform);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneBuilder] Saved {scenePath}");
        }

        // ================================================================
        //  R001_WakeRoom シーン
        // ================================================================
        private static void CreateR001Scene()
        {
            var scenePath = SceneDir + "/Chapter1/R001_WakeRoom.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- カメラ ----
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            camGo.AddComponent<AudioListener>();

            // ---- EventSystem ----
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            // ---- Managers（スタンドアロン起動対応） ----
            BuildManagers();

            // ---- RoomManager ----
            var rmGo = new GameObject("RoomManager");
            rmGo.AddComponent<RoomManager>();

            // ---- 背景 ----
            BuildBackground();

            // ---- Room Objects ----
            var doorObj = BuildDoorObject();
            var keyBoxPuzzle = BuildKeyBoxObject(doorObj);
            BuildShelfObject();

            // ---- UI Canvas ----
            var canvas = BuildCanvas("HUDCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;

            // 下部HUDバー
            BuildHUDBar(canvas.transform);

            // ExaminePanel
            var examinePanel = BuildExaminePanel(canvas.transform);

            // PuzzleUIPanel
            BuildPuzzleUIPanel(canvas.transform);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneBuilder] Saved {scenePath}");
        }

        // ----------------------------------------------------------------
        //  背景クワッド
        // ----------------------------------------------------------------
        private static void BuildBackground()
        {
            // 壁（後ろ）
            var wall = CreateQuad("Background_Wall", new Vector3(0, 0, 1f),
                new Vector3(20, 12, 1), new Color(0.18f, 0.20f, 0.25f));

            // 床
            var floor = CreateQuad("Background_Floor", new Vector3(0, -3.5f, 0.5f),
                new Vector3(20, 5, 1), new Color(0.12f, 0.13f, 0.16f));

            // 部屋ラベル
            var canvas = BuildCanvas("RoomLabelCanvas", RenderMode.WorldSpace, 0);
            canvas.GetComponent<RectTransform>().localScale = Vector3.one * 0.01f;
            canvas.transform.position = new Vector3(-7f, 4.5f, 0);
            CreateText(canvas.transform, "RoomLabel", "R001: 目覚めの部屋",
                Vector2.zero, new Vector2(400, 60),
                new Color(0.5f, 0.7f, 1f), 24, TextAnchor.UpperLeft);
        }

        // ----------------------------------------------------------------
        //  本棚
        // ----------------------------------------------------------------
        private static void BuildShelfObject()
        {
            var go = CreateQuad("Shelf", new Vector3(-5f, 0f, 0),
                new Vector3(2.5f, 3.5f, 1), new Color(0.35f, 0.22f, 0.10f));

            var col = go.AddComponent<BoxCollider2D>();

            var examinable = go.AddComponent<ExaminableObject>();
            var so = new SerializedObject(examinable);
            so.FindProperty("objectId").stringValue = "shelf_01";
            so.FindProperty("examineText").stringValue =
                "古びた本棚。\n本がぎっしりと詰まっている。\nふと、一冊の本に挟まったメモが目に入った。\n\n　『 1 2 3 4 』\n\n何かの暗号だろうか……";
            so.ApplyModifiedPropertiesWithoutUndo();

            // ラベル
            AddWorldLabel(go, "本棚", new Vector3(0, 2f, 0));
        }

        // ----------------------------------------------------------------
        //  暗号ロックボックス
        // ----------------------------------------------------------------
        private static NumericCodePuzzle BuildKeyBoxObject(DoorObject door)
        {
            var go = CreateQuad("KeyBox", new Vector3(0f, -1f, 0),
                new Vector3(1.5f, 1.5f, 1), new Color(0.25f, 0.27f, 0.30f));

            go.AddComponent<BoxCollider2D>();

            var puzzle = go.AddComponent<NumericCodePuzzle>();
            var pso = new SerializedObject(puzzle);
            pso.FindProperty("puzzleId").stringValue = "r001_code";
            pso.FindProperty("digitCount").intValue = 4;
            pso.FindProperty("puzzleData").objectReferenceValue = _puzzleData;

            pso.ApplyModifiedPropertiesWithoutUndo();

            // onSolved → DoorObject.Unlock() を UnityEventTools で設定
            if (door != null)
            {
                var fieldInfo = typeof(PuzzleBase).GetField(
                    "onSolved", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    var evt = (UnityEngine.Events.UnityEvent)fieldInfo.GetValue(puzzle);
                    UnityEventTools.AddVoidPersistentListener(evt, door.Unlock);
                }
                else
                {
                    Debug.LogWarning("[SceneBuilder] Could not find 'onSolved' field via reflection.");
                }
            }

            var container = go.AddComponent<PuzzleContainer>();
            var cso = new SerializedObject(container);
            cso.FindProperty("objectId").stringValue = "keybox_01";
            cso.FindProperty("examineText").stringValue =
                "4桁の暗号ロック。\n番号を入力すれば開くはずだ。";
            cso.FindProperty("puzzle").objectReferenceValue = puzzle;
            cso.ApplyModifiedPropertiesWithoutUndo();

            AddWorldLabel(go, "暗号ロック", new Vector3(0, 1f, 0));
            return puzzle;
        }

        // ----------------------------------------------------------------
        //  ドア
        // ----------------------------------------------------------------
        private static DoorObject BuildDoorObject()
        {
            var go = CreateQuad("Door", new Vector3(5f, 0.5f, 0),
                new Vector3(1.8f, 4f, 1), new Color(0.20f, 0.35f, 0.50f));

            go.AddComponent<BoxCollider2D>();

            var door = go.AddComponent<DoorObject>();
            var dso = new SerializedObject(door);
            dso.FindProperty("objectId").stringValue = "door_01";
            dso.FindProperty("targetSceneName").stringValue = ""; // 次の部屋は未実装
            dso.FindProperty("isLocked").boolValue = true;
            dso.FindProperty("lockedMessage").stringValue =
                "ドアはロックされている。\n暗号を解除しないと開かない。";
            dso.FindProperty("openMessage").stringValue =
                "カチャリ——！\nドアが開いた。脱出成功！";
            dso.ApplyModifiedPropertiesWithoutUndo();

            AddWorldLabel(go, "出口", new Vector3(0, 2.2f, 0));
            return door;
        }

        // ----------------------------------------------------------------
        //  HUD バー
        // ----------------------------------------------------------------
        private static void BuildHUDBar(Transform parent)
        {
            // 下部バー背景
            var barBg = CreatePanel(parent, "HUDBar",
                new Vector2(0, -500), new Vector2(1920, 90),
                new Color(0.05f, 0.05f, 0.08f, 0.95f));
            var barRt = barBg.GetComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0, 0);
            barRt.anchorMax = new Vector2(1, 0);
            barRt.pivot = new Vector2(0.5f, 0);
            barRt.anchoredPosition = new Vector2(0, 0);
            barRt.sizeDelta = new Vector2(0, 90);

            // インベントリバーコンテナ（RectTransform 付きで作成してから HLG 追加）
            var invBarGo = new GameObject("InventoryBar", typeof(RectTransform));
            invBarGo.transform.SetParent(barBg.transform, false);
            var invRt = (RectTransform)invBarGo.transform;
            var hlg = invBarGo.AddComponent<HorizontalLayoutGroup>();
            invRt.anchorMin = new Vector2(0, 0);
            invRt.anchorMax = new Vector2(1, 1);
            invRt.offsetMin = new Vector2(10, 5);
            invRt.offsetMax = new Vector2(-200, -5);
            hlg.spacing = 5;
            hlg.childForceExpandHeight = true;
            hlg.childControlHeight = false;
            hlg.childControlWidth = false;

            // メニューボタン
            var menuBtn = CreateButton(barBg.transform, "MenuButton", "Menu",
                new Vector2(-80, 0), new Vector2(80, 40),
                new Color(0.3f, 0.3f, 0.4f));
            var menuBtnRt = menuBtn.gameObject.GetComponent<RectTransform>();
            menuBtnRt.anchorMin = new Vector2(1, 0.5f);
            menuBtnRt.anchorMax = new Vector2(1, 0.5f);
            menuBtnRt.pivot = new Vector2(1, 0.5f);
            menuBtnRt.anchoredPosition = new Vector2(-10, 0);

            // ItemSlot Prefab
            var slotPrefab = CreateItemSlotPrefab();

            // HUDController
            var hudGo = new GameObject("HUDController");
            hudGo.transform.SetParent(barBg.transform, false);
            var hud = hudGo.AddComponent<HUDController>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("inventoryBar").objectReferenceValue =
                invBarGo.GetComponent<RectTransform>();
            hudSo.FindProperty("itemSlotPrefab").objectReferenceValue = slotPrefab;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            menuBtn.onClick.AddListener(hud.OnMenuButtonClicked);
        }

        // ----------------------------------------------------------------
        //  ExaminePanel
        // ----------------------------------------------------------------
        private static ExaminePanel BuildExaminePanel(Transform parent)
        {
            var panelBg = CreatePanel(parent, "ExaminePanel",
                new Vector2(0, -150), new Vector2(900, 120),
                new Color(0.05f, 0.05f, 0.08f, 0.95f));

            var panelRt = panelBg.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0);
            panelRt.anchorMax = new Vector2(0.5f, 0);
            panelRt.pivot = new Vector2(0.5f, 0);
            panelRt.anchoredPosition = new Vector2(0, 100);

            // テキスト
            var txtGo = new GameObject("MessageText");
            txtGo.transform.SetParent(panelBg.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text = "";
            txt.fontSize = 20;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(20, 10);
            trt.offsetMax = new Vector2(-20, -10);

            panelBg.SetActive(false);

            var ep = panelBg.transform.parent.gameObject.AddComponent<ExaminePanel>();
            var so = new SerializedObject(ep);
            so.FindProperty("panel").objectReferenceValue = panelBg;
            so.FindProperty("messageText").objectReferenceValue = txt;
            so.ApplyModifiedPropertiesWithoutUndo();

            return ep;
        }

        // ----------------------------------------------------------------
        //  PuzzleUIPanel（数字キーパッド）
        // ----------------------------------------------------------------
        private static void BuildPuzzleUIPanel(Transform parent)
        {
            // モーダル背景
            var overlay = CreatePanel(parent, "PuzzleOverlay",
                Vector2.zero, Vector2.zero,
                new Color(0, 0, 0, 0.7f));
            var ort = overlay.GetComponent<RectTransform>();
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.sizeDelta = Vector2.zero;

            // パネル本体
            var panelBg = CreatePanel(overlay.transform, "PuzzlePanel",
                Vector2.zero, new Vector2(350, 450),
                new Color(0.1f, 0.12f, 0.15f));

            // タイトル
            CreateText(panelBg.transform, "TitleText", "暗号ロック",
                new Vector2(0, 180), new Vector2(320, 50),
                Color.white, 24, TextAnchor.MiddleCenter);

            // 表示テキスト
            var dispGo = new GameObject("DisplayText");
            dispGo.transform.SetParent(panelBg.transform, false);
            var dispTxt = dispGo.AddComponent<Text>();
            dispTxt.text = "____";
            dispTxt.fontSize = 40;
            dispTxt.color = new Color(0.3f, 0.9f, 0.4f);
            dispTxt.alignment = TextAnchor.MiddleCenter;
            dispTxt.fontStyle = FontStyle.Bold;
            var drt = dispGo.GetComponent<RectTransform>();
            drt.anchoredPosition = new Vector2(0, 120);
            drt.sizeDelta = new Vector2(300, 60);

            // キーパッドグリッド（GridLayoutGroup を先に追加して RectTransform を確保）
            var keypadGo = new GameObject("Keypad");
            keypadGo.transform.SetParent(panelBg.transform, false);
            var grid = keypadGo.AddComponent<GridLayoutGroup>(); // これで RectTransform が付く
            var krt = (RectTransform)keypadGo.transform;
            krt.anchoredPosition = new Vector2(0, -20);
            krt.sizeDelta = new Vector2(300, 260);
            grid.cellSize = new Vector2(88, 60);
            grid.spacing = new Vector2(8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            // PuzzleUIPanel コンポーネント
            overlay.SetActive(false);

            var puiGo = new GameObject("PuzzleUIPanel");
            puiGo.transform.SetParent(parent, false);
            var pui = puiGo.AddComponent<PuzzleUIPanel>();
            var pso = new SerializedObject(pui);
            pso.FindProperty("panel").objectReferenceValue = overlay;
            pso.FindProperty("displayText").objectReferenceValue = dispTxt;
            pso.ApplyModifiedPropertiesWithoutUndo();

            // 数字ボタン 1-9, 0, Del, Close
            int[] layout = { 1, 2, 3, 4, 5, 6, 7, 8, 9, -2, 0, -1 };
            // -1: Del, -2: CLR
            foreach (int d in layout)
            {
                string label;
                if (d == -1) label = "DEL";
                else if (d == -2) label = "CLR";
                else label = d.ToString();

                var btn = CreateButton(keypadGo.transform, $"Btn_{label}", label,
                    Vector2.zero, new Vector2(88, 60),
                    d >= 0 ? new Color(0.25f, 0.28f, 0.32f) : new Color(0.4f, 0.2f, 0.2f));

                var digit = d;
                if (digit >= 0)
                    btn.onClick.AddListener(() => pui.OnDigitButtonClicked(digit));
                else if (digit == -1)
                    btn.onClick.AddListener(pui.OnDeleteButtonClicked);
                else
                    btn.onClick.AddListener(pui.OnClearButtonClicked);
            }

            // 閉じるボタン
            var closeBtn = CreateButton(panelBg.transform, "CloseButton", "✕",
                new Vector2(0, -190), new Vector2(120, 45),
                new Color(0.45f, 0.2f, 0.2f));
            closeBtn.onClick.AddListener(pui.OnCloseButtonClicked);
        }

        // ================================================================
        //  Build Settings に追加
        // ================================================================
        private static void AddScenesToBuildSettings()
        {
            var scenes = new[]
            {
                SceneDir + "/MainMenu.unity",
                SceneDir + "/Chapter1/R001_WakeRoom.unity",
            };

            var builds = new EditorBuildSettingsScene[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                builds[i] = new EditorBuildSettingsScene(scenes[i], true);

            EditorBuildSettings.scenes = builds;
            Debug.Log("[SceneBuilder] Build Settings updated");
        }

        // ================================================================
        //  共通マネージャー生成
        // ================================================================
        private static void BuildManagers()
        {
            CreateSingleton<GameManager>("GameManager");
            CreateSingleton<InventoryManager>("InventoryManager");
            CreateSingleton<SaveManager>("SaveManager");
            BuildAudioManager();
            CreateSingleton<SceneLoader>("SceneLoader");
        }

        private static T CreateSingleton<T>(string name) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            return go.AddComponent<T>();
        }

        private static void BuildAudioManager()
        {
            var go = new GameObject("AudioManager");
            var am = go.AddComponent<AudioManager>();

            // BGM AudioSource
            var bgm = go.AddComponent<AudioSource>();
            bgm.loop = true;
            bgm.volume = 0.5f;
            bgm.playOnAwake = false;

            // SE AudioSource × 8
            var sePool = new AudioSource[8];
            for (int i = 0; i < sePool.Length; i++)
            {
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.volume = 0.8f;
                sePool[i] = s;
            }

            var so = new SerializedObject(am);
            so.FindProperty("bgmSource").objectReferenceValue = bgm;
            var poolProp = so.FindProperty("sePool");
            poolProp.arraySize = sePool.Length;
            for (int i = 0; i < sePool.Length; i++)
                poolProp.GetArrayElementAtIndex(i).objectReferenceValue = sePool[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ================================================================
        //  ヘルパー
        // ================================================================
        private static Canvas BuildCanvas(string name, RenderMode mode, int sortOrder)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = mode;
            if (mode == RenderMode.ScreenSpaceOverlay)
                canvas.sortingOrder = sortOrder;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return go;
        }

        private static Text CreateText(Transform parent, string name, string content,
            Vector2 anchoredPos, Vector2 size, Color color,
            int fontSize, TextAnchor anchor, bool bold = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = anchor;
            if (bold) txt.fontStyle = FontStyle.Bold;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return txt;
        }

        private static Button CreateButton(Transform parent, string name, string label,
            Vector2 anchoredPos, Vector2 size, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text = label;
            txt.fontSize = 20;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontStyle = FontStyle.Bold;
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
            trt.anchoredPosition = Vector2.zero;

            return btn;
        }

        private static GameObject CreateQuad(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            Object.DestroyImmediate(go.GetComponent<MeshCollider>());

            var mr = go.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            mr.sharedMaterial = mat;

            return go;
        }

        private static void AddWorldLabel(GameObject parent, string text, Vector3 localOffset)
        {
            var canvas = new GameObject("LabelCanvas");
            canvas.transform.SetParent(parent.transform, false);
            canvas.transform.localPosition = localOffset;
            canvas.transform.localScale = Vector3.one * 0.02f;
            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(canvas.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = 24;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            var rt = txtGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 60);
        }
    }
}
