using Homura.ORM;
using System;
using System.Data;

namespace boilersGraphics.Extensions
{
    static class Extensions2
    {
        public static bool SafeGetBoolean(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = rdr.CheckColumnExists(columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? throw new NullReferenceException($"expected bool value but {columnName} value is null") : rdr.GetBoolean(index);
        }

        public static Guid SafeGetGuid(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = rdr.CheckColumnExists(columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? throw new NullReferenceException($"expected Guid value but {columnName} value is null") : rdr.GetGuid(index);
        }

        public static string SafeGetString(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetString(index);
        }

        public static int SafeGetInt(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? throw new NullReferenceException($"expected int value but {columnName} value is null") : rdr.GetInt32(index);
        }

        public static long SafeGetLong(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? throw new NullReferenceException($"expected long value but {columnName} value is null") : rdr.GetInt64(index);
        }

        public static int? SafeGetNullableInt(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (int?)rdr.GetInt32(index);
        }

        public static long? SafeNullableGetLong(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (long?)rdr.GetInt64(index);
        }

        public static DateTime SafeGetDateTime(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            return rdr.GetDateTime(index);
        }

        public static DateTime? SafeGetNullableDateTime(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (DateTime?)rdr.GetDateTime(index);
        }

        public static TimeSpan SafeGetTimeSpan(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            return (TimeSpan)rdr.GetValue(index);
        }

        public static TimeSpan? SafeGetNullableTimeSpan(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (TimeSpan?)rdr.GetValue(index);
        }

        public static int CheckColumnExists(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = rdr.GetOrdinal(columnName);
            if (index == -1)
            {
                var adding = "";
                if (!(table is null))
                {
                    adding = $" in {table.Name} {table.SpecifiedVersion} {table.DefaultVersion} {table.EntityClassType}";
                }
                throw new NotExistColumnException($"{columnName} ordinal is {index}" + adding);
            }

            return index;
        }

        public static bool IsDBNull(this IDataRecord rdr, string columnName)
        {
            return rdr.IsDBNull(rdr.GetOrdinal(columnName));
        }

        public static TimeSpan Truncate(this TimeSpan timeSpan, TimeSpan timeSpan1)
        {
            if (timeSpan == TimeSpan.Zero) return timeSpan;
            return timeSpan.Add(TimeSpan.FromTicks(-(timeSpan.Ticks % timeSpan1.Ticks)));
        }
    }
}
