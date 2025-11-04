using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AgroTili.Models
{
    public class Campos
    {
        [Required]
        [Key]
        public int id_campo { get; set; }
        [Required]
        public string? nombre_campo { get; set; }
        [ForeignKey("Empleados")]
        public int id_empleado{ get; set; }
       
        [Required]
        public decimal superficie { get; set; }
        public decimal latitud { get; set; }
        public decimal longitud { get; set; }
        [Required]
        public bool activo { get; set; }
        
    }
    }
    