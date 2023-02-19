﻿using System.Collections.Generic;
using boilersGraphics.Dao.Migration.Version;
using Homura.ORM.Migration;

namespace boilersGraphics.Dao.Migration.Plan;

internal class ChangePlan_bG_Version6 : ChangePlanByVersion<Version6>
{
    public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
    {
        get { yield return new ChangePlan_bG_Statistics_Version4(); }
    }
}