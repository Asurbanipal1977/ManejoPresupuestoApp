using ManejoPresupuestoApp.Models;

namespace ManejoPresupuestoApp.Services
{
    public interface IServiciosReporte
    {
        Task<IEnumerable<TransaccionesPorSemana>> ObtenerReporteSemanal(int usuarioId, int mes, int año, dynamic viewBag);
        Task<TransaccionesDetalladas> ObtenerTransaccionesDetalladasPorCuenta(int usuarioId, int? cuentaId, int mes, int año, dynamic viewBag);
    }
    public class ServiciosReporte : IServiciosReporte
    {
        private readonly IRepositoriosTransacciones _repositoriosTransacciones;
        private readonly HttpContext _httpContext;

        public ServiciosReporte (IRepositoriosTransacciones repositoriosTransacciones, 
                        IHttpContextAccessor httpContextAccessor)
        {
            _repositoriosTransacciones = repositoriosTransacciones;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<TransaccionesDetalladas> 
            ObtenerTransaccionesDetalladasPorCuenta(int usuarioId, int? cuentaId, int mes, int año, dynamic viewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = ObtenerFechas(mes, año);

            IEnumerable<Transaccion> transacciones;
            if (cuentaId != null)
            {
                transacciones = await _repositoriosTransacciones.ObtenerPorCuentaId(new TransaccionesPorCuentaModel()
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    UsuarioId = usuarioId,
                    CuentaId = cuentaId ?? 0
                });
            }
            else
            {
                transacciones = await _repositoriosTransacciones.ObtenerPorUsuarioId(new TransaccionesPorUsuarioModel()
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    UsuarioId = usuarioId
                });
            }

            var modelo = new TransaccionesDetalladas();

            var transaccionesPorFecha = transacciones.OrderByDescending(t => t.FechaTransaccion)
                .GroupBy(t => t.FechaTransaccion).Select(grupo => new TransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;

            viewBag.MesAnterior = fechaInicio.AddMonths(-1).Month;
            viewBag.AñoAnterior = fechaInicio.AddMonths(-1).Year;
            viewBag.MesPosterior = fechaInicio.AddMonths(1).Month;
            viewBag.AñoPosterior = fechaInicio.AddMonths(1).Year;
            viewBag.urlRetorno = _httpContext.Request.Path + _httpContext.Request.QueryString;

            return modelo;
        }


        public async Task<IEnumerable<TransaccionesPorSemana>> 
            ObtenerReporteSemanal(int usuarioId, int mes, int año, dynamic viewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = ObtenerFechas(mes, año);

            var parametro = new TransaccionesPorUsuarioModel()
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                UsuarioId = usuarioId
            };

            viewBag.MesAnterior = fechaInicio.AddMonths(-1).Month;
            viewBag.AñoAnterior = fechaInicio.AddMonths(-1).Year;
            viewBag.MesPosterior = fechaInicio.AddMonths(1).Month;
            viewBag.AñoPosterior = fechaInicio.AddMonths(1).Year;
            viewBag.urlRetorno = _httpContext.Request.Path + _httpContext.Request.QueryString;

            var modelo = await _repositoriosTransacciones.ObtenerPorSemana(parametro);

            return modelo;
        }


        private (DateTime fechaInicio, DateTime fechaFin) ObtenerFechas(int mes, int año)
        {
            DateTime fechaInicio, fechaFin;

            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
                fechaInicio = new DateTime(año, mes, 1);

            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio,fechaFin);
        }
    }
}
