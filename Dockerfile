# Dockerfile optimizado para despliegue en la nube (Railway)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Configurar para usar el puerto de la variable de entorno (Railway, Render, etc.)
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivo de proyecto (ruta corregida para Railway)
COPY ["turno-smart.csproj", "./"]
RUN dotnet restore "turno-smart.csproj"

# Copiar el resto de los archivos
COPY . .
RUN dotnet build "turno-smart.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "turno-smart.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear directorio de logs
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "turno-smart.dll"]