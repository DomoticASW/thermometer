using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Thermometer.Core;
using Thermometer.Services;

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
    public IActionResult Register()
    {
        if (Environment.GetEnvironmentVariable("SERVER_ADDRESS") is null &&
            Environment.GetEnvironmentVariable("SERVER_PORT") is null)
        {
            _thermometerAgent.SetServerAddress(
                Request.Host.Host,
                Request.Host.Port ?? 8080
            );
        }
        _thermometerAgent.Start(TimeSpan.FromSeconds(30));
        var device = new
        {
            id = "328122790945",
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
