# 🚀 TurnoSmart - Configuración Post-Deploy

## ✅ Deploy Exitoso - Siguientes Pasos

### 1. 🗄️ Configurar Base de Datos PostgreSQL

#### En Railway Dashboard:
1. Ir a tu proyecto en Railway
2. Hacer clic en **"+ New Service"**
3. Seleccionar **"Database"** → **"Add PostgreSQL"**
4. Railway creará automáticamente:
   - Una instancia de PostgreSQL
   - Variable `DATABASE_URL` con la conexión

### 2. 🔧 Verificar Variables de Entorno

En **Project → Variables**, verificar que tienes:
```
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=postgresql://username:password@host:port/database
PORT=8080
```

### 3. 🔄 Ejecutar Migraciones

#### Opción A: Desde Railway CLI (Recomendado)
```bash
# Instalar Railway CLI
npm install -g @railway/cli

# Login a Railway
railway login

# Conectar al proyecto
railway link

# Ejecutar migraciones
railway run dotnet ef database update
```

#### Opción B: Configurar Auto-Migraciones
Ya configuramos tu `Program.cs` para ejecutar migraciones automáticamente en startup.

### 4. 👤 Crear Usuario Administrador

Una vez que la app esté funcionando:

1. **Ir a tu URL de Railway**: `https://tu-app.railway.app`
2. **Registrar el primer usuario** (será Admin automáticamente)
3. **O configurar usuarios por defecto** (ver sección siguiente)

### 5. 🛠️ Configurar Datos Iniciales (Opcional)

#### Crear un Seeder para datos de prueba:

```csharp
// En Program.cs, después de var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Ejecutar seed
    await SeedData.Initialize(context, userManager, roleManager);
}
```

### 6. 🔍 Verificar Funcionamiento

#### Checklist de Verificación:
- [ ] La aplicación carga sin errores
- [ ] Se puede registrar/login usuarios
- [ ] La base de datos se conecta correctamente
- [ ] Los modales de médicos funcionan
- [ ] Se pueden crear turnos
- [ ] Las migraciones se ejecutaron

### 7. 🔧 Configuración Opcional

#### A. Custom Domain (Si tienes)
```bash
# En Railway
railway domain add tu-dominio.com
```

#### B. Configurar HTTPS (Automático en Railway)
Railway proporciona HTTPS automáticamente.

#### C. Monitoreo y Logs
```bash
# Ver logs en tiempo real
railway logs
```

### 8. 🐛 Troubleshooting Común

#### Si la aplicación no carga:
1. Verificar logs: `railway logs`
2. Verificar variables de entorno
3. Verificar que PostgreSQL esté conectado

#### Si hay errores de base de datos:
1. Verificar `DATABASE_URL`
2. Ejecutar migraciones manualmente
3. Verificar que Npgsql esté instalado

#### Si los modales no funcionan:
1. Verificar que los archivos JS se cargan
2. Verificar errores en consola del navegador

## 🎯 URL de Tu Aplicación

Tu aplicación debería estar disponible en:
`https://[tu-proyecto].railway.app`

## 📞 Próximos Pasos

1. **Testear todas las funcionalidades**
2. **Crear datos de prueba** (especialidades, médicos, etc.)
3. **Configurar usuarios administradores**
4. **Documentar credenciales de acceso**

¡Tu aplicación TurnoSmart está lista para producción! 🎉
