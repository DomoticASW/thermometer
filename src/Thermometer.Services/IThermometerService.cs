using Thermometer.Core;

namespace Thermometer.Services;

public interface IThermometerService
{
    BasicThermometer Thermometer { get; }
}
