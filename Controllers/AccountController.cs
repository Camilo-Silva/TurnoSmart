using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using turno_smart.Models;
using turno_smart.ViewModels.AccountVM;
using turno_smart.Interfaces;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using turno_smart.ViewModels.PacienteVM;

namespace turno_smart.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<Usuarios> _signInManager;
        private readonly UserManager<Usuarios> _userManager;
        private readonly IPacienteService _pacienteService;
        public AccountController(ILogger<HomeController> logger, SignInManager<Usuarios> signInManager, UserManager<Usuarios> userManager, IPacienteService pacienteService)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _pacienteService = pacienteService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            var loginVM = new LoginVM();
            return PartialView("_LoginModal", loginVM);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return Json(new { redirectUrl = Url.Action("Index", "Home") });
                }
                ModelState.AddModelError("", "Email or password is incorrect.");
            }
            
            return PartialView("_LoginModal", model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var registerVM = new RegisterVM();
            return PartialView("_RegistrationModal", registerVM);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Iniciando registro de paciente con email: {Email}", model.Email);
                    
                    int dni;
                    if (!int.TryParse(model.DNI, out dni))
                    {
                        _logger.LogWarning("DNI inválido ingresado: {DNI}", model.DNI);
                        ModelState.AddModelError("DNI", "DNI ingresado invalido");
                        return PartialView("_RegistrationModal", model);
                    }
                    if (!model.AceptoTerminos)
                    {
                        _logger.LogWarning("Usuario no aceptó términos y condiciones");
                        ModelState.AddModelError("AceptoTerminos", "Debes aceptar los terminos para registrarte.");
                        return PartialView("_RegistrationModal", model);
                    }
                    
                    // Verificar si el email ya existe
                    var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUserByEmail != null)
                    {
                        _logger.LogWarning("Intento de registro con email ya existente: {Email}", model.Email);
                        ModelState.AddModelError("Email", "Ya existe un usuario registrado con este email.");
                        return PartialView("_RegistrationModal", model);
                    }
                    
                    // Verificar si el DNI ya existe
                    var existingUserByDNI = await _userManager.Users.FirstOrDefaultAsync(u => u.DNI == dni);
                    if (existingUserByDNI != null)
                    {
                        // Verificar si tiene un paciente asociado
                        var existingPaciente = _pacienteService.GetByDNI(dni);
                        if (existingPaciente == null)
                        {
                            _logger.LogWarning("Usuario huérfano detectado con DNI: {DNI} (sin paciente asociado). Eliminando usuario huérfano.", dni);
                            // Eliminar usuario huérfano para permitir re-registro
                            await _userManager.DeleteAsync(existingUserByDNI);
                            _logger.LogInformation("Usuario huérfano eliminado. Procediendo con nuevo registro.");
                        }
                        else
                        {
                            _logger.LogWarning("Intento de registro con DNI ya existente: {DNI} para usuario existente: {ExistingEmail}", dni, existingUserByDNI.Email);
                            ModelState.AddModelError("DNI", "Ya existe un usuario registrado con este DNI.");
                            return PartialView("_RegistrationModal", model);
                        }
                    }
                    
                    _logger.LogInformation("DNI {DNI} verificado como disponible, procediendo con la creación", dni);
                    
                    _logger.LogInformation("Creando usuario de Identity para DNI: {DNI}", dni);
                    Usuarios users = new Usuarios
                    {
                        Email = model.Email,
                        UserName = model.Email,
                        DNI = dni,
                    };

                    var result = await _userManager.CreateAsync(users, model.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Usuario de Identity creado exitosamente, agregando rol Paciente");
                        await _userManager.AddToRoleAsync(users, "Paciente");
                        
                        _logger.LogInformation("Verificando si paciente ya existe con DNI: {DNI}", users.DNI);
                        Paciente paciente = _pacienteService.GetByDNI(users.DNI);
                        if (paciente == null)
                        {
                            _logger.LogInformation("Paciente no existe, creando nuevo registro de paciente");
                            paciente = new Paciente()
                            {
                                Nombre = model.Nombre,
                                Apellido = model.Apellido,
                                DNI = dni,
                                FechaNacimiento = model.FechaNacimiento,
                                Email = model.Email,
                                FechaAlta = DateTime.Now,
                                Usuario = users,
                                Ciudad = "",
                                Provincia = "",
                                Domicilio = "",
                                Cobertura = 0,
                                Telefono = 0,
                                Estado = 1, // 1 = Activo/Habilitado, 0 = Inactivo/Deshabilitado
                            };
                            
                            // Establecer la relación bidireccional
                            users.Paciente = paciente;
                        }
                        else
                        {
                            _logger.LogInformation("Paciente ya existe con DNI: {DNI}", users.DNI);
                            // Establecer la relación con el paciente existente
                            users.Paciente = paciente;
                            paciente.Usuario = users;
                        }
                        
                        _logger.LogInformation("Guardando paciente en base de datos");
                        try 
                        {
                            _pacienteService.Create(paciente);
                            _logger.LogInformation("Paciente guardado exitosamente en la base de datos");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "ERROR: No se pudo guardar el paciente en la base de datos");
                            _logger.LogError("Detalles del error: {Message}", ex.Message);
                            if (ex.InnerException != null)
                            {
                                _logger.LogError("Error interno: {InnerMessage}", ex.InnerException.Message);
                            }
                            throw; // Re-lanzar la excepción para que se vea el error
                        }
                        
                        // Actualizar el usuario con la relación establecida
                        try 
                        {
                            await _userManager.UpdateAsync(users);
                            _logger.LogInformation("Usuario actualizado exitosamente");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "ERROR: No se pudo actualizar el usuario");
                        }
                        
                        _logger.LogInformation("Registro de paciente completado exitosamente para email: {Email}", model.Email);
                        return PartialView("_RegistrationSuccess", model);
                    }
                    else
                    {
                        _logger.LogWarning("Error al crear usuario de Identity. Errores: {Errors}", 
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                        
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        return PartialView("_RegistrationModal", model);
                    }
                }
                catch (Exception ex)
                {
                    // Log del error para debugging
                    _logger.LogError(ex, "Error al procesar el registro de paciente: {Message}", ex.Message);
                    
                    // Manejo específico de errores de PostgreSQL
                    string errorMessage = "Error al procesar la solicitud.";
                    
                    if (ex.Message.Contains("duplicate key value violates unique constraint") && ex.Message.Contains("DNI"))
                    {
                        errorMessage = "El DNI ingresado ya está registrado en el sistema.";
                        _logger.LogWarning("Intento de registro con DNI duplicado detectado en catch: {DNI}", model.DNI);
                    }
                    else if (ex.Message.Contains("foreign key constraint"))
                    {
                        errorMessage = "Error de base de datos: problema con relación de datos.";
                        _logger.LogError("Error de foreign key constraint detectado: {Error}", ex.Message);
                    }
                    else
                    {
                        errorMessage = "Error al procesar la solicitud: " + ex.Message;
                    }
                    
                    ModelState.AddModelError("", errorMessage);
                    return PartialView("_RegistrationModal", model);
                }
            }
            
            _logger.LogWarning("ModelState no es válido para registro de paciente. Errores: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            
            return PartialView("_RegistrationModal", model);
        }

        [HttpGet]
        public IActionResult Details(string? ActiveTab = "general")
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                _logger.LogWarning("Usuario no autenticado intentó acceder a Details");
                return RedirectToAction("Index", "Home");
            }
            
            var paciente = _pacienteService.GetByDNI(user.DNI);
            if (paciente == null)
            {
                _logger.LogError("No se encontró paciente con DNI: {DNI} para usuario: {Email}", user.DNI, user.Email);
                TempData["ErrorMessage"] = "No se encontró información de paciente. Contacte al administrador.";
                return RedirectToAction("Index", "Home");
            }

            var pacienteVM = new ProfileVM
            {
                Id = paciente.Id,
                Nombre = paciente.Nombre,
                Apellido = paciente.Apellido,
                FechaNacimiento = paciente.FechaNacimiento,
                DNI = paciente.DNI,
                Domicilio = paciente.Domicilio,
                Ciudad = paciente.Ciudad,
                Provincia = paciente.Provincia,
                Telefono = paciente.Telefono,
                Email = paciente.Email,
                ActiveTab = ActiveTab,
            };

            return View(pacienteVM);
        }

        public async Task<IActionResult> UpdatePassword(string password)
        {
            try
            {

                if(string.IsNullOrEmpty(password)) {
                    TempData["ErrorMessage"] = "Por favor, agregue una contraseña";
                    return RedirectToAction("Details");
                }

                var user = await _userManager.GetUserAsync(User);
                if(user == null) {
                    return NotFound();
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, password);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al actualizar la contraseña.";
                }

                return RedirectToAction("Details");
            }
            catch
            {
                TempData["ErrorMessage"] = "Error al actualizar la contraseña.";
                return RedirectToAction("Details");
            }
        }

        [HttpPost]
        public IActionResult UpdateProfile(ProfileVM model)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ActiveTab");
            if (ModelState.IsValid)
            {
                try
                {
                    var paciente = _pacienteService.GetById(model.Id);
                    paciente.Nombre = model.Nombre;
                    paciente.Apellido = model.Apellido;
                    paciente.FechaNacimiento = model.FechaNacimiento;
                    paciente.DNI = model.DNI;
                    paciente.Domicilio = model.Domicilio;
                    paciente.Ciudad = model.Ciudad;
                    paciente.Provincia = model.Provincia;
                    paciente.Telefono = model.Telefono;
                    paciente.Email = model.Email;

                    _pacienteService.Update(paciente);

                    TempData["SuccessMessage"] = "Datos actualizados correctamente.";
                    return RedirectToAction("Details");
                } catch (Exception ex) {
                    TempData["ErrorMessage"] = "Error al actualizar los datos." + ex.Message;
                    return RedirectToAction("Details");
                }
            } 
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
