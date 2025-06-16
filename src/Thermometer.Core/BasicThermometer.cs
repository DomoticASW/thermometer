namespace Thermometer.Core;

public class BasicThermometer
{
    public string Name { get; } = Environment.GetEnvironmentVariable("NAME") ?? "Thermometer-01";
    public double Temperature { get; private set; } = 20.0;

    public double GetTemperature()
    {
        return Temperature;
    }

    public void SetTemperature(double temperature)
    {
        if (temperature < 16.0) temperature = 16.0;
        if (temperature > 30.0) temperature = 30.0;
        Temperature = temperature;
    }
}
