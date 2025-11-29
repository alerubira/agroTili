using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgroTili.Models;
using AgroTili.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using AgroTili.Utils;
namespace AgroTili.Api
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly SeguridadService _seguridadService;
        private readonly EmailService _emailService;
         private readonly IWebHostEnvironment _environment; 

        public LoginController(DataContext context, IConfiguration config, SeguridadService seguridadService,
         EmailService emailService,IWebHostEnvironment environment)
        {
            _context = context;//accede al dataContext
            _configuration = config;//accede a la configuracion de la aplicacion,appseting y claves en .env
            _seguridadService = seguridadService;//accede al servicio de seguridad para generar y validar tokens
            _emailService = emailService;
            _environment = environment;//para acceder a rutas del servidor
            
        }
         [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] string Usuario, [FromForm] string Clave)
        {
            try
            {
                if (string.IsNullOrEmpty(Usuario) || string.IsNullOrEmpty(Clave))
                    return BadRequest("Usuario y clave son requeridos");
                var empleado = await _context.Empleados
                       .FirstOrDefaultAsync(p => p.email == Usuario&&p.activo);

                if (empleado == null)
                    return Unauthorized("El Usuario no existe");
               
                if (!_seguridadService.VerificarContraseña(Clave, empleado.clave))
                    return Unauthorized("Usuario o clave incorrectos");
                

                var secretKey = _configuration["TokenAuthentication:SecretKey"];
                var issuer = _configuration["TokenAuthentication:Issuer"];
                var audience = _configuration["TokenAuthentication:Audience"];
              

                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey ?? ""));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, empleado.email),
                    new Claim("id_empleado", empleado.id_empleado.ToString()),
                    new Claim("id_role", empleado.id_role.ToString()),
                    new Claim(ClaimTypes.Role, empleado.id_role.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(28),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(tokenString);
            }
            catch (Exception ex)
            {
                return BadRequest("desde api: " + ex.Message);
            }
        }
         [HttpPut("cambiarClave")]
        public async Task<IActionResult> CambiarClave([FromForm] string claveActual, [FromForm] string claveNueva)
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");

                var empleado = await _context.Empleados
                    .Include(e => e.Roles)
                    .FirstOrDefaultAsync(e => e.email == usuario&&e.activo);
                if (empleado == null)
                    return NotFound($"No se encontró el empleado con email {usuario}");

                // Verificar contraseña actual
               
                if (!_seguridadService.VerificarContraseña(claveActual, empleado.clave))
                    return Unauthorized("La contraseña actual es incorrecta");

                // Actualizar a la nueva contraseña
                empleado.clave = _seguridadService.HashearContraseña(claveNueva).Trim();
                _context.Empleados.Update(empleado);
                await _context.SaveChangesAsync();

                return NoContent(); // Devuelve 204, sin contenido
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
         [HttpPost("enviarMail")]
        [AllowAnonymous]
        public async Task<IActionResult> EnviarMail([FromForm] string Usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(Usuario))
                    return BadRequest("Usuario es requerido");
                var empleado = await _context.Empleados
                       .FirstOrDefaultAsync(p => p.email == Usuario&&p.activo);

                if (empleado == null)
                    return NotFound("El Usuario no existe");
                
                int n = numeroAleatorio();
                empleado.clave_provisoria = _seguridadService.HashearContraseña(n.ToString());
                _context.Empleados.Update(empleado);  
                await _context.SaveChangesAsync();
  
                await _emailService.EnviarCorreoAsync(
                    empleado.email,
                    "Recuperación de cuenta",
                    $"Hola {empleado.nombre},  desde AgroTili.Este estu numero para recuperar tu cuenta: {n}"
                );
                return Ok("Correo electrónico enviado correctamente");
            }
            catch (Exception ex)
            {
                return BadRequest("Error al enviar correo: " + ex.Message);
            }
        }
        [HttpPut("recuperarClave")]
         [AllowAnonymous]
        public async Task<IActionResult> RecuperarClave([FromForm] string email,[FromForm] string claveEmail, [FromForm] string claveNueva)
        {
            try
            {

                var empleado = await _context.Empleados
                    .Include(e => e.Roles)
                    .FirstOrDefaultAsync(e => e.email == email&&e.activo);
                if (empleado == null)
                    return NotFound($"No se encontró el empleado con email {email}");

                
                if (!_seguridadService.VerificarContraseña(claveEmail, empleado.clave_provisoria ?? ""))
                    return Unauthorized("La contraseña del mail es incorrecta");

                // Actualizar a la nueva contraseña
                empleado.clave = _seguridadService.HashearContraseña(claveNueva).Trim();
                empleado.clave_provisoria = null;
                _context.Empleados.Update(empleado);
                await _context.SaveChangesAsync();

                return NoContent(); // Devuelve 204, sin contenido
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
         private static readonly Random rnd = new Random();

                private int numeroAleatorio()
                {
                    return rnd.Next(1000, 10000);
                }
    }
}