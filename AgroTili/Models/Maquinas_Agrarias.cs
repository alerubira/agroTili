using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AgroTili.Models
{
    public class Maquinas_Agrarias
    {
        [Key]
        public int id_maquina_agraria { get; set; }
        [ForeignKey("Tipos_Tareas")]
        public int id_tipo_tarea { get; set; }
        public string? patente { get; set; }
        public bool ocupado { get; set; }
        public bool activo { get; set; }
        // Propiedad de navegación opcional
        public Tipos_Tareas? Tipos_Tareas { get; set; }
    }
}
    