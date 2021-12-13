using Homura.ORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Models
{
    public class LogSetting : PkIdEntity
    {
        private string _LogLevel;

        [Column("LogLevel", "TEXT", 1)]
        public string LogLevel
        {
            get { return _LogLevel; }
            set { SetProperty(ref _LogLevel, value); }
        }
    }
}
