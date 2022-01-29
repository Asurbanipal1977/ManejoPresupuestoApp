namespace ManejoPresupuestoApp.Models
{
    public class TransaccionesPorMes
    {
        public int Mes { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaReferencia { get; set; }
        public TipoOperacion TipoOperacionId { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
    }
}
