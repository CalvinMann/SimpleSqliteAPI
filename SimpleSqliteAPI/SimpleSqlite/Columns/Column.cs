using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleSqlite.Base;
using SimpleSqlite.Cells;
using SimpleSqlite.Exceptions;
using SimpleSqlite.ForeignKeys;
using SimpleSqlite.Helpers;
using SimpleSqlite.Properties;
using SimpleSqlite.Tables;

namespace SimpleSqlite.Columns
{
    [DebuggerDisplay("{Name}")]
    public class Column : INamedObject
    {
        private const string CreateSql = @"ALTER TABLE ""{0}"" ADD COLUMN {1}";
        private const string ListColumnsTable = "simplesqlite_listcolumns";
        private const string CreateListColumnSql =
            @"INSERT INTO """ + ListColumnsTable + @""" (""table"", ""column"", ""type"") VALUES ('{0}','{1}',{2});";
        private readonly Regex _blobRegex = new Regex(@"[xX]['""]([0-9a-fA-F]*)['""]");

        private readonly string _dbType;
        private string _name;
        private ColumnType _type;
        private ColumnQuantity _quantity;
        private bool _isNullable;
        private bool _isPrimaryKey;
        private DbValue _defaultValue;
        
        private ColumnForeignKeyCollection _foreignKeys;

        public Table Table { get; private set; }

        public ColumnForeignKeyCollection ForeignKeys
        {
            get { return _foreignKeys ?? (_foreignKeys = new ColumnForeignKeyCollection()); }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                if (!IsNameValid(value))
                    throw new InvalidNameException(value, Resources.InvalidColumnName.FormatExt(value));
                if (IsAttached)
                    throw new InvalidOperationException(Resources.CannotChangeAttachedColumn.FormatExt(Name));
                _name = value;
            }
        }

        public ColumnType Type
        {
            get { return _type; }
            set
            {
                if (_type == value) return;
                if (IsAttached)
                    throw new InvalidOperationException(Resources.CannotChangeAttachedColumn.FormatExt(Name));
                _type = value;
            }
        }

        public ColumnQuantity Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity == value) return;
                if (IsAttached)
                    throw new InvalidOperationException(Resources.CannotChangeAttachedColumn.FormatExt(Name));
                _quantity = value;
            }
        }

        public bool IsNullable
        {
            get { return _isNullable; }
            set
            {
                if (_isNullable == value) return;
                if (IsAttached)
                    throw new InvalidOperationException(Resources.CannotChangeAttachedColumn.FormatExt(Name));
                _isNullable = value;
            }
        }

        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set
            {
                if (_isPrimaryKey == value) return;
                if (IsAttached)
                    throw new InvalidOperationException(Resources.CannotChangePrimaryKey.FormatExt(Name));
                if (Table != null && !Table.PrimaryKey.Contains(this))
                    Table.PrimaryKey.Add(this);
                _isPrimaryKey = value;
            }
        }

        public DbValue DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                if (_defaultValue == value) return;
                if (IsAttached)
                    throw new InvalidOperationException(Resources.CannotChangePrimaryKey.FormatExt(Name));
                _defaultValue = value;
            }
        }

        /// <summary>
        /// Indicates whether the row is linked to a physical row in a database.
        /// </summary>
        public bool IsAttached { get { return Table != null && Table.IsAttached; } }

        public Column()
        {
            _dbType = Type.ToDbString();
            IsNullable = true;
        }

        public Column(string name) 
            : this()
        {
            Name = name;
        }

        public Column(string name, ColumnType type) 
            : this(name)
        {
            Type = type;
            _dbType = Type.ToDbString();
        }

        public Column(string name, ColumnType type, bool isNullable) 
            : this(name, type)
        {
            IsNullable = isNullable;
        }

        public Column(string name, ColumnType type, bool isNullable, bool isKey) 
            : this(name, type, isNullable)
        {
            IsPrimaryKey = isKey;
        }

        public Column(string name, ColumnType type, DbValue defaultValue) 
            : this(name, type)
        {
            DefaultValue = defaultValue;
        }

        public Column(string name, ColumnType type, bool isNullable, DbValue defaultValue)
            : this(name, type, isNullable)
        {
            DefaultValue = defaultValue;
        }

        public Column(string name, ColumnType type, bool isNullable, bool isKey, DbValue defaultValue) 
            : this(name, type, isNullable, isKey)
        {
            DefaultValue = defaultValue;
        }

        public Column(string name, ColumnType type, ColumnQuantity quantity)
            : this(name, type)
        {
            Quantity = quantity;
            if (quantity == ColumnQuantity.List)
                _dbType = ColumnType.BLOB.ToDbString();
        }

        public Column(string name, ColumnType type, ColumnQuantity quantity, bool isNullable)
            : this(name, type, isNullable)
        {
            Quantity = quantity;
            if (quantity == ColumnQuantity.List)
                _dbType = ColumnType.BLOB.ToDbString();
        }

        public Column(string name, ColumnType type, ColumnQuantity quantity, bool isNullable, bool isKey)
            : this(name, type, isNullable, isKey)
        {
            Quantity = quantity;
            if (quantity == ColumnQuantity.List)
                _dbType = ColumnType.BLOB.ToDbString();
        }

        public Column(string name, ColumnType type, ColumnQuantity quantity, bool isNullable, bool isKey, DbValue defaultValue)
            : this(name, type, isNullable, isKey, defaultValue)
        {
            Quantity = quantity;
            if (quantity == ColumnQuantity.List)
                _dbType = ColumnType.BLOB.ToDbString();
        }

        internal Column(Table table, string name, ColumnType type, ColumnQuantity quantity, bool isNullable, bool isKey, string defaultValue)
        {
            Table = table;
            _name = name;
            _dbType = (quantity == ColumnQuantity.List) ? ColumnType.BLOB.ToDbString() : type.ToDbString();
            _type = type;
            _quantity = quantity;
            _isNullable = isNullable;
            _isPrimaryKey = isKey;
            _defaultValue = ParseDefaultValue(defaultValue);
        }

        internal void AddToTable(Table table)
        {
            if (table.IsAttached)
            {
                if (!IsNameValid(Name))
                    throw new InvalidNameException(Name, Resources.InvalidColumnName.FormatExt(Name));
                if (IsPrimaryKey)
                    throw new TableChangeNotSupported(table, this, Resources.CannotAddPrimaryKey.FormatExt(Name));
                if (ForeignKeys.Any())
                    throw new TableChangeNotSupported(table, this,
                        Resources.CannotAddForeignKeyToAttachedTable.FormatExt(Name, table.Name));
                if (!IsNullable && DefaultValue == null)
                    throw new InvalidOperationException(Resources.CannotAddNotNullWithoutValue.FormatExt(Name));
                var sql = CreateSql.FormatExt(table.Name, Definition);
                table.Database.Connection.ExecuteNonQuery(sql);
                if (Quantity == ColumnQuantity.List)
                    table.Database.Connection.ExecuteNonQuery(CreateListColumnSql.FormatExt(table.Name, Name,
                        (long)Type));
            }
            Table = table;
        }

        internal void RemoveFromTable()
        {
            if (IsAttached)
                throw new InvalidOperationException(Resources.CannotRemoveAttachedColumn.FormatExt(Name, Table.Name));
            Table.PrimaryKey.Remove(this);
            Table = null;
        }

        internal string Definition
        {
            get
            {
                
                return @"""{0}"" {1} {2} {3}".FormatExt(Name, _dbType, 
                    IsNullable ? "" : "NOT NULL",
                    DefaultValue != null ? "DEFAULT {0}".FormatExt(SqlDefaultValue) : "");
            }
        }

        private string SqlDefaultValue
        {
            get
            {
                return Type == ColumnType.Text ? (string)DefaultValue :
                       Type == ColumnType.BLOB ? "X\'{0}\'".FormatExt(((byte[])DefaultValue).ToHex()) :
                       Type == ColumnType.Integer ? ((long)DefaultValue).ToString(CultureInfo.InvariantCulture) :
                       Type == ColumnType.Real ? ((double)DefaultValue).ToString(CultureInfo.InvariantCulture) :
                       Type == ColumnType.Numeric ? ((decimal)DefaultValue).ToString(CultureInfo.InvariantCulture) :
                       null;
            }
        }

        private DbValue ParseDefaultValue(string value)
        {
            if (value == null) return null;
            if (Type != ColumnType.BLOB) value = value.Trim('"','\'');
            try
            {
                return Type == ColumnType.Text ? value :
                       Type == ColumnType.BLOB ? ParseBLOBString(value) :
                       Type == ColumnType.Integer ? Int64.Parse(value, CultureInfo.InvariantCulture) :
                       Type == ColumnType.Real ? Double.Parse(value, CultureInfo.InvariantCulture) :
                       Type == ColumnType.Numeric ? Decimal.Parse(value, CultureInfo.InvariantCulture) :
                       (DbValue)null;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FormatException || ex is OverflowException)
                {
                    throw new ArgumentException("value",
                        Resources.InvalidColumnDefaultValue.FormatExt(Name, value, Type), ex);
                }
                throw;
            }
        }

        private DbValue ParseBLOBString(string value)
        {
            var match = _blobRegex.Match(value);
            if (!match.Success)
                throw new FormatException(Resources.InvalidBLOBString.FormatExt(value));
            return match.Groups[1].Value.HexToByte();
        }

        public static bool IsNameValid(string name)
        {
            return !String.IsNullOrWhiteSpace(name) && !name.Contains("\"");
        }

        internal void AddListColumn(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(CreateListColumnSql.FormatExt(Table.Name, Name, (long)Type), connection))
            {
                command.ExecuteNonQuery();
            }
        }

        internal DbType DbType
        {
            get { return Quantity == ColumnQuantity.List ? DbType.Binary : Type.ToDbType(); }
        }
    }
}
