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
namespace AgroTili.Api
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class EmpleadosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly SeguridadService _seguridadService;

        public EmpleadosController(DataContext context, IConfiguration config, SeguridadService seguridadService)
        {
            _context = context;//accede al dataContext
            _configuration = config;//accede a la configuracion de la aplicacion,appseting y claves en .env
            _seguridadService = seguridadService;//accede al servicio de seguridad para generar y validar tokens
        }
        [HttpGet]
        public async Task<ActionResult<Empleados>> Get()
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                //string usuario = User?.Identity?.Name ?? "";
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del usuario");

                var res = await _context.Empleados.SingleOrDefaultAsync(x => x.email == usuario);
                if (res == null)
                    return NotFound($"No se encontró el empleado con email {usuario}");
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
                       .FirstOrDefaultAsync(p => p.email == Usuario);

                if (empleado == null)
                    return Unauthorized("El Usuario no existe");
                 if (string.IsNullOrEmpty(empleado.clave))
                    return Unauthorized("Error en los datos del usuario");    

                if(!_seguridadService.VerificarContraseña(Clave,empleado.clave))
                    return Unauthorized("Usuario o clave incorrectos");
               /* var hashed = _seguridadService.HashearContraseña(Clave).Trim();
                var claveGuardada = empleado.clave.Trim();

                if (hashed != claveGuardada)
                    return Unauthorized("Usuario o clave incorrectos");*/

                var secretKey = _configuration["TokenAuthentication:SecretKey"];
                var issuer = _configuration["TokenAuthentication:Issuer"];
                var audience = _configuration["TokenAuthentication:Audience"];
                // Validar que la clave no sea nula o vacía
                if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
                    return StatusCode(500, "Configuración de token incompleta");

                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                if (string.IsNullOrWhiteSpace(empleado.email))
                   return StatusCode(500, "Datos del usuario incompletos: email no disponible");

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, empleado.email),
                    new Claim("id_empleado", empleado.id_empleado.ToString()),
                    new Claim("id_role", empleado.id_role.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.Now.AddHours(5),
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
    }
}