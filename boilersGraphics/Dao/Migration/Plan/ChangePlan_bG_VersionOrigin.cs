using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Dao.Migration.Plan
{
    class ChangePlan_bG_VersionOrigin : ChangePlanByVersion<VersionOrigin>
    {
        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new ChangePlan_bG_Statistics_VersionOrigin();
            }
        }
    }
}
