using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuestoApp.Models
{
    public class CuentaViewModel : Cuenta
    {
        public IEnumerable<SelectListItem> TiposCuentas  { get; set; }
    }
}
