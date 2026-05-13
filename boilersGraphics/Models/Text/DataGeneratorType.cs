namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2-a §3.2 / Q-2: DataGeneratorTextBlock が生成するダミーデータの種類。
/// 8 種類 (Hex / Binary / Ipv4 / Ipv6 / Uuid / Timestamp / RandomCode / LogLine) に確定。
/// </summary>
public enum DataGeneratorType
{
    Hex,
    Binary,
    Ipv4Address,
    Ipv6Address,
    Uuid,
    Timestamp,
    RandomCode,
    LogLine,
}
