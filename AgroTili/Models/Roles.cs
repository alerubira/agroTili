using System.ComponentModel.DataAnnotations;
namespace AgroTili.Models
{
    public class Roles
    {
        [Required]
        [Key]
        public int id_role { get; set; }
        [Required]
        public string? nombre_role { get; set; }

    }
}