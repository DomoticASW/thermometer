using System;
using System.Threading;
using System.Threading.Tasks;

namespace Thermometer.Core
{
    public class BasicThermometer
    {
        private readonly Random _random = new();
        private readonly Timer _timer;

        public string Name { get; } = Environment.GetEnvironmentVariable("NAME") ?? "Thermometer-01";

        public double ActualTemperature { get; private set; } = 20.0;
        public double RequiredTemperature { get; private set; } = 20.0;

        public BasicThermometer()
        {
            int interval = _random.Next(20, 61) * 1000;
            _timer = new Timer(UpdateTemperature, null, interval, interval);
        }

        private void UpdateTemperature(object? state)
        {
            if (Math.Abs(ActualTemperature - RequiredTemperature) < 0.1)
                return;

            ActualTemperature += ActualTemperature < RequiredTemperature ? 0.5 : 0.5;

            int nextInterval = _random.Next(20, 61) * 1000;
            _timer.Change(nextInterval, Timeout.Infinite);
        }

        public void SetRequiredTemperature(double temperature)
        {
            if (temperature % 0.5 != 0)
            {
                temperature = Math.Round(temperature * 2) / 2.0;
            }
            if (temperature < 16.0) temperature = 16.0;
            if (temperature > 30.0) temperature = 30.0;
            RequiredTemperature = temperature;
        }

        public double GetActualTemperature() => ActualTemperature;

        public double GetRequiredTemperature() => RequiredTemperature;
    }
}
