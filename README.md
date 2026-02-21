# Escape Game *(仮称)*

2D ポイント&クリック 謎解き脱出ゲーム — Steam リリース予定

**エンジン:** Unity 6 (C#)
**プラットフォーム:** Steam (Windows / macOS)

---

## ゲームを起動する

### SceneBuilder 実行（初回 / 更新後）

新しいシーン・アセットを生成するには Unity バッチモードで SceneBuilder を実行します。

```bash
# Unity インストール先は環境に合わせて変更してください
/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath /Users/yamaguchiminato/apps/toshin/escape-game \
  -executeMethod EscapeGame.Editor.SceneBuilder.BuildAll \
  -quit
```

生成されるファイル:
- `Assets/_Project/Scenes/Chapter1/R001_WakeRoom.unity` (更新)
- `Assets/_Project/Scenes/Chapter1/R002_Corridor.unity`
- `Assets/_Project/Scenes/Chapter1/R003_LabA.unity`
- `Assets/_Project/Scenes/Ending_True.unity`
- `Assets/_Project/Scenes/Ending_Normal.unity`
- `Assets/_Project/Data/Items/item_*.asset` × 7
- `Assets/_Project/Data/Logs/storylog_*.asset` × 3

### Unity Editor で実行

1. **Unity Hub** を開く
2. 「Open」をクリック → `/Users/yamaguchiminato/apps/toshin/escape-game` を選択
3. Unity Editor が起動したら **Scenes/MainMenu.unity** を開く
   （`Assets/_Project/Scenes/MainMenu.unity`）
4. **Play ボタン（▶）** を押してゲーム開始
5. 「New Game」を押すと `R001_WakeRoom` に移行

---

## ゲームフロー（3部屋 プロトタイプ）

```
MainMenu
  ↓ New Game
R001_WakeRoom（目覚めの部屋）
  本棚のメモ「1234」を確認
  暗号ロック → 4桁コード入力 → IDカード入手
  引き出し → 錆びた鍵 / 薬棚 → 潤滑油
  インベントリで 錆びた鍵＋潤滑油 を選んで合成 → 使える鍵
  ドアに「使える鍵」を使用 → R002 へ
  ↓
R002_Corridor（廊下）
  回路図でヒント「3→1→2→4」を確認
  配電盤のボタンを 3→1→2→4 の順に押す → ドア解錠
  ↓
R003_LabA（研究室A）
  色見本「青→赤→黄」を確認
  色パネルを 青→赤→黄 の順に選択 → セーフ開錠 → 最終の鍵入手
  出口ドアに「最終の鍵」を使用 → エンディングへ
  ↓
Ending_True（全ストーリーログ収集済）
  または
Ending_Normal（ログ未収集）
```

### 操作方法

| 操作 | 内容 |
|------|------|
| オブジェクトをクリック | 調査 / アイテム入手 |
| インベントリのアイテムをクリック | 選択（ハイライト） |
| 選択中にアイテムを再クリック | 選択解除 |
| 選択中に別アイテムをクリック | アイテム合成を試みる |
| 選択中にオブジェクトをクリック | そのオブジェクトにアイテムを使用 |

### ストーリーログの場所

| ログ | 場所 |
|------|------|
| 研究日誌 #1 | R001 本棚の隣（日誌アイコン） |
| 博士の手紙 | R002 廊下の奥 |
| 最終記録 | R003 研究室の棚 |

3つ全て収集すると **Ending_True** へ。

---

## ドキュメント

| ファイル | 内容 |
|---------|------|
| [docs/GDD.md](docs/GDD.md) | ゲームデザインドキュメント |
| [docs/TECHNICAL.md](docs/TECHNICAL.md) | 技術設計ドキュメント |
| [docs/STEAM_RELEASE.md](docs/STEAM_RELEASE.md) | Steam リリース計画 |
| [docs/ROADMAP.md](docs/ROADMAP.md) | 開発ロードマップ |

---

## フォルダ構成

```
Assets/
└── _Project/
    ├── Scripts/
    │   ├── Core/          # GameManager, SceneLoader, IInteractable
    │   ├── Data/          # ItemData, PuzzleData, RoomData, StoryLogData
    │   ├── Room/          # InteractableObject, ExaminableObject, DoorObject,
    │   │                  #   PuzzleContainer, StoryLogObject, SafeObject
    │   ├── Puzzle/        # PuzzleBase, NumericCodePuzzle, SequencePuzzle, ColorPuzzle
    │   ├── Inventory/     # InventoryManager, ItemPickup
    │   ├── UI/            # HUDController, ExaminePanel, PuzzleUIPanel,
    │   │                  #   SequencePuzzleUIPanel, ColorPuzzleUIPanel,
    │   │                  #   MainMenuUI, EndingUI
    │   ├── Audio/         # AudioManager
    │   └── Save/          # SaveManager, SaveData
    ├── Scenes/
    │   ├── MainMenu.unity
    │   ├── Ending_True.unity
    │   ├── Ending_Normal.unity
    │   └── Chapter1/
    │       ├── R001_WakeRoom.unity
    │       ├── R002_Corridor.unity
    │       └── R003_LabA.unity
    ├── Data/
    │   ├── Items/         # item_*.asset × 7
    │   ├── Logs/          # storylog_*.asset × 3
    │   └── Puzzles/       # puzzle_r001_code.asset
    └── Prefabs/
        └── UI/
            └── ItemSlot.prefab
```

---

## 開発フェーズ

- [x] Phase 0: 準備・設計（ドキュメント完成）
- [x] Phase 1: プロトタイプ（R001 プレイアブル）
- [x] Phase 1.5: 拡張プロトタイプ（3部屋・合成・ストーリー・エンディング分岐）
- [ ] Phase 2: アルファ版
- [ ] Phase 3: ベータ版
- [ ] Phase 4: リリース準備
