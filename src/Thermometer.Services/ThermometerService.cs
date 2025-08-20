using Thermometer.Core;
using Thermometer.Ports;
using Microsoft.Extensions.Hosting;

namespace Thermometer.Services;

public class ThermometerService : IThermometerService, IHostedService
{
    public ThermometerAgent Thermometer { get; }
    public bool IsRunning { get; private set; }
    private readonly CancellationTokenSource _cts = new();

    public ThermometerService()
    {
        Thermometer = new ThermometerAgent(new ServerCommunicationProtocolHttpAdapter());
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsRunning) return Task.CompletedTask;

        else
        {
            if (!Thermometer.Registered)
            {
                Console.WriteLine("Thermometer not registered, starting presence announcement loop");
                _ = Task.Run(async () =>
                {
                    while (!_cts.IsCancellationRequested && !Thermometer.Registered)
                    {
                    try
                    {
                        await Thermometer.AnnouncePresenceAsync();
                        await Task.Delay(5000, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
                if (Thermometer.Registered)
                {
                    Thermometer.Start(TimeSpan.FromSeconds(30));
                }
            }, _cts.Token);
        }
        IsRunning = true;
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!IsRunning) return Task.CompletedTask;
        
        _cts.Cancel();
        Thermometer.Stop();
        IsRunning = false;
        return Task.CompletedTask;
    }
}
