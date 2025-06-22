using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using Thermometer.Ports;

namespace Thermometer.Core
{
    public class ThermometerAgent
    {
        private readonly HttpClient _httpClient = new();
        private readonly ServerCommunicationProtocolHttpAdapter _server;
        private ServerAddress _serverAddress;
        private Timer? _timer;
        private double _lastActualTemperature;
        public BasicThermometer thermometer = new();

        public ThermometerAgent(ServerCommunicationProtocolHttpAdapter server, BasicThermometer thermometer)
        {
            _server = server;
            _lastActualTemperature = thermometer.ActualTemperature;
        }

        public void Start(TimeSpan interval)
        {
            _timer = new Timer(UpdateAndSend, null, TimeSpan.Zero, interval);
            Console.WriteLine($"Thermometer agent started. Sending updates every {interval.TotalSeconds} seconds.");
        }

        public void Stop()
        {
            _timer?.Dispose();
        }

        private async void UpdateAndSend(object? state)
        {
            thermometer.SimulateTemperatureStep();

            if (Math.Abs(thermometer.ActualTemperature - _lastActualTemperature) > 0.01)
            {
                await _server.SendEvent(_serverAddress, "temperature-changed", thermometer.id);
                _lastActualTemperature = thermometer.ActualTemperature;
            }

            await _server.UpdateState(_serverAddress, "actualTemperature", thermometer.ActualTemperature, thermometer.id);
            await _server.UpdateState(_serverAddress, "requiredTemperature", thermometer.RequiredTemperature, thermometer.id);
        }

        public void SetServerAddress(string host, int port)
        {
            _serverAddress = new ServerAddress(host, port);
        }
    }
}
