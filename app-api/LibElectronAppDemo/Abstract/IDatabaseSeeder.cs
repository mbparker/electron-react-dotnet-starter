using LibSqlite3Orm.Abstract;

namespace LibElectronAppDemo.Abstract;

public interface IDatabaseSeeder
{
    void SeedDatabase(ISqliteConnection connection);
}