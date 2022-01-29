namespace ManejoPresupuestoApp.Models
{
    public class TransaccionesReportePorSemana
    {
        public decimal Ingresos => TransaccionesTotalesPorSemana.Sum(x => x.Ingresos);
        public decimal Gastos => TransaccionesTotalesPorSemana.Sum(x => x.Gastos);
        public decimal Total => Ingresos - Gastos;

        public DateTime FechaReferencia { get; set; }
        public IEnumerable<TransaccionesPorSemana> TransaccionesTotalesPorSemana { get; set; }
    }
}
