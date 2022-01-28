using Resources;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestoApp.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required"), 
         DataType(DataType.Date)]
        [Display(Name = "Fecha de Transacción")]
        public DateTime FechaTransaccion { get; set; } = DateTime.Now;
        //DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

        [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Required"),
         DataType(DataType.Currency, ErrorMessage = "No es una cantidad correcta")]
        [Range(0, Double.MaxValue, ErrorMessageResourceName = "Range", 
            ErrorMessageResourceType = typeof(SharedResource))]
        [RegularExpression(@"^\d+([.|,]\d+)?$", ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "Number")]
        public decimal Monto { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Debe seleccionar una Cuenta")]
        [Display(Name = "Cuenta")]
        public int CuentaId { get; set; }

        [Range(0,int.MaxValue,ErrorMessage ="Debe seleccionar una categoría")]
        [Display(Name = "Categoria")]
        public int CategoriaId { get; set; }

        [StringLength(maximumLength:100,ErrorMessage = "La nota no puede superar los {1} caracteres")]
        public string Nota { get; set; }

        [Display(Name = "Tipo de Operación")]
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingreso;

        public string Categoria { get; set; }
        public string Cuenta { get; set; }
    }
}
