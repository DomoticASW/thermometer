namespace Thermometer.Ports
{
    public record ServerAddress(string Host, int ServerPort);

    public interface IServerCommunicationProtocol
    {
        Task SendEvent(ServerAddress serverAddress, string eventName, string deviceId);
        Task UpdateState(ServerAddress serverAddress, string propertyName, object propertyValue, string deviceId);
        Task Announce(ServerAddress discoveryBroadcastAddress, int devicePort, string deviceId, string deviceName, string lanHostname);
    }
}
