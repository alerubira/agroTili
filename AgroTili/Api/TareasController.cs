using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgroTili.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using AgroTili.Utils;


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
        [HttpGet]
        public async Task<ActionResult<List<Tareas>>> Get()
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");
                var capataz = await _context.Empleados
                       .FirstOrDefaultAsync(p => p.email == usuario && p.activo);
                if (capataz == null)
                    return Unauthorized("El Usuario no existe");       

                  var tareas = await _context.Tareas
            .Include(t => t.Campos)
            .Include(t => t.Tipos_Tareas)
            .Include(t => t.Empleados)
            .Include(t => t.Maquinas_Agrarias)
            .Where(t => t.Campos!.id_empleado == capataz.id_empleado)
            .Select(t => new
            {
                t.id_tarea,
                id_tipo_tarea = t.id_tipo_tarea,
                id_campo = t.id_campo,
                id_maquina_agraria = t.id_maquina_agraria,
                id_empleado = t.id_empleado,
                fecha_inicio = t.fecha_inicio.ToString("dd-MM-yyyy"),
                fecha_fin = t.fecha_fin.HasValue ? t.fecha_fin.Value.ToString("dd-MM-yyyy") : null,
                t.realizada,
                t.observaciones,
                Tipo_Tarea = t.Tipos_Tareas!,//como esta declarado posible null en el modelo, pero en la consulta siempre va a traer valor
                Campo = t.Campos!,
                Maquina_Agraria = t.Maquinas_Agrarias!,
                Empleado = EmpleadoMapper.MapearEmpleadoDto(t.Empleados!)

            })
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
                       .FirstOrDefaultAsync(p => p.email == Usuario && p.activo);

                if (capataz == null)
                    return Unauthorized("El Usuario no existe");

                var tipoTarea = await _context.Tipos_Tareas
                       .FirstOrDefaultAsync(t => t.id_tipo_tarea == idTipoTarea);
                if (tipoTarea == null)
                    return BadRequest("El Tipo de Tarea no existe");

                var campo = await _context.Campos
                       .FirstOrDefaultAsync(c => c.id_campo == IdCampo && c.activo);
                if (campo == null)
                    return BadRequest("El Campo no existe");

                var maquinaAgraria = await _context.Maquinas_Agrarias
                       .FirstOrDefaultAsync(m => m.id_maquina_agraria == idMaquinaAgraria && m.activo && !m.ocupado);
                if (maquinaAgraria == null)
                    return BadRequest("La Maquina Agraria no esta disponible");
                if (maquinaAgraria.id_tipo_tarea != idTipoTarea)
                    return BadRequest("La Maquina Agraria no es apta para este tipo de tarea");

                var operario = await _context.Empleados
                       .FirstOrDefaultAsync(e => e.id_empleado == idEmpleado && e.activo && !e.ocupado);
                if (operario == null)
                    return BadRequest("El Empleado no esta disponible");

                if (campo.id_empleado != capataz.id_empleado)
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
           [Authorize(Roles = "2")]
          [HttpPut("finalizarTarea")]
        
        public async Task<IActionResult> FinalizarTarea([FromForm] int idTipoTarea,
         [FromForm] string observaciones)
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

                var tarea = await _context.Tareas
                       .FirstOrDefaultAsync(t => t.id_tipo_tarea == idTipoTarea && !t.realizada);
                if (tarea == null)
                    return BadRequest("La Tarea no existe o ya fue finalizada");           

                var operario = await _context.Empleados
                       .FirstOrDefaultAsync(e => e.id_empleado == tarea.id_empleado);
                if (operario == null)
                    return BadRequest("El Empleado node la tarea no existe");

                var maquinaAgraria = await _context.Maquinas_Agrarias
                       .FirstOrDefaultAsync(m => m.id_maquina_agraria == tarea.id_maquina_agraria);
                if (maquinaAgraria == null)
                    return BadRequest("La Maquina Agraria de la tarea no existe");            

                var campo = await _context.Campos
                       .FirstOrDefaultAsync(c => c.id_campo == tarea.id_campo && c.activo); 
                if (campo == null)
                    return BadRequest("El Campo de la tarea no existe");

                if (campo.id_empleado != capataz.id_empleado)
                    return Unauthorized("No tiene permisos para finalizar la tarea  en este campo");  
                      
                 //Actualizar estados relacionados
                tarea.fecha_fin = DateTime.Now;
                tarea.realizada = true;
                tarea.observaciones = observaciones;
                maquinaAgraria.ocupado = false;
                operario.ocupado = false;
                     _context.Tareas.Update(tarea);
                    _context.Maquinas_Agrarias.Update(maquinaAgraria);
                    _context.Empleados.Update(operario);

                    // se guardan todos los cambios
                    await _context.SaveChangesAsync();

                    // se confirma la transacción
                    await transaction.CommitAsync();

                return Ok("Tarea Finalizada con exito");
            }
            catch (Exception ex)
            {
                return BadRequest("desde api: " + ex.Message);
            }
        }
    }
}