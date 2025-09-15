using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityUpdater : IEntityUpdater
{
    private readonly Func<ISqliteConnection> connectionFactory;
    private readonly Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory;
    private readonly ISqliteParameterPopulator  parameterPopulator;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityUpdater(Func<ISqliteConnection> connectionFactory,
        Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory,
        ISqliteParameterPopulator  parameterPopulator, ISqliteOrmDatabaseContext context)
    {
        this.connectionFactory = connectionFactory;
        this.dmlSqlSynthesizerFactory = dmlSqlSynthesizerFactory;
        this.parameterPopulator = parameterPopulator;
        this.context = context;
    }
    
    public bool Update<T>(T entity)
    {
        var synthesisResult = SynthesizeSql<T>();
        using (var connection = connectionFactory())
        {
            connection.Open(context.Filename, true);
            return Update(connection, synthesisResult, entity);
        }
    }

    public bool Update<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity)
    {
        var cmd = connection.CreateCommand();
        parameterPopulator.Populate(synthesisResult, cmd.Parameters, entity);
        return cmd.ExecuteNonQuery(synthesisResult.SqlText) == 1;
    }

    public int UpdateMany<T>(IEnumerable<T> entities)
    {
        var synthesisResult = SynthesizeSql<T>();
        using (var connection = connectionFactory())
        {
            connection.Open(context.Filename, true);
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var cnt = 0;
                    foreach (var entity in entities)
                    {
                        if (Update(connection, synthesisResult, entity))
                            cnt++;
                    }

                    transaction.Commit();
                    return cnt;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
    
    private DmlSqlSynthesisResult SynthesizeSql<T>()
    {
        var synthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Update, context.Schema);
        return synthesizer.Synthesize<T>(SqliteDmlSqlSynthesisArgs.Empty);
    }
}