using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Thermometer.Ports;

namespace Thermometer.Core
{
    public class ThermometerAgent
    {
        private readonly ServerCommunicationProtocolHttpAdapter _server;
        private ServerAddress? _serverAddress;
        private readonly ServerAddress _discoveryBroadcastAddress;
        private Timer? _timer;
        public BasicThermometer Thermometer { get; private set; }
        private double _lastActualTemperature;
        private readonly int _devicePort;
        public bool Registered { get; set; } = false;

        public ThermometerAgent(ServerCommunicationProtocolHttpAdapter server)
        {
            _devicePort = int.Parse(Environment.GetEnvironmentVariable("DEVICE_PORT") ?? "8080");
            string? serverAddress = Environment.GetEnvironmentVariable("SERVER_ADDRESS");
            string? serverPort = Environment.GetEnvironmentVariable("SERVER_PORT");

            if (serverAddress is not null && serverPort is not null)
            {
                _serverAddress = new ServerAddress(serverAddress, int.Parse(serverPort));
                Registered = true;
            }

            string? discoveryAddress = Environment.GetEnvironmentVariable("DISCOVERY_ADDRESS");
            string? discoveryPort = Environment.GetEnvironmentVariable("DISCOVERY_PORT");

            if (discoveryAddress is not null && discoveryPort is not null)
            {
                _discoveryBroadcastAddress = new ServerAddress(discoveryAddress, int.Parse(discoveryPort));
            }
            else
            {
            _discoveryBroadcastAddress = new ServerAddress("255.255.255.255", 30000);
            }            

            _server = server;
            Thermometer = new BasicThermometer();
            _lastActualTemperature = Thermometer.ActualTemperature;
            _ = AnnouncePresenceAsync();
        }

        public async Task<bool> AnnouncePresenceAsync()
        {
            try
            {
                await _server.Announce(_discoveryBroadcastAddress, _devicePort, Thermometer.Id, Thermometer.Name);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send announcement: {ex.Message}");
                return false;
            }
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
            Thermometer.SimulateTemperatureStep();

            if (Math.Abs(Thermometer.ActualTemperature - _lastActualTemperature) > 0.01)
            {
                await _server.SendEvent(_serverAddress!, "temperature-changed", Thermometer.Id);
                _lastActualTemperature = Thermometer.ActualTemperature;
            }

            await _server.UpdateState(_serverAddress!, "actualTemperature", Thermometer.ActualTemperature, Thermometer.Id);
            await _server.UpdateState(_serverAddress!, "requiredTemperature", Thermometer.RequiredTemperature, Thermometer.Id);
        }

        public void SetServerAddress(string host, int port)
        {
            _serverAddress = new ServerAddress(host, port);
        }
    }
}
