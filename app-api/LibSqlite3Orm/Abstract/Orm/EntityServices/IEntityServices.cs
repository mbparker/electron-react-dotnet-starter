namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityServices : IEntityCreator, IEntityGetter, IEntityDetailGetter, IEntityUpdater, IEntityDeleter,
    IEntityUpserter
{
}