# Phase 2.5 テキストマトリクス・テキストパス チュートリアル

> 関連: 設計仕様書 [`phase2-text-data-elements.md`](./phase2-text-data-elements.md)
> 前提: [`phase2-tutorial.md`](./phase2-tutorial.md) で Phase 2 の 3 要素 (MonoText/DataGen/NumSeq) と `TextElementBase` の共通プロパティを把握していること。

このドキュメントは Phase 2.5 で追加された **2 つの FUI 向けテキスト要素** の操作ガイドです。
セル状にテキストを並べる **テキストマトリクス** と、PolyBezier パスに沿って文字を配置する **テキストパス** の使い方と固有プロパティを解説します。

---

## 1. Phase 2.5 で追加された 2 要素

| ツール | UI 表記 | 内部型 | 用途 |
|---|---|---|---|
| **テキストマトリクス** | 「テキストマトリクス」 | `TextMatrixBlock` | 行 × 列のテキスト格子を 1 つの要素で配置 |
| **テキストパス** | 「テキストパス」 | `TextOnPathBlock` | 既存 PolyBezier に沿って 1 文字ずつ文字を並べる |

両要素とも `TextElementBase` を継承するため、Phase 2 の MonoText / DataGen / NumSeq と同じ共通プロパティ (フォント / サイズ / 色 / 行間 / 字間 / 不透明度 / 折返し) を持ちます。

---

## 2. テキストマトリクス (TextMatrixBlock)

行 × 列の格子状にテキストを並べる要素です。連番テーブル、Hex バイト列の格子、任意ラベルの整列など、FUI で頻出する「テーブル状の数値・文字列の塊」を 1 要素で組めます。

### 2.1 配置する

1. 画面左ツールバーから **「テキストマトリクス」** (アイコン: MDI grid) を選択します。
2. キャンバス上でドラッグして配置範囲を決めます。
3. 配置直後にデフォルト設定 (4×4 Sequential 連番) でテーブル状の文字列が表示されます。

### 2.2 プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Rows` | `int` | `4` | 行数 |
| `Columns` | `int` | `4` | 列数 |
| `CellMode` | `TextMatrixCellMode` | `Sequential` | セル内容の生成モード (下記 3 つ) |
| `Separator` | `string` | `" "` (半角空白) | 行内の区切り文字 (行間は常に改行) |
| `SequenceStart` | `int` | `0` | Sequential モードの開始値 |
| `SequenceFormat` | `string` | `""` | Sequential モードの .NET 書式指定子 (`"D3"` / `"X2"` 等) |
| `DataGenType` | `DataGeneratorType` | `Hex` | DataGenerator モードの生成種別 (8 種) |
| `DataGenSeed` | `int` | ランダム | DataGenerator モードのルート Seed |
| `CustomItems` | `string` | `""` | CustomList モードの改行区切り文字列リスト |

### 2.3 CellMode (3 モード、Q-6 案 D)

#### Sequential — 連番モード
`SequenceStart + (row * Columns + col)` の通し番号を `SequenceFormat` で書式化します。

| 設定 | 出力 (Rows=2, Columns=3) |
|---|---|
| Start=0, Format=`""` | `0 1 2` / `3 4 5` |
| Start=10, Format=`""` | `10 11 12` / `13 14 15` |
| Start=0, Format=`"D3"` | `000 001 002` / `003 004 005` |
| Start=254, Format=`"X2"` | `FE FF 100` / `101 102 103` |

> Format 先頭が `D` / `X` / `B` の場合は整数書式扱い、それ以外は double 書式扱い (`F2` / `N0` / `E3` 等)。

#### DataGenerator — ダミーデータ埋め込みモード
各セルの実 Seed は `HashCode.Combine(rootSeed, r, c)` で決定し、`DataGenerator.Generate` で 1 件生成します。同じ `DataGenSeed` なら同じ格子が再現されます (Reproducible)。

> `DataGenType` は Phase 2 の DataGenerator と同じ 8 種から選択可能 (Hex / Binary / IPv4 / IPv6 / UUID / Timestamp / RandomCode / LogLine)。

#### CustomList — 任意文字列リストモード
`CustomItems` に改行 (LF / CRLF どちらでも) 区切りで文字列を渡すと、左上から行優先で詰めていきます。
セル数に満たない場合の余りは **空文字列で埋める** (循環はしない)。

```text
CustomItems = "foo\nbar\nbaz\nqux"
Rows=2, Columns=2
→ "foo bar\nbaz qux"
```

### 2.4 再生成のタイミング
`Rows` / `Columns` / `CellMode` / `Separator` / `SequenceStart` / `SequenceFormat` / `DataGenType` / `DataGenSeed` / `CustomItems` のいずれかを変更すると、**同期的に** 再生成されます (Q-5 案 A)。

### 2.5 出力形式
すべて 1 つの `Text` 文字列にまとめられ、行内は `Separator`、行間は `Environment.NewLine` (Windows では `\r\n`) で連結されます。

---

## 3. テキストパス (TextOnPathBlock)

既存の **PolyBezier** に沿って文字を 1 つずつ並べる要素です。円周上の目盛り風文字列、曲線に沿ったラベルなど、FUI の主要モチーフを実現します (確定版 v1.0 Q-7 案 B)。

### 3.1 配置する手順 (PolyBezier との連携)

1. 先に **ベジエ曲線ツール (PolyBezier)** で文字を載せたいパスを描きます。
2. 画面左ツールバーから **「テキストパス」** (アイコン: MDI vector-curve) を選択します。
3. キャンバス上でドラッグして「テキストパス要素の領域」を確保します。配置直後はサンプルテキスト `"TEXT ON PATH"` が入り、PathReferenceId は **未設定 (null)** のため、まだ描画されません。
4. 配置した TextOnPath 要素の `PathReferenceId` に、参照する **PolyBezier の ID (Guid)** を設定します (UI から指定する手段は Phase 2.5 後続で対応予定。それまではプロジェクトファイルの編集 / `Detail*` ダイアログ未実装)。
5. `Text` / `StartOffset` / `Spacing` / `Side` / `Rotation` を調整します。

### 3.2 プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Text` | `string` | `""` | パスに沿って配置する文字列 |
| `PathReferenceId` | `Guid?` | `null` | 参照する PolyBezier の ID。null なら描画されない |
| `StartOffset` | `double` | `0.0` | パス全長に対するオフセット (0.0〜1.0) |
| `Spacing` | `double` | `0.0` | 文字間の追加スペース (px)。負値で詰める |
| `Side` | `TextOnPathSide` | `On` | `Above` / `On` / `Below` (パスのどちら側に配置するか) |
| `Rotation` | `TextOnPathRotation` | `Tangent` | `Tangent` (接線方向に回転) / `Upright` (常に直立) |

### 3.3 文字配置の計算ロジック

各文字 i は以下の式でパス上に配置されます (`TextOnPathGenerator.Generate`):

```
step      = FontSize × 0.6 + Spacing
cumLength = i × step
fraction  = StartOffset + cumLength / pathTotalLength
```

- `pathTotalLength` は PolyBezier を **直線近似 (Flatten)** してセグメント長を合計したもの。
- `fraction < 0` または `fraction > 1` の文字は描画されない (パスからはみ出る)。
- `PathGeometry.GetPointAtFractionLength(fraction, out point, out tangent)` で位置と接線を取得。
- `Side` は接線を 90 度回転した法線方向に `±FontSize/2` ぶんオフセットさせる。
- `Rotation = Tangent` は接線の角度 (度数法) を文字に適用、`Upright` は常に 0 度。

> 等幅フォント前提なので、`FontSize × 0.6` で 1 文字あたりの進む距離を概算しています。可変幅フォントだと詰まり / 隙間が出る可能性があります。

### 3.4 Side による配置の違い (水平パス例)

| Side | 結果 (FontSize=12 のとき) |
|---|---|
| `Above` | パスより上側に y = -6 ずれる |
| `On` | パス上にちょうど (y = 0) |
| `Below` | パスより下側に y = +6 ずれる |

### 3.5 Rotation の違い

| Rotation | 効果 |
|---|---|
| `Tangent` | 各文字を接線方向に回転 (FUI の円周ラベルに最適) |
| `Upright` | 各文字を常に直立 (パスに沿って文字位置だけ動かしたいケース) |

### 3.6 再生成のタイミング
`Text` / `FontSize` / `PathReferenceId` / `StartOffset` / `Spacing` / `Side` / `Rotation` のいずれかを変更すると、PolyBezier 実体を `Owner.AllItems` から ID で解決し直し、`Placements` を再構築します。

PolyBezier 自体の `Points` を変更してもこの段階では追従しません (Phase 2.5 後続で `Points` 変更時の自動再生成 / または「再生成」コマンド UI を検討)。

### 3.7 描画
DataTemplate は **ItemsControl + Canvas + ContentPresenter + RotateTransform** で各文字を個別配置します:

```xml
<ContentPresenter Canvas.Left="{Binding X}"
                  Canvas.Top="{Binding Y}"
                  RenderTransformOrigin="0.5, 0.5">
    <ContentPresenter.RenderTransform>
        <RotateTransform Angle="{Binding Angle}" />
    </ContentPresenter.RenderTransform>
</ContentPresenter>
```

`TextBlock` の `FontFamily` / `FontSize` / `Foreground` / `Opacity` は `RelativeSource AncestorType=ItemsControl` 経由で親 VM のプロパティを参照しています。

---

## 4. シリアライズ仕様

Phase 2 / 2.5 の他要素と同じく `<DesignerItems>` 配下に並びます。

### 4.1 要素タグ
| 要素 | XML タグ |
|---|---|
| TextMatrixBlock | `<TextMatrixBlock ...>` |
| TextOnPathBlock | `<TextOnPathBlock ...>` |

### 4.2 プロパティ名衝突回避のプレフィックス
他要素と被るプロパティは `TextMatrix*` / `TextOnPath*` プレフィックス付きで保存:
- TextMatrix: `TextMatrixRows` / `TextMatrixColumns` / `TextMatrixCellMode` / `TextMatrixSeparator` / `TextMatrixSequenceStart` / `TextMatrixSequenceFormat` / `TextMatrixDataGenType` / `TextMatrixDataGenSeed` / `TextMatrixCustomItems`
- TextOnPath: `TextOnPathReferenceId` (Guid) / `TextOnPathStartOffset` / `TextOnPathSpacing` / `TextOnPathSide` / `TextOnPathRotation`

### 4.3 後方互換
`TextOnPathReferenceId` 要素が無いときは `PathReferenceId = null` のまま (= 描画されない) として復元されます。

---

## 5. 制約・既知の事項

| 項目 | 内容 |
|---|---|
| プロパティダイアログ未実装 | 両要素とも `SupportsPropertyDialog = false`。値の編集は ExposedProperty 経由か、Phase 2.5 後続で `Detail*` ダイアログを実装する必要あり |
| TextMatrix のセル寸法 / Gap / Alignment | Phase 2.5 最小実装ではサポート無し (将来追加候補)。現状は単一 `Text` 文字列を `TextBlock` 単独で描画 |
| TextOnPath で PolyBezier 参照 UI なし | `PathReferenceId` を UI から選ぶ手段が未実装。プロジェクトファイル直接編集 or Phase 2.5 後続で「ピックする」ボタンを追加予定 |
| TextOnPath は PolyBezier の Points 変更に追従しない | 編集後に TextOnPath の任意プロパティを 1 回触れば再生成される (workaround) |
| 等幅フォント前提 | TextOnPath の文字幅は `FontSize × 0.6` で概算。可変幅フォントだと詰まり / 隙間が出る |
| アニメーション未対応 | Phase 5 (モーション/アニメーション) スコープ |

---

## 6. トラブルシューティング

| 症状 | 確認ポイント |
|---|---|
| TextOnPath を配置したが何も見えない | `PathReferenceId` が `null` か、参照先 PolyBezier が見つからない可能性。AllItems に PolyBezier が存在し、その `ID` (Guid) と一致しているか確認 |
| TextOnPath の文字がパスから飛び出して見えない | `fraction > 1` の文字は描画されない仕様。文字数を減らすか、Spacing を負値にして詰めるか、FontSize を小さくするか、PolyBezier 自体を長くする |
| TextOnPath で文字の回転が不自然 | `Rotation = Upright` で常に直立にできる。または PolyBezier の制御点を直して接線方向を変える |
| TextMatrix の DataGenerator モードで毎回違う出力 | `DataGenSeed` を明示的に固定する。同じ Seed なら同じ格子が再現される |
| TextMatrix の CustomList で余りセルに空欄が出る | 仕様 (循環はしない)。必要数ぶんの行を `CustomItems` に渡す |

---

## 7. 関連ドキュメント

- 設計仕様書: [`phase2-text-data-elements.md`](./phase2-text-data-elements.md) (Phase 2 / 2.5 共通)
- Phase 2 チュートリアル: [`phase2-tutorial.md`](./phase2-tutorial.md)
- Phase 1 チュートリアル: [`phase1-tutorial.md`](./phase1-tutorial.md)
- 設計者意図: [`../FUI_DESIGNER_INTENT.md`](../FUI_DESIGNER_INTENT.md)
- 一般操作: [`../../HowToUse.md`](../../HowToUse.md)
