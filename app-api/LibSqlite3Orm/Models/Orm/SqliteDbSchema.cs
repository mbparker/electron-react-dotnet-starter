using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Models.Orm;

[Serializable]
public class SqliteDbSchema
{
    public Dictionary<string, SqliteDbSchemaTable> Tables { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, SqliteDbSchemaIndex> Indexes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class SqliteDbSchemaTable
{
    public string Name { get; set; }
    public string ModelTypeName { get; set; }
    public Dictionary<string, SqliteDbSchemaTableColumn> Columns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public SqliteDbSchemaTablePrimaryKeyColumn PrimaryKey { get; set; }
    public List<SqliteDbSchemaTableForeignKey> ForeignKeys { get; set; } = [];
    public string[] CompositePrimaryKeyFields { get; set; } = [];
    public List<SqliteOneToManyRelationship> DetailListProperties { get; set; } = [];
    public List<SqliteOneToOneRelationship> DetailProperties { get; set; } = [];
}

public class SqliteOneToManyRelationship
{
    public string DetailsListPropertyName { get; set; }
    public string DetailTableName { get; set; }
    public string DetailTableTypeName { get; set; }
}

public class SqliteOneToOneRelationship
{
    public string DetailsPropertyName { get; set; }
    public string DetailTableName { get; set; }
    public string DetailTableTypeName { get; set; }
}

public class SqliteDbSchemaTableColumn
{
    public string Name { get; set; }
    public string ModelFieldName { get; set; }
    public string ModelFieldTypeName { get; set; }
    public string SerializedFieldTypeName { get; set; }
    public SqliteColType DbFieldTypeAffinity { get; set; }
    public bool IsNotNull { get; set; }
    public SqliteLiteConflictAction? IsNotNullConflictAction { get; set; }
    public bool IsUnique { get; set; }
    public bool IsImmutable { get; set; }
    public SqliteLiteConflictAction? IsUniqueConflictAction { get; set; }
    public SqliteCollation? Collation { get; set; }
    public string DefaultValueLiteral { get; set; }
    //public string SerializerTypeName { get; set; }
}

public class SqliteDbSchemaTablePrimaryKeyColumn
{
    public string FieldName { get; set; }
    public bool Ascending { get; set; }
    public SqliteLiteConflictAction? PrimaryKeyConflictAction { get; set; }
    public bool AutoIncrement { get; set; }
    public bool AutoGuid { get; set; }
}

public class SqliteDbSchemaTableForeignKey
{
    public string[] FieldNames { get; set; }
    public string ForeignTableName { get; set; }
    public string ForeignTableModelTypeName { get; set; }
    public string[] ForeignTableFields { get; set; }
    public SqliteForeignKeyAction? UpdateAction { get; set; }
    public SqliteForeignKeyAction? DeleteAction { get; set; }
}

public class SqliteDbSchemaIndex
{
    public string TableName { get; set; }
    public string IndexName { get; set; }
    public bool IsUnique { get; set; }
    public List<SqliteDbSchemaIndexColumn> Columns { get; set; } = new();
}

public class SqliteDbSchemaIndexColumn
{
    public string Name { get; set; }
    public SqliteCollation? Collation { get; set; }
    public bool SortDescending { get; set; }
}