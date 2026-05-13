# Phase 2-a: テキスト・データ要素 設計仕様書 (ドラフト)

> このドキュメントは `docs/FUI_DESIGNER_INTENT.md` Phase 2 の **サブフェーズ 2-a** に対応する設計仕様書ドラフトである。
> 実装着手前に作者 (dhq_boiler) のレビューを経て確定する。
> 確定前の項目は **オープン問題 (§10)** に集約してある。

---

## 1. 概要

### 1.1 目的
FUI らしさの肝である「微細な技術テキストとデータっぽい数値・コードが大量に配置される」表現を、ユーザーが少ない操作で組み上げられるようにするための、専用のテキスト/データ要素群を追加する。

### 1.2 この機能が満たすべき体験
1. キャンバスに **モノスペース等の技術系フォント** で大量の数値・コード列を素早く配置できる。
2. ダミーデータ (hex 列、IP、UUID、ログ行風文字列など) は **タイプを選ぶだけで自動生成** され、形を変えたければシードや桁数を変えるだけで再生成できる。
3. **テーブル / 行列風** の文字配置は、行数・列数・セル幅・行間を数値指定で組み立てられる。
4. **テキストパス (パスに沿った文字列)** で円周上の目盛り風文字列が描ける。
5. **カウンタ / 数値ロガー** で「N から M まで」「ステップ S」のような連番列を一括生成できる。

### 1.3 非スコープ (Phase 2-a では扱わない)
- アニメーション付きのカウンタ (例: 0 → 1234.56 を時間で増加させる) は **Phase 5 以降**。
- スクリプト/式によるテキスト生成は **Phase 7 以降**。
- 多言語 / 縦書きの完全対応 (簡易対応はする)。
- IME 状態を考慮した複雑なテキスト編集 (既存 Letter ベース最低限のみ)。

### 1.4 Phase 2-a のスコープに含むもの (改めて整理)
- モノスペーステキストブロック型 (`MonoTextBlock`)
- ダミーデータジェネレータ型 (`DataGenerator`、hex/binary/ip/uuid/timestamp/random/log 等)
- テーブル/行列レイアウト型 (`TextMatrix`)
- テキストパス型 (`TextOnPath`)
- カウンタ列型 (`NumberSequence`)
- 上記すべてを **Phase 1 のパーツ機構の上で扱える** ように設計する (専用コードパスを増やさない)

---

## 2. 用語定義

| 用語 | 意味 | UI 表記 (案) |
|---|---|---|
| **モノスペーステキスト** | 等幅フォントを前提とする技術系テキストブロック | 「モノスペーステキスト」 |
| **データジェネレータ** | ランダム / 規則的なダミーデータを生成するテキスト要素 | 「データジェネレータ」 |
| **テキストマトリクス** | 行 × 列の格子状にテキストを並べるレイアウト | 「テキストマトリクス」 |
| **テキストパス** | 任意のパスに沿って配置されるテキスト | 「テキストパス」 |
| **数値列** | 開始値 / 終了値 / ステップから自動生成される連番列 | 「数値列」 |

これらは Phase 1 の「パーツ (Part)」と直交する概念。Phase 1 のパーツ化機構を使えば、`MonoTextBlock` + 既存図形を組み合わせた構成も「パーツ」として再利用できる。

---

## 3. 各機能の設計案

### 3.1 モノスペーステキストブロック (MonoTextBlock)
既存 `LetterDesignerItemViewModel` の拡張として位置付け、または新規派生として実装するかは §10 Q-1 で決める。

主要プロパティ:
- `Text : string` (複数行可。改行は `\n`)
- `FontFamily : FontFamilyEx` (デフォルト候補は JetBrains Mono / IBM Plex Mono / Cascadia Code / Source Code Pro / Consolas / MS Gothic)
- `FontSize : int`
- `Foreground : Brush`
- `Background : Brush?` (透過可)
- `LineHeight : double?` (auto / 数値指定)
- `LetterSpacing : double` (FUI で頻出する「文字をスカスカに配置する」表現用)
- `TextOpacity : double` (透過テキスト)
- `IsWordWrap : bool` (既存 IsAutoLineBreak と同様)

### 3.2 データジェネレータ (DataGenerator)
**型 (DataGeneratorType, enum)**:
- `Hex` — `A3 F0 12 7E B4 ...` 16 進バイト列
- `Binary` — `0010 1101 0110 ...` 2 進列
- `Ipv4Address` — `192.168.1.42` 形式
- `Ipv6Address` — 完全形 + 短縮形
- `Uuid` — RFC 4122 v4 形式
- `Timestamp` — ISO 8601 / Unix epoch / カスタム書式
- `RandomCode` — 英数字混在のランダム文字列 (英大文字+数字 等)
- `LogLine` — 疑似ログ行 (`[INFO] 2026-05-13 10:24:33 module: message`)

共通プロパティ:
- `Type : DataGeneratorType`
- `Seed : int?` — 同じ Seed なら同じ出力 (Reproducible)
- `Count : int` — 何件生成するか (改行区切りで複数行出力)
- `Separator : string` — 単一行に並べる場合のセパレータ
- `Layout : OneLine / MultiLine` — 単一行 / 複数行のいずれ
- 表示系プロパティは `MonoTextBlock` と共有する (FontFamily, FontSize, Foreground, ...)

### 3.3 テキストマトリクス (TextMatrix)
**主要プロパティ**:
- `Rows : int`
- `Columns : int`
- `CellTextProvider` — 何を表示するか
  - 案 A: 単純な数値 (`r * Columns + c`)
  - 案 B: DataGenerator 1 個を「セルの中身」として埋め込む (各セルが独立した乱数生成器、Seed = (rootSeed, r, c))
  - 案 C: 任意の文字列 (改行区切りリストで直接入力)
- `CellWidth / CellHeight : double` — セル寸法 (auto = フォント幅基準)
- `RowGap / ColumnGap : double` — 行間 / 列間
- `Alignment : Left / Center / Right`
- 表示系プロパティは MonoTextBlock と共有

### 3.4 テキストパス (TextOnPath)
**主要プロパティ**:
- `Text : string` (文字列)
- `Path : PathGeometry` — 配置先のパス (既存 `PolyBezierViewModel` 等からも生成可)
- `StartOffset : double` — パス先頭からのオフセット (0.0 〜 1.0)
- `Spacing : double` — 文字間の追加スペース (px or em)
- `Side : Above / On / Below` — パスのどの位置に配置するか
- `Rotation : Tangent / Upright` — 各文字を接線方向に回転 / 常に直立

### 3.5 数値列 (NumberSequence)
**主要プロパティ**:
- `Start : double`
- `End : double`
- `Step : double`
- `Format : string` — `"D2"`, `"X4"`, `"F2"` 等の .NET 書式指定 (`{value:format}` で適用)
- `Separator : string` — 改行 / カンマ / スペース / 任意
- `Direction : Horizontal / Vertical / Grid(rows, cols)`
- 表示系プロパティは MonoTextBlock と共有

---

## 4. データモデル

### 4.1 全体図 (案)

```
TextElementBase (abstract, 既存 DesignerItemViewModelBase 派生)
  ├─ Text          : string  (生成後の最終文字列。表示はこれ)
  ├─ FontFamily    : FontFamilyEx
  ├─ FontSize      : int
  ├─ Foreground    : Brush
  ├─ Background    : Brush?
  ├─ LineHeight    : double?
  ├─ LetterSpacing : double
  ├─ TextOpacity   : double
  └─ IsWordWrap    : bool

MonoTextBlock         : TextElementBase
DataGeneratorTextBlock: TextElementBase + (Type, Seed, Count, Separator, Layout)
TextMatrixBlock       : TextElementBase + (Rows, Columns, CellTextProvider, CellWidth/Height, Gaps, Alignment)
TextOnPathBlock       : TextElementBase + (Path, StartOffset, Spacing, Side, Rotation)
NumberSequenceBlock   : TextElementBase + (Start, End, Step, Format, Separator, Direction)
```

### 4.2 クラス配置案
- `boilersGraphics\Models\Text\` (新規) — Models 層
- `boilersGraphics\ViewModels\Text\` (新規) — ViewModel 層
- 各 ViewModel は既存 `DesignerItemViewModelBase` 派生 (Letter ファミリと同じパターン)

### 4.3 既存 Letter との関係
- 既存 `LetterDesignerItemViewModel` / `LetterVerticalDesignerItemViewModel` は触らない (Phase 2-a スコープ外、後方互換維持)
- `MonoTextBlock` は別途新規追加。**Letter のデザイン上の置き換えではない**。
- ただし、共通する表示処理 (フォント描画、文字メトリクス計算等) は再利用する

---

## 5. 編集動作仕様

### 5.1 ツールバー / メニューからの追加
- 画面左ツールバーに **「テキスト要素」セクション** を追加し、5 種類のアイコンを並べる (要 UI モック)
- または右クリックメニュー > 「テキスト要素を追加」サブメニュー

### 5.2 プロパティ編集
- プロパティパネル (右側) で各要素固有のパラメータを編集できる
- Phase 1-c-6-d-4 で実装した PartEditor の Property Panel と同じスタイルを踏襲
- 数値プロパティの変更は **リアクティブに即時反映** (R3)

### 5.3 Phase 1 パーツ機構との関係
- これらの要素を **Phase 1 のパーツ機構で「公開パラメータ」として宣言できる** ことを要件とする
- 例: 「同心円リング + 半径ごとの目盛りラベル」パーツを定義し、`半径` と `ラベル数` を公開パラメータにすれば、配置のたびに `NumberSequence.Count` と `Ring.Radius` を 1 つの値から駆動できる
- 要件: `TextElementBase` の各プロパティが Binding ターゲットとして使える

### 5.4 リジェネレーション (DataGenerator / NumberSequence)
- DataGenerator の `Seed` を変えると **即座に再生成**
- NumberSequence の Start/End/Step を変えると **即座に再生成**
- 再生成中に表示がチラつかないよう、計算は背景スレッド可 (要 §10 Q-5)

---

## 6. シリアライズ仕様

### 6.1 プロジェクトファイル拡張
- 既存 XML 形式に、`<TextElements>` セクションを追加 (案)
- もしくは既存 `DesignerItems` の中に並べる (`<MonoTextBlock>`, `<DataGeneratorTextBlock>` 等の要素タグ)
- どちらを採用するかは §10 Q-4 で決める

### 6.2 後方互換
- 旧プロジェクトファイルは新要素を持たないので、読み込み時に未存在ならスキップ
- 既存 Letter / LetterVertical は触らないので影響なし

---

## 7. 既存基盤との接続点

実装着手 (Phase 2-b) 時に詳細を確認すべき既存コードの位置:

- `LetterDesignerItemViewModel.cs` — Letter ファミリの実装パターン参照
- `LetterDesignerItemDataTemplate.xaml` — DataTemplate のひな形
- `App.xaml` の `MergedDictionaries` — DataTemplate 登録先
- `ObjectSerializer.cs` / `ObjectDeserializer.cs` — シリアライズ拡張
- ツールバー (`MainWindow.xaml` 内) — ツール追加先
- `FontFamilyEx` — フォント選択用既存型

---

## 8. テスト戦略

`boilersGraphics.Test` に追加するテスト案:

1. **モデル単体**:
   - DataGenerator の各 Type で正しい形式の文字列が生成されるか
   - Seed が同じなら出力も同じ (Reproducibility)
   - NumberSequence の Start/End/Step 組み合わせで件数が正しいか
2. **シリアライズ**:
   - 各 TextElement を含むプロジェクトファイルが保存 → 読み込みで完全復元
   - 旧形式の読み込みでクラッシュしないか
3. **パーツ機構との統合**:
   - TextElement を内部に持つパーツ定義を作成 → 公開パラメータを変更 → 各要素のプロパティが更新される
4. **既存テスト**: [[project_test_baseline]] (Phase 2-a 着手時点 1050 件) はすべて緑のまま

---

## 9. Phase 2 のサブフェーズ分割案

- **2-a**: 設計仕様書 (本ドキュメント) を確定 (§10 のオープン問題を全件確定)
- **2-b**: `TextElementBase` 抽象クラス + `MonoTextBlock` の最小実装 + ツールバー追加
- **2-c**: `DataGeneratorTextBlock` 実装 (5 種の Type すべて)
- **2-d**: `NumberSequenceBlock` 実装
- **2-e**: `TextMatrixBlock` 実装
- **2-f**: `TextOnPathBlock` 実装
- **2-g**: シリアライズ対応 + プロジェクト保存/読み込み
- **2-h**: Phase 1 のパーツ機構と統合 (Binding ターゲットとして使える)
- **2-i**: チュートリアル整備 (`docs/fui/phase2-tutorial.md`)

---

## 10. オープン問題 (要決定事項)

実装着手前に作者と合意したい設計判断:

### Q-1. `MonoTextBlock` は既存 `LetterDesignerItemViewModel` の派生 / 拡張 / 完全新規のいずれにするか?
- **案 A**: 完全新規 (`TextElementBase` を新たに作り、Letter ファミリには手を出さない)
  - メリット: 既存 Letter の挙動を一切壊さない。FUI 特化機能を素直に増やせる。
  - デメリット: 似たコードが Letter / TextElement で並ぶ可能性。
- **案 B**: 既存 `AbstractLetterDesignerItemViewModel` を拡張して共通基底化
  - メリット: 共通コード集約。
  - デメリット: 既存 Letter への影響を慎重に評価する必要がある。
- **案 C**: 既存 Letter に新プロパティだけ生やす (LineHeight / LetterSpacing 等)
  - メリット: 「モノスペーステキスト」を新クラスとして増やさない。
  - デメリット: Letter が肥大化。Letter / FUI 特化の責務が混ざる。

### Q-2. ダミーデータの Type は §3.2 の 8 種で十分か、足し引きあるか?
- 候補: Hex / Binary / IPv4 / IPv6 / UUID / Timestamp / RandomCode / LogLine
- 追加候補: MAC アドレス、Base64、Color hex (`#FF00AA`)、座標タプル (`(x, y, z)`)、ASCII art ノイズ
- 削除候補: なし (作者判断)

### Q-3. DataGenerator の Seed 仕様
- **案 A**: Seed は明示的な int (ユーザーが「Seed = 42」と入力)。同じ Seed → 同じ出力。
- **案 B**: Seed は自動 (要素生成時に乱数決定、保存時に固定)。ユーザーが「再生成」ボタンを押すと新しい Seed に振り直し。
- **案 C**: A + B のハイブリッド (デフォルトは B、必要なら明示指定に切り替え可)

### Q-4. シリアライズの方式
- **案 A**: `<DesignerItems>` 配下に `<MonoTextBlock>` 等を並べる (既存と同じ)
- **案 B**: `<TextElements>` セクションを新規追加して区別する
- **案 C**: パーツ機構と同じく、専用セクション + Id 索引方式

### Q-5. DataGenerator の再生成は同期 / 非同期どちらか?
- **案 A**: 常に同期 (UI スレッドで即時計算)。Count が小さい場合は問題なし、巨大な場合は UI ブロック懸念。
- **案 B**: Count が一定以上で非同期。タスクで生成して Dispatcher 経由で UI 更新。
- **案 C**: 常に非同期 (Reactive Stream 風)。

### Q-6. テキストマトリクスのセル内容指定 (§3.3)
- **案 A**: 単純な連番 (`r * Columns + c` の数値)
- **案 B**: DataGenerator を埋め込み (各セルが独立)
- **案 C**: 任意の文字列リストを直接指定 (改行区切り)
- **案 D**: A + B + C すべてサポート (ユーザーがモード切替)

### Q-7. テキストパスの実装方式
- **案 A**: WPF の `Path.Data` をベースに、各文字を `RotateTransform` で配置 (ジオメトリ計算は自前)
- **案 B**: 既存の `PolyBezierViewModel` を Path として利用できるようにする (パーツ機構と統合)
- **案 C**: SkiaSharp / D2D など外部ライブラリ依存 (重い → 不採用候補)

### Q-8. デフォルトフォント
- 候補: JetBrains Mono / IBM Plex Mono / Cascadia Code / Source Code Pro / Consolas / MS Gothic
- システムに無いフォントを指定したらどうフォールバック?
- ライセンス的に同梱できるフォントはあるか? (JetBrains Mono は OFL、IBM Plex は OFL、Cascadia Code は OFL、Source Code Pro は OFL、Consolas は Microsoft 同梱)

### Q-9. Phase 1 パーツ機構との統合粒度
- TextElement の各プロパティを ExposedProperty の Binding ターゲットにできる必要がある
- Binding が対応すべき型: string / int / double / Brush / Boolean (Phase 1-c の §5.2 と整合済み)
- 追加で必要な型は?
  - `DataGeneratorType` (enum) を ExposedProperty.Enum で扱う?
  - `Format string` を ExposedProperty.String で扱う?
- ExposedProperty が現状 `IsArray` のみ持つので、`Point[]` / `Double[]` 等が必要になるユースケースが Phase 2 で発生するか?

### Q-10. UI 文言・ツールバーアイコン
- ツールバーアイコンは何を使うか? (Material Design Icons 候補)
- 各要素の UI 表記 (日本語) を本文中の案 (「モノスペーステキスト」等) で問題ないか?

### Q-11. Phase 2 のスコープを Phase 1 同様に「ロードマップ」として残しつつ、最小実装 (例: MonoTextBlock + DataGenerator のみ) を先にリリースする選択肢
- **案 A**: 全項目を Phase 2 内で実装してからリリース
- **案 B**: MonoTextBlock + DataGenerator のみで一旦リリース、TextMatrix / TextOnPath / NumberSequence は Phase 2.5 として後送り
- どちらが作者の制作実感に合うか?

---

## 11. Phase 2-a 完了基準

このドキュメントが以下を満たすことをもって Phase 2-a 完了とする:

- [ ] §10 のオープン問題すべてに作者の判断が反映されている (Q-1〜Q-11 全件確定)
- [ ] §4 のデータモデル図がレビュー済み
- [ ] §6 のシリアライズ仕様が既存形式と矛盾しない
- [ ] このドキュメントが `docs/fui/phase2-text-data-elements.md` に保存されている
- [ ] 後続の Phase 2-b 以降で参照されるべき既存コード位置がリストアップされている (§7)
- [ ] Phase 1 のパーツ機構との統合点が明示されている (§5.3, §9)

---

## 12. 確定事項サマリー (Phase 2-b 実装時のクイックリファレンス)

Phase 2-a 完了時に本セクションに追記する (Phase 1-a と同じパターン)。

---

*Last updated: 2026-05-13 (ドラフト v0.1)*
*Author: Claude (for review by dhq_boiler)*
