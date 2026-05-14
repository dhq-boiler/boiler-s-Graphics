# Phase 5-a: モーション / アニメーション 設計仕様書 (確定版 v1.0)

> このドキュメントは `docs/FUI_DESIGNER_INTENT.md` Phase 5「モーション / アニメーション」に対応する設計仕様書 (確定版 v1.0) である。
> §10 のオープン問題は Q-1 〜 Q-12 すべて確定済み (2026-05-14、作者判断)。
> §12 が Phase 5-b 実装時のクイックリファレンス。

---

## 1. 概要

### 1.1 目的
FUI / モニターグラフィックスは静的画像だけでは完結しない。**「時間軸の上で要素が動く / 数値が刻々と更新される / 線がスキャンするように描画される」** という時間表現が、FUI の説得力の半分を担う。Phase 5 では、After Effects 相当のキーフレームベースのプロパティアニメーションを `boilersGraphics` に組み込み、Phase 1 〜 4 で作った静的 FUI に時間軸を与える。

### 1.2 この機能が満たすべき体験
1. **タイムラインベースの編集** — シーン (= CanvasPage) ごとに時間軸を持ち、ユーザーは現在時刻 (Now) を進めながら各プロパティに「この時刻でこの値」とキーフレームを打てる。
2. **任意プロパティのアニメーション** — DesignerItem の主要プロパティ (位置、回転、不透明度、線種) と、Phase 1 で定義した **ExposedProperty (公開パラメータ)** の双方をアニメーション対象にできる。
3. **イージング** — 線形だけではなく、CubicEase / ElasticEase / BackEase など WPF 標準互換のイージングをキーフレーム間に適用できる。
4. **トリムパス (パス描画アニメ)** — ロード演出に頻出する「線が端から徐々に伸びる」表現を、専用プロパティ `DrawProgress` で簡単に作れる。
5. **数値カウンタ** — Phase 2 の `DataGeneratorTextBlock` / `NumberSequenceBlock` の Value プロパティをキーフレーム化することで、`0 → 1234.56` の数値カウンタ演出を作れる。
6. **再生 / 停止 / スクラブ** — タイムライン上でプレビュー再生・一時停止・任意時刻ジャンプ・ループ・再生範囲 (In/Out) 指定ができる。
7. **PNG 連番書出** — 指定範囲をフレーム単位でレンダリングし、PNG 連番として書き出せる (Phase 5-f)。
8. **Phase 5.5 (XAML 出力) のための中間表現 (IR) を持つ** — Phase 5 のデータモデルは WPF Storyboard / MAUI / Lottie 等への変換器が読み込みやすい IR として設計する。

### 1.3 非スコープ (Phase 5 では扱わない)
- **MP4 / WebM 書出** — Phase 6 (`docs/FUI_DESIGNER_INTENT.md` §Phase 6) に後送り。Phase 5-f は PNG 連番のみ。
- **Lottie JSON 書出** — Phase 6 後送り。
- **WPF / MAUI XAML 出力** — Phase 5.5 (`docs/FUI_DESIGNER_INTENT.md` §Phase 5.5) として別フェーズ。Phase 5 では IR 設計までで止める。
- **オーディオトラック / SFX** — Phase 7 以降。
- **3D 変換 (Transform3D) のアニメーション** — Phase 5 では 2D の `RenderTransform` のみ。
- **エクスプレッション (After Effects の式言語相当)** — Phase 7 以降のスクリプタビリティに統合する形を想定。Phase 5 では純粋なキーフレーム + イージングのみ。
- **物理シミュレーション (Bounce / Spring 物理)** — `BounceEase` / `ElasticEase` で代替可能なので Phase 5 では別実装しない。
- **マルチカメラ / ビューポートアニメーション** — Phase 8 以降。
- **動的テーマトランジション** (Bladerunner → Matrix を時間遷移) — Phase 4 で非スコープ宣言済み。Phase 5 でも基本は静的適用とするが、テーマ色を直接キーフレーム化することは Q-1 案 B で可能。

### 1.4 Phase 5 のスコープに含むもの (Q-1 案 B + Q-11 案 A 反映)
- **`Timeline` モデル** — シーン (CanvasPage) ごとの時間軸、開始 / 終端 / FPS / 再生レンジ。
- **`Keyframe<T>` モデル** — 時刻 (秒) + 値 + 補間タイプ + イージング関数。
- **`AnimationTrack` モデル** — 1 つの「対象オブジェクト × プロパティ」に対するキーフレームの並び。
- **`PropertyRef` モデル** — アニメーション対象を「DesignerItem ID + プロパティ名」で参照する不変キー (IR の核)。
- **`Interpolator` (pure helper)** — 型ごとの補間ロジック (Double / Int / Color / Point / Brush / String / Enum)。
- **`EasingFunction` 群** — WPF 標準互換 (`LinearEase`, `CubicEase`, `QuadraticEase`, `ElasticEase`, `BackEase`, `BounceEase`, `CircleEase`, `ExponentialEase`, `PowerEase`, `QuarticEase`, `QuinticEase`, `SineEase`) + `EasingMode` (`EaseIn` / `EaseOut` / `EaseInOut`)。
- **タイムラインペイン UI** — シーン下部に時間軸 + 各トラックを表示、キーフレーム追加 / 編集 / 削除 / ドラッグ移動。
- **再生エンジン** — `Now : double` を Subscribe し、各トラックの補間値を該当プロパティへ書き戻す。
- **PNG 連番書出** — 開始 / 終端時刻 / FPS 指定で各フレームを `BitmapSource` 化して保存。
- **シリアライズ** — `.bg` プロジェクトファイルに `<Timeline>` セクションを追加。
- **テスト** — `Interpolator` / `EasingFunction` / `Timeline` / `AnimationTrack` / シリアライズの単体テスト追加。`project_test_baseline` (1424) を維持しつつ純増。

---

## 2. 用語定義

| 用語 | 意味 | UI 表記 |
|---|---|---|
| **タイムライン (Timeline)** | シーンごとの時間軸。開始 / 終端 / FPS / 再生レンジを束ねる単位 | 「タイムライン」 |
| **トラック (Track)** | 「対象オブジェクト × プロパティ」1 対 1 のキーフレーム列 | 「トラック」 |
| **キーフレーム (Keyframe)** | 時刻 (秒) + 値 + イージングの組 | 「キーフレーム」 |
| **イージング (Easing)** | キーフレーム間の値変化曲線 (Linear / Cubic / Elastic 等) | 「イージング」 |
| **現在時刻 (Now)** | タイムライン上で再生中またはスクラブ中の時刻 (秒、double) | 「現在時刻」 |
| **再生レンジ (Play Range)** | 再生 / 書出 で対象とする時間範囲 (In/Out) | 「再生範囲」 |
| **DrawProgress** | パス図形の描画進行度 (0〜1)。`StrokeDashArray` + `StrokeDashOffset` を自動制御する派生プロパティ | 「描画進行」 |
| **PropertyRef** | アニメーション対象の不変キー (`ItemId + PropertyPath`) | (内部) |
| **中間表現 (IR)** | Phase 5.5 で WPF / MAUI / Lottie に変換するためのターゲット非依存モデル | (内部) |

---

## 3. 各機能の設計案

### 3.1 アニメーション対象範囲 (Q-1)

#### 推奨: 案 B (ExposedProperty + DesignerItem 主要プロパティ)
| 対象 | プロパティ |
|---|---|
| **DesignerItem 共通** | `Left.Value`, `Top.Value`, `Width.Value`, `Height.Value`, `RotationAngle.Value`, `Opacity.Value` |
| **図形系** | `EdgeBrush.Value` (SolidColorBrush として補間), `FillBrush.Value`, `EdgeThickness.Value` |
| **コネクタ系 (Phase 3)** | 上記 + `DrawProgress` (新設、Q-7) |
| **テキスト系 (Phase 2)** | 上記 + `Value`, `FontSize`, `Foreground` |
| **テーマ系 (Phase 4)** | `GlowRadius`, `GlowIntensity`, `GlowColor` |
| **パーツインスタンス (Phase 1)** | 上記 + **すべての ExposedProperty 値** |

#### 代替: 案 A (ExposedProperty のみ) / 案 C (全プロパティ Reflection 経由)
- 案 A: 設計はシンプルだが INTENT.md §Phase 5 の「位置・回転・不透明度・線の長さ」を満たせない。
- 案 C: 汎用性最大だが Reflection と PropertyPath パース実装が必要で、Phase 5-b の工数が大幅増。Phase 7 (スクリプタビリティ) に格上げが妥当。

### 3.2 タイムラインの所在 (Q-2)

#### 推奨: 案 B (シーン = CanvasPage ごとに 1 つ)
- 既存 `CanvasPages` の各ページに `Timeline` を 1:1 で持たせる。
- Phase 5.5 で 1 シーン = 1 UserControl/Storyboard に自然にマップできる。
- 複数ページ持つプロジェクトは「複数の独立シーン」として扱う (Phase 2 の挙動と一致)。

#### 代替: 案 A (グローバル単一) / 案 C (パーツ定義ごと) / 案 D (階層型)
- 案 A: 単純だが複数シーンを別アニメーションとして書き分けられない。
- 案 C: パーツ自体をアニメーション化したい場合に必要。**Phase 5 では Q-1 案 B 経由でパーツインスタンスの ExposedProperty をシーンタイムラインからアニメ化** することで代替可能。パーツ定義内タイムラインは Phase 5.5 以降。
- 案 D: 階層型は将来的に必要だが Phase 5 ではオーバーエンジニアリング。

### 3.3 タイムラインの時間粒度 (Q-3)

#### 推奨: 案 C (秒ベース、double)
- 時刻は `double` 秒 (例: `1.5`, `0.0333` = 30FPS 1 フレーム分)。
- WPF `TimeSpan` への変換は自然 (`TimeSpan.FromSeconds(t)`)。
- FPS は表示・編集の補助単位として `Timeline.Fps : int` (デフォルト 30) を別途持つ。再生・書出時のフレーム刻みに使う。
- シリアライズも素直 (`<Time>1.5</Time>`)。

#### 代替: 案 A (フレーム整数) / 案 B (ミリ秒整数)
- 案 A: FPS 切替で時刻全体がズレるので扱いにくい。
- 案 B: 整数精度は嬉しいが double に比べての利点が小さい。Phase 6 で MP4 書出時のフレーム揃えは Fps から逆算可能。

### 3.4 キーフレームの補間モデル (Q-4)

#### 推奨: 型ごとに以下を採用
| 型 | 補間 | 注記 |
|---|---|---|
| `Double` / `Int` / `Single` | 線形 + イージング | Int は補間後 `Math.Round` で整数化 |
| `Color` | **RGB 線形 + イージング** | WPF `ColorAnimation` と一致、Phase 5.5 マップ容易 |
| `Point` | X / Y 個別線形 + 同一イージング | |
| `Brush` | `SolidColorBrush` のみ Color として補間。それ以外は離散ジャンプ | グラデーション補間は Phase 5.5 後送り |
| `Bool` / `String` / `Enum` | 離散ジャンプ (次キーフレームの時刻で切替) | |

#### 代替: 案 D (Color を OKLab / HSV 線形)
- 知覚的に自然だが Phase 5.5 で WPF `ColorAnimation` (RGB線形) にマップする際に色が一致しなくなる。Phase 5 では一貫性優先で RGB 線形。OKLab はオプション化を Phase 6 で検討。

### 3.5 イージング (Q-5)

#### 推奨: 案 A (WPF 標準互換セット)
- **イージング種別** (12 種、WPF と同名):
  - `LinearEase` (既定)
  - `CubicEase`, `QuadraticEase`, `QuarticEase`, `QuinticEase`
  - `SineEase`, `ExponentialEase`, `CircleEase`, `PowerEase`
  - `ElasticEase`, `BackEase`, `BounceEase`
- **`EasingMode`**: `EaseIn` / `EaseOut` / `EaseInOut`
- 各イージングは pure な `double Apply(double normalizedT)` メソッドを提供 (テスト容易)。
- Phase 5.5 で WPF Storyboard へ 1:1 マップ可能。

#### 代替: 案 B (Linear/Step/CubicBezier の最小セット) / 案 C (案 A + ベジエ自由曲線)
- 案 B: シンプルだが FUI らしい「カクッ」「ぐにゃっ」の表現に弱い。
- 案 C: Adobe AE の Graph Editor 相当。Phase 6 以降の高度化候補。

### 3.6 再生エンジン (Q-6)

#### 推奨: 案 B (Play / Pause / Stop / スライダー + ループ + 再生範囲)
- **トランスポートコントロール**:
  - Play / Pause / Stop ボタン
  - 現在時刻表示 (秒 + フレーム番号、例: `1.500s (45f@30)`)
  - 時刻スライダー (タイムラインペインと連動)
- **ループ**: ON/OFF 切替、ループ範囲は再生レンジに従う。
- **再生レンジ (In/Out)**: 編集中のプレビューだけ短く再生したい用途。書出時もこの範囲を使う。
- **タイマー駆動**: `DispatcherTimer` (60Hz) で `Now` を進める。
- **既存 ExposedProperty 値の Save/Restore**: 再生開始時に各トラックの「再生前の値」を保存し、Stop で復元 (編集モードでの値破壊を防ぐ)。

#### 代替: 案 A (最小: Play/Pause/Stop/スライダーのみ) / 案 C (案 B + 速度倍率)
- 案 A: 範囲書出と相性が悪い。In/Out は Phase 5-f の前に欲しい。
- 案 C: 速度倍率はあれば便利だが Phase 5.5 で WPF Storyboard へのマップが複雑化。Phase 6 で検討。

### 3.7 トリムパスアニメーション (Q-7)

#### 推奨: 案 A (専用プロパティ `DrawProgress: double (0〜1)`)
- すべての「ストロークを持つ図形」(`PathViewModel`, `OrthogonalConnectorViewModel`, `AnchorBezierConnectorViewModel`, `PolyBezierViewModel` 等) に共通プロパティ `DrawProgress.Value : double` を派生で追加。
- 値が `1.0` → 完全描画、`0.0` → 非表示、`0.5` → 半分描画。
- 実装: `StrokeDashArray` + `StrokeDashOffset` を `(1 - DrawProgress) * GeometryLength` で連動制御する派生 ReactiveProperty。
- Phase 5 では `DrawProgress` を Double 型のキーフレーム化対象として扱う。
- Phase 5.5 で WPF `Storyboard` の `StrokeDashOffset` `DoubleAnimation` に自動マップ可能。

#### 代替: 案 B (生 StrokeDashArray キーフレーム) / 案 C (Geometry の Slice)
- 案 B: ユーザーが直接 dashArray を書く必要があり UX 悪い。
- 案 C: 真のジオメトリ切断。理想だが PathGeometry のサブセグメント取得は実装が重い。Phase 6 で検討。

### 3.8 数値カウンタアニメーション (Q-8)

#### 推奨: 案 A (既存 Phase 2 Block の Value をキーフレーム化)
- 既存 `DataGeneratorTextBlock` / `NumberSequenceBlock` / `TextMatrixBlock` には数値系プロパティ (Seed, Start, End 等) があり、これらを Q-1 案 B の対象に含めることで自然に「数値カウンタ」演出が作れる。
- 専用カウンタ型は新設しない。Phase 2 の機構を再利用する。
- フォーマット指定 (例: `{0:N2}` で `1234.56`) は既存 `Format` プロパティを利用。

#### 代替: 案 B (専用 `CounterTextBlock` 新設)
- API が増えてユーザー学習コストが増す。Phase 2 で対応済みの型を活用する方が一貫性がある。

### 3.9 ExposedProperty / プロパティパネル UI への統合 (Q-9)

#### 推奨: 案 A (プロパティ行右に「キーフレーム ◇」インジケータ)
- 各プロパティ行の右端に `◇` (ダイヤモンド) アイコンを表示。
- アイコンの状態:
  - **空 ◇**: そのプロパティはアニメーション化されていない
  - **半透明 ◇**: アニメーション化されているが現在時刻にキーフレームなし
  - **塗りつぶし ◆**: 現在時刻にキーフレームあり
- クリック:
  - 空 → 現在時刻にキーフレーム追加 (= トラック新規作成 + 最初のキーフレーム)
  - 半透明 → 現在時刻にキーフレーム追加
  - 塗りつぶし → 現在時刻のキーフレームを削除
- ホバー時にツールチップで補助操作 (右クリック → イージング選択、Shift クリック → 削除等) を案内。
- After Effects の Stopwatch アイコンと挙動を一致させる (ユーザーの既存知識を活用)。

#### 代替: 案 B (タイムラインペインのみ) / 案 C (案 A + 案 B)
- 案 B: プロパティ行を見ただけではアニメ化状態が分からず UX 悪い。
- 案 C: 両方表示は冗長。タイムラインペインで詳細編集、プロパティ行は状態表示のみ、で十分。

### 3.10 シリアライズ (Q-10)

#### 推奨: 案 A (既存 `.bg` に `<Timeline>` セクション追加)
- 各 `<CanvasPage>` の子要素として `<Timeline>` を追加。
- 既存プロジェクトファイル (Phase 4 以前) は `<Timeline>` セクションが無いので、Deserializer 側でデフォルト空 Timeline (Duration=0) を生成して後方互換維持。
- スキーマ概略:
  ```xml
  <CanvasPage>
    <!-- 既存要素 (Items, BackgroundColor, ...) -->
    <Timeline>
      <Duration>10.0</Duration>
      <Fps>30</Fps>
      <PlayRangeStart>0.0</PlayRangeStart>
      <PlayRangeEnd>10.0</PlayRangeEnd>
      <Loop>false</Loop>
      <Tracks>
        <Track>
          <ItemId>{guid}</ItemId>
          <PropertyPath>Left.Value</PropertyPath>
          <ValueType>Double</ValueType>
          <Keyframes>
            <Keyframe>
              <Time>0.0</Time>
              <Value>100.0</Value>
              <Easing>CubicEase</Easing>
              <EasingMode>EaseInOut</EasingMode>
            </Keyframe>
            <Keyframe>
              <Time>2.5</Time>
              <Value>400.0</Value>
              <Easing>LinearEase</Easing>
              <EasingMode>EaseIn</EasingMode>
            </Keyframe>
          </Keyframes>
        </Track>
      </Tracks>
    </Timeline>
  </CanvasPage>
  ```

#### 代替: 案 B (別ファイル `.bg.motion`)
- 1 プロジェクト = 複数ファイルになるので管理コスト増。`.bg` 単一ファイル原則を維持。

### 3.11 書出 (Q-11)

#### 推奨: 案 A (Phase 5-f は PNG 連番のみ、他は Phase 6 後送り)
- **Phase 5-f**: PNG 連番のみ実装。
  - 出力: `{baseName}_{frameIndex:0000}.png` (例: `scene1_0000.png`〜`scene1_0300.png`)
  - フレーム生成: 既存 `DesignerCanvas` を `RenderTargetBitmap` でレンダリング (`AnimationGifExporter` のレンダリング部を再利用)。
  - 各フレームレンダリング前に `Timeline.Now` を進めて補間値を適用。
  - 透過 PNG 対応、解像度はキャンバスサイズと一致 (高解像度書出はオプションで Phase 6)。
- **Phase 6 後送り**: MP4 (FFmpeg), WebM, GIF (再実装), Lottie JSON, WPF/MAUI XAML。

#### 代替: 案 B (PNG + MP4) / 案 C (PNG + MP4 + Lottie) / 案 D (フル)
- 案 B 以降は FFmpeg 依存追加・外部プロセス起動・出力中断時の挙動など Phase 5-f の枠を超える。Phase 6 で別立てが妥当。

### 3.12 Phase 5.5 (XAML 出力) との関係 (Q-12)

#### 推奨: 案 A (Phase 5 完了後の別フェーズ、ただし IR を意識して設計)
- Phase 5 のデータモデル (`Timeline`, `AnimationTrack`, `Keyframe`, `EasingFunction`, `PropertyRef`) を **ターゲット非依存の中間表現 (IR)** として設計する。
- IR からの変換器 (`IAnimationExporter`) インターフェースだけ Phase 5 で先に定義し、`WpfStoryboardExporter` / `MauiAnimationExporter` / `LottieExporter` の実装は Phase 5.5 / Phase 6 で別途。
- Phase 5 完了基準には「IR が `IAnimationExporter` で外から読める形になっていること」を含める。

#### 代替: 案 B (Phase 5 に Phase 5.5 を並行で取り込む)
- 1 フェーズが肥大化、テスト・レビュー単位が大きすぎる。`INTENT.md` の段階方針 (§2.2) に反する。

---

## 4. データモデル (推奨案ベース)

### 4.1 クラス図 (概略)

```
DiagramViewModel
  └── CanvasPages : ObservableCollection<CanvasPageViewModel>
        └── Timeline : TimelineViewModel
              ├── Duration : double (秒)
              ├── Fps : int (既定 30)
              ├── PlayRangeStart / PlayRangeEnd : double
              ├── Loop : bool
              ├── Now : BindableReactiveProperty<double>
              ├── IsPlaying : BindableReactiveProperty<bool>
              └── Tracks : ObservableCollection<AnimationTrack>
                    ├── PropertyRef (ItemId, PropertyPath, ValueType)
                    └── Keyframes : ObservableCollection<Keyframe>
                          ├── Time : double
                          ├── Value : object
                          ├── Easing : EasingKind (enum)
                          └── EasingMode : EasingMode (enum)
```

### 4.2 主要クラス雛形 (Phase 5-b 着手時に具体化)

#### TimelineViewModel
```csharp
public class TimelineViewModel : BindableBase, IDisposable
{
    public BindableReactiveProperty<double> Duration { get; }      // 秒
    public BindableReactiveProperty<int> Fps { get; }               // 既定 30
    public BindableReactiveProperty<double> PlayRangeStart { get; }
    public BindableReactiveProperty<double> PlayRangeEnd { get; }
    public BindableReactiveProperty<bool> Loop { get; }
    public BindableReactiveProperty<double> Now { get; }            // 現在時刻
    public BindableReactiveProperty<bool> IsPlaying { get; }
    public ObservableCollection<AnimationTrack> Tracks { get; }

    public ReactiveCommand PlayCommand { get; }
    public ReactiveCommand PauseCommand { get; }
    public ReactiveCommand StopCommand { get; }

    // Now 変更 → 各 Track の補間値を該当プロパティへ適用
    public void ApplyAtTime(double t);
}
```

#### AnimationTrack
```csharp
public class AnimationTrack : BindableBase
{
    public PropertyRef Target { get; }     // (ItemId, PropertyPath, ValueType)
    public ObservableCollection<Keyframe> Keyframes { get; }

    // 指定時刻における補間値を返す (Interpolator pure helper に委譲)
    public object EvaluateAt(double t);
}
```

#### Keyframe
```csharp
public class Keyframe : BindableBase
{
    public BindableReactiveProperty<double> Time { get; }
    public BindableReactiveProperty<object> Value { get; }
    public BindableReactiveProperty<EasingKind> Easing { get; }    // LinearEase / CubicEase / ...
    public BindableReactiveProperty<EasingMode> Mode { get; }      // EaseIn / EaseOut / EaseInOut
}
```

#### PropertyRef (IR の核、不変)
```csharp
public readonly record struct PropertyRef(
    Guid ItemId,
    string PropertyPath,    // "Left.Value", "Opacity.Value", "ExposedProperties[{guid}]" 等
    AnimatedValueType ValueType  // Double / Int / Color / Point / Brush / String / Enum / Bool
);
```

#### Interpolator (pure)
```csharp
public static class Interpolator
{
    public static object Interpolate(
        AnimatedValueType type,
        object from, object to,
        double normalizedT);  // 0..1 (イージング適用後)

    // 型ごとに switch
    //   Double  → Lerp
    //   Int     → Round(Lerp)
    //   Color   → RGB線形
    //   Point   → X/Y個別Lerp
    //   Brush   → SolidColorBrush なら Color として補間、それ以外は離散
    //   Bool/String/Enum → t < 1 なら from、t >= 1 なら to (離散ジャンプ)
}
```

#### EasingFunction
```csharp
public static class EasingFunctions
{
    public static double Apply(EasingKind kind, EasingMode mode, double t)
    {
        // t は 0..1
        // mode に応じて以下を選択:
        //   EaseIn:    f(t)
        //   EaseOut:   1 - f(1 - t)
        //   EaseInOut: t < 0.5 ? f(2t)/2 : 1 - f(2 - 2t)/2
    }
}
```

### 4.3 PropertyPath 仕様 (案)

PropertyPath は **ドット区切り文字列** で対象プロパティを表現する。

| パターン | 例 | 意味 |
|---|---|---|
| `<Property>.Value` | `Left.Value` | DesignerItem の ReactiveProperty 系プロパティ |
| `<Property>` | `RotationAngle` (将来予約) | プレーン CLR プロパティ |
| `ExposedProperties[{guid}]` | `ExposedProperties[abc...]` | ExposedProperty の値 (PartInstanceViewModel.ParameterValues 経由) |
| `DrawProgress.Value` | `DrawProgress.Value` | Q-7 で追加される派生プロパティ |

**実装上のポイント**:
- Phase 5-b では `Left.Value`, `Top.Value`, `Width.Value`, `Height.Value`, `RotationAngle.Value`, `Opacity.Value`, `EdgeBrush.Value`, `FillBrush.Value`, `EdgeThickness.Value`, `GlowRadius`, `GlowIntensity`, `GlowColor` だけ pure な dispatch メソッド (`ApplyAnimatedValue(item, path, value)`) で書く。
- ExposedProperty 経由は `PartInstanceViewModel._parameterValues[guid].Value = value` への直接代入。
- Reflection は使わない (Phase 5-b の段階では)。

---

## 5. UI フロー

### 5.1 タイムラインペイン

```
┌─────────────────────────────────────────────────────────────┐
│ [▶ Play] [⏸ Pause] [⏹ Stop]   01.500s / 30f / Loop:☑     │ ← トランスポート
├─────────────────────────────────────────────────────────────┤
│ ╋ Rect1 / Left.Value          ◆----◆--------◆               │ ← トラック行
│ ╋ Rect1 / Opacity.Value       ◆-------◆---------◆           │
│ ╋ Connector1 / DrawProgress   ◆-----◆                       │
│ ╋ PartInstance1 / RingCount   ◆-◆-◆-◆-◆                     │
├─────────────────────────────────────────────────────────────┤
│ 0.0s     1.0s     2.0s     3.0s     4.0s     5.0s ─[▼Now]   │ ← 時間軸
└─────────────────────────────────────────────────────────────┘
```

- 上段: トランスポート (Play/Pause/Stop/現在時刻表示/ループ切替)
- 中段: 各トラック (= 「対象 / プロパティ名 + キーフレーム ◆ 列」)
- 下段: 時間軸スケール + 現在時刻インジケータ
- キーフレーム◆をドラッグで時刻移動、右クリックで削除 / イージング変更
- 空エリア右クリックで「現在時刻にキーフレームを追加」

### 5.2 プロパティパネルからのキーフレーム追加

```
┌─ プロパティ ──────────────────┐
│ Left                100.0  ◇ │ ← 空のダイヤ = アニメ化されていない
│ Top                  50.0  ◆ │ ← 塗りつぶし = 現在時刻にキーフレームあり
│ Width               200.0  ◇ │
│ Opacity              0.8   ◇ │
│ EdgeBrush     [Color]      ◇ │
│ RotationAngle        45.0  ◇ │
└──────────────────────────────┘
```

- `◇` クリック → そのプロパティにトラックを新規作成、現在時刻に最初のキーフレーム追加
- `◆` クリック → そのキーフレームを削除
- 右クリック → イージング選択メニュー
- Shift クリック → トラック全削除

### 5.3 再生フロー

1. ユーザーが Play ボタンクリック
2. `DispatcherTimer` (60Hz) が `Now += dt` を進める
3. `Now` の Observable が各 `AnimationTrack` の `EvaluateAt(Now)` を呼び、補間値を該当プロパティへ書き戻す
4. ループ有効時、`Now` が `PlayRangeEnd` に到達したら `PlayRangeStart` に巻き戻す
5. Stop 時、`Now = PlayRangeStart` に戻し、各プロパティを再生開始前の値に復元

### 5.4 書出フロー (Phase 5-f)

1. メニュー「ファイル → 書き出し → PNG 連番」を選択
2. ダイアログで開始時刻 / 終端時刻 / FPS / 出力先 / ベース名を指定
3. 開始時刻から終端まで `1/Fps` 秒ずつ `Now` を進めながら、各時刻で `RenderTargetBitmap` を生成
4. 各フレームを `{baseName}_{frameIndex:0000}.png` で保存
5. プログレスバー表示、キャンセル可能
6. 書出完了後、`Now` は元の位置に戻し、各プロパティを書出前の値に復元

---

## 6. シリアライズ仕様 (Q-10 案 A)

§3.10 のスキーマ概略参照。詳細は Phase 5-b 着手時に Phase 4-f と同じスタイルで `ObjectSerializer.cs` / `ObjectDeserializer.cs` に実装する。

### 6.1 後方互換性
- Phase 4 以前のプロジェクトファイル (= `<Timeline>` セクション無し) を開いた場合、Deserializer は各 CanvasPage に空 Timeline (Duration=0, Fps=30, Tracks=empty) を生成して読み込みを継続する。
- 保存時、Timeline が空 (Duration=0 かつ Tracks 空) の場合は `<Timeline>` セクション自体を出力しないオプションを検討 (= Phase 4 以前互換のファイル形式維持)。
- これにより既存テスト (1424 件) のシリアライズ系は影響を受けない。

### 6.2 PropertyRef の永続化
- `ItemId` は `DesignerItemViewModelBase.ID` を使用 (既存と一貫)。
- `PropertyPath` は §4.3 のドット区切り文字列をそのまま XML 文字列として保存。
- `ValueType` は `AnimatedValueType` enum (`Double` / `Int` / `Color` / `Point` / `Brush` / `String` / `Enum` / `Bool`) を文字列化。

### 6.3 ExposedProperty 連動
- PartInstance の ExposedProperty 値をアニメ化した場合、`PropertyPath` は `ExposedProperties[{exposedPropertyId-guid}]` 形式。
- Deserialize 時、対応する ExposedProperty が見つからない場合 (PartDefinition が変わって削除された等) はトラックを破棄 + 警告ログ。

---

## 7. テスト方針

Phase 5 で追加する単体テスト (Phase 5-b 〜 5-f を通して):

### 7.1 Interpolator (Phase 5-c)
- Double 線形補間 (境界: t=0, t=1, t=0.5, t<0, t>1 のクランプ挙動)
- Int 線形補間 + Round (例: 1, 10 を t=0.5 で補間 → 6 ではなく 5 or 6 のラウンドルール明文化)
- Color RGB 線形補間 (RGB 各成分独立、α 補間含む)
- Point 線形補間 (X/Y 個別)
- Brush 補間 (`SolidColorBrush` 同士 OK、それ以外は離散)
- 離散ジャンプ (`Bool`, `String`, `Enum`): t<1 で from、t≥1 で to

### 7.2 EasingFunctions (Phase 5-c)
- 各 12 種 × 3 モード = 36 組合せの `Apply(t=0) = 0`, `Apply(t=1) = 1` (端点不変式)
- `EaseInOut` の中点 `Apply(0.5) = 0.5` 性 (対称イージングのみ)
- `LinearEase` が `t` を返す恒等性

### 7.3 AnimationTrack (Phase 5-b)
- キーフレームなし → デフォルト値返却
- キーフレーム 1 個 → その値を常に返却
- キーフレーム 2 個 → 線形 + イージング補間
- キーフレーム 3 個以上 → 該当区間検出 + 補間
- 時刻が範囲外 → 端の値を返却 (クランプ)

### 7.4 TimelineViewModel (Phase 5-e)
- Play → Now が進む
- Pause → Now 停止
- Stop → Now = PlayRangeStart に戻る + 各プロパティが再生開始前の値に復元
- Loop ON で PlayRangeEnd → PlayRangeStart にラップ
- ApplyAtTime の冪等性 (同じ t で複数回呼んでも同じ結果)

### 7.5 シリアライズ (Phase 5-b 後半)
- 空 Timeline → ファイル出力 + 再読込で空のまま (Phase 4 以前互換)
- Track 1 個 + Keyframes 2 個 → RoundTrip
- ExposedProperty 経由 Track の RoundTrip
- 旧フォーマット (`<Timeline>` 無し) の読込 → 空 Timeline 生成

### 7.6 DrawProgress (Phase 5-c または -d)
- DrawProgress=1.0 で完全描画 (StrokeDashArray が「線全長 + 0」)
- DrawProgress=0.5 で半分描画 (StrokeDashArray が「線全長/2 + 線全長/2」)
- DrawProgress=0.0 で非表示 (StrokeDashArray が「0 + 線全長」)
- Geometry が変わったとき DrawProgress の効果が正しく追従

### 7.7 既存テスト保護
- `project_test_baseline.md` 1424 件は Phase 5-b 開始時点で全緑、各サブフェーズ完了時にも全緑を維持。

---

## 8. 段階的実装順序

| サブフェーズ | 内容 | 主要成果物 |
|---|---|---|
| **5-a (本書)** | 設計仕様確定 | `docs/fui/phase5-motion-animation.md` v1.0 (Q-1〜12 全件確定) |
| **5-b** | データモデル + シリアライズ | `Models/Animation/*.cs`, `ViewModels/Animation/*.cs`, `ObjectSerializer/Deserializer` への Timeline 追加 |
| **5-c** | 補間エンジン (pure) + EasingFunctions | `Helpers/Animation/Interpolator.cs`, `EasingFunctions.cs`, `PropertyApplier.cs` (PropertyPath → set) |
| **5-d** | タイムラインペイン UI + プロパティ行 ◇ アイコン | `Views/Animation/TimelinePane.xaml`, プロパティパネル拡張 |
| **5-e** | 再生エンジン (DispatcherTimer + Now 駆動 + Save/Restore) | `TimelineViewModel.Play/Pause/Stop`, `RestoreSnapshot` |
| **5-f** | PNG 連番書出 | `Helpers/Animation/PngSequenceExporter.cs`, 書出ダイアログ |
| **5-g** | チュートリアル + IR インターフェース定義 | `docs/fui/phase5-tutorial.md`, `IAnimationExporter` インターフェース |

`5-a` 完了後、ブランチ運用は Phase 4 と同じ流儀:
- `feature/fui-phase-5-b` 〜 `feature/fui-phase-5-g` を各サブフェーズで worktree 派生
- 各サブフェーズ完了で develop へ `--no-ff` マージ + origin push
- すべて完了したら `feature/fui-phase-5` メタブランチでチュートリアル追加 + INTENT.md 更新

---

## 9. パフォーマンス考慮点

- **再生中のレンダリング負荷**: `Now` の変更が大量のプロパティ書き戻しを起こすため、各シーンで何百キーフレームあっても 30FPS で再生できる必要がある。対策:
  - `AnimationTrack.EvaluateAt` は前回キーフレーム区間をキャッシュ (大半は同区間で連続呼び出しされる)
  - `Interpolator` は pure かつアロケーション最小化 (Color/Point は struct のまま扱う)
- **書出 (PNG 連番) 中の I/O**: 各フレーム個別保存はディスク I/O が線形に効く。非同期書込 + 書込中も次フレーム RenderTargetBitmap を準備する pipeline 化を Phase 5-f 後半で検討。
- **UI 応答性**: タイムラインペインのキーフレーム表示は仮想化 (大量トラックでも表示できる)。

---

## 10. Q-N 確定一覧 (全件確定済み)

| ID | テーマ | 推奨案 | 状態 |
|---|---|---|---|
| **Q-1** | アニメーション対象範囲 | 案 B (ExposedProperty + DesignerItem 主要プロパティ) | ✅ 確定 |
| **Q-2** | タイムラインの所在 | 案 B (シーン = CanvasPage ごと) | ✅ 確定 |
| **Q-3** | 時間粒度 | 案 C (秒ベース double + Fps 補助) | ✅ 確定 |
| **Q-4** | 補間モデル (型ごと) | RGB 線形 (Color)、線形 (Double/Int/Point)、離散 (Bool/String/Enum)、SolidColorBrush のみ Brush 補間 | ✅ 確定 |
| **Q-5** | イージング種別 | 案 A (WPF 標準 12 種 + 3 モード互換) | ✅ 確定 |
| **Q-6** | 再生 UI スコープ | 案 B (Play/Pause/Stop/スライダー + ループ + 再生範囲) | ✅ 確定 |
| **Q-7** | トリムパス実装 | 案 A (`DrawProgress: double` 派生プロパティ) | ✅ 確定 |
| **Q-8** | 数値カウンタ | 案 A (Phase 2 既存 Block の Value プロパティをキーフレーム化) | ✅ 確定 |
| **Q-9** | プロパティ行 UI | 案 A (◇ ダイヤモンドインジケータ、AE 流) | ✅ 確定 |
| **Q-10** | シリアライズ形式 | 案 A (`.bg` 既存ファイルに `<Timeline>` セクション追加) | ✅ 確定 |
| **Q-11** | 書出フォーマット (Phase 5-f) | 案 A (PNG 連番のみ。MP4/Lottie/XAML は Phase 6 / 5.5 後送り) | ✅ 確定 |
| **Q-12** | Phase 5.5 (XAML 出力) との関係 | 案 A (別フェーズ、ただし Phase 5 で IR を意識した設計 + `IAnimationExporter` 定義) | ✅ 確定 |

---

## 11. Phase 5.5 への接続 (中間表現 IR)

### 11.1 IR の責任範囲
Phase 5 のデータモデルは、以下の意味で **ターゲット非依存の中間表現 (IR)** として設計する:

| IR の構成要素 | Phase 5 内 | Phase 5.5 (WPF) | Phase 5.5 (MAUI) | Phase 6 (Lottie) |
|---|---|---|---|---|
| `Timeline` | シーンタイムライン | → 1 Storyboard | → 1 Animation グループ | → 1 Composition |
| `AnimationTrack` | プロパティ単位 | → `DoubleAnimation` 等 | → `Animation.Add` 一連 | → 1 layer property |
| `Keyframe` | 時刻 + 値 + イージング | → `DoubleKeyFrame` 派生 | → keyframe 配列 | → keyframe オブジェクト |
| `EasingKind` + `EasingMode` | 列挙体 | → `EasingFunctionBase` 派生インスタンス | → `Easing` 関数 | → cubic-bezier 制御点 |
| `PropertyRef` | (ItemId, PropertyPath, ValueType) | → `Storyboard.TargetName` + `TargetProperty` | → `BindableObject` + プロパティ名 | → layer property path |

### 11.2 `IAnimationExporter` インターフェース (Phase 5-g で先行定義)

```csharp
public interface IAnimationExporter
{
    string TargetName { get; }                     // "WPF Storyboard" / "MAUI Animation" / "Lottie JSON"
    string FileExtension { get; }                  // ".xaml" / ".cs" / ".json"
    Task ExportAsync(
        TimelineViewModel timeline,
        IEnumerable<DesignerItemViewModelBase> items,
        ExportOptions options,
        Stream output);
}
```

実装は Phase 5.5 / Phase 6 で別途。Phase 5 ではこの **インターフェースの定義** と「Timeline / Track / Keyframe を外から読み出せる API 表面」だけ整える。

### 11.3 Phase 5 で守るべき IR 設計ルール
1. **WPF 固有の型に依存しない**: `Timeline` 等のモデルは `DoubleAnimation` 等の WPF Media クラスを直接参照しない (UI 層と隔離)。
2. **時刻は double 秒で統一**: `TimeSpan` に依存しない。エクスポーター側で `TimeSpan.FromSeconds` 変換する。
3. **イージングは enum で表現**: `EasingFunctionBase` 派生インスタンスを Track に持たせない。エクスポーター側で enum → 具体クラスにマップする。
4. **PropertyPath は文字列**: PropertyPath を `Expression<Func<T>>` のような型情報付きにしない (Phase 7 で必要になれば別途追加)。

---

## 12. クイックリファレンス (Phase 5-b 着手時の手引き)

> 各 Q の確定案を 1 行で再掲。Phase 5-b の最初のコミットで参照するためのチートシート。

| 確定項目 | 確定案 |
|---|---|
| Q-1 アニメ対象 | 案 B (ExposedProperty + DesignerItem 主要プロパティ) |
| Q-2 タイムライン所在 | 案 B (シーン = CanvasPage ごとに 1 つ) |
| Q-3 時間粒度 | 案 C (秒ベース double + Timeline.Fps:int=30 補助) |
| Q-4 補間モデル | Double/Int/Point=線形+EZ, Color=RGB線形+EZ, Brush=SolidColorBrush のみ Color 補間, Bool/String/Enum=離散ジャンプ |
| Q-5 イージング | 案 A (WPF 標準 12 種 + EaseIn/EaseOut/EaseInOut 互換、Phase 5.5 で WPF Storyboard へ 1:1 マップ) |
| Q-6 再生 UI | 案 B (Play/Pause/Stop + 現在時刻表示 + スライダー + ループ + 再生範囲 In/Out。書出範囲もこれを使う) |
| Q-7 トリムパス | 案 A (Stroke 系全図形に DrawProgress.Value:double を共通派生プロパティで追加、StrokeDashArray+Offset を内部で連動制御) |
| Q-8 数値カウンタ | 案 A (Phase 2 既存 Block の数値プロパティを Q-1 案 B 経由でキーフレーム化、専用型は新設しない、Format で表示書式を制御) |
| Q-9 プロパティ UI | 案 A (各プロパティ行の右端に AE 流 ◇ アイコン、空/半透明/塗りつぶしの 3 状態、クリックで追加/削除、右クリックでイージング選択) |
| Q-10 シリアライズ | 案 A (各 CanvasPage の子に <Timeline> 追加、空 Timeline はセクション省略で Phase 4 以前と完全互換) |
| Q-11 書出 | 案 A (Phase 5-f は PNG 連番のみ、{baseName}_{0000}.png、透過対応、解像度はキャンバスと一致。MP4/Lottie/XAML は Phase 6 / 5.5 後送り) |
| Q-12 Phase 5.5 関係 | 案 A (別フェーズ、ただし Phase 5 で IR (ターゲット非依存中間表現) を意識設計、Phase 5-g で IAnimationExporter 先行定義) |

---

*Phase 5-a 確定版 v1.0 (2026-05-14)*
*次回更新: Phase 5-b 着手時にデータモデル詳細を追記、または Phase 5 完了時にチュートリアル参照を追加*
