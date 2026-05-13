# Phase 3-a: 接続線・関係表現 設計仕様書 (確定版 v1.0)

> このドキュメントは `docs/FUI_DESIGNER_INTENT.md` Phase 3 「接続線・関係表現」に対応する設計仕様書である (確定版 v1.0)。
> §10 のオープン問題は Q-1 〜 Q-11 すべて確定済み (2026-05-13)。
> §12 が Phase 3-b 実装時のクイックリファレンス。

---

## 1. 概要

### 1.1 目的
要素同士を「配線」して関係を示すのは FUI の重要言語。
ノードと接続線で「データフロー」「コマンドフロー」「依存関係」「信号経路」を表現できる土台を整える。

### 1.2 この機能が満たすべき体験
1. **L 字 (カギ型) コネクタ** で図形同士を直角に折れる線で結べる。配線図 / 機械系 FUI で頻出する表現。
2. **ベジエコネクタ (新規)** で図形同士を曲線で結べる。フローダイアグラム / 神経網風 FUI で頻出する表現。
3. **アンカー (Snap Target)** が既存図形に複数持てて、コネクタが吸着する。図形を動かすとコネクタも追従する。
4. **ノード - エッジ風グラフ** が組める。任意の DesignerItem を「ノード」とみなし、グラフモード UI で選択時に関連エッジをハイライトできる。
5. これら全てを **Phase 1 のパーツ機構** で再利用可能パーツとして登録できる (例: 「2 つの円 + L 字結線 + ラベル」を 1 パーツ化)。

### 1.3 非スコープ (Phase 3 では扱わない)
- アニメーション付きコネクタ (例: 流れる電子、信号パルス) は **Phase 5 以降**。
- 自動配置アルゴリズム (force-directed / hierarchical layout) や **コネクタの図形回避ルーティング** は **Phase 7 以降** (Phase 3 では手動配置 + 直結ルーティングのみ)。
- グラフ理論操作 (DFS / BFS / 最短経路) は本ツールのスコープ外。
- リアルタイムデータバインディング (例: 外部 JSON ストリーム → ノード更新) は **Phase 7 以降**。

### 1.4 Phase 3 のスコープに含むもの (改めて整理、Q-11 案 B 反映)
- **L 字 (Orthogonal) コネクタ型** (`OrthogonalConnectorViewModel`、完全新規)
- **新規ベジエコネクタ型** (`AnchorBezierConnectorViewModel`、完全新規。既存 `BezierCurveViewModel` には触らない)
- **アンカー (Anchor) 機構** — 図形に複数の吸着点を持たせる
- **ノード - エッジ風グラフ モード UI** — DesignerItem に `IsNode` を追加し、選択時に関連エッジをハイライト
- 上記すべての **シリアライズ対応** + **Phase 1 パーツ機構との統合** + **チュートリアル**

---

## 2. 用語定義

| 用語 | 意味 | UI 表記 |
|---|---|---|
| **L 字コネクタ (Orthogonal)** | 直角に折れる接続線 (0〜N の折れ点) | 「L 字コネクタ」 |
| **ベジエコネクタ (Anchor 接続版)** | 始点・終点 + 制御点 2 つで曲線を描く Anchor 接続コネクタ | 「ベジエコネクタ」 |
| **アンカー** | 図形に紐づく吸着点。コネクタの端点が吸着し、図形移動に追従 | 「アンカー」 |
| **エッジ (ノード - エッジ図のエッジ)** | コネクタの別名。グラフモード時のラベル | 「エッジ」 |
| **ノード** | グラフモード時の DesignerItem の別名 | 「ノード」 |

既存の `StraightConnector` / `BezierCurve` / `PolyBezier` には触らない (後方互換維持、Q-1 案 B 反映)。Phase 3 で新たに追加される接続表現は別系統として実装する。

---

## 3. 各機能の設計案

### 3.1 L 字 (Orthogonal) コネクタ (Q-2 / Q-3 / Q-4 反映)
**主要プロパティ**:
- `BeginPoint : Point` — 始点座標
- `EndPoint : Point` — 終点座標
- `RoutingMode : OrthogonalRoutingMode` — `Auto` (デフォルト) / `HFirst` / `VFirst` / `Manual`
- `MidPoints : ObservableCollection<Point>` — 折れ点座標 (常に任意数 0..N、Q-3 案 B)
- `EdgeBrush / EdgeThickness / StrokeDashArray` (既存 ConnectorBase と共通)
- `CornerRadius : double` — 折れ点の角を丸める半径 (デフォルト 0 で直角、Q-4 案 A)
- `BeginAnchorRef : Guid?` / `EndAnchorRef : Guid?` — 始点・終点アンカー参照

**ルーティングロジック**:
- `Auto` (Q-2 案 A): 始点と終点の差分から H-First / V-First を選ぶ (横の差分 ≥ 縦の差分なら H-First、それ以外 V-First)。MidPoints は内部計算した 1 個のみ保持
- `HFirst` / `VFirst`: 固定方向。MidPoints は内部計算 1 個
- `Manual`: ユーザーが `MidPoints` を 0..N 個明示。Path = `Begin → MidPoints[0] → ... → MidPoints[n-1] → End` を直角に繋ぐ
- `CornerRadius > 0` の場合、折れ点を ArcSegment で丸める (Q-4 案 A)

### 3.2 ベジエコネクタ (Q-1 案 B 反映、完全新規)
新規 `AnchorBezierConnectorViewModel` を `ConnectorBaseViewModel` 直接派生として実装。既存 `BezierCurveViewModel` は **一切触らない**。

**主要プロパティ**:
- `BeginPoint : Point` / `EndPoint : Point`
- `BeginControlPoint : Point` / `EndControlPoint : Point` — ハンドル絶対座標
- `BeginAnchorRef : Guid?` / `EndAnchorRef : Guid?` — 始点・終点アンカー参照
- `EdgeBrush / EdgeThickness / StrokeDashArray` (既存と共通)

**Path 計算**:
- WPF `BezierSegment(BeginControlPoint, EndControlPoint, EndPoint)` を `StartPoint = BeginPoint` の PathFigure に乗せる

### 3.3 アンカー機構 (Snap Target、Q-5 / Q-6 / Q-7 / Q-8 反映)

#### 3.3.1 暗黙の 9 点アンカー (Q-5 案 A)
全 `DesignerItemViewModelBase` 派生に対し、**暗黙で** 9 点アンカー (4 角 + 4 辺中央 + 中心) を仮想的に持たせる。`AnchorViewModel` を 9 個作るのではなく、`AnchorRef = "{ownerId}#tl"` のような **位置予約語** で識別。

| 位置 | 予約語 | 相対座標 (RelativeX, RelativeY) |
|---|---|---|
| 左上 | `tl` | (0.0, 0.0) |
| 上中央 | `tc` | (0.5, 0.0) |
| 右上 | `tr` | (1.0, 0.0) |
| 左中央 | `lc` | (0.0, 0.5) |
| 中心 | `c`  | (0.5, 0.5) |
| 右中央 | `rc` | (1.0, 0.5) |
| 左下 | `bl` | (0.0, 1.0) |
| 下中央 | `bc` | (0.5, 1.0) |
| 右下 | `br` | (1.0, 1.0) |

シリアライズの軽量化と UX 両立を狙う (案 A の理由)。

#### 3.3.2 ユーザー追加アンカー (Q-6 案 A)
専用ツールバー「アンカー追加」(MDI vector-point-edit 系アイコン) を選択 → 図形クリックで Anchor を追加する。

**`AnchorViewModel` (新規)**:
- `Id : Guid` — アンカー一意 ID
- `OwnerId : Guid` — 紐づく DesignerItem の ID
- `RelativeX : double` (0.0〜1.0) — 図形の Bounds に対する相対 X
- `RelativeY : double` (0.0〜1.0) — 図形の Bounds に対する相対 Y
- `Name : string?` — UI 表示用のラベル (省略可)

#### 3.3.3 吸着判定 (Q-7 案 C)
吸着距離はグローバル設定 (`Settings.AnchorSnapDistance`) で変更可能 (デフォルト 10 px)。プロジェクトファイルではなくアプリ設定に保存する。

#### 3.3.4 動作
- 図形を Move / Resize / Rotate するとアンカーの絶対座標が R3 リアクティブに再計算される (暗黙 9 点 + 明示 Anchor 両方)
- コネクタの `BeginAnchorRef` / `EndAnchorRef` が設定されていれば、コネクタ端点は対応アンカーの絶対座標に追従

#### 3.3.5 アンカー削除時の挙動 (Q-8 案 B)
アンカーを削除すると、その Anchor を参照しているコネクタ (BeginAnchorRef / EndAnchorRef が一致するもの) も **一緒に削除** される。Undo で復元可能。
※暗黙の 9 点アンカーは「削除不可」(常に図形に紐づく)。明示 Anchor (`AnchorViewModel`) のみ削除対象。

### 3.4 ノード - エッジ風グラフ (Q-11 案 B 反映)

#### 3.4.1 IsNode フラグ
`DesignerItemViewModelBase` に `bool IsNode { get; set; }` を追加 (デフォルト false)。

#### 3.4.2 グラフモード UI
Phase 3 内で実装する最小グラフモード UI:
- 選択中の DesignerItem が `IsNode = true` の場合、その図形のアンカーに繋がっている **全コネクタを強調表示** (太線 / 色変化)
- コネクタの反対端の DesignerItem も `IsNode = true` なら **薄く強調** (関連ノード表示)
- 「ノード化 / ノード解除」を図形右クリックメニューから切替可能

将来 Phase 3.5+ では: 自動配置 / グラフ階層表示 / エッジラベル等を追加可能。

---

## 4. データモデル

### 4.1 全体図

```
DesignerItemViewModelBase (既存、IsNode のみ追加)
  └─ IsNode : bool  (新規、デフォルト false)
  └─ Anchors : ObservableCollection<AnchorViewModel>  (新規)

AnchorViewModel (新規)
  ├─ Id : Guid
  ├─ OwnerId : Guid
  ├─ RelativeX : double  (0..1)
  ├─ RelativeY : double  (0..1)
  └─ Name : string?

ConnectorBaseViewModel (既存)
  └─ (Phase 3 では触らない。AnchorRef は新規派生だけが持つ)

OrthogonalConnectorViewModel : ConnectorBaseViewModel  (新規)
  ├─ BeginPoint / EndPoint : Point
  ├─ RoutingMode : OrthogonalRoutingMode (Auto/HFirst/VFirst/Manual)
  ├─ MidPoints : ObservableCollection<Point>
  ├─ CornerRadius : double
  └─ BeginAnchorRef / EndAnchorRef : Guid|string?
     (Guid = 明示 AnchorViewModel の Id, "{ownerId}#tl" 形式 = 暗黙アンカー予約語)

AnchorBezierConnectorViewModel : ConnectorBaseViewModel  (新規)
  ├─ BeginPoint / EndPoint : Point
  ├─ BeginControlPoint / EndControlPoint : Point
  └─ BeginAnchorRef / EndAnchorRef : Guid|string?
```

### 4.2 クラス配置案
- `boilersGraphics\Models\Connectors\` (新規) — 接続線関連の Model 層
- `boilersGraphics\Models\Anchors\` (新規) — アンカー関連の Model 層
- `boilersGraphics\ViewModels\Connectors\` (新規) — VM 層
- `boilersGraphics\ViewModels\Anchors\` (新規) — VM 層

### 4.3 既存 Connector との関係
- `StraightConnectorViewModel` / `BezierCurveViewModel` / `PolyBezierViewModel` は触らない (Q-1 案 B 反映)
- `ConnectorBaseViewModel` も基本触らない (Phase 3 後の影響範囲を最小化)
- AnchorRef は新規 `OrthogonalConnectorViewModel` / `AnchorBezierConnectorViewModel` のみが持つ

### 4.4 AnchorRef の表現
`BeginAnchorRef` / `EndAnchorRef` の型は `string?` とし、以下のフォーマットで表現:
- 明示 Anchor: `Guid` の文字列表現 (例: `"a1b2c3d4-...-..."`)
- 暗黙 9 点: `"{ownerId}#{position}"` (例: `"a1b2c3d4-...-...#tl"`)

検索ロジック: `#` の有無で分岐。`#` を含めばオーナー ID と位置予約語で解決、なければ Guid と見て `AnchorViewModel` を `AllItems` 経由で検索。

---

## 5. 編集動作仕様

### 5.1 ツールバー / メニューからの追加
画面左ツールバーに以下のツールを追加:
- **「L 字コネクタ」** (MDI vector-polyline 系アイコン、`orthogonal_dark.png`)
- **「ベジエコネクタ」** (MDI vector-bezier 系アイコン、`anchorbezier_dark.png`) ※既存 BezierCurve とは別アイコン
- **「アンカー追加」** (MDI vector-point-edit 系アイコン、`anchor_dark.png`、Q-6 案 A)

### 5.2 L 字コネクタの作成フロー
1. ツールを選択
2. キャンバス上で始点をクリック (吸着距離内にアンカーがあれば自動で `BeginAnchorRef` 設定)
3. ドラッグ / クリックで終点を指定 (吸着距離内にアンカーがあれば自動で `EndAnchorRef` 設定)
4. デフォルト `RoutingMode = Auto` で自動ルーティング (Q-2 案 A)
5. プロパティパネルから `RoutingMode` 切替、`Manual` 時は `MidPoints` を任意数編集可能 (Q-3 案 B)

### 5.3 アンカー追加フロー (Q-6 案 A)
1. ツールバーから「アンカー追加」を選択
2. 対象の DesignerItem をクリックすると、その図形の上にアンカー候補 (暗黙 9 点 + 既存 Anchor) がハイライト表示
3. 任意位置をクリックして RelativeX/Y を計算し、`AnchorViewModel` を生成
4. ユーザー追加 Anchor は永続化 (シリアライズ対象)

### 5.4 アンカー追従動作
- 図形 (Move / Resize / Rotate) の変化時、R3 リアクティブで Anchor の絶対座標が再計算される
- アンカーに紐づくコネクタ (`BeginAnchorRef` / `EndAnchorRef`) の端点を追従更新

### 5.5 アンカー削除動作 (Q-8 案 B)
- 明示 Anchor (`AnchorViewModel`) を削除すると、その Anchor を参照しているコネクタも一緒に削除
- 暗黙 9 点アンカーは削除不可
- Undo で削除前の状態に復元可能

### 5.6 グラフモード UI (Q-11 案 B)
- 図形右クリック → 「ノード化」/「ノード解除」で `IsNode` 切替
- `IsNode = true` の図形が選択された時、関連コネクタを `EdgeThickness × 1.5` / `EdgeBrush` 反転 で強調表示 (一時的、選択解除で戻る)

### 5.7 Phase 1 パーツ機構との関係 (Q-9 案 A)
- パーツに L 字コネクタ / ベジエコネクタ / Anchor / IsNode 付き図形を含められる
- 公開可能プロパティ:
  - **OrthogonalConnector**: `BeginPoint` / `EndPoint` / `CornerRadius` / `EdgeBrush` / `EdgeThickness`
  - **AnchorBezierConnector**: `BeginPoint` / `EndPoint` / `BeginControlPoint` / `EndControlPoint` / `EdgeBrush` / `EdgeThickness`
  - **Anchor**: `RelativeX` / `RelativeY`
  - **DesignerItem**: `IsNode`
- Phase 1 の 8 型 (Double / Int / Boolean / Point / Color / Brush / String / Enum) でカバー、型追加不要

---

## 6. シリアライズ仕様

### 6.1 プロジェクトファイル拡張
- 既存 `<DesignerItems>` 配下に並べる方針 (Phase 2-e と同じ、Q-4 案 A 流儀)
- 新タグ:
  - `<OrthogonalConnector ...>` — 既存コネクタと同じ枠組み + Orthogonal 固有プロパティ
  - `<AnchorBezierConnector ...>` — 既存ベジエとは別の新規コネクタ
  - `<Anchor ...>` — `AnchorViewModel` のシリアライズ
- 既存 `<DesignerItem>` 直下に `<IsNode>` 子要素を追加

### 6.2 プロパティ命名衝突回避
- プレフィックス: `Orthogonal*` / `AnchorBezier*` / `Anchor*` / `IsNode`
- 例: `OrthogonalRoutingMode` / `OrthogonalCornerRadius` / `OrthogonalMidPoints` / `OrthogonalBeginAnchorRef`

### 6.3 後方互換
- 旧プロジェクトファイルには新タグが存在しないので、読み込み時に未存在ならスキップ / デフォルト値
- 既存 StraightConnector / BezierCurve / PolyBezier は触らないので影響なし
- `IsNode` 未存在時は false がデフォルト

### 6.4 AnchorRef のシリアライズ
- `string` 型でそのまま保存: `"<Guid>"` または `"<OwnerGuid>#<position>"`
- 復元時は `string` のまま読み込み、コネクタの Path 計算時に解決

---

## 7. 既存基盤との接続点

実装着手 (Phase 3-b) 時に詳細を確認すべき既存コードの位置:

| 機能 | ファイルパス | 役割 |
|---|---|---|
| Connector 基底 | `boilersGraphics\ViewModels\ConnectorBaseViewModel.cs:14` | Points / EdgeBrush / EdgeThickness / SnapPoint0VM / SnapPoint1VM |
| 直線 (touchしない) | `boilersGraphics\ViewModels\StraightConnectorViewModel.cs:12` | 2 点直線 |
| ベジエ (touchしない) | `boilersGraphics\ViewModels\BezierCurveViewModel.cs:15` | 制御点 2 つ |
| PolyBezier (touchしない) | `boilersGraphics\ViewModels\PolyBezierViewModel.cs:15` | 任意 N 点 |
| Snap (吸着、参考) | `boilersGraphics\ViewModels\SnapPointViewModel.cs:10` | コネクタ端点専用 (Phase 3 では別系統の Anchor を新規実装) |
| Connect (touchしない) | `boilersGraphics\ViewModels\SelectableDesignerItemViewModelBase.cs:273` | SnapPointAdsorptionInformation で Subscribe ベース接続 |
| Serializer | `boilersGraphics\Helpers\ObjectSerializer.cs:331-352` | コネクタの Points / ControlPoint シリアライズ (Phase 3 でも踏襲) |
| Deserializer | `boilersGraphics\Helpers\ObjectDeserializer.cs:401-428` | XML から ConnectorBaseViewModel 復元 (Phase 3 でも踏襲) |
| DesignerItem 基底 | `boilersGraphics\ViewModels\DesignerItemViewModelBase.cs` | `IsNode` プロパティ追加先 |

---

## 8. テスト戦略

`boilersGraphics.Test` に追加するテスト案:

1. **モデル単体**:
   - OrthogonalConnector の Auto ルーティング (H-First / V-First の閾値判定、|dx| vs |dy| の比較)
   - MidPoints 編集後の Manual モード Path 生成 (0..N 任意数)
   - CornerRadius > 0 時の ArcSegment 挿入確認
   - Anchor の RelativeX/Y から絶対座標計算 (図形の Move / Resize / Rotate 各ケース)
   - 暗黙 9 点アンカーの予約語解決 (`#tl` 〜 `#br` の 9 通り)
2. **シリアライズ**:
   - OrthogonalConnector / AnchorBezierConnector / Anchor / IsNode を含むプロジェクトファイルが保存 → 読み込みで完全復元
   - AnchorRef (Guid / 予約語両方) のラウンドトリップ
   - 旧形式の読み込みでクラッシュしない (後方互換)
3. **接続追従**:
   - アンカーに繋いだコネクタが、図形を Move した時に追従する
   - 明示 Anchor 削除時にコネクタが一緒に削除される (Q-8 案 B 検証)
   - 暗黙 9 点アンカーは削除不可 (図形は削除されない限り常に存在)
4. **グラフモード**:
   - `IsNode = true` の図形を選択するとコネクタが強調表示される
   - IsNode の切替 (右クリックメニュー) が動く
5. **パーツ機構との統合**:
   - OrthogonalConnector + Anchor を含むパーツ定義が Promote/Detach/Clone で完全に動く
   - 公開パラメータ経由で BeginPoint / CornerRadius / IsNode が変更できる
6. **既存テスト**: [[project_test_baseline]] (Phase 3-b 着手時点 1253 件) はすべて緑のまま

---

## 9. Phase 3 のサブフェーズ分割 (Q-11 案 B 反映、Phase 3.5 後送り部分は最小)

### Phase 3 (今リリース対象)
- **3-a**: 設計仕様書 (本ドキュメント) を確定 — Q-1 〜 Q-11 全件確定で **完了**
- **3-b**: Anchor 機構 (Model + VM + 暗黙 9 点 + 明示 AnchorViewModel + 「アンカー追加」ツール)
- **3-c**: OrthogonalConnector 実装 (Auto / HFirst / VFirst / Manual モード + CornerRadius)
- **3-d**: AnchorBezierConnector 実装 (新規ベジエコネクタ、Anchor 接続)
- **3-e**: コネクタ — Anchor 追従機構 (R3 リアクティブ配線、IsNode フラグ追加)
- **3-f**: シリアライズ対応 (OrthogonalConnector / AnchorBezierConnector / Anchor / IsNode / AnchorRef)
- **3-g**: グラフモード UI (`IsNode` 視覚化、選択時の関連コネクタハイライト、右クリック「ノード化」/「ノード解除」)
- **3-h**: Phase 1 パーツ機構との統合 (公開パラメータ Binding ターゲット化、Q-9 確定スコープ)
- **3-i**: 吸着距離のグローバル設定 UI (Q-7 案 C)
- **3-j**: チュートリアル整備 (`docs/fui/phase3-tutorial.md`)

### Phase 3.5 (後送り、機能拡張のみ)
- **3.5-a**: コネクタの再ルーティング高度化 (図形回避アルゴリズム)
- **3.5-b**: 自動配置アルゴリズム (force-directed / hierarchical)
- **3.5-c**: エッジラベル (コネクタ中点に文字を載せる)
- **3.5-d**: ズーム連動の吸着距離

---

## 10. オープン問題 (要決定事項)

すべての設計判断は確定済み (2026-05-13、作者判断)。

### Q-1. ベジエコネクタは既存 `BezierCurveViewModel` の拡張 / 完全新規のいずれにするか? ✅ **確定**
- **採用: 案 B (`AnchorBezierConnectorViewModel` 完全新規)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: 既存 Letter / コネクタ系を一切触らない方針 (§2.1 既存コードへの敬意、Phase 1/2 と同じ哲学)。コード重複は Phase 3 のスコープ内であれば許容。
- **不採用**: 案 A (既存拡張) — 既存テストへの影響範囲が読みづらい / 案 C (ConnectorBase に追加) — 全コネクタへの影響が大きい

### Q-2. L 字コネクタのデフォルト RoutingMode は? ✅ **確定**
- **採用: 案 A (Auto)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: 始点・終点の差分から自然な向きを選択するので、初手の見た目が直感的。固定モードへの切替は後からプロパティパネルで可能なので柔軟性も担保される。

### Q-3. L 字コネクタの折れ点 (MidPoints) は何個まで? ✅ **確定**
- **採用: 案 B (常に任意数 0..N)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: 配線図の自由度を最大化。Auto/HFirst/VFirst は内部計算で 1 個生成、Manual は明示 0..N 個。シリアライズも `MidPoints` を一律 ObservableCollection<Point> として扱える。
- **不採用**: 案 A (1 個固定 + Manual のみ任意) — Manual モードへの切替コストが UI 上で発生 / 案 C (最大 2 個) — 過剰な制限

### Q-4. CornerRadius (折れ点の角丸) は実装する? ✅ **確定**
- **採用: 案 A (実装する、デフォルト 0)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: PathGeometry の `ArcSegment` を挟むだけの軽実装で、FUI らしさが大幅に上がる (現実の機械系 FUI では丸角が一般的)。

### Q-5. Anchor のプリセット (Bounds 9 点) は? ✅ **確定**
- **採用: 案 A (全 DesignerItem に暗黙 9 点)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: シリアライズが軽い、UX が直感的、暗黙 9 点は AnchorRef 予約語 (`#tl` 等) で解決。プロジェクトファイルに 9 個 × N 図形分の Anchor 要素が出ない。

### Q-6. Anchor 追加 UI は? ✅ **確定**
- **採用: 案 A (専用ツールバー「アンカー追加」)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: 既存ツール (矩形 / 楕円 / モノテキスト等) と同じ操作体系。Behavior + Adorner + ToolBar 追加で他テキスト系と一貫。

### Q-7. コネクタの吸着距離 (吸着判定の閾値 px) は? ✅ **確定**
- **採用: 案 C (ユーザー設定、グローバル)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: ユーザーの作業スタイル (細かい配線 vs ざっくり配線) に応じて変更できる柔軟性。アプリ設定 (Settings ファイル) に保存、デフォルト 10 px。
- **実装上の注意**: プロジェクトファイルではなくグローバル設定 (`Properties\Settings.settings` 系) に保存して、複数プロジェクト間で共有

### Q-8. アンカーを削除したらコネクタの参照はどうなる? ✅ **確定**
- **採用: 案 B (コネクタごと削除)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: Anchor 削除時の「自由端コネクタが残る」状況を避け、データの一貫性を保つ。Undo で復元できるのでユーザーの不安を緩和。
- **実装上の注意**: 削除前にユーザーへ「関連コネクタ N 本も削除されます」を通知 (Dialog ではなく Statusbar 等、軽い告知)

### Q-9. Phase 1 パーツ機構との統合粒度 ✅ **確定**
- **採用: 案 A (主要プロパティのみ ExposedProperty で公開可能)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: Phase 1 の 8 型 (Double / Int / Boolean / Point / Color / Brush / String / Enum) でカバー可能。型追加不要、スコープがクリーン。
- **公開可能スコープ**:
  - OrthogonalConnector: `BeginPoint` / `EndPoint` / `CornerRadius` / `EdgeBrush` / `EdgeThickness`
  - AnchorBezierConnector: `BeginPoint` / `EndPoint` / `BeginControlPoint` / `EndControlPoint` / `EdgeBrush` / `EdgeThickness`
  - Anchor: `RelativeX` / `RelativeY`
  - DesignerItem: `IsNode`
- **非公開**: RoutingMode / MidPoints / AnchorRef (骨格部分は非公開)

### Q-10. UI 文言 ✅ **確定**
- **採用: 案 A (日本語維持)**
- 確定日: 2026-05-13 (作者判断)
- **採用文言**: 「L 字コネクタ」「ベジエコネクタ」「アンカー」「ノード」「エッジ」
- **採用理由**: 既存 boilersGraphics の UI が日本語、Phase 2 (「モノスペーステキスト」「データジェネレータ」「数値列」等) と一貫。

### Q-11. Phase 3 のリリース粒度 ✅ **確定**
- **採用: 案 B (グラフモード UI も Phase 3 に含める)**
- 確定日: 2026-05-13 (作者判断)
- **採用理由**: ノード - エッジ表現は FUI の「関係性可視化」の肝。最小グラフモード UI (IsNode 視覚化 + 関連コネクタ強調) は実装軽く、リリース 1 本で「実用的な FUI 関係表現が組める」状態に持っていける。
- **Phase 3 スコープ**: L 字 + 新規ベジエ + Anchor + Anchor 追従 + シリアライズ + グラフモード UI + パーツ統合 + 吸着距離設定 + チュートリアル
- **Phase 3.5 (後送り)**: 図形回避ルーティング / 自動配置 / エッジラベル / ズーム連動吸着

---

## 11. Phase 3-a 完了基準

このドキュメントが以下を満たすことをもって Phase 3-a 完了とする:

- [x] §10 のオープン問題すべてに作者の判断が反映されている (Q-1〜Q-11 全件確定)
- [x] §4 のデータモデル図がレビュー済み (Q-1 で新規派生確定 / Q-9 で公開プロパティ確定)
- [x] §6 のシリアライズ仕様が既存形式と矛盾しない (Q-4 流儀の `<DesignerItems>` 配下 + プレフィックス)
- [x] このドキュメントが `docs/fui/phase3-connectors.md` に保存されている
- [x] 後続の Phase 3-b 以降で参照されるべき既存コード位置がリストアップされている (§7)
- [x] Phase 1 のパーツ機構との統合点が明示されている (§5.7, §9, Q-9)

**Phase 3-a 完了。Phase 3-b (実装着手) に進む準備が整った。**

---

## 12. 確定事項サマリー (Phase 3-b 実装時のクイックリファレンス)

| 項目 | 確定内容 |
|---|---|
| 実装方針 | 完全新規 `OrthogonalConnectorViewModel` / `AnchorBezierConnectorViewModel` (既存 Straight/Bezier/PolyBezier には手を出さない) |
| 公開パラメータ型 | Phase 1 の 8 型をそのまま流用 (型追加なし) |
| L 字デフォルトモード | Auto (始点・終点の差分で H/V-First 自動選択) |
| L 字 MidPoints | 任意数 (0..N)、Manual モードで明示編集可 |
| CornerRadius | 実装、デフォルト 0 (直角)、ArcSegment で実装 |
| Anchor プリセット | 暗黙 9 点 (4 角 + 4 辺 + 中心)、`{ownerId}#tl` 形式の予約語で識別 |
| ユーザー追加 Anchor | `AnchorViewModel` (Id / OwnerId / RelativeX / RelativeY / Name) |
| Anchor 追加 UI | 専用ツール「アンカー追加」(Behavior + Adorner + ToolBar) |
| 吸着距離 | グローバル設定、デフォルト 10 px |
| Anchor 削除時挙動 | 参照コネクタごと削除 (Undo で復元可能) |
| シリアライズ | `<DesignerItems>` 配下に `<OrthogonalConnector>` / `<AnchorBezierConnector>` / `<Anchor>` + `<IsNode>` |
| AnchorRef 表現 | `string`: Guid 文字列 or `{ownerId}#{position}` |
| パーツ統合粒度 | 主要プロパティのみ ExposedProperty 公開可能 (骨格は非公開) |
| UI 文言 | L 字コネクタ / ベジエコネクタ / アンカー / ノード / エッジ |
| Phase 3 スコープ | L 字 + 新規ベジエ + Anchor + 追従 + シリアライズ + グラフモード UI + パーツ統合 + 吸着距離設定 + チュートリアル |

---

*Last updated: 2026-05-13 (確定版 v1.0)*
*Reviewer: dhq_boiler*
