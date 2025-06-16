# thermometer

A Thermometer simulated device to test DomoticASW

## Docker Hub

[Docker Hub - fracarluccii/thermometer](https://hub.docker.com/repository/docker/fracarluccii/thermometer/general)

## Run with Docker

To run the thermometer device using Docker, you can use the following commands:

```bash
docker pull fracarluccii/thermometer:latest
docker run -d -p 8080:80 -e NAME=Thermometer-01 fracarluccii/thermometer
```

IF you want you can pass the name of the thermometer as an environment variable `NAME`

## Endpoints

| Metodo | URL                       | Descrizione                            |
| ------ | ------------------------- | -------------------------------------- |
| GET    | `/check-status`           | Current status of the thermometer      |
| POST   | `/register`               | Register the thermometer in the server |
| POST   | `/execute/set-temperature`| Set the temperature of the thermometer |

Body example for setting temperature:

```json
{
  "input": 25.5
}
```
