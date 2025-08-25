using Thermometer.Core;

namespace Thermometer.Services;

public interface IThermometerService
{
    ThermometerAgent Thermometer { get; }
    bool IsRunning { get; }
    void Start();
    void Stop();
}
