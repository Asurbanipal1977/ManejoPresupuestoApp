using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuestoApp.Models;
using ManejoPresupuestoApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Linq;

namespace ManejoPresupuestoApp.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IRepositoriosCategorias _repositoriosCategorias;
        private readonly IRepositoriosCuentas _repositoriosCuentas;
        private readonly IRepositoriosTransacciones _repositoriosTransacciones;
        private readonly IServiciosUsuarios _serviciosUsuarios;
        private readonly IMapper _mapper;
        private readonly IServiciosReporte _serviciosReporte;
        private readonly IXLWorkbook _workBook;

        public TransaccionesController(IRepositoriosCategorias repositoriosCategorias,
                                    IRepositoriosCuentas repositoriosCuentas,
                                    IRepositoriosTransacciones repositoriosTransacciones,
                                    IServiciosUsuarios serviciosUsuarios,
                                    IServiciosReporte serviciosReporte,
                                    IMapper mapper,
                                    IXLWorkbook workBook)
        {
            _repositoriosCategorias = repositoriosCategorias;
            _repositoriosCuentas = repositoriosCuentas;
            _repositoriosTransacciones = repositoriosTransacciones;
            _serviciosUsuarios = serviciosUsuarios;
            _mapper = mapper;
            _serviciosReporte = serviciosReporte;
            _workBook = workBook;
        }

        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();

            var modelo = await _serviciosReporte.ObtenerTransaccionesDetalladasPorCuenta(usuarioId, null, mes, año, ViewBag);

            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes, int año)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();

            IEnumerable<TransaccionesPorSemana> transaccionesPorSemana = 
                await _serviciosReporte.ObtenerReporteSemanal(usuarioId,mes,año, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana)
                .Select(x => new TransaccionesPorSemana()
                {
                    Semana = x.Key,
                    Ingresos = x.Where(t=>t.TipoOperacionId==TipoOperacion.Ingreso).Select(t=>t.Monto).FirstOrDefault(),
                    Gastos = x.Where(t => t.TipoOperacionId == TipoOperacion.Gasto).Select(t => t.Monto).FirstOrDefault(),

                }).ToList();

            if (año==0 || mes==0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(año,mes,1);
            var diasdelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);
            var diasSegmentados = diasdelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count; i++)
            {
                var semana = i+1;
                var fechaInicio = new DateTime(año,mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());

                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);
                if (grupoSemana == null)
                {
                    agrupado.Add(new TransaccionesPorSemana
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }              
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new TransaccionesReportePorSemana()
            {
                TransaccionesTotalesPorSemana = agrupado,
                FechaReferencia = fechaReferencia,
            };

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();

            if (año == 0)
                año = DateTime.Today.Year;

            var transaccionesMes = await _repositoriosTransacciones.ObtenerPorMes(usuarioId, año);
            var agrupado = transaccionesMes.GroupBy(x=>x.Mes)
                .Select(x=>new TransaccionesPorMes()
                {
                    Mes = x.Key,
                    Ingresos= x.Where(t => t.TipoOperacionId == TipoOperacion.Ingreso).Select(t => t.Monto).FirstOrDefault(),
                    Gastos = x.Where(t => t.TipoOperacionId == TipoOperacion.Gasto).Select(t => t.Monto).FirstOrDefault()
                }).ToList();

            var mesesAño = Enumerable.Range(1, 12).ToList();
            for (int mes = 1; mes <= mesesAño.Count; mes++)
            {
                var agrupadoMes = agrupado.Where(x => x.Mes == mes).FirstOrDefault();
                var fechaReferencia = new DateTime(año, mes, 1);
                if (agrupadoMes==null)
                {
                    agrupado.Add(new TransaccionesPorMes()
                    {
                        FechaReferencia = fechaReferencia,
                        Mes = mes
                    });
                }
                else
                {
                    agrupadoMes.FechaReferencia = fechaReferencia;
                }

            }

            agrupado = agrupado.OrderByDescending(x => x.Mes).ToList();

            var modelo = new TransaccionesReportePorMes()
            {
                TransaccionesTotalesPorMes = agrupado,
                Año = año
            };

            return View(modelo);
        }

        public IActionResult Excel()
        {

            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int año)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();

            var transacciones = await _repositoriosTransacciones
                .ObtenerPorUsuarioId(new TransaccionesPorUsuarioModel()
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Presupuesto - {fechaInicio.ToString("MMyyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAño(int año)
        {
            var fechaInicio = new DateTime(año, 1, 1);
            var fechaFin = new DateTime(año, 12, 31);
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();

            var transacciones = await _repositoriosTransacciones
                .ObtenerPorUsuarioId(new TransaccionesPorUsuarioModel()
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelTotal()
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();

            var transacciones = await _repositoriosTransacciones
                .ObtenerPorUsuarioId(new TransaccionesPorUsuarioModel()
                {
                    UsuarioId = usuarioId,
                    FechaInicio = DateTime.Today.AddYears(-100),
                    FechaFin= DateTime.Today.AddYears(1000)
                });

            var nombreArchivo = $"Presupuesto Total.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            });

            foreach (var t in transacciones)
            {
                dataTable.Rows.Add(t.FechaTransaccion, t.Cuenta, t.Categoria, t.Nota,
                    t.Monto * ((t.TipoOperacionId == TipoOperacion.Gasto) ? -1 : 1), t.TipoOperacionId.ToString());
            }

            dataTable.Rows.Add(null, null, null, null, transacciones.Sum(x=>x.Monto * (x.TipoOperacionId==TipoOperacion.Gasto ? -1: 1)));

            _workBook.Worksheets.Add(dataTable);
            //Ajustas la anchura de las columnas
            _workBook.Worksheets.First().Columns().AdjustToContents();


            using (MemoryStream stream = new MemoryStream())
            {
                _workBook.SaveAs(stream);
                _workBook.Dispose();
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    nombreArchivo);                
            }      
        }


        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();

            var transacciones = await _repositoriosTransacciones
               .ObtenerPorUsuarioId(new TransaccionesPorUsuarioModel()
               {
                   UsuarioId = usuarioId,
                   FechaInicio = start,
                   FechaFin = end
               });

            var json = transacciones.Select(x => new EventoCalendario()
            {
                Title = x.Monto.ToString("N"),
                Start = x.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = x.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = (x.TipoOperacionId == TipoOperacion.Gasto) ? "red" : null
            });

            return Json(json);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var transacciones = await _repositoriosTransacciones
               .ObtenerPorUsuarioId(new TransaccionesPorUsuarioModel()
               {
                   UsuarioId = usuarioId,
                   FechaInicio = fecha,
                   FechaFin = fecha
               });

            return Json(transacciones);
        }

        public IActionResult Cancelar(string urlRetorno = null)
        {
            if (string.IsNullOrEmpty(urlRetorno))
                return RedirectToAction("Index");
            else
                return LocalRedirect(urlRetorno);
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionModel();
            modelo.UsuarioId = usuarioId;
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

            return View(modelo);
        }

       

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionModel modelo)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            modelo.UsuarioId=usuarioId;
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await _repositoriosCuentas.ConsultarPorId(modelo.CuentaId, usuarioId);
            if (cuenta == null)
                return RedirectToAction("NoEncontrado","Home");

            var categoria = await _repositoriosCategorias.ConsultarPorId(modelo.CategoriaId, usuarioId);
            if (categoria == null)
                return RedirectToAction("NoEncontrado", "Home");

            modelo.Monto = (modelo.TipoOperacionId == TipoOperacion.Gasto) ? modelo.Monto * -1 : modelo.Monto;

            await _repositoriosTransacciones.Crear(modelo);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Modificar(int id, string urlRetorno=null)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var transaccion = await _repositoriosTransacciones.ConsultarPorId(id, usuarioId);
            if (transaccion == null)
                return RedirectToAction("NoEncontrado", "Home");

            var modelo = _mapper.Map<TransaccionModificacionModel>(transaccion);
            modelo.UsuarioId = usuarioId;
            modelo.MontoAnterior = transaccion.Monto;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
                modelo.MontoAnterior = modelo.Monto * -1;
            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            modelo.UrlRetorno = urlRetorno;


            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Modificar(TransaccionModificacionModel modelo)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            modelo.UsuarioId = usuarioId;
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await _repositoriosCuentas.ConsultarPorId(modelo.CuentaId, usuarioId);
            if (cuenta == null)
                return RedirectToAction("NoEncontrado", "Home");

            var categoria = await _repositoriosCategorias.ConsultarPorId(modelo.CategoriaId, usuarioId);
            if (categoria == null)
                return RedirectToAction("NoEncontrado", "Home");

            modelo.Monto = (modelo.TipoOperacionId == TipoOperacion.Gasto) ? modelo.Monto * -1 : modelo.Monto;

            await _repositoriosTransacciones.Modificar(modelo, modelo.MontoAnterior, modelo.CuentaAnteriorId);


            if (string.IsNullOrEmpty(modelo.UrlRetorno))
                return RedirectToAction("Index");
            else
                return LocalRedirect(modelo.UrlRetorno);
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var transaccion = await _repositoriosTransacciones.ConsultarPorId(id, usuarioId);
            if (transaccion == null)
                return RedirectToAction("NoEncontrado", "Home");

            await _repositoriosTransacciones.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
                return RedirectToAction("Index");
            else
                return LocalRedirect(urlRetorno);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias (int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await _repositoriosCategorias.ListadoCategorias(usuarioId);
            return categorias.Where(x => x.TipoOperacionId == tipoOperacion)
                            .Select(x=>new SelectListItem(x.Nombre,x.Id.ToString()));
        }

        public async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await _repositoriosCuentas.ListadoCuentas(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }

    }   
}
