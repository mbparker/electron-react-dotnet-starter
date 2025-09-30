using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityGetter : IEntityGetter
{
    private readonly Func<ISqliteConnection> connectionFactory;
    private readonly Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory;
    private readonly ISqliteParameterPopulator  parameterPopulator;
    private readonly ISqliteEntityWriter entityWriter;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityGetter(Func<ISqliteConnection> connectionFactory,
        Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory,
        ISqliteParameterPopulator parameterPopulator,
        Func<ISqliteOrmDatabaseContext, ISqliteEntityWriter> entityWriterFactory, ISqliteOrmDatabaseContext context)
    {
        this.connectionFactory = connectionFactory;
        this.dmlSqlSynthesizerFactory = dmlSqlSynthesizerFactory;
        this.parameterPopulator = parameterPopulator;
        entityWriter = entityWriterFactory(context);
        this.context = context;
    }

    public ISqliteQueryable<T> Get<T>(bool loadNavigationProps = false) where T : new()
    {
        return Get<T>(() =>
        {
            var connection = connectionFactory();
            connection.Open(context.Filename, true);
            return connection;
        }, loadNavigationProps, true);
    }

    public ISqliteQueryable<T> Get<T>(ISqliteConnection connection, bool loadNavigationProps = false) where T : new()
    {
        return Get<T>(() => connection, loadNavigationProps, false);
    }

    private ISqliteQueryable<T> Get<T>(Func<ISqliteConnection> connectionAllocator, bool loadNavigationProps, bool disposeConnection) where T : new()
    {
        var entityTypeName = typeof(T).AssemblyQualifiedName;
        var table = context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);
        if (table is not null)
        {
            ISqliteDataReader ExecuteQuery(SynthesizeSelectSqlArgs args)
            {
                var synthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Select, context.Schema);
                var synthesisResult = synthesizer.Synthesize<T>(new SqliteDmlSqlSynthesisArgs(args));
                using (var cmd = connectionAllocator().CreateCommand())
                {
                    parameterPopulator.Populate<T>(synthesisResult, cmd.Parameters);
                    return cmd.ExecuteQuery(synthesisResult.SqlText);
                }
            }

            T DeserializeRow(ISqliteDataRow row)
            {
                return entityWriter.Deserialize<T>(table, row, loadNavigationProps, connectionAllocator());
            }
            
            return new SqliteOrderedQueryable<T>(ExecuteQuery, DeserializeRow, disposeConnection);
        }
        
        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }
}