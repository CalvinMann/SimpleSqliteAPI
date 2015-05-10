namespace SimpleSqlite.Rows
{
    public class Order
    {
        public string Column { get; set; }
        public bool IsAscending { get; set; }

        public Order() { }

        public Order(string column, bool isAscending)
        {
            Column = column;
            IsAscending = isAscending;
        }
    }
}
