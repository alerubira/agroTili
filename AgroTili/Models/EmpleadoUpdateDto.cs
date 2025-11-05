namespace AgroTili.Models
{
    public class EmpleadoUpdateDto
    {
        public int id_empleado { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string apellido { get; set; } = string.Empty;
        public bool activo { get; set; }
        public bool ocupado { get; set; }
        // solo incluir lod datos necesarios para actualizar
    }
}
