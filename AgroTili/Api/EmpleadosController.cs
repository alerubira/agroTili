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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;



//using System.Net;
//using System.Net.Mail;

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
        private readonly EmailService _emailService;
         private readonly IWebHostEnvironment _environment; 

        public EmpleadosController(DataContext context, IConfiguration config, SeguridadService seguridadService,
         EmailService emailService,IWebHostEnvironment environment)
        {
            _context = context;//accede al dataContext
            _configuration = config;//accede a la configuracion de la aplicacion,appseting y claves en .env
            _seguridadService = seguridadService;//accede al servicio de seguridad para generar y validar tokens
            _emailService = emailService;
            _environment = environment;//para acceder a rutas del servidor
            
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
                    return BadRequest("No se pudo obtener el email del Empleado");

                var res = await _context.Empleados
                    .Include(e => e.Roles)
                    .FirstOrDefaultAsync(e => e.email == usuario&&e.activo);
                if (res == null)
                    return NotFound($"No se encontró el empleado con email {usuario}");
                
                return Ok(EmpleadoMapper.MapearEmpleadoDto(res));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       
        [HttpPut("actualizar")]
        public async Task<ActionResult<Empleados>> Actualizar([FromBody] EmpleadoUpdateDto datosActualizados)
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

                var idClaim = User?.Claims?.FirstOrDefault(c => c.Type == "id_empleado")?.Value ?? "";
                if (datosActualizados.id_empleado.ToString() != idClaim)
                    return Unauthorized("No tienes permiso para actualizar este Empleadoo.");

                //  Actualizar los campos permitidos
                empleado.nombre = datosActualizados.nombre;
                empleado.apellido = datosActualizados.apellido;
                //  Guardar cambios
                _context.Empleados.Update(empleado);
                await _context.SaveChangesAsync();

                // Devolver el empleado actualizado
                return Ok(EmpleadoMapper.MapearEmpleadoDto(empleado));
            }
            catch (Exception ex)
            {
                return BadRequest("Error al actualizar: " + ex.Message);
            }
        }
       
        [Authorize(Roles = "2")]
        [HttpGet("operariosDesocupados")]
        public async Task<ActionResult<List<Empleados>>> OperariosDesocupados()
        {
            try
            {
                if (User == null)
                    return Unauthorized("Usuario no autenticado");
                string usuario = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest("No se pudo obtener el email del Empleado");


                var operarios = await _context.Empleados
                .Include(e => e.Roles)
                .Where(e => !e.ocupado && e.id_role == 3&&e.activo)
                .ToListAsync();
                if (operarios == null || operarios.Count == 0)
                {
                    return NotFound("No hay operarios desocupados disponibles");
                }
                // Mapear cada empleado a tu DTO anónimo
                var listaDto = operarios.Select(e => EmpleadoMapper.MapearEmpleadoDto(e)).ToList();
                return Ok(listaDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       
     [HttpPut("modificarImagen")]
        public async Task<ActionResult<Empleados>> ModificarImagen(
    [FromForm] IFormFile imagen) 
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
                  //  insertar imagen del usuario
                //  Guardar la imagen en wwwroot/Uploads
                string wwwPath = _environment.WebRootPath;
                string uploadPath = Path.Combine(wwwPath, "Uploads");
                string fileName;
                string filePath;
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);
                 if (imagen == null || imagen.Length == 0 || !imagen.ContentType.StartsWith("image/"))
                {
                    // Nombre único de la imagen: "imagen_perfil_<Id>.ext"
                         fileName = $"imagen_perfil_0.png";
                         filePath = Path.Combine(uploadPath, fileName);
                }
                else
                {
                    // Nombre único de la imagen: "imagen_perfil_<Id>.ext"
                    fileName = $"imagen_perfil_{empleado.id_empleado}{Path.GetExtension(imagen.FileName)}";
                    filePath = Path.Combine(uploadPath, fileName);
                       using (var image = await Image.LoadAsync<Rgba32>(imagen.OpenReadStream()))
                        {
                            var exif = image.Metadata.ExifProfile;
                            ushort orientation = 1;

                            if (exif != null && exif.TryGetValue(ExifTag.Orientation, out IExifValue<ushort> orientationValue))
                            {
                                orientation = orientationValue.Value;
                            }

                              switch (orientation)
                            {
                                case 3:
                                    image.Mutate(x => x.Rotate(180));
                                    break;
                                case 6:
                                    image.Mutate(x => x.Rotate(90));
                                    break;
                                case 8:
                                    image.Mutate(x => x.Rotate(270));
                                    break;
                            }

                         if (orientation == 1)
                            {
                                bool esVertical = image.Height > image.Width;

                                // Foto vertical: NO rotar nunca (ya viene bien la de galería)
                                if (!esVertical) // o sea: es horizontal
                                {
                                    // Si la imagen está acostada pero debería ser vertical (muy raro)
                                    // rotamos solo en ese caso
                                    if (image.Width > image.Height && image.Height < image.Width / 1.5)
                                    {
                                        image.Mutate(x => x.Rotate(90));
                                    }
                                }
                            }

                               await image.SaveAsync(filePath);
                        }

                 }     
                
                empleado.imagen_perfil = Path.Combine("/Uploads", fileName);    
                 //  Guardar cambios
                _context.Empleados.Update(empleado);
                await _context.SaveChangesAsync();

                // Devolver el empleado actualizado
                return Ok(EmpleadoMapper.MapearEmpleadoDto(empleado));
            }
            catch (Exception ex)
            {
                return BadRequest("Error al actualizar La Imagen de perfil: " + ex.Message);
            }

        }
        
     }
}