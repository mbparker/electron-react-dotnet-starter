using LibElectronAppApi.Shared.Abstract;
using LibSqlite3Orm.Abstract;

namespace LibElectronAppDemo.Abstract;

public interface IDatabaseSeeder
{
    void SeedDatabase(IBackgroundTaskProgressHandler progressHandler, ISqliteConnection connection);
}