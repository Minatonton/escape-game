# 技術設計ドキュメント (TDD)

**プロジェクト名:** *(仮称) Escape Game*
**バージョン:** 0.1
**最終更新:** 2026-02-21
**エンジン:** Unity 6 (LTS)
**言語:** C#

---

## 目次

1. [プロジェクト構成](#1-プロジェクト構成)
2. [コアシステム設計](#2-コアシステム設計)
3. [シーン設計](#3-シーン設計)
4. [データ設計](#4-データ設計)
5. [セーブ・ロードシステム](#5-セーブロードシステム)
6. [ローカライズ実装](#6-ローカライズ実装)
7. [Steam 連携](#7-steam-連携)
8. [パフォーマンス指針](#8-パフォーマンス指針)
9. [コーディング規約](#9-コーディング規約)

---

## 1. プロジェクト構成

### Unity プロジェクトフォルダ構成

```
Assets/
├── _Project/                  # プロジェクト固有アセット（命名: _Project でトップに固定）
│   ├── Scripts/
│   │   ├── Core/              # ゲームマネージャー、状態管理
│   │   ├── Room/              # 部屋システム
│   │   ├── Puzzle/            # パズルロジック
│   │   ├── Inventory/         # インベントリシステム
│   │   ├── UI/                # UIコントローラー
│   │   ├── Audio/             # オーディオマネージャー
│   │   ├── Save/              # セーブシステム
│   │   └── Localization/      # ローカライズ
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Chapter1/
│   │   │   ├── R001_WakeRoom.unity
│   │   │   ├── R002_Corridor.unity
│   │   │   └── R003_LabA.unity
│   │   └── ...
│   ├── Prefabs/
│   │   ├── Interactive/       # インタラクト可能オブジェクト
│   │   ├── UI/                # UI部品
│   │   └── Puzzles/           # パズル部品
│   ├── Sprites/
│   │   ├── Backgrounds/
│   │   ├── Objects/
│   │   └── UI/
│   ├── Audio/
│   │   ├── BGM/
│   │   ├── SE/
│   │   └── VO/
│   ├── Data/                  # ScriptableObject データ
│   │   ├── Items/
│   │   ├── Rooms/
│   │   └── Puzzles/
│   └── Localization/          # CSV or JSON 翻訳ファイル
│
├── Plugins/
│   └── Steamworks.NET/        # Steam SDK
│
└── StreamingAssets/
    └── Localization/          # 実行時読み込みテキスト
```

---

## 2. コアシステム設計

### 2.1 GameManager（シングルトン）

ゲーム全体の状態を管理する中心クラス。

```csharp
// Scripts/Core/GameManager.cs
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public RoomManager RoomManager { get; private set; }
    public InventoryManager InventoryManager { get; private set; }
    public SaveManager SaveManager { get; private set; }
    public AudioManager AudioManager { get; private set; }

    public event Action<GameState> OnStateChanged;

    public void ChangeState(GameState newState) { ... }
}

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Cutscene,
    GameOver
}
```

### 2.2 RoomManager

部屋のロード・アンロード、部屋間の遷移を管理。

```csharp
// Scripts/Room/RoomManager.cs
public class RoomManager : MonoBehaviour
{
    [SerializeField] private RoomDatabase roomDatabase;

    public RoomData CurrentRoom { get; private set; }

    // シーン非同期ロードで部屋を切り替える
    public async UniTask TransitionToRoom(string roomId)
    {
        await FadeOut();
        await SceneManager.LoadSceneAsync(roomId, LoadSceneMode.Single);
        CurrentRoom = roomDatabase.GetRoom(roomId);
        await FadeIn();
    }
}
```

### 2.3 InteractableObject（基底クラス）

すべてのインタラクト可能オブジェクトが継承する基底クラス。

```csharp
// Scripts/Room/InteractableObject.cs
public abstract class InteractableObject : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string objectId;
    [SerializeField] private LocalizedString displayName;
    [SerializeField] private bool isEnabled = true;

    public string ObjectId => objectId;

    // オーバーライドして各オブジェクトの固有動作を実装
    protected abstract void OnInteract();
    protected abstract void OnItemUsed(ItemData item);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled) return;

        var selectedItem = InventoryManager.Instance.SelectedItem;
        if (selectedItem != null)
            OnItemUsed(selectedItem);
        else
            OnInteract();
    }
}
```

**具体的なサブクラス例:**

| クラス名 | 用途 |
|---------|------|
| `ItemPickup` | アイテムを取得する |
| `ExaminableObject` | テキスト説明を表示する |
| `ZoomableObject` | ズームビューを開く |
| `DoorObject` | 別部屋へ遷移する |
| `PuzzleContainer` | パズルを起動する |

### 2.4 PuzzleSystem

パズルの状態管理と検証を担当。

```csharp
// Scripts/Puzzle/PuzzleBase.cs
public abstract class PuzzleBase : MonoBehaviour
{
    [SerializeField] private string puzzleId;
    [SerializeField] private UnityEvent onSolved;

    public bool IsSolved { get; private set; }

    // サブクラスで解答検証ロジックを実装
    protected abstract bool ValidateSolution();

    protected void TrySolve()
    {
        if (ValidateSolution())
        {
            IsSolved = true;
            GameManager.Instance.SaveManager.SetPuzzleSolved(puzzleId);
            onSolved?.Invoke();
        }
        else
        {
            OnWrongAnswer();
        }
    }

    protected virtual void OnWrongAnswer()
    {
        // デフォルト: 振動アニメーション + SE
    }
}
```

**パズルサブクラス:**

| クラス名 | 概要 |
|---------|------|
| `NumericCodePuzzle` | 数字入力（4〜6桁） |
| `SequencePuzzle` | 順番を選ぶ |
| `ColorPuzzle` | 色の配列 |
| `WiringPuzzle` | 端子を繋ぐ |
| `SlidePuzzle` | スライドパズル |
| `SymbolPuzzle` | 記号の並び |

### 2.5 InventoryManager

```csharp
// Scripts/Inventory/InventoryManager.cs
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int maxSlots = 16;

    private List<ItemData> items = new();
    public ItemData SelectedItem { get; private set; }

    public event Action<ItemData> OnItemAdded;
    public event Action<ItemData> OnItemRemoved;
    public event Action<ItemData> OnItemSelected;

    public bool AddItem(ItemData item) { ... }
    public bool RemoveItem(string itemId) { ... }
    public void SelectItem(ItemData item) { ... }
    public ItemData TryCombine(ItemData a, ItemData b) { ... }
}
```

### 2.6 AudioManager

BGM のクロスフェード、SE の再生を管理。

```csharp
// Scripts/Audio/AudioManager.cs
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource[] sePool;

    public void PlayBGM(AudioClip clip, float fadeDuration = 1.0f) { ... }
    public void PlaySE(string seId) { ... }
    public void SetBGMVolume(float volume) { ... }
    public void SetSEVolume(float volume) { ... }
}
```

---

## 3. シーン設計

### シーン一覧

| シーン名 | 役割 |
|---------|------|
| `Bootstrap` | 起動時の初期化処理（GameManager等の生成）、その後MainMenuへ遷移 |
| `MainMenu` | タイトル画面 |
| `R001_WakeRoom` ～ | 各ゲームプレイ部屋 |
| `Ending_True` / `_Normal` / `_Bad` | エンディングシーン |

### シーン間遷移

- **Additive ロード不使用** — 各部屋は Single モードでロード（メモリ節約）
- **DontDestroyOnLoad** — GameManager, AudioManager, UICanvas を永続化
- **非同期ロード** — `UnityEngine.SceneManagement.SceneManager.LoadSceneAsync` を使用し、ロード中はローディング画面を表示

---

## 4. データ設計

### ScriptableObject によるデータ定義

#### ItemData

```csharp
// Scripts/Inventory/ItemData.cs
[CreateAssetMenu(menuName = "EscapeGame/Item")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public LocalizedString displayName;
    public LocalizedString description;
    public Sprite icon;
    public ItemCategory category;
    public bool isConsumable;

    // 合成レシピ（このアイテムが結果として生成される場合）
    public ItemData combineIngredientA;
    public ItemData combineIngredientB;
}
```

#### RoomData

```csharp
[CreateAssetMenu(menuName = "EscapeGame/Room")]
public class RoomData : ScriptableObject
{
    public string roomId;
    public LocalizedString roomName;
    public string sceneAssetPath;
    public AudioClip bgmClip;
    public string[] requiredPuzzleIds;  // この部屋をクリアするために解く必要があるパズル
    public string nextRoomId;
}
```

#### PuzzleData

```csharp
[CreateAssetMenu(menuName = "EscapeGame/Puzzle")]
public class PuzzleData : ScriptableObject
{
    public string puzzleId;
    public PuzzleType type;
    public string[] answers;            // 正解リスト（複数正解対応）
    public HintData[] hints;            // 段階的ヒント
}

[System.Serializable]
public class HintData
{
    public int level;                   // 1=ぼんやり, 2=方向性, 3=具体的
    public LocalizedString hintText;
}
```

---

## 5. セーブ・ロードシステム

### セーブデータ構造

```csharp
// Scripts/Save/SaveData.cs
[System.Serializable]
public class SaveData
{
    public string saveId;
    public DateTime savedAt;
    public string currentRoomId;
    public string currentChapter;
    public List<string> collectedItemIds;
    public List<string> solvedPuzzleIds;
    public List<string> viewedLogIds;       // ストーリーログ閲覧記録
    public Dictionary<string, bool> flags;  // 汎用フラグ（分岐管理用）
    public float playtime;                  // プレイ時間（秒）
    public int hintUsedCount;
}
```

### セーブ方式

- **保存形式:** JSON（`JsonUtility` または `Newtonsoft.Json`）
- **保存先:** `Application.persistentDataPath/saves/save{slot}.json`
- **スロット数:** 3スロット + オートセーブ1スロット
- **暗号化:** XOR 簡易難読化（チート防止）

```csharp
// Scripts/Save/SaveManager.cs
public class SaveManager : MonoBehaviour
{
    private const int SlotCount = 3;
    private const string AutoSaveSlot = "autosave";

    public void Save(int slot) { ... }
    public SaveData Load(int slot) { ... }
    public void AutoSave() { ... }

    // ゲームフラグ操作
    public void SetFlag(string key, bool value) { ... }
    public bool GetFlag(string key) { ... }
    public void SetPuzzleSolved(string puzzleId) { ... }
    public bool IsPuzzleSolved(string puzzleId) { ... }
}
```

---

## 6. ローカライズ実装

**Unity Localization Package** を使用する。

```
Package: com.unity.localization
```

### 実装方針

- `LocalizedString` を ScriptableObject データと UI テキストに使用
- String Table を言語ごとに管理
- 実行時言語切り替え対応

### テキストキー命名規則

```
{カテゴリ}_{ID}_{種別}
例:
  item_rusty_key_name      → 「錆びた鍵」
  item_rusty_key_desc      → 「古い錆びた鍵。何かに使えるかもしれない。」
  puzzle_r001_hint1        → ヒント1テキスト
  room_r001_examine_shelf  → 本棚を調査したときのテキスト
```

---

## 7. Steam 連携

### 使用ライブラリ

**Steamworks.NET** (`https://steamworks.github.io/`)

### 実装する機能

#### 実績 (Achievements)

```csharp
// Scripts/Steam/SteamManager.cs
public class SteamManager : MonoBehaviour
{
    public void UnlockAchievement(string achievementId)
    {
        if (!SteamManager.Initialized) return;
        SteamUserStats.SetAchievement(achievementId);
        SteamUserStats.StoreStats();
    }
}
```

**実績リスト（例）:**

| ID | 名前 | 条件 |
|----|------|------|
| `ACH_FIRST_ESCAPE` | 最初の一歩 | 最初の部屋を脱出 |
| `ACH_NO_HINTS` | 天才探偵 | ヒントを使わずクリア |
| `ACH_TRUE_END` | 真相究明 | True Endを見る |
| `ACH_ALL_LOGS` | 記録者 | 全音声ログを収集 |
| `ACH_SPEED_RUN` | 脱出の達人 | 2時間以内にクリア |

#### クラウドセーブ

```csharp
// Steam Cloud を有効化（Steamworks設定で有効化 + 実装）
SteamRemoteStorage.FileWrite("save0.json", data, data.Length);
```

#### スクリーンショット

```csharp
SteamScreenshots.TriggerScreenshot(); // F12 のデフォルト動作
```

### Steamworks 設定項目

| 設定 | 値 |
|------|-----|
| App ID | *(Steamworks で取得)* |
| ストア対応言語 | 日本語, 英語 |
| Cloud Save クォータ | 1 MB |
| 実績数 | 10〜15個 |

---

## 8. パフォーマンス指針

### ターゲットスペック

| 項目 | 最低スペック | 推奨スペック |
|------|------------|------------|
| OS | Windows 10 (64bit) | Windows 10/11 |
| CPU | Core i3 相当 | Core i5 相当 |
| RAM | 4 GB | 8 GB |
| GPU | Intel HD Graphics | GeForce GTX 960 相当 |
| ストレージ | 2 GB | 2 GB |
| 解像度 | 1280×720 | 1920×1080 |

### 最適化方針

- **スプライトアトラス:** 部屋ごとにスプライトアトラスをまとめる（Draw Call 削減）
- **BGM:** MP3/OGG、SEは WAV
- **テクスチャ:** 最大 2048×2048、不要な Mipmap は無効化
- **非同期ロード:** 部屋のシーンは非同期ロード（フリーズ防止）
- **オブジェクトプール:** SE再生用AudioSourceはプーリング

---

## 9. コーディング規約

### 命名規則

| 種別 | 規則 | 例 |
|------|------|-----|
| クラス名 | PascalCase | `InventoryManager` |
| メソッド名 | PascalCase | `AddItem()` |
| プロパティ | PascalCase | `CurrentRoom` |
| プライベートフィールド | _camelCase | `_currentState` |
| シリアライズフィールド | camelCase | `maxSlots` |
| 定数 | UPPER_SNAKE | `MAX_SLOT_COUNT` |
| インターフェース | I + PascalCase | `IInteractable` |

### アーキテクチャ方針

- **依存性注入:** マネージャーはシングルトン + `GameManager.Instance.XXX` で参照
- **イベント駆動:** UI 更新は `C# event` / `UnityEvent` で疎結合を保つ
- **ScriptableObject 活用:** データとロジックを分離

### フォルダ・ファイル規約

- 1クラス 1ファイル
- 名前空間: `EscapeGame.{SubSystem}` (例: `EscapeGame.Inventory`)
- テストクラスは `Tests/` フォルダに配置し、`_Tests.cs` サフィックス

---

*このドキュメントは実装開始前に確定させ、変更時はチームで合意の上更新すること。*
