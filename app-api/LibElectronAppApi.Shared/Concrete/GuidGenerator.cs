using LibElectronAppApi.Shared.Abstract;

namespace LibElectronAppApi.Shared.Concrete;

public class GuidGenerator : IGuidGenerator
{
    public Guid Create()
    {
        return Guid.NewGuid();
    }
}