using SimpleSqlite.Base;

namespace SimpleSqlite.ForeignKeys
{
    public class ColumnForeignKeyCollection : SimpleCollection<ColumnForeignKey>
    {
        public override void Add(ColumnForeignKey key)
        {
            //TODO: checks
            base.Add(key);
        }

        internal void AddExisting(ColumnForeignKey key)
        {
            base.Add(key);
        }
    }
}
