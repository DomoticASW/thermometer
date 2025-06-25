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
        private ServerAddress? _serverAddress;     
        private Timer? _timer;
        public BasicThermometer thermometer;
        private double _lastActualTemperature;

        public ThermometerAgent(ServerCommunicationProtocolHttpAdapter server)
        {
            string? serverAddress = Environment.GetEnvironmentVariable("SERVER_ADDRESS");
            string? serverPort = Environment.GetEnvironmentVariable("SERVER_PORT");

            if (serverAddress is not null && serverPort is not null)
            {
                _serverAddress = new ServerAddress(serverAddress, int.Parse(serverPort));
            }
            _server = server;
            thermometer = new BasicThermometer();
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
                await _server.SendEvent(_serverAddress!, "temperature-changed", thermometer.Id);
                _lastActualTemperature = thermometer.ActualTemperature;
            }

            await _server.UpdateState(_serverAddress!, "actualTemperature", thermometer.ActualTemperature, thermometer.Id);
            await _server.UpdateState(_serverAddress!, "requiredTemperature", thermometer.RequiredTemperature, thermometer.Id);
        }

        public void SetServerAddress(string host, int port)
        {
            _serverAddress = new ServerAddress(host, port);
            Console.WriteLine(_serverAddress);
        }
    }
}
