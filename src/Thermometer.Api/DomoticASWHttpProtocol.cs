using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Thermometer.Core;
using Thermometer.Services;
using Thermometer.Ports;

[ApiController]
[Route("/")]
public class DomoticASWHttpProtocol : ControllerBase
{
    private readonly IThermometerService _thermometerService;
    private readonly ThermometerAgent _thermometerAgent;
    private readonly BasicThermometer _thermometer;

    public DomoticASWHttpProtocol(IThermometerService thermometerService)
    {
        _thermometerService = thermometerService;
        _thermometerAgent = _thermometerService.Thermometer;
        _thermometer = _thermometerAgent.thermometer;
    }

    [HttpGet("check-status")]
    public IActionResult CheckStatus()
    {
        return Ok(new { _thermometer.Name, ActualTemperature = _thermometer.ActualTemperature, RequiredTemperature = _thermometer.RequiredTemperature });
    }

    [HttpPost("execute/{deviceActionId}")]
    public IActionResult ExecuteAction(string deviceActionId, [FromBody] ExecuteInput input)
    {
        switch (deviceActionId.ToLower())
        {
            case "set-temperature":
                if (input?.Input is JsonElement tempElement && tempElement.TryGetDouble(out double tempValue))
                {
                    _thermometer.SetRequiredTemperature(tempValue);
                    return Ok(new { Temperature = _thermometer.RequiredTemperature });
                }
                return BadRequest("Invalid input for temperature");
            default:
                return NotFound("Unknown action");
        }
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] ServerAddress input)
    {
        if (input?.Host is string host && input?.Port is int port && !string.IsNullOrEmpty(host) && port > 0)
        {
            _thermometerAgent.SetServerAddress(host, port);
            _thermometerAgent.Start(TimeSpan.FromSeconds(30));
        }
        var device = new
        {
            id = _thermometer.Id,
            name = _thermometer.Name,
            properties = new object[]
            {
                new {
                    id = "actualTemperature",
                    name = "ActualTemperature",
                    value = _thermometer.ActualTemperature,
                    typeConstraints = new {
                        constraint = "DoubleRange",
                        min = 16.0,
                        max = 30.0
                    }
                },
                new {
                    id = "requiredTemperature",
                    name = "RequiredTemperature",
                    value = _thermometer.RequiredTemperature,
                    typeConstraints = new {
                        constraint = "DoubleRange",
                        min = 16.0,
                        max = 30.0
                    }
                }
            },
            actions = new object[]
            {
                new {
                    id = "set-temperature",
                    name = "Set Temperature",
                    description = "Sets the thermometer temperature",
                    inputTypeConstraints = new {
                        type = "Double",
                        constraint = "DoubleRange",
                        min = 16.0,
                        max = 30.0
                    }
                }
            },
            events = new[] { "temperature-changed" }
        };

        return Ok(device);
    }

    public class ExecuteInput
    {
        public JsonElement Input { get; set; }
    }
}
