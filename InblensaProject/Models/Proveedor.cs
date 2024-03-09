using System.ComponentModel.DataAnnotations;
namespace InblensaProject.Models
{
    public class Proveedor
    {
        [Key]
        public int ID_Proveedor { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
    }
}
