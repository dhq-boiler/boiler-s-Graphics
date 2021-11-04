﻿using boilersGraphics.Extensions;
using boilersGraphics.Models;
using Homura.ORM;
using System;
using System.Data;

namespace boilersGraphics.Dao
{
    class StatisticsDao : Dao<Statistics>
    {
        public StatisticsDao()
            : base()
        { }

        public StatisticsDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        protected override Statistics ToEntity(IDataRecord reader)
        {
            return new Statistics()
            {
                ID = reader.SafeGetGuid("ID", Table),
                NumberOfBoots = reader.SafeGetInt("NumberOfBoots", Table)
            };
        }
    }
}
