using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using turno_smart.Data;
using turno_smart.Interfaces;
using turno_smart.Models;
using turno_smart.Services;

namespace turno_smart
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var builder = WebApplication.CreateBuilder(args); Inicializamos el builder dentro del tryy del logger

            // Configurar Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting up");

                var builder = WebApplication.CreateBuilder(args);

                // Configurar Serilog como el logger de la aplicación
                builder.Host.UseSerilog();

                // Add services to the container.
                string connectionString;

                // Prioridad de configuración:
                // 1. DATABASE_URL (Railway, Render, Heroku)
                // 2. Variables específicas de Docker
                // 3. Connection string del appsettings
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
                var dbName = Environment.GetEnvironmentVariable("DB_NAME");
                var dbPassword = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");
                
                Log.Information($"dbHost: {dbHost}, dbName: {dbName}, dbPassword: {dbPassword}");
                Log.Information($"Environment: {builder.Environment.EnvironmentName}");
                Log.Information($"DATABASE_URL variable: {databaseUrl ?? "NULL"}");

                if (!string.IsNullOrEmpty(databaseUrl))
                {
                    // Para Railway, Render, Heroku (PostgreSQL) - CONVERTIR URL
                    connectionString = ConvertPostgresUrlToConnectionString(databaseUrl);
                    Log.Information("Using DATABASE_URL for PostgreSQL connection - CONVERTED");
                }
                else if (builder.Environment.IsProduction())
                {
                    // FORCE: En Production, usar PostgreSQL con Railway
                    var railwayDbUrl = "postgresql://postgres:YwGewhgtdosfCLUDjogpTGSIGbGMisbW@postgres.railway.internal:5432/railway";
                    
                    // Convertir URL de PostgreSQL a connection string de Npgsql
                    connectionString = ConvertPostgresUrlToConnectionString(railwayDbUrl);
                    Log.Information("Using FORCED PostgreSQL connection for Railway Production");
                }
                else if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbName) && !string.IsNullOrEmpty(dbPassword))
                {
                    // Para Docker con SQL Server
                    connectionString = $"Data Source={dbHost};Initial Catalog={dbName};User ID=sa;Password={dbPassword};TrustServerCertificate=True;";
                    Log.Information("Using Docker SQL Server configuration");
                }
                else
                {
                    // Para desarrollo local - leer directamente de appsettings
                    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                    Log.Information("Using local development configuration");
                }
                
                Log.Information($"connectionString: {connectionString}");
                Log.Information($"Environment: {builder.Environment.EnvironmentName}");
                Log.Information($"Database URL exists: {!string.IsNullOrEmpty(databaseUrl)}");

                // Configurar el contexto de base de datos
                if ((!string.IsNullOrEmpty(databaseUrl) && databaseUrl.Contains("postgres")) || builder.Environment.IsProduction())
                {
                    // Usar PostgreSQL para servicios en la nube Y para Production
                    Log.Information("Configuring PostgreSQL database context");
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseLazyLoadingProxies().UseNpgsql(connectionString));
                }
                else
                {
                    // Usar SQL Server para desarrollo local
                    Log.Information("Configuring SQL Server database context");
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseLazyLoadingProxies().UseSqlServer(connectionString));
                }
                
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

                builder.Services.AddIdentity<Usuarios, IdentityRole>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Home/Index";
                    options.AccessDeniedPath = "/Home/Index"; // Ruta para acceso denegado
                });

                builder.Services.AddControllersWithViews();
                builder.Services.AddRazorPages();
                builder.Services.AddScoped<IPacienteService, PacienteService>();
                builder.Services.AddScoped<IEspecialidadService, EspecialidadService>();
                builder.Services.AddScoped<IHistorialMedicoService, HistorialMedicoService>();
                builder.Services.AddScoped<IEstudioService, EstudioService>();
                builder.Services.AddScoped<IMedicoService, MedicoService>();
                builder.Services.AddScoped<ITurnoService, TurnoService>();
                builder.Services.AddScoped<IRecepcionistaService, RecepcionistaService>(); 
                var app = builder.Build();

                // Ejecutar migraciones automáticamente
                using (var scope = app.Services.CreateScope())
                {
                    try
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        
                        Log.Information("Iniciando configuración de base de datos...");
                        Log.Information("Connection String: {ConnectionString}", connectionString.Substring(0, Math.Min(connectionString.Length, 50)) + "...");
                        
                        // Verificar conexión a la base de datos
                        var canConnect = await context.Database.CanConnectAsync();
                        Log.Information("¿Puede conectarse a la base de datos? {CanConnect}", canConnect);
                        
                        if (canConnect)
                        {
                            // Verificar si las tablas principales ya existen
                            bool tablesExist = false;
                            try
                            {
                                await context.Users.AnyAsync();
                                tablesExist = true;
                                Log.Information("Las tablas principales ya existen en la base de datos.");
                            }
                            catch
                            {
                                Log.Information("Las tablas principales no existen, necesitan crearse.");
                            }

                            if (tablesExist)
                            {
                                // Si las tablas existen, NO aplicar migraciones para evitar conflictos
                                Log.Information("Las tablas ya existen. Verificando compatibilidad...");
                                
                                // Para desarrollo local, simplemente continúa sin tocar las migraciones
                                if (builder.Environment.IsDevelopment())
                                {
                                    Log.Information("Entorno de desarrollo: manteniendo estructura existente de base de datos.");
                                }
                                else
                                {
                                    // Solo para producción (PostgreSQL), verificar migraciones
                                    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                                    Log.Information("Migraciones aplicadas: {Count}", appliedMigrations.Count());
                                    
                                    if (!appliedMigrations.Any())
                                    {
                                        // Marcar la migración como aplicada para PostgreSQL
                                        Log.Information("Sincronizando historial de migraciones PostgreSQL...");
                                        await context.Database.ExecuteSqlRawAsync(
                                            "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({0}, {1})",
                                            "20250803063000_InitialPostgreSQLCompatible", "8.0.8");
                                        Log.Information("Historial sincronizado para PostgreSQL.");
                                    }
                                    else
                                    {
                                        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                                        if (pendingMigrations.Any())
                                        {
                                            Log.Information("Aplicando migraciones pendientes en PostgreSQL...");
                                            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                                            await context.Database.MigrateAsync(cts.Token);
                                            Log.Information("Migraciones aplicadas exitosamente.");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Las tablas no existen, crear base de datos según el proveedor
                                if (connectionString.Contains("postgres") || connectionString.Contains("Host="))
                                {
                                    // PostgreSQL - usar migraciones
                                    Log.Information("Aplicando migraciones PostgreSQL para crear la base de datos...");
                                    await context.Database.MigrateAsync();
                                    Log.Information("Base de datos PostgreSQL creada exitosamente con migraciones.");
                                }
                                else
                                {
                                    // SQL Server - usar EnsureCreated para desarrollo
                                    Log.Information("Creando base de datos SQL Server para desarrollo local...");
                                    await context.Database.EnsureCreatedAsync();
                                    Log.Information("Base de datos SQL Server creada exitosamente.");
                                    
                                    // Marcar como migrada para consistencia
                                    await context.Database.ExecuteSqlRawAsync(
                                        "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ({0}, {1})",
                                        "20250803063000_InitialPostgreSQLCompatible", "8.0.8");
                                }
                            }
                        }
                        else
                        {
                            Log.Warning("No se puede conectar a la base de datos. Intentando crear con migraciones...");
                            await context.Database.MigrateAsync();
                            Log.Information("Base de datos creada y migraciones aplicadas exitosamente.");
                        }
                        
                        // Verificar que las tablas principales existan
                        try
                        {
                            var usersCount = await context.Users.CountAsync();
                            var pacientesCount = await context.Pacientes.CountAsync();
                            
                            Log.Information("Tabla Usuarios tiene {Count} registros", usersCount);
                            Log.Information("Tabla Pacientes tiene {Count} registros", pacientesCount);
                        }
                        catch (Exception tableEx)
                        {
                            Log.Warning(tableEx, "No se pudieron verificar las tablas. Posiblemente no existan aún.");
                        }
                        
                        Log.Information("Configuración de base de datos completada exitosamente.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error detallado al configurar base de datos");
                        Log.Error("Tipo de excepción: {ExceptionType}", ex.GetType().Name);
                        Log.Error("Mensaje: {Message}", ex.Message);
                        if (ex.InnerException != null)
                        {
                            Log.Error("Excepción interna: {InnerMessage}", ex.InnerException.Message);
                        }
                        throw; // Re-lanzar para que la aplicación falle rápido
                    }
                }

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                // Solo redirigir HTTPS en desarrollo local, no en Railway
                if (app.Environment.IsDevelopment())
                {
                    app.UseHttpsRedirection();
                }
                
                app.UseStaticFiles();

                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                app.MapRazorPages();

                using (var scope = app.Services.CreateScope())
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    string[] roles = new string[] { "Admin", "Medico", "Paciente", "Recepcionista" };
                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }
                }
                using (var scope = app.Services.CreateScope())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuarios>>();

                    List<TestUser> testUsers = new List<TestUser>()
                {
                    new TestUser()
                    {
                        Email  = "admin@turno-smart.com",
                        Password = "Admin123!",
                        Role = "Admin",
                        DNI = 12345678
                    },
                };

                    foreach (var testUser in testUsers)
                    {
                        if (await userManager.FindByEmailAsync(testUser.Email) == null)
                        {
                            var user = new Usuarios()
                            {
                                UserName = testUser.Email,
                                Email = testUser.Email,
                                DNI = 12345678,
                            };

                            await userManager.CreateAsync(user, testUser.Password);

                            await userManager.AddToRoleAsync(user, testUser.Role);
                        }
                    }
                }

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static string ConvertPostgresUrlToConnectionString(string databaseUrl)
        {
            // Convertir de postgresql://user:password@host:port/database
            // a Host=host;Port=port;Database=database;Username=user;Password=password
            try 
            {
                Log.Information($"Converting PostgreSQL URL: {databaseUrl?.Substring(0, Math.Min(databaseUrl.Length, 30))}...");
                
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':');
                var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
                
                Log.Information($"Converted connection string: Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password=***;SSL Mode=Require;Trust Server Certificate=true");
                
                return connectionString;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error converting PostgreSQL URL, using fallback");
                // Fallback manual para Railway
                var fallback = "Host=postgres.railway.internal;Port=5432;Database=railway;Username=postgres;Password=YwGewhgtdosfCLUDjogpTGSIGbGMisbW;SSL Mode=Require;Trust Server Certificate=true";
                Log.Information("Using fallback connection string");
                return fallback;
            }
        }
    }
    public class TestUser()
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int DNI { get; set; }
    }
}
