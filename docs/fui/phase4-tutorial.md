# Phase 4 スタイル・テーマ・カラー チュートリアル

> 関連: 設計仕様書 [`phase4-styles-themes.md`](./phase4-styles-themes.md)
> 前提: [`phase1-tutorial.md`](./phase1-tutorial.md) を読んでパーツ機構の基本操作を理解していること。

このドキュメントは Phase 4 で追加された **スタイル・テーマ・カラー** の操作ガイドです。
組込テーマでパレットを一括適用し、線種ライブラリから破線・段階線を選び、図形にグローを乗せて FUI らしい世界観を統一するまでを解説します。

---

## 1. Phase 4 で追加された要素

| 機能 | UI 表記 | 内部型 / 経路 | 用途 |
|---|---|---|---|
| テーマ選択 / パレット適用 | メニュー「キャンバス → テーマ選択 / パレット適用...」 | `ThemeManager` ダイアログ | 組込 4 テーマでパレット色 / 線種 / グローを一括適用 |
| カラーパレット | 「パレット」 (ダイアログ内) | `ColorPalette` (5 セマンティックスロット) | 役割名 (primary/accent/warning/info/background) で色を管理 |
| 線種プリセット | 「線種」 (ダイアログ内) | `LineStyle` × 6 (Solid/Dash/Dot/DashDot/LongDash/Stepped) | StrokeDashArray + StrokeLineJoin + 任意グロー |
| グロー設定 | 「グロー設定を適用」 (チェックボックス) | `GlowRadius` / `GlowIntensity` / `GlowColor` | WPF `DropShadowEffect` 経由の擬似グロー |

既存の `EdgeBrush` / `FillBrush` / `StrokeDashArray` を直接書き換える方式 (Q-3 案 A) なので、テーマ適用後も既存ファイル形式とは互換のまま。

---

## 2. テーマ選択ダイアログを開く

1. メニューバー **「キャンバス」→「テーマ選択 / パレット適用...」** を選ぶ。
   - もしくは `MenuItem_ThemeManager` (AutomationId) を vs-mcp UIA / accessibility ツール経由で叩く。
2. ダイアログが開いたら以下を選択:
   - **テーマ**: 組込 4 種 (Bladerunner / Matrix / MedicalBlueWhite / AmberCrt) から選ぶ。
   - **適用範囲**: `SelectedItems` (選択中図形のみ) / `ActiveLayer` (アクティブレイヤー全体) / `EntireProject` (プロジェクト全体)。
   - **適用対象**: `EdgeOnly` (線色のみ) / `FillOnly` (塗り色のみ) / `Both` (両方、デフォルト)。
   - **線種**: 未選択なら StrokeDashArray は変更しない。
   - **「グロー設定を適用」**: チェックすると、テーマの `DefaultGlow` から GlowRadius/Intensity/Color を流し込む。
3. 「適用」ボタンで対象範囲の図形が即時書き換わる。`Ctrl+Z` で `BindableReactiveProperty` 単位で取り消し可能。

---

## 3. カラーパレット適用 (Q-2 案 A、Q-3 案 A)

### 3.1 セマンティックスロット (固定 5 種)

| キー | 役割 | Bladerunner | Matrix | MedicalBlueWhite | AmberCrt |
|---|---|---|---|---|---|
| `primary` | 主役色 | `#FF5733` 暖色赤 | `#00FF41` 蛍光緑 | `#3FE0FF` 蛍光青 | `#FFB000` 琥珀 |
| `accent` | アクセント | `#FFB94B` 暖色黄 | `#33B85A` 中緑 | `#FFFFFF` 白 | `#FFD568` 淡琥珀 |
| `warning` | 警告 | `#9E2A1B` 暗赤 | `#0F5A1F` 暗緑 | `#B0C8D5` 灰青 | `#7A5500` 暗琥珀 |
| `info` | 情報 / 影 | `#2C1810` 暗黒 | `#001500` 黒緑 | `#0A1A30` 暗青 | `#1F1500` 黒褐 |
| `background` | 背景 | `#0A0303` 黒 | `#000000` 黒 | `#001020` 黒青 | `#0A0700` 暗黒 |

既定マッピングは「**Edge = primary / Fill = background**」(`ThemeApplier.ResolveBrushes`)。`ThemeApplyTarget.EdgeOnly` で塗りを保持したまま線色だけ反転、`FillOnly` で逆、`Both` で両方書き換え。

### 3.2 適用範囲の決め方 (Q-10 案 C)
- **`SelectedItems`**: 現在選択している図形のみ。部分的にテーマを変えたいとき。
- **`ActiveLayer`**: アクティブレイヤー (`SelectedLayers.FirstOrDefault()`) 配下のすべての `SelectableDesignerItemViewModelBase` 派生。
- **`EntireProject`**: 全レイヤーの全図形。Bladerunner → Matrix にプロジェクト全体を切り替えるユースケース向け。

---

## 4. 線種ライブラリ (Q-5 案 B)

`LineStyle` 型でテーマ単位にプリセット群を持ちます。組込は 6 種:

| Name | StrokeDashArray | 用途 |
|---|---|---|
| `Solid` | (空) | ベタ線 |
| `Dash` | `4, 2` | 一般的破線 |
| `Dot` | `1, 2` | 点線 |
| `DashDot` | `4, 2, 1, 2` | 鎖線 |
| `LongDash` | `8, 4` | 長破線 |
| `Stepped` | `8, 4, 2, 4` | 段階線 (FUI 頻出、Q-6 案 A) |

ThemeManager ダイアログの「線種」コンボボックスで選んで「適用」を押すと、対象範囲の図形の `StrokeDashArray` / `StrokeLineJoin` が書き換わります。
DoubleCollection は `ThemeApplier.CopyDashArray()` で独立コピーするので、テーマ側プリセットの参照は共有されません。後で個別図形だけダッシュパターンを変えても他に影響しません。

未選択 (コンボボックス未操作) なら StrokeDashArray は変更しません。

---

## 5. グロー (擬似 DropShadow ベース、Q-7 案 A の MVP)

### 5.1 図形側プロパティ (Q-9 案 A、非破壊)

`SelectableDesignerItemViewModelBase` に 3 つの `BindableReactiveProperty`:

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `GlowRadius` | `double` | `0` | ぼかし半径 (px)。0 でグロー無効 |
| `GlowIntensity` | `double` | `0.5` | 加算合成の強度 (0..1)、`DropShadowEffect.Opacity` にマップ |
| `GlowColor` | `Color?` | `null` | グロー色。null なら `EdgeBrush` と同色 (SolidColorBrush 以外は白フォールバック) |

派生プロパティ `GlowEffect` (`IReadOnlyBindableReactiveProperty<Effect>`) が `GlowRadius/Intensity/Color/EdgeBrush` の `CombineLatest` で `DropShadowEffect` を作って各 DataTemplate にバインドします (`BlurRadius = Radius * 2`、`ShadowDepth = 0`、`Direction = 0`)。

### 5.2 テーマからの一括適用

ThemeManager ダイアログで「**グロー設定を適用**」をチェックすると、テーマの `DefaultGlow` (Radius/Intensity/Color) を対象範囲の全図形に流し込みます。例えば Bladerunner なら `Radius=6 / Intensity=0.6 / Color=#FF5733` が全部に乗ります。

### 5.3 視覚化されている DataTemplate (Phase 4-e-2)
- `NRectangleViewModel` / `NEllipseViewModel` / `NPolygonViewModel` / `PathDesignerItemViewModel`
- `OrthogonalConnectorViewModel` / `AnchorBezierConnectorViewModel`

テキスト系 (`LetterDesignerItemViewModel` / `MonoTextBlock` / `DataGeneratorTextBlock` / `NumberSequenceBlock` / `TextMatrixBlock` / `TextOnPathBlock`) / 効果系 (`BlurViewModel` / `MosaicViewModel`) / `NPieViewModel` / `PictureDesignerItemViewModel` の DataTemplate には Effect を未配線。データ層には `GlowRadius` 等を保持できますが、画面では描画されない点に注意。

### 5.4 個別図形のグロー設定
ThemeManager 経由ではなく、ExposedProperty 経由 (パーツインスタンス) や直接 VM 操作で個別図形の `GlowRadius.Value = 5` のように値を設定できます。詳細は §7 参照。

---

## 6. テーマ切替の操作例

### 6.1 プロジェクト全体を Bladerunner に染める
1. メニュー → キャンバス → 「テーマ選択 / パレット適用...」
2. テーマ: `Bladerunner`、適用範囲: `EntireProject`、適用対象: `Both`、線種: `Solid`、グロー: チェック
3. 「適用」→ プロジェクト全体が暖色赤系 + 暗黒背景 + グロー付きに

### 6.2 選択中の数枠だけ Matrix 化
1. 緑にしたい図形を Shift+ クリックで複数選択
2. 「テーマ選択 / パレット適用...」 → テーマ: `Matrix`、範囲: `SelectedItems`、対象: `EdgeOnly`、線種: `Dash`
3. 「適用」 → 選択した枠だけが蛍光緑の破線に。塗りは元のまま

### 6.3 グローを外す
ThemeManager の「グロー設定を適用」をチェックしないで適用すれば、既存のグローには触れません。
グローを完全に消したい個別図形は `GlowRadius.Value = 0` をセット (PropertyPanel or 別ダイアログ経由)。

---

## 7. Phase 1 パーツ機構との統合 (Q-11 案 A)

`GlowRadius` / `GlowIntensity` / `GlowColor` は `PartEditor` の「Phase 4 公開可能プロパティ (グロー)」セクションでピン留め可能:

| プロパティ | ExposedPropertyType |
|---|---|
| `GlowRadius` | `Double` |
| `GlowIntensity` | `Double` |
| `GlowColor` | `Color` |

これでパーツインスタンスごとに「グロー色」「グロー強度」をパラメータ化したパーツが組めます。例:
- **「光る目盛り」パーツ**: 目盛り図形の `GlowColor` を公開 → インスタンス配置時にテーマ色を差し替え
- **「呼吸する HUD リング」パーツ**: `GlowRadius` を公開 → 配置ごとに強弱を変えられる

ColorPalette / LineStyle 参照そのものの公開 (テーマ追従パーツ) は Phase 4-g-2 として後送り中です。

---

## 8. シリアライズ (Phase 4-f)

図形の `GlowRadius > 0` 時のみ、`<DesignerItem>` 配下に `<GlowRadius>` / `<GlowIntensity>` / `<GlowColor>` (#AARRGGBB 形式、null は要素ごと省略) を出力します:

```xml
<DesignerItem ...>
  ...
  <GlowRadius>6</GlowRadius>
  <GlowIntensity>0.6</GlowIntensity>
  <GlowColor>#FFFF5733</GlowColor>
</DesignerItem>
```

Phase 4 以前の古いプロジェクトファイルは要素が存在しないため、デフォルト (0 / 0.5 / null) で復元されます。完全後方互換。

`<Themes>` セクション (ユーザー追加テーマ + ActiveThemeId) のシリアライズは Phase 4-f-2 として後送り中です。組込 4 テーマは起動時に毎回コードハードコードからロードされるので、現状でも「次回起動時にアクティブテーマだけ忘れる」程度。図形の色は EdgeBrush/FillBrush に直接書き込まれているので、見た目は完全に維持されます。

---

## 9. 既知の制約 (Phase 4.5 / 後送り候補)

- **DataTemplate 視覚化の未対応分**: テキスト系・効果系・Pie・Picture では `GlowRadius > 0` でも描画されない (§5.3)。データ層には保持される。
- **OpenCV 版 `GlowEffectViewModel`** (仕様書 Q-7 案 A の本来の実装): WPF DropShadowEffect の擬似グローで MVP 済。ラスター品質が必要な場合 Phase 4.5 で実装予定。
- **テーマシリアライズ** (`<Themes>` セクション): Phase 4-f-2 後送り。組込テーマの Id を固定化 → ActiveThemeId + ユーザー追加テーマを保存。
- **ColorPaletteRef / LineStyleRef のパーツ公開**: Phase 4-g-2 後送り。テーマシリアライズ後に着手予定。
- **バインドモード (Q-3 案 B)**: テーマ変更で図形が動的追従するモード。MVP では直接書換のみ。
- **テーマ切替アニメーション** (Bladerunner → Matrix のトランジション): Phase 5 以降。

---

## 10. 関連ドキュメント

- 設計仕様: [`phase4-styles-themes.md`](./phase4-styles-themes.md) (Q-1 〜 Q-12 確定版)
- Phase 1 (パーツ機構): [`phase1-tutorial.md`](./phase1-tutorial.md)
- Phase 2 / 2.5 (テキスト): [`phase2-tutorial.md`](./phase2-tutorial.md) / [`phase2-5-tutorial.md`](./phase2-5-tutorial.md)
- Phase 3 (接続線): [`phase3-tutorial.md`](./phase3-tutorial.md)
- FUI 進化計画全体: [`../FUI_DESIGNER_INTENT.md`](../FUI_DESIGNER_INTENT.md)
