using ManejoPresupuestoApp.Validations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestoApp.Models
{
    public class Cuenta
    {
        public int Id { get; set; }

        [Required(ErrorMessage="El campo {0} es requerido")]
        [StringLength(maximumLength:50)]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }

        [Display(Name="Tipo de Cuenta")]
        public int TipoCuentaId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal Balance { get; set; }

        [StringLength(maximumLength: 1000)]
        public string Descripcion  { get; set; }

        [Display(Name = "Tipo de Cuenta")]
        public string TipoCuenta { get; set; }
    }
}
