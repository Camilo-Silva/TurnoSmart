# TurnoSmart - Sistema de Gestión de Turnos Médicos

## 📋 Descripción del Proyecto

**TurnoSmart** es un sistema integral de gestión de turnos médicos desarrollado con **ASP.NET Core 8.0 MVC**. La aplicación facilita la administración de citas médicas, permitiendo la gestión de pacientes, médicos, especialidades, turnos y historiales médicos de manera eficiente y centralizada.

## 🚀 Características Principales

### 🏥 Gestión Completa del Centro Médico
- **Administración de Pacientes**: Registro, edición y gestión completa de datos de pacientes
- **Gestión de Médicos**: Control de profesionales médicos con especialidades y matrículas
- **Especialidades Médicas**: Categorización y administración de especialidades
- **Sistema de Turnos**: Programación y gestión de citas médicas
- **Historiales Médicos**: Registro detallado de consultas y estudios
- **Estudios Médicos**: Gestión de estudios y análisis clínicos

### 👥 Sistema de Roles y Autenticación
- **Administrador**: Acceso completo al sistema
- **Médico**: Gestión de turnos y historiales de sus pacientes
- **Recepcionista**: Administración de turnos y datos de pacientes

### 🎨 Interfaz de Usuario Moderna
- **Diseño Responsivo**: Compatible con dispositivos móviles y desktop
- **Bootstrap 5**: Interfaz moderna y accesible
- **Modales Interactivos**: Formularios dinámicos para operaciones CRUD
- **Navegación Intuitiva**: Experiencia de usuario optimizada

## 🛠️ Stack Tecnológico

### Backend
- **Framework**: ASP.NET Core 8.0 MVC
- **Base de Datos**: SQL Server / LocalDB
- **ORM**: Entity Framework Core 8.0
- **Autenticación**: ASP.NET Core Identity
- **Logging**: Serilog

### Frontend
- **CSS Framework**: Bootstrap 5
- **JavaScript**: jQuery
- **Iconos**: Font Awesome / Bootstrap Icons
- **Patrón**: MVC con Razor Views

### Infraestructura
- **Contenedores**: Docker
- **Entorno**: .NET 8.0
- **Base de Datos**: SQL Server (LocalDB para desarrollo)

## 📦 Dependencias Principales

```xml
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="8.0.6" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.10" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Proxies" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.10" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
```

## 🗂️ Estructura del Proyecto

```
turno-smart/
├── Controllers/              # Controladores MVC
│   ├── AccountController.cs
│   ├── HomeController.cs
│   ├── MedicoController.cs
│   ├── PacienteController.cs
│   ├── TurnoController.cs
│   └── ...
├── Models/                   # Modelos de entidades
│   ├── Medicos.cs
│   ├── Pacientes.cs
│   ├── Turnos.cs
│   ├── Especialidades.cs
│   └── ...
├── ViewModels/              # Modelos de vista
│   ├── MedicoVM/
│   ├── PacienteVM/
│   ├── TurnoVM/
│   └── ...
├── Services/                # Servicios de negocio
│   ├── MedicoService.cs
│   ├── PacienteService.cs
│   └── ...
├── Data/                    # Contexto y migraciones
│   ├── ApplicationDbContext.cs
│   └── Migrations/
├── Views/                   # Vistas Razor
│   ├── Home/
│   ├── Medico/
│   ├── Paciente/
│   └── Shared/
└── wwwroot/                 # Archivos estáticos
    ├── css/
    ├── js/
    └── lib/
```

## 🚀 Instalación y Configuración

### Prerrequisitos
- **.NET 8.0 SDK** o superior
- **SQL Server** / **SQL Server LocalDB**
- **Visual Studio 2022** / **VS Code** (recomendado)
- **Git**

### 🔧 Configuración Local

1. **Clonar el repositorio**
```bash
git clone [URL_DEL_REPOSITORIO]
cd turno-smart
```

2. **Configurar la base de datos**
```bash
# Actualizar la base de datos con las migraciones
dotnet ef database update
```

3. **Configurar connection string** (si es necesario)
Editar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-turno_smart-382f0976-3bd1-415e-a9cb-d85b894228fd;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

4. **Ejecutar la aplicación**
```bash
dotnet run
```

5. **Acceder a la aplicación**
Abrir el navegador en: `https://localhost:7139` o `http://localhost:5139`

### 🐳 Configuración con Docker

1. **Construir la imagen**
```bash
docker build -t turno-smart .
```

2. **Ejecutar el contenedor**
```bash
docker run -p 8080:8080 -p 8081:8081 turno-smart
```

## 📊 Modelos de Datos Principales

### 👤 Paciente
```csharp
public class Paciente
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public int DNI { get; set; }
    public string Email { get; set; }
    // ... más propiedades
}
```

### 👨‍⚕️ Médico
```csharp
public class Medico
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public int DNI { get; set; }
    public int IdEspecialidad { get; set; }
    public int Matricula { get; set; }
    // ... más propiedades
}
```

### 📅 Turno
```csharp
public class Turno
{
    public int Id { get; set; }
    public int IdPaciente { get; set; }
    public int IdMedico { get; set; }
    public DateTime FechaTurno { get; set; }
    public TimeSpan HoraTurno { get; set; }
    // ... más propiedades
}
```

## 🔐 Sistema de Autenticación

### Roles Disponibles
- **Admin**: Acceso completo al sistema
- **Medico**: Gestión de consultas y pacientes asignados
- **Recepcionista**: Administración de turnos y datos básicos

### Configuración de Identity
```csharp
// Program.cs
builder.Services.AddDefaultIdentity<Usuarios>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();
```

## 🎯 Funcionalidades por Módulo

### 🏠 Dashboard Principal
- Vista general del centro médico
- Listado aleatorio de médicos destacados
- Navegación rápida a módulos principales

### 👥 Gestión de Pacientes
- ✅ Registro de nuevos pacientes
- ✅ Edición de datos personales
- ✅ Historial de turnos
- ✅ Gestión de historiales médicos

### 👨‍⚕️ Gestión de Médicos
- ✅ Registro con especialidad y matrícula
- ✅ Asignación de especialidades
- ✅ Gestión de horarios disponibles
- ✅ Visualización de turnos asignados

### 📅 Sistema de Turnos
- ✅ Programación de citas
- ✅ Validación de disponibilidad
- ✅ Notificaciones de estado
- ✅ Reprogramación de turnos

### 🏥 Especialidades
- ✅ Catálogo de especialidades médicas
- ✅ Asignación a médicos
- ✅ Filtrado por especialidad

## 🧪 Testing y Desarrollo

### Ejecutar en modo desarrollo
```bash
dotnet run --environment Development
```

### Logs del sistema
Los logs se almacenan en:
- **Consola**: Información general
- **Archivo**: `logs/log-YYYYMMDD.txt`

### Estructura de logs con Serilog
```csharp
// Program.cs
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
```

## 🚀 Despliegue

### Variables de Entorno para Producción
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="[CONNECTION_STRING_PRODUCCION]"
```

### Configuración Docker para Producción
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
```

## 🤝 Contribución

1. Fork el proyecto
2. Crear una rama para la feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit los cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Crear un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

## 📞 Contacto y Soporte

Para consultas o soporte técnico, crear un issue en el repositorio del proyecto.

---

**Desarrollado con ❤️ usando ASP.NET Core 8.0**
