using System.Reflection;
using LibElectronAppDemo.Abstract;

namespace LibElectronAppDemo.Concrete;

public class ResourceExtractor : IResourceExtractor
{
    public byte[] ExtractResource(params string[] path)
    {
        using var resStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Join(".", nameof(LibElectronAppDemo),
                string.Join(".", path)));
        if (resStream is null) return [];
        using var outStream = new MemoryStream();
        resStream.CopyTo(outStream);
        return outStream.ToArray();
    }
}