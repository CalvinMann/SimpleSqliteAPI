using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SimpleSqlite.Columns;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;

namespace SimpleSqlite.Cells
{
    /// <summary>
    /// Represents a value in SQLite database.
    /// </summary>
    /// <remarks>
    /// Supports .NET types: long, string, byte[], double, decimal, null and arrays of these types.
    /// The class is immutable, even if the value itself is of mutable type (arrays), a copy of the source array is created.
    /// </remarks>
    [DebuggerDisplay("{Value}")]
    public class DbValue
    {
        internal object Value { get; private set; }

        public Type Type { get { return Value != null ? Value.GetType() : null; } }

        private DbValue(object value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is DbValue && Equals(Value, ((DbValue)obj).Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(DbValue value1, DbValue value2)
        {
            return Equals(value1, value2);
        }

        public static bool operator !=(DbValue value1, DbValue value2)
        {
            return !Equals(value1, value2);
        }

        public static implicit operator DbValue(long value)
        {
            return new DbValue(value);
        }

        public static explicit operator long(DbValue value)
        {
            return (long)value.Value;
        }

        public static implicit operator DbValue(string value)
        {
            return new DbValue(value);
        }

        public static explicit operator string(DbValue value)
        {
            return (string)value.Value;
        }

        public static implicit operator DbValue(byte[] value)
        {
            return new DbValue(value.Copy());
        }

        public static explicit operator byte[](DbValue value)
        {
            return ((byte[])value.Value).Copy();
        }

        public static implicit operator DbValue(double value)
        {
            return new DbValue(value);
        }

        public static explicit operator double(DbValue value)
        {
            return (double)value.Value;
        }

        public static implicit operator DbValue(decimal value)
        {
            return new DbValue(value);
        }

        public static explicit operator decimal(DbValue value)
        {
            return (decimal)value.Value;
        }

        public static implicit operator DbValue(long[] value)
        {
            return new DbValue(value);
        }

        public static explicit operator long[](DbValue value)
        {
            return ((long[])value.Value).Copy();
        }

        public static implicit operator DbValue(string[] value)
        {
            return new DbValue(value.Copy());
        }

        public static explicit operator string[](DbValue value)
        {
            return ((string[])value.Value).Copy();
        }

        public static implicit operator DbValue(byte[][] value)
        {
            return new DbValue(value.Copy());
        }

        public static explicit operator byte[][](DbValue value)
        {
            return ((byte[][])value.Value).Copy();
        }

        public static implicit operator DbValue(double[] value)
        {
            return new DbValue(value.Copy());
        }

        public static explicit operator double[](DbValue value)
        {
            return ((double[])value.Value).Copy();
        }

        public static implicit operator DbValue(decimal[] value)
        {
            return new DbValue(value.Copy());
        }

        public static explicit operator decimal[](DbValue value)
        {
            return ((decimal[])value.Value).Copy();
        }

        public static DbValue operator +(DbValue value1, DbValue value2)
        {
            return DbValueMath.Sum(value1, value2);
        }

        internal static DbValue FromDb(object value, ColumnType type, ColumnQuantity quantity)
        {
            DbValue newValue;
            if (value is DBNull) newValue = null;
            else if (quantity == ColumnQuantity.List) newValue = UnpackArray((byte[])value, type);
            else if (value is long) newValue = (long)value;
            else if (value is string) newValue = (string)value;
            else if (value is byte[]) newValue = (byte[])value;
            else if (value is double) newValue = (double)value;
            else if (value is decimal) newValue = (decimal)value;
            else throw new ArgumentException("Unknown database type {0}".FormatExt(value.GetType()), "value");
            return newValue;
        }

        private static DbValue UnpackArray(byte[] packed, ColumnType type)
        {
            if (type == ColumnType.Integer)
                return UnpackUniformSizeArray(packed, sizeof(long)).Select(item => BitConverter.ToInt64(item, 0)).ToArray();
            if (type == ColumnType.Text)
                return UnpackVariableSizeArray(packed).Select(Encoding.Unicode.GetString).ToArray();
            if (type == ColumnType.BLOB)
                return UnpackVariableSizeArray(packed).ToArray();
            if (type == ColumnType.Real)
                return UnpackUniformSizeArray(packed, sizeof(double)).Select(item => BitConverter.ToDouble(item, 0)).ToArray();
            if (type == ColumnType.Numeric)
                return UnpackUniformSizeArray(packed, sizeof(decimal)).Select(BitConverterExt.ToDecimal).ToArray();

            throw new ArgumentException("Unsupported type {0}".FormatExt(type), "type");
        }

        internal byte[] PackArray()
        {
            if (Value is long[])
                return PackUniformSizeArray(((long[])Value).Select(BitConverter.GetBytes).ToArray(), sizeof(long));
            if (Value is string[])
                return PackVariableSizeArray(((string[])Value).Select(Encoding.Unicode.GetBytes).ToArray());
            if (Value is byte[][])
                return PackVariableSizeArray((byte[][])Value);
            if (Value is double[])
                return PackUniformSizeArray(((double[])Value).Select(BitConverter.GetBytes).ToArray(), sizeof(double));
            if (Value is decimal[])
                return PackUniformSizeArray(((decimal[])Value).Select(BitConverterExt.GetBytes).ToArray(), sizeof(decimal));

            throw new InvalidOperationException(Resources.IsNotSupportedArrayType.FormatExt(Value.GetType()));
        }

        private static byte[] PackVariableSizeArray(byte[][] array)
        {
            var result = new byte[array.Sum(arr => sizeof(int) + arr.Length)]; // additional int to write array length
            var curLength = 0;
            foreach (var item in array)
            {
                BitConverter.GetBytes(item.Length).CopyTo(result, curLength);
                curLength += sizeof(int);
                item.CopyTo(result, curLength);
                curLength += item.Length;
            }
            return result;
        }

        private static byte[] PackUniformSizeArray(byte[][] array, int size)
        {
            var result = new byte[array.Length * size];
            for (var i = 0; i < array.Length; i++)
            {
                array[i].CopyTo(result, i * size);
            }
            return result;
        }

        private static IEnumerable<byte[]> UnpackVariableSizeArray(byte[] packed)
        {
            var curPos = 0;
            var result = new List<byte[]>();
            while (curPos < packed.Length)
            {
                var curItemLength = BitConverter.ToInt32(packed, curPos);
                curPos += sizeof(int);
                result.Add(packed.SubArray(curPos, curItemLength));
                curPos += curItemLength;
            }
            return result;
        }

        private static IEnumerable<byte[]> UnpackUniformSizeArray(byte[] packed, int size)
        {
            var result = new byte[packed.Length / size][];
            for (var i = 0; i < packed.Length; i += size)
            {
                result[i / size] = packed.SubArray(i, size);
            }
            return result;
        }

        internal bool IsListType
        {
            get
            {
                return Value is long[] || Value is string[] || Value is byte[][] || Value is double[] ||
                       Value is decimal[];
            }
        }
    }

    internal static class DbValueExt
    {
        public static object ToDb(this DbValue value)
        {
            return value != null
                ? value.IsListType
                    ? value.PackArray()
                    : value.Value
                : null;
        }
    }
}
