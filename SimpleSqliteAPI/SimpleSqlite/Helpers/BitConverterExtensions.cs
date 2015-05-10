using System;

namespace SimpleSqlite.Helpers
{
    public class BitConverterExt
    {
        public static byte[] GetBytes(decimal dec)
        {
            var bits = decimal.GetBits(dec);
            var bytes = new byte[bits.Length * sizeof(int)];
            for (var i = 0; i < bits.Length; i++)
            {
                BitConverter.GetBytes(bits[i]).CopyTo(bytes, i * sizeof(int));
            }
            return bytes;
        }

        public static decimal ToDecimal(byte[] bytes)
        {
            if (bytes.Length != 16)
                throw new ArgumentException("A decimal must be created from exactly 16 bytes");
            var bits = new int[4];
            for (var i = 0; i < 16; i += 4)
            {
                bits[i / 4] = BitConverter.ToInt32(bytes, i);
            }
            return new decimal(bits);
        }
    }
}
