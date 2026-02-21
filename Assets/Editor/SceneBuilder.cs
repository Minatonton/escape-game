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

        // ---- アセット参照（BuildAll実行中に保持） ----
        private static PuzzleData   _puzzleData;
        private static ItemData     _itemRustyKey, _itemOil, _itemWorkingKey;
        private static ItemData     _itemIdCard, _itemScrewdriver, _itemWire, _itemFinalKey;
        private static StoryLogData _log001, _log002, _log003;

        // ================================================================
        //  メインエントリーポイント
        // ================================================================
        public static void BuildAll()
        {
            Debug.Log("[SceneBuilder] Start BuildAll");
            EnsureDirectories();
            CreateAllItems();
            CreatePuzzleData();
            CreateAllStoryLogs();
            CreateMainMenuScene();
            CreateR001Scene();
            CreateR002Scene();
            CreateR003Scene();
            CreateEndingScene(true);
            CreateEndingScene(false);
            AddScenesToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SceneBuilder] BuildAll complete");
        }

        // ================================================================
        //  ディレクトリ整備
        // ================================================================
        private static void EnsureDirectories()
        {
            var dirs = new[]
            {
                SceneDir + "/Chapter1",
                DataDir + "/Puzzles",
                DataDir + "/Rooms",
                DataDir + "/Items",
                DataDir + "/Logs",
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

        // ================================================================
        //  アイテムアセット生成（7種）
        // ================================================================
        private static void CreateAllItems()
        {
            _itemRustyKey    = CreateOrLoadItem("item_rusty_key",    "錆びた鍵",   ItemCategory.Key);
            _itemOil         = CreateOrLoadItem("item_oil",          "潤滑油",     ItemCategory.Material);
            _itemIdCard      = CreateOrLoadItem("item_id_card",      "IDカード",   ItemCategory.Information);
            _itemScrewdriver = CreateOrLoadItem("item_screwdriver",  "ドライバー", ItemCategory.Tool);
            _itemWire        = CreateOrLoadItem("item_wire",         "ワイヤー",   ItemCategory.Material);
            _itemFinalKey    = CreateOrLoadItem("item_final_key",    "最終の鍵",   ItemCategory.Key);

            // working_key は合成で生成されるアイテム
            _itemWorkingKey = CreateOrLoadItem("item_working_key",  "使える鍵",   ItemCategory.Key);
            var wkSo = new SerializedObject(_itemWorkingKey);
            wkSo.FindProperty("combineIngredientA").objectReferenceValue = _itemRustyKey;
            wkSo.FindProperty("combineIngredientB").objectReferenceValue = _itemOil;
            wkSo.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        private static ItemData CreateOrLoadItem(string id, string displayName, ItemCategory category)
        {
            var path = DataDir + "/Items/" + id + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (existing != null) return existing;

            var item = ScriptableObject.CreateInstance<ItemData>();
            item.itemId      = id;
            item.displayName = displayName;
            item.category    = category;
            AssetDatabase.CreateAsset(item, path);
            return item;
        }

        // ================================================================
        //  パズルデータアセット生成
        // ================================================================
        private static void CreatePuzzleData()
        {
            var path = DataDir + "/Puzzles/puzzle_r001_code.asset";
            _puzzleData = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            if (_puzzleData == null)
            {
                _puzzleData = ScriptableObject.CreateInstance<PuzzleData>();
                _puzzleData.puzzleId = "r001_code";
                _puzzleData.type     = PuzzleType.NumericCode;
                _puzzleData.answers  = new[] { "1234" };
                _puzzleData.hints    = new[]
                {
                    new HintData { level = 1, hintText = "何か数字のヒントが部屋のどこかにあるはずだ。" },
                    new HintData { level = 2, hintText = "本棚のメモをよく見てみよう。" },
                    new HintData { level = 3, hintText = "「1234」を入力してみよう。" },
                };
                AssetDatabase.CreateAsset(_puzzleData, path);
            }
        }

        // ================================================================
        //  ストーリーログアセット生成（3件）
        // ================================================================
        private static void CreateAllStoryLogs()
        {
            _log001 = CreateOrLoadLog("storylog_001", "001", "研究日誌 #1",
                "この施設で行われていた実験は……\n記録によれば、被験者の記憶を操作する技術が開発されていたという。");
            _log002 = CreateOrLoadLog("storylog_002", "002", "博士の手紙",
                "君が目覚めたなら、信じてほしい……\n私はただ、世界を救いたかっただけなのだ。");
            _log003 = CreateOrLoadLog("storylog_003", "003", "最終記録",
                "AIが制御を奪いはじめた。もう時間がない……\n扉の向こうに答えがある。急いでくれ。");
        }

        private static StoryLogData CreateOrLoadLog(string fileName, string logId, string title, string bodyText)
        {
            var path = DataDir + "/Logs/" + fileName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<StoryLogData>(path);
            if (existing != null) return existing;

            var log = ScriptableObject.CreateInstance<StoryLogData>();
            log.logId    = logId;
            log.title    = title;
            log.bodyText = bodyText;
            AssetDatabase.CreateAsset(log, path);
            return log;
        }

        // ================================================================
        //  ItemSlot Prefab
        // ================================================================
        private static GameObject CreateItemSlotPrefab()
        {
            var prefabPath = PrefabDir + "/UI/ItemSlot.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return existing;

            var root = new GameObject("ItemSlot");
            var img = root.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f);
            root.AddComponent<Button>();
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(root.transform, false);
            var txt = textGo.AddComponent<Text>();
            txt.text      = "Item";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize  = 11;
            txt.color     = Color.white;
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin       = Vector2.zero;
            trt.anchorMax       = Vector2.one;
            trt.sizeDelta       = Vector2.zero;
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

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.05f, 0.05f, 0.1f);
            camGo.AddComponent<AudioListener>();

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            BuildManagers();

            var canvas = BuildCanvas("MainMenuCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;

            CreatePanel(canvas.transform, "Background",
                Vector2.zero, new Vector2(1920, 1080), new Color(0.06f, 0.07f, 0.12f));

            CreateText(canvas.transform, "TitleText", "ESCAPE",
                new Vector2(0, 300), new Vector2(600, 120),
                Color.white, 72, TextAnchor.MiddleCenter, bold: true);

            CreateText(canvas.transform, "SubTitleText", "〜 脱出ゲーム 〜",
                new Vector2(0, 220), new Vector2(400, 60),
                new Color(0.8f, 0.8f, 0.8f), 28, TextAnchor.MiddleCenter);

            float btnY = 80f, btnStep = 70f;
            var mainMenu = canvas.gameObject.AddComponent<MainMenuUI>();

            var btnNewGame = CreateButton(canvas.transform, "NewGameButton", "New Game",
                new Vector2(0, btnY), new Vector2(280, 55), new Color(0.2f, 0.6f, 0.9f));
            btnNewGame.onClick.AddListener(mainMenu.OnNewGameClicked);

            var btnLoad = CreateButton(canvas.transform, "LoadButton", "Load Game",
                new Vector2(0, btnY - btnStep), new Vector2(280, 55), new Color(0.3f, 0.3f, 0.4f));
            btnLoad.onClick.AddListener(mainMenu.OnLoadGameClicked);

            var btnOptions = CreateButton(canvas.transform, "OptionsButton", "Options",
                new Vector2(0, btnY - btnStep * 2), new Vector2(280, 55), new Color(0.3f, 0.3f, 0.4f));
            btnOptions.onClick.AddListener(mainMenu.OnOptionsClicked);

            var btnQuit = CreateButton(canvas.transform, "QuitButton", "Quit",
                new Vector2(0, btnY - btnStep * 3), new Vector2(280, 55), new Color(0.5f, 0.2f, 0.2f));
            btnQuit.onClick.AddListener(mainMenu.OnQuitClicked);

            BuildExaminePanel(canvas.transform);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneBuilder] Saved {scenePath}");
        }

        // ================================================================
        //  R001_WakeRoom シーン（強化版）
        // ================================================================
        private static void CreateR001Scene()
        {
            var scenePath = SceneDir + "/Chapter1/R001_WakeRoom.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildRoomCamera(new Color(0.12f, 0.14f, 0.18f));
            BuildEventSystem();
            BuildManagers();
            new GameObject("RoomManager").AddComponent<RoomManager>();

            // 背景
            CreateQuad("Background_Wall",  new Vector3(0, 0, 1f),    new Vector3(20, 12, 1), new Color(0.18f, 0.20f, 0.25f));
            CreateQuad("Background_Floor", new Vector3(0, -3.5f, 0.5f), new Vector3(20, 5, 1), new Color(0.12f, 0.13f, 0.16f));
            BuildRoomLabel("R001: 目覚めの部屋");

            // ---- ドア（working_key が必要、R002へ） ----
            var door = BuildDoor("door_01", "R002_Corridor",
                new Vector3(5f, 0.5f, 0), new Color(0.20f, 0.35f, 0.50f),
                "ドアはロックされている。\n鍵を使わないと開かない。",
                "カチャリ——！\nドアが開いた！");
            var doorSo = new SerializedObject(door);
            doorSo.FindProperty("requiredKey").objectReferenceValue = _itemWorkingKey;
            doorSo.ApplyModifiedPropertiesWithoutUndo();
            AddWorldLabel(door.gameObject, "出口", new Vector3(0, 2.2f, 0));

            // ---- 暗号ロックボックス → IDカード入手 ----
            // まず IDカード Pickup を作成（非表示）
            var idCardGo = BuildItemPickupGo("IDCardPickup",
                new Vector3(1.5f, -2f, 0), new Color(0.7f, 0.8f, 0.3f),
                new Vector3(0.8f, 0.5f, 1), _itemIdCard, "IDカード");
            idCardGo.SetActive(false);

            // SafeObject で IDカードを開放
            var keyboxSafeGo = new GameObject("KeyBoxSafe");
            var safe = keyboxSafeGo.AddComponent<SafeObject>();
            var safeSo = new SerializedObject(safe);
            safeSo.FindProperty("itemToReveal").objectReferenceValue = idCardGo;
            safeSo.FindProperty("unlockMessage").stringValue = "錠前が開いた！中にIDカードがあった！";
            safeSo.ApplyModifiedPropertiesWithoutUndo();

            BuildKeyBox(safe);

            // ---- 本棚（ヒント: 1234） ----
            BuildShelf();

            // ---- 引き出し（錆びた鍵） ----
            var drawerGo = BuildItemPickupGo("Drawer",
                new Vector3(-2f, -2f, 0), new Color(0.50f, 0.35f, 0.20f),
                new Vector3(2f, 1f, 1), _itemRustyKey, "引き出し");
            AddExaminable(drawerGo, "drawer_01", "古い引き出し。\n錆びた鍵が転がっている。");

            // ---- 薬棚（潤滑油） ----
            var medGo = BuildItemPickupGo("MedicineCabinet",
                new Vector3(3f, 1.5f, 0), new Color(0.30f, 0.50f, 0.40f),
                new Vector3(1.5f, 2f, 1), _itemOil, "薬棚");
            AddExaminable(medGo, "med_01", "薬棚。\n潤滑油のスプレー缶がある。");

            // ---- ストーリーログ #1 ----
            BuildStoryLog("StoryLog_001", new Vector3(-6.5f, 0.5f, 0),
                new Color(0.40f, 0.30f, 0.20f), _log001, "日誌");

            // ---- UI ----
            var canvas = BuildCanvas("HUDCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;
            BuildHUDBar(canvas.transform);
            BuildExaminePanel(canvas.transform);
            BuildPuzzleUIPanel(canvas.transform);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneBuilder] Saved {scenePath}");
        }

        // ================================================================
        //  R002_Corridor シーン
        // ================================================================
        private static void CreateR002Scene()
        {
            var scenePath = SceneDir + "/Chapter1/R002_Corridor.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildRoomCamera(new Color(0.08f, 0.09f, 0.12f));
            BuildEventSystem();
            BuildManagers();
            new GameObject("RoomManager").AddComponent<RoomManager>();

            // 背景（暗い廊下）
            CreateQuad("Background_Wall",  new Vector3(0, 0, 1f),    new Vector3(24, 12, 1), new Color(0.10f, 0.11f, 0.14f));
            CreateQuad("Background_Floor", new Vector3(0, -3.5f, 0.5f), new Vector3(24, 5, 1), new Color(0.07f, 0.08f, 0.10f));
            BuildRoomLabel("R002: 廊下");

            // ---- ドア（R003へ、SequencePuzzle解除後） ----
            var door = BuildDoor("door_02", "R003_LabA",
                new Vector3(7f, 0.5f, 0), new Color(0.15f, 0.25f, 0.40f),
                "配電盤を操作しないと開かない。",
                "カチャリ——！\n電源が入り、ドアが開いた！");
            AddWorldLabel(door.gameObject, "出口", new Vector3(0, 2.2f, 0));

            // ---- 配電盤（SequencePuzzle: 3→1→2→4） ----
            var fusebox = BuildSequencePuzzleContainer("FuseBox",
                new Vector3(-4f, 0f, 0), new Color(0.20f, 0.22f, 0.25f),
                new Vector3(2f, 3f, 1),
                "fuse_seq",
                new[] { 3, 1, 2, 4 },
                "配電盤。\nボタンを正しい順序で押せ。",
                door);
            AddWorldLabel(fusebox, "配電盤", new Vector3(0, 1.7f, 0));

            // ---- 回路図（ヒント） ----
            var diagramGo = CreateQuad("CircuitDiagram",
                new Vector3(-1f, 1f, 0), new Vector3(1.5f, 1f, 1), new Color(0.25f, 0.25f, 0.20f));
            diagramGo.AddComponent<BoxCollider2D>();
            var diag = diagramGo.AddComponent<ExaminableObject>();
            var diagSo = new SerializedObject(diag);
            diagSo.FindProperty("objectId").stringValue   = "circuit_01";
            diagSo.FindProperty("examineText").stringValue = "回路図。\n「3→1→2→4の順に接続せよ」と書かれている。";
            diagSo.ApplyModifiedPropertiesWithoutUndo();
            AddWorldLabel(diagramGo, "回路図", new Vector3(0, 0.7f, 0));

            // ---- 工具箱（ドライバー） ----
            var toolboxGo = BuildItemPickupGo("Toolbox",
                new Vector3(2f, -2f, 0), new Color(0.40f, 0.30f, 0.15f),
                new Vector3(1.8f, 1f, 1), _itemScrewdriver, "工具箱");
            AddExaminable(toolboxGo, "toolbox_01", "工具箱。\nドライバーが入っている。");

            // ---- ストーリーログ #2 ----
            BuildStoryLog("StoryLog_002", new Vector3(4f, -1f, 0),
                new Color(0.30f, 0.30f, 0.20f), _log002, "手紙");

            // ---- UI ----
            var canvas = BuildCanvas("HUDCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;
            BuildHUDBar(canvas.transform);
            BuildExaminePanel(canvas.transform);
            BuildSequencePuzzleUIPanel(canvas.transform);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneBuilder] Saved {scenePath}");
        }

        // ================================================================
        //  R003_LabA シーン
        // ================================================================
        private static void CreateR003Scene()
        {
            var scenePath = SceneDir + "/Chapter1/R003_LabA.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildRoomCamera(new Color(0.05f, 0.10f, 0.12f));
            BuildEventSystem();
            BuildManagers();
            new GameObject("RoomManager").AddComponent<RoomManager>();

            // 背景（研究室）
            CreateQuad("Background_Wall",  new Vector3(0, 0, 1f),    new Vector3(22, 12, 1), new Color(0.08f, 0.12f, 0.14f));
            CreateQuad("Background_Floor", new Vector3(0, -3.5f, 0.5f), new Vector3(22, 5, 1), new Color(0.05f, 0.09f, 0.11f));
            BuildRoomLabel("R003: 研究室A");

            // ---- 最終鍵のPickup（セーフ解除後に表示） ----
            var finalKeyGo = BuildItemPickupGo("FinalKeyPickup",
                new Vector3(0f, -1.5f, 0), new Color(0.8f, 0.7f, 0.1f),
                new Vector3(0.7f, 0.4f, 1), _itemFinalKey, "最終の鍵");
            finalKeyGo.SetActive(false);

            // ---- セーフ（SafeObject: ColorPuzzle解除後に最終鍵を開放） ----
            var safeGo = CreateQuad("Safe",
                new Vector3(-1f, -1f, 0), new Vector3(1.5f, 1.5f, 1), new Color(0.30f, 0.30f, 0.35f));
            safeGo.AddComponent<BoxCollider2D>();
            var safeObj = safeGo.AddComponent<SafeObject>();
            var safeObjSo = new SerializedObject(safeObj);
            safeObjSo.FindProperty("itemToReveal").objectReferenceValue = finalKeyGo;
            safeObjSo.FindProperty("unlockMessage").stringValue = "セーフが開いた！最終の鍵が見つかった！";
            safeObjSo.ApplyModifiedPropertiesWithoutUndo();
            AddWorldLabel(safeGo, "セーフ", new Vector3(0, 1f, 0));

            // ---- 色パネル（ColorPuzzle: Blue→Red→Yellow → SafeObject.Unlock） ----
            var colorPanel = BuildColorPuzzleContainer("ColorPanel",
                new Vector3(-4f, 0f, 0), new Color(0.15f, 0.18f, 0.25f),
                new Vector3(2f, 3f, 1),
                "r003_color",
                new[] { "Blue", "Red", "Yellow" },
                "色パネル。\n正しい順序で色を選べ。",
                safeObj);
            AddWorldLabel(colorPanel, "色パネル", new Vector3(0, 1.7f, 0));

            // ---- 色見本表（ヒント） ----
            var chartGo = CreateQuad("ColorChart",
                new Vector3(2f, 1f, 0), new Vector3(1.5f, 1f, 1), new Color(0.20f, 0.20f, 0.15f));
            chartGo.AddComponent<BoxCollider2D>();
            var chart = chartGo.AddComponent<ExaminableObject>();
            var chartSo = new SerializedObject(chart);
            chartSo.FindProperty("objectId").stringValue   = "colorchart_01";
            chartSo.FindProperty("examineText").stringValue = "色見本表。\n「青 → 赤 → 黄」の順と書かれている。";
            chartSo.ApplyModifiedPropertiesWithoutUndo();
            AddWorldLabel(chartGo, "色見本", new Vector3(0, 0.7f, 0));

            // ---- ストーリーログ #3 ----
            BuildStoryLog("StoryLog_003", new Vector3(4.5f, 0f, 0),
                new Color(0.20f, 0.25f, 0.30f), _log003, "記録");

            // ---- 出口ドア（最終鍵が必要、TriggerEnding） ----
            var exitDoor = BuildDoor("door_03", "",
                new Vector3(7f, 0.5f, 0), new Color(0.25f, 0.40f, 0.55f),
                "ここを開けるには鍵が必要だ。",
                "光が差し込んできた……！");
            var exitDoorSo = new SerializedObject(exitDoor);
            exitDoorSo.FindProperty("requiredKey").objectReferenceValue = _itemFinalKey;
            exitDoorSo.FindProperty("triggerEnding").boolValue = true;
            exitDoorSo.ApplyModifiedPropertiesWithoutUndo();
            AddWorldLabel(exitDoor.gameObject, "脱出口", new Vector3(0, 2.2f, 0));

            // ---- UI ----
            var canvas = BuildCanvas("HUDCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;
            BuildHUDBar(canvas.transform);
            BuildExaminePanel(canvas.transform);
            BuildColorPuzzleUIPanel(canvas.transform);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneBuilder] Saved {scenePath}");
        }

        // ================================================================
        //  エンディングシーン（True / Normal）
        // ================================================================
        private static void CreateEndingScene(bool isTrueEnding)
        {
            string sceneName = isTrueEnding ? "Ending_True" : "Ending_Normal";
            var scenePath = SceneDir + "/" + sceneName + ".unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = isTrueEnding ? new Color(0.05f, 0.10f, 0.15f) : new Color(0.10f, 0.08f, 0.05f);
            camGo.AddComponent<AudioListener>();

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            // Managers（スタンドアロン起動対応）
            BuildManagers();

            var canvas = BuildCanvas("EndingCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;

            CreatePanel(canvas.transform, "Background",
                Vector2.zero, new Vector2(1920, 1080),
                isTrueEnding ? new Color(0.04f, 0.08f, 0.12f) : new Color(0.08f, 0.06f, 0.04f));

            var titleTxt = CreateText(canvas.transform, "EndingTitle",
                isTrueEnding ? "TRUE ENDING" : "NORMAL ENDING",
                new Vector2(0, 200), new Vector2(700, 100),
                isTrueEnding ? new Color(0.3f, 0.9f, 1f) : new Color(1f, 0.8f, 0.4f),
                56, TextAnchor.MiddleCenter, bold: true);

            var endingTextGo = new GameObject("EndingText");
            endingTextGo.transform.SetParent(canvas.transform, false);
            var endingTxt = endingTextGo.AddComponent<Text>();
            endingTxt.fontSize  = 28;
            endingTxt.color     = Color.white;
            endingTxt.alignment = TextAnchor.MiddleCenter;
            var ert = endingTextGo.GetComponent<RectTransform>();
            ert.anchoredPosition = new Vector2(0, 50);
            ert.sizeDelta        = new Vector2(800, 200);

            var menuBtn = CreateButton(canvas.transform, "MenuButton", "メインメニューへ",
                new Vector2(0, -150), new Vector2(300, 60), new Color(0.2f, 0.4f, 0.6f));

            // EndingUI コンポーネント
            var endingUIGo = new GameObject("EndingUI");
            endingUIGo.transform.SetParent(canvas.transform, false);
            var endingUI = endingUIGo.AddComponent<EndingUI>();
            var euSo = new SerializedObject(endingUI);
            euSo.FindProperty("endingText").objectReferenceValue = endingTxt;
            euSo.FindProperty("isTrueEnding").boolValue          = isTrueEnding;
            euSo.ApplyModifiedPropertiesWithoutUndo();

            menuBtn.onClick.AddListener(endingUI.OnMenuButtonClicked);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneBuilder] Saved {scenePath}");
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
                SceneDir + "/Chapter1/R002_Corridor.unity",
                SceneDir + "/Chapter1/R003_LabA.unity",
                SceneDir + "/Ending_True.unity",
                SceneDir + "/Ending_Normal.unity",
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

            var imGo = new GameObject("InventoryManager");
            var im   = imGo.AddComponent<InventoryManager>();
            if (_itemWorkingKey != null)
            {
                var imSo      = new SerializedObject(im);
                var craftProp = imSo.FindProperty("craftableItems");
                craftProp.arraySize = 1;
                craftProp.GetArrayElementAtIndex(0).objectReferenceValue = _itemWorkingKey;
                imSo.ApplyModifiedPropertiesWithoutUndo();
            }

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
            var bgm = go.AddComponent<AudioSource>();
            bgm.loop        = true;
            bgm.volume      = 0.5f;
            bgm.playOnAwake = false;
            var sePool = new AudioSource[8];
            for (int i = 0; i < sePool.Length; i++)
            {
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.volume      = 0.8f;
                sePool[i]     = s;
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
        //  部屋共通ヘルパー
        // ================================================================
        private static void BuildRoomCamera(Color bgColor)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = bgColor;
            camGo.AddComponent<AudioListener>();
        }

        private static void BuildEventSystem()
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static void BuildRoomLabel(string text)
        {
            var canvas = BuildCanvas("RoomLabelCanvas", RenderMode.WorldSpace, 0);
            canvas.GetComponent<RectTransform>().localScale = Vector3.one * 0.01f;
            canvas.transform.position = new Vector3(-7f, 4.5f, 0);
            CreateText(canvas.transform, "RoomLabel", text,
                Vector2.zero, new Vector2(400, 60),
                new Color(0.5f, 0.7f, 1f), 24, TextAnchor.UpperLeft);
        }

        // ----------------------------------------------------------------
        //  DoorObject
        // ----------------------------------------------------------------
        private static DoorObject BuildDoor(string doorId, string targetScene,
            Vector3 pos, Color color, string lockedMsg, string openMsg)
        {
            var go = CreateQuad("Door_" + doorId, pos, new Vector3(1.8f, 4f, 1), color);
            go.AddComponent<BoxCollider2D>();
            var door = go.AddComponent<DoorObject>();
            var dso = new SerializedObject(door);
            dso.FindProperty("objectId").stringValue        = doorId;
            dso.FindProperty("targetSceneName").stringValue = targetScene;
            dso.FindProperty("isLocked").boolValue          = true;
            dso.FindProperty("lockedMessage").stringValue   = lockedMsg;
            dso.FindProperty("openMessage").stringValue     = openMsg;
            dso.ApplyModifiedPropertiesWithoutUndo();
            return door;
        }

        // ----------------------------------------------------------------
        //  本棚（R001）
        // ----------------------------------------------------------------
        private static void BuildShelf()
        {
            var go = CreateQuad("Shelf", new Vector3(-5f, 0f, 0),
                new Vector3(2.5f, 3.5f, 1), new Color(0.35f, 0.22f, 0.10f));
            go.AddComponent<BoxCollider2D>();
            var examinable = go.AddComponent<ExaminableObject>();
            var so = new SerializedObject(examinable);
            so.FindProperty("objectId").stringValue    = "shelf_01";
            so.FindProperty("examineText").stringValue =
                "古びた本棚。\n本がぎっしりと詰まっている。\nふと、一冊の本に挟まったメモが目に入った。\n\n　『 1 2 3 4 』\n\n何かの暗号だろうか……";
            so.ApplyModifiedPropertiesWithoutUndo();
            AddWorldLabel(go, "本棚", new Vector3(0, 2f, 0));
        }

        // ----------------------------------------------------------------
        //  KeyBox（R001）
        // ----------------------------------------------------------------
        private static void BuildKeyBox(SafeObject safeObj)
        {
            var go = CreateQuad("KeyBox", new Vector3(0f, -1f, 0),
                new Vector3(1.5f, 1.5f, 1), new Color(0.25f, 0.27f, 0.30f));
            go.AddComponent<BoxCollider2D>();

            var puzzle = go.AddComponent<NumericCodePuzzle>();
            var pso    = new SerializedObject(puzzle);
            pso.FindProperty("puzzleId").stringValue            = "r001_code";
            pso.FindProperty("digitCount").intValue             = 4;
            pso.FindProperty("puzzleData").objectReferenceValue = _puzzleData;
            pso.ApplyModifiedPropertiesWithoutUndo();

            // onSolved → SafeObject.Unlock()
            if (safeObj != null)
            {
                var fi = typeof(PuzzleBase).GetField("onSolved",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi != null)
                {
                    var evt = (UnityEngine.Events.UnityEvent)fi.GetValue(puzzle);
                    UnityEventTools.AddVoidPersistentListener(evt, safeObj.Unlock);
                }
            }

            var container = go.AddComponent<PuzzleContainer>();
            var cso = new SerializedObject(container);
            cso.FindProperty("objectId").stringValue   = "keybox_01";
            cso.FindProperty("examineText").stringValue = "4桁の暗号ロック。\n番号を入力すれば開くはずだ。";
            cso.FindProperty("puzzle").objectReferenceValue = puzzle;
            cso.ApplyModifiedPropertiesWithoutUndo();

            AddWorldLabel(go, "暗号ロック", new Vector3(0, 1f, 0));
        }

        // ----------------------------------------------------------------
        //  ItemPickup GameObject 作成
        // ----------------------------------------------------------------
        private static GameObject BuildItemPickupGo(string name, Vector3 pos, Color color,
            Vector3 scale, ItemData itemData, string label)
        {
            var go = CreateQuad(name, pos, scale, color);
            go.AddComponent<BoxCollider2D>();
            var pickup = go.AddComponent<ItemPickup>();
            var pso    = new SerializedObject(pickup);
            pso.FindProperty("objectId").stringValue            = name.ToLower();
            pso.FindProperty("itemData").objectReferenceValue   = itemData;
            pso.ApplyModifiedPropertiesWithoutUndo();
            AddWorldLabel(go, label, new Vector3(0, scale.y * 0.6f, 0));
            return go;
        }

        // ---- ExaminableObject を既存 GameObject に追加 ----
        private static void AddExaminable(GameObject go, string objId, string text)
        {
            // ItemPickup が既に付いている場合は、examineText は上書きしない（別コンポでは困る）
            // 引き出しや薬棚はそれ自体 ExaminableObject にはしない（ItemPickup が全部担当）
            // ここでは objectId だけ SerializedObject 経由で設定
            var pickup = go.GetComponent<ItemPickup>();
            if (pickup == null) return;
            var pso = new SerializedObject(pickup);
            pso.FindProperty("objectId").stringValue = objId;
            pso.ApplyModifiedPropertiesWithoutUndo();
        }

        // ----------------------------------------------------------------
        //  SequencePuzzle コンテナ作成
        // ----------------------------------------------------------------
        private static GameObject BuildSequencePuzzleContainer(
            string name, Vector3 pos, Color color, Vector3 scale,
            string puzzleId, int[] sequence, string examineText, DoorObject doorToUnlock)
        {
            var go = CreateQuad(name, pos, scale, color);
            go.AddComponent<BoxCollider2D>();

            var puzzle = go.AddComponent<SequencePuzzle>();
            var pso    = new SerializedObject(puzzle);
            pso.FindProperty("puzzleId").stringValue = puzzleId;
            var seqProp = pso.FindProperty("correctSequence");
            seqProp.arraySize = sequence.Length;
            for (int i = 0; i < sequence.Length; i++)
                seqProp.GetArrayElementAtIndex(i).intValue = sequence[i];
            pso.ApplyModifiedPropertiesWithoutUndo();

            // onSolved → DoorObject.Unlock()
            if (doorToUnlock != null)
            {
                var fi = typeof(PuzzleBase).GetField("onSolved",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi != null)
                {
                    var evt = (UnityEngine.Events.UnityEvent)fi.GetValue(puzzle);
                    UnityEventTools.AddVoidPersistentListener(evt, doorToUnlock.Unlock);
                }
            }

            var container = go.AddComponent<PuzzleContainer>();
            var cso = new SerializedObject(container);
            cso.FindProperty("objectId").stringValue            = name.ToLower();
            cso.FindProperty("examineText").stringValue         = examineText;
            cso.FindProperty("puzzle").objectReferenceValue     = puzzle;
            cso.ApplyModifiedPropertiesWithoutUndo();

            return go;
        }

        // ----------------------------------------------------------------
        //  ColorPuzzle コンテナ作成
        // ----------------------------------------------------------------
        private static GameObject BuildColorPuzzleContainer(
            string name, Vector3 pos, Color color, Vector3 scale,
            string puzzleId, string[] colorSeq, string examineText, SafeObject safeToUnlock)
        {
            var go = CreateQuad(name, pos, scale, color);
            go.AddComponent<BoxCollider2D>();

            var puzzle = go.AddComponent<ColorPuzzle>();
            var pso    = new SerializedObject(puzzle);
            pso.FindProperty("puzzleId").stringValue = puzzleId;
            var seqProp = pso.FindProperty("colorSequence");
            seqProp.arraySize = colorSeq.Length;
            for (int i = 0; i < colorSeq.Length; i++)
                seqProp.GetArrayElementAtIndex(i).stringValue = colorSeq[i];
            pso.ApplyModifiedPropertiesWithoutUndo();

            // onSolved → SafeObject.Unlock()
            if (safeToUnlock != null)
            {
                var fi = typeof(PuzzleBase).GetField("onSolved",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi != null)
                {
                    var evt = (UnityEngine.Events.UnityEvent)fi.GetValue(puzzle);
                    UnityEventTools.AddVoidPersistentListener(evt, safeToUnlock.Unlock);
                }
            }

            var container = go.AddComponent<PuzzleContainer>();
            var cso = new SerializedObject(container);
            cso.FindProperty("objectId").stringValue            = name.ToLower();
            cso.FindProperty("examineText").stringValue         = examineText;
            cso.FindProperty("puzzle").objectReferenceValue     = puzzle;
            cso.ApplyModifiedPropertiesWithoutUndo();

            return go;
        }

        // ----------------------------------------------------------------
        //  ストーリーログオブジェクト
        // ----------------------------------------------------------------
        private static void BuildStoryLog(string name, Vector3 pos, Color color,
            StoryLogData logData, string label)
        {
            var go = CreateQuad(name, pos, new Vector3(1f, 1.5f, 1), color);
            go.AddComponent<BoxCollider2D>();
            var storyLog = go.AddComponent<StoryLogObject>();
            var sso      = new SerializedObject(storyLog);
            sso.FindProperty("objectId").stringValue           = name.ToLower();
            sso.FindProperty("logData").objectReferenceValue   = logData;
            sso.ApplyModifiedPropertiesWithoutUndo();
            AddWorldLabel(go, label, new Vector3(0, 1f, 0));
        }

        // ================================================================
        //  HUD バー
        // ================================================================
        private static void BuildHUDBar(Transform parent)
        {
            var barBg = CreatePanel(parent, "HUDBar",
                Vector2.zero, new Vector2(0, 90), new Color(0.05f, 0.05f, 0.08f, 0.95f));
            var barRt = barBg.GetComponent<RectTransform>();
            barRt.anchorMin       = new Vector2(0, 0);
            barRt.anchorMax       = new Vector2(1, 0);
            barRt.pivot           = new Vector2(0.5f, 0);
            barRt.anchoredPosition = new Vector2(0, 0);
            barRt.sizeDelta       = new Vector2(0, 90);

            var invBarGo = new GameObject("InventoryBar", typeof(RectTransform));
            invBarGo.transform.SetParent(barBg.transform, false);
            var invRt = (RectTransform)invBarGo.transform;
            var hlg   = invBarGo.AddComponent<HorizontalLayoutGroup>();
            invRt.anchorMin  = new Vector2(0, 0);
            invRt.anchorMax  = new Vector2(1, 1);
            invRt.offsetMin  = new Vector2(10, 5);
            invRt.offsetMax  = new Vector2(-200, -5);
            hlg.spacing      = 5;
            hlg.childForceExpandHeight = true;
            hlg.childControlHeight     = false;
            hlg.childControlWidth      = false;

            var menuBtn    = CreateButton(barBg.transform, "MenuButton", "Menu",
                Vector2.zero, new Vector2(80, 40), new Color(0.3f, 0.3f, 0.4f));
            var menuBtnRt  = menuBtn.gameObject.GetComponent<RectTransform>();
            menuBtnRt.anchorMin       = new Vector2(1, 0.5f);
            menuBtnRt.anchorMax       = new Vector2(1, 0.5f);
            menuBtnRt.pivot           = new Vector2(1, 0.5f);
            menuBtnRt.anchoredPosition = new Vector2(-10, 0);

            var slotPrefab = CreateItemSlotPrefab();
            var hudGo      = new GameObject("HUDController");
            hudGo.transform.SetParent(barBg.transform, false);
            var hud   = hudGo.AddComponent<HUDController>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("inventoryBar").objectReferenceValue  = invBarGo.GetComponent<RectTransform>();
            hudSo.FindProperty("itemSlotPrefab").objectReferenceValue = slotPrefab;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            menuBtn.onClick.AddListener(hud.OnMenuButtonClicked);
        }

        // ================================================================
        //  ExaminePanel
        // ================================================================
        private static ExaminePanel BuildExaminePanel(Transform parent)
        {
            var panelBg = CreatePanel(parent, "ExaminePanel",
                Vector2.zero, new Vector2(900, 120), new Color(0.05f, 0.05f, 0.08f, 0.95f));
            var panelRt = panelBg.GetComponent<RectTransform>();
            panelRt.anchorMin       = new Vector2(0.5f, 0);
            panelRt.anchorMax       = new Vector2(0.5f, 0);
            panelRt.pivot           = new Vector2(0.5f, 0);
            panelRt.anchoredPosition = new Vector2(0, 100);

            var txtGo = new GameObject("MessageText");
            txtGo.transform.SetParent(panelBg.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text      = "";
            txt.fontSize  = 20;
            txt.color     = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin  = Vector2.zero;
            trt.anchorMax  = Vector2.one;
            trt.offsetMin  = new Vector2(20, 10);
            trt.offsetMax  = new Vector2(-20, -10);

            panelBg.SetActive(false);

            var ep   = panelBg.transform.parent.gameObject.AddComponent<ExaminePanel>();
            var so   = new SerializedObject(ep);
            so.FindProperty("panel").objectReferenceValue       = panelBg;
            so.FindProperty("messageText").objectReferenceValue = txt;
            so.ApplyModifiedPropertiesWithoutUndo();
            return ep;
        }

        // ================================================================
        //  PuzzleUIPanel（数字キーパッド）
        // ================================================================
        private static void BuildPuzzleUIPanel(Transform parent)
        {
            var overlay = CreatePanel(parent, "PuzzleOverlay",
                Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.7f));
            var ort = overlay.GetComponent<RectTransform>();
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.sizeDelta = Vector2.zero;

            var panelBg = CreatePanel(overlay.transform, "PuzzlePanel",
                Vector2.zero, new Vector2(350, 450), new Color(0.1f, 0.12f, 0.15f));

            CreateText(panelBg.transform, "TitleText", "暗号ロック",
                new Vector2(0, 180), new Vector2(320, 50),
                Color.white, 24, TextAnchor.MiddleCenter);

            var dispGo = new GameObject("DisplayText");
            dispGo.transform.SetParent(panelBg.transform, false);
            var dispTxt = dispGo.AddComponent<Text>();
            dispTxt.text      = "____";
            dispTxt.fontSize  = 40;
            dispTxt.color     = new Color(0.3f, 0.9f, 0.4f);
            dispTxt.alignment = TextAnchor.MiddleCenter;
            dispTxt.fontStyle = FontStyle.Bold;
            var drt = dispGo.GetComponent<RectTransform>();
            drt.anchoredPosition = new Vector2(0, 120);
            drt.sizeDelta        = new Vector2(300, 60);

            var keypadGo = new GameObject("Keypad");
            keypadGo.transform.SetParent(panelBg.transform, false);
            var grid = keypadGo.AddComponent<GridLayoutGroup>();
            var krt  = (RectTransform)keypadGo.transform;
            krt.anchoredPosition    = new Vector2(0, -20);
            krt.sizeDelta           = new Vector2(300, 260);
            grid.cellSize           = new Vector2(88, 60);
            grid.spacing            = new Vector2(8, 8);
            grid.constraint         = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount    = 3;

            overlay.SetActive(false);

            var puiGo = new GameObject("PuzzleUIPanel");
            puiGo.transform.SetParent(parent, false);
            var pui = puiGo.AddComponent<PuzzleUIPanel>();
            var pso = new SerializedObject(pui);
            pso.FindProperty("panel").objectReferenceValue       = overlay;
            pso.FindProperty("displayText").objectReferenceValue = dispTxt;
            pso.ApplyModifiedPropertiesWithoutUndo();

            int[] layout = { 1, 2, 3, 4, 5, 6, 7, 8, 9, -2, 0, -1 };
            foreach (int d in layout)
            {
                string label;
                if (d == -1)      label = "DEL";
                else if (d == -2) label = "CLR";
                else              label = d.ToString();

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

            var closeBtn = CreateButton(panelBg.transform, "CloseButton", "✕",
                new Vector2(0, -190), new Vector2(120, 45), new Color(0.45f, 0.2f, 0.2f));
            closeBtn.onClick.AddListener(pui.OnCloseButtonClicked);
        }

        // ================================================================
        //  SequencePuzzleUIPanel（4ボタン）
        // ================================================================
        private static void BuildSequencePuzzleUIPanel(Transform parent)
        {
            var overlay = CreatePanel(parent, "SeqPuzzleOverlay",
                Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.7f));
            var ort = overlay.GetComponent<RectTransform>();
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.sizeDelta = Vector2.zero;

            var panelBg = CreatePanel(overlay.transform, "SeqPuzzlePanel",
                Vector2.zero, new Vector2(380, 380), new Color(0.1f, 0.12f, 0.15f));

            CreateText(panelBg.transform, "TitleText", "配電盤",
                new Vector2(0, 155), new Vector2(340, 50),
                Color.white, 24, TextAnchor.MiddleCenter);

            var statusGo = new GameObject("StatusText");
            statusGo.transform.SetParent(panelBg.transform, false);
            var statusTxt = statusGo.AddComponent<Text>();
            statusTxt.text      = "ボタンを正しい順序で押せ";
            statusTxt.fontSize  = 18;
            statusTxt.color     = new Color(0.8f, 0.9f, 0.8f);
            statusTxt.alignment = TextAnchor.MiddleCenter;
            var srt = statusGo.GetComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(0, 100);
            srt.sizeDelta        = new Vector2(340, 50);

            // 2×2 ボタングリッド（1,2,3,4）
            var keypadGo = new GameObject("ButtonGrid");
            keypadGo.transform.SetParent(panelBg.transform, false);
            var grid = keypadGo.AddComponent<GridLayoutGroup>();
            var krt  = (RectTransform)keypadGo.transform;
            krt.anchoredPosition = new Vector2(0, -20);
            krt.sizeDelta        = new Vector2(300, 200);
            grid.cellSize        = new Vector2(135, 90);
            grid.spacing         = new Vector2(10, 10);
            grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            overlay.SetActive(false);

            var suiGo = new GameObject("SequencePuzzleUIPanel");
            suiGo.transform.SetParent(parent, false);
            var sui = suiGo.AddComponent<SequencePuzzleUIPanel>();
            var sso = new SerializedObject(sui);
            sso.FindProperty("panel").objectReferenceValue      = overlay;
            sso.FindProperty("statusText").objectReferenceValue = statusTxt;
            sso.ApplyModifiedPropertiesWithoutUndo();

            for (int i = 1; i <= 4; i++)
            {
                var btn   = CreateButton(keypadGo.transform, $"Btn_{i}", i.ToString(),
                    Vector2.zero, new Vector2(135, 90), new Color(0.20f, 0.25f, 0.35f));
                var btnId = i;
                btn.onClick.AddListener(() => sui.OnButtonClicked(btnId));
            }

            var closeBtn = CreateButton(panelBg.transform, "CloseButton", "✕",
                new Vector2(0, -155), new Vector2(120, 45), new Color(0.45f, 0.2f, 0.2f));
            closeBtn.onClick.AddListener(sui.OnCloseButtonClicked);
        }

        // ================================================================
        //  ColorPuzzleUIPanel（3色ボタン）
        // ================================================================
        private static void BuildColorPuzzleUIPanel(Transform parent)
        {
            var overlay = CreatePanel(parent, "ColorPuzzleOverlay",
                Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.7f));
            var ort = overlay.GetComponent<RectTransform>();
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.sizeDelta = Vector2.zero;

            var panelBg = CreatePanel(overlay.transform, "ColorPuzzlePanel",
                Vector2.zero, new Vector2(400, 360), new Color(0.1f, 0.12f, 0.15f));

            CreateText(panelBg.transform, "TitleText", "色パネル",
                new Vector2(0, 145), new Vector2(360, 50),
                Color.white, 24, TextAnchor.MiddleCenter);

            var statusGo = new GameObject("StatusText");
            statusGo.transform.SetParent(panelBg.transform, false);
            var statusTxt = statusGo.AddComponent<Text>();
            statusTxt.text      = "色を正しい順序で選べ";
            statusTxt.fontSize  = 18;
            statusTxt.color     = new Color(0.8f, 0.9f, 0.8f);
            statusTxt.alignment = TextAnchor.MiddleCenter;
            var srt = statusGo.GetComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(0, 90);
            srt.sizeDelta        = new Vector2(360, 50);

            overlay.SetActive(false);

            var cuiGo = new GameObject("ColorPuzzleUIPanel");
            cuiGo.transform.SetParent(parent, false);
            var cui = cuiGo.AddComponent<ColorPuzzleUIPanel>();
            var cso = new SerializedObject(cui);
            cso.FindProperty("panel").objectReferenceValue      = overlay;
            cso.FindProperty("statusText").objectReferenceValue = statusTxt;
            cso.ApplyModifiedPropertiesWithoutUndo();

            // 色ボタン（青、赤、黄）
            var colorDefs = new (string id, string label, Color btnColor)[]
            {
                ("Blue",   "青",  new Color(0.2f, 0.4f, 0.9f)),
                ("Red",    "赤",  new Color(0.9f, 0.2f, 0.2f)),
                ("Yellow", "黄",  new Color(0.9f, 0.8f, 0.1f)),
            };

            float btnX = -130f;
            foreach (var (id, label, btnColor) in colorDefs)
            {
                var btn    = CreateButton(panelBg.transform, $"Btn_{id}", label,
                    new Vector2(btnX, 10), new Vector2(110, 90), btnColor);
                var colorId = id;
                btn.onClick.AddListener(() => cui.OnColorClicked(colorId));
                btnX += 130f;
            }

            var closeBtn = CreateButton(panelBg.transform, "CloseButton", "✕",
                new Vector2(0, -140), new Vector2(120, 45), new Color(0.45f, 0.2f, 0.2f));
            closeBtn.onClick.AddListener(cui.OnCloseButtonClicked);
        }

        // ================================================================
        //  共通ヘルパー
        // ================================================================
        private static Canvas BuildCanvas(string name, RenderMode mode, int sortOrder)
        {
            var go     = new GameObject(name);
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
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt  = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            return go;
        }

        private static Text CreateText(Transform parent, string name, string content,
            Vector2 anchoredPos, Vector2 size, Color color,
            int fontSize, TextAnchor anchor, bool bold = false)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.text      = content;
            txt.fontSize  = fontSize;
            txt.color     = color;
            txt.alignment = anchor;
            if (bold) txt.fontStyle = FontStyle.Bold;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            return txt;
        }

        private static Button CreateButton(Transform parent, string name, string label,
            Vector2 anchoredPos, Vector2 size, Color bgColor)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            var rt  = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text      = label;
            txt.fontSize  = 20;
            txt.color     = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontStyle = FontStyle.Bold;
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin        = Vector2.zero;
            trt.anchorMax        = Vector2.one;
            trt.sizeDelta        = Vector2.zero;
            trt.anchoredPosition = Vector2.zero;

            return btn;
        }

        private static GameObject CreateQuad(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name            = name;
            go.transform.position   = pos;
            go.transform.localScale = scale;
            Object.DestroyImmediate(go.GetComponent<MeshCollider>());
            var mr  = go.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color       = color;
            mr.sharedMaterial = mat;
            return go;
        }

        private static void AddWorldLabel(GameObject parent, string text, Vector3 localOffset)
        {
            var canvas = new GameObject("LabelCanvas");
            canvas.transform.SetParent(parent.transform, false);
            canvas.transform.localPosition = localOffset;
            canvas.transform.localScale    = Vector3.one * 0.02f;
            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(canvas.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text      = text;
            txt.fontSize  = 24;
            txt.color     = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            var rt = txtGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 60);
        }
    }
}
