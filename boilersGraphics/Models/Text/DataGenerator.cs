using System;

namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2-c: Seed を受けて再現可能なダミーデータを生成する純関数群 (Q-3 / Q-5)。
/// 同期実行 (Q-5 案 A: 常に同期) で、Random(seed) ベースなので外部状態に依存しない。
/// </summary>
public static class DataGenerator
{
    public static string Generate(
        DataGeneratorType type,
        int seed,
        int count,
        string separator,
        DataGeneratorLayout layout)
    {
        if (count <= 0) return string.Empty;

        var rng = new Random(seed);
        var items = new string[count];
        for (var i = 0; i < count; i++)
        {
            items[i] = type switch
            {
                DataGeneratorType.Hex => GenerateHex(rng),
                DataGeneratorType.Binary => GenerateBinary(rng),
                DataGeneratorType.Ipv4Address => GenerateIpv4(rng),
                DataGeneratorType.Ipv6Address => GenerateIpv6(rng),
                DataGeneratorType.Uuid => GenerateUuid(rng),
                DataGeneratorType.Timestamp => GenerateTimestamp(rng),
                DataGeneratorType.RandomCode => GenerateRandomCode(rng),
                DataGeneratorType.LogLine => GenerateLogLine(rng),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }

        var join = layout == DataGeneratorLayout.MultiLine ? Environment.NewLine : (separator ?? string.Empty);
        return string.Join(join, items);
    }

    private static string GenerateHex(Random rng) => rng.Next(256).ToString("X2");

    private static string GenerateBinary(Random rng)
    {
        Span<char> chars = stackalloc char[4];
        var v = rng.Next(16);
        for (var i = 3; i >= 0; i--)
        {
            chars[i] = (v & 1) == 1 ? '1' : '0';
            v >>= 1;
        }
        return new string(chars);
    }

    private static string GenerateIpv4(Random rng)
        => $"{rng.Next(256)}.{rng.Next(256)}.{rng.Next(256)}.{rng.Next(256)}";

    private static string GenerateIpv6(Random rng)
    {
        var parts = new string[8];
        for (var i = 0; i < 8; i++)
            parts[i] = rng.Next(0x10000).ToString("x4");
        return string.Join(":", parts);
    }

    private static string GenerateUuid(Random rng)
    {
        Span<byte> bytes = stackalloc byte[16];
        rng.NextBytes(bytes);
        // RFC 4122 v4: 文字列表現の "xxxxxxxx-xxxx-Mxxx-Nxxx-xxxxxxxxxxxx" の M を 4、N の上位 2bit を 10。
        // .NET の Guid は Data3 (bytes[6..7]) をリトルエンディアンで格納するため、
        // 文字列上の M 位置に対応するのは bytes[7] (上位バイト)。bytes[8] は Data4 で BE なのでそのまま。
        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x40);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new Guid(bytes).ToString();
    }

    private static readonly DateTime TimestampOrigin = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime TimestampEnd = new(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    private static string GenerateTimestamp(Random rng)
    {
        var rangeSeconds = (long)(TimestampEnd - TimestampOrigin).TotalSeconds;
        var sec = (long)(rng.NextDouble() * rangeSeconds);
        return TimestampOrigin.AddSeconds(sec).ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    // O / 0 / I / 1 は除外 (FUI で可読性を優先)
    private const string RandomCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int RandomCodeLength = 6;

    private static string GenerateRandomCode(Random rng)
    {
        Span<char> chars = stackalloc char[RandomCodeLength];
        for (var i = 0; i < RandomCodeLength; i++)
            chars[i] = RandomCodeChars[rng.Next(RandomCodeChars.Length)];
        return new string(chars);
    }

    private static readonly string[] LogLevels = { "INFO", "WARN", "ERROR", "DEBUG", "TRACE" };
    private static readonly string[] LogModules = { "auth", "cache", "db", "io", "net", "render", "worker" };
    private static readonly string[] LogMessages =
    {
        "request handled",
        "cache miss",
        "connection retry",
        "task complete",
        "buffer flushed",
        "config loaded",
        "task queued",
        "timeout exceeded",
    };

    private static readonly DateTime LogOrigin = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static string GenerateLogLine(Random rng)
    {
        var level = LogLevels[rng.Next(LogLevels.Length)];
        var module = LogModules[rng.Next(LogModules.Length)];
        var message = LogMessages[rng.Next(LogMessages.Length)];
        var sec = rng.Next(365 * 24 * 60 * 60);
        var ts = LogOrigin.AddSeconds(sec);
        return $"[{level}] {ts:yyyy-MM-dd HH:mm:ss} {module}: {message}";
    }
}
