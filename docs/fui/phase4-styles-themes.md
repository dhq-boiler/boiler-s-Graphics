# Phase 4-a: スタイル・テーマ・カラー 設計仕様書 (確定版 v1.0)

> このドキュメントは `docs/FUI_DESIGNER_INTENT.md` Phase 4 「スタイル・テーマ・カラー」に対応する設計仕様書 (確定版 v1.0) である。
> §10 のオープン問題は Q-1 〜 Q-12 すべて確定済み (2026-05-14、作者判断)。
> §12 が Phase 4-b 実装時のクイックリファレンス。

---

## 1. 概要

### 1.1 目的
FUI は **「世界観 (世界の色 / 線 / 質感)」が統一されている** ことで初めて成立する。Phase 4 では、個別の図形ごとに色や線種を逐一設定する手間を減らし、**プロジェクト全体で一貫したビジュアルテーマ** を簡単に組めるようにする土台を整える。

### 1.2 この機能が満たすべき体験
1. **カラーパレット管理** — 「Bladerunner 系」「Matrix 系」「医療系青白」「アンバー CRT」のような **テーマ単位のカラーパレット** を保存・読み込み・適用できる。
2. **線種ライブラリ** — 破線・点線・段階線・グロー風線のプリセットを **ライブラリから選ぶだけ** で図形に適用できる (StrokeDashArray を手で覚えなくて良い)。
3. **グロー / ブルーム風エフェクト** — 線や図形に「光って滲む」雰囲気を **簡易な擬似グロー** で乗せられる (FUI らしい光沢の表現)。
4. これら全てを **非破壊** で適用できる。元の図形 (EdgeBrush / FillBrush / StrokeDashArray の生値) は保持され、グローはオーバーレイ的に乗る。
5. これらを **Phase 1 のパーツ機構** で公開可能パラメータとして利用できる (例: 「ノード色」をパレット選択で切り替えられるパーツ)。

### 1.3 非スコープ (Phase 4 では扱わない)
- **シェーダ実装の物理ベース光学処理** (実際の HDR ブルーム、リニア色空間補正) は Phase 5 以降。Phase 4 は **擬似** グロー (OpenCV ガウシアン + 加算合成) で十分。
- **動的テーマ切替アニメーション** (Bladerunner → Matrix にトランジション再生) は Phase 5 以降。Phase 4 は **静的適用 (即時切替)** のみ。
- **任意ピクセルシェーダのインポート機構** (.fx ファイル外部読み込み) は Phase 7 以降。
- **テーマ別フォント切替** (Bladerunner 用フォント → Matrix 用フォント) はテーマシステムから外す (Phase 2 のフォント設定で個別管理)。

### 1.4 Phase 4 のスコープに含むもの (Q-12 案 A 反映)
- **`ColorPalette` モデル** — 順序色 + セマンティックスロット
- **`Theme` モデル** — パレット + 線種プリセット + デフォルトグロー設定を束ねる単位
- **`LineStyle` モデル** — StrokeDashArray + StrokeLineJoin + 簡易グロー設定の束 (Q-5 案 B 反映、完全新規型)
- **`GlowEffectViewModel`** — 既存 `BlurEffectViewModel` 派生、OpenCV ガウシアン + 加算合成 (Q-7 案 A 反映)
- 組み込みプリセットテーマ 4 種 (Bladerunner / Matrix / 医療系青白 / アンバー CRT、Q-4 案 A 反映)
- 上記すべての **シリアライズ** + **Phase 1 パーツ機構との統合** + **チュートリアル**

---

## 2. 用語定義

| 用語 | 意味 | UI 表記 |
|---|---|---|
| **テーマ (Theme)** | パレット + 線種プリセット + デフォルトグロー設定を束ねる単位 | 「テーマ」 |
| **カラーパレット (ColorPalette)** | 順序色 (N 色) + セマンティックスロット (primary/accent/warning/info/background) | 「パレット」 |
| **セマンティックスロット (Slot)** | パレット内の役割名付きエントリ (primary 等) | 「役割」 |
| **線種 (LineStyle)** | StrokeDashArray + StrokeLineJoin + 簡易グロー の束 | 「線種」 |
| **段階線 (Stepped Line)** | 等間隔で太さ / ダッシュが変化する線 (FUI 頻出) | 「段階線」 |
| **擬似グロー (Faux Glow)** | OpenCV ガウシアン + 加算合成での簡易光沢表現 | 「グロー」 |

---

## 3. 各機能の設計案

### 3.1 カラーパレット管理 (Q-1 案 C / Q-2 案 A / Q-3 案 A / Q-4 案 A / Q-10 案 C 反映)

#### 3.1.1 データモデル
```csharp
public class ColorPalette
{
    public string Name { get; set; }                           // "Bladerunner" 等
    public ObservableCollection<Color> Colors { get; set; }    // 順序色 N 色 (任意数 ≥ 5)
    public Dictionary<string, int> SemanticSlots { get; set; } // "primary" -> 0, "accent" -> 1, ...
    public Guid Id { get; set; }
    public bool IsBuiltIn { get; set; }                        // 組み込み or ユーザー追加
}
```

#### 3.1.2 セマンティックスロット (Q-2 案 A 反映、固定 5 種)
- 固定 5 種: `primary` / `accent` / `warning` / `info` / `background`
- セマンティックは「役割」を指す。各テーマで具体色は異なる (Bladerunner の primary = 暖色赤、Matrix の primary = 蛍光緑)。
- 順序色は **5 色以上** 必須。6 個目以降はスロット未割当でも可。

#### 3.1.3 パレット適用方法 (Q-3 案 A 反映、直接書き換え)
- クリックで対象図形の `EdgeBrush` / `FillBrush` を直接書き換え (非バインド)。
- 操作は TsOperationHistory で Undo 可能。
- バインドモードは **Phase 4.5 後送り** (シンプルさを優先)。

#### 3.1.4 組み込みプリセット (Q-4 案 A 反映、4 種)
| テーマ | primary | accent | warning | info | background |
|---|---|---|---|---|---|
| **Bladerunner** | `#FF5733` (暖色赤) | `#FFB94B` (暖色黄) | `#9E2A1B` (暗赤) | `#2C1810` (暗黒) | `#0A0303` (黒) |
| **Matrix** | `#00FF41` (蛍光緑) | `#33B85A` (中緑) | `#0F5A1F` (暗緑) | `#001500` (黒緑) | `#000000` (黒) |
| **医療系青白** | `#3FE0FF` (蛍光青) | `#FFFFFF` (白) | `#B0C8D5` (灰青) | `#0A1A30` (暗青) | `#001020` (黒青) |
| **アンバー CRT** | `#FFB000` (琥珀) | `#FFD568` (淡琥珀) | `#7A5500` (暗琥珀) | `#1F1500` (黒褐) | `#0A0700` (暗黒) |

#### 3.1.5 適用 UI
- 図形選択時、プロパティパネル上部に「パレット適用」セクションを追加。
- セマンティック適用: 「primary」「accent」「warning」「info」「background」ボタン (`SetEdge` / `SetFill` 切替トグル)。
- 順序色適用: 色サンプル N 個並べ、クリックで適用。
- 適用範囲 (Q-10 案 C 反映): 「選択中図形 / レイヤー全体 / プロジェクト全体」ラジオで選択。

### 3.2 線種ライブラリ (Q-5 案 B / Q-6 案 A 反映)

#### 3.2.1 データモデル (Q-5 案 B 反映、完全新規 LineStyle 型)
```csharp
public class LineStyle
{
    public string Name { get; set; }                  // "Solid" / "Dash" / "Dot" / "DashDot" / "LongDash" / "Stepped"
    public DoubleCollection StrokeDashArray { get; set; }
    public PenLineJoin StrokeLineJoin { get; set; }
    public double? GlowRadius { get; set; }           // null なら未適用
    public double? GlowIntensity { get; set; }
    public Guid Id { get; set; }
    public bool IsBuiltIn { get; set; }
}
```

#### 3.2.2 組み込みプリセット (Q-6 案 A 反映、StrokeDashArray パターン)
| Name | StrokeDashArray | 用途 |
|---|---|---|
| `Solid` | (空) | ベタ線 |
| `Dash` | `4, 2` | 一般的破線 |
| `Dot` | `1, 2` | 点線 |
| `DashDot` | `4, 2, 1, 2` | 鎖線 |
| `LongDash` | `8, 4` | 長破線 |
| `Stepped` | `8, 4, 2, 4` | 段階線 (FUI 頻出) |

#### 3.2.3 段階線の実装 (Q-6 案 A 反映)
- 完全新規 ViewModel は作らず、`StrokeDashArray` パターンで表現。
- 太さ可変の段階線は Phase 4.5 後送り。

#### 3.2.4 適用 UI
- プロパティパネルに「線種プリセット」コンボボックス + プレビュー。
- 選択中の図形に即時適用。
- カスタム線種登録: 現在の StrokeDashArray を「現在の設定を線種として保存」ボタンで追加。

### 3.3 グロー / ブルーム風エフェクト (Q-7 案 A / Q-8 案 C / Q-9 案 A 反映)

#### 3.3.1 グローを乗せる方式 (Q-9 案 A 反映、元プロパティ保持)
- `SelectableDesignerItemViewModelBase` に **3 つのプロパティを追加**:
  - `BindableReactiveProperty<double> GlowRadius` (デフォルト 0、0 でグロー無効)
  - `BindableReactiveProperty<double> GlowIntensity` (デフォルト 0.5、0..1)
  - `BindableReactiveProperty<Color?> GlowColor` (デフォルト null、null なら EdgeBrush と同色)
- 元の `EdgeBrush` / `FillBrush` は **そのまま保持**。グローは描画時に追加レイヤーとして合成。
- Effect レイヤー方式 (Q-9 案 B) は採用しない (テーマ切替時に Effect レイヤーを総入れ替えする手間を避けるため)。

#### 3.3.2 実装方式 (Q-7 案 A 反映、BlurEffectViewModel 派生)
- `GlowEffectViewModel : BlurEffectViewModel` を新規実装。
  - OpenCV `GaussianBlur` で元図形をぼかし、`Cv2.Add` (加算合成) で重ねる。
  - KernelWidth / KernelHeight は `Radius * 2 + 1` で奇数化。
  - Intensity は加算合成時の係数で重み付け (例: `dest = src + glow * intensity`)。
- 既存 `EffectViewModel` パイプライン (`Render` / `OnRectChanged` / `Clone`) を再利用。

#### 3.3.3 適用範囲 (Q-8 案 C 反映、線 + 図形 + テキスト)
- すべての `SelectableDesignerItemViewModelBase` 派生に適用可能。
  - Connector / Path / NPolygon / NEllipse 等の線系
  - NRectangle / Picture / Mosaic 等の図形系
  - LetterDesignerItem / LetterVerticalDesignerItem / MonoTextBlock / DataGeneratorTextBlock 等のテキスト系

### 3.4 テーマ切替の適用範囲 (Q-10 案 C 反映)
- メニューバー「テーマ」プルダウン → テーマ選択ダイアログ:
  - **適用範囲ラジオ**: 「選択中図形のみ / アクティブレイヤー全体 / プロジェクト全体」
  - 「適用」ボタン: 選択範囲に対して新テーマのセマンティックスロット色を一括反映 (元の役割マッピングを保ったまま色のみ書換)。
  - 「キャンセル」: 元の状態に戻る。
- 適用は TsOperationHistory の 1 操作にまとめる (Undo で一括ロールバック)。

---

## 4. データモデル

```
Theme
  ├ Name : string                               ("Bladerunner")
  ├ Id : Guid
  ├ IsBuiltIn : bool
  ├ Palette : ColorPalette
  │   ├ Colors : ObservableCollection<Color>
  │   └ SemanticSlots : Dictionary<string, int>
  ├ LineStyles : ObservableCollection<LineStyle>
  └ DefaultGlow : GlowSettings
      ├ Radius : double
      ├ Intensity : double
      └ Color : Color?

GlowSettings : POCO で Theme 内部のみ使用 (SelectableDesignerItemViewModelBase は個別プロパティ保持)
```

- **アクティブテーマ** は `DiagramViewModel.ActiveTheme` で 1 つだけ保持。
- **組み込みテーマ** (Bladerunner / Matrix / 医療系青白 / アンバー CRT) はコードハードコードで `ThemeRepository.BuiltIn` から取得。
- **ユーザー追加テーマ** はプロジェクトファイルに保存 (Q-1 案 C)。

---

## 5. UI / UX

### 5.1 テーマ選択 UI
- メニューバー「ツール」→「テーマ」サブメニュー: 「テーマ選択…」「テーマ編集…」「現在のテーマをユーザーテーマに保存…」
- 「テーマ選択」ダイアログ: コンボボックス (組込 + ユーザー追加) + 適用範囲ラジオ + 「適用」「キャンセル」。

### 5.2 パレット適用 UI (プロパティパネル右側)
- 「パレット」セクション: パレット選択コンボ + Edge/Fill 切替トグル + セマンティックボタン × 5 + 順序色サンプル。

### 5.3 線種ライブラリ UI (プロパティパネル右側)
- 「線種」セクション: 線種プリセットコンボ + プレビュー + 「現在の設定を線種として保存」ボタン。

### 5.4 グロー UI (プロパティパネル右側)
- 「グロー」セクション: Radius (TextBox) + Intensity (Slider 0..1) + GlowColor (ColorPicker、null チェックボックス付き)。
- Radius=0 でグロー無効 (描画パイプラインは GlowRadius>0 のみグロー処理)。

---

## 6. シリアライズ仕様

### 6.1 プロジェクトファイルへの拡張

既存ルート直下 (現状: `<Diagram>` の直下に `<DesignerItems>` / `<Connectors>` / `<Layers>` 等が並ぶ) に **`<Themes>` セクションを追加**:

```xml
<Themes>
  <ActiveThemeId>00000000-...-...</ActiveThemeId>
  <Theme Id="..." Name="MyCustomTheme" IsBuiltIn="false">
    <Palette>
      <Colors>
        <Color>#FF6633</Color>
        <Color>#FFB94B</Color>
        ...
      </Colors>
      <SemanticSlots>
        <Slot Name="primary" Index="0" />
        <Slot Name="accent" Index="1" />
        <Slot Name="warning" Index="2" />
        <Slot Name="info" Index="3" />
        <Slot Name="background" Index="4" />
      </SemanticSlots>
    </Palette>
    <LineStyles>
      <LineStyle Id="..." Name="CustomDash" Dash="6,3" Join="Round" GlowRadius="2" GlowIntensity="0.4" />
    </LineStyles>
    <DefaultGlow Radius="4" Intensity="0.6" Color="#3FE0FF" />
  </Theme>
</Themes>
```

- 組み込みテーマ (Bladerunner 等) は **保存しない**。`ActiveThemeId` が組込のものを指していれば、ロード時にコードから補完。
- ユーザー追加テーマのみ保存。

### 6.2 図形のグロー設定

各 `SelectableDesignerItemViewModelBase` 派生のシリアライズに `<GlowRadius>` / `<GlowIntensity>` / `<GlowColor>` を追加 (デフォルト値時は省略):

```xml
<DesignerItem ...>
  ...
  <GlowRadius>4</GlowRadius>
  <GlowIntensity>0.6</GlowIntensity>
  <GlowColor>#FFB000</GlowColor>
</DesignerItem>
```

### 6.3 後方互換
- 旧プロジェクト (Themes セクションなし) を開いた場合:
  - `<Themes>` がなければ ActiveTheme = null (組込デフォルト適用なし)。
  - `<GlowRadius>` 等がなければデフォルト 0 (グロー無効)。
- 既存テストへの影響を最小化 (新規セクション省略時は無視)。

---

## 7. 既存コード位置 (Phase 4-b 以降の参照)

| 機能 | 参照先 |
|---|---|
| Brush プロパティ基盤 | `boilersGraphics/ViewModels/SelectableDesignerItemViewModelBase.cs` (`EdgeBrush` / `FillBrush`、95-96 行) |
| StrokeDashArray 基盤 | 同上 (`StrokeDashArray` プロパティ、107 行) |
| ColorPicker (Solid) | `boilersGraphics/Views/SolidColorPicker.xaml(.cs)` / `ViewModels/SolidColorPickerViewModel.cs` |
| グラデーション Brush | `boilersGraphics/Views/LinearGradientBrushPicker.xaml` / `RadialGradientBrushPicker.xaml` |
| 既存 Effect 基底 | `boilersGraphics/ViewModels/EffectViewModel.cs` |
| 既存 BlurEffect 実装 | `boilersGraphics/ViewModels/BlurViewModel.cs` (OpenCV GaussianBlur、`Render` メソッド) |
| 既存 MosaicEffect 実装 | `boilersGraphics/ViewModels/MosaicViewModel.cs` |
| ShaderEffect | `boilersGraphics/Controls/Effects/BlurEffect.cs` / `MosaicEffect.cs` |
| シリアライズ拡張点 | `boilersGraphics/Helpers/ObjectSerializer.cs` / `ObjectDeserializer.cs` (`<Themes>` 追加、`<GlowRadius>` 等を designerItem 共通ブロックに追加) |
| Diagram レイヤー | `boilersGraphics/ViewModels/DiagramViewModel.cs` (`ActiveTheme` プロパティを追加) |
| Phase 1 パーツ統合 | `boilersGraphics/ViewModels/Parts/PartEditorViewModel.cs` (`_exposureFlags` に Glow 系を追加、Phase 3-h と同様の流儀) |
| Undo 機構 | TsOperationHistory ベース (既存 `RemoveItemCommand` 等と同じ) |

---

## 8. テスト方針

| 項目 | テスト方針 |
|---|---|
| ColorPalette モデル | 順序色追加 / 削除 / スロット割当 / IsBuiltIn フラグの単体テスト |
| Theme.BuiltIn 4 種 | Bladerunner / Matrix / 医療系青白 / アンバー CRT が定義どおりに読める / セマンティック 5 スロット全部埋まる |
| LineStyle | 既定 6 種 (Solid / Dash / Dot / DashDot / LongDash / Stepped) の値が正しい / 適用で StrokeDashArray が書き換わる |
| GlowEffect.Render | KernelWidth/Height が奇数化される / Width≤0 で early return / Bitmap が WriteableBitmap で返る |
| テーマ適用 (適用範囲) | 選択中図形のみ / レイヤー全体 / プロジェクト全体で書き換え範囲が正しい / Undo で一括ロールバック |
| シリアライズ | `<Themes>` セクションの RoundTrip (空 / 1 テーマ / 複数テーマ) |
| 図形のグロー設定 RoundTrip | GlowRadius/Intensity/Color の保存・復元 |
| 後方互換 | 旧プロジェクト (Themes / Glow なし) を開いてもクラッシュしない / グロー無効状態で読み込む |
| Phase 1 パーツ統合 | _exposureFlags に Glow 系が追加されている / TogglePropertyExposure で no-op ガードが機能する |

---

## 9. Phase 4 のサブフェーズ分割 (Q-12 案 A 反映、全機能含む)

### Phase 4 (今リリース対象)
- **4-a**: 設計仕様書 (本ドキュメント) を確定 — Q-1 〜 Q-12 全件確定で **完了**
- **4-b**: `ColorPalette` / `Theme` モデル + 組み込みプリセット 4 種 + `DiagramViewModel.ActiveTheme`
- **4-c**: パレット適用 UI (プロパティパネルセクション、セマンティック + 順序色)
- **4-d**: `LineStyle` モデル + 組み込み 6 種 + 適用 UI (プロパティパネル)
- **4-e**: `GlowEffectViewModel` 実装 (BlurEffect 派生、OpenCV ガウシアン + 加算) + 図形側プロパティ追加 + グロー UI
- **4-f**: シリアライズ対応 (`<Themes>` セクション + `<GlowRadius>` 等)
- **4-g**: Phase 1 パーツ機構との統合 (Glow 系プロパティ + LineStyleRef を ExposedProperty 公開可能化)
- **4-h**: チュートリアル整備 (`docs/fui/phase4-tutorial.md` + HowToUse.md + INTENT.md 更新)

### Phase 4.5 (後送り、機能拡張のみ)
- **4.5-a**: パレットのバインドモード (Q-3 案 B 相当、パレット変更で全図形が追従)
- **4.5-b**: テーマ切替アニメーション
- **4.5-c**: 太さ可変の段階線 (Q-6 案 B 相当、`SteppedStrokeViewModel` 完全新規)
- **4.5-d**: ShaderEffect 版 Glow (.fx) / 任意ピクセルシェーダのインポート
- **4.5-e**: テーマ別フォント切替

---

## 10. オープン問題 (要決定事項)

すべての設計判断は確定済み (2026-05-14、作者判断)。

### Q-1. カラーパレットの保持スコープ ✅ **確定**
- **採用: 案 C (組み込み + ユーザー両方)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: 組み込みは更新時に増える可能性があるのでアプリ側 (コードハードコード)、ユーザー追加はプロジェクトファイルに保存。配布パッケージとプロジェクト個別の両軸が成立。
- **不採用**: 案 A (プロジェクト内のみ) — アプリ更新で組込パレットを増やせない / 案 B (アプリ全体のみ) — プロジェクト固有のテーマが共有できない

### Q-2. パレット内の色名 (セマンティックスロット) のキー ✅ **確定**
- **採用: 案 A (固定 5 種: `primary` / `accent` / `warning` / `info` / `background`)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: FUI らしい役割語が明確、テーマ切替時にスロット越しで色が動く。順序のみ (案 B) では「役割を保ったままテーマ切替」ができない。
- **不採用**: 案 B (順序のみ) / 案 C (自由命名) — 切替時のマッピングが破綻

### Q-3. パレット適用方法 ✅ **確定**
- **採用: 案 A (直接書き換え、非バインド)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: シンプル、Phase 4 のスコープに収まる。Undo は TsOperationHistory で確保。バインドモードは Phase 4.5 後送り。
- **不採用**: 案 B (バインド) — 動的追従の設計負荷が高い / 案 C (両対応) — UI が複雑化

### Q-4. 組み込みプリセットテーマの種類数 ✅ **確定**
- **採用: 案 A (4 種: Bladerunner / Matrix / 医療系青白 / アンバー CRT)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: INTENT.md で明示されている 4 種を組み込み。プラスアルファはユーザー定義で対応。
- **不採用**: 案 B (6 種) — Tokyo Tower / Mono CRT 緑は需要が読みづらい / 案 C (空) — 初手の UX が悪い

### Q-5. 線種ライブラリの実装方法 ✅ **確定**
- **採用: 案 B (完全新規 `LineStyle` 型で抽象化)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: StrokeDashArray + StrokeLineJoin + GlowSettings をまとめて 1 つの線種として扱える。テーマと同じ流儀でユーザー定義線種が登録できる。
- **不採用**: 案 A (StrokeDashArray プリセットのみ) — Glow と一体管理ができない / 案 C (A + B) — モデル重複

### Q-6. 段階線の実装 ✅ **確定**
- **採用: 案 A (StrokeDashArray パターンで表現)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: 既存基盤のみで実装、Phase 4 のスコープに収まる。太さ可変段階線は Phase 4.5 で `SteppedStrokeViewModel` 完全新規で対応 (後送り)。
- **不採用**: 案 B (完全新規 SteppedStrokeViewModel) — Phase 4 スコープが膨らむ

### Q-7. グロー風線の実装方式 ✅ **確定**
- **採用: 案 A (`BlurEffectViewModel` 派生、OpenCV GaussianBlur + 加算合成)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: 既存パターン (BlurEffect / MosaicEffect) と一貫、エクスポート (PDF / PNG) でもラスター品質が安定。
- **不採用**: 案 B (DropShadowEffect 流用) — 加算合成にならない・ベクター品質依存 / 案 C (新規 ShaderEffect) — Phase 4 スコープが膨らむ

### Q-8. グロー / ブルームの適用範囲 ✅ **確定**
- **採用: 案 C (線 + 図形 + テキスト 全部)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: FUI 表現上、テキストグローも頻出 (Bladerunner ロゴ等)。SelectableDesignerItemViewModelBase に GlowRadius/Intensity/Color を持たせれば、全派生に自動波及。
- **不採用**: 案 A (線のみ) / 案 B (線 + 図形) — 適用範囲が中途半端

### Q-9. 非破壊性の実装方式 ✅ **確定**
- **採用: 案 A (元プロパティ保持で Glow プロパティを別途持つ)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: テーマ切替時に Effect レイヤーを総入れ替えする手間を避ける。図形ごとに GlowRadius / GlowIntensity / GlowColor が独立。Effect レイヤー方式 (案 B) は描画パイプラインに追加コストがかかる。
- **不採用**: 案 B (Effect レイヤー) — テーマとの相性が悪い

### Q-10. テーマ切替の適用範囲 ✅ **確定**
- **採用: 案 C (適用範囲を選択中図形 / レイヤー / プロジェクト全体で選べる)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: 大規模プロジェクトで一部だけテーマを変えるユースケースに対応。Undo で一括ロールバック可能。
- **不採用**: 案 A (全図形一括) — 細かい運用ができない / 案 B (Live preview) — 実装負荷が高い

### Q-11. パーツ機構との統合粒度 ✅ **確定**
- **採用: 案 A (ColorPaletteRef / LineStyleRef / GlowRadius / GlowIntensity / GlowColor を ExposedProperty 公開可能)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: Phase 1 の 8 型 (Double / Color / String 等) でカバー可能。型追加不要。ノードパーツの色テーマ追従 / Glow パラメトリック制御 / 線種統一に必須。
- **不採用**: 案 B (非公開) — 一貫テーマパーツが作れない / 案 C (一部のみ) — スコープが中途半端

### Q-12. Phase 4 のリリース粒度 ✅ **確定**
- **採用: 案 A (パレット + 線種 + グロー の 3 機能すべて含む)**
- 確定日: 2026-05-14 (作者判断)
- **採用理由**: INTENT.md で 4 要素 (パレット / 線種 / グロー / 非破壊) を Phase 4 スコープと明示。3 機能すべて入れることで「FUI らしさ」が一段引き上がる。
- **不採用**: 案 B (パレットのみ) — 線種・グローなしでは FUI らしさが出ない / 案 C (グローのみ後送り) — Phase 4.5 が分散

---

## 11. Phase 4-a 完了基準

このドキュメントが以下を満たすことをもって Phase 4-a 完了とする:

- [x] §10 のオープン問題すべてに作者の判断が反映されている (Q-1〜Q-12 全件確定)
- [x] §4 のデータモデル図がレビュー済み (Q-1 で組込+ユーザー、Q-2 で固定 5 スロット、Q-9 で図形側プロパティ確定)
- [x] §6 のシリアライズ仕様が既存形式と矛盾しない (ルート直下 `<Themes>` + designerItem 共通ブロックに `<Glow*>` 追加)
- [x] このドキュメントが `docs/fui/phase4-styles-themes.md` に保存されている
- [x] 後続の Phase 4-b 以降で参照されるべき既存コード位置がリストアップされている (§7)
- [x] Phase 1 のパーツ機構との統合点が明示されている (§9 + Q-11)

**Phase 4-a 完了。Phase 4-b (実装着手) に進む準備が整った。**

---

## 12. 確定事項サマリー (Phase 4-b 実装時のクイックリファレンス)

| 項目 | 確定内容 |
|---|---|
| パレット保持スコープ | 組込 (アプリ側コードハードコード) + ユーザー追加 (プロジェクトファイル) の両方 |
| セマンティックスロット | 固定 5 種: primary / accent / warning / info / background |
| パレット適用方法 | 直接書き換え (非バインド、Undo 可) |
| 組み込みプリセット | 4 種: Bladerunner / Matrix / 医療系青白 / アンバー CRT |
| 線種ライブラリの実装 | 完全新規 `LineStyle` 型 (StrokeDashArray + StrokeLineJoin + GlowSettings) |
| 線種組み込み 6 種 | Solid / Dash / Dot / DashDot / LongDash / Stepped |
| 段階線の実装 | StrokeDashArray パターン (`8, 4, 2, 4`) |
| グロー実装方式 | `BlurEffectViewModel` 派生、OpenCV `GaussianBlur` + 加算合成 (`Cv2.Add`) |
| グロー適用範囲 | 全 `SelectableDesignerItemViewModelBase` 派生 (線 + 図形 + テキスト) |
| 非破壊性の実装 | 図形側に `GlowRadius` / `GlowIntensity` / `GlowColor` を持たせる (元 EdgeBrush/FillBrush は保持) |
| テーマ切替範囲 | 選択中図形 / アクティブレイヤー全体 / プロジェクト全体 から選択可 |
| パーツ統合 | ColorPaletteRef / LineStyleRef / Glow 系を `ExposedProperty` 公開可能 |
| Phase 4 スコープ | パレット + 線種 + グロー + シリアライズ + パーツ統合 + チュートリアル |
| UI 文言 | 「テーマ」「パレット」「役割」「線種」「グロー」「段階線」 |

---

*Last updated: 2026-05-14 (確定版 v1.0)*
*Reviewer: dhq_boiler*
