# 🚀 Guía de Despliegue - TurnoSmart

## ✅ Configuración Actual - Railway + PostgreSQL

**TurnoSmart** está deployado exitosamente en **Railway** con las siguientes características:

### 🎯 Stack Tecnológico de Producción
- **Frontend**: ASP.NET Core 8.0 MVC
- **Backend**: C# con Entity Framework Core
- **Base de Datos**: PostgreSQL (Railway)
- **Hosting**: Railway.app
- **SSL**: Habilitado automáticamente
- **Domain**: `turnosmart-production.up.railway.app`

### 🏗️ Arquitectura de Deploy

```
GitHub Repository → Railway → PostgreSQL
      ↓                ↓           ↓
   git push    →   Auto Deploy → Migrations
```

## 🚀 Railway - Configuración Actual

### ¿Por qué Railway?
- ✅ **Deploy automático** desde GitHub
- ✅ **PostgreSQL gratis** incluido
- ✅ **SSL/HTTPS automático**
- ✅ **Detección automática** de .NET
- ✅ **Logs en tiempo real**
- ✅ **Variables de entorno** automáticas

### Configuración Actual
```yaml
# railway.yml
version: 2
build:
  provider: dockerfile
deploy:
  healthcheckPath: /
  healthcheckTimeout: 100
  restartPolicyType: always
```

## 📊 Soluciones Implementadas

### 1. Compatibilidad PostgreSQL vs SQL Server
**Problema**: La app funcionaba en localhost (SQL Server) pero fallaba en Railway (PostgreSQL)

**Solución**:
```csharp
// ApplicationDbContext.cs - Converter automático UTC
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    foreach (var property in entityType.GetProperties())
    {
        if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
        {
            property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        }
    }
}
```

### 2. Problemas de Fechas UTC
**Cambios realizados**:
- `DateTime.Now` → `DateTime.UtcNow`
- `DateTime.Today` → `DateTime.UtcNow.Date`
- Converter automático en DbContext

### 3. Relaciones DNI-based
**Configuración correcta**:
```csharp
// Relación Usuario -> Paciente por DNI
modelBuilder.Entity<Paciente>()
    .HasOne(p => p.Usuario)
    .WithOne()
    .HasForeignKey<Paciente>(p => p.DNI)
    .HasPrincipalKey<Usuarios>(u => u.DNI)
    .OnDelete(DeleteBehavior.Restrict);
```
```

### 4. Dockerfile Optimizado
```dockerfile
# Dockerfile actual (funcional)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["turno-smart.csproj", "."]
RUN dotnet restore "./turno-smart.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "turno-smart.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "turno-smart.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "turno-smart.dll"]
```

## 🔄 Proceso de Deploy Actual

### 1. Desarrollo Local
```bash
# Desarrollo con SQL Server LocalDB
dotnet run
# → http://localhost:5031
```

### 2. Commit y Push
```bash
git add .
git commit -m "Feature: Nueva funcionalidad"
git push origin main
```

### 3. Deploy Automático
```
Railway detecta cambios → Build con Dockerfile → Deploy automático
```

### 4. Resultado
```
🌐 https://turnosmart-production.up.railway.app
📊 PostgreSQL en la nube
🔒 SSL automático
```

## 🗄️ Base de Datos - Dual Environment

### Desarrollo (Local)
```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-turno_smart;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Producción (Railway)
```csharp
// Program.cs - Detección automática
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(connectionString))
{
    // Railway PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Local SQL Server
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
```

## 🎯 Funcionalidades Desplegadas

### ✅ Sistema Completo Funcionando
- **👤 Autenticación**: Registro y login de pacientes
- **⚕️ Gestión de Médicos**: CRUD completo con especialidades
- **📅 Sistema de Turnos**: Creación, edición, confirmación
- **🏥 Especialidades**: Gestión de especialidades médicas
- **📋 Historial Médico**: Registro de consultas
- **� Roles**: Admin, Paciente, Médico, Recepcionista

### ✅ Características Técnicas
- **🔄 Migraciones automáticas** en Railway
- **🔗 Relaciones DNI-based** Usuario↔Paciente
- **🕐 Fechas UTC** compatibles con PostgreSQL
- **📱 Responsive design** con Bootstrap
- **🎨 Modales** para formularios
- **⚡ AJAX** para mejor UX

## 🛠️ Troubleshooting - Problemas Resueltos

### Error 1: DateTime UTC vs Local
```
❌ Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone'
✅ Solución: Converter automático + DateTime.UtcNow
```

### Error 2: NullReferenceException en Turnos
```
❌ currentUser.Paciente?.Id era null
✅ Solución: Usar PacienteService.GetByDNI(user.DNI)
```

### Error 3: Foreign Key Constraints
```
❌ Migración con estructura incorrecta de DNI
✅ Solución: Nueva migración PostgreSQL-compatible
```

### Error 4: Timestamp Comparison
```
❌ Cannot apply binary operation on 'timestamp with time zone' and 'timestamp without time zone'
✅ Solución: DateTime.UtcNow.Date en lugar de DateTime.Today
```

## 📈 Monitoreo y Logs

### Railway Dashboard
- **📊 Metrics**: CPU, Memory, Requests
- **📝 Logs**: Tiempo real
- **🔄 Deployments**: Historial completo
- **⚙️ Variables**: Environment settings

### Logs Importantes
```bash
# Startup exitoso
[INFO] Starting TurnoSmart
[INFO] Database connection established
[INFO] Migration applied: InitialPostgreSQLCompatible

# Registro de paciente exitoso
[INFO] Usuario de Identity creado exitosamente
[INFO] Paciente guardado exitosamente en la base de datos
```

## 🚀 Cómo Deployar Cambios

### 1. Desarrollo Local
```bash
# Probar localmente con SQL Server
dotnet run
# Verificar funcionalidad
```

### 2. Commit y Push
```bash
git add .
git commit -m "feat: nueva funcionalidad"
git push origin main
```

### 3. Verificar Deploy
```
Railway → Logs → Ver deployment
```

### 4. Probar en Producción
```
https://turnosmart-production.up.railway.app
```

## 📱 URLs y Accesos

- **🌐 Producción**: `https://turnosmart-production.up.railway.app`
- **🏠 Local**: `http://localhost:5031`
- **📊 Railway Dashboard**: `https://railway.app/project/[project-id]`
- **📁 GitHub**: `https://github.com/Camilo-Silva/TurnoSmart`

## 🎉 Estado Actual: ✅ FUNCIONANDO 100%

La aplicación **TurnoSmart** está completamente operativa en Railway con todas las funcionalidades:
- ✅ Registro y autenticación
- ✅ Gestión de turnos
- ✅ CRUD de médicos y especialidades
- ✅ Sistema de roles
- ✅ Base de datos PostgreSQL
- ✅ Deploy automático desde GitHub
