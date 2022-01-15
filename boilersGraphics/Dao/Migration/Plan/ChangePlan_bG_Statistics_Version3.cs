using boilersGraphics.Dao.Migration.Version;
using boilersGraphics.Models;
using Homura.ORM;
using Homura.ORM.Migration;

namespace boilersGraphics.Dao.Migration.Plan
{
    class ChangePlan_bG_Statistics_Version3 : ChangePlanByTable<Statistics, Version3>
    {
        public override void CreateTable(IConnection connection)
        {
            var dao = new StatisticsDao(typeof(Version3));
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
            dao.CreateIndexIfNotExists();
            ++ModifiedCount;
        }

        public override void DropTable(IConnection connection)
        {
            var dao = new StatisticsDao(typeof(Version3));
            dao.CurrentConnection = connection;
            dao.DropTable();
            ++ModifiedCount;
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new StatisticsDao(typeof(Version3));
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
            dao.CreateIndexIfNotExists();
            ++ModifiedCount;
            dao.UpgradeTable(new VersionChangeUnit(typeof(Version2), TargetVersion.GetType()));
            ++ModifiedCount;
        }
    }
}
