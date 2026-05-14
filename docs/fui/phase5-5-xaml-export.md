# Phase 5.5 XAML アニメーション出力 設計仕様書 v1.0

> ステータス: **v1.0 確定** (2026-05-15、Q-1〜Q-12 全件「推奨案」で確定)
> 関連: [`phase5-motion-animation.md`](./phase5-motion-animation.md) (Phase 5 IR の確定版)、[`../FUI_DESIGNER_INTENT.md`](../FUI_DESIGNER_INTENT.md) §Phase 5.5

---

## 1. 目的 / 動機

`boilersGraphics` の **最大の独自性** となる機能 ([`FUI_DESIGNER_INTENT.md`](../FUI_DESIGNER_INTENT.md) §Phase 5.5)。
作者は WPF / .NET MAUI でアプリを開発しているので、`boilersGraphics` で描いた FUI アニメをそのままアプリへコピペで組み込める形に出力できれば、Illustrator / After Effects で実現できない領域に踏み込める。

具体的なゴール:
- Phase 5 で構築した IR (`Timeline` / `AnimationTrack` / `Keyframe`) を、WPF `Storyboard` または MAUI `Animation` の XAML に変換する。
- 出力は **依存ライブラリ無しで動くプレーン XAML** を第一目標とする。コピペで使える。
- 名前空間 / クラス名 / アクセス修飾子はユーザーが指定可能。

---

## 2. スコープ

### 2.1 含むもの (Phase 5.5)

- **WPF Storyboard XAML 出力** (Phase 5.5-b / 5.5-c): `UserControl` 単体ファイル形式。
- **MAUI Animation XAML 出力** (Phase 5.5-d): `ContentView` + 必要最小限の Code-behind。
- 図形マッピング: `NRectangleViewModel` / `NEllipseViewModel` / `PolygonViewModel` / `PathViewModel` / `StraightConnectorViewModel` / `OrthogonalConnectorViewModel` / `AnchorBezierConnectorViewModel` / `PolyBezierViewModel` / `LetterDesignerItemViewModel` の 9 種。
- アニメーション対応: Phase 5-c の `PropertyApplier` 対応パス全部 (`Left/Top/Width/Height/RotationAngle/EdgeBrush/FillBrush/EdgeThickness/Glow*`)。
- イージング: Phase 5-b の 12 種 × 3 モード → WPF / MAUI 標準のイージング関数に 1:1 マップ。
- Loop / PlayRange の反映。
- 出力ダイアログ + DiagramViewModel コマンド + TimelinePane ボタン。
- チュートリアル + サンプル出力。

### 2.2 含まないもの (将来枠)

- **テキスト系図形** (`MonoTextBlock` / `DataGeneratorTextBlock` / `NumberSequenceBlock` / `TextMatrixBlock` / `TextOnPathBlock`): フォント / 文字列を XAML 化する設計が別途必要。Phase 5.5 範囲外。
- **`PartInstance` の `ExposedProperties` バインド**: 展開後の `RenderedItems` を出力するので構造的には等価だが、ユーザー側で値を差し替える仕組みは未提供。
- **データバインド** (`ItemsControl` 化など): 静的なアニメ XAML を出すのみ。
- **MP4 / Lottie 出力**: Phase 5 で `IAnimationExporter` 枠組みを作ったが、Phase 5.5 では XAML 形式のみ実装。
- **MAUI VisualStateManager / Triggers 経由**: Phase 5.5 では `Animation` クラスベースのみ。
- **ResourceDictionary 形式**: 第一形式は `UserControl` / `ContentView`。ResourceDictionary 化は将来検討。

---

## 3. ターゲット形式

### 3.1 WPF (System.Windows.Media.Animation)

- 出力: 1 ファイル `<ProjectName>.xaml` + (任意で) `<ProjectName>.xaml.cs`。
- 形状ルート: `UserControl` 派生 (`x:Class` 指定)。
- アニメ: `Resources` の `Storyboard` + `DoubleAnimationUsingKeyFrames` / `ColorAnimationUsingKeyFrames` / `PointAnimationUsingKeyFrames` / `ObjectAnimationUsingKeyFrames`。
- 起動: `Loaded` イベントで `BeginStoryboard` を発火 (XAML 側のみで完結)、加えて公開メソッド `BeginAnimation()` / `StopAnimation()` を `.xaml.cs` に置き、ユーザーが手動制御もできる。
- イージング: `IEasingFunction` 派生 (`SineEase` / `QuadraticEase` / `CubicEase` / `QuarticEase` / `QuinticEase` / `BackEase` / `BounceEase` / `CircleEase` / `ElasticEase` / `ExponentialEase` / `PowerEase` / `Linear (= null)`) を `EasingFunction` プロパティに指定。
- Loop: `RepeatBehavior="Forever"` (Phase 5 で `Timeline.Loop = true` のとき)。
- 図形: `Path` / `Rectangle` / `Ellipse` / `Polygon` / `Line` 等の `System.Windows.Shapes`。

### 3.2 MAUI (Microsoft.Maui.Controls)

- 出力: 1 ファイル `<ProjectName>.xaml` + 1 ファイル `<ProjectName>.xaml.cs` (Animation API はコード側必須)。
- 形状ルート: `ContentView` 派生 (`x:Class` 指定)。
- アニメ: コードビハインドで `Microsoft.Maui.Controls.Animation` を組み立て、`OnAppearing` で開始。1 プロパティ = 1 `Animation`、`AnimationExtensions.Commit` でドライブ。
- イージング: MAUI 標準 (`Easing.Linear` / `Easing.SinIn` 等) + カスタム `Easing` を `Easing.CubicInOut(x => ...)` で構築。
- Loop: `Commit(... repeat: () => true ...)`。
- 図形: `Microsoft.Maui.Controls.Shapes` (`Path` / `Rectangle` / `Ellipse` / `Polygon` / `Line`)。

---

## 4. アーキテクチャ概観

```
boilersGraphics IR (Phase 5)
   Timeline / AnimationTrack / Keyframe
            │
            ▼
┌────────────────────────────────────────┐
│ Helpers/Animation/Export/               │
│  ├ IAnimationExporter (Phase 5-g)       │
│  ├ WpfStoryboardXamlBuilder (pure)      │  Phase 5.5-b
│  ├ WpfStoryboardXamlExporter            │  Phase 5.5-c
│  ├ MauiAnimationXamlBuilder (pure)      │  Phase 5.5-d
│  └ MauiAnimationXamlExporter            │  Phase 5.5-d
└────────────────────────────────────────┘
            │
            ▼
   ファイル出力 (`<Name>.xaml` + 必要時 `.xaml.cs`)
```

### 4.1 共通レイヤ
- **`XamlExportSettings`** (record): 出力先 / 名前空間 / クラス名 / XAML 整形オプション (インデント幅、改行コード)。
- **`PropertyToXamlMapper`** (pure): `PropertyApplier` 対応パス → XAML プロパティ名 + アニメ要素種別 (`DoubleAnimationUsingKeyFrames` / `ColorAnimationUsingKeyFrames` 等) の dispatch。
- **`EasingToXamlMapper`** (pure): `EasingKind × EasingMode` → WPF / MAUI イージング XAML 文字列。
- **`ShapeToXamlMapper`** (pure): `SelectableDesignerItemViewModelBase` 派生 → 対応 XAML 要素生成。

### 4.2 WPF 専用
- **`WpfStoryboardXamlBuilder`** (pure): `Timeline + AllItems + XamlExportSettings` を入力に、`UserControl` XAML 文字列を返す。
- **`WpfStoryboardXamlExporter`**: `IAnimationExporter` 実装。`Export()` で `WpfStoryboardXamlBuilder` を呼び、`outputPath` (= 単一ファイルパス) に書き出す。
- `IsMultiFile = false`、`DefaultFileExtension = ".xaml"`。
- 出力後、ユーザーが `.xaml.cs` も望めば生成 (オプション)。

### 4.3 MAUI 専用
- **`MauiAnimationXamlBuilder`** (pure): 上記 WPF と同じ役割の MAUI 版。`.xaml` (Shapes 配置) と `.xaml.cs` (Animation コード) の 2 つの文字列を返す。
- **`MauiAnimationXamlExporter`**: 同様に `IAnimationExporter` 実装。`Export()` で 2 ファイル書き出し (= 厳密には複数ファイルだが、ペアなので `IsMultiFile = false` + 補助 `.xaml.cs` も同時保存とする)。

---

## 5. Q&A (Q-1 〜 Q-12、全件 v1.0 で確定)

> Phase 5-a 流儀で、Phase 5.5-a (本仕様書) 確定時にすべて推奨案で承認。

### Q-1: ターゲット優先順 / 段階実装 — 案 A (確定)
WPF を先に完成させ、MAUI は WPF の構造をそのままシフトして実装する。
理由: WPF Storyboard が古くから安定しており、`DoubleAnimationUsingKeyFrames` の作りが IR と最も近い。MAUI は WPF 完成後に「同じ Builder シグネチャで MAUI 用文字列を吐く」段階で着手する。

### Q-2: 出力ルート要素 — 案 A (確定)
WPF: `UserControl`、MAUI: `ContentView`。
理由: コピペで使える「単一コンポーネント」が一番ユースケースに近い。ResourceDictionary 形式は再利用性は高いが「実アプリへ組み込む」のオーバヘッドが大きいので将来枠。

### Q-3: アニメ要素種別の統一 — 案 A (確定)
すべての Track を `*UsingKeyFrames` (= `LinearDoubleKeyFrame` / `EasingDoubleKeyFrame` / `SplineDoubleKeyFrame` のうち `EasingDoubleKeyFrame` を基本) に統一する。区間ごとに `DoubleAnimation` を並べる方式は採らない。
理由: Phase 5 IR と 1:1。各 `Keyframe` がそのまま 1 つの `EasingXxxKeyFrame` になる。

### Q-4: Storyboard 起動方法 — 案 C (確定)
**Loaded 自動起動 + 公開メソッド両対応**。
- XAML 側: `<Storyboard x:Key="MainStoryboard">` を `UserControl.Resources` に置き、`<EventTrigger RoutedEvent="UserControl.Loaded">` で `<BeginStoryboard Storyboard="{StaticResource MainStoryboard}" />` を発火。
- `.xaml.cs` 側 (任意): `public void BeginAnimation()` / `public void StopAnimation()` で同 Storyboard を `Begin()` / `Stop()`。
- ユーザーが「自動起動だけでよい」なら `.xaml.cs` を出力しないオプションも提供。

### Q-5: イージング 12 種マッピング — 案 A (確定)
Phase 5-b で WPF 互換命名にしてあるので **1:1 同名マップ**。Linear だけ `EasingFunction = null` で表現。
| Phase 5 EasingKind | WPF | MAUI |
|---|---|---|
| LinearEase | (null) | `Easing.Linear` |
| SineEase | `SineEase` | `Easing.Sin*` |
| QuadraticEase | `QuadraticEase` | カスタム `Easing(t => t*t)` |
| CubicEase | `CubicEase` | `Easing.CubicIn/Out/InOut` |
| QuarticEase | `QuarticEase` | カスタム |
| QuinticEase | `QuinticEase` | カスタム |
| ExponentialEase | `ExponentialEase` | `Easing.SpringIn/Out` 近似 |
| CircleEase | `CircleEase` | カスタム |
| BackEase | `BackEase` | `Easing.SpringIn/Out` |
| BounceEase | `BounceEase` | `Easing.BounceIn/Out` |
| ElasticEase | `ElasticEase` | カスタム |
| PowerEase | `PowerEase` | カスタム |

EasingMode は WPF 側はそのままプロパティで設定、MAUI 側は `In/Out/InOut` 接尾辞で振り分け。

### Q-6: Loop の表現 — 案 A (確定)
- `Timeline.Loop = true`: WPF `RepeatBehavior="Forever"` / MAUI `Commit(... repeat: () => true ...)`。
- `Loop = false`: 1 周のみ。
- `PlayRangeStart/End` は Storyboard の `BeginTime` / `Duration` には反映しない (全 Track の最大 Duration を使う)。実装シンプル優先。Phase 5.5 後の改善余地として PlayRange サブセット出力を残す。

### Q-7: 図形マッピング 9 種 — 案 A (確定)
| boilersGraphics ViewModel | WPF Shape | MAUI Shape |
|---|---|---|
| `NRectangleViewModel` | `Rectangle` | `Rectangle` |
| `NEllipseViewModel` | `Ellipse` | `Ellipse` |
| `PolygonViewModel` | `Polygon` | `Polygon` |
| `PathViewModel` | `Path` | `Path` |
| `StraightConnectorViewModel` | `Line` | `Line` |
| `OrthogonalConnectorViewModel` | `Path` (`PathGeometry`) | `Path` |
| `AnchorBezierConnectorViewModel` | `Path` (`PathGeometry`) | `Path` |
| `PolyBezierViewModel` | `Path` (`PathGeometry`) | `Path` |
| `LetterDesignerItemViewModel` | `TextBlock` (簡易) | `Label` |

`LetterDesignerItemViewModel` のみテキスト系で例外的に対応 (Phase 2 の他テキスト Block は範囲外、Q-8 参照)。

### Q-8: テキスト系図形のスコープ — 案 A (確定)
`MonoTextBlock` / `DataGeneratorTextBlock` / `NumberSequenceBlock` / `TextMatrixBlock` / `TextOnPathBlock` は **Phase 5.5 では対応しない**。
理由: フォント / 文字列 / DataGenerator のシード制御 / PathGeometry テキスト配置を XAML に等価変換するのは別途設計が必要。`LetterDesignerItemViewModel` のみは単純な単一文字なので `TextBlock` 1 つで足り、最小コストで対応可能。

### Q-9: PartInstance の扱い — 案 B (確定)
PartInstance は **展開後の `RenderedItems`** を直接 XAML 化する (= フラット化)。`PartDefinition` の構造は再現しない。
理由: Phase 5.5 の出力先 (WPF / MAUI) には「ExposedProperties + Binding」の対応物が無い。フラット化すれば動作確実。

### Q-10: テーマ / グロー / 線種の反映 — 案 A (確定)
- EdgeBrush / FillBrush / StrokeDashArray / StrokeLineJoin: そのまま `Stroke` / `Fill` / `StrokeDashArray` / `StrokeLineJoin` 属性に変換。
- Glow: `Effect="<DropShadowEffect Color=... BlurRadius=... ShadowDepth=0 Opacity=...>"` (= Phase 4-e-2 と同じ実装)。MAUI は `Shadow` プロパティ。
- テーマ自体は出力時点での値を埋め込み、後でテーマ切替可能にする `DynamicResource` 化は将来枠。

### Q-11: 出力 XAML 整形 — 案 A (確定)
- インデント: 半角スペース 4 (デフォルト)、設定で 2 にも切替可能。
- 改行: Environment.NewLine (Windows = CRLF)。設定で LF 強制も可能。
- コメント: 重要箇所 (`<!-- Generated by boilersGraphics Phase 5.5 v1.0 -->` ヘッダ + 各 Storyboard ブロックの簡易説明)。
- 属性順序: `x:Name` → `Canvas.Left` / `Canvas.Top` → `Width` / `Height` → `Stroke` / `Fill` 系 → `Effect`。

### Q-12: Phase 5.5 スコープ全体 — 案 A (確定)
**WPF / MAUI の両方を Phase 5.5 で完成させる**。ただしサブフェーズで段階実装:
- 5.5-a: 設計仕様書 (本書) ← Now
- 5.5-b: WPF 純粋ロジック (`WpfStoryboardXamlBuilder`) + テスト
- 5.5-c: WPF Exporter + ダイアログ + 配線
- 5.5-d: MAUI 純粋ロジック + Exporter + ダイアログ
- 5.5-e: チュートリアル + サンプル

---

## 6. 図形マッピング詳細

### 6.1 WPF
```xml
<!-- NRectangleViewModel: Left/Top/Width/Height/RotationAngle + EdgeBrush/FillBrush/EdgeThickness/StrokeDashArray/Glow -->
<Rectangle x:Name="Item_{Guid:N}"
           Canvas.Left="{Left}" Canvas.Top="{Top}"
           Width="{Width}" Height="{Height}"
           Stroke="{EdgeBrush:HexAARRGGBB}" StrokeThickness="{EdgeThickness}"
           Fill="{FillBrush:HexAARRGGBB}"
           StrokeDashArray="{StrokeDashArray:space-separated}"
           StrokeLineJoin="{StrokeLineJoin}"
           RenderTransformOrigin="0.5,0.5"
           Effect="{Glow→DropShadowEffect or null}">
    <Rectangle.RenderTransform>
        <RotateTransform Angle="{RotationAngle}" />
    </Rectangle.RenderTransform>
</Rectangle>
```

### 6.2 MAUI
```xml
<!-- 同上、Canvas.Left/Top の代わりに AbsoluteLayout.LayoutBounds で配置 -->
<Rectangle x:Name="Item_..."
           AbsoluteLayout.LayoutBounds="{Left},{Top},{Width},{Height}"
           AbsoluteLayout.LayoutFlags="None"
           Stroke="{EdgeBrush}" StrokeThickness="{EdgeThickness}"
           Fill="{FillBrush}"
           StrokeDashArray="{StrokeDashArray}"
           Shadow="{Glow→Shadow or null}">
    <Rectangle.Rotation>{RotationAngle}</Rectangle.Rotation>
</Rectangle>
```

### 6.3 Path 系 (PathViewModel / OrthogonalConnector / AnchorBezier / PolyBezier)
- `PathGeometry` を `Geometry` 属性または `Path.Data` に直接展開。
- `Phase 5.5-b` で各 ViewModel の `GeometryCreator` を再利用して mini-string-builder に流す。

---

## 7. プロパティマッピング詳細

| Phase 5 PropertyPath | WPF Target | MAUI Target | アニメ要素種別 |
|---|---|---|---|
| `Left.Value` | `Canvas.Left` (attached) | `AbsoluteLayout.LayoutBounds` の X 成分 (※)  | `DoubleAnimationUsingKeyFrames` |
| `Top.Value` | `Canvas.Top` | 同 Y | `DoubleAnimationUsingKeyFrames` |
| `Width.Value` | `Width` | `WidthRequest` | `DoubleAnimationUsingKeyFrames` |
| `Height.Value` | `Height` | `HeightRequest` | `DoubleAnimationUsingKeyFrames` |
| `RotationAngle.Value` | `RotateTransform.Angle` | `Rotation` | `DoubleAnimationUsingKeyFrames` |
| `EdgeBrush.Value` | `Stroke` | `Stroke` | `ColorAnimationUsingKeyFrames` (※ SolidColorBrush の Color に対し) |
| `FillBrush.Value` | `Fill` | `Fill` | 同上 |
| `EdgeThickness.Value` | `StrokeThickness` | `StrokeThickness` | `DoubleAnimationUsingKeyFrames` |
| `GlowRadius.Value` | `Effect.BlurRadius` | `Shadow.Radius` | `DoubleAnimationUsingKeyFrames` |
| `GlowIntensity.Value` | `Effect.Opacity` | `Shadow.Opacity` | `DoubleAnimationUsingKeyFrames` |
| `GlowColor.Value` | `Effect.Color` | `Shadow.Brush.Color` | `ColorAnimationUsingKeyFrames` |
| `ExposedProperties[...]` | — | — | (Q-9 で展開後の RenderedItems に解決) |

(※) MAUI `AbsoluteLayout.LayoutBounds` は `Rect` 値なので、Left/Top/Width/Height が個別に動く Phase 5 IR からは合成が必要。`MauiAnimationXamlBuilder` で `BoundsTypeConverter` を介して再構築する。

---

## 8. ファイル構成 (実装時の想定)

```
boilersGraphics/
  Helpers/Animation/Export/
    XamlExportSettings.cs            (record DTO)
    PropertyToXamlMapper.cs          (pure)
    EasingToXamlMapper.cs            (pure)
    ShapeToXamlMapper.cs             (pure)
    WpfStoryboardXamlBuilder.cs      (pure)        ← 5.5-b
    WpfStoryboardXamlExporter.cs     (IAnimationExporter 実装) ← 5.5-c
    MauiAnimationXamlBuilder.cs      (pure)        ← 5.5-d
    MauiAnimationXamlExporter.cs                  ← 5.5-d
  Views/Animation/
    WpfXamlExportDialog.xaml(.cs)
    MauiXamlExportDialog.xaml(.cs)
  ViewModels/Animation/
    WpfXamlExportDialogViewModel.cs
    MauiXamlExportDialogViewModel.cs

boilersGraphics.Test/
  Helpers/Animation/Export/
    PropertyToXamlMapperTest.cs
    EasingToXamlMapperTest.cs
    ShapeToXamlMapperTest.cs
    WpfStoryboardXamlBuilderTest.cs
    MauiAnimationXamlBuilderTest.cs
    WpfStoryboardXamlExporterTest.cs
    MauiAnimationXamlExporterTest.cs

docs/fui/
  phase5-5-tutorial.md               ← 5.5-e
  phase5-5-xaml-export.md            ← 本書 (5.5-a)
```

### DiagramViewModel 配線
- `OpenWpfXamlExportDialogCommand : DelegateCommand`
- `OpenMauiXamlExportDialogCommand : DelegateCommand`
- TimelinePane のトランスポートバーに「WPF XAML...」「MAUI XAML...」ボタンを追加 (PNG 連番ボタンの隣)。

---

## 9. 既知の制約 / 将来枠

- **データバインド** (`{Binding ...}`) は出さない。出力 XAML は静的アニメ。
- **テキスト系図形** (MonoText / DataGen / NumSeq / TextMatrix / TextOnPath) は Phase 5.5 範囲外。フォント XML 化 / シード再現 / Path テキストの再現を含む別フェーズ (Phase 6 候補)。
- **MAUI VisualStateManager / Triggers**: Animation API ベースのみ。状態駆動アニメは将来枠。
- **MAUI Easing カスタム** が必要な 8 種 (Quadratic / Quartic / Quintic / Circle / Exponential / Elastic / Power / 一部 Back): `Easing(t => f(t))` のラムダ Code-behind 展開で対応。
- **PlayRangeStart != 0 / PlayRangeEnd != Duration** の出力: 全 Track の `Duration` をマックスとし、それより前の `BeginTime` 設定はしない (PlayRange は Phase 5.5 では無視)。
- **ResourceDictionary 形式 / DynamicResource テーマ切替**: 将来枠。
- **MP4 / Lottie 出力**: `IAnimationExporter` で並列実装可能だが Phase 5.5 範囲外。

---

## 10. クイックリファレンス (Phase 5.5-b 以降の実装時参照)

### 10.1 出力フロー (WPF)

```
WpfStoryboardXamlExporter.Export(timeline, outputPath, resolver, options)
  ↓
WpfStoryboardXamlBuilder.Build(timeline, allItems, XamlExportSettings)
  → string xaml
  ↓
File.WriteAllText(outputPath, xaml, encoding)
[+ options.GenerateCodeBehind なら outputPath + ".cs" に code-behind を書く]
```

### 10.2 オプション (`XamlExportSettings`)

```csharp
public sealed record class XamlExportSettings
{
    public string TargetNamespace { get; init; } = "MyApp.Animations";
    public string ClassName { get; init; } = "FuiAnimation";
    public string AccessModifier { get; init; } = "public"; // "internal" も可
    public bool GenerateCodeBehind { get; init; } = true;
    public int IndentWidth { get; init; } = 4;
    public string NewLine { get; init; } = "\r\n";
    public bool IncludeHeaderComment { get; init; } = true;
}
```

### 10.3 確定 Q セット早見表

| Q | 一行サマリ |
|---|---|
| Q-1 | WPF を先、MAUI を後 |
| Q-2 | UserControl / ContentView 単体ファイル形式 |
| Q-3 | `*UsingKeyFrames` (EasingXxxKeyFrame) 統一 |
| Q-4 | Loaded 自動 + 公開 BeginAnimation() の両対応 |
| Q-5 | WPF 12 種同名 1:1 (Linear は null)、MAUI は In/Out 接尾辞 |
| Q-6 | Loop=true → RepeatBehavior=Forever / MAUI repeat: () => true |
| Q-7 | 9 図形を WPF/MAUI 標準 Shape にマップ |
| Q-8 | テキスト系 (Letter 以外) は Phase 5.5 範囲外 |
| Q-9 | PartInstance は RenderedItems を展開して出力 |
| Q-10 | EdgeBrush/FillBrush/Glow/LineStyle はそのまま埋め込み |
| Q-11 | スペース 4 / CRLF / ヘッダーコメント有り |
| Q-12 | WPF + MAUI 両方を Phase 5.5 で完成 |
