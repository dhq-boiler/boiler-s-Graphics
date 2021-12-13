using boilersGraphics.Extensions;
using boilersGraphics.Models;
using Homura.ORM;
using System;
using System.Data;

namespace boilersGraphics.Dao
{
    class LogSettingDao : Dao<LogSetting>
    {
        public LogSettingDao()
            : base()
        { }

        public LogSettingDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        protected override LogSetting ToEntity(IDataRecord reader)
        {
            return new LogSetting()
            {
                ID = reader.SafeGetGuid("ID", Table),
                LogLevel = reader.SafeGetString("LogLevel", Table),
            };
        }
    }
}
