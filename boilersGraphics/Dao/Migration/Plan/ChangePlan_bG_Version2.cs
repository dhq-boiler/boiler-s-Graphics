﻿using boilersGraphics.Dao.Migration.Version;
using Homura.ORM.Migration;
using System.Collections.Generic;

namespace boilersGraphics.Dao.Migration.Plan;

internal class ChangePlan_bG_Version2 : ChangePlanByVersion<Version2>
{
    public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
    {
        get { yield return new ChangePlan_bG_PrivacyPolicyAgreement_VersionOrigin(); }
    }
}