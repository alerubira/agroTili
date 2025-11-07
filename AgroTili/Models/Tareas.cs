using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AgroTili.Models
{
    public class Tareas
    {
        [Required]
        [Key]
        public int id_tarea { get; set; }
        [Required]
        [ForeignKey("Tipos_Tareas")]
        public int id_tipo_tarea { get; set; }
        [Required]
        [ForeignKey("Campos")]
        public int id_campo { get; set; }
        [Required]
        [ForeignKey("Maquinas_Agrarias")]
        public int id_maquina_agraria { get; set; }
        [Required]
        [ForeignKey("Empleados")]
        public int id_empleado { get; set; }
        [Required]
        public DateTime fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
        public bool realizada { get; set; }
        public String? observaciones { get; set; }
        // Propiedades de navegación opcionales
        public Tipos_Tareas? Tipos_Tareas { get; set; }
        public Campos? Campos { get; set; }
        public Maquinas_Agrarias? Maquinas_Agrarias { get; set; }
        public Empleados? Empleados { get; set; }
        

    }
}