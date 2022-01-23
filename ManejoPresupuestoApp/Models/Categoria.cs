using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestoApp.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage = "El campo no puede ser mayor de {1} caracteres")]
        public string Nombre { get; set; }

        [Display(Name = "Tipo de Operación")]
        [DisplayName("Tipo de Operación Id")]
        public TipoOperacion TipoOperacionId { get; set; }
        public int UsuarioId { get; set; }
    }
}
