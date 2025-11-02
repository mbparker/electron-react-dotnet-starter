namespace LibElectronAppApi.Models;

public class AppNotificationEventArgs
{
    public AppNotificationEventArgs(AppNotificationKind kind, object eventData = null)
    {
        Kind = kind;
        EventData = eventData;
    }
        
    public AppNotificationKind Kind { get; }
        
    public object EventData { get; }
}