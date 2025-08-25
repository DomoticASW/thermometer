# thermometer

A Thermometer simulated device to test DomoticASW

## Docker Hub

[Docker Hub - fracarluccii/domoticasw-thermometer](https://hub.docker.com/repository/docker/fracarluccii/domoticasw-thermometer/general)

## Run with Docker

To run the thermometer device using Docker, you can use the following commands:

```bash
docker run fracarluccii/domoticasw-thermometer
```

### Variables

The following configurations can be passed to the container as environment variables

| Variable name     | Default value   | Explanation                         |
| ----------------- | --------------- | ----------------------------------- |
| ID                | thermometer-01  | thermometer ID                      |
| NAME              | Thermometer     | thermometer name                    |
| DEVICE_PORT       | 8090            | Port used by the thermometer device |
| SERVER_ADDRESS    | /               | Address:port of the server          |
| DISCOVERY_ADDRESS | 255.255.255.255 | Address for discovery broadcasts    |
| DISCOVERY_PORT    | 30000           | Port for discovery broadcasts       |

## How to use

At first send <code><\<device-address\>>/register</code> request to the device to register it in the server.

## Properties

- <b>actualTemperature</b>: The current temperature reading of the thermometer.
- <b>requiredTemperature</b>: The desired temperature to be set on the thermometer.

## Actions

- <code><\<device-address\>>/execute/set-temperature</code>: Set the desired temperature on the thermometer.

Body example for set-temperature:

```json
{
  "input": 25.5
}
```

## Events

- <b>temperatureChanged</b>: Triggered when the actual temperature changes.
