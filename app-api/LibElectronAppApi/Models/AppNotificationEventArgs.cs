namespace LibElectronAppApi.Models;

public class AppNotificationEventArgs
{
    public AppNotificationEventArgs(int eventId, object eventData = null)
    {
        EventId = eventId;
        EventData = eventData;
    }
        
    public int EventId { get; }
        
    public object EventData { get; }
}