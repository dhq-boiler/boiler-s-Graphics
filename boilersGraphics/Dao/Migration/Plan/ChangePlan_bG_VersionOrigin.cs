using System.Collections.Generic;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;

namespace boilersGraphics.Dao.Migration.Plan;

internal class ChangePlan_bG_VersionOrigin : ChangePlanByVersion<VersionOrigin>
{
    public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
    {
        get { yield return new ChangePlan_bG_Statistics_VersionOrigin(); }
    }
}