using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Thermometer.Core;
using Thermometer.Services;

[ApiController]
[Route("/")]
public class DomoticASWHttpProtocol(IThermometerService thermometerService) : ControllerBase
{
    private readonly BasicThermometer thermometer = thermometerService.Thermometer;

    [HttpGet("check-status")]
    public IActionResult CheckStatus()
    {
        return Ok(new { thermometer.Name, ActualTemperature = thermometer.ActualTemperature, RequiredTemperature = thermometer.RequiredTemperature });
    }

    [HttpPost("execute/{deviceActionId}")]
    public IActionResult ExecuteAction(string deviceActionId, [FromBody] ExecuteInput input)
    {
        switch (deviceActionId.ToLower())
        {
            case "set-temperature":
                if (input?.Input is JsonElement tempElement && tempElement.TryGetDouble(out double tempValue))
                {
                    thermometer.SetRequiredTemperature(tempValue);
                    return Ok(new { Temperature = thermometer.RequiredTemperature });
                }
                return BadRequest("Invalid input for temperature");
            default:
                return NotFound("Unknown action");
        }
    }

    [HttpPost("register")]
    public IActionResult Register()
    {
        var device = new
        {
            id = "328122790945",
            name = thermometer.Name,
            properties = new object[]
            {
                new {
                    id = "actualTemperature",
                    name = "ActualTemperature",
                    value = thermometer.ActualTemperature,
                    typeConstraints = new {
                        constraint = "DoubleRange",
                        min = 16.0,
                        max = 30.0
                    }
                },
                new {
                    id = "requiredTemperature",
                    name = "RequiredTemperature",
                    value = thermometer.RequiredTemperature,
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
