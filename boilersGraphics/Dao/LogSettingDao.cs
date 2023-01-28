using System;
using System.Data;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using Homura.ORM;

namespace boilersGraphics.Dao;

internal class LogSettingDao : Dao<LogSetting>
{
    public LogSettingDao()
    {
    }

    public LogSettingDao(Type entityVersionType)
        : base(entityVersionType)
    {
    }

    protected override LogSetting ToEntity(IDataRecord reader)
    {
        return new LogSetting
        {
            ID = reader.SafeGetGuid("ID", Table),
            LogLevel = reader.SafeGetString("LogLevel", Table)
        };
    }
}