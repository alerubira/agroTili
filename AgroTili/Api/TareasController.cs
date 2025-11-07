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
    public class TareasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;


        public TareasController(DataContext context, IConfiguration config)
        {
            _context = context;//accede al dataContext
            _configuration = config;//accede a la configuracion de la aplicacion,appseting

        }
        [Authorize(Roles = "2")]
        [HttpGet("tareasPorCapataz")]
        public async Task<ActionResult<List<Campos>>> TareasPorCapataz(int id_empleado)
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");


                var tareas = await _context.Tareas
                //.Include(e => e.Tipos_Tareas)
                .Where(e => e.id_empleado == id_empleado)
                .ToListAsync();
                if (tareas == null || tareas.Count == 0)
                {
                    return NotFound("No hay Tareas disponibles para este Capataz");
                }

                return Ok(tareas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
          [Authorize(Roles = "2")]
          [HttpPost("crearTarea")]
        
        public async Task<IActionResult> CrearTarea([FromForm] int idTipoTarea,
         [FromForm] int IdCampo, [FromForm] int idMaquinaAgraria, [FromForm] int idEmpleado)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                string Usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(Usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");    
                var capataz = await _context.Empleados
                       .FirstOrDefaultAsync(p => p.email == Usuario&&p.activo);

                if (capataz == null)
                    return Unauthorized("El Usuario no existe");

                var tipoTarea = await _context.Tipos_Tareas
                       .FirstOrDefaultAsync(t => t.id_tipo_tarea == idTipoTarea);
                if (tipoTarea == null)
                    return BadRequest("El Tipo de Tarea no existe");

                var campo = await _context.Campos
                       .FirstOrDefaultAsync(c => c.id_campo == IdCampo&&c.activo);
                if (campo == null)
                    return BadRequest("El Campo no existe");

                var maquinaAgraria = await _context.Maquinas_Agrarias
                       .FirstOrDefaultAsync(m => m.id_maquina_agraria == idMaquinaAgraria&&m.activo);
                if (maquinaAgraria == null)
                    return BadRequest("La Maquina Agraria no existe");

                var operario = await _context.Empleados
                       .FirstOrDefaultAsync(e => e.id_empleado == idEmpleado && e.activo);
                if (operario == null)
                    return BadRequest("El Empleado no existe");           

                if(campo.id_empleado!=capataz.id_empleado)
                    return Unauthorized("No tiene permisos para asignar tareas en este campo");    
                //  creación de la tarea
        var nuevaTarea = new Tareas
        {
            id_tipo_tarea = idTipoTarea,
            id_campo = IdCampo,
            id_maquina_agraria = idMaquinaAgraria,
            id_empleado = idEmpleado,
            fecha_inicio = DateTime.Now,
            realizada = false
        };

        _context.Tareas.Add(nuevaTarea);

        //Actualizar estados relacionados
        maquinaAgraria.ocupado = true;
        operario.ocupado = true;
        _context.Maquinas_Agrarias.Update(maquinaAgraria);
        _context.Empleados.Update(operario);

        // se guardan todos los cambios
        await _context.SaveChangesAsync();

        // se confirma la transacción
        await transaction.CommitAsync();

                return Ok("Tarea Generada con exito");
            }
            catch (Exception ex)
            {
                return BadRequest("desde api: " + ex.Message);
            }
        }
    }
}