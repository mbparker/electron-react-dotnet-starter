using System.Reflection;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Models.Orm;

public class SqliteDbSchemaOptions
{
    public Dictionary<string, SqliteTableOptions> Tables { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, List<SqliteIndexOptions>> Indexes { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public class SqliteTableOptions
{
    public SqliteTableOptions(SqliteDbSchemaOptions schemaOptions)
    {
        SchemaOptions = schemaOptions;
    }
    
    public SqliteDbSchemaOptions SchemaOptions { get; }
    public Type TableType { get; set; }
    public string Name { get; set; }
    public Dictionary<string, SqliteTableColumnOptions> Columns { get; } = new(StringComparer.OrdinalIgnoreCase);
    public SqliteTablePrimaryKeyColumnOptions PrimaryKeyColumnOptions { get; set; }
    public MemberInfo[] CompositePrimaryKeyProperties { get; set; } = [];
    public List<SqliteTableForeignKeyOptions> ForeignKeys { get; } = [];

    public Dictionary<string, SqliteTableForeignKeyOptions> DetailProperties { get; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public class SqliteTableColumnOptions
{
    public SqliteTableColumnOptions(SqliteTableOptions tableOptions)
    {
        TableOptions = tableOptions;
    }
    
    public SqliteTableOptions TableOptions { get; }
    
    public MemberInfo Member { get; set; }
    
    public string Name { get; set; }
    
    public bool IsNotNull { get; set; }
    public SqliteLiteConflictAction? IsNotNullConflictAction { get; set; }
    
    public bool IsUnique { get; set; }
    public SqliteLiteConflictAction? IsUniqueConflictAction { get; set; }
    
    public SqliteCollation? Collation { get; set; }

    public string DefaultValueLiteral { get; set; }
    
    public bool IsImmutable { get; set; }
}

public class SqliteTablePrimaryKeyColumnOptions : SqliteTableColumnOptions
{
    public SqliteTablePrimaryKeyColumnOptions(SqliteTableOptions tableOptions)
        : base(tableOptions)
    {
    }
    
    public bool Ascending { get; set; }
    public SqliteLiteConflictAction? PrimaryKeyConflictAction { get; set; }
    public bool AutoIncrement { get; set; }
    public bool AutoGuid { get; set; }
}

public class SqliteTableForeignKeyOptions
{
    public SqliteTableForeignKeyOptions(SqliteTableOptions tableOptions)
    {
        TableOptions = tableOptions;
    }
    
    public SqliteTableOptions TableOptions { get; }
    
    public MemberInfo[] TableProperties { get; set; }
    public Type ForeignTableType { get; set; }
    public MemberInfo[] ForeignTableProperties { get; set; }
    public MemberInfo ForeignTableDetailListProperty { get; set; }
    public MemberInfo ForeignTableDetailProperty { get; set; }
    public SqliteForeignKeyAction? UpdateAction { get; set; }
    public SqliteForeignKeyAction? DeleteAction { get; set; }
}

public class SqliteIndexOptions
{
    public SqliteIndexOptions(SqliteDbSchemaOptions schemaOptions)
    {
        SchemaOptions = schemaOptions;
    }
    public SqliteDbSchemaOptions SchemaOptions { get; }
    public Type TableType { get; set; }
    public string IndexName { get; set; }
    public bool IsUnique { get; set; }
    
    public List<SqliteIndexColumnOptions> Columns { get; } = new();
}

public class SqliteIndexColumnOptions
{
    public SqliteIndexColumnOptions(SqliteIndexOptions indexOptions)
    {
        IndexOptions = indexOptions;
    }
    
    public SqliteIndexOptions IndexOptions { get; }
    public MemberInfo Member { get; set; }
    public SqliteCollation? Collation { get; set; }
    public bool SortDescending { get; set; }
}