# boilersGraphics → FUI Designer 進化計画 (意図ファイル)

> このドキュメントは、`boilersGraphics` を **FUI (Fictional User Interface) / モニターグラフィックス制作ツール** へ進化させるための意図と方針を Claude Code に伝えるためのものです。
> 個別の実装タスクではなく、**作者 (dhq_boiler) の意図・哲学・ロードマップ** を共有することが目的です。
> 個別タスクは別途 issue / 作業指示として切り出します。

---

## 1. 背景と動機

### 1.1 きっかけ
- Richard Falcema 氏 (falcema.com) のような、Linux の `chrony` や CRT、多面体といった **実在の技術・科学概念をモチーフにした FUI・モニターグラフィックス作品** に強く惹かれている。
- 同種の制作は通常 Adobe Illustrator + After Effects + Cinema 4D を用いるが、これらのライセンスは保有していない。
- 一方、自分は WPF アプリ `boilersGraphics` の作者であり、内部構造を把握している。
- **「持っていないツールを買う」より「持っているツールを進化させる」** ほうが、思想的にも経済的にも合理的。

### 1.2 なぜ boilersGraphics か
- すでにベクター描画、レイヤー、ViewModel ベースのプロパティ編集など、グラフィックエディタとしての骨格が存在する。
- MVVM + Prism + R3 (Rx) の構成は、FUI 制作に必要な「パラメトリックな図形生成」「データドリブンな視覚化」と相性が良い。
- C# / .NET は数値計算・幾何処理・SVG/PNG エクスポートに十分な能力を持つ。

### 1.3 ゴールイメージ
- **「エンジニアが自分の理解している技術を FUI として視覚化できる」** 専用ツール。
- 単なる Illustrator クローンを目指さない。FUI/サイバーパンク/モニターグラフィックス特化の機能を持つことで、「Illustrator では面倒な作業がこれ1つで完結する」状態にする。
- **本ツール最大の独自性**: 制作した FUI アニメーションを **WPF / .NET MAUI の XAML としてエクスポート** でき、作者自身が開発する .NET 系アプリ (STREAM DIRECTOR、MHWildsDataCollector 等) に直接組み込める。Illustrator / After Effects では到達不可能な領域。
- 想定する作品例:
  - Rails ActionCable の Pub/Sub 状態を可視化する HUD
  - DirectX Hook の内部処理フロー図
  - WASAPI / VST3 オーディオチェーンの解剖図
  - Kamal デプロイのプロセスモニター風グラフィック
  - Solid Queue のジョブ実行ダッシュボード風 FUI
  - **STREAM DIRECTOR / MHWildsDataCollector など自身の WPF アプリへの UI 部品** (XAML 出力経由)
  - **将来の .NET MAUI アプリへの FUI 風 UI 部品**

---

## 2. 基本方針 (Claude Code への原則)

### 2.1 既存コードへの敬意
- **既存の図形描画 / レイヤー / SelectionTool / SnapPoint 等の基盤コードは可能な限り壊さない**。
- 新機能は基本的に **拡張** として追加する (新しい ViewModel、新しい Tool、新しいオプションパネル)。
- 既存の名前空間 (`boilersGraphics.ViewModels`, `boilersGraphics.Views`, etc.) の命名規則・配置に従う。
- 既存の MVVM (Prism) / Rx (R3) パターンを徹底する。新たに別パターンを混ぜ込まない。

### 2.2 段階的に進める
- 一度に大改造しない。Phase 単位でブランチを切り、レビュー可能な粒度で PR を作る。
- 各 Phase の完了基準を明確にし、達成してから次の Phase へ。
- **「動くこと」と「既存機能を壊さないこと」を毎フェーズで確認**。テストがあるものは通すこと。

### 2.3 不可逆な変更は事前確認
- 既存クラスのリネーム、削除、シグネチャ変更は事前に意図を共有して確認を取る。
- DB / 設定ファイル / プロジェクトファイル形式の破壊的変更は特に慎重に。
- 過去バージョンのファイル (`.bg` などプロジェクトファイル形式があれば) との後方互換を意識する。

### 2.4 「便利だから」で機能を増やさない
- FUI 制作という目的から外れる機能 (例: 一般的なドローツールに普通あるが FUI には不要なもの) は安易に追加しない。
- 追加する前に「これは FUI のどの表現に貢献するか?」を意図ファイル or issue に書く。

### 2.5 報告は正直に
- 「全部できました」と言わない。やった部分・やっていない部分・うまく動かない部分を明確に分けて報告する。
- 既知の不具合や未対応箇所がある場合は隠さない。

### 2.6 完成済みテンプレートではなく、「パラメトリックな部品化」をサポートする機能を提供する
- 本ツールは完成済みの FUI デザイン (例: 「サイバーパンクHUD テンプレ」「アンバーCRT 風コックピット計器」など) を **配布しない**。
- 代わりに、ユーザーが自分で描いた図形群を **「部品 (Component)」として登録** し、その部品に **公開パラメータ (Properties)** を定義して、配置のたびに数値で形・色・本数などを変えられるようにする **「パラメトリック化支援機能」** を提供する。
- 体験イメージ:
  1. ユーザーが「同心円リング 5 本 + 目盛り 30 個」を自分で描く
  2. それを選択して「部品化」する
  3. 部品の中で「半径」「リング数」「目盛り数」「色」を公開パラメータとして定義する
  4. その部品をキャンバスに何度でも配置でき、配置ごとにパラメータを変えられる
- 参考になる先行例: Figma の Component + Properties、After Effects のヌル + スライダー制御 + エクスプレッション、Blender のジオメトリノード + Input Sockets、WPF の UserControl + DependencyProperty、Unity の Prefab + Public フィールド。
- **思想**: FUI デザイン本体は **ユーザーが手で組み上げる行為** であり、テンプレート配布はその創造性をスポイルする。本ツールが提供するのは「組み上げを楽にする道具」と「再利用を可能にする部品化機構」のみ。
- 組み込みの「円環ジェネレータ」「目盛りジェネレータ」のような **既製パラメトリック図形** も、この機構の上に乗せる形で実装する (= 同じ仕組みでサンプルとして提供される)。専用コードパスを別に持たない。

---

## 3. 進化のためのロードマップ (Phase 構成)

> 各 Phase の詳細は別 issue で具体化する。ここでは **全体像と順序** のみ示す。

### Phase 0: 棚卸しと土台整備 (準備)
- 現状の `boilersGraphics` のアーキテクチャを再確認 (どの図形タイプがあり、どこで描画され、どこで保存されているか)。
- FUI 機能追加に向けた拡張ポイント (新しい Tool を足す場所、新しい図形タイプを足す場所) を文書化する。
- ライブラリ依存関係の整理 (削除可能なもの、アップデートが必要なもの)。
- 進化を見据えた CHANGELOG / README の更新。

### Phase 1: パラメトリックな部品化機構 (本ツールの中核機能) 🌟
方針 2.6 に基づき、Phase 1 の主軸は **「ユーザー定義パラメトリック部品 (Parametric Component)」の機構そのもの** の設計と実装とする。組み込みのジェネレータ図形を先に作るのではなく、まず **メタ機能** を作る。

#### 1.1 機能の骨子
- **部品化 (Componentize)**: 既存の図形を 1 つ以上選択し、「部品として登録」する操作。
- **公開パラメータ (Exposed Properties)**: 部品内部の図形プロパティ (位置、サイズ、色、線種、本数、角度、etc.) のうち、外部から書き換えたいものを **公開パラメータ** として登録する。
- **インスタンス配置**: 登録した部品をキャンバスに何度でも配置できる。配置ごとに公開パラメータの値を変えると、その部品インスタンスの中の図形がそれに従って再生成される。
- **編集の双方向性 (要検討)**: 部品の中身を編集したとき、既存の配置インスタンスにどう反映するか (即時反映 / 同期解除 / バリアント) は設計判断として別途決める。
- **保存**: 部品定義はプロジェクトファイルの一部として保存される (プロジェクトをまたいだ再利用は Phase 1 ではスコープ外)。

#### 1.2 サブフェーズ
1. **1-a: 設計仕様書** — `docs/fui/phase1-parametric-components.md` を作成し、データモデル・UI フロー・編集動作・シリアライズ仕様を確定する。
2. **1-b: データモデル実装** — `ComponentDefinition`, `ComponentInstance`, `ExposedProperty` 等のモデル/ViewModel を追加。既存の `DesignerItemViewModelBase` 派生として実装する。
3. **1-c: 部品化 UI** — 選択中の図形群を「部品化」するメニュー/ショートカット、公開パラメータ定義ダイアログ、部品インスタンス配置ツール、プロパティパネルでのパラメータ編集 UI を追加。
4. **1-d: シリアライズ対応** — `ObjectSerializer` / `ObjectDeserializer` に部品定義と部品インスタンスのシリアライズを追加。既存プロジェクトファイルとの後方互換を維持。
5. **1-e: 既存基礎図形のサンプル化** — 元の Phase 1 で予定していた以下のジェネレータを、**この機構の上に乗せたサンプル部品** として実装する (専用コードパスを増やさない):
   - 円環 / 同心円
   - 目盛り (Tick)
   - 正多角形 / 星形
   - グリッド / ドットマトリクス
   - ラベル付き寸法線 / 引出し線

#### 1.3 完了基準
- ユーザーが任意の図形群を選択 → 部品化 → パラメータ定義 → 別の場所に配置 → パラメータ変更で形が変わる、までが UI 操作で完結する。
- プロジェクトファイルに部品定義を保存し、再オープン後も部品とインスタンスが正しく復元される。
- 既存テストがすべて緑 ([[project_test_baseline]] を維持)。新規追加機能には対応する単体テストを追加。
- 1.2-e で挙げたサンプル部品が 1 つ以上、機構の上で動作することを確認 (PoC として円環 or 目盛りのどちらかを最初に実装)。

### Phase 2: テキスト・データ要素 (FUI の魂) ✅ 完了
FUI らしさの肝は「微細な技術テキスト」と「データっぽい数値・コード」が大量に配置されていること。

#### Phase 2 で実装した要素 (確定版 v1.0 Q-11 案 B スコープ)
- **モノスペーステキスト (`MonoTextBlock`)**: JetBrains Mono 同梱 (OFL) + Cascadia Code → Consolas → MS Gothic フォールバックチェーン
- **データジェネレータ (`DataGeneratorTextBlock`)**: 8 種 (Hex / Binary / IPv4 / IPv6 / UUID / Timestamp / RandomCode / LogLine)、ハイブリッド Seed (Q-3 案 C)
- **数値列 (`NumberSequenceBlock`)**: Start / End / Step / Format / Separator / Direction (Horizontal / Vertical / Grid)
- Phase 1 のパーツ機構と統合 (`RenderedItems` 経由の値伝搬機構、ExposedProperty 公開可能プロパティを Q-9 で確定)

#### Phase 2.5 で実装した要素 ✅ 完了
- **テキストマトリクス (`TextMatrixBlock`)**: 3 モード (連番 / DataGenerator 埋め込み / 任意文字列リスト)
- **テキストパス (`TextOnPathBlock`)**: 既存 `PolyBezierViewModel` を Path として利用 (Q-7 案 B)

#### 参考ドキュメント
- 設計仕様書: [`fui/phase2-text-data-elements.md`](./fui/phase2-text-data-elements.md)
- Phase 2 チュートリアル: [`fui/phase2-tutorial.md`](./fui/phase2-tutorial.md)
- Phase 2.5 チュートリアル: [`fui/phase2-5-tutorial.md`](./fui/phase2-5-tutorial.md)

### Phase 3: 接続線・関係表現 ✅ 完了
要素同士を「配線」して関係を示すのは FUI の重要言語。
- **L字 / カギ型コネクタ** (`OrthogonalConnectorViewModel`): Auto/HFirst/VFirst/Manual の 4 ルーティング + CornerRadius
- **ベジエコネクタ** (`AnchorBezierConnectorViewModel`): Anchor 接続対応の完全新規型 (既存 `BezierCurveViewModel` には触らない、Q-1 案 B)
- **アンカー (snap target)**: 暗黙 9 点 + 明示 `AnchorViewModel` の 2 系統。コネクタが吸着し、図形移動に追従
- **ノード - エッジ風グラフモード**: `IsNode` フラグ + 選択時の関連コネクタ強調表示 (EdgeThickness × 1.5 + EdgeBrush 反転)
- **Phase 1 パーツ機構との統合**: 主要プロパティを ExposedProperty で公開可能 (Q-9 案 A)
- **吸着距離設定**: グローバル設定 (Q-7 案 C)

#### 関連ドキュメント
- 設計仕様: [`fui/phase3-connectors.md`](./fui/phase3-connectors.md) (Q-1 〜 Q-11 確定版)
- チュートリアル: [`fui/phase3-tutorial.md`](./fui/phase3-tutorial.md)

### Phase 4: スタイル・テーマ・カラー ✅ 完了
FUI は「世界観」を統一することで完成度が決まる。
- **カラーパレット管理** (`ColorPalette`): 「Bladerunner」「Matrix」「MedicalBlueWhite」「AmberCrt」の組込 4 テーマ。固定 5 セマンティックスロット (primary/accent/warning/info/background、Q-2 案 A)
- **線種ライブラリ** (`LineStyle`): Solid/Dash/Dot/DashDot/LongDash/Stepped の組込 6 種。完全新規型で StrokeDashArray + StrokeLineJoin + 任意グローを束ねる (Q-5 案 B)
- **グロー / ブルーム風エフェクト** (簡易): `SelectableDesignerItemViewModelBase` に GlowRadius/Intensity/Color を持たせ、WPF `DropShadowEffect` で擬似グロー化 (Q-7 案 A MVP)。OpenCV ガウシアン + 加算合成版は Phase 4.5 後送り
- **エフェクト適用は破壊的にしない** (Q-9 案 A): 元の EdgeBrush/FillBrush は保持、Glow は別プロパティで保持
- **テーマ切替の範囲選択** (Q-10 案 C): SelectedItems / ActiveLayer / EntireProject から選択可能
- **パーツ機構統合** (Q-11 案 A): Glow 3 プロパティを `ExposedProperty` で公開可能化

#### 関連ドキュメント
- 設計仕様: [`fui/phase4-styles-themes.md`](./fui/phase4-styles-themes.md) (Q-1 〜 Q-12 確定版)
- チュートリアル: [`fui/phase4-tutorial.md`](./fui/phase4-tutorial.md)

### Phase 5: モーション / アニメーション (After Effects 代替の最初の一歩)
ここから先は野心的。最低限のタイムラインベースアニメーション。
- **キーフレームベースのプロパティアニメーション** (位置、回転、不透明度、線の長さなど)
- **トリムパス (パスの一部だけを描画) アニメーション**: ローディング表現に必須
- **数値カウンタアニメーション**: 0 → 1234.56 などをイージング付きで表示
- **書き出し**: PNG 連番、MP4 (FFmpeg 連携)、Lottie JSON (可能なら)

### Phase 5.5: XAML アニメーション出力 (WPF / .NET MAUI 連携) 🌟 重点
**`boilersGraphics` の最大の独自性となる機能**。

作者は WPF / .NET MAUI で実アプリを開発しているため、`boilersGraphics` で作った FUI アニメーションを **そのまま自身のアプリへ組み込める形で出力できる** ことに極めて高い価値がある。これは Illustrator / After Effects では実現困難な領域であり、本ツールの差別化要因とする。

#### 5.5.1 出力ターゲット
1. **WPF (System.Windows.Media.Animation)**
   - `Storyboard` + `DoubleAnimation` / `ColorAnimation` / `PointAnimation` / `DoubleAnimationUsingKeyFrames`
   - パスアニメーションは `PathGeometry` + `DoubleAnimationUsingPath`
   - イージングは `CubicEase`, `ElasticEase`, `BackEase` 等の `EasingFunctionBase` 派生
   - 図形は `Path`, `Ellipse`, `Rectangle`, `Polygon` 等の WPF Shape にマップ
   - ResourceDictionary 形式で再利用可能な形に出力できると望ましい

2. **.NET MAUI (Microsoft.Maui.Controls Animation)**
   - `Animation` クラスベースのコード型アニメーション、または XAML での `VisualStateManager` / `Triggers`
   - MAUI には WPF の Storyboard 相当が存在しないため、**コードビハインド (.xaml.cs) + XAML の組み合わせ** での出力を基本とする
   - もしくは CommunityToolkit.Maui.Animations の `BaseAnimation` 派生としての出力も検討
   - 図形は `Microsoft.Maui.Controls.Shapes` 名前空間 (`Path`, `Ellipse` 等) にマップ

3. **共通仕様 (両ターゲット共通)**
   - 出力は **単一の UserControl / ContentView として完結する** ことを目指す。コピペで使える形。
   - 名前空間、クラス名、x:Name はユーザーが指定可能。
   - 出力 XAML は人間が読める整形済み (適切なインデント、コメント付き)。
   - 依存ライブラリ無しで動くプレーンな XAML を第一目標とする。

#### 5.5.2 マッピング設計の指針
- 内部の `boilersGraphics` モデル (図形 + キーフレーム + イージング) を、**ターゲット非依存の中間表現 (IR)** に一度落とす。
- IR から WPF / MAUI それぞれのエクスポーターに分岐する設計とする (Strategy パターン)。
- 将来 Avalonia / Uno Platform / WinUI 3 への拡張余地を残す。

```
[boilersGraphics 内部モデル]
        ↓
[Animation IR (中間表現)]
        ↓
   ┌────┴────┐
[WPF]    [MAUI]    [将来: Avalonia / WinUI 3]
```

#### 5.5.3 出力の例 (イメージ)
WPF 出力イメージ:
```xml
<UserControl x:Class="MyApp.FuiClock" ...>
  <UserControl.Resources>
    <Storyboard x:Key="RotateRing">
      <DoubleAnimation Storyboard.TargetName="Ring"
                       Storyboard.TargetProperty="(UIElement.RenderTransform).(RotateTransform.Angle)"
                       From="0" To="360" Duration="0:0:10"
                       RepeatBehavior="Forever" />
    </Storyboard>
  </UserControl.Resources>
  <Grid>
    <Ellipse x:Name="Ring" .../>
  </Grid>
</UserControl>
```

#### 5.5.4 制約と注意点
- WPF と MAUI ではアニメーションのプロパティ名・適用方法が異なる (例: `RenderTransform` の扱い、`Path.Data` の Mini-Language 互換性)。**完全互換は目指さず、それぞれのターゲットで自然な形** に出力する。
- 一部の FUI 表現 (大量パーティクル、複雑なグロー) は XAML だけでは表現困難。その場合は**エクスポート時に警告を出し、代替表現 (PNG 連番、Lottie) を推奨**する。
- MAUI のアニメーション機能は WPF より制限が多いため、**WPF 出力を完全版、MAUI 出力を簡易版** という位置付けで開発を進める。

#### 5.5.5 段階的な実装順序
1. **静的 XAML 出力 (アニメーション無し)** — まず図形だけを WPF / MAUI の XAML として出力できるようにする
2. **WPF Storyboard 出力 (単純なプロパティアニメーション)** — 位置・回転・不透明度のキーフレームから Storyboard を生成
3. **WPF Path アニメーション (TrimPath 相当)** — `StrokeDashArray` + `StrokeDashOffset` のアニメーションで線描画アニメを実現
4. **WPF 複合アニメーション** — 複数 Storyboard の合成、イージング対応
5. **MAUI 静的 XAML 出力**
6. **MAUI コードビハインド型アニメーション出力**

### Phase 6: エクスポート / 連携
- **SVG エクスポート** (まだ無ければ): Web や他ツールへの橋渡し
- **PNG エクスポート (高解像度・透過対応)**
- **アニメーション書き出し**: PNG 連番 / MP4 / WebM / GIF / Lottie
- **XAML エクスポート** (Phase 5.5 と連動): WPF / MAUI アプリへの直接組み込み用
- **クリップボード経由でのコピー** (他アプリへ貼り付け)

### Phase 7: スクリプタビリティ (将来構想)
将来的に、**C# Scripting (Roslyn) または独自 DSL** で図形をプログラマブルに生成できる窓口を設ける。
- 「Processing / p5.js のような感覚で、コードから図形を生成」できると FUI 制作の幅が桁違いに広がる。
- 例: `for (int i = 0; i < 360; i += 5) DrawTick(center, radius, angle: i, length: 8);`

---

## 4. 非機能要件

### 4.1 パフォーマンス
- ジェネレータで大量の図形 (1万要素クラス) を生成しても UI が破綻しないこと。
- 描画は WPF の `DrawingVisual` / `Geometry` を優先的に使い、`Shape` 派生を大量に貼らない設計を検討。

### 4.2 操作性
- パラメトリック要素は **後から数値を変えられる**。再生成しなくても良い。
- スナップ・ガイド・整列は既存基盤を活用しつつ、FUI 特有の極座標スナップ (角度刻み) を追加検討。

### 4.3 ファイル形式
- 既存プロジェクトファイル形式があれば後方互換を維持。新要素はフォーマットを拡張する形で追加。

### 4.4 国際化
- UI 言語は日本語優先 (既存に合わせる)。
- 文字コードは UTF-8、改行は CRLF (Windows プロジェクトのため)。

---

## 5. 参考リソース (作者の頭の中)

- Richard Falcema: https://falcema.com/works/ (CHRONY, TUBEAGE, HEDRON 等)
- HUDS+GUIS: https://www.hudsandguis.com/ (FUI 業界リソース)
- Territory Studio, Cantina Creative, Ash Thorp, GMUNK, Jayse Hansen の作品群
- 書籍: 『FUI: How to Design User Interfaces for Film and Games』(Jono Yuen)

これらは「目指す表現の方向性」の参照点。コードや仕様の引き写しではなく、**美意識の共有** のために掲示する。

---

## 6. Claude Code への具体的なお願い

このファイルを読んだ上で、最初のアクションとして以下を実行してほしい:

1. **このファイル `FUI_DESIGNER_INTENT.md` の内容を理解した旨を、自分の言葉で短く要約して報告する** (取り違いがないか確認したい)。
2. **現状の `boilersGraphics` リポジトリを軽くスキャンし、Phase 0 (棚卸し) の出発点として以下を出す**:
   - 既存の図形タイプ (Rectangle, Ellipse, Polygon, ... 等) の一覧と、それぞれがどのファイルで定義されているか
   - 新しい図形タイプを追加する場合の「追加すべき場所」のテンプレート的な手順 (どのクラスを継承し、どの ViewModel を作り、どの ToolBar に登録するか)
   - 現在の依存ライブラリ (Prism, ReactiveProperty などのバージョン)
   - 既存のテスト構成 (もしあれば)
3. **不明点・前提確認したい点があれば必ず先に質問する**。勝手に推測で進めない。

具体的な Phase 1 以降の実装着手は、上記の棚卸しが完了し、進め方の方針を作者と合意してからとする。

---

## 7. このファイルの位置づけと運用

- このファイルは **生きたドキュメント** であり、Phase が進むごとに更新する。
- 大きな方針転換 (例: 「Phase 5 のモーションは諦めて Lottie 出力に振る」など) があれば、変更点と理由を追記する。
- Claude Code は、新しい作業に取り掛かる前にこのファイルを参照すること。
- 個別タスクの詳細仕様は、このファイルではなく `docs/fui/` 以下や issue で管理する。

---

*Last updated: 2026-05-14 (Phase 4 完了 / phase4-tutorial.md 追加)*
*Author: dhq_boiler*
