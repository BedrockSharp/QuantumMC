FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "src/QuantumMC/QuantumMC.csproj"

RUN dotnet publish "src/QuantumMC/QuantumMC.csproj" -c Release -o /app/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

RUN groupadd -r quantum && useradd -r -g quantum quantum
RUN mkdir -p /app/data && chown -R quantum:quantum /app

COPY --from=build /app/publish .

EXPOSE 19132/udp

USER quantum

ENTRYPOINT ["dotnet", "QuantumMC.dll"]
