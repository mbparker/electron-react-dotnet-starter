using LibElectronAppApi.Abstract;

namespace LibElectronAppApi.Concrete;

public class GuidGenerator : IGuidGenerator
{
    public Guid Create()
    {
        return Guid.NewGuid();
    }
}