using Homura.ORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Models
{
    public class TerminalInfo : PkIdEntity
    {
        private Guid _TerminalId;
        private string _BuildComposition;

        /// <summary>
        /// 端末ID
        /// </summary>
        [Column("TerminalId", "NUMERIC", 1), NotNull]
        public Guid TerminalId
        {
            get { return _TerminalId; }
            set { SetProperty(ref _TerminalId, value); }
        }

        /// <summary>
        /// ビルド構成(Debug/Production)
        /// </summary>
        [Column("BuildComposition", "TEXT", 1), NotNull]
        public string BuildComposition
        {
            get { return _BuildComposition; }
            set { SetProperty(ref _BuildComposition, value); }
        }


    }
}
