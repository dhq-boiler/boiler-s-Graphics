using boilersGraphics.Dao.Migration.Version;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Dao.Migration.Plan
{
    class ChangePlan_bG_Version1 : ChangePlanByVersion<Version1>
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
}
