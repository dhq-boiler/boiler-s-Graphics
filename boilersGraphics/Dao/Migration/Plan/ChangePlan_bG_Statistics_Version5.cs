using boilersGraphics.Dao.Migration.Version;
using boilersGraphics.Models;
using Homura.ORM;
using Homura.ORM.Migration;

namespace boilersGraphics.Dao.Migration.Plan;

internal class ChangePlan_bG_Statistics_Version5 : ChangePlanByTable<Statistics, Version5>
{
    public override void CreateTable(IConnection connection)
    {
        var dao = new StatisticsDao(typeof(Version5));
        dao.CurrentConnection = connection;
        dao.CreateTableIfNotExists();
        ++ModifiedCount;
        dao.CreateIndexIfNotExists();
        ++ModifiedCount;
    }

    public override void DropTable(IConnection connection)
    {
        var dao = new StatisticsDao(typeof(Version5));
        dao.CurrentConnection = connection;
        dao.DropTable();
        ++ModifiedCount;
    }

    public override void UpgradeToTargetVersion(IConnection connection)
    {
        var dao = new StatisticsDao(typeof(Version5));
        dao.CurrentConnection = connection;
        dao.CreateTableIfNotExists();
        ++ModifiedCount;
        dao.CreateIndexIfNotExists();
        ++ModifiedCount;
        dao.UpgradeTable(new VersionChangeUnit(typeof(Version4), TargetVersion.GetType()));
        ++ModifiedCount;
    }
}