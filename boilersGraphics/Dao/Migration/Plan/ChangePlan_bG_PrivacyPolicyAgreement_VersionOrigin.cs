using boilersGraphics.Models;
using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;

namespace boilersGraphics.Dao.Migration.Plan
{
    internal class ChangePlan_bG_PrivacyPolicyAgreement_VersionOrigin : ChangePlanByTable<PrivacyPolicyAgreement, VersionOrigin>
    {
        public override void CreateTable(IConnection connection)
        {
            var dao = new PrivacyPolicyAgreementDao(typeof(VersionOrigin));
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
            dao.CreateIndexIfNotExists();
            ++ModifiedCount;
        }

        public override void DropTable(IConnection connection)
        {
            var dao = new PrivacyPolicyAgreementDao(typeof(VersionOrigin));
            dao.CurrentConnection = connection;
            dao.DropTable();
            ++ModifiedCount;
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}