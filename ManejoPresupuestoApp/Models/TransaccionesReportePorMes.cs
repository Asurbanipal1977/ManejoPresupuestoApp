namespace ManejoPresupuestoApp.Models
{
    public class TransaccionesReportePorMes
    {
        public decimal Ingresos => TransaccionesTotalesPorMes.Sum(x => x.Ingresos);
        public decimal Gastos => TransaccionesTotalesPorMes.Sum(x => x.Gastos);
        public decimal Total => Ingresos - Gastos;

        public int Año { get; set; }
        public IEnumerable<TransaccionesPorMes> TransaccionesTotalesPorMes { get; set; }
    }
}
