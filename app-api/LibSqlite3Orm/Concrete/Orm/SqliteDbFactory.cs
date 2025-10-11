using System.Text;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteDbFactory : ISqliteDbFactory
{
    private readonly Func<ISqliteConnection> connectionFactory;
    private readonly Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer> ddlSqlSynthesizerFactory;
    
    public SqliteDbFactory(Func<ISqliteConnection> connectionFactory,
        Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer> ddlSqlSynthesizerFactory)
    {
        this.connectionFactory = connectionFactory ?? throw new  ArgumentNullException(nameof(connectionFactory));
        this.ddlSqlSynthesizerFactory = ddlSqlSynthesizerFactory ?? throw new  ArgumentNullException(nameof(ddlSqlSynthesizerFactory));
    }
    
    public void Create(SqliteDbSchema schema, string dbFilename, bool dbFileMustExist) 
    {
        if (schema is null) throw new ArgumentNullException(nameof(schema));
        if (dbFilename is null) throw new ArgumentNullException(nameof(dbFilename));
        if (dbFilename.Trim() == string.Empty) throw new ArgumentException(nameof(dbFilename));
        var sql = SynthesizeCreateTablesAndIndexes(schema);
        using (var connection = connectionFactory())
        {
            connection.OpenReadWrite(dbFilename, dbFileMustExist);
            using (var cmd = connection.CreateCommand())
            {
                cmd.ExecuteNonQuery(sql);
            }
        }
    }
    
    public ISqliteConnection CreateSchema(ISqliteConnection connection, SqliteDbSchema schema) 
    {
        if (schema is null) throw new ArgumentNullException(nameof(schema));
        var sql = SynthesizeCreateTablesAndIndexes(schema);
        if (connection is null)
        {
            connection = connectionFactory();
            connection.OpenReadWriteInMemory();
        }
        using (var cmd = connection.CreateCommand())
        {
            cmd.ExecuteNonQuery(sql);
        }

        return connection;
    }
    
    private string SynthesizeCreateTablesAndIndexes(SqliteDbSchema schema)
    {
        var tableSynthesizer = ddlSqlSynthesizerFactory(SqliteDdlSqlSynthesisKind.TableOps, schema);
        var indexSynthesizer = ddlSqlSynthesizerFactory(SqliteDdlSqlSynthesisKind.IndexOps, schema);
        
        var sb = new StringBuilder();
        sb.AppendLine("PRAGMA foreign_keys = off;");
        sb.AppendLine("SAVEPOINT 'create_db';");

        sb.AppendLine("SAVEPOINT 'create_tables';");
        foreach (var table in schema.Tables.Values)
        {
            sb.AppendLine(tableSynthesizer.SynthesizeCreate(table.Name));
        }
        sb.AppendLine("RELEASE SAVEPOINT 'create_tables';");
        sb.AppendLine("PRAGMA foreign_keys = on;");

        if (schema.Indexes.Count != 0)
        {
            sb.AppendLine("SAVEPOINT 'create_indexes';");
            foreach (var index in schema.Indexes.Values)
            {
                sb.AppendLine(indexSynthesizer.SynthesizeCreate(index.IndexName));
            }

            sb.AppendLine("RELEASE SAVEPOINT 'create_indexes';");
        }

        sb.AppendLine("RELEASE SAVEPOINT 'create_db';");
        
        return sb.ToString();
    }
}