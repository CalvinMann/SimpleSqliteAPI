using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleSqlite.Helpers
{
    public static class Extensions
    {
        public static string FormatExt(this string value, params object[] args)
        {
            return String.Format(value, args);
        }

        public static string JoinExt(this IEnumerable<string> value, string separator)
        {
            return String.Join(separator, value);
        }

        public static string ToHex(this IEnumerable<byte> bytes)
        {
            var hex = new StringBuilder(bytes.Count() * 2);
            foreach (var b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] HexToByte(this string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string GetNullableString(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetString(i);
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
            return collection;
        }

        public static IEnumerable<T> Copy<T>(this IEnumerable<T> collection)
        {
            return collection.ToArray(); // ToArray() always returns new instance
        }

        public static T[] Copy<T>(this T[] array)
        {
            var newArray = new T[array.Length];
            array.CopyTo(newArray, 0);
            return newArray;
        }

        public static T[][] Copy<T>(this T[][] array)
        {
            var newArray = new T[array.Length][];
            for (var i = 0; i < array.Length; i++)
            {
                newArray[i] = new T[array[i].Length];
                array[i].CopyTo(newArray[i], 0);
            }
            return newArray;
        }

        public static string EscapeIdentifier(this string identifier)
        {
            return String.Concat("\"", identifier, "\"");
        }

        public static bool CollectionEqual<T>(this IEnumerable<T> collection, IEnumerable<T> otherCollection)
        {
            return new MultiSetComparer<T>().Equals(collection, otherCollection);
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        /// <summary>
        /// Determines whether a specific character string matches a specified pattern. Implementation of SQL LIKE operator.
        /// </summary>
        /// <param name="text">A string to search in.</param>
        /// <param name="pattern">A string to search for, may contain wildcard characters.</param>
        /// <returns>True if the text matches the specified pattern. False otherwise.</returns>
        public static bool Like(this string text, string pattern)
        {
            // http://stackoverflow.com/a/5419544/860913 with some amendments suggested in comments.
            return
                new Regex(
                    @"\A" +
                    new Regex(@"\.|\$|\{|\(|\||\)|\*|\+|\?|\\", RegexOptions.IgnoreCase).Replace(pattern, ch => @"\" + ch)
                        .Replace('_', '.')
                        .Replace("%", ".*") + @"\z", RegexOptions.Singleline | RegexOptions.IgnoreCase).IsMatch(text);
        }

        public static TResult IfNotNull<T,TResult>(this T nullable, Func<T,TResult> func) where TResult: class
        {
            return nullable != null ? func(nullable) : null;
        }
    }
}
