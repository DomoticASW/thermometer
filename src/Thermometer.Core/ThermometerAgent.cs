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
        private readonly string _lanHostname;
        private Timer? _timer;
        private Thread? _workerThread;
        public BasicThermometer Thermometer { get; private set; }
        private double _lastActualTemperature;
        private double _lastRequiredTemperature;
        private readonly int _devicePort;
        private volatile bool _isRunning;
        public bool Registered { get; set; } = false;

        public ThermometerAgent(ServerCommunicationProtocolHttpAdapter server)
        {
            _lanHostname = Environment.GetEnvironmentVariable("LAN_HOSTNAME")!;
            if (_lanHostname is null)
            {
                throw new ArgumentException("LAN_HOSTNAME environment variable is not set.");
            }
            
            _devicePort = int.Parse(Environment.GetEnvironmentVariable("DEVICE_PORT") ?? "8090");
            string? serverAddress = Environment.GetEnvironmentVariable("SERVER_ADDRESS");

            if (serverAddress is not null && serverAddress.Contains(':'))
            {
                string[] parts = serverAddress.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int port))
                {
                    _serverAddress = new ServerAddress(parts[0], port);
                    Registered = true;
                }
                else
                {
                    throw new ArgumentException("Invalid SERVER_ADDRESS format. Expected format: 'host:port'.");
                }
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
            _lastRequiredTemperature = Thermometer.RequiredTemperature;
            _ = AnnouncePresenceAsync();
        }

        public async Task<bool> AnnouncePresenceAsync()
        {
            try
            {
                await _server.Announce(_discoveryBroadcastAddress, _devicePort, Thermometer.Id, Thermometer.Name, _lanHostname);
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
            _timer = new Timer(_ => Thermometer.SimulateTemperatureStep(), null, TimeSpan.Zero, interval);
            _isRunning = true;
            _workerThread = new Thread(UpdateAndSend)
            {
                IsBackground = true,
                Name = "ThermometerAgentWorker"
            };
            
            _workerThread.Start();
            Console.WriteLine($"Thermometer agent started");
        }

        public void Stop()
        {
            _timer?.Dispose();
            _isRunning = false;
            _workerThread?.Join();
        }

        private async void UpdateAndSend(object? state)
        {
            while (_isRunning)
            {
                if (Math.Abs(Thermometer.ActualTemperature - _lastActualTemperature) > 0.01)
                {
                    await _server.SendEvent(_serverAddress!, "temperature-changed", Thermometer.Id);
                    _lastActualTemperature = Thermometer.ActualTemperature;
                    await _server.UpdateState(_serverAddress!, "actualTemperature", Thermometer.ActualTemperature, Thermometer.Id);
                }

                if (Math.Abs(Thermometer.RequiredTemperature - _lastRequiredTemperature) > 0.01)
                {
                    _lastRequiredTemperature = Thermometer.RequiredTemperature;
                    await _server.UpdateState(_serverAddress!, "requiredTemperature", Thermometer.RequiredTemperature, Thermometer.Id);
                }
                await Task.Delay(200);
            }
        }

        public void SetServerAddress(string host, int port)
        {
            _serverAddress = new ServerAddress(host, port);
        }
    }
}
