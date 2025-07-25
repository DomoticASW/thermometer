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
        _thermometer = _thermometerAgent.Thermometer;
    }

    [HttpGet("check-status")]
    public IActionResult CheckStatus()
    {
        return Ok(new { _thermometer.Name, ActualTemperature = _thermometer.ActualTemperature, RequiredTemperature = _thermometer.RequiredTemperature });
    }

    [HttpPost("execute/{deviceActionId}")]
    public IActionResult ExecuteAction(string deviceActionId, [FromBody] ExecuteInput input)
    {
        Console.WriteLine($"Executing action: {deviceActionId} with input: {input?.Input}");
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
    public IActionResult Register([FromBody] JsonElement Input)
    {
        int port = Input.GetProperty("serverPort").GetInt32();
        if (port <= 0 || port > 65535)
        {
            return BadRequest("Invalid port number");
        }
        {
            _thermometerAgent.SetServerAddress(Request.HttpContext.Connection.RemoteIpAddress!.ToString(), port);
            Console.WriteLine($"Thermometer registered at {Request.HttpContext.Connection.RemoteIpAddress}:{port}");
            _thermometerAgent.Registered = true;
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
                    setterActionId = "set-temperature",
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
