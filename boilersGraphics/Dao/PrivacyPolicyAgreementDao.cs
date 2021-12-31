using boilersGraphics.Extensions;
using boilersGraphics.Models;
using Homura.ORM;
using Homura.QueryBuilder.Iso.Dml;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Dao
{
    internal class PrivacyPolicyAgreementDao : Dao<PrivacyPolicyAgreement>
    {
        public PrivacyPolicyAgreementDao()
            : base()
        { }

        public PrivacyPolicyAgreementDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        public IEnumerable<PrivacyPolicyAgreement> GetAgree(IDbConnection conn = null)
        {
            bool isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = GetConnection();
                }

                using (var command = conn.CreateCommand())
                {
                    using (var query = new Select().Asterisk().From.Table(new Table<PrivacyPolicyAgreement>())
                                                              .Where.Column(nameof(PrivacyPolicyAgreement.IsAgree)).EqualTo.Value(true)
                                                              .OrderBy.Column(nameof(PrivacyPolicyAgreement.DateOfEnactment)).Desc)

                    {
                        string sql = query.ToSql();
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        query.SetParameters(command);

                        LogManager.GetCurrentClassLogger().Debug(sql);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                yield return ToEntity(reader);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Dispose();
                }
            }
        }

        public IEnumerable<PrivacyPolicyAgreement> GetAgreeOrDisagree(IDbConnection conn = null)
        {
            bool isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = GetConnection();
                }

                using (var command = conn.CreateCommand())
                {
                    using (var query = new Select().Asterisk().From.Table(new Table<PrivacyPolicyAgreement>())
                                                              .OrderBy.Column(nameof(PrivacyPolicyAgreement.DateOfEnactment)).Desc)

                    {
                        string sql = query.ToSql();
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        query.SetParameters(command);

                        LogManager.GetCurrentClassLogger().Debug(sql);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                yield return ToEntity(reader);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Dispose();
                }
            }
        }

        protected override PrivacyPolicyAgreement ToEntity(IDataRecord reader)
        {
            return new PrivacyPolicyAgreement()
            {
                DateOfEnactment = reader.SafeGetDateTime("DateOfEnactment", Table),
                IsAgree = reader.SafeGetBoolean("IsAgree", Table),
                DateOfAgreement = reader.SafeGetDateTime("DateOfAgreement", Table),
            };
        }
    }
}
