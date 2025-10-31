namespace LibElectronAppApi.Shared;

public static class SharedConstants
{
    public const string AppName = "ElectronApp";
    public static readonly DateTime UnixEpoch = DateTime.UnixEpoch;
    public static readonly DateTime HfsEpoch = new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime CocoaEpoch = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime Sas4GlEpoch = new DateTime(1960, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime WebKitEpoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}