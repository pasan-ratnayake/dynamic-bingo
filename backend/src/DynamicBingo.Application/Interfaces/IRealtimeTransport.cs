namespace DynamicBingo.Application.Interfaces;

public interface IRealtimeTransport
{
    Task SendToUserAsync(Guid userId, string method, object data);
    Task SendToGroupAsync(string groupName, string method, object data);
    Task AddToGroupAsync(string connectionId, string groupName);
    Task RemoveFromGroupAsync(string connectionId, string groupName);
    Task SendToConnectionAsync(string connectionId, string method, object data);
}
