using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityCreator : IEntityCreator
{
    private readonly Func<ISqliteConnection> connectionFactory;
    private readonly Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory;
    private readonly ISqliteParameterPopulator  parameterPopulator;
    private readonly ISqliteEntityWriter entityWriter;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityCreator(Func<ISqliteConnection> connectionFactory,
        Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory,
        ISqliteParameterPopulator  parameterPopulator, ISqliteEntityWriter entityWriter, ISqliteOrmDatabaseContext context)
    {
        this.connectionFactory = connectionFactory;
        this.dmlSqlSynthesizerFactory = dmlSqlSynthesizerFactory;
        this.parameterPopulator = parameterPopulator;
        this.entityWriter = entityWriter;
        this.context = context;
    }

    public bool Insert<T>(T entity)
    {
        var synthesisResult = SynthesizeSql<T>();
        using (var connection = connectionFactory())
        {
            connection.Open(context.Filename, true);
            return Insert(connection, synthesisResult, entity);
        }
    }
    
    public bool Insert<T>(ISqliteConnection connection, T entity)
    {
        var synthesisResult = SynthesizeSql<T>();
        return Insert(connection, synthesisResult, entity);
    }    

    public bool Insert<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity)
    {
        using (var cmd = connection.CreateCommand())
        {
            parameterPopulator.Populate(synthesisResult, cmd.Parameters, entity);
            if (cmd.ExecuteNonQuery(synthesisResult.SqlText) == 1)
            {
                entityWriter.SetGeneratedKeyOnEntityIfNeeded(context.Schema, connection, entity);
                return true;
            }

            return false;
        }
    }

    public int InsertMany<T>(IEnumerable<T> entities)
    {
        using (var connection = connectionFactory())
        {
            connection.Open(context.Filename, true);
            return InsertMany(connection, entities);
        }   
    }
    
    public int InsertMany<T>(ISqliteConnection connection, IEnumerable<T> entities)
    {
        var synthesisResult = SynthesizeSql<T>();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                var cnt = 0;
                foreach (var entity in entities)
                {
                    if (Insert(connection, synthesisResult, entity))
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

    private DmlSqlSynthesisResult SynthesizeSql<T>()
    {
        var synthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Insert, context.Schema);
        return synthesizer.Synthesize<T>(SqliteDmlSqlSynthesisArgs.Empty);
    }
}