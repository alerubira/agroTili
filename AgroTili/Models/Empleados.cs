
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroTili.Models
{
    public class Empleados
    {
        [Required]
        [Key]
        public int id_empleado { get; set; }
        [Required]
       [ForeignKey("Roles")]
        public int id_role { get; set; }
         [Required]
        public string? apellido { get; set; }
         [Required]
        public string? nombre { get; set; }
        [Required]
        public string? email { get; set; }
         [Required]
        public string? clave { get; set; }
         [Required]
        public bool ocupado { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime fecha_ingreso { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
         public DateTime? fecha_egreso { get; set; }
        [Required]
        public bool activo { get; set; }
        public String? clave_provisoria{ get; set; }
         // Propiedad de navegación opcional
        public Roles? Roles { get; set; }

}
}


