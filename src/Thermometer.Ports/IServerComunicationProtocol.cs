namespace Thermometer.Ports
{
    public record ServerAddress(string Host, int Port);

    public interface IServerCommunicationProtocol
    {
        Task SendEvent(ServerAddress serverAddress, string eventName, string deviceId);
        Task UpdateState(ServerAddress serverAddress, string propertyName, object propertyValue, string deviceId);
    }
}
