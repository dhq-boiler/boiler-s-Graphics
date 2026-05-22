# Phase 6 テキスト系図形 XAML 出力 チュートリアル

> 関連: 設計仕様書 [`phase6-text-xaml-export.md`](./phase6-text-xaml-export.md) (v1.0 確定、2026-05-22)
> 前提: [`phase5-5-tutorial.md`](./phase5-5-tutorial.md) で WPF / MAUI XAML 出力の基本フローを把握していること。

このドキュメントは Phase 6 で追加された **テキスト系図形 5 種** (MonoText / DataGenerator / NumberSequence / TextMatrix / TextOnPath) の WPF Storyboard / MAUI Animation XAML 出力対応の操作ガイドです。

---

## 1. Phase 6 で対応されたテキスト系図形

| 図形 | 用途 | WPF 出力 | MAUI 出力 |
|---|---|---|---|
| **MonoText** | 静的な単行テキスト | `<TextBlock>` 1 つ | `<Label>` 1 つ |
| **DataGenerator** | Hex / Binary / UUID / IPv4 / IPv6 / Timestamp / RandomCode / LogLine | `<TextBlock>` + 生成元 Generator コメント | `<Label>` + 生成元 Generator コメント |
| **NumberSequence** | 等差数列 (Start/End/Step/Format/Direction) | 同上 | 同上 |
| **TextMatrix** | 行列状テキスト (Sequential / DataGen / CustomList) | `<TextBlock>` (Text 内に改行) + コメント | `<Label>` (Text 内に改行) + コメント |
| **TextOnPath** | PolyBezier に沿った文字配置 | `<Canvas>` + 個別 `<TextBlock>` × N | `<AbsoluteLayout>` + 個別 `<Label>` × N |

新しい UI 要素は追加されていません。**Phase 5.5 の「WPF XAML...」「MAUI XAML...」ボタンがそのままテキスト系図形にも対応**します。

---

## 2. 出力フロー

Phase 5.5 と同じです。

1. ツールボックスからテキスト系図形 (TextElement カテゴリ) をキャンバスに配置。
2. プロパティダイアログでテキスト・フォント・色などを設定 (各図形の生成パラメータも)。
3. Timeline で `FontSize.Value` / `Foreground.Value` / `TextOpacity.Value` のキーフレームを打つ (アニメさせたい場合)。
4. Timeline ペイン > **「WPF XAML...」** または **「MAUI XAML...」** ボタンをクリック。
5. ダイアログで名前空間・クラス名・出力先などを入力 → 「書き出し」。

---

## 3. 出力例

### 3.1 MonoText (WPF)

```xml
<TextBlock x:Name="Item_..."
    Canvas.Left="10" Canvas.Top="20"
    FontSize="24"
    FontFamily="JetBrains Mono"
    Foreground="#FFFFFFFF"
    Opacity="1"
    TextWrapping="NoWrap"
    Text="STATUS OK" />
```

### 3.2 DataGenerator (WPF)

```xml
<!-- Generator: DataGenerator (Type=Uuid, Seed=12345, Count=4, Separator=" ", Layout=OneLine) -->
<TextBlock x:Name="Item_..."
    Canvas.Left="0" Canvas.Top="0"
    FontSize="14"
    FontFamily="JetBrains Mono"
    Foreground="#FF00FF00"
    TextWrapping="NoWrap"
    Text="3f2504e0-4f89-11d3-9a0c-0305e82c3301 ..." />
```

`<!-- Generator: ... -->` コメントには出力時点での **生成パラメータ**が記録されます。同じ条件で値を再生成したいときは、このコメントを見て手作業 / 別ツールで再現できます。

### 3.3 TextOnPath (WPF)

```xml
<!-- Generator: TextOnPath (PathRefId=..., StartOffset=0, Spacing=0, Side=On, Rotation=Tangent, Placements=12) -->
<Canvas x:Name="Item_..."
    Canvas.Left="0" Canvas.Top="0">
    <TextBlock
        Canvas.Left="40" Canvas.Top="80"
        FontSize="20" FontFamily="JetBrains Mono" Foreground="#FFFFFFFF"
        Text="W"
        RenderTransformOrigin="0,0">
        <TextBlock.RenderTransform>
            <RotateTransform Angle="-15" />
        </TextBlock.RenderTransform>
    </TextBlock>
    <!-- ...残りの文字も同様に -->
</Canvas>
```

PolyBezier に沿って 1 文字ずつ `Canvas.Left/Top` + `RotateTransform Angle` で配置されます。**出力時点の Placements が硬化**するので、出力後に PolyBezier が変わっても XAML 上の文字配置は更新されません (静的なスナップショット出力)。

### 3.4 MonoText (MAUI)

```xml
<Label x:Name="Item_..."
    AbsoluteLayout.LayoutBounds="10,20,100,30"
    AbsoluteLayout.LayoutFlags="None"
    FontSize="24"
    FontFamily="JetBrains Mono"
    TextColor="#FFFFFFFF"
    Opacity="1"
    LineBreakMode="NoWrap"
    Text="STATUS OK" />
```

WPF の `Canvas.Left/Top` → MAUI の `AbsoluteLayout.LayoutBounds`、`Foreground` → `TextColor`、`TextWrapping` → `LineBreakMode` と対応します。

---

## 4. アニメ対応プロパティ

Phase 6 で **3 つの新しいプロパティ**がアニメ対応に加わりました。

| Property | WPF Target | MAUI Target | キーフレーム種別 |
|---|---|---|---|
| `FontSize.Value` | `FontSize` | `{0}.FontSize = d;` | Double |
| `Foreground.Value` | `(TextBlock.Foreground).(SolidColorBrush.Color)` | `{0}.TextColor = c;` | Color |
| `TextOpacity.Value` | `Opacity` | `{0}.Opacity = d;` | Double |

これらに Phase 5 IR の Track + Keyframe を打てば、Phase 5.5 のときと同じく Storyboard / Animation API のキーフレーム XAML に変換されます。

例: MonoText の FontSize を 12 → 32 へ補間したい場合、プロパティダイアログで FontSize 行の ◇ ボタンを 0 秒で、Now を 1 秒に進めて FontSize を変更して再度 ◇、これだけで `<DoubleAnimationUsingKeyFrames Storyboard.TargetProperty="FontSize">` 付きの XAML が生成されます。

---

## 5. 既知の制約

- **フォント Family**: 出力時点で **`#` 以降のシステムフォント名のみ**が出力されます (例: `pack://application:,,,/.../#JetBrains Mono` → `JetBrains Mono`)。pack URI のリソースパスは引き継がれないため、出力先プロジェクトで JetBrains Mono がインストールされていないとデフォルトフォントにフォールバックします。MAUI では `MauiProgram.ConfigureFonts(...)` での登録が別途必要です。
- **Text 値のアニメ**: Phase 6 では `Text.Value` の Keyframe 補間はサポートされません (アニメ対応は FontSize / Foreground / TextOpacity の 3 つのみ)。
- **DataGenerator / NumberSequence の再生成**: 出力 XAML は **静的スナップショット**です。実行時に同じ Seed から再生成する仕組みはありません。Generator コメントを見て手動で再生成してください。
- **TextOnPath の動的再計算**: PathReferenceId が指す PolyBezier が出力後に変わっても、XAML の文字配置は追従しません。
- **WPF の LetterSpacing**: WPF `TextBlock` には素の LetterSpacing 属性が無いため、出力で省略されます (MAUI は `CharacterSpacing` として出力)。

---

## 6. 関連ファイル

- 設計仕様書: [`phase6-text-xaml-export.md`](./phase6-text-xaml-export.md)
- 実装: `boilersGraphics/Helpers/Animation/Export/ShapeToXamlMapper.cs` (BuildText / BuildTextOnPath / ShortenFontFamily 等)、`MauiShapeToXamlMapper.cs` (BuildLabelText / BuildTextOnPathMaui)、`PropertyToXamlMapper.cs` (FontSize / Foreground / TextOpacity 追加)、`MauiPropertyToCSharpMapper.cs` (同)
- テスト: `boilersGraphics.Test/Helpers/Animation/Export/ShapeToXamlMapperTextTest.cs` ほか
