using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteOrmDatabaseContext
{
    string Filename { get; set; }
    SqliteDbSchema Schema { get; }
}