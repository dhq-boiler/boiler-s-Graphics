using boilersGraphics.Extensions;
using boilersGraphics.Models;
using Homura.ORM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Dao
{
    internal class TerminalInfoDao : Dao<TerminalInfo>
    {
        public TerminalInfoDao()
            : base()
        { }

        public TerminalInfoDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        protected override TerminalInfo ToEntity(IDataRecord reader)
        {
            return new TerminalInfo()
            {
                ID = reader.SafeGetGuid("ID", Table),
                TerminalId = reader.SafeGetGuid("TerminalId", Table),
            };
        }
    }
}
