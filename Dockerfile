FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ENV NAME=Thermometer-01

WORKDIR /app

COPY . ./
RUN dotnet restore

RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80

ENTRYPOINT ["dotnet", "Thermometer.Api.dll"]
