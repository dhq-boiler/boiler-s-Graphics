# Phase 6 テキスト系図形 XAML 出力 設計仕様書 v1.0

> ステータス: **v1.0 確定** (2026-05-22、Q-1〜Q-9 全件「推奨案」で確定)
> 関連: [`phase5-5-xaml-export.md`](./phase5-5-xaml-export.md) (Phase 5.5 確定版、Q-8 で本フェーズを「範囲外」と確定済み)、[`phase2-text-data-elements.md`](./phase2-text-data-elements.md) (テキスト系 5 種 Model/VM の元仕様)、[`../FUI_DESIGNER_INTENT.md`](../FUI_DESIGNER_INTENT.md) §Phase 6

---

## 1. 目的 / 動機

Phase 5.5 でアニメーション XAML 出力 (WPF Storyboard / MAUI Animation) は完成したが、Q-8 で **テキスト系 5 種** を範囲外と確定。
本フェーズではその穴を埋める:

- **MonoTextBlock** — 単行テキスト
- **DataGeneratorTextBlock** — Hex/Binary/IPv4/IPv6/UUID/Timestamp/RandomCode/LogLine の各種ランダム文字列 (Seed あり)
- **NumberSequenceBlock** — 等差数列 (Start/End/Step/Format/Direction)
- **TextMatrixBlock** — 行列状テキスト (Sequential/DataGenerator/CustomList)
- **TextOnPathBlock** — PolyBezier に沿った 1 文字毎配置

これらが出力できれば、FUI で頻出する「ステータステキスト」「カウンタ」「ログ風流れ」「曲線ラベル」を WPF / MAUI アプリにコピペ可能になる。

---

## 2. スコープ

### 2.1 含むもの

- テキスト系 **5 種すべて** を WPF / MAUI XAML に出力 (Q-1 案 A)。
- `TextElementBase` 共通プロパティのマッピング: `Text` / `FontFamily` / `FontSize` / `Foreground` / `Background` / `TextOpacity` / `LineHeight` / `LetterSpacing` / `IsWordWrap`。
- アニメ対応プロパティの拡張 (`FontSize.Value` / `Foreground.Value` / `TextOpacity.Value`) を `PropertyApplier` / `PropertyToXamlMapper` に追加 (Q-6)。
- 出力ダイアログ・コマンド・TimelinePane ボタンは Phase 5.5 のものを **そのまま流用** (新規 UI なし)。
- チュートリアル + サンプル出力 (Phase 5.5-e と同じ運用)。

### 2.2 含まないもの (将来枠)

- **動的テキスト変更アニメ**: `Text.Value` を Keyframe で書き換える機能。文字列補間の意味論が必要、別フェーズ。
- **DataGenerator / NumberSequence のランタイム再生成**: 出力 XAML は **静的スナップショット**。実行時に同じ Seed から再計算する仕組みは未実装 (Q-3 案 B のコメント形式で生成元情報のみ残す)。
- **pack URI フォント埋め込み**: フォント本体のコピーは行わない。pack URI はフォント名のみに短縮して出力 (Q-2)。
- **TextOnPath の動的再計算**: 出力時点の Placements を硬化。実行時に PathGeometry が変わっても文字配置は追従しない。
- **Letter の再実装**: `AbstractLetterDesignerItemViewModel` は Phase 5.5-b-3 で対応済み、本フェーズでは触らない (Q-8)。

---

## 3. ターゲット形式

### 3.1 WPF

- 出力先要素は **`TextBlock`** (Mono/DataGen/NumSeq/TextMatrix) または **`Canvas` + 個別 `TextBlock` × N** (TextOnPath)。
- Canvas 配置: 既存図形と同様 `Canvas.Left` / `Canvas.Top` でルート位置決め。
- アニメ: 既存の `Storyboard` ベースを継承。`FontSize` は `DoubleAnimationUsingKeyFrames`、`Foreground` は `ColorAnimationUsingKeyFrames`、`TextOpacity` は `DoubleAnimationUsingKeyFrames` で `Opacity` プロパティ宛 (TextElement の `Opacity` を使用)。

```xml
<!-- MonoText / DataGen / NumSeq / TextMatrix 共通 -->
<TextBlock x:Name="Item_{Guid:N}"
           Canvas.Left="{Left}" Canvas.Top="{Top}"
           Width="{Width}" Height="{Height}"
           FontSize="{FontSize}"
           FontFamily="{FontFamilyShortName}"
           Foreground="{Foreground:HexAARRGGBB}"
           Background="{Background:HexAARRGGBB}"
           Opacity="{TextOpacity}"
           Text="{Text:escaped}"
           TextWrapping="{Wrap or NoWrap}"
           LineHeight="{LineHeight}" />
```

```xml
<!-- TextOnPath: Placements を個別子要素として展開 -->
<Canvas x:Name="Item_{Guid:N}"
        Canvas.Left="0" Canvas.Top="0">
    <!-- N 個の文字を Canvas.Left/Top + RotateTransform で配置 -->
    <TextBlock Canvas.Left="{p.X}" Canvas.Top="{p.Y}"
               FontSize="{FontSize}" FontFamily="..." Foreground="..."
               Text="{p.Char}"
               RenderTransformOrigin="0.5,0.5">
        <TextBlock.RenderTransform>
            <RotateTransform Angle="{p.Angle}" />
        </TextBlock.RenderTransform>
    </TextBlock>
    <!-- ... -->
</Canvas>
```

### 3.2 MAUI

- ルート要素: **`Label`** (Mono/DataGen/NumSeq/TextMatrix) または **`AbsoluteLayout` + 個別 `Label` × N** (TextOnPath)。
- 配置: `AbsoluteLayout.LayoutBounds="{Left},{Top},{Width},{Height}"` + `LayoutFlags="None"`。
- アニメ: WPF と同じく `FontSize`/`TextColor`/`Opacity` を `Animation` API で個別駆動。
- フォント: MAUI は `FontFamily` 属性に登録名を指定するが、Phase 6 では `FontFamily` 文字列をそのまま出す。利用側 `MauiProgram.cs` での font 登録は範囲外。

---

## 4. アーキテクチャ概観

```
boilersGraphics IR (Phase 5)
   Timeline / AnimationTrack / Keyframe (テキスト系プロパティ含む)
            │
            ▼
┌──────────────────────────────────────────────────┐
│ Helpers/Animation/Export/                          │
│  ├ ShapeToXamlMapper (Phase 5.5-b)                 │
│  │   + TryMapWpfText() ← Phase 6-b で新規追加        │
│  │   + TryMapMauiText() ← Phase 6-b で新規追加       │
│  ├ PropertyToXamlMapper (Phase 5.5-b)              │
│  │   + FontSize / Foreground / TextOpacity 追加     │
│  ├ WpfStoryboardXamlBuilder (Phase 5.5-b)          │
│  │   + テキスト系 dispatch 追加                       │
│  └ MauiAnimationXamlBuilder (Phase 5.5-d)          │
│      + テキスト系 dispatch 追加                       │
└──────────────────────────────────────────────────┘
            │
            ▼
   既存 Exporter (WpfStoryboardXamlExporter / MauiAnimationXamlExporter) 経由でファイル出力
```

新規ファイルは作らず、**既存 Mapper / Builder を拡張する**方針。テストも既存テスト群に追加する。

---

## 5. Q&A (Q-1 〜 Q-9 推奨案)

### Q-1: 対象スコープ — 案 A (推奨)
**テキスト系 5 種すべてを Phase 6 で対応**。
- 理由: TextOnPath だけ将来枠に回すと、Phase 6 を再開する動機が下がる。Placements 展開は重いが「Canvas + N 個 TextBlock を静的に書き出す」だけなのでロジックは単純。
- 代案 B: 4 種だけ Phase 6、TextOnPath は Phase 6.5 へ。
- 代案 C: TextOnPath は ItemsControl + DataTemplate で動的に。MAUI 等価表現が無いため却下。

### Q-2: フォント Family の直列化 — 案 B (推奨)
**System フォント名のみを出力**。pack URI / 埋め込みフォント参照を含む文字列からはフォント名 (`#` 以降) を抽出して出力。
- 理由: 出力先プロジェクトでは pack URI のリソースパスが解決できないため、フォントが見つからずデフォルトフォントにフォールバックする。System フォント名のみであれば「無ければデフォルトフォント」で済むため互換性が高い。
- 抽出ロジック: 文字列に `#` が含まれていれば最後の `#` 以降を採用 (`pack://application:,,,/...;component/Fonts/#JetBrains Mono` → `JetBrains Mono`)。それ以外はそのまま。
- 代案 A: pack URI をそのまま出力。実環境で動かないため却下。
- 代案 C: ユーザーオプション化。Phase 6 のスコープを膨らませるので将来枠。

### Q-3: DataGen / NumSeq の値の扱い — 案 B (推奨)
**出力時点のスナップショット + XAML コメントで生成元情報を残す**。
- DataGeneratorTextBlock → `<!-- Generator: DataGenerator (Type=Hex, Seed=12345, Count=8, Separator=" ", Layout=Inline) -->` を直前に出力。
- NumberSequenceBlock → `<!-- Generator: NumberSequence (Start=0, End=100, Step=1, Format=D3, Direction=Horizontal) -->`。
- TextMatrixBlock → `<!-- Generator: TextMatrix (Rows=4, Columns=4, CellMode=Sequential, SequenceStart=0) -->`。
- TextOnPathBlock → `<!-- Generator: TextOnPath (PathRefId={Guid}, StartOffset=0, Spacing=0, Side=On, Rotation=Tangent) -->`。
- 理由: 静的 XAML としては Text 値の硬化が最もシンプル。ユーザーが「同じ条件で再生成したい」場合に必要な情報をコメントとして残しておけば、手作業でも別ツールでも再生成できる。
- 代案 A: スナップショットのみ (コメントなし)。生成元情報を失うので不便。
- 代案 C: ランタイム再生成。Behavior / コードビハインドの依存が増え、Phase 5.5 の「依存ライブラリ無し」方針と衝突するため却下。

### Q-4: TextMatrix のレイアウト — 案 A (推奨)
**単一 TextBlock + 改行**。既存の `Text.Value` (改行 + Separator 連結済み) をそのまま `Text` 属性に流す。
- 理由: 視覚的には行列に見えれば十分で、Grid 構造は不要。改行は `&#x0A;` でエンコード。
- 代案 B: Grid + TextBlock × R×C のセル分割。動的だが XAML が肥大化、フォント幅変更時にズレるリスク。

### Q-5: TextOnPath の表現 — 案 A (推奨)
**Canvas (WPF) / AbsoluteLayout (MAUI) + 個別 TextBlock/Label × Placements.Count**。
- 出力時点の `Placements` (X/Y/Angle) を全件展開して個別子要素として書き出す。
- 各文字は `Canvas.Left` / `Canvas.Top` + `RenderTransform RotateTransform` (WPF) / `AbsoluteLayout.LayoutBounds` + `Rotation` (MAUI) で配置。
- 理由: 静的 XAML としては個別 TextBlock 群が最も整合する。WPF の ItemsControl は MAUI に等価が無く、両プラットフォームで同じ構造を取れる「個別配置」が現実的。
- 代案 B: ItemsControl + DataTemplate。MAUI で困難なので却下。
- 代案 C: 将来枠。Phase 6 の魅力が下がるので却下。

### Q-6: アニメ可能プロパティの拡張 — 案 C (推奨)
**`FontSize.Value` / `Foreground.Value` / `TextOpacity.Value` の 3 つを `PropertyApplier` / `PropertyToXamlMapper` に追加**。
- `FontSize` → WPF: `TextBlock.FontSize` (double)、MAUI: `Label.FontSize` (double)。`DoubleAnimationUsingKeyFrames`。
- `Foreground` → WPF: `(TextBlock.Foreground).(SolidColorBrush.Color)`、MAUI: `Label.TextColor` (Color)。`ColorAnimationUsingKeyFrames`。
- `TextOpacity` → WPF: `TextBlock.Opacity` (double)、MAUI: `Label.Opacity` (double)。
- 理由: テキスト系の見栄えを変える最も需要が高い 3 プロパティ。`TextOpacity` は Phase 5.5 で将来枠だった「Opacity アニメ」を一部カバーできる。
- 代案 A: `FontSize` + `Foreground` のみ。`TextOpacity` を入れない理由が無い。
- 代案 B: 共通プロパティ (Left/Top/Width/Height/Rotation/Glow) のみ。テキスト変更が地味になるので却下。

### Q-7: MAUI 側スコープ — 案 A (推奨)
**WPF と同等の対応**。Phase 5.5-d と同じ並列実装パターン。
- 理由: 「WPF と MAUI 両対応」は boilersGraphics の独自性、片方だけだと魅力半減。
- 代案 B: WPF だけ Phase 6、MAUI は Phase 6.5。スコープ膨張回避だが、Phase 5.5 流儀から外れる。

### Q-8: Letter との関係 — 案 A (推奨)
**`AbstractLetterDesignerItemViewModel` は既存実装そのまま、`TextElementBaseViewModel` 派生 5 種は別パス**。
- 理由: Letter は Phase 5.5-b-3 で動作確定、テスト済み。統一すると既存テストへの影響範囲が大きい。
- `ShapeToXamlMapper.TryMapWpfShape` の switch に **5 種を個別ケース追加** (`is TextElementBaseViewModel` の単一ケースで分岐) → 派生ごとに必要なら個別関数呼出し。

### Q-9: テスト戦略 — 案 A (推奨)
**Phase 5.5 と同じ 2 段構成**。
1. `ShapeToXamlMapperTest` にテキスト系の pure 関数テストを追加 (1 種につき 2〜4 件)。
2. `WpfStoryboardXamlBuilderTest` / `MauiAnimationXamlBuilderTest` に dispatch テストを追加。
3. `WpfStoryboardXamlExporterTest` / `MauiAnimationXamlExporterTest` の writeAllText delegate inject を流用して I/O 無しでファイル出力経路を検証。
- 想定追加テスト件数: 30〜50 件 (1873 → 1903〜1923)。

---

## 6. プロパティマッピング詳細

| TextElementBase プロパティ | WPF 出力 (TextBlock) | MAUI 出力 (Label) | アニメ可 (Phase 6) |
|---|---|---|:---:|
| `Text` | `Text="..."` 属性 (XML escape) | `Text="..."` 属性 | ✗ |
| `FontFamily` | `FontFamily="{short name}"` | `FontFamily="..."` | ✗ |
| `FontSize` | `FontSize="{double}"` | `FontSize="{double}"` | ✓ |
| `Foreground` | `Foreground="{#AARRGGBB}"` | `TextColor="{#AARRGGBB}"` | ✓ |
| `Background` | `Background="{#AARRGGBB}"` | `BackgroundColor="{#AARRGGBB}"` | ✗ |
| `LineHeight` | `LineHeight="{double}"` | `LineHeight="{double}"` | ✗ |
| `LetterSpacing` | (WPF にない、`TextOptions.TextFormattingMode`...) — 出力しない | `CharacterSpacing="{double}"` | ✗ |
| `TextOpacity` | `Opacity="{double}"` (TextBlock 自体の Opacity) | `Opacity="{double}"` | ✓ |
| `IsWordWrap` | `TextWrapping="{Wrap or NoWrap}"` | `LineBreakMode="{WordWrap or NoWrap}"` | ✗ |

### TextOnPath 追加プロパティ (ノンアニメ、出力時点で硬化)

| プロパティ | 用途 | XAML 出力先 |
|---|---|---|
| `PathReferenceId` | PolyBezier 参照 → Placements 計算 | XAML コメントに記載のみ |
| `StartOffset` | パス開始からの相対位置 (0..1) | コメントのみ |
| `Spacing` | 文字間隔 (px) | コメントのみ |
| `Side` | Above / On / Below | コメントのみ |
| `Rotation` | Fixed / Tangent | コメントのみ |
| `Placements[i].X/Y/Angle` | 各文字の最終座標 | 各 TextBlock の Canvas.Left/Top + RotateTransform |

---

## 7. ファイル構成 (実装時の想定)

```
boilersGraphics/
  Helpers/Animation/Export/
    ShapeToXamlMapper.cs           (Phase 5.5-b 既存 → テキスト系 5 種を switch に追加)
    PropertyToXamlMapper.cs        (Phase 5.5-b 既存 → FontSize/Foreground/TextOpacity 追加)
    MauiShapeToXamlMapper.cs       (Phase 5.5-d 既存 → テキスト系 5 種を switch に追加)
    MauiPropertyToCSharpMapper.cs  (Phase 5.5-d 既存 → 3 プロパティ追加)
    WpfStoryboardXamlBuilder.cs    (Phase 5.5-b 既存 → 必要に応じテキスト系 dispatch)
    MauiAnimationXamlBuilder.cs    (Phase 5.5-d 既存 → 同上)
    (新規ファイルなし)

  ※ FontFamily 短縮の共通ヘルパが必要なら ShapeToXamlMapper.cs にプライベートメソッドとして追加

boilersGraphics.Test/
  Helpers/Animation/Export/
    ShapeToXamlMapperTextTest.cs            (新規、5 種 pure テスト)
    MauiShapeToXamlMapperTextTest.cs        (新規)
    PropertyToXamlMapperTest.cs             (既存に 3 パス追加)
    MauiPropertyToCSharpMapperTest.cs       (既存に 3 パス追加)
    WpfStoryboardXamlBuilderTextTest.cs     (新規、Builder dispatch テスト)
    MauiAnimationXamlBuilderTextTest.cs     (新規)

docs/fui/
  phase6-text-xaml-export.md      (本書)
  phase6-tutorial.md              (Phase 6-e で作成)
```

DiagramViewModel / TimelinePane / ダイアログは **変更なし** (Phase 5.5 のものをそのまま使う)。

---

## 8. 既知の制約 / 将来枠

- **Text アニメ非対応**: `Text.Value` の Keyframe 補間は未実装。複数 Keyframe で文字列を切替えたい場合は別途検討。
- **DataGen / NumSeq のランタイム再生成**: Phase 6 では静的スナップショット。コメントに生成元情報のみ残す。
- **pack URI フォント埋め込み**: フォント本体のコピーは行わない。出力先プロジェクトでフォントが見つからない場合は OS デフォルトフォントになる。
- **TextOnPath の動的再計算**: PathReferenceId が指す PolyBezier が出力後に変更されても、出力 XAML の文字配置は更新されない。
- **LetterSpacing on WPF**: WPF の `TextBlock` には素の `LetterSpacing` がない。`Typography.*` を使う方法もあるが Phase 6 では対応せず属性出力を省略する。
- **MAUI フォント登録**: `MauiProgram.ConfigureFonts(fonts => fonts.AddFont(...))` への登録は出力先プロジェクトで手動対応。コメントで案内する。

---

## 9. クイックリファレンス

### 9.1 出力フロー (WPF、Phase 5.5 と共通)

```
WpfStoryboardXamlExporter.Export(timeline, outputPath, resolver, options)
  ↓
WpfStoryboardXamlBuilder.Build(timeline, allItems, settings, resolver)
  ├ allItems を走査
  │   ├ 既存図形 → ShapeToXamlMapper.TryMapWpfShape / TryMapWpfPath
  │   └ テキスト系 → ShapeToXamlMapper.TryMapWpfShape (Phase 6-b で 5 種追加)
  │       └ TextOnPath は専用処理 (Canvas + 個別 TextBlock 展開)
  ↓
File.WriteAllText(outputPath, xaml, encoding)
```

### 9.2 確定 Q セット早見表 (v1.0)

| Q | 一行サマリ |
|---|---|
| Q-1 | テキスト系 5 種すべて Phase 6 で対応 |
| Q-2 | フォント Family は `#` 以降の短縮名のみ出力 |
| Q-3 | DataGen/NumSeq/TextMatrix は値スナップショット + 生成元コメント |
| Q-4 | TextMatrix は単一 TextBlock + 改行で表現 |
| Q-5 | TextOnPath は Canvas + 個別 TextBlock × N で展開 |
| Q-6 | FontSize / Foreground / TextOpacity の 3 つをアニメ対応に追加 |
| Q-7 | WPF / MAUI 両方を Phase 6 で完成 |
| Q-8 | Letter は既存そのまま、TextElementBase 派生は別パス |
| Q-9 | pure 関数テスト + Builder dispatch テストの 2 段構成 |

---

## 10. サブフェーズ計画

| サブ | 内容 | 想定ファイル / 想定テスト追加 |
|---|---|---|
| **6-a** (本書) | 設計仕様書 + Q&A 確定 | docs/fui/phase6-text-xaml-export.md |
| **6-b** | Mapper 拡張 (pure) | ShapeToXamlMapper.cs / MauiShapeToXamlMapper.cs / PropertyToXamlMapper.cs / MauiPropertyToCSharpMapper.cs + pure テスト 30〜50 件 |
| **6-c** | WPF Builder 統合 | WpfStoryboardXamlBuilder.cs + dispatch テスト |
| **6-d** | MAUI Builder 統合 | MauiAnimationXamlBuilder.cs + dispatch テスト |
| **6-e** | チュートリアル + intent 更新 + autodebugger 検証 | docs/fui/phase6-tutorial.md / HowToUse.md / FUI_DESIGNER_INTENT.md |

ビルド・テスト・コミットは各サブフェーズ完了時。最終的に develop に push。
