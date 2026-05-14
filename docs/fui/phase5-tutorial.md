# Phase 5 モーション・アニメーション チュートリアル

> 関連: 設計仕様書 [`phase5-motion-animation.md`](./phase5-motion-animation.md)
> 前提: [`phase1-tutorial.md`](./phase1-tutorial.md) を読んでパーツ機構の基本操作を理解していること。Phase 2〜4 は読まなくても進められますが、テーマ適用 (Phase 4) 後の色変化もアニメ可能です。

このドキュメントは Phase 5 で追加された **モーション・アニメーション** 機能の操作ガイドです。図形に時間軸を与え、補間で値を動かし、最終的に PNG 連番として書き出すまでを一通り解説します。

---

## 1. Phase 5 で追加された要素

| 機能 | UI 表記 / 場所 | 内部型 / 経路 | 用途 |
|---|---|---|---|
| タイムラインペイン | 画面下部の `Timeline` Expander | `TimelinePane.xaml` | Track 一覧 + トランスポート + 時間軸表示 |
| ◇ キーフレームボタン | 図形のプロパティダイアログ各行右端 | `Detail.xaml` 第 3 列 | 現時刻 (Now) にキーフレームを追加 / 削除 |
| Track | タイムライン中段の行 | `AnimationTrack` | 1 つの (Item, PropertyPath) ペアに対する時系列値 |
| Keyframe | Track 上の ◆ アイコン | `Keyframe` | 時刻 t における値 + イージング |
| 再生 / 一時停止 / 停止 | トランスポートバーの ▶ / ❚❚ / ■ | `TimelineViewModel.Play/Pause/Stop` | DispatcherTimer で Now を進めて補間値を適用 |
| Loop | トランスポートバーの `Loop` チェック | `TimelineViewModel.Loop` | end に達したら start に折返し |
| PNG 連番書出 | トランスポートバーの「PNG 連番...」ボタン | `PngSequenceExportDialog` | Renderer で時刻ごとにフレーム画像を保存 |

---

## 2. 用語 / 内部モデル早見表

```
Timeline (シーンに 1 つ)
 ├ Duration (秒) / Fps / PlayRangeStart / PlayRangeEnd / Loop / Now
 └ Tracks: [Track1, Track2, ...]
      └ Track
         ├ Target: PropertyRef (ItemId, PropertyPath, ValueType)
         └ Keyframes: [{Time, Value, Easing, Mode}, ...]
```

- **`Timeline`** はシーン (CanvasPage) ごとに 1 つ。`.bgff` ファイルに `<Timeline>` セクションでシリアライズされる (空なら省略 → Phase 4 以前互換)。
- **`Track`** は (図形 ID, プロパティパス) で一意。`"Left.Value"` / `"EdgeBrush.Value"` / `"ExposedProperties[{guid}]"` などのパスを `PropertyApplier` で吸収する。
- **`Keyframe`** は時刻 + 値 + イージング (12 種 × 3 モード)。同じ Track 内に複数並べると `Interpolator` でリニア補間 + `EasingFunctions` で時間軸方向の歪みが乗る。

---

## 3. キーフレームを打つ (Phase 5-d-3)

### 3.1 ◇ ボタンの読み方

| 見た目 | 意味 |
|---|---|
| 🔶 (塗りつぶし、黄色) | この (Item, Property) に対する Track が存在する |
| ◇ (枠線のみ) | Track が存在しない |

> ※ 「Now ちょうどにキーフレームがあるか」を区別する表示 (`TrackOnly` vs `HasKeyframeAtNow`) は将来のフォローアップ。現状は Track の有無のみ。

### 3.2 打ち方

1. 図形をダブルクリックしてプロパティダイアログを開く。
2. アニメさせたいプロパティ行 (例: `Left.Value`) の右端 ◇ ボタンをクリック。
   - **Track 無し**: その場で Track + Keyframe (現在値) が作られる。Timeline ペインに行が追加され、ダイアログの ◇ が塗りに変わる。
   - **Track 有り + Now にキーフレーム無し**: 現時刻にキーフレームが追加される。
   - **Now にキーフレーム有り**: そのキーフレームが削除される (= トグル)。最後の 1 つを消すと Track ごと消える。
3. Timeline ペインの `Now` を別の時刻に進めて、プロパティの値を変えてからもう一度 ◇ を押すと、2 個目のキーフレームができる。これで補間アニメ完成。

### 3.3 内部処理 (詳細)

`KeyframeToggleHelper.ToggleKeyframeAtNow(itemId, propertyPath, currentValue, timeline)` が:
1. 既存 Track を `(ItemId, PropertyPath)` で探す。
2. Now と一致するキーフレーム (許容差 1ms) があれば削除、無ければ追加。
3. AnimatedValueType は現在値の CLR 型から推論 (`InferValueType`)。

---

## 4. Timeline ペインの読み方 (Phase 5-d-2)

```
+-------------------------------------------------------------+
| ▶ ❚❚ ■   Now 1.50 s   Fps 30   ☑Loop   | PNG 連番... |
+--------+----------------------------------------------------+
| Left.Value             |          🔶          🔶            |
| Top.Value              |    🔶                              |
+--------+----------------------------------------------------+
|                      0s          1s          2s             |  ← 時間軸 + 赤い縦線 (現在位置)
+-------------------------------------------------------------+
```

- **上段**: トランスポート + 現在時刻 + Fps + Loop + PNG 連番ダイアログ起動。
- **中段**: Tracks ItemsControl。各行に Property 名 + Canvas、Canvas 上に ◆ がキーフレーム位置、赤い縦線が現在時刻。
- **下段**: 時間軸 + 現在時刻インジケータ。

Expander のヘッダーは折りたたんでもアクセスできる位置 (キャンバス下、Canvas タブ + StatusBar の上) に常駐。

---

## 5. 再生 (Phase 5-e)

### 5.1 トランスポート

- **▶ Play**: 現在の Now から再生開始。再生前のアイテム値を `PlaybackEngine.Snapshot` で保存する。
- **❚❚ Pause**: Now を維持して停止。再 Play で続行 (Snapshot は保持)。
- **■ Stop**: Now を PlayRangeStart に戻し、`PlaybackEngine.Restore` で Snapshot を書き戻す = Play 前の状態に巻き戻す。

### 5.2 Loop / PlayRange

- **Loop=true**: 再生中に `Now >= PlayRangeEnd` になると `Now - (end - start)` で先頭に戻る。`PlaybackEngine.NormalizeTime` が折返し計算 (2 周以上のオーバーシュート / 負の値 / span<=0 も処理)。
- **Loop=false**: PlayRangeEnd で自動停止 (Snapshot Restore はしない、その時刻の補間値が表示に残る)。

### 5.3 サンプリングレート

`DispatcherTimer` の Interval は `1/Fps` 秒 (Render priority)。Tick ごとに実時間 (DateTime.UtcNow 差分) を Now に加算するので、Fps を小さくしても再生時間がスローモーションになるわけではなく、フレーム間隔が荒くなる。

---

## 6. PNG 連番書出 (Phase 5-f)

### 6.1 起動

トランスポートバーの **「PNG 連番...」** ボタンで `PngSequenceExportDialog` が開く。Timeline.PlayRangeStart/End/Fps/Duration が初期値として渡される。

### 6.2 設定項目

| 項目 | 既定 | 意味 |
|---|---|---|
| 開始時刻 (s) | `Timeline.PlayRangeStart` | 書出開始時刻 (両端含む) |
| 終了時刻 (s) | `Timeline.PlayRangeEnd or Duration` | 書出終了時刻 |
| Fps | `Timeline.Fps` (= 30 既定) | 書出 fps (再生 fps と独立可、1〜240) |
| 出力フォルダ | `Pictures` フォルダ | 連番 PNG の保存先 (参照... で `FolderBrowserDialog` 起動) |
| ファイル名 prefix | `frame_` | 各ファイル名の頭 (例: `frame_0001.png`) |

設定変更ごとに「書き出しフレーム数」と「バリデーションメッセージ」がライブで更新される (R3 `CombineLatest`)。`Start >= 0`、`End > Start`、`End <= Duration`、`1 ≤ Fps ≤ 240`、空でない `OutputDirectory` / `FilenamePrefix` を満たすまで「書き出し」ボタンは無効。

### 6.3 書出処理 (Q-11 案 A)

「書き出し」を押すと:
1. `PngSequenceExporter.Snapshot` で書出前の状態を保存。
2. `0` 〜 `total-1` 番のフレームについて:
   - `time = Start + i / Fps`
   - `PlaybackEngine.ApplyAt(timeline, time, resolver)` でアイテム値を時刻 `time` に進める。
   - `Renderer.Render(null, designerCanvas, this, background, background)` で RenderTargetBitmap を生成。
   - `PngBitmapEncoder` で `{OutputDirectory}/{prefix}{i:0000}.png` に保存 (桁数は `max(4, log10(total))`)。
3. `finally` で `PlaybackEngine.Restore` で書出前の値に戻す (例外時も同じ)。

進捗 UI は現状なし (Phase 5-f-2 では即時実行)。フレーム数 × 1 枚あたりのレンダ時間に比例して時間がかかる。

---

## 7. シリアライズ (Q-10 案 A、Phase 5-d-1)

- `Timeline.IsEmpty` (= `Duration==0 && Tracks.Count==0`) のとき `<Timeline>` セクションは書き出さない → Phase 4 以前と完全互換。
- 空でない場合は `<Timeline>` 配下に Tracks / Keyframes を `TimelineSerializer.Serialize` で xml にする。デシリアライズは `ObjectDeserializer.RestoreTimelineSection` が `FinalizeAnchorsAndFollowers` の前で呼ばれ、Track の Target / Keyframe.Value (型ごとに `#AARRGGBB` 等) を復元する。

---

## 8. アーキ概観

```
TimelineViewModel
  ├ Tracks / Keyframes (Phase 5-b)
  ├ ItemResolver: Guid -> SelectableDesignerItemViewModelBase
  ├ Play/Pause/Stop コマンド (Phase 5-e-2)
  └ DispatcherTimer
       └ AdvanceBy(dt) → NormalizeTime → ApplyAt

Helpers/Animation/
  ├ Interpolator         (Phase 5-c, pure)
  ├ EasingFunctions      (Phase 5-c, pure: 12 kinds × 3 modes)
  ├ PropertyApplier      (Phase 5-c/5-e-1: Apply + TryGet)
  ├ PlaybackEngine       (Phase 5-e-1, pure: Snapshot/Restore/ApplyAt + NormalizeTime)
  ├ KeyframeToggleHelper (Phase 5-d-3, pure: ToggleKeyframeAtNow / GetStatus)
  ├ TimelineSerializer   (Phase 5-b, pure: Serialize/Deserialize)
  ├ PngSequenceExporter  (Phase 5-f-1/2: Validate/ComputeFrameCount/.../Export)
  ├ IAnimationExporter        (Phase 5-g, interface)
  └ PngSequenceExporterAdapter (Phase 5-g: PNG 連番を IAnimationExporter 経由で扱う)

Views/Animation/
  ├ TimelinePane.xaml          (Phase 5-d-2)
  └ PngSequenceExportDialog    (Phase 5-f-2)
```

---

## 9. 既知の制約 / 将来枠

- **CanvasPage 単位 Timeline swap**: 現状 `DiagramViewModel.Timeline` は単一インスタンス。複数 CanvasPages を切替えると Timeline は共有されている (= ページ切替で消える挙動はしない)。複数シーン対応は将来検討。
- **◇ アイコンの状態表示**: 「Track 有 / 無」は反映するが、「Now ちょうどにキーフレームがあるか」は未反映。Now の subscribe を増やすと表示細分化できる。
- **Opacity アニメ**: `SelectableDesignerItemViewModelBase` に `Opacity` プロパティが無いため未対応。Phase 5 後半 or 別フェーズで `Opacity` 追加 + PropertyApplier 拡張が必要。
- **DrawProgress アニメ** (Q-7 案 A): Stroke 系図形に `DrawProgress.Value:double` 派生プロパティを追加する案は未実装。
- **テキスト系 Block の Value/FontSize/Foreground**: `PropertyApplier` でテキスト系 Block 派生クラスへの dispatch を未実装。
- **PNG 書出進捗 UI**: ダイアログ閉じてから完了まで「書き出し中…」表示なし。`IProgress<int>` で連動した ProgressBar を追加できる。
- **Phase 5.5 (WPF/MAUI XAML 出力)**: `IAnimationExporter` インターフェースを Phase 5-g で定義済み。Phase 5.5 で `WpfStoryboardXamlExporter` 等を実装する設計上の準備完了。

---

## 10. クイックリファレンス: 主要 PropertyPath

`PropertyApplier` が対応する `PropertyPath` (= Track.Target.PropertyPath に入る文字列):

| パス | 型 | 対応図形 |
|---|---|---|
| `Left.Value` / `Top.Value` / `Width.Value` / `Height.Value` | double | DesignerItemViewModelBase 派生全部 |
| `RotationAngle.Value` | double | SelectableDesignerItemViewModelBase 派生全部 |
| `EdgeBrush.Value` / `FillBrush.Value` | Brush | 同上 |
| `EdgeThickness.Value` | double | 同上 |
| `GlowRadius.Value` / `GlowIntensity.Value` | double | 同上 (Phase 4-e) |
| `GlowColor.Value` | Color | 同上 |
| `ExposedProperties[{guid}]` | 任意 | PartInstanceViewModel (Phase 1) |

これ以外のパス (例: `Opacity.Value`, テキスト系プロパティ) はキーフレームを打っても `PropertyApplier.Apply` が false を返すので再生時に効果が出ない。
