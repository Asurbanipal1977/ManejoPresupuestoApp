using ManejoPresupuestoApp.Models;
using ManejoPresupuestoApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AutoMapper;

namespace ManejoPresupuestoApp.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositoriosTiposCuentas _repositoriosTiposCuentas;
        private readonly IRepositoriosCuentas _repositoriosCuentas;
        private readonly IRepositoriosTransacciones _repositoriosTransacciones;
        private readonly IServiciosUsuarios _serviciosUsuarios;
        private readonly IServiciosReporte _serviciosReporte;

        private readonly IMapper _mapper;

        public CuentasController (IRepositoriosTiposCuentas repositoriosTiposCuentas,
                                    IRepositoriosCuentas repositoriosCuentas,
                                    IServiciosUsuarios serviciosUsuarios,
                                    IRepositoriosTransacciones repositoriosTransacciones,
                                    IServiciosReporte serviciosReporte,
                                    IMapper mapper)
        {
            _repositoriosTiposCuentas = repositoriosTiposCuentas;
            _repositoriosCuentas = repositoriosCuentas;
            _serviciosUsuarios = serviciosUsuarios;
            _repositoriosTransacciones = repositoriosTransacciones;
            _mapper = mapper;
            _serviciosReporte = serviciosReporte;
        }


        public async Task<IActionResult> Index()
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var lstCuentas = await _repositoriosCuentas.ListadoCuentas(usuarioId);

            var modelo = lstCuentas.GroupBy(x => x.TipoCuenta).Select(x => new IndiceCuentaModel
            {
                TipoCuenta = x.Key,
                Cuentas = x.AsEnumerable()
            });         

            return View(modelo);
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            CuentaViewModel cuenta = new CuentaViewModel();
            var tipoCuenta = await _repositoriosTiposCuentas.ConsultarPorId(cuenta.TipoCuentaId, usuarioId);
            if (tipoCuenta == null)
                RedirectToAction("NoEncontrado", "Home");

            cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaViewModel cuenta)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await _repositoriosTiposCuentas.ConsultarPorId(cuenta.TipoCuentaId, usuarioId);
            if (tipoCuenta == null)
                RedirectToAction("NoEncontrado","Home");

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await _repositoriosCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }

        public async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await _repositoriosTiposCuentas.ListadoTiposCuentas(usuarioId);            
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }


        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            Cuenta cuenta = await _repositoriosCuentas.ConsultarPorId(id, usuarioId);
            if (cuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tiposCuentas = await ObtenerTiposCuentas(usuarioId);
            var cuentaModel = _mapper.Map<CuentaViewModel>(cuenta);
            cuentaModel.TiposCuentas = tiposCuentas;

            return View(cuentaModel);
        }

        [HttpPost]
        public async Task<IActionResult> Modificar(CuentaViewModel cuenta)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await _repositoriosTiposCuentas.ConsultarPorId(cuenta.TipoCuentaId, usuarioId);
            if (tipoCuenta == null)

                RedirectToAction("NoEncontrado", "Home");

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await _repositoriosCuentas.Modificar(cuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var cuenta = await _repositoriosCuentas.ConsultarPorId(id, usuarioId);
            if (cuenta == null)
                return RedirectToAction("NoEncontrado", "Home");

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            try
            {
                int resultado = await _repositoriosCuentas.Borrar(id);
                if (resultado == 0)
                    throw new Exception("No hay datos a borrar");
            }
            catch (Exception ex)
            {
                //Se añade error al modelo
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var cuenta = await _repositoriosCuentas.ConsultarPorId(id, usuarioId);
            if (cuenta == null)
                return RedirectToAction("NoEncontrado", "Home");

            var modelo = await _serviciosReporte.ObtenerTransaccionesDetalladasPorCuenta(usuarioId, id, mes, año, ViewBag);
            ViewBag.Cuenta = cuenta.Nombre;

            return View(modelo);
        }

    }
}
