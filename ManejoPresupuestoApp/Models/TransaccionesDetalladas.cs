namespace ManejoPresupuestoApp.Models
{
    public class TransaccionesDetalladas
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public IEnumerable<TransaccionesPorFecha> TransaccionesAgrupadas { get; set; }
        public decimal BalanceDepositos => TransaccionesAgrupadas.Sum(t => t.BalanceDepositos);
        public decimal BalanceGastos => TransaccionesAgrupadas.Sum(t => t.BalanceGastos);

        public decimal Total => BalanceDepositos - BalanceGastos;

        public class TransaccionesPorFecha
        {
            public DateTime FechaTransaccion { get; set; }

            public IEnumerable<Transaccion> Transacciones { get; set; }

            public decimal BalanceDepositos => Transacciones.Where(t => t.TipoOperacionId == TipoOperacion.Ingreso).Sum(t => t.Monto);
            public decimal BalanceGastos => Transacciones.Where(t => t.TipoOperacionId == TipoOperacion.Gasto).Sum(t => t.Monto);

        }
    }
}
