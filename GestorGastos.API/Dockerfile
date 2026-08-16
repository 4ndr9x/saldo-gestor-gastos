FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GestorGastos.API/GestorGastos.API.csproj", "GestorGastos.API/"]
COPY ["GestorGastos.Data/GestorGastos.Data.csproj", "GestorGastos.Data/"]
COPY ["GestorGastos.Domain/GestorGastos.Domain.csproj", "GestorGastos.Domain/"]
COPY ["GestorGastos.Services/GestorGastos.Services.csproj", "GestorGastos.Services/"]
RUN dotnet restore "GestorGastos.API/GestorGastos.API.csproj"
COPY . .
WORKDIR "/src/GestorGastos.API"
RUN dotnet build "./GestorGastos.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GestorGastos.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GestorGastos.API.dll"]
