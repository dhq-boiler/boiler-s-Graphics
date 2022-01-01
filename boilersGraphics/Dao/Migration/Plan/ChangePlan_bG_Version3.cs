using boilersGraphics.Dao.Migration.Version;
using Homura.ORM.Migration;
using System.Collections.Generic;

namespace boilersGraphics.Dao.Migration.Plan
{
    class ChangePlan_bG_Version3 : ChangePlanByVersion<Version3>
    {
        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new ChangePlan_bG_Statistics_Version2();
            }
        }
    }
}
