using System.ComponentModel.DataAnnotations;
namespace AgroTili.Models
{
    public class Tipos_Tareas
    {
        [Required]
        [Key]
        public int id_tipo_tarea { get; set; }
        [Required]
        public string? nombre_tipo_tarea { get; set; }

    }
}