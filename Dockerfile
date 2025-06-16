FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ENV NAME=Thermometer-01

WORKDIR /src

# Copia i progetti
COPY src/Thermometer.Core/*.csproj Thermometer.Core/
COPY src/Thermometer.Services/*.csproj Thermometer.Services/
COPY src/Thermometer.Api/*.csproj Thermometer.Api/

# Ripristina dipendenze
RUN dotnet restore Thermometer.Api/Thermometer.Api.csproj

# Copia tutto il codice sorgente
COPY src/ .

# Costruisci
WORKDIR /src/Thermometer.Api
RUN dotnet publish Thermometer.Api.csproj -c Release -o /app/out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "Thermometer.Api.dll"]
