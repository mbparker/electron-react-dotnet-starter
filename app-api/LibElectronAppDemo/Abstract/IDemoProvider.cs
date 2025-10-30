using LibElectronAppApi.Shared.Abstract;
using LibSqlite3Orm.Abstract;

namespace LibElectronAppDemo.Abstract;

public interface IDemoProvider
{
    ISqliteConnection TryConnectToDemoDb(string dbFilename = null);
    void CreateDemoDb(IBackgroundTaskProgressHandler progressHandler, bool dropExisting = false, string dbFilename = null);
}