# Phase 2 テキスト・データ要素 チュートリアル

> 関連: 設計仕様書 [`phase2-text-data-elements.md`](./phase2-text-data-elements.md)
> 前提: [`phase1-tutorial.md`](./phase1-tutorial.md) を読んでパーツ機構の基本操作を理解していること。

このドキュメントは Phase 2 で追加された **3 つの FUI 向けテキスト・データ要素** の操作ガイドです。
モノスペース等幅フォント前提の技術系テキスト、自動生成されるダミーデータ、連番列を素早く配置し、Phase 1 のパーツ機構と組み合わせて再利用する流れまでを解説します。

---

## 1. Phase 2 で追加された 3 要素

| ツール | UI 表記 | 内部型 | 用途 |
|---|---|---|---|
| **モノスペーステキスト** | 「モノスペーステキスト」 | `MonoTextBlock` | 等幅フォントで任意の固定文字列を配置 |
| **データジェネレータ** | 「データジェネレータ」 | `DataGeneratorTextBlock` | Hex / IP / UUID 等のダミーデータを自動生成 |
| **数値列** | 「数値列」 | `NumberSequenceBlock` | `Start..End` を `Step` 刻みで連番展開 |

3 要素はすべて `TextElementBase` を共通基底とし、表示まわりのプロパティ (フォント / サイズ / 色 / 行間 / 字間 / 不透明度 / 折返し) を共有します。
既存の `LetterDesignerItemViewModel` ファミリには手を加えていないため、従来の「文字ツール」「縦書きツール」もこれまで通り使えます。

---

## 2. 共通プロパティ (TextElementBase)

3 要素すべてに共通する表示プロパティ:

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Text` | `string` | `""` (空) | 表示テキスト。DataGen / NumSeq では再生成結果が自動的に入る |
| `FontFamily` | `string` | JetBrains Mono (同梱) | フォント。アプリ内蔵フォントから優先解決される ([§7](#7-デフォルトフォント-jetbrains-mono) 参照) |
| `FontSize` | `int` | `12` | フォントサイズ (pt) |
| `Foreground` | `Brush` | `White` | 文字色。**デフォルトは白なので、白背景キャンバスでは視認できない点に注意** |
| `Background` | `Brush?` | `null` | 背景色 (透過可) |
| `LineHeight` | `double?` | `null` (auto) | 行高さ |
| `LetterSpacing` | `double` | `0` | 文字間隔。FUI で頻出する「スカスカ配置」用 |
| `TextOpacity` | `double` | `1.0` | 文字の不透明度 (0.0〜1.0) |
| `IsWordWrap` | `bool` | `false` | 自動折返しの有無 |

> **白テキストについて**: Phase 2 はダークテーマ前提の FUI 用途を想定して `Foreground` のデフォルトを白にしています。白背景上で見えないときは Background を黒/濃灰にするか、Foreground を黒に変更してください。

---

## 3. モノスペーステキスト (MonoTextBlock)

固定文字列を等幅フォントで表示するシンプルな要素です。FUI 内のラベルや小さな技術メモを敷き詰める用途を想定しています。

### 3.1 配置する
1. 画面左ツールバーから **「モノスペーステキスト」** (アイコン: MDI console-line) を選択します。
2. キャンバス上でドラッグして配置範囲を決めます (既存の文字ツールと同じ操作感)。
3. 配置直後はテキストが空なので、後述のプロパティ編集で `Text` を設定します。

### 3.2 プロパティ
専用プロパティはなく、[§2 共通プロパティ](#2-共通プロパティ-textelementbase) のみを持ちます。
複数行を入力したい場合は `Text` に改行 (`\n`) を含めてください。

### 3.3 リサイズ
`IsResizable = true` のためマウスドラッグでリサイズ可能。ただし `IsWordWrap = false` のままだとリサイズしてもテキスト自体は折り返しません。

---

## 4. データジェネレータ (DataGeneratorTextBlock)

ハイブリッド Seed (確定版 v1.0 Q-3 案 C) で再現可能なダミーデータを自動生成します。

### 4.1 配置する
1. 画面左ツールバーから **「データジェネレータ」** (アイコン: MDI shuffle-variant) を選択します。
2. キャンバス上でドラッグして配置します。
3. 配置直後にデフォルト設定 (Hex × 8 件 × OneLine) で 1 回生成され、結果が `Text` に入ります。

### 4.2 プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Type` | `DataGeneratorType` | `Hex` | 生成種別 (下表参照) |
| `Seed` | `int` | ランダム | 乱数シード。同値なら同じ出力 (Reproducible) |
| `IsSeedLocked` | `bool` | `false` | true: 明示 Seed を保持 / false: 「再生成」時に新規 Seed |
| `Count` | `int` | `8` | 生成件数 |
| `Separator` | `string` | `" "` (半角空白) | OneLine 時の区切り文字 |
| `Layout` | `DataGeneratorLayout` | `OneLine` | `OneLine` / `MultiLine` (改行区切り) |

### 4.3 生成種別 (DataGeneratorType)

| Type | 生成例 |
|---|---|
| `Hex` | `A3` `F0` `12` `7E` `B4` … (16 進バイト 2 桁) |
| `Binary` | `0010` `1101` `0110` … (4 ビット単位) |
| `Ipv4Address` | `192.168.1.42` |
| `Ipv6Address` | `2001:0db8:85a3:0000:0000:8a2e:0370:7334` (完全形) |
| `Uuid` | `xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx` (RFC 4122 v4) |
| `Timestamp` | `2026-05-13T10:24:33Z` (2020-01-01 〜 2030-12-31 の範囲、ISO 8601 / UTC) |
| `RandomCode` | `Q5T2RP` (英数字 6 桁、`O/0/I/1` は誤読防止で除外) |
| `LogLine` | `[INFO] 2026-05-13 10:24:33 module: message` (擬似ログ行) |

### 4.4 再生成のタイミング
`Type` / `Seed` / `Count` / `Separator` / `Layout` のいずれかを変更すると、**同期的に** 再生成されます (確定版 v1.0 Q-5 案 A)。
`IsSeedLocked` は UI のスイッチ用フラグなので、変更しても再生成は走りません。

### 4.5 Seed 運用パターン (Q-3 案 C ハイブリッド)
- **そのまま使う**: 配置時点で決定された `Seed` がプロジェクトファイルに保存され、次回開いたときも同じ見た目を維持。
- **見た目を固定**: `IsSeedLocked = true` にしてから `Seed` を任意の値に固定。後から `Seed` の自動更新 UI を追加しても上書きされません。
- **ガチャ的に振り直す**: `IsSeedLocked = false` で「再生成」を繰り返し、気に入った見た目になったら `IsSeedLocked = true` で固定する想定です (再生成 UI は今後追加予定)。

---

## 5. 数値列 (NumberSequenceBlock)

開始値 / 終了値 / ステップから連番を生成し、書式指定で文字列化して並べる要素です。`0..255` を `X2` で並べれば 16 進テーブル、`0..360` を `5` 刻みで並べれば角度目盛りなど、FUI 頻出の「数値が一面に並ぶ」表現を 1 要素で組めます。

### 5.1 配置する
1. 画面左ツールバーから **「数値列」** (アイコン: MDI numeric) を選択します。
2. キャンバス上でドラッグして配置します。
3. 配置直後にデフォルト設定 (`0..10` step `1` × Horizontal) で生成され、`Text` に `"0 1 2 3 4 5 6 7 8 9 10"` が入ります。

### 5.2 プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Start` | `double` | `0` | 開始値 |
| `End` | `double` | `10` | 終了値 (`End` を超えない範囲まで列挙) |
| `Step` | `double` | `1` | ステップ。負値で降順可。`0` のときは 0 件 |
| `Format` | `string` | `""` (Invariant) | .NET 書式指定子。`"D3"` / `"X4"` / `"B16"` / `"F2"` 等 |
| `Separator` | `string` | `" "` | Horizontal / Grid の行内区切り |
| `Direction` | `NumberSequenceDirection` | `Horizontal` | `Horizontal` / `Vertical` / `Grid` |
| `GridRows` | `int` | `1` | Grid 時の行数 |
| `GridColumns` | `int` | `1` | Grid 時の列数 |

### 5.3 書式指定の注意点
- `Format` 先頭が `D` / `X` / `B` (大文字小文字無視) の場合は **整数系書式** とみなし、`long` に丸めてから書式化されます。`0.5` 等の端数は切り捨てられる点に注意。
- それ以外の書式 (`F2` / `N0` / `E3` 等) は `double` のまま書式化されます。
- すべて `CultureInfo.InvariantCulture` 固定なので、ロケールによって小数点が `,` になることはありません (FUI でレイアウト崩れを防ぐ意図)。

### 5.4 列挙アルゴリズム
- 値は `start + step * i` の整数インデックスで列挙されるため、`Step = 0.1` を 100 回足したときの累積誤差を回避できます。
- `step > 0` で `start > end` の場合 (またはその逆) は 0 件出力。

### 5.5 Direction の振る舞い
| Direction | 出力例 (`0..3` step `1`、Separator=" ") |
|---|---|
| `Horizontal` | `0 1 2 3` |
| `Vertical` | `0\n1\n2\n3` (改行区切り) |
| `Grid` (rows=2, cols=2) | `0 1\n2 3` |

`Grid` で列挙件数が `rows * cols` に満たないときは末尾を空文字で埋め、超過時は切り捨てます。

---

## 6. パーツ機構との統合 (Phase 1 連携)

Phase 2 の 3 要素は Phase 1 のパーツ機構の上で **そのまま** 動きます。専用の追加コードパスはありません。

### 6.1 公開可能プロパティ (Q-9 確定スコープ)

各要素のうち、Phase 1 の `ExposedProperty` (8 型) で公開可能なのは以下のみです。
「骨格」を成すプロパティ (Type / FontFamily / Layout / Direction / Side / Rotation 等) は **意図的に非公開** に留めています (Phase 2 スコープ)。

| 要素 | 公開可能なプロパティ |
|---|---|
| MonoTextBlock | `Text` / `FontSize` / `Foreground` / `Background` / `LetterSpacing` |
| DataGeneratorTextBlock | `Seed` / `Count` / `Separator` |
| NumberSequenceBlock | `Start` / `End` / `Step` / `Format` / `Separator` |

### 6.2 統合例: ラベル付き目盛りリングパーツ
1. キャンバスに「円環 (Ellipse) + 数値列 (NumberSequence)」を描き、両方選択 → 右クリック「パーツ化…」で `RingScale` 等の名前を付けます。
2. パーツ編集ウィンドウを開き、右パネル下部の「公開パラメータを追加…」から:
   - `LabelStart : Double` (デフォルト `0`)
   - `LabelEnd : Double` (デフォルト `360`)
   - `LabelStep : Double` (デフォルト `30`)
   - `LabelFormat : String` (デフォルト `"D3"`)
3. パーツ内部の NumberSequence の `Start` / `End` / `Step` / `Format` をそれぞれ上記公開パラメータに Binding します。
4. パーツインスタンスを配置するたびに、4 つの数値で目盛りの密度と書式を制御できる「目盛りリングパーツ」として再利用できます。

### 6.3 パーツインスタンスの値伝搬機構 (Phase 2-f)
- パーツインスタンスが内部に Phase 2 要素を持っていても、`PartInstanceViewModel.RenderedItems` に **`Items` の ID 引継ぎ Clone** として展開されます (DataTemplate は `ItemsControl + Canvas` で内部描画)。
- 公開パラメータの値はリフレクション (`BindableReactiveProperty<T>.Value`) で内部要素に直接代入され、`int → double` 等の `Convert.ChangeType` 経由の型変換、および `Nullable<T>` 解決にも対応しています。
- 詳細は設計仕様書 [`phase2-text-data-elements.md`](./phase2-text-data-elements.md) §5.3 と Phase 2-f コミット (`d33b7c11` / `14ecc69b` / `fb8f3023`) を参照。

---

## 7. デフォルトフォント (JetBrains Mono)

確定版 v1.0 Q-8 で確定した同梱フォントです。

- **同梱ライセンス**: SIL Open Font License v1.1 (OFL) — リポジトリ同梱可
- **同梱ファミリ**: Regular / Bold / Italic / BoldItalic の 4 ウェイト
- **配置先**: `boilersGraphics/Fonts/JetBrainsMono-*.ttf` + `OFL.txt` + `AUTHORS.txt`
- **参照方法**: `pack://application:,,,/boilersGraphics;component/Fonts/#JetBrains Mono`
- **フォールバックチェーン**: JetBrains Mono → Cascadia Code → Consolas → MS Gothic

`TextElementBase.DefaultFontFamily` で上記の pack URI + 3 段フォールバックを丸ごと指定しているので、インストール済みの JetBrains Mono が無くてもアプリ同梱版が必ず使われます。

---

## 8. シリアライズ仕様

確定版 v1.0 Q-4 案 A により、3 要素とも既存の `<DesignerItems>` 配下に並びます (専用セクションを切らない)。

### 8.1 要素タグ
| 要素 | XML タグ |
|---|---|
| MonoTextBlock | `<MonoTextBlock ...>` |
| DataGeneratorTextBlock | `<DataGeneratorTextBlock ...>` |
| NumberSequenceBlock | `<NumberSequenceBlock ...>` |

### 8.2 プロパティ名衝突回避のプレフィックス
共通プロパティ (Text / FontFamily / ...) は同名タグですが、要素固有プロパティのうち命名衝突しうるものはプレフィックス付きで保存されます:
- DataGen: `DataGenType` / `DataGenSeparator` / `DataGenLayout`
- NumSeq: `NumFormat` / `NumSeqSeparator`

### 8.3 後方互換
- `Background` / `LineHeight` が `null` のときは要素そのものが省略されます。
- `double.Parse` は `CultureInfo.InvariantCulture` 固定。
- 旧形式プロジェクトファイルにはこれらの要素タグが存在しないため、`ObjectDeserializer` は自然にスキップします。

---

## 9. 制約・既知の事項

| 項目 | 内容 |
|---|---|
| プロパティダイアログ未実装 | 3 要素とも `SupportsPropertyDialog = false`。値の編集は ExposedProperty 経由か、パーツ編集ウィンドウのプロパティパネル拡張を待つ必要あり (将来の Phase で対応) |
| Foreground デフォルト白 | ダークテーマ前提。白キャンバス上では視認できないため、Background を濃色にするかソース改変で対応 |
| 再生成は常に同期 | Q-5 案 A: Count が小さい想定でチラつき回避を優先。Phase 5+ で大量配置時のボトルネックが出たら別案 |
| アニメーション未対応 | Phase 5 (モーション/アニメーション) スコープ |
| TextMatrix / TextOnPath は Phase 2.5 | Q-11 案 B により後送り (`phase2-text-data-elements.md` §9 参照) |

---

## 10. トラブルシューティング

| 症状 | 確認ポイント |
|---|---|
| 配置したのに何も見えない | `Foreground = White` がデフォルトなので、白キャンバス上では透明同然になる。Background を黒、または Foreground を黒に切替 |
| Hex / Binary の出力が毎回変わる | `IsSeedLocked = false` の状態。固定したいなら `IsSeedLocked = true` にして Seed を明示 |
| NumberSequence の件数が思った数より 1 多い / 少ない | `end` 端点は **超えない範囲** まで含む仕様。`0..10 step 1` は **11 件** (0,1,...,10) |
| 数値が `0,5` のように区切られる | 起こらない設計 (Invariant 固定)。OS のロケール変更影響を受けないことを確認 |
| パーツ内部の Phase 2 要素が空表示 | `PartInstance.RenderedItems` の初期化漏れの可能性。`InitializeRenderedItems` が走るのは ExecuteAddItem 経由 / ObjectDeserializer 経由 / PartDefinitions の Add/Replace/Reset。Remove は **意図的に対象外** |

---

## 11. 関連ドキュメント

- 設計仕様書: [`phase2-text-data-elements.md`](./phase2-text-data-elements.md)
- Phase 1 チュートリアル: [`phase1-tutorial.md`](./phase1-tutorial.md)
- Phase 1 設計仕様書: [`phase1-parametric-components.md`](./phase1-parametric-components.md)
- 設計者意図: [`../FUI_DESIGNER_INTENT.md`](../FUI_DESIGNER_INTENT.md)
- 一般操作: [`../../HowToUse.md`](../../HowToUse.md)
