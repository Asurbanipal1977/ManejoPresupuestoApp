using AutoMapper;
using ManejoPresupuestoApp.Models;
using ManejoPresupuestoApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public TransaccionesController(IRepositoriosCategorias repositoriosCategorias,
                                    IRepositoriosCuentas repositoriosCuentas,
                                    IRepositoriosTransacciones repositoriosTransacciones,
                                    IServiciosUsuarios serviciosUsuarios,
                                    IServiciosReporte serviciosReporte,
                                    IMapper mapper)
        {
            _repositoriosCategorias = repositoriosCategorias;
            _repositoriosCuentas = repositoriosCuentas;
            _repositoriosTransacciones = repositoriosTransacciones;
            _serviciosUsuarios = serviciosUsuarios;
            _mapper = mapper;
            _serviciosReporte = serviciosReporte;
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

            IEnumerable<TransaccionesPorSemana> transaccionesPorSemana = await _serviciosReporte.ObtenerReporteSemanal(usuarioId,mes,año, ViewBag);
            return View();
        }

        public IActionResult Mensual()
        {
            return View();
        }

        public IActionResult Excel()
        {
            return View();
        }

        public IActionResult Calendario()
        {
            return View();
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
