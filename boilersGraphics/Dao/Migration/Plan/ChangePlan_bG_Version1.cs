using boilersGraphics.Dao.Migration.Version;
using Homura.ORM.Migration;
using System.Collections.Generic;

namespace boilersGraphics.Dao.Migration.Plan;

internal class ChangePlan_bG_Version1 : ChangePlanByVersion<Version1>
{
    public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
    {
        get
        {
            yield return new ChangePlan_bG_Statistics_Version1();
            yield return new ChangePlan_bG_LogSetting_VersionOrigin();
        }
    }
}