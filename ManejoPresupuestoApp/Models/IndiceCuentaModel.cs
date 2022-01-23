using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestoApp.Models
{
    public class IndiceCuentaModel
    {
        [Display(Name = "Tipo de Cuenta")]
        [DisplayName("Tipo de Cuentas")]
        public string TipoCuenta { get; set; }
        public IEnumerable<Cuenta> Cuentas { get; set; }

        public decimal Balance => Cuentas.Sum(c => c.Balance);
    }
}
