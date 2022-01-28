namespace ManejoPresupuestoApp.Models
{
    public class TransaccionModificacionModel : TransaccionCreacionModel
    {
        public int CuentaAnteriorId { get; set; }
        public decimal MontoAnterior { get; set; }

        public string UrlRetorno { get; set; }
    }
}
