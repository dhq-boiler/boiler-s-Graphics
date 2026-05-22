# boiler's Graphics の使い方

## boiler's Graphicsを起動する

アプリケーション(boilersGraphics.exe)が配置されているフォルダを開き、boilersGraphics.exeをダブルクリックします。

## 図形を描画する

### 直線を描画する

1. 画面左側のツールから直線ツールを選択します。
2. キャンパス上でドラッグして直線を描画します。

![draw_straight_line](https://user-images.githubusercontent.com/7916855/128829810-e9b51cd9-ac7a-4904-a93d-703a53df85dc.gif)

### 四角形を描画する

1. 画面左側のツールから四角形ツールを選択します。
2. キャンパス上でドラッグして四角形を描画します。

![draw_rectangle](https://user-images.githubusercontent.com/7916855/128829838-3e35fdf2-649b-4ca8-9435-51bdca7380ff.gif)

### 円を描画する

1. 画面左側のツールから円ツールを選択します。
2. キャンパス上でドラッグして円を描画します。
3. Shift押しながらドラッグすると幅と高さが等しい円が描けます。
4. Alt押しながらドラッグすると、マウスダウンした点を中心にした円が描けます。

![draw_ellipse](https://user-images.githubusercontent.com/7916855/128829886-5a8dee3f-7015-40c2-af7c-4ad3247a8048.gif)

### 多角形を描画する

1. 画面左側のツールから多角形ツールを選択します。
2. 多角形ウィンドウが表示されます。ここでは、多角形を構成する頂点の情報を入力します。頂点の情報は角度（度）と半径です。頂点の情報は追加ボタンを押下することで、追加できます。必要のない頂点の情報は削除ボタンで削除することができます。
3. 多角形を構成する頂点の情報を入力したら、始点Xと始点Yを入力します。
4. 描画ボタンを押下します。
5. キャンパス上でドラッグして多角形を描画します。

![draw_polygon](https://user-images.githubusercontent.com/7916855/128829906-cd101656-9c0e-449d-b77e-409e291e4e7f.gif)

### ベジエ曲線を描画する

1. 画面左側のツールからベジエ曲線ツールを選択します。
2. キャンパス上でドラッグしてベジエ曲線を描画します。
3. ベジエ曲線を描画すると、制御点１と制御点２が現れます。これらをドラッグしてベジエ曲線の制御点の位置を調節します。

![draw_bezier](https://user-images.githubusercontent.com/7916855/128829931-262a1055-1943-4f3c-a466-39101664dcd2.gif)

## 画像ファイルを埋め込む

1. 画面左側のツールから画像ツールを選択します。
2. ファイルダイアログが開きます。描画したい画像ファイルを選択します。
3. キャンパス上でドラッグして画像を描画します。
4. Shift押しながらドラッグすると画像のアスペクト比を維持したまま描画できます。

![draw_picture](https://user-images.githubusercontent.com/7916855/129468587-624de219-6fc4-44e1-b3f8-667614bd4dc0.gif)

## 文字を描画する

### 文字ツール

1. 画面左側のツールから文字ツールを選択します。
2. キャンパス上でドラッグして描画範囲を決定します。
3. 画面左側のツールから選択ツールを選択します。
4. 2で決定した描画範囲をクリックして選択します。
5. レタリングダイアログが開きます。ここでは描画する文字列、フォント、太字か、イタリックか、フォントサイズ、自動改行するかを指定します。
6. 例えば、文字列：boiler's Graphics、フォント：メイリオ、太字：チェックしない、イタリック：チェックしない、フォントサイズ：30、自動改行：チェックしないの場合は下記GIFアニメのようになります。

![draw_letter](https://user-images.githubusercontent.com/7916855/129468591-38274328-bf44-4919-85b4-b38c21a0316e.gif)

### 縦書きツール

1. 画面左側のツールから縦書きツールを選択します。
2. キャンパス上でドラッグして描画範囲を決定します。
3. 画面左側のツールから選択ツールを選択します。
4. 2で決定した描画範囲をクリックして選択します。
5. レタリングダイアログが開きます。ここでは描画する文字列、フォント、太字か、イタリックか、フォントサイズ、自動改行するかを指定します。
6. 例えば、文字列：ボイラーズグラフィックス、フォント：メイリオ、太字：チェックしない、イタリック：チェックしない、フォントサイズ：30、自動改行：チェックするの場合は下記GIFアニメのようになります。

![draw_letter_vertical](https://user-images.githubusercontent.com/7916855/129468596-4fecea05-ea50-4c43-97d1-d55093d0d440.gif)

### モノスペーステキスト・データジェネレータ・数値列 (FUI 向け)

FUI (Fictional User Interface) 制作向けに、等幅フォント前提のテキスト・自動生成ダミーデータ・連番列を素早く配置できる 3 つの専用ツールが追加されています。デフォルトフォントとして JetBrains Mono をアプリ同梱しています。

> 詳しい使い方とプロパティ仕様は [docs/fui/phase2-tutorial.md](docs/fui/phase2-tutorial.md) を参照してください。

#### モノスペーステキスト

1. 画面左側のツールから「モノスペーステキスト」ツールを選択します。
2. キャンパス上でドラッグして配置範囲を決定します。
3. `Text` プロパティに表示したい文字列 (複数行可) を設定すると、等幅フォントで描画されます。

#### データジェネレータ

1. 画面左側のツールから「データジェネレータ」ツールを選択します。
2. キャンパス上でドラッグして配置範囲を決定します。
3. 配置直後に Hex × 8 件のダミーデータが自動生成されます。
4. `Type` (Hex / Binary / IPv4 / IPv6 / UUID / Timestamp / RandomCode / LogLine の 8 種)、`Seed`、`Count`、`Separator`、`Layout` を変更すると同期的に再生成されます。

#### 数値列

1. 画面左側のツールから「数値列」ツールを選択します。
2. キャンパス上でドラッグして配置範囲を決定します。
3. デフォルト設定 (`0..10` Step `1` Horizontal) で生成された連番列が表示されます。
4. `Start` / `End` / `Step` / `Format` (`"D3"` / `"X4"` / `"F2"` 等の .NET 書式指定子) / `Separator` / `Direction` (Horizontal / Vertical / Grid) を変更すると同期的に再生成されます。

> これら 3 要素は Phase 1 のパーツ機構と統合されており、`ExposedProperty` で主要プロパティ (MonoText: Text/FontSize/Foreground/Background/LetterSpacing、DataGen: Seed/Count/Separator、NumSeq: Start/End/Step/Format/Separator) を公開して再利用パーツの一部にできます。

### テキストマトリクス・テキストパス (FUI Phase 2.5)

Phase 2.5 で「行 × 列の格子状にテキストを並べる」テキストマトリクスと、「PolyBezier に沿って 1 文字ずつ文字を並べる」テキストパスが追加されています。

> 詳しい使い方とプロパティ仕様は [docs/fui/phase2-5-tutorial.md](docs/fui/phase2-5-tutorial.md) を参照してください。

#### テキストマトリクス

1. 画面左側のツールから「テキストマトリクス」ツールを選択します。
2. キャンパス上でドラッグして配置範囲を決定します。
3. デフォルト 4×4 の連番テーブルが表示されます。
4. `CellMode` (Sequential / DataGenerator / CustomList の 3 モード)、`Rows` / `Columns` / `Separator` / 各モード固有のプロパティを変更すると同期的に再生成されます。

#### テキストパス

1. 先にベジエ曲線ツールでパスを描いておきます。
2. 画面左側のツールから「テキストパス」ツールを選択します。
3. キャンパス上でドラッグして配置範囲を決定します。
4. 配置後、`PathReferenceId` に参照する PolyBezier の `ID` (Guid) を設定すると、`Text` が PolyBezier に沿って 1 文字ずつ並びます。
5. `StartOffset` / `Spacing` / `Side` (Above/On/Below) / `Rotation` (Tangent/Upright) で配置を調整できます。

### 接続線・関係表現 (FUI Phase 3)

Phase 3 で「L 字 (Orthogonal) コネクタ」「ベジエコネクタ (Anchor 接続版)」「アンカー (吸着点)」「ノード - エッジ風グラフモード」が追加されています。

> 詳しい使い方とプロパティ仕様、Phase 1 パーツ統合例は [docs/fui/phase3-tutorial.md](docs/fui/phase3-tutorial.md) を参照してください。設計仕様は [docs/fui/phase3-connectors.md](docs/fui/phase3-connectors.md) にあります。

#### L 字コネクタ
1. 画面左側のツールから「L 字コネクタ」を選択します。
2. キャンパス上でドラッグ開始 → 終点でリリースすると、`RoutingMode = Auto` で 1 中間点が自動挿入された L 字が確定します。
3. ドラッグ終端が既存図形の 9 点アンカー (暗黙) や明示 AnchorViewModel に **吸着距離 (デフォルト 10 px)** 以内なら、自動でアンカー参照が設定され、図形移動に追従します。
4. `RoutingMode` を `Manual` にすると `MidPoints` を 0..N 個任意に編集できます。`CornerRadius` で折れ点を丸めることもできます。

#### ベジエコネクタ
1. 画面左側のツールから「ベジエコネクタ」を選択します。
2. ドラッグで始点 → 終点を指定。制御点は自動で 1/3, 2/3 位置に配置されます。
3. アンカー吸着の挙動は L 字コネクタと同じです。

#### アンカー (明示)
1. 画面左側のツールから「アンカー」を選択します。
2. 図形 (DesignerItem) の内側をクリックすると、その図形 ID + 相対座標 (RelativeX/Y) を持つアンカーが配置されます。
3. アンカーは図形の位置・サイズ・回転に追従します。

#### ノード化 / グラフモード UI
1. 任意の図形を選択し、右クリックメニューの「ノード化 / ノード解除」をクリックすると `IsNode` フラグがトグルされます。
2. `IsNode = true` の図形を選択すると、その図形を参照しているコネクタが「太さ 1.5 倍 + 色反転」で強調表示されます。選択解除で元に戻ります。

#### 吸着距離の設定
設定 → 設定ダイアログ → 「アンカー吸着距離 (px)」で全コネクタ共通の吸着距離を変更できます (0 〜 任意。推奨 5 〜 30)。

### スタイル・テーマ・カラー (FUI Phase 4)

Phase 4 で「組込 4 テーマ (Bladerunner / Matrix / MedicalBlueWhite / AmberCrt)」「線種ライブラリ 6 種」「擬似グロー」が追加されています。FUI らしい世界観をテーマ単位で一括適用できます。

> 詳しい使い方とプロパティ仕様、Phase 1 パーツ統合例は [docs/fui/phase4-tutorial.md](docs/fui/phase4-tutorial.md) を参照してください。設計仕様は [docs/fui/phase4-styles-themes.md](docs/fui/phase4-styles-themes.md) にあります。

#### テーマ選択 / パレット適用
1. メニューバー「キャンバス → テーマ選択 / パレット適用...」を選びます。
2. テーマ (Bladerunner / Matrix / MedicalBlueWhite / AmberCrt)、適用範囲 (選択中図形 / アクティブレイヤー / プロジェクト全体)、適用対象 (Edge のみ / Fill のみ / 両方) を選びます。
3. 「適用」ボタンで対象範囲の EdgeBrush / FillBrush がテーマの primary / background 色で書き換わります。Ctrl+Z で取り消し可能です。

#### 線種ライブラリ
ダイアログの「線種」コンボボックスで Solid / Dash / Dot / DashDot / LongDash / Stepped (段階線) のいずれかを選び「適用」を押すと、対象範囲の StrokeDashArray と StrokeLineJoin が書き換わります。未選択なら StrokeDashArray は変更しません。

#### グロー (擬似 DropShadow)
ダイアログの「グロー設定を適用」をチェックして「適用」すると、テーマの DefaultGlow (Radius / Intensity / Color) が対象範囲の図形に流し込まれます。各図形は `GlowRadius > 0` の間 WPF DropShadowEffect でグロー化されます。図形ごとに `GlowRadius = 0` をセットすればグローを外せます。

#### パーツ機構との統合
パーツエディタの「Phase 4 公開可能プロパティ (グロー)」セクションで GlowRadius / GlowIntensity / GlowColor をピン留めすると、パーツインスタンスごとにグローパラメータをカスタマイズできます。

## 描画補助点（スナップポイント）を設置する

1. 画面左側のツールからスナップポイントツールを選択します。
2. キャンパス上で任意の点をクリックしてスナップポイントを追加します。
3. 画面右側のレイヤーウィンドウで追加されたアイテム（スナップポイント）を右クリックし、スナップポイントを移動を選択します。
4. スナップポイントの移動先の点の座標を入力します。
5. スナップポイントを移動したら、そのスナップポイントを使って図形を描画します。

![draw_with_snappoint](https://user-images.githubusercontent.com/7916855/129469177-be822ee9-6b5f-4531-8aa6-4e71c2db7b94.gif)

## グループ化・グループ化解除する

### グループ化

1. 画面左側のツールから選択ツールを選択します。
2. グループ化したい、キャンパス上の任意の図形をクリックします。複数の図形を選択状態にするにはShiftキーを押しながらクリックします。
3. 画面上部のツールバーからグループ化をクリックします。

### グループ化解除

1. グループ化解除したいキャンパス上の図形を選択します。
2. 画面上部のツールバーからグループ化解除をクリックします。

![grouping_ungrouping](https://user-images.githubusercontent.com/7916855/129528212-4693c643-75c7-464e-bca1-3c6c6e3f0b2d.gif)

## 整列

### 最前面へ移動

![move_to_frontmost](https://user-images.githubusercontent.com/7916855/129554426-936f737c-b060-413a-8dd5-fd1b3a16906a.gif)

### 前面へ移動

![move_to_front](https://user-images.githubusercontent.com/7916855/129554448-7fe2e743-ef8f-42fd-8e19-8e4f047f0890.gif)

### 背面へ移動

![move_to_back](https://user-images.githubusercontent.com/7916855/129554467-f16d3e5f-6634-434e-8604-5f62fd16ca66.gif)

### 最背面へ移動

![move_to_backmost](https://user-images.githubusercontent.com/7916855/129554512-4ef9eca7-ffb6-4fc4-815e-a8b48fe0069d.gif)

### 上揃え

![top_alignment](https://user-images.githubusercontent.com/7916855/129554549-a7281529-16c6-4e7c-9a75-12742941dca0.gif)

### 上下中央揃え

![top_and_bottom_center_alignment](https://user-images.githubusercontent.com/7916855/129554582-80804673-6b1c-441c-9c10-71bfc07c32d8.gif)

### 下揃え

![bottom_alignment](https://user-images.githubusercontent.com/7916855/129554606-5f3e100f-6ee4-4ef3-8814-7953ab2d9a5a.gif)

### 左揃え

![left_alignment](https://user-images.githubusercontent.com/7916855/129554636-b1034326-f1a7-41b7-b73a-bb3286a85132.gif)

### 左右中央揃え

![left_and_right_center_alignment](https://user-images.githubusercontent.com/7916855/129554677-1fbdd05f-99b7-4b96-b99a-5b6dcc4a1297.gif)

### 右揃え

![right_alignment](https://user-images.githubusercontent.com/7916855/129554707-a15086a6-2379-43af-ad59-b6479e0a0278.gif)

### 左右に整列

![distribute_horizontal](https://user-images.githubusercontent.com/7916855/129554739-23788ac8-c66e-4be0-9924-ae2f25070f49.gif)

### 上下に整列

![distribute_vertical](https://user-images.githubusercontent.com/7916855/129554752-69d0a1c2-32d9-4c64-bf6c-c7bf7eb66d6d.gif)

### 幅を合わせる

![uniform_width](https://user-images.githubusercontent.com/7916855/129554779-3bdb13c2-6063-4a18-a263-760ae297f8ae.gif)

### 高さを合わせる

![uniform_height](https://user-images.githubusercontent.com/7916855/129554802-bd62ab9c-b380-4142-b99f-a9eeff045f92.gif)

## ファイル関連

### 開く

メニュー > ファイル > 開く

または

![open_file](https://user-images.githubusercontent.com/7916855/129572298-dd12d14d-e731-4925-923a-b92215c22fd5.png)

### 名前を付けて保存

メニュー > ファイル > 名前を付けて保存

### 上書き保存

メニュー > ファイル > 上書き保存

または

![overwrite_file](https://user-images.githubusercontent.com/7916855/129572332-ed48adba-d4d5-4832-8421-9ff90f914dbe.png)

### エクスポート

メニュー > ファイル > エクスポート

または

![export](https://user-images.githubusercontent.com/7916855/129572363-cd29ea70-5d16-4273-88e4-9fc40e209026.png)

## パーツ機能

複数の図形をひとまとめにして「パーツ」として登録し、配置のたびに公開パラメータで形を変えられる再利用可能な単位を作る機能です。FUI (Fictional User Interface) 制作向けに、同じ図形パターンを繰り返し使うワークフローを支援します。

> 詳しい使い方とチュートリアルは [docs/fui/phase1-tutorial.md](docs/fui/phase1-tutorial.md) を参照してください。

### パーツ化 (Promote)

1. 画面左側のツールから選択ツールを選択します。
2. パーツにしたい図形をクリックして選択します。複数選択にはShiftキーを押しながらクリックします。
3. 選択した図形を右クリックし、「パーツ化…」を選択します。
4. パーツ名を入力するダイアログが開くので、名前を入力して OK を押します。
5. 元の場所にパーツインスタンスが配置され、内部の図形はパーツ定義として登録されます。

### パーツの編集 (PartEditor)

1. キャンパス上のパーツインスタンスをダブルクリックします。
2. 専用のパーツ編集ウィンドウが開き、パーツ定義の中身を編集できます。
3. 「矩形を追加」ボタンで図形を追加、「選択を削除」ボタン (または Delete キー) で削除できます。
4. 右側のプロパティパネルで選択中の図形の位置・サイズ・枠線・色を編集できます。
5. ウィンドウを閉じると変更は同じパーツ定義の全インスタンスに反映されます。

### 公開パラメータの管理

パーツ編集ウィンドウの右側「公開パラメータ」セクションから、パーツ定義に対して公開パラメータを追加・削除できます。

1. パーツ編集ウィンドウを開き、右側パネル下部の「公開パラメータを追加…」を押します。
2. ダイアログで名前、型 (Double / Int / Boolean / Point / Color / Brush / String / Enum)、デフォルト値を入力します。
3. OK で追加されると、そのパーツ定義を参照する全インスタンスに公開状態を示す「公」バッジが表示されます。
4. 不要になった公開パラメータは一覧項目右端の「×」ボタンで削除できます。

### パーツの複製 (Clone & Customize)

1. キャンパス上のパーツインスタンスを右クリックし、「複製して新パーツとして登録」を選択します。
2. 元のパーツ定義のディープコピーが新パーツとして登録され、専用編集ウィンドウが開きます。
3. 新パーツは元パーツから独立しているため、自由に編集しても元のパーツには影響しません。

### パーツの切り離し (Detach)

1. キャンパス上のパーツインスタンスを右クリックし、「パーツから切り離す」を選択します。
2. インスタンスが生の図形群に展開され、以後パーツ定義との関連が失われます。
3. パーツ定義そのものや他のインスタンスは影響を受けません。Undo で切り離し前の状態に戻せます。

### パーツの単独ファイル保存 / 読み込み (.bgpart)

- **エクスポート**: パーツインスタンスを右クリック → 「パーツをエクスポート…」で `.bgpart` 形式の単独ファイルとして保存。
- **インポート**: メニュー > ファイル > 「パーツをインポート… (.bgpart)」で `.bgpart` を読み込み、現プロジェクトのパーツ定義に追加。

### 未使用パーツ定義の削除

切り離し後などで、もうどのインスタンスからも参照されていないパーツ定義が残ることがあります。

1. メニュー > ファイル > 「未使用パーツ定義を削除…」を選択します。
2. 未使用件数を確認するダイアログが出るので OK を押すと一括削除されます。
3. Undo で削除前の状態に戻せます。

## アニメーション (Phase 5 / Phase 5.5)

詳しい操作は以下のチュートリアルを参照してください。

- [`docs/fui/phase5-tutorial.md`](docs/fui/phase5-tutorial.md) — Timeline / Track / Keyframe / 再生 / PNG 連番書出
- [`docs/fui/phase5-5-tutorial.md`](docs/fui/phase5-5-tutorial.md) — WPF Storyboard / MAUI Animation XAML 出力
- [`docs/fui/phase6-tutorial.md`](docs/fui/phase6-tutorial.md) — テキスト系 5 種 (MonoText / DataGen / NumSeq / TextMatrix / TextOnPath) の XAML 出力対応

要点:

1. 画面下部 `Timeline` Expander を開く。
2. 図形のプロパティダイアログで各行の ◇ ボタンをクリックして現時刻にキーフレームを打つ。
3. Now を別時刻に進めて値を変更 → もう一度 ◇ で 2 つ目のキーフレーム。これで補間アニメ完成。
4. トランスポートバーの ▶ で再生 / Loop チェックで折返し。
5. 「PNG 連番...」「WPF XAML...」「MAUI XAML...」ボタンから書き出し可能。
   - PNG 連番: 各フレームを 0001.png〜 として保存。
   - WPF XAML: `UserControl` + `Storyboard` の `.xaml` (+ オプションで `.xaml.cs`) を生成、実 WPF アプリへコピペで組み込み可能。
   - MAUI XAML: `ContentView` の `.xaml` + Animation コードビハインドの `.xaml.cs` を生成、実 .NET MAUI アプリへコピペで組み込み可能。

