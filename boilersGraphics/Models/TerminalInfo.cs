using Homura.ORM.Mapping;
using System;

namespace boilersGraphics.Models;

public class TerminalInfo : PkIdEntity
{
    private string _BuildComposition;
    private Guid _TerminalId;

    /// <summary>
    ///     端末ID
    /// </summary>
    [Column("TerminalId", "NUMERIC", 1)]
    [NotNull]
    public Guid TerminalId
    {
        get => _TerminalId;
        set => SetProperty(ref _TerminalId, value);
    }

    /// <summary>
    ///     ビルド構成(Debug/Production)
    /// </summary>
    [Column("BuildComposition", "TEXT", 1)]
    [NotNull]
    public string BuildComposition
    {
        get => _BuildComposition;
        set => SetProperty(ref _BuildComposition, value);
    }
}