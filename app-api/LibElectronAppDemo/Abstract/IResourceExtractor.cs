namespace LibElectronAppDemo.Abstract;

public interface IResourceExtractor
{
    byte[] ExtractResource(params string[] path);
}