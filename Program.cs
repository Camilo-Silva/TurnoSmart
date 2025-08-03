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
                    // Para Railway, Render, Heroku (PostgreSQL)
                    connectionString = databaseUrl;
                    Log.Information("Using DATABASE_URL for PostgreSQL connection");
                }
                else if (builder.Environment.IsProduction())
                {
                    // FORCE: En Production, usar PostgreSQL con Railway
                    var railwayDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                                     "postgresql://postgres:YwGewhgtdosfCLUDjogpTGSIGbGMisbW@postgres.railway.internal:5432/railway";
                    
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

                // Ejecutar migraciones automáticamente en producción
                using (var scope = app.Services.CreateScope())
                {
                    try
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        
                        // Aplicar migraciones pendientes automáticamente
                        Log.Information("Verificando y aplicando migraciones de base de datos...");
                        await context.Database.MigrateAsync();
                        Log.Information("Migraciones aplicadas exitosamente.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error al aplicar migraciones de base de datos");
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
                var uri = new Uri(databaseUrl);
                var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
                return connectionString;
            }
            catch
            {
                // Fallback manual para Railway
                return "Host=postgres.railway.internal;Port=5432;Database=railway;Username=postgres;Password=YwGewhgtdosfCLUDjogpTGSIGbGMisbW;SSL Mode=Require;Trust Server Certificate=true";
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
