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
        private const string SceneDir  = "Assets/_Project/Scenes";
        private const string DataDir   = "Assets/_Project/Data";
        private const string PrefabDir = "Assets/_Project/Prefabs";

        // ================================================================
        //  Dark Science カラーパレット
        // ================================================================
        // 背景
        private static readonly Color C_BgDeep      = new Color(0.04f, 0.05f, 0.09f);
        private static readonly Color C_BgPanel     = new Color(0.06f, 0.09f, 0.16f, 0.97f);
        private static readonly Color C_BgPanelDark = new Color(0.04f, 0.06f, 0.12f, 0.98f);
        // アクセント
        private static readonly Color C_Cyan        = new Color(0.00f, 0.85f, 1.00f);
        private static readonly Color C_CyanDim     = new Color(0.00f, 0.55f, 0.72f);
        private static readonly Color C_CyanGlow    = new Color(0.00f, 0.85f, 1.00f, 0.50f);
        private static readonly Color C_Amber       = new Color(1.00f, 0.72f, 0.00f);
        private static readonly Color C_AmberGlow   = new Color(1.00f, 0.72f, 0.00f, 0.50f);
        private static readonly Color C_Red         = new Color(0.85f, 0.18f, 0.18f);
        // テキスト
        private static readonly Color C_TextPri     = new Color(0.90f, 0.96f, 1.00f);
        private static readonly Color C_TextSec     = new Color(0.45f, 0.68f, 0.82f);
        // ボタン
        private static readonly Color C_BtnNorm     = new Color(0.08f, 0.14f, 0.27f, 0.96f);
        private static readonly Color C_BtnDanger   = new Color(0.22f, 0.07f, 0.07f, 0.96f);
        // 枠
        private static readonly Color C_BorderCyan  = new Color(0.00f, 0.72f, 0.90f, 0.85f);
        private static readonly Color C_BorderAmber = new Color(0.90f, 0.65f, 0.00f, 0.85f);
        private static readonly Color C_BorderRed   = new Color(0.80f, 0.15f, 0.15f, 0.80f);

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
            _itemRustyKey    = CreateOrLoadItem("item_rusty_key",   "錆びた鍵",   ItemCategory.Key);
            _itemOil         = CreateOrLoadItem("item_oil",         "潤滑油",     ItemCategory.Material);
            _itemIdCard      = CreateOrLoadItem("item_id_card",     "IDカード",   ItemCategory.Information);
            _itemScrewdriver = CreateOrLoadItem("item_screwdriver", "ドライバー", ItemCategory.Tool);
            _itemWire        = CreateOrLoadItem("item_wire",        "ワイヤー",   ItemCategory.Material);
            _itemFinalKey    = CreateOrLoadItem("item_final_key",   "最終の鍵",   ItemCategory.Key);

            _itemWorkingKey = CreateOrLoadItem("item_working_key", "使える鍵", ItemCategory.Key);
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
            var img  = root.AddComponent<Image>();
            img.color = C_BtnNorm;
            root.AddComponent<Button>();
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            // ラベル
            var textGo = new GameObject("Label");
            textGo.transform.SetParent(root.transform, false);
            var txt = textGo.AddComponent<Text>();
            txt.text      = "Item";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize  = 11;
            txt.color     = C_TextPri;
            AddTextShadow(txt);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin        = Vector2.zero;
            trt.anchorMax        = Vector2.one;
            trt.sizeDelta        = Vector2.zero;
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
            cam.backgroundColor  = C_BgDeep;
            camGo.AddComponent<AudioListener>();

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            BuildManagers();

            var canvas = BuildCanvas("MainMenuCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;
            var ct = canvas.transform;

            // 背景
            var bg = CreatePanel(ct, "Background", Vector2.zero, new Vector2(1920, 1080), C_BgDeep);
            SetStretch(bg);

            // 上部装飾ライン
            var topLine = CreatePanel(ct, "TopLine", Vector2.zero, new Vector2(0, 2), C_CyanGlow);
            SetAnchorStretchH(topLine, 1, 1, 0);

            // 下部装飾ライン
            var botLine = CreatePanel(ct, "BotLine", Vector2.zero, new Vector2(0, 2), C_CyanGlow);
            SetAnchorStretchH(botLine, 0, 0, 0);

            // 中央パネル（タイトル＋ボタン）
            var (centerOuter, centerInner) = CreateStyledPanel(ct, "CenterPanel",
                new Vector2(0, 30), new Vector2(420, 480),
                C_BgPanel, C_BorderCyan, 1.5f);

            // タイトル "ESCAPE"
            var titleTxt = CreateText(centerInner.transform, "TitleText", "ESCAPE",
                new Vector2(0, 165), new Vector2(380, 100),
                C_Cyan, 68, TextAnchor.MiddleCenter, bold: true);
            AddTextOutline(titleTxt, new Color(0f, 0.4f, 0.6f, 0.7f));
            AddTextShadow(titleTxt, new Color(0f, 0.5f, 0.8f, 0.5f));

            // サブタイトル
            var subTxt = CreateText(centerInner.transform, "SubTitle", "〜  脱  出  ゲ  ー  ム  〜",
                new Vector2(0, 110), new Vector2(380, 40),
                C_TextSec, 18, TextAnchor.MiddleCenter);
            AddTextShadow(subTxt);

            // 区切りライン
            CreatePanel(centerInner.transform, "Divider",
                new Vector2(0, 78), new Vector2(340, 1), C_CyanDim);

            // ボタン群
            var mainMenu = canvas.gameObject.AddComponent<MainMenuUI>();

            float btnY    = 40f;
            float btnStep = 66f;

            var btnNewGame = CreateStyledButton(centerInner.transform, "NewGameButton", "▶  NEW GAME",
                new Vector2(0, btnY), new Vector2(340, 52), C_BtnNorm, C_BorderCyan);
            var newGameLbl = btnNewGame.GetComponentInChildren<Text>();
            newGameLbl.color = C_Cyan;
            AddTextShadow(newGameLbl, C_CyanGlow);
            btnNewGame.onClick.AddListener(mainMenu.OnNewGameClicked);

            var btnLoad = CreateStyledButton(centerInner.transform, "LoadButton", "LOAD GAME",
                new Vector2(0, btnY - btnStep), new Vector2(340, 52), C_BtnNorm, C_BorderCyan);
            GetBtnLabel(btnLoad).color = C_TextPri;
            btnLoad.onClick.AddListener(mainMenu.OnLoadGameClicked);

            var btnOptions = CreateStyledButton(centerInner.transform, "OptionsButton", "OPTIONS",
                new Vector2(0, btnY - btnStep * 2), new Vector2(340, 52), C_BtnNorm, C_BorderCyan);
            GetBtnLabel(btnOptions).color = C_TextPri;
            btnOptions.onClick.AddListener(mainMenu.OnOptionsClicked);

            var btnQuit = CreateStyledButton(centerInner.transform, "QuitButton", "QUIT",
                new Vector2(0, btnY - btnStep * 3), new Vector2(340, 52), C_BtnDanger, C_BorderRed);
            GetBtnLabel(btnQuit).color = new Color(1f, 0.6f, 0.6f);
            btnQuit.onClick.AddListener(mainMenu.OnQuitClicked);

            // 画面四隅 HUD
            BuildCornerHUD(ct, 55f, 2f);

            // ExaminePanel
            BuildExaminePanel(ct);

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

            BuildRoomCamera(new Color(0.04f, 0.05f, 0.09f));
            BuildEventSystem();
            BuildManagers();
            new GameObject("RoomManager").AddComponent<RoomManager>();

            BuildScienceBackground("R001: 目覚めの部屋",
                new Color(0.12f, 0.14f, 0.20f),
                new Color(0.07f, 0.08f, 0.12f),
                C_Cyan);

            // ---- ドア ----
            var door = BuildDoor("door_01", "R002_Corridor",
                new Vector3(5f, 0.5f, 0), new Color(0.10f, 0.18f, 0.30f),
                "ドアはロックされている。\n鍵を使わないと開かない。",
                "カチャリ——！\nドアが開いた！");
            var doorSo = new SerializedObject(door);
            doorSo.FindProperty("requiredKey").objectReferenceValue = _itemWorkingKey;
            doorSo.ApplyModifiedPropertiesWithoutUndo();
            AddObjectFrame(door.gameObject, C_Cyan, 0.10f);
            AddWorldLabel(door.gameObject, "出口", new Vector3(0, 2.2f, 0));

            // ---- IDカード Pickup（非表示、KeyBox解除後に出現） ----
            var idCardGo = BuildItemPickupGo("IDCardPickup",
                new Vector3(1.5f, -2f, 0), new Color(0.55f, 0.65f, 0.20f),
                new Vector3(0.8f, 0.5f, 1), _itemIdCard, "IDカード");
            idCardGo.SetActive(false);

            var keyboxSafeGo = new GameObject("KeyBoxSafe");
            var safe = keyboxSafeGo.AddComponent<SafeObject>();
            var safeSo = new SerializedObject(safe);
            safeSo.FindProperty("itemToReveal").objectReferenceValue   = idCardGo;
            safeSo.FindProperty("unlockMessage").stringValue = "錠前が開いた！\nIDカードがあった！";
            safeSo.ApplyModifiedPropertiesWithoutUndo();

            BuildKeyBox(safe);

            // ---- 本棚 ----
            BuildShelf();

            // ---- 引き出し（錆びた鍵） ----
            var drawerGo = BuildItemPickupGo("Drawer",
                new Vector3(-2.2f, -2.2f, 0), new Color(0.30f, 0.20f, 0.10f),
                new Vector3(2f, 1f, 1), _itemRustyKey, "引き出し");
            AddObjectFrame(drawerGo, C_Amber, 0.07f);

            // ---- 薬棚（潤滑油） ----
            var medGo = BuildItemPickupGo("MedicineCabinet",
                new Vector3(3f, 1.5f, 0), new Color(0.12f, 0.22f, 0.20f),
                new Vector3(1.5f, 2f, 1), _itemOil, "薬棚");
            AddObjectFrame(medGo, C_Cyan, 0.07f);

            // ---- ストーリーログ #1 ----
            BuildStoryLog("StoryLog_001", new Vector3(-6.5f, 0.5f, 0),
                new Color(0.22f, 0.17f, 0.10f), _log001, "日誌");

            // ---- UI ----
            var canvas = BuildCanvas("HUDCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;
            BuildHUDBar(canvas.transform);
            BuildExaminePanel(canvas.transform);
            BuildPuzzleUIPanel(canvas.transform);
            BuildCornerHUD(canvas.transform, 45f, 1.5f);

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

            BuildRoomCamera(new Color(0.03f, 0.04f, 0.07f));
            BuildEventSystem();
            BuildManagers();
            new GameObject("RoomManager").AddComponent<RoomManager>();

            BuildScienceBackground("R002: 廊下",
                new Color(0.07f, 0.08f, 0.12f),
                new Color(0.04f, 0.05f, 0.08f),
                C_Amber);

            // ---- ドア（R003へ） ----
            var door = BuildDoor("door_02", "R003_LabA",
                new Vector3(7f, 0.5f, 0), new Color(0.10f, 0.16f, 0.26f),
                "配電盤を操作しないと開かない。",
                "カチャリ——！\n電源が入り、ドアが開いた！");
            AddObjectFrame(door.gameObject, C_Cyan, 0.10f);
            AddWorldLabel(door.gameObject, "出口", new Vector3(0, 2.2f, 0));

            // ---- 配電盤 ----
            var fusebox = BuildSequencePuzzleContainer("FuseBox",
                new Vector3(-4f, 0f, 0), new Color(0.15f, 0.17f, 0.20f),
                new Vector3(2f, 3f, 1),
                "fuse_seq", new[] { 3, 1, 2, 4 },
                "配電盤。\nボタンを正しい順序で押せ。", door);
            AddObjectFrame(fusebox, C_Amber, 0.08f);
            AddWorldLabel(fusebox, "配電盤", new Vector3(0, 1.7f, 0));

            // ---- 回路図 ----
            var diagramGo = CreateQuad("CircuitDiagram",
                new Vector3(-1f, 1f, 0), new Vector3(1.5f, 1f, 1), new Color(0.18f, 0.18f, 0.14f));
            diagramGo.AddComponent<BoxCollider2D>();
            var diag   = diagramGo.AddComponent<ExaminableObject>();
            var diagSo = new SerializedObject(diag);
            diagSo.FindProperty("objectId").stringValue    = "circuit_01";
            diagSo.FindProperty("examineText").stringValue = "回路図。\n「3→1→2→4の順に接続せよ」と書かれている。";
            diagSo.ApplyModifiedPropertiesWithoutUndo();
            AddObjectFrame(diagramGo, C_Amber, 0.06f);
            AddWorldLabel(diagramGo, "回路図", new Vector3(0, 0.7f, 0));

            // ---- 工具箱 ----
            var toolboxGo = BuildItemPickupGo("Toolbox",
                new Vector3(2f, -2f, 0), new Color(0.28f, 0.20f, 0.10f),
                new Vector3(1.8f, 1f, 1), _itemScrewdriver, "工具箱");
            AddObjectFrame(toolboxGo, C_Amber, 0.07f);

            // ---- ストーリーログ #2 ----
            BuildStoryLog("StoryLog_002", new Vector3(4f, -1f, 0),
                new Color(0.20f, 0.18f, 0.12f), _log002, "手紙");

            // ---- UI ----
            var canvas = BuildCanvas("HUDCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;
            BuildHUDBar(canvas.transform);
            BuildExaminePanel(canvas.transform);
            BuildSequencePuzzleUIPanel(canvas.transform);
            BuildCornerHUD(canvas.transform, 45f, 1.5f);

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

            BuildRoomCamera(new Color(0.03f, 0.06f, 0.09f));
            BuildEventSystem();
            BuildManagers();
            new GameObject("RoomManager").AddComponent<RoomManager>();

            BuildScienceBackground("R003: 研究室A",
                new Color(0.05f, 0.09f, 0.13f),
                new Color(0.03f, 0.06f, 0.09f),
                C_Cyan);

            // ---- 最終鍵のPickup（セーフ解除後に出現） ----
            var finalKeyGo = BuildItemPickupGo("FinalKeyPickup",
                new Vector3(0f, -1.5f, 0), new Color(0.6f, 0.55f, 0.05f),
                new Vector3(0.7f, 0.4f, 1), _itemFinalKey, "最終の鍵");
            finalKeyGo.SetActive(false);

            // ---- セーフ ----
            var safeGo = CreateQuad("Safe",
                new Vector3(-1f, -1f, 0), new Vector3(1.5f, 1.5f, 1), new Color(0.20f, 0.22f, 0.28f));
            safeGo.AddComponent<BoxCollider2D>();
            var safeObj   = safeGo.AddComponent<SafeObject>();
            var safeObjSo = new SerializedObject(safeObj);
            safeObjSo.FindProperty("itemToReveal").objectReferenceValue = finalKeyGo;
            safeObjSo.FindProperty("unlockMessage").stringValue = "セーフが開いた！\n最終の鍵が見つかった！";
            safeObjSo.ApplyModifiedPropertiesWithoutUndo();
            AddObjectFrame(safeGo, C_Amber, 0.08f);
            AddWorldLabel(safeGo, "セーフ", new Vector3(0, 1f, 0));

            // ---- 色パネル ----
            var colorPanel = BuildColorPuzzleContainer("ColorPanel",
                new Vector3(-4f, 0f, 0), new Color(0.10f, 0.12f, 0.18f),
                new Vector3(2f, 3f, 1),
                "r003_color", new[] { "Blue", "Red", "Yellow" },
                "色パネル。\n正しい順序で色を選べ。", safeObj);
            AddObjectFrame(colorPanel, C_Cyan, 0.08f);
            AddWorldLabel(colorPanel, "色パネル", new Vector3(0, 1.7f, 0));

            // ---- 色見本 ----
            var chartGo = CreateQuad("ColorChart",
                new Vector3(2f, 1f, 0), new Vector3(1.5f, 1f, 1), new Color(0.14f, 0.16f, 0.14f));
            chartGo.AddComponent<BoxCollider2D>();
            var chart   = chartGo.AddComponent<ExaminableObject>();
            var chartSo = new SerializedObject(chart);
            chartSo.FindProperty("objectId").stringValue    = "colorchart_01";
            chartSo.FindProperty("examineText").stringValue = "色見本表。\n「青 → 赤 → 黄」の順と書かれている。";
            chartSo.ApplyModifiedPropertiesWithoutUndo();
            AddObjectFrame(chartGo, C_Cyan, 0.06f);
            AddWorldLabel(chartGo, "色見本", new Vector3(0, 0.7f, 0));

            // ---- ストーリーログ #3 ----
            BuildStoryLog("StoryLog_003", new Vector3(4.5f, 0f, 0),
                new Color(0.18f, 0.20f, 0.25f), _log003, "記録");

            // ---- 出口ドア ----
            var exitDoor   = BuildDoor("door_03", "",
                new Vector3(7f, 0.5f, 0), new Color(0.10f, 0.20f, 0.32f),
                "ここを開けるには鍵が必要だ。",
                "光が差し込んできた……！");
            var exitDoorSo = new SerializedObject(exitDoor);
            exitDoorSo.FindProperty("requiredKey").objectReferenceValue = _itemFinalKey;
            exitDoorSo.FindProperty("triggerEnding").boolValue          = true;
            exitDoorSo.ApplyModifiedPropertiesWithoutUndo();
            AddObjectFrame(exitDoor.gameObject, C_Cyan, 0.10f);
            AddWorldLabel(exitDoor.gameObject, "脱出口", new Vector3(0, 2.2f, 0));

            // ---- UI ----
            var canvas = BuildCanvas("HUDCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;
            BuildHUDBar(canvas.transform);
            BuildExaminePanel(canvas.transform);
            BuildColorPuzzleUIPanel(canvas.transform);
            BuildCornerHUD(canvas.transform, 45f, 1.5f);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneBuilder] Saved {scenePath}");
        }

        // ================================================================
        //  エンディングシーン
        // ================================================================
        private static void CreateEndingScene(bool isTrueEnding)
        {
            string sceneName = isTrueEnding ? "Ending_True" : "Ending_Normal";
            var scenePath    = SceneDir + "/" + sceneName + ".unity";
            var scene        = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = isTrueEnding ? new Color(0.03f, 0.06f, 0.12f) : new Color(0.08f, 0.06f, 0.04f);
            camGo.AddComponent<AudioListener>();

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            BuildManagers();

            var canvas = BuildCanvas("EndingCanvas", RenderMode.ScreenSpaceOverlay, 99);
            canvas.sortingOrder = 10;
            var ct = canvas.transform;

            // 背景
            var bg = CreatePanel(ct, "Background", Vector2.zero, new Vector2(1920, 1080),
                isTrueEnding ? new Color(0.03f, 0.06f, 0.12f) : new Color(0.08f, 0.05f, 0.03f));
            SetStretch(bg);

            // 放射状グロー演出（中央の輝き）
            Color glowColor = isTrueEnding ? new Color(0.00f, 0.60f, 0.80f, 0.08f) : new Color(0.70f, 0.45f, 0.00f, 0.08f);
            for (int i = 3; i >= 1; i--)
            {
                var glow = CreatePanel(ct, $"Glow_{i}", Vector2.zero, new Vector2(500f * i, 500f * i), glowColor);
                SetAnchorCenter(glow);
            }

            // タイプ別アクセントカラー
            Color accentColor = isTrueEnding ? C_Cyan : C_Amber;

            // 上部ライン
            var topLine = CreatePanel(ct, "TopLine", Vector2.zero, new Vector2(0, 2),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.8f));
            SetAnchorStretchH(topLine, 1, 1, 0);
            var botLine = CreatePanel(ct, "BotLine", Vector2.zero, new Vector2(0, 2),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.8f));
            SetAnchorStretchH(botLine, 0, 0, 0);

            // タイトル
            var titleLabel = isTrueEnding ? "◆  TRUE ENDING  ◆" : "NORMAL ENDING";
            var titleTxt   = CreateText(ct, "EndingTitle", titleLabel,
                new Vector2(0, 160), new Vector2(800, 90),
                accentColor, 52, TextAnchor.MiddleCenter, bold: true);
            SetAnchorCenter(titleTxt.gameObject);
            AddTextOutline(titleTxt, new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f));
            AddTextShadow(titleTxt, new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f));

            // 区切りライン
            var divider = CreatePanel(ct, "Divider",
                new Vector2(0, 110), new Vector2(600, 1),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f));
            SetAnchorCenter(divider);

            // エンディングテキスト（EndingUI が書き換える）
            var endingTextGo = new GameObject("EndingText");
            endingTextGo.transform.SetParent(ct, false);
            var endingTxt        = endingTextGo.AddComponent<Text>();
            endingTxt.fontSize   = 26;
            endingTxt.color      = C_TextPri;
            endingTxt.alignment  = TextAnchor.MiddleCenter;
            endingTxt.lineSpacing = 1.4f;
            var ert = endingTextGo.GetComponent<RectTransform>();
            ert.anchorMin        = new Vector2(0.5f, 0.5f);
            ert.anchorMax        = new Vector2(0.5f, 0.5f);
            ert.pivot            = new Vector2(0.5f, 0.5f);
            ert.anchoredPosition = new Vector2(0, 30);
            ert.sizeDelta        = new Vector2(700, 180);
            AddTextShadow(endingTxt, new Color(accentColor.r, accentColor.g, accentColor.b, 0.35f));

            // メニューボタン
            var menuBtn = CreateStyledButton(ct, "MenuButton", "メインメニューへ",
                new Vector2(0, -130), new Vector2(300, 54),
                C_BtnNorm, C_BorderCyan);
            GetBtnLabel(menuBtn).color = C_TextPri;
            SetAnchorCenter(menuBtn.gameObject);

            // EndingUI
            var endingUIGo = new GameObject("EndingUI");
            endingUIGo.transform.SetParent(ct, false);
            var endingUI = endingUIGo.AddComponent<EndingUI>();
            var euSo = new SerializedObject(endingUI);
            euSo.FindProperty("endingText").objectReferenceValue = endingTxt;
            euSo.FindProperty("isTrueEnding").boolValue          = isTrueEnding;
            euSo.ApplyModifiedPropertiesWithoutUndo();
            menuBtn.onClick.AddListener(endingUI.OnMenuButtonClicked);

            BuildCornerHUD(ct, 45f, 1.5f);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneBuilder] Saved {scenePath}");
        }

        // ================================================================
        //  Build Settings
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
            var go  = new GameObject("AudioManager");
            var am  = go.AddComponent<AudioManager>();
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
                sePool[i] = s;
            }
            var so       = new SerializedObject(am);
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

        // ----------------------------------------------------------------
        //  Science 背景（壁・床・装飾ライン）
        // ----------------------------------------------------------------
        private static void BuildScienceBackground(
            string roomName, Color wallColor, Color floorColor, Color accentColor)
        {
            // 壁
            CreateQuad("BG_Wall", new Vector3(0, 0.5f, 2f), new Vector3(24, 12, 1), wallColor);

            // 床
            CreateQuad("BG_Floor", new Vector3(0, -4f, 1.8f), new Vector3(24, 4.5f, 1), floorColor);

            // 床/壁の境界ライン（アクセント）
            CreateQuad("BG_FloorLine", new Vector3(0, -1.85f, 1.7f),
                new Vector3(24, 0.035f, 1),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.70f));

            // 壁パネルの縦縫い目
            for (int i = -4; i <= 4; i++)
            {
                CreateQuad($"BG_Seam_{i}",
                    new Vector3(i * 2.8f, 0.8f, 1.95f),
                    new Vector3(0.022f, 4.8f, 1),
                    new Color(accentColor.r, accentColor.g, accentColor.b, 0.09f));
            }

            // 天井グロー（アクセントカラーの淡い発光）
            CreateQuad("BG_CeilGlow", new Vector3(0, 5.0f, 1.6f),
                new Vector3(24, 1.0f, 1),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.06f));

            // 天井ライン
            CreateQuad("BG_CeilLine", new Vector3(0, 5.3f, 1.55f),
                new Vector3(24, 0.035f, 1),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.45f));

            // 床グリッドライン（水平）
            for (int i = 0; i < 4; i++)
            {
                float y = -2.4f - i * 0.5f;
                CreateQuad($"BG_FloorGrid_{i}",
                    new Vector3(0, y, 1.75f),
                    new Vector3(24, 0.012f, 1),
                    new Color(accentColor.r, accentColor.g, accentColor.b, 0.04f));
            }

            // 部屋ラベル（ワールドスペース）
            var canvas = BuildCanvas("RoomLabelCanvas", RenderMode.WorldSpace, 0);
            canvas.GetComponent<RectTransform>().localScale = Vector3.one * 0.01f;
            canvas.transform.position = new Vector3(-8f, 4.4f, 0.5f);
            var lbl = CreateText(canvas.transform, "RoomLabel", roomName,
                Vector2.zero, new Vector2(500, 50),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.80f),
                20, TextAnchor.UpperLeft);
            AddTextShadow(lbl, new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f));
        }

        // ----------------------------------------------------------------
        //  四隅 HUD コーナー装飾
        // ----------------------------------------------------------------
        private static void BuildCornerHUD(Transform parent, float len, float thick)
        {
            Color c = new Color(C_Cyan.r, C_Cyan.g, C_Cyan.b, 0.65f);
            BuildOneCorner(parent, "TL", new Vector2(0, 1), new Vector2(0, 1),  1,  1, len, thick, c);
            BuildOneCorner(parent, "TR", new Vector2(1, 1), new Vector2(1, 1), -1,  1, len, thick, c);
            BuildOneCorner(parent, "BL", new Vector2(0, 0), new Vector2(0, 0),  1, -1, len, thick, c);
            BuildOneCorner(parent, "BR", new Vector2(1, 0), new Vector2(1, 0), -1, -1, len, thick, c);
        }

        private static void BuildOneCorner(Transform parent, string id,
            Vector2 anchor, Vector2 pivot, int signX, int signY, float len, float thick, Color c)
        {
            float margin = 18f;

            // 水平ライン
            var h    = CreatePanel(parent, $"HUD_{id}_H", Vector2.zero, new Vector2(len, thick), c);
            var hrt  = h.GetComponent<RectTransform>();
            hrt.anchorMin        = anchor;
            hrt.anchorMax        = anchor;
            hrt.pivot            = new Vector2(signX > 0 ? 0 : 1, signY > 0 ? 0 : 1);
            hrt.anchoredPosition = new Vector2(signX * margin, signY * margin);

            // 垂直ライン
            var v    = CreatePanel(parent, $"HUD_{id}_V", Vector2.zero, new Vector2(thick, len), c);
            var vrt  = v.GetComponent<RectTransform>();
            vrt.anchorMin        = anchor;
            vrt.anchorMax        = anchor;
            vrt.pivot            = new Vector2(signX > 0 ? 0 : 1, signY > 0 ? 0 : 1);
            vrt.anchoredPosition = new Vector2(signX * margin, signY * margin);
        }

        // ----------------------------------------------------------------
        //  ドア
        // ----------------------------------------------------------------
        private static DoorObject BuildDoor(string doorId, string targetScene,
            Vector3 pos, Color color, string lockedMsg, string openMsg)
        {
            var go   = CreateQuad("Door_" + doorId, pos, new Vector3(1.8f, 4f, 1), color);
            go.AddComponent<BoxCollider2D>();
            var door = go.AddComponent<DoorObject>();
            var dso  = new SerializedObject(door);
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
                new Vector3(2.5f, 3.5f, 1), new Color(0.24f, 0.16f, 0.08f));
            go.AddComponent<BoxCollider2D>();
            var examinable = go.AddComponent<ExaminableObject>();
            var so         = new SerializedObject(examinable);
            so.FindProperty("objectId").stringValue    = "shelf_01";
            so.FindProperty("examineText").stringValue =
                "古びた本棚。\n本がぎっしりと詰まっている。\n\nふと、一冊の本に挟まったメモが目に入った。\n\n　『 1 2 3 4 』\n\n何かの暗号だろうか……";
            so.ApplyModifiedPropertiesWithoutUndo();
            AddObjectFrame(go, C_Amber, 0.08f);
            AddWorldLabel(go, "本棚", new Vector3(0, 2f, 0));
        }

        // ----------------------------------------------------------------
        //  暗号ロックボックス（R001）
        // ----------------------------------------------------------------
        private static void BuildKeyBox(SafeObject safeObj)
        {
            var go     = CreateQuad("KeyBox", new Vector3(0f, -1f, 0),
                new Vector3(1.5f, 1.5f, 1), new Color(0.16f, 0.18f, 0.22f));
            go.AddComponent<BoxCollider2D>();

            var puzzle = go.AddComponent<NumericCodePuzzle>();
            var pso    = new SerializedObject(puzzle);
            pso.FindProperty("puzzleId").stringValue            = "r001_code";
            pso.FindProperty("digitCount").intValue             = 4;
            pso.FindProperty("puzzleData").objectReferenceValue = _puzzleData;
            pso.ApplyModifiedPropertiesWithoutUndo();

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
            var cso       = new SerializedObject(container);
            cso.FindProperty("objectId").stringValue            = "keybox_01";
            cso.FindProperty("examineText").stringValue         = "4桁の暗号ロック。\n番号を入力すれば開くはずだ。";
            cso.FindProperty("puzzle").objectReferenceValue     = puzzle;
            cso.ApplyModifiedPropertiesWithoutUndo();

            AddObjectFrame(go, C_Cyan, 0.08f);
            AddWorldLabel(go, "暗号ロック", new Vector3(0, 1f, 0));
        }

        // ----------------------------------------------------------------
        //  ItemPickup オブジェクト生成
        // ----------------------------------------------------------------
        private static GameObject BuildItemPickupGo(string name, Vector3 pos, Color color,
            Vector3 scale, ItemData itemData, string label)
        {
            var go     = CreateQuad(name, pos, scale, color);
            go.AddComponent<BoxCollider2D>();
            var pickup = go.AddComponent<ItemPickup>();
            var pso    = new SerializedObject(pickup);
            pso.FindProperty("objectId").stringValue            = name.ToLower();
            pso.FindProperty("itemData").objectReferenceValue   = itemData;
            pso.ApplyModifiedPropertiesWithoutUndo();
            AddWorldLabel(go, label, new Vector3(0, scale.y * 0.6f, 0));
            return go;
        }

        // ---- ExaminableObject の objectId を設定 ----
        private static void AddExaminable(GameObject go, string objId, string text)
        {
            var pickup = go.GetComponent<ItemPickup>();
            if (pickup == null) return;
            var pso = new SerializedObject(pickup);
            pso.FindProperty("objectId").stringValue = objId;
            pso.ApplyModifiedPropertiesWithoutUndo();
        }

        // ----------------------------------------------------------------
        //  SequencePuzzle コンテナ
        // ----------------------------------------------------------------
        private static GameObject BuildSequencePuzzleContainer(
            string name, Vector3 pos, Color color, Vector3 scale,
            string puzzleId, int[] sequence, string examineText, DoorObject doorToUnlock)
        {
            var go     = CreateQuad(name, pos, scale, color);
            go.AddComponent<BoxCollider2D>();

            var puzzle = go.AddComponent<SequencePuzzle>();
            var pso    = new SerializedObject(puzzle);
            pso.FindProperty("puzzleId").stringValue = puzzleId;
            var seqProp = pso.FindProperty("correctSequence");
            seqProp.arraySize = sequence.Length;
            for (int i = 0; i < sequence.Length; i++)
                seqProp.GetArrayElementAtIndex(i).intValue = sequence[i];
            pso.ApplyModifiedPropertiesWithoutUndo();

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
            var cso       = new SerializedObject(container);
            cso.FindProperty("objectId").stringValue            = name.ToLower();
            cso.FindProperty("examineText").stringValue         = examineText;
            cso.FindProperty("puzzle").objectReferenceValue     = puzzle;
            cso.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        // ----------------------------------------------------------------
        //  ColorPuzzle コンテナ
        // ----------------------------------------------------------------
        private static GameObject BuildColorPuzzleContainer(
            string name, Vector3 pos, Color color, Vector3 scale,
            string puzzleId, string[] colorSeq, string examineText, SafeObject safeToUnlock)
        {
            var go     = CreateQuad(name, pos, scale, color);
            go.AddComponent<BoxCollider2D>();

            var puzzle = go.AddComponent<ColorPuzzle>();
            var pso    = new SerializedObject(puzzle);
            pso.FindProperty("puzzleId").stringValue = puzzleId;
            var seqProp = pso.FindProperty("colorSequence");
            seqProp.arraySize = colorSeq.Length;
            for (int i = 0; i < colorSeq.Length; i++)
                seqProp.GetArrayElementAtIndex(i).stringValue = colorSeq[i];
            pso.ApplyModifiedPropertiesWithoutUndo();

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
            var cso       = new SerializedObject(container);
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
            var go   = CreateQuad(name, pos, new Vector3(1f, 1.5f, 1), color);
            go.AddComponent<BoxCollider2D>();
            var sl   = go.AddComponent<StoryLogObject>();
            var sso  = new SerializedObject(sl);
            sso.FindProperty("objectId").stringValue          = name.ToLower();
            sso.FindProperty("logData").objectReferenceValue  = logData;
            sso.ApplyModifiedPropertiesWithoutUndo();
            AddObjectFrame(go, C_Amber, 0.06f);
            AddWorldLabel(go, label, new Vector3(0, 1f, 0));
        }

        // ================================================================
        //  HUD バー
        // ================================================================
        private static void BuildHUDBar(Transform parent)
        {
            // 下部バー（縁取り付き）
            var (barOuter, barInner) = CreateStyledPanel(parent, "HUDBar",
                Vector2.zero, new Vector2(0, 90),
                new Color(0.04f, 0.06f, 0.12f, 0.97f), C_BorderCyan, 1.5f);
            var barRt = barOuter.GetComponent<RectTransform>();
            barRt.anchorMin        = new Vector2(0, 0);
            barRt.anchorMax        = new Vector2(1, 0);
            barRt.pivot            = new Vector2(0.5f, 0);
            barRt.anchoredPosition = new Vector2(0, 0);
            barRt.sizeDelta        = new Vector2(0, 90);

            // インベントリバー
            var invBarGo = new GameObject("InventoryBar", typeof(RectTransform));
            invBarGo.transform.SetParent(barInner.transform, false);
            var invRt = (RectTransform)invBarGo.transform;
            var hlg   = invBarGo.AddComponent<HorizontalLayoutGroup>();
            invRt.anchorMin  = new Vector2(0, 0);
            invRt.anchorMax  = new Vector2(1, 1);
            invRt.offsetMin  = new Vector2(10, 5);
            invRt.offsetMax  = new Vector2(-130, -5);
            hlg.spacing      = 6;
            hlg.childForceExpandHeight = true;
            hlg.childControlHeight     = false;
            hlg.childControlWidth      = false;

            // メニューボタン
            var menuBtn = CreateStyledButton(barInner.transform, "MenuButton", "MENU",
                Vector2.zero, new Vector2(100, 44), C_BtnNorm, C_BorderCyan);
            GetBtnLabel(menuBtn).fontSize = 16;
            GetBtnLabel(menuBtn).color    = C_CyanDim;
            var menuRt = menuBtn.gameObject.GetComponent<RectTransform>();
            menuRt.anchorMin        = new Vector2(1, 0.5f);
            menuRt.anchorMax        = new Vector2(1, 0.5f);
            menuRt.pivot            = new Vector2(1, 0.5f);
            menuRt.anchoredPosition = new Vector2(-12, 0);

            // ItemSlot Prefab
            var slotPrefab = CreateItemSlotPrefab();
            var hudGo      = new GameObject("HUDController");
            hudGo.transform.SetParent(barInner.transform, false);
            var hud   = hudGo.AddComponent<HUDController>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("inventoryBar").objectReferenceValue   = invBarGo.GetComponent<RectTransform>();
            hudSo.FindProperty("itemSlotPrefab").objectReferenceValue = slotPrefab;
            hudSo.ApplyModifiedPropertiesWithoutUndo();
            menuBtn.onClick.AddListener(hud.OnMenuButtonClicked);
        }

        // ================================================================
        //  ExaminePanel
        // ================================================================
        private static ExaminePanel BuildExaminePanel(Transform parent)
        {
            var (panelOuter, panelInner) = CreateStyledPanel(parent, "ExaminePanel",
                Vector2.zero, new Vector2(860, 120),
                C_BgPanelDark, C_BorderCyan, 1.5f);
            var prt = panelOuter.GetComponent<RectTransform>();
            prt.anchorMin        = new Vector2(0.5f, 0);
            prt.anchorMax        = new Vector2(0.5f, 0);
            prt.pivot            = new Vector2(0.5f, 0);
            prt.anchoredPosition = new Vector2(0, 98);

            var txtGo = new GameObject("MessageText");
            txtGo.transform.SetParent(panelInner.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text        = "";
            txt.fontSize    = 20;
            txt.color       = C_TextPri;
            txt.alignment   = TextAnchor.MiddleCenter;
            txt.lineSpacing = 1.3f;
            AddTextShadow(txt);
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin  = Vector2.zero;
            trt.anchorMax  = Vector2.one;
            trt.offsetMin  = new Vector2(20, 8);
            trt.offsetMax  = new Vector2(-20, -8);

            panelOuter.SetActive(false);

            var ep   = panelOuter.transform.parent.gameObject.AddComponent<ExaminePanel>();
            var so   = new SerializedObject(ep);
            so.FindProperty("panel").objectReferenceValue       = panelOuter;
            so.FindProperty("messageText").objectReferenceValue = txt;
            so.ApplyModifiedPropertiesWithoutUndo();
            return ep;
        }

        // ================================================================
        //  PuzzleUIPanel（数字キーパッド）
        // ================================================================
        private static void BuildPuzzleUIPanel(Transform parent)
        {
            // オーバーレイ
            var overlay = CreatePanel(parent, "PuzzleOverlay", Vector2.zero, Vector2.zero,
                new Color(0, 0, 0, 0.75f));
            SetStretch(overlay);

            // パネル本体（縁取り付き）
            var (panelOuter, panelInner) = CreateStyledPanel(overlay.transform, "PuzzlePanel",
                Vector2.zero, new Vector2(360, 480), C_BgPanel, C_BorderCyan, 2f);
            SetAnchorCenter(panelOuter);

            // タイトル
            var titleTxt = CreateText(panelInner.transform, "TitleText", "■  暗号ロック",
                new Vector2(0, 188), new Vector2(330, 48), C_Cyan, 22, TextAnchor.MiddleCenter, bold: true);
            AddTextShadow(titleTxt, C_CyanGlow);

            // タイトル下ライン
            CreatePanel(panelInner.transform, "TitleLine",
                new Vector2(0, 162), new Vector2(300, 1), C_CyanDim);

            // 表示テキスト
            var dispGo  = new GameObject("DisplayText");
            dispGo.transform.SetParent(panelInner.transform, false);
            var dispTxt = dispGo.AddComponent<Text>();
            dispTxt.text       = "_ _ _ _";
            dispTxt.fontSize   = 42;
            dispTxt.color      = new Color(0.3f, 0.95f, 0.5f);
            dispTxt.alignment  = TextAnchor.MiddleCenter;
            dispTxt.fontStyle  = FontStyle.Bold;
            AddTextShadow(dispTxt, new Color(0.1f, 0.5f, 0.2f, 0.6f));
            var drt = dispGo.GetComponent<RectTransform>();
            drt.anchoredPosition = new Vector2(0, 120);
            drt.sizeDelta        = new Vector2(310, 60);

            // キーパッドグリッド
            var keypadGo = new GameObject("Keypad");
            keypadGo.transform.SetParent(panelInner.transform, false);
            var grid = keypadGo.AddComponent<GridLayoutGroup>();
            var krt  = (RectTransform)keypadGo.transform;
            krt.anchoredPosition  = new Vector2(0, -14);
            krt.sizeDelta         = new Vector2(308, 270);
            grid.cellSize         = new Vector2(92, 64);
            grid.spacing          = new Vector2(8, 8);
            grid.constraint       = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount  = 3;

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
                string lbl = d == -1 ? "DEL" : d == -2 ? "CLR" : d.ToString();
                Color  bgc = d >= 0 ? C_BtnNorm : C_BtnDanger;
                Color  bdc = d >= 0 ? C_BorderCyan : C_BorderRed;
                var btn = CreateStyledButton(keypadGo.transform, $"Btn_{lbl}", lbl,
                    Vector2.zero, new Vector2(92, 64), bgc, bdc);
                GetBtnLabel(btn).color    = d >= 0 ? C_TextPri : new Color(1f, 0.6f, 0.6f);
                GetBtnLabel(btn).fontSize = 22;
                var digit = d;
                if (digit >= 0)      btn.onClick.AddListener(() => pui.OnDigitButtonClicked(digit));
                else if (digit == -1) btn.onClick.AddListener(pui.OnDeleteButtonClicked);
                else                  btn.onClick.AddListener(pui.OnClearButtonClicked);
            }

            var closeBtn = CreateStyledButton(panelInner.transform, "CloseButton", "✕  CLOSE",
                new Vector2(0, -200), new Vector2(180, 42), C_BtnDanger, C_BorderRed);
            GetBtnLabel(closeBtn).color    = new Color(1f, 0.6f, 0.6f);
            GetBtnLabel(closeBtn).fontSize = 17;
            closeBtn.onClick.AddListener(pui.OnCloseButtonClicked);
        }

        // ================================================================
        //  SequencePuzzleUIPanel（4ボタン）
        // ================================================================
        private static void BuildSequencePuzzleUIPanel(Transform parent)
        {
            var overlay = CreatePanel(parent, "SeqPuzzleOverlay", Vector2.zero, Vector2.zero,
                new Color(0, 0, 0, 0.75f));
            SetStretch(overlay);

            var (panelOuter, panelInner) = CreateStyledPanel(overlay.transform, "SeqPuzzlePanel",
                Vector2.zero, new Vector2(400, 400), C_BgPanel, C_BorderAmber, 2f);
            SetAnchorCenter(panelOuter);

            var titleTxt = CreateText(panelInner.transform, "TitleText", "⚡  配電盤",
                new Vector2(0, 160), new Vector2(360, 46), C_Amber, 22, TextAnchor.MiddleCenter, bold: true);
            AddTextShadow(titleTxt, C_AmberGlow);
            CreatePanel(panelInner.transform, "TitleLine",
                new Vector2(0, 136), new Vector2(320, 1), new Color(C_Amber.r, C_Amber.g, C_Amber.b, 0.5f));

            var statusGo  = new GameObject("StatusText");
            statusGo.transform.SetParent(panelInner.transform, false);
            var statusTxt = statusGo.AddComponent<Text>();
            statusTxt.text        = "ボタンを正しい順序で押せ";
            statusTxt.fontSize    = 17;
            statusTxt.color       = C_TextSec;
            statusTxt.alignment   = TextAnchor.MiddleCenter;
            statusTxt.lineSpacing = 1.2f;
            AddTextShadow(statusTxt);
            var srt = statusGo.GetComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(0, 96);
            srt.sizeDelta        = new Vector2(360, 44);

            // 2×2 ボタンレイアウト
            var keypadGo = new GameObject("ButtonGrid");
            keypadGo.transform.SetParent(panelInner.transform, false);
            var grid = keypadGo.AddComponent<GridLayoutGroup>();
            var krt  = (RectTransform)keypadGo.transform;
            krt.anchoredPosition = new Vector2(0, -24);
            krt.sizeDelta        = new Vector2(300, 200);
            grid.cellSize        = new Vector2(138, 88);
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
                var btn = CreateStyledButton(keypadGo.transform, $"Btn_{i}", i.ToString(),
                    Vector2.zero, new Vector2(138, 88), C_BtnNorm, C_BorderAmber);
                GetBtnLabel(btn).color    = C_Amber;
                GetBtnLabel(btn).fontSize = 30;
                var btnId = i;
                btn.onClick.AddListener(() => sui.OnButtonClicked(btnId));
            }

            var closeBtn = CreateStyledButton(panelInner.transform, "CloseButton", "✕  CLOSE",
                new Vector2(0, -165), new Vector2(180, 42), C_BtnDanger, C_BorderRed);
            GetBtnLabel(closeBtn).color    = new Color(1f, 0.6f, 0.6f);
            GetBtnLabel(closeBtn).fontSize = 17;
            closeBtn.onClick.AddListener(sui.OnCloseButtonClicked);
        }

        // ================================================================
        //  ColorPuzzleUIPanel（3色ボタン）
        // ================================================================
        private static void BuildColorPuzzleUIPanel(Transform parent)
        {
            var overlay = CreatePanel(parent, "ColorPuzzleOverlay", Vector2.zero, Vector2.zero,
                new Color(0, 0, 0, 0.75f));
            SetStretch(overlay);

            var (panelOuter, panelInner) = CreateStyledPanel(overlay.transform, "ColorPuzzlePanel",
                Vector2.zero, new Vector2(440, 370), C_BgPanel, C_BorderCyan, 2f);
            SetAnchorCenter(panelOuter);

            var titleTxt = CreateText(panelInner.transform, "TitleText", "◈  色パネル",
                new Vector2(0, 142), new Vector2(400, 46), C_Cyan, 22, TextAnchor.MiddleCenter, bold: true);
            AddTextShadow(titleTxt, C_CyanGlow);
            CreatePanel(panelInner.transform, "TitleLine",
                new Vector2(0, 118), new Vector2(360, 1), C_CyanDim);

            var statusGo  = new GameObject("StatusText");
            statusGo.transform.SetParent(panelInner.transform, false);
            var statusTxt = statusGo.AddComponent<Text>();
            statusTxt.text        = "色を正しい順序で選べ";
            statusTxt.fontSize    = 17;
            statusTxt.color       = C_TextSec;
            statusTxt.alignment   = TextAnchor.MiddleCenter;
            AddTextShadow(statusTxt);
            var srt = statusGo.GetComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(0, 80);
            srt.sizeDelta        = new Vector2(400, 40);

            overlay.SetActive(false);

            var cuiGo = new GameObject("ColorPuzzleUIPanel");
            cuiGo.transform.SetParent(parent, false);
            var cui = cuiGo.AddComponent<ColorPuzzleUIPanel>();
            var cso = new SerializedObject(cui);
            cso.FindProperty("panel").objectReferenceValue      = overlay;
            cso.FindProperty("statusText").objectReferenceValue = statusTxt;
            cso.ApplyModifiedPropertiesWithoutUndo();

            // 色ボタン（青・赤・黄）
            var colorDefs = new (string id, string label, Color fill, Color border)[]
            {
                ("Blue",   "青", new Color(0.06f, 0.20f, 0.60f), new Color(0.30f, 0.55f, 1.00f)),
                ("Red",    "赤", new Color(0.55f, 0.07f, 0.07f), new Color(1.00f, 0.35f, 0.35f)),
                ("Yellow", "黄", new Color(0.45f, 0.38f, 0.00f), new Color(1.00f, 0.85f, 0.10f)),
            };

            float btnX = -135f;
            foreach (var (id, lbl, fill, border) in colorDefs)
            {
                var btn = CreateStyledButton(panelInner.transform, $"Btn_{id}", lbl,
                    new Vector2(btnX, 14), new Vector2(118, 90), fill, border);
                GetBtnLabel(btn).color    = border;
                GetBtnLabel(btn).fontSize = 26;
                AddTextShadow(GetBtnLabel(btn), new Color(border.r, border.g, border.b, 0.5f));
                var colorId = id;
                btn.onClick.AddListener(() => cui.OnColorClicked(colorId));
                btnX += 135f;
            }

            var closeBtn = CreateStyledButton(panelInner.transform, "CloseButton", "✕  CLOSE",
                new Vector2(0, -146), new Vector2(180, 42), C_BtnDanger, C_BorderRed);
            GetBtnLabel(closeBtn).color    = new Color(1f, 0.6f, 0.6f);
            GetBtnLabel(closeBtn).fontSize = 17;
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

        /// <summary>縁取り付きパネル（outer=枠, inner=塗り）</summary>
        private static (GameObject outer, GameObject inner) CreateStyledPanel(
            Transform parent, string name, Vector2 pos, Vector2 size,
            Color fillColor, Color borderColor, float border = 1.5f)
        {
            var outer = CreatePanel(parent, name, pos, size, borderColor);
            var inner = CreatePanel(outer.transform, name + "_Fill", Vector2.zero, Vector2.zero, fillColor);
            var irt   = inner.GetComponent<RectTransform>();
            irt.anchorMin  = Vector2.zero;
            irt.anchorMax  = Vector2.one;
            irt.offsetMin  = new Vector2(border, border);
            irt.offsetMax  = new Vector2(-border, -border);
            return (outer, inner);
        }

        /// <summary>縁取り付きボタン</summary>
        private static Button CreateStyledButton(Transform parent, string name, string label,
            Vector2 pos, Vector2 size, Color fillColor, Color borderColor)
        {
            var (outer, inner) = CreateStyledPanel(parent, name, pos, size, fillColor, borderColor, 1.5f);

            var btn = outer.AddComponent<Button>();
            var img = outer.GetComponent<Image>();
            // ColorBlock 設定
            var cb = btn.colors;
            cb.normalColor      = Color.white;
            cb.highlightedColor = new Color(1.30f, 1.35f, 1.50f);
            cb.pressedColor     = new Color(0.65f, 0.70f, 0.80f);
            cb.selectedColor    = new Color(1.10f, 1.15f, 1.25f);
            cb.disabledColor    = new Color(0.50f, 0.50f, 0.50f, 0.4f);
            cb.colorMultiplier  = 1f;
            cb.fadeDuration     = 0.08f;
            btn.colors          = cb;
            btn.targetGraphic   = img;

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(inner.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text      = label;
            txt.fontSize  = 20;
            txt.color     = C_TextPri;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontStyle = FontStyle.Bold;
            AddTextShadow(txt);
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin        = Vector2.zero;
            trt.anchorMax        = Vector2.one;
            trt.sizeDelta        = Vector2.zero;
            trt.anchoredPosition = Vector2.zero;

            return btn;
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

        private static GameObject CreateQuad(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name             = name;
            go.transform.position    = pos;
            go.transform.localScale  = scale;
            Object.DestroyImmediate(go.GetComponent<MeshCollider>());
            var mr  = go.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color        = color;
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
            txt.fontSize  = 22;
            txt.color     = C_TextPri;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontStyle = FontStyle.Bold;
            AddTextShadow(txt, new Color(0f, 0.5f, 0.7f, 0.5f));
            var rt = txtGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 60);
        }

        // ---- テキスト効果 ----
        private static void AddTextShadow(Text txt, Color? color = null)
        {
            var s = txt.gameObject.AddComponent<Shadow>();
            s.effectColor    = color ?? new Color(0f, 0.3f, 0.5f, 0.55f);
            s.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private static void AddTextOutline(Text txt, Color? color = null)
        {
            var o = txt.gameObject.AddComponent<Outline>();
            o.effectColor    = color ?? new Color(0f, 0.4f, 0.6f, 0.6f);
            o.effectDistance = new Vector2(1f, -1f);
        }

        // ---- オブジェクトフレーム（アクセントカラーの薄い縁） ----
        private static void AddObjectFrame(GameObject obj, Color frameColor, float margin)
        {
            var frame = CreateQuad(obj.name + "_Frame",
                new Vector3(obj.transform.position.x, obj.transform.position.y,
                            obj.transform.position.z + 0.05f),
                new Vector3(obj.transform.localScale.x + margin * 2,
                            obj.transform.localScale.y + margin * 2, 1),
                new Color(frameColor.r, frameColor.g, frameColor.b, 0.65f));
            frame.transform.SetParent(obj.transform.parent, true);
            frame.transform.SetSiblingIndex(obj.transform.GetSiblingIndex());
        }

        // ---- ボタンのラベル Text を返す ----
        private static Text GetBtnLabel(Button btn)
            => btn.GetComponentInChildren<Text>();

        // ---- RectTransform アンカーユーティリティ ----
        private static void SetStretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.sizeDelta        = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static void SetAnchorCenter(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
        }

        private static void SetAnchorStretchH(GameObject go, float anchorY, float pivotY, float offset)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0, anchorY);
            rt.anchorMax        = new Vector2(1, anchorY);
            rt.pivot            = new Vector2(0.5f, pivotY);
            rt.anchoredPosition = new Vector2(0, offset);
            rt.sizeDelta        = new Vector2(0, rt.sizeDelta.y);
        }
    }
}
