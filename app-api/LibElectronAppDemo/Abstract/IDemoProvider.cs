namespace LibElectronAppDemo.Abstract;

public interface IDemoProvider
{
    string CreateDemoDb(bool dropExisting = false, string dbFilename = null);
}