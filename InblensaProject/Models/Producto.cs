using System.ComponentModel.DataAnnotations;

namespace InblensaProject.Models
{
    public class Producto
    {
        [Key]
        public int ID_Producto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
    }
}
