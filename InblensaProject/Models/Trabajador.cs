using System.ComponentModel.DataAnnotations;

namespace InblensaProject.Models
{
    public class Trabajador
    {
        [Key]
        public int ID_Trabajador { get; set; }
        public string Nombre { get; set; }
        public string Cargo { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaContratacion { get; set; }


    }
}
