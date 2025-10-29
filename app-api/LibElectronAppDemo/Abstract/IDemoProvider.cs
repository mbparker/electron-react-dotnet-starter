namespace LibElectronAppDemo.Abstract;

public interface IDemoProvider
{
    string CreateDemoDb(string dbFilename = null);
}