namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2.5-a / Q-6 案 D: TextMatrix のセル内容生成モード。
/// 3 モードすべてサポート (連番 / DataGenerator 埋め込み / 任意文字列リスト)。
/// </summary>
public enum TextMatrixCellMode
{
    /// <summary>連番。SequenceStart + 行優先の通し番号を Format で書式化。</summary>
    Sequential,

    /// <summary>DataGenerator 埋め込み。DataGenType と Seed の組み合わせで各セルを生成。</summary>
    DataGenerator,

    /// <summary>任意文字列リスト。CustomItems (改行区切り) を順番に並べる。</summary>
    CustomList,
}
