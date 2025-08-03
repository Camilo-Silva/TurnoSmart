# Guía de Despliegue - TurnoSmart

## 🚀 Opciones de Despliegue para ASP.NET Core

### 1. Azure App Service (Recomendado)

#### Configuración de Base de Datos
1. **Crear Azure SQL Database**
```bash
# Crear resource group
az group create --name turno-smart-rg --location "East US"

# Crear SQL Server
az sql server create \
  --name turno-smart-sql-server \
  --resource-group turno-smart-rg \
  --location "East US" \
  --admin-user tu_usuario \
  --admin-password tu_password_segura

# Crear base de datos
az sql db create \
  --resource-group turno-smart-rg \
  --server turno-smart-sql-server \
  --name turno-smart-db \
  --service-objective Basic
```

#### Connection String para Azure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:turno-smart-sql-server.database.windows.net,1433;Initial Catalog=turno-smart-db;Persist Security Info=False;User ID=tu_usuario;Password=tu_password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 2. Railway (Alternativa Gratuita)

1. **Conectar GitHub a Railway**
2. **Configurar variables de entorno:**
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `DATABASE_URL=postgresql://...` (Railway provee PostgreSQL gratis)

### 3. Render (Otra alternativa)

#### Configuración para Render
```dockerfile
# Dockerfile para Render
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["turno-smart.csproj", "."]
RUN dotnet restore "./turno-smart.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "turno-smart.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "turno-smart.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "turno-smart.dll"]
```

### 4. Heroku (Con contenedores)

#### Archivos necesarios:
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY bin/Release/net8.0/publish/ .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet turno-smart.dll
```

```json
// heroku.yml
build:
  docker:
    web: Dockerfile
```

## 📊 Comparación de Plataformas

| Plataforma | Costo | .NET Support | Database | Dificultad |
|------------|-------|--------------|----------|------------|
| Azure App Service | $$ | ✅ Excelente | Azure SQL | Fácil |
| Railway | $ (Gratis inicial) | ✅ Bueno | PostgreSQL | Fácil |
| Render | $ | ✅ Bueno | PostgreSQL | Medio |
| Heroku | $$ | ✅ Bueno | PostgreSQL | Medio |
| Netlify | ❌ | ❌ No compatible | ❌ | Imposible |

## 🗄️ Migración de Base de Datos

### Para PostgreSQL (Railway/Render)

1. **Instalar paquete PostgreSQL**
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

2. **Actualizar Program.cs**
```csharp
// Cambiar de SQL Server a PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

3. **Nueva migration para PostgreSQL**
```bash
dotnet ef migrations add InitialPostgreSQL -o Data/Migrations/PostgreSQL
dotnet ef database update
```

### Para Azure SQL

1. **Mantener configuración actual de SQL Server**
2. **Actualizar connection string en appsettings.Production.json**
3. **Ejecutar migraciones en Azure**

## 🔧 Configuración de Producción

### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${DATABASE_URL}" // Variable de entorno
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Program.cs - Configuración de producción
```csharp
// Configurar para usar variables de entorno
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // o UseNpgsql para PostgreSQL
```

## 📝 Pasos Siguientes

1. **Elegir plataforma de hosting**
2. **Configurar base de datos en la nube**
3. **Actualizar connection strings**
4. **Configurar CI/CD desde GitHub**
5. **Ejecutar migraciones en producción**

## 💡 Recomendación

Para un proyecto .NET Core como TurnoSmart, recomiendo **Azure App Service** porque:
- Soporte nativo para .NET
- Integración perfecta con Azure SQL
- Escalabilidad automática
- Herramientas de monitoreo incluidas
