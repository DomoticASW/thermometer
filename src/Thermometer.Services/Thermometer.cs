using Thermometer.Core;

namespace Thermometer.Services;

public class ThermometerService : IThermometerService
{
    public BasicThermometer Thermometer { get; } = new BasicThermometer();
}
