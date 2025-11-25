using AgroTili.Models;
namespace AgroTili.Utils
{
    public static class EmpleadoMapper
    {
        public static object MapearEmpleadoDto(Empleados res)
        {
            if (res == null) return null!;

            return new
            {
                id_empleado = res.id_empleado,
                id_role = res.id_role,
                apellido = res.apellido ?? string.Empty,
                nombre = res.nombre ?? string.Empty,
                email = res.email ?? string.Empty,
                ocupado = res.ocupado,
                fecha_ingreso = res.fecha_ingreso.ToString("dd-MM-yyyy"),
                fecha_egreso = res.fecha_egreso?.ToString("dd-MM-yyyy"),
                activo = res.activo,
                nombre_role = res.Roles?.nombre_role,
                imagen_perfil=res.imagen_perfil
            };
        }
    }
}
