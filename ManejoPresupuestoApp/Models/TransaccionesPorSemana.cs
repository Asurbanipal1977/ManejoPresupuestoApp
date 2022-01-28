namespace ManejoPresupuestoApp.Models
{
    public class TransaccionesPorSemana
    {
        public int Semana { get; set; }
        public decimal Monto { get; set; }
        public TipoOperacion TipoOperacionId { get; set; }

    }
}
