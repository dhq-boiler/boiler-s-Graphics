using Homura.ORM.Mapping;

namespace boilersGraphics.Models;

public class LogSetting : PkIdEntity
{
    private string _LogLevel;

    [Column("LogLevel", "TEXT", 1)]
    public string LogLevel
    {
        get => _LogLevel;
        set => SetProperty(ref _LogLevel, value);
    }
}