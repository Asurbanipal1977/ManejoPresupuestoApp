namespace ManejoPresupuestoApp.Models
{
    public class TransaccionesPorSemana
    {
        public int Semana { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Monto { get; set; }
        public TipoOperacion TipoOperacionId { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
    }
}
