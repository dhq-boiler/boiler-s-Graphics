# Phase 3 接続線・関係表現 チュートリアル

> 関連: 設計仕様書 [`phase3-connectors.md`](./phase3-connectors.md)
> 前提: [`phase1-tutorial.md`](./phase1-tutorial.md) を読んでパーツ機構の基本操作を理解していること。

このドキュメントは Phase 3 で追加された **接続線・関係表現** の操作ガイドです。
L 字 / ベジエコネクタで図形同士を結び、アンカーで吸着追従させ、ノード - エッジ風グラフを Phase 1 のパーツ機構に組み込むまでを解説します。

---

## 1. Phase 3 で追加された要素

| ツール | UI 表記 | 内部型 | 用途 |
|---|---|---|---|
| L 字コネクタ | 「L 字コネクタ」 | `OrthogonalConnectorViewModel` | 直角に折れる配線図 / 機械系 FUI 向けコネクタ |
| ベジエコネクタ (Anchor 接続版) | 「ベジエコネクタ」 | `AnchorBezierConnectorViewModel` | 曲線フロー / 神経網風 FUI 向けコネクタ |
| アンカー追加 | 「アンカー」 | `AnchorViewModel` | 図形に追加可能な吸着点 (図形 ID + 相対座標で管理) |
| ノード化 / ノード解除 | 右クリックメニュー | `DesignerItemViewModelBase.IsNode` | 選択した DesignerItem をノードに昇格 / 戻す |

既存の `StraightConnector` / `BezierCurve` / `PolyBezier` には触っていないため、従来の直線・曲線ツールも問題なく使えます (Q-1 案 B)。

---

## 2. L 字コネクタ (OrthogonalConnector)

直角に折れる接続線。配線図やラックダイアグラム、機械系 FUI のパイプラインなどで使います。

### 2.1 配置する
1. 左ツールバーの **「L 字コネクタ」** (MDI vector-polyline アイコン) を選択。
2. キャンバスでドラッグ開始 → 始点が確定し、マウスに追従して赤いプレビュー線が描画されます。
3. ドラッグ終点でリリース → コネクタが確定。デフォルトは `RoutingMode = Auto` で、|dx| ≥ |dy| なら水平先行 (HFirst)、それ以外は垂直先行 (VFirst) で 1 中間点が自動で挿入されます。

### 2.2 プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `RoutingMode` | `OrthogonalRoutingMode` | `Auto` | `Auto` / `HFirst` / `VFirst` / `Manual`。Manual は MidPoints をユーザが直接編集 |
| `MidPoints` | `ObservableCollection<Point>` | 空 | 折れ点列。Manual では 0..N 個任意 (Q-3 案 B) |
| `CornerRadius` | `double` | `0` | 折れ点をアークで丸める半径。隣接辺の半分にクランプ |
| `BeginAnchorRef` | `string` | `""` | 始点アンカー参照。詳細は §4 |
| `EndAnchorRef` | `string` | `""` | 終点アンカー参照 |

### 2.3 BeginPoint / EndPoint のプロキシ
`Points[0]` / `Points[1]` をそれぞれ `BeginPoint` / `EndPoint` (`BindableReactiveProperty<Point>`) として公開しています。これは Phase 3-h のパーツ統合 (§6) で Binding ターゲットになれるようにするためで、内部的には Points と双方向同期します。

---

## 3. ベジエコネクタ (AnchorBezier)

始点・終点 + 制御点 2 つで曲線を描く、Anchor 接続対応の完全新規コネクタ。既存の `BezierCurveViewModel` には触りません (Q-1 案 B)。

### 3.1 配置する
1. 左ツールバーの **「ベジエコネクタ」** (MDI vector-bezier アイコン) を選択。
2. ドラッグで始点 → 終点を指定。
3. リリースで確定。制御点は始点〜終点ベクトルの 1/3, 2/3 位置に自動配置されます。

### 3.2 プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `BeginPoint` / `EndPoint` | `Point` | Points[0] / Points[1] のプロキシ (パーツ統合用) |
| `BeginControlPoint` | `Point` | 始点側制御点 (絶対座標) |
| `EndControlPoint` | `Point` | 終点側制御点 (絶対座標) |
| `BeginAnchorRef` / `EndAnchorRef` | `string` | アンカー参照 (§4 参照) |

---

## 4. アンカー (Anchor) と吸着

アンカーは「図形に紐づく吸着点」です。コネクタの端点が吸着し、図形を動かすとコネクタも追従します (Phase 3-e の AnchorFollower)。

### 4.1 暗黙 9 点アンカー (Q-5 案 A)
すべての `DesignerItem` は AnchorViewModel を生成しなくても **暗黙的に 9 点** (4 隅 + 4 辺中央 + 中心) の吸着点を持ちます。これらは AnchorRef 文字列の形式 `{ownerGuid}#{position}` で表現されます。

| 略号 | 位置 |
|---|---|
| `tl` `tc` `tr` | 上左 / 上中央 / 上右 |
| `cl` `c` `cr` | 左中央 / 中心 / 右中央 |
| `bl` `bc` `br` | 下左 / 下中央 / 下右 |

例: コネクタの始点が `aabbccdd-1122-3344-5566-778899aabbcc#tl` を参照していれば、その図形の左上角に追従します。

### 4.2 明示アンカー (AnchorViewModel)
9 点以外の任意位置に吸着点を置きたい場合は **「アンカー追加」** ツール (MDI vector-point-edit アイコン) を使います。

1. 左ツールバーから「アンカー」を選択。
2. 図形 (DesignerItem) の内側をクリック → 図形の AABB に対する相対座標 (`RelativeX`, `RelativeY`) を計算してオレンジ 8×8 円が配置されます。
3. アンカーは図形の Left/Top/Width/Height/RotationAngle の変化を Subscribe して追従します。

AnchorRef 文字列は **Guid 単体** (`#` を含まない) になり、対応する `AnchorViewModel.ID` を逆引きします。

### 4.3 コネクタが吸着する瞬間
L 字 / ベジエコネクタをドラッグ確定した瞬間、始点・終点それぞれについて `AnchorSnap.FindNearestAnchorRef` が走り、**吸着距離 (デフォルト 10 px)** 以内に暗黙 9 点 / 明示アンカーがあれば AnchorRef を確定します。確定された AnchorRef は `BeginAnchorRef` / `EndAnchorRef` に書き込まれ、`StartAnchorFollowers()` で追従購読が起動します。

### 4.4 吸着距離を変更する (Phase 3-i / Q-7 案 C)
設定 → 設定ダイアログ → 「アンカー吸着距離 (px)」で変更できます。0 にすると事実上「クリック点と一致するときだけ吸着」になります。グローバル設定で、現状はアプリ起動毎に 10 px にリセットされます (Properties\Settings.settings への永続化は将来対応)。

---

## 5. グラフモード UI (IsNode + 関連エッジ強調)

Q-11 案 B: 任意の DesignerItem を「ノード」とみなして、選択時に関連コネクタを視覚強調する仕組みです。

### 5.1 ノードに昇格する
1. 図形を選択。
2. 右クリックメニューから **「ノード化 / ノード解除」** をクリック。
3. `IsNode = true` になります。複数選択していれば全部 true、すでに全部 true なら全部 false に反転します。

### 5.2 強調表示の仕組み
`IsNode = true` のノードが選択されると、`NodeHighlightController` が以下の関連コネクタを抽出して強調します。

- `BeginAnchorRef` / `EndAnchorRef` がそのノードを暗黙参照 (`{nodeId}#xx`)
- そのノード ID を `OwnerId` に持つ AnchorViewModel を明示 Guid 参照

強調内容:
- **EdgeThickness** が 1.5 倍
- **EdgeBrush** の色を反転 (SolidColorBrush 限定)

選択を解除すると元の値に戻ります。強調表示中にユーザが EdgeBrush / EdgeThickness を手動編集すると、戻し時に元の値で上書きされる制約があります (§7 既知制約)。

---

## 6. Phase 1 パーツ機構との統合 (Q-9 案 A)

Phase 3 で追加された型もパーツに含めて再利用できます。

### 6.1 含められる型
- `OrthogonalConnectorViewModel`
- `AnchorBezierConnectorViewModel`
- `AnchorViewModel`
- `IsNode = true` な任意 DesignerItem

### 6.2 公開可能プロパティ (Q-9 案 A)
パーツ編集画面の「Phase 3 公開可能プロパティ」セクションでピン留めできます。

| 型 | 公開可能プロパティ |
|---|---|
| OrthogonalConnector | `BeginPoint` / `EndPoint` / `CornerRadius` / `EdgeBrush` / `EdgeThickness` |
| AnchorBezierConnector | `BeginPoint` / `EndPoint` / `BeginControlPoint` / `EndControlPoint` / `EdgeBrush` / `EdgeThickness` |
| Anchor | `RelativeX` / `RelativeY` |
| DesignerItem | `IsNode` |

非公開: `RoutingMode` / `MidPoints` / `AnchorRef` の骨格部分 (パーツ利用者には触らせたくない構造的属性のため)。

### 6.3 例: 「2 つの円 + L 字結線 + ラベル」を 1 パーツ化
1. 円 A、円 B を配置。両方とも IsNode 化。
2. L 字コネクタで円 A → 円 B を接続 (始点・終点とも 9 点アンカーに吸着)。
3. ラベル (モノスペーステキスト) を 1 つ配置。
4. すべてを範囲選択 → 右クリック → **「パーツ化…」** → 名前を入力。
5. パーツ編集画面で:
   - 円 A の `FillBrush` をピン留め (ノードの色を外部から変えられるように)
   - L 字コネクタの `CornerRadius` をピン留め (折れ点の丸みを調整可能に)
   - ラベルの `Text` をピン留め
6. パーツインスタンスを配置すると、ピン留めしたプロパティだけが外部から触れます。

---

## 7. 既知の制約

| 制約 | 影響 | 回避策 / 解消予定 |
|---|---|---|
| Rotation 非 0 の図形にアンカー追加 | RelativeX/Y 計算は AABB 近似 | 回転 0 でアンカー配置 → その後回転。Phase 3.5 で逆回転変換に改善余地 |
| 強調表示中の手動 EdgeBrush 変更 | 選択解除時に元の値で上書き | Phase 3.5 で「derived view property」化を検討 |
| 吸着距離の永続化 | アプリ起動毎にデフォルト 10 px にリセット | Properties\Settings.settings 経由化を将来対応 |
| AnchorViewModel 削除時の関連コネクタ | 自動削除されない (Q-8 案 B は未実装) | Phase 3-f / 3-g 後の宿題。手動でコネクタも消す |
| BeginPoint/EndPoint Binding | 連動する `Points` は ObservableCollection なので、UI から直接参照しにくい | プロキシ経由で参照する (Phase 3-h で公開済み) |

---

## 8. 関連ドキュメント

- 設計仕様: [`phase3-connectors.md`](./phase3-connectors.md) — Q-1 〜 Q-11 の確定事項と §12 のサマリー表
- Phase 1: [`phase1-tutorial.md`](./phase1-tutorial.md) / [`phase1-parametric-components.md`](./phase1-parametric-components.md)
- Phase 2 / 2.5: [`phase2-tutorial.md`](./phase2-tutorial.md) / [`phase2-5-tutorial.md`](./phase2-5-tutorial.md)
- 上位設計: [`../FUI_DESIGNER_INTENT.md`](../FUI_DESIGNER_INTENT.md)
