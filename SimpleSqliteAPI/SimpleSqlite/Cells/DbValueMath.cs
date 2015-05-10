using System;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;

namespace SimpleSqlite.Cells
{
    internal static class DbValueMath
    {
        public static bool Compare(DbValue value1, DbValue value2, Comparison comparison)
        {
            var compResult = Compare((value1 != null) ? value1.Value : null, (value2 != null) ? value2.Value : null,
                comparison);
            switch (comparison)
            {
                case Comparison.Equal:
                    return compResult == 0;
                case Comparison.NotEqual:
                    return compResult != 0;
                case Comparison.GreaterThan:
                    return compResult > 0;
                case Comparison.GreaterThanOrEqual:
                    return compResult >= 0;
                case Comparison.LessThan:
                    return compResult < 0;
                case Comparison.LessThanOrEqual:
                    return compResult <= 0;
                default:
                    throw new ArgumentException("Unknown ComparisonType member.", "comparison");
            }
        }

        private static int? Compare(object value1, object value2, Comparison comparison)
        {
            if (value1 is long && value2 is long) return Compare((long)value1, (long)value2);
            if (value1 is long && value2 is decimal) return Compare((long)value1, (decimal)value2);
            if (value1 is long && value2 is double) return Compare((long)value1, (double)value2);
            if (value1 is decimal && value2 is long) return -Compare((long)value2, (decimal)value1);
            if (value1 is decimal && value2 is decimal) return Compare((decimal)value1, (decimal)value2);
            if (value1 is double && value2 is long) return -Compare((long)value2, (double)value1);
            if (value1 is double && value2 is double) return Compare((double)value1, (double)value2);
            if (comparison == Comparison.Equal || comparison == Comparison.NotEqual)
            {
                if (value1 == null || value2 == null) return CompareNulls(value1, value2);
                if (value1 is string && value2 is string) return Compare((string)value1, (string)value2);
                if (value1 is Array && value2 is Array) return Compare((Array)value1, (Array)value2);
            }
            return null;
        }

        private static int Compare(long value1, long value2)
        {
            return value1 == value2 ? 0 : value1 > value2 ? 1 : -1;
        }

        private static int Compare(long value1, decimal value2)
        {
            return value1 == value2 ? 0 : value1 > value2 ? 1 : -1;
        }
        
        private static int Compare(long value1, double value2)
        {
            return value1 == value2 ? 0 : value1 > value2 ? 1 : -1;
        }

        private static int Compare(decimal value1, decimal value2)
        {
            return value1 == value2 ? 0 : value1 > value2 ? 1 : -1;
        }

        private static int Compare(double value1, double value2)
        {
            return value1 == value2 ? 0 : value1 > value2 ? 1 : -1;
        }

        private static int Compare(string value1, string value2)
        {
            return value1 == value2 ? 0 : 1;
        }

        private static int Compare(Array value1, Array value2)
        {
            if (value1.Length != value2.Length) return 1;
            for (var i = 0; i < value1.Length; i++)
            {
                if (!value1.GetValue(i).Equals(value2.GetValue(i))) return 1;
            }
            return 0;
        }

        private static int CompareNulls(object value1, object value2)
        {
            return value1 == null && value2 == null ? 0 : 1;
        }

        public static DbValue Sum(DbValue value1, DbValue value2)
        {
            if (value1 == null) return value2;
            if (value2 == null) return value1;

            object v1 = value1.Value, v2 = value2.Value;
            if (v1 is long && v2 is long) return (long)value1 + (long)value2;
            if (v1 is long && v2 is decimal) return (long)value1 + (decimal)value2;
            if (v1 is long && v2 is double) return (long)value1 + (double)value2;
            if (v1 is decimal && v2 is long) return (long)value2 + (decimal)value1;
            if (v1 is decimal && v2 is decimal) return (decimal)value1 + (decimal)value2;
            if (v1 is decimal && v2 is double) return (double)(decimal)value1 + (double)value2;
            if (v1 is double && v2 is long) return (long)value2 + (double)value1;
            if (v1 is double && v2 is decimal) return (double)value1 + (double)(decimal)value2;
            if (v1 is double && v2 is double) return (double)value1 + (double)value2;
            if (v1 is string && v2 is string) return (string)value1 + (string)value2;

            throw new InvalidTypeException(value2.Type, Resources.CannotSumTypes.FormatExt(value1.Type, value2.Type));
        }
    }
}
