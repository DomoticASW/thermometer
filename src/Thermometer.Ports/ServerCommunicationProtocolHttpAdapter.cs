using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Thermometer.Ports
{
    public class ServerCommunicationProtocolHttpAdapter : IServerCommunicationProtocol
    {
        private readonly HttpClient _httpClient;

        public ServerCommunicationProtocolHttpAdapter()
        {
            _httpClient = new HttpClient();
        }

        public async Task SendEvent(ServerAddress serverAddress, string eventName, string deviceId)
        {
            var url = $"http://{serverAddress.Host}:{serverAddress.Port}/api/devices/{deviceId}/events";
            var payload = JsonSerializer.Serialize(new { @event = eventName });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            Console.WriteLine($"CLIENT: Sending event to {url}");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                Console.WriteLine($"CLIENT: Event response: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CLIENT ERROR: {ex.Message}");
            }
        }

        public async Task UpdateState(ServerAddress serverAddress, string propertyName, object propertyValue, string deviceId)
        {
            var url = $"http://{serverAddress.Host}:{serverAddress.Port}/api/devices/{deviceId}/properties/{propertyName}";
            var payload = JsonSerializer.Serialize(new { value = propertyValue });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            Console.WriteLine($"CLIENT: Updating state {propertyName}={propertyValue} to {url}");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                Console.WriteLine($"CLIENT: Update response: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CLIENT ERROR: {ex.Message}");
            }
        }
    }
}
