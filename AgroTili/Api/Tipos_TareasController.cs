using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgroTili.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;


namespace AgroTili.Api
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class Tipos_TareasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;


        public Tipos_TareasController(DataContext context, IConfiguration config)
        {
            _context = context;//accede al dataContext
            _configuration = config;//accede a la configuracion de la aplicacion,appseting

        }
        [Authorize(Roles = "2")]
        [HttpGet]
        public async Task<ActionResult<List<Tipos_Tareas>>> Get()
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");


                var tiposTareas = await _context.Tipos_Tareas
                .ToListAsync();
                if (tiposTareas == null || tiposTareas.Count == 0)
                {
                    return NotFound("No hay Tipos de Tareas disponibles");
                }
               
                return Ok(tiposTareas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}