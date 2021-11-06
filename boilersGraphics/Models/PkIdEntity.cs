using Homura.ORM;
using Homura.ORM.Mapping;
using System;
using System.Diagnostics;

namespace boilersGraphics.Models
{
    public abstract class PkIdEntity : EntityBaseObject, IId
    {
        private Guid _ID;

        [Column("ID", "NUMERIC", 0), PrimaryKey, Index]
        [Since(typeof(VersionOrigin))]
        public Guid ID
        {
            [DebuggerStepThrough]
            get
            { return _ID; }
            set { SetProperty(ref _ID, value); }
        }
    }
}
