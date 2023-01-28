using boilersGraphics.Dao.Migration.Version;
using boilersGraphics.Models;
using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;

namespace boilersGraphics.Dao.Migration.Plan;

internal class ChangePlan_bG_Statistics_Version1 : ChangePlanByTable<Statistics, Version1>
{
    public override void CreateTable(IConnection connection)
    {
        var dao = new StatisticsDao(typeof(Version1));
        dao.CurrentConnection = connection;
        dao.CreateTableIfNotExists();
        ++ModifiedCount;
        dao.CreateIndexIfNotExists();
        ++ModifiedCount;
    }

    public override void DropTable(IConnection connection)
    {
        var dao = new StatisticsDao(typeof(Version1));
        dao.CurrentConnection = connection;
        dao.DropTable();
        ++ModifiedCount;
    }

    public override void UpgradeToTargetVersion(IConnection connection)
    {
        var dao = new StatisticsDao(typeof(Version1));
        dao.CurrentConnection = connection;
        dao.CreateTableIfNotExists();
        ++ModifiedCount;
        dao.CreateIndexIfNotExists();
        ++ModifiedCount;
        dao.UpgradeTable(new VersionChangeUnit(typeof(VersionOrigin), TargetVersion.GetType()));
        ++ModifiedCount;
    }
}