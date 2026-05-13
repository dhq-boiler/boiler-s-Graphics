# Phase 2-a: テキスト・データ要素 設計仕様書 (確定版 v1.0)

> このドキュメントは `docs/FUI_DESIGNER_INTENT.md` Phase 2 の **サブフェーズ 2-a** に対応する設計仕様書である (確定版 v1.0)。
> §10 のオープン問題は Q-1 〜 Q-11 すべて確定済み (2026-05-13)。
> §12 が Phase 2-b 実装時のクイックリファレンス。

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

## 9. Phase 2 のサブフェーズ分割 (Q-11 採用 案 B 反映済み)

### Phase 2 (今リリース対象)
- **2-a**: 設計仕様書 (本ドキュメント) を確定 — Q-1 〜 Q-11 全件確定で **完了**
- **2-b**: `TextElementBase` 抽象クラス + `MonoTextBlock` の最小実装 + JetBrains Mono 同梱 (Q-1, Q-8) + ツールバー追加
- **2-c**: `DataGeneratorTextBlock` 実装 (8 種の Type すべて、Q-2 / Q-3 ハイブリッド Seed 含む)
- **2-d**: `NumberSequenceBlock` 実装
- **2-e**: シリアライズ対応 (`<DesignerItems>` 配下、Q-4)
- **2-f**: Phase 1 のパーツ機構と統合 (主要プロパティを ExposedProperty Binding ターゲット化、Q-9)
- **2-g**: チュートリアル整備 (`docs/fui/phase2-tutorial.md`)

### Phase 2.5 (後送り、Q-11)
- **2.5-a**: `TextMatrixBlock` 実装 (Q-6 3 モード)
- **2.5-b**: `TextOnPathBlock` 実装 (Q-7 既存 PolyBezier 利用)
- **2.5-c**: シリアライズ拡張 + チュートリアル更新

---

## 10. オープン問題 (要決定事項)

実装着手前に作者と合意したい設計判断:

### Q-1. `MonoTextBlock` は既存 `LetterDesignerItemViewModel` の派生 / 拡張 / 完全新規のいずれにするか? ✅ **確定**
- **採用: 案 A (完全新規 / `TextElementBase` を新たに作る)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: 既存 Letter の挙動を一切壊さない方針 (§2.1 既存コードへの敬意) と一致。Letter ファミリは触らず、FUI 特化のテキスト系は新基底で素直に分離する。
- **不採用**: 案 B (AbstractLetter 共通基底化) — 既存 Letter への影響評価が重い / 案 C (Letter に直接プロパティ追加) — Letter が肥大化し責務が混ざる

### Q-2. ダミーデータの Type は §3.2 の 8 種で十分か? ✅ **確定**
- **採用: 8 種 (Hex / Binary / IPv4 / IPv6 / UUID / Timestamp / RandomCode / LogLine) で確定**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: スコープを最小に抑えてリリースまで早く到達するため。追加 Type (MAC / Base64 / Color hex / 座標タプル / ASCII art ノイズ) は Phase 2.5 以降で必要に応じて足す。

### Q-3. DataGenerator の Seed 仕様 ✅ **確定**
- **採用: 案 C (ハイブリッド)**
- 確定日: 2026-05-13 (作者判断)
- **具体仕様**:
  - デフォルトは **自動 Seed** (要素生成時に乱数決定、ファイルに保存して再現)
  - UI に「Seed をロックして明示指定する」モード切り替えを用意
  - 「再生成」ボタンは常に押せる (自動モードなら新しい Seed、明示モードなら入力値で再生成)
- **採用理由**: 「見た目をキープしたい」ケースと「気軽にダミーデータを試したい」ケースの両方に対応できる。

### Q-4. シリアライズの方式 ✅ **確定**
- **採用: 案 A (`<DesignerItems>` 配下に並べる)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: 既存の Rectangle / Ellipse / Letter と同じ扱いになり、ObjectSerializer / ObjectDeserializer の拡張が最小。Letter ファミリと一貫性のある形式。

### Q-5. DataGenerator / NumberSequence の再生成は同期 / 非同期どちらか? ✅ **確定**
- **採用: 案 A (常に同期)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: Count が小さい間はシンプルでチラつきなし。Phase 5+ で大量要素アニメーションをやる段階でボトルネックが出たら他案に振り直す方針。コードパスを最初から二重化しない。

### Q-6. TextMatrix のセル内容指定 ✅ **確定**
- **採用: 案 D (3 モード全部サポート)**
- 確定日: 2026-05-13 (作者判断)
- **具体仕様**: 連番モード / DataGenerator 埋め込みモード / 任意文字列リストモードの 3 つを UI で切り替え可能。各モードは独立しているため Phase 2.5 内で段階実装可能。
- **採用理由**: FUI の表現幅が一番広くなる。3 モードどれも独立なので、実装は重くなるが切り出しやすい。

### Q-7. TextOnPath の実装方式 ✅ **確定**
- **採用: 案 B (既存 `PolyBezierViewModel` を Path として利用)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: 「パスを描いて → そのパスにテキストを乗せる」という自然なフローになる。Phase 1 のパーツ機構とも統合しやすい。外部ライブラリ依存も避けられる (§2.1 既存コード尊重)。
- **不採用**: 案 A (自前計算) — UX が重複しがち / 案 C (SkiaSharp / D2D) — §2.1 既存コード尊重原則に反する

### Q-8. デフォルトフォント ✅ **確定**
- **採用: JetBrains Mono を同梱、デフォルトに設定**
- 確定日: 2026-05-13 (作者判断)
- **ライセンス**: OFL — リポジトリ同梱可
- **フォールバックチェーン**: JetBrains Mono → Cascadia Code → Consolas → MS Gothic
- **採用理由**: エンジニアリング色の強いグリフ、リガチャ豊富で FUI 進化計画の世界観に合う。
- **実装上の注意**:
  - フォントファイル (`.ttf` or `.otf`) を `boilersGraphics/Fonts/` に配置し、`pack://application:,,,/boilersGraphics;component/Fonts/#JetBrains Mono` で参照
  - インストール済みフォントを優先しないように、リソースから埋め込みフォントを使う設計
  - 既存 `FontFamilyEx` の選択肢にも追加

### Q-9. Phase 1 パーツ機構との統合粒度 ✅ **確定**
- **採用: 主要プロパティのみ ExposedProperty で公開可能**
- 確定日: 2026-05-13 (作者判断)
- **公開可能プロパティ (Phase 2 スコープ)**:
  - MonoTextBlock: Text / FontSize / Foreground / Background / LetterSpacing
  - DataGenerator: Seed / Count / Separator
  - NumberSequence: Start / End / Step / Format / Separator
- **非公開プロパティ (Phase 2 スコープ)**: Type / FontFamily / Layout / Direction / Side / Rotation 等の「骨格」部分
- **採用理由**: Phase 1 の 8 型 (Double / Int / Boolean / Point / Color / Brush / String / Enum) でカバー可能。ExposedProperty 側の型追加が不要でスコープがクリーン。
- **影響**: Phase 1 の `IsArray` フラグはそのまま使う。Point[] / Double[] 等の配列ユースケースは Phase 2 では発生しない。

### Q-10. UI 文言 ✅ **確定**
- **採用: 仕様書の 5 つの名前をそのまま UI 表記に**
  - 「モノスペーステキスト」
  - 「データジェネレータ」
  - 「テキストマトリクス」 (Phase 2.5)
  - 「テキストパス」 (Phase 2.5)
  - 「数値列」
- 確定日: 2026-05-13 (作者判断)
- **ツールバーアイコン**: Material Design Icons から選定 (Phase 2-b 実装時に確定)
- **採用理由**: 既存 boilersGraphics の日本語 UI と一貫。FUI 制作者にも意味が伝わる。

### Q-11. Phase 2 のリリース粒度 ✅ **確定**
- **採用: 案 B (最小実装で一旦リリース)**
- 確定日: 2026-05-13 (作者判断)
- **Phase 2 スコープ**: MonoTextBlock + DataGenerator + NumberSequence の 3 要素
- **Phase 2.5 スコープ (後送り)**: TextMatrix + TextOnPath
- **採用理由**: develop ブランチに長期滞在を避ける。「数値モノ表現」(NumberSequence) は FUI の肝なので Phase 2 内に含める。TextMatrix / TextOnPath はその次のリリースで安定して足す。

---

## 11. Phase 2-a 完了基準

このドキュメントが以下を満たすことをもって Phase 2-a 完了とする:

- [x] §10 のオープン問題すべてに作者の判断が反映されている (Q-1〜Q-11 全件確定)
- [x] §4 のデータモデル図がレビュー済み (Q-1 で `TextElementBase` 新規確定 / Q-9 で公開プロパティ確定)
- [x] §6 のシリアライズ仕様が既存形式と矛盾しない (Q-4 で `<DesignerItems>` 配下に決定)
- [x] このドキュメントが `docs/fui/phase2-text-data-elements.md` に保存されている
- [x] 後続の Phase 2-b 以降で参照されるべき既存コード位置がリストアップされている (§7)
- [x] Phase 1 のパーツ機構との統合点が明示されている (§5.3, §9, Q-9)

**Phase 2-a 完了。Phase 2-b (実装着手) に進む準備が整った。**

---

## 12. 確定事項サマリー (Phase 2-b 実装時のクイックリファレンス)

| 項目 | 確定内容 |
|---|---|
| 実装方針 | 完全新規 `TextElementBase` 派生 (既存 Letter ファミリに手を出さない) |
| 公開パラメータ型 | Phase 1 の 8 型をそのまま流用 (型追加なし) |
| DataGenerator Type | 8 種 (Hex / Binary / IPv4 / IPv6 / UUID / Timestamp / RandomCode / LogLine) |
| Seed 仕様 | ハイブリッド (デフォルト自動、UI でロックして明示指定モードに切替可) |
| シリアライズ | `<DesignerItems>` 配下に `<MonoTextBlock>` 等として並べる |
| 再生成方式 | 常に同期 (Phase 5+ でボトルネック出たら振り直し) |
| TextMatrix セル内容 | 3 モード (連番 / DataGenerator 埋め込み / 任意文字列) すべて対応 (Phase 2.5) |
| TextOnPath 実装方式 | 既存 `PolyBezierViewModel` を Path として利用 (Phase 2.5) |
| デフォルトフォント | JetBrains Mono 同梱 (OFL)、フォールバック Cascadia Code → Consolas → MS Gothic |
| パーツ統合粒度 | 主要プロパティのみ ExposedProperty 公開可能 (骨格部分は非公開) |
| UI 文言 | モノスペーステキスト / データジェネレータ / テキストマトリクス / テキストパス / 数値列 |
| Phase 2 スコープ | MonoTextBlock + DataGenerator + NumberSequence (TextMatrix / TextOnPath は Phase 2.5) |

---

*Last updated: 2026-05-13 (確定版 v1.0)*
*Reviewer: dhq_boiler*
