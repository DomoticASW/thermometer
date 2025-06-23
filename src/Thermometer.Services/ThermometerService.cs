using Thermometer.Core;

namespace Thermometer.Services;

public class ThermometerService : IThermometerService
{
    public ThermometerAgent Thermometer { get; } = new ThermometerAgent(new Ports.ServerCommunicationProtocolHttpAdapter());
}
