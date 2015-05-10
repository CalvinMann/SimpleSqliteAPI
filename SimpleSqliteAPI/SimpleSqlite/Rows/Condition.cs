using SimpleSqlite.Cells;

namespace SimpleSqlite.Rows
{
    public class Condition
    {
        public string Column { get; set; }
        public Comparison Comparison { get; set; }
        public DbValue Value { get; set; }

        public Condition() { }

        public Condition(string column, Comparison comparison, DbValue value)
        {
            Column = column;
            Comparison = comparison;
            Value = value;
        }
    }
}
