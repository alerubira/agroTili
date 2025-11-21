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
    public class Maquinas_AgrariasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;


        public Maquinas_AgrariasController(DataContext context, IConfiguration config)
        {
            _context = context;//accede al dataContext
            _configuration = config;//accede a la configuracion de la aplicacion,appseting

        }
        [Authorize(Roles = "2")]
        [HttpGet("maquinasDesocupadasPorTarea")]
        public async Task<ActionResult<List<Maquinas_Agrarias>>> MaquinasDesocupadasPorTarea(int id_tipo_tarea)
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");
                 
                  var maquinas = await _context.Maquinas_Agrarias
                .Include(e => e.Tipos_Tareas)
                .Where(e => !e.ocupado && e.id_tipo_tarea == id_tipo_tarea&&e.activo)
                .ToListAsync();
                if (maquinas == null || maquinas.Count == 0)
                {
                    return NotFound("No hay Maquinas desocupadas disponibles");
                }
               
                return Ok(maquinas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       /* [Authorize(Roles = "2")]
        [HttpPut("ocuparMaquina")]
        public async Task<IActionResult> OcuparMaquina([FromBody] Maquinas_Agrarias datosActualizados)
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                //string usuario = User?.Identity?.Name ?? "";
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");

                // var res = await _context.Empleados.SingleOrDefaultAsync(x => x.email == usuario);
                var empleado = await _context.Empleados
                    .Include(e => e.Roles)
                    .FirstOrDefaultAsync(e => e.email == usuario&&e.activo);
                if (empleado == null)
                    return NotFound($"No se encontró el empleado con email {usuario}");

                var maquina = await _context.Maquinas_Agrarias
                    .FirstOrDefaultAsync(m => m.id_maquina_agraria == datosActualizados.id_maquina_agraria);
                if (maquina == null)
                    return NotFound($"No se encontró la máquina con ID {datosActualizados.id_maquina_agraria}");

                //  Actualizar los campos permitidos
                maquina.ocupado = true;
                //  Guardar cambios
                _context.Maquinas_Agrarias.Update(maquina);
                await _context.SaveChangesAsync();


                return NoContent(); // Devuelve 204, sin contenido;
            }
            catch (Exception ex)
            {
                return BadRequest("Error al actualizar: " + ex.Message);
            }
        }
          [Authorize(Roles = "2")]
         [HttpPut("desocuparMaquina")]
        public async Task<IActionResult> DesocuparMaquina([FromBody] Tareas datosActualizados)
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                //string usuario = User?.Identity?.Name ?? "";
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");

                // var res = await _context.Empleados.SingleOrDefaultAsync(x => x.email == usuario);
                var empleado = await _context.Empleados
                    .Include(e => e.Roles)
                    .FirstOrDefaultAsync(e => e.email == usuario);
                if (empleado == null)
                    return NotFound($"No se encontró el empleado con email {usuario}");
                // verificar que el emleado de la claim sea capataz de campo que trae la tarea
                //verificar que la maquina este asociada a la tarea
                //verificar en todos lados que el empleado exista
                var maquina =  await _context.Maquinas_Agrarias
                    .FirstOrDefaultAsync(m => m.id_maquina_agraria == datosActualizados.id_maquina_agraria);
                if (maquina == null)
                    return NotFound($"No se encontró la máquina con ID {datosActualizados.id_maquina_agraria}");

                //  Actualizar los campos permitidos
                maquina.ocupado =true;
                //  Guardar cambios
                _context.Maquinas_Agrarias.Update(maquina);
                await _context.SaveChangesAsync();

                
                return  NoContent(); // Devuelve 204, sin contenido;
            }
            catch (Exception ex)
            {
                return BadRequest("Error al actualizar: " + ex.Message);
            }
        }*/
    }
}
