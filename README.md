# Escape Game *(仮称)*

2D ポイント&クリック 謎解き脱出ゲーム — Steam リリース予定

**エンジン:** Unity 6 (C#)
**プラットフォーム:** Steam (Windows / macOS)

---

## ゲームを起動する

1. **Unity Hub** を開く
2. 「Open」をクリック → `/Users/yamaguchiminato/apps/toshin/escape-game` を選択
3. Unity Editor が起動したら **Scenes/MainMenu.unity** を開く
   （`Assets/_Project/Scenes/MainMenu.unity`）
4. **Play ボタン（▶）** を押してゲーム開始
5. 「New Game」を押すと `R001_WakeRoom` (目覚めの部屋) に移行

### 目覚めの部屋 (R001) の遊び方

| オブジェクト | クリックで起こること |
|-------------|-------------------|
| 本棚（茶色） | 「1234」と書かれたメモを発見 |
| 暗号ロック（灰色） | 4桁コード入力パネルが開く |
| ドア（青） | 解錠前はロック中メッセージ |

1. 本棚をクリック → 「1234」のヒントを確認
2. 暗号ロックをクリック → キーパッドが開く → `1`→`2`→`3`→`4` と入力
3. ドアのロックが解除される → ドアをクリックで脱出成功！

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
    │   ├── Data/          # ItemData, PuzzleData, RoomData (ScriptableObject)
    │   ├── Room/          # InteractableObject, ExaminableObject, DoorObject, RoomManager, PuzzleContainer
    │   ├── Puzzle/        # PuzzleBase, NumericCodePuzzle
    │   ├── Inventory/     # InventoryManager, ItemPickup
    │   ├── UI/            # HUDController, ExaminePanel, PuzzleUIPanel, MainMenuUI
    │   ├── Audio/         # AudioManager
    │   └── Save/          # SaveManager, SaveData
    ├── Scenes/
    │   ├── MainMenu.unity
    │   └── Chapter1/
    │       └── R001_WakeRoom.unity   ← プレイアブルプロトタイプ
    ├── Data/
    │   └── Puzzles/
    │       └── puzzle_r001_code.asset
    └── Prefabs/
        └── UI/
            └── ItemSlot.prefab
```

---

## 開発フェーズ

- [x] Phase 0: 準備・設計（ドキュメント完成）
- [x] Phase 1: プロトタイプ（R001 プレイアブル）
- [ ] Phase 2: アルファ版
- [ ] Phase 3: ベータ版
- [ ] Phase 4: リリース準備
