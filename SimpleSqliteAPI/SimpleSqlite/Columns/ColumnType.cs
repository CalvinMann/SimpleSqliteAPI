using System;
using System.Data;

namespace SimpleSqlite.Columns
{
    public enum ColumnType
    {
        Integer,
        Text,
        BLOB,
        Real,
        Numeric
    }

    internal static class ColumnTypeExtensions
    {
        public static string ToDbString(this ColumnType type)
        {
            return type.ToString().ToUpper();
        }

        public static DbType ToDbType(this ColumnType type)
        {
            switch (type)
            {
                case ColumnType.Integer:
                    return DbType.Int64;
                case ColumnType.Text:
                    return DbType.String;
                case ColumnType.BLOB:
                    return DbType.Binary;
                case ColumnType.Real:
                    return DbType.Double;
                case ColumnType.Numeric:
                    return DbType.Decimal;
                default:
                    throw new InvalidOperationException("Unsupported ColumnType value");
            }
        }

        public static ColumnType Parse(string columnName)
        {
            // https://www.sqlite.org/datatype3.html 2.1 Determination Of Column Affinity
            var lowerName = columnName.ToLower();
            if (lowerName.Contains("int")) return ColumnType.Integer;
            if (lowerName.Contains("char") || lowerName.Contains("clob") || lowerName.Contains("text")) return ColumnType.Text;
            if (lowerName.Contains("blob") || String.IsNullOrEmpty(lowerName)) return ColumnType.BLOB;
            if (lowerName.Contains("real") || lowerName.Contains("floa") || lowerName.Contains("doub")) return ColumnType.Real;
            return ColumnType.Numeric;
        }
    }
}
