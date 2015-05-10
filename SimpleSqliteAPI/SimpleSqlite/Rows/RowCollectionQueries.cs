using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSqlite.Cells;
using SimpleSqlite.Exceptions;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;

namespace SimpleSqlite.Rows
{
    public static class RowCollectionQueries
    {
        public static IEnumerable<Row> Where(this RowCollection rowCollection, string column, Comparison comparison, DbValue value)
        {
            return rowCollection.Where(new Condition(column, comparison, value));
        }

        public static IEnumerable<Row> Where(this RowCollection rowCollection, params Condition[] conditions)
        {
            return rowCollection.Where((IEnumerable<Condition>)conditions);
        }

        public static IEnumerable<Row> Where(this RowCollection rowCollection, IEnumerable<Condition> conditions)
        {
            return rowCollection.Where(row => CheckRowForConditions(row, conditions)).ToList();
        }

        private static bool CheckRowForConditions(Row row, IEnumerable<Condition> conditions)
        {
            foreach (var condition in conditions)
            {
                var cell = row.Cells[condition.Column];
                if (cell == null) return false;
                if (!DbValueMath.Compare(cell.Value, condition.Value, condition.Comparison)) return false;
            }
            return true;
        }

        public static IEnumerable<Row> Like(this RowCollection rowCollection, string column, string pattern)
        {
            return rowCollection.Where(row =>
                row.Cells[column] != null && row.Cells[column].Value != null &&
                row.Cells[column].Value.Type == typeof (string) &&
                ((string)row.Cells[column].Value).Like(pattern));
        }

        public static IEnumerable<Row> Order(this RowCollection rowCollection, string column)
        {
            return rowCollection.Order(new Order(column, true));
        }

        public static IEnumerable<Row> OrderDescending(this RowCollection rowCollection, string column)
        {
            return rowCollection.Order(new Order(column, false));
        }

        public static IEnumerable<Row> Order(this RowCollection rowCollection, params Order[] orders)
        {
            return rowCollection.Order((IEnumerable<Order>)orders);
        }

        public static IEnumerable<Row> Order(this RowCollection rowCollection, IEnumerable<Order> orders)
        {
            if (!orders.Any()) return rowCollection;

            var ordered = orders.First().IsAscending
                ? rowCollection.OrderBy(row =>
                    row.Cells[orders.First().Column].IfNotNull(cell => cell.Value.IfNotNull(value => value.Value)))
                : rowCollection.OrderByDescending(row =>
                    row.Cells[orders.First().Column].IfNotNull(cell => cell.Value.IfNotNull(value => value.Value)));

            foreach (var order in orders.Skip(1))
            {
                ordered = order.IsAscending
                    ? ordered.ThenBy(
                        row => row.Cells[order.Column].IfNotNull(cell => cell.Value.IfNotNull(value => value.Value)))
                    : ordered.ThenByDescending(
                        row => row.Cells[order.Column].IfNotNull(cell => cell.Value.IfNotNull(value => value.Value)));
            }
            return ordered;
        }

        public static DbValue Max(this RowCollection rowCollection, string column)
        {
            return rowCollection.AggregateByColumn(column, (max, current) =>
                (max == null || DbValueMath.Compare(current, max, Comparison.GreaterThan)) ? current : max);
        }

        public static DbValue Min(this RowCollection rowCollection, string column)
        {
            return rowCollection.AggregateByColumn(column, (min, current) =>
                (min == null || DbValueMath.Compare(current, min, Comparison.LessThan)) ? current : min);
        }

        public static DbValue Sum(this RowCollection rowCollection, string column)
        {
            return rowCollection.AggregateByColumn(column, DbValueMath.Sum);
        }

        public static DbValue Average(this RowCollection rowCollection, string column)
        {
            var sum = rowCollection.AggregateByColumn(column, DbValueMath.Sum);
            var count = rowCollection.Count(row => row.Cells[column] != null && row.Cells[column].Value != null);
            if (sum.Value is long) return (double)(long)sum.Value / count;
            if (sum.Value is decimal) return (decimal)sum.Value / count;
            if (sum.Value is double) return (double)sum.Value / count;
            throw new InvalidTypeException(sum.Type, Resources.CannotAverageType.FormatExt(sum.Type));
        }

        private static DbValue AggregateByColumn(this RowCollection rowCollection, string column,
            Func<DbValue, DbValue, DbValue> aggregator)
        {
            return rowCollection
                .Select(row => row.Cells[column])
                .Where(cell => cell != null)
                .Select(cell => cell.Value)
                .Aggregate(aggregator);
        }
    }
}
