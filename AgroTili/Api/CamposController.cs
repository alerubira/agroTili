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
    public class CamposController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;


        public CamposController(DataContext context, IConfiguration config)
        {
            _context = context;//accede al dataContext
            _configuration = config;//accede a la configuracion de la aplicacion,appseting

        }
        [Authorize(Roles = "2")]
        [HttpGet("camposPorCapataz")]
        public async Task<ActionResult<List<Campos>>> CamposPorCapataz(int id_empleado)
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");


                var campos = await _context.Campos
                //.Include(e => e.Tipos_Tareas)
                .Where(e => e.activo && e.id_empleado == id_empleado)
                .ToListAsync();
                if (campos == null || campos.Count == 0)
                {
                    return NotFound("No hay Campos disponibles para este Capataz");
                }

                return Ok(campos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}