using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityCreator
{
    bool Insert<T>(T entity);
    bool Insert<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity);
    int InsertMany<T>(IEnumerable<T> entities);
}