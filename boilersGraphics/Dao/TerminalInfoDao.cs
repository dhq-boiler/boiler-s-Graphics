﻿using boilersGraphics.Extensions;
using boilersGraphics.Models;
using Homura.ORM;
using System;
using System.Data;

namespace boilersGraphics.Dao;

internal class TerminalInfoDao : Dao<TerminalInfo>
{
    public TerminalInfoDao()
    {
    }

    public TerminalInfoDao(Type entityVersionType)
        : base(entityVersionType)
    {
    }

    protected override TerminalInfo ToEntity(IDataRecord reader)
    {
        return new TerminalInfo
        {
            ID = reader.SafeGetGuid("ID", Table),
            TerminalId = reader.SafeGetGuid("TerminalId", Table)
        };
    }
}