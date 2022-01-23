using Dapper;
using ManejoPresupuestoApp.Models;
using ManejoPresupuestoApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace ManejoPresupuestoApp.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositoriosTiposCuentas _repositoriosTiposCuentas;
        private readonly IServiciosUsuarios _serviciosUsuarios;
        public TiposCuentasController(IRepositoriosTiposCuentas repositoriosTiposCuentas,
                                      IServiciosUsuarios serviciosUsuarios)
        {
            _repositoriosTiposCuentas = repositoriosTiposCuentas;
            _serviciosUsuarios = serviciosUsuarios;
        }

        public async Task<IActionResult> Index()
        { 
            IEnumerable<TipoCuenta> lstCuentas = await _repositoriosTiposCuentas.ListadoTiposCuentas(_serviciosUsuarios.ObtenerUsuarioId());

            return View(lstCuentas);
        }


        public IActionResult Crear()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await _repositoriosTiposCuentas.ConsultarPorId(id, usuarioId);

            if (tipoCuenta == null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            if (!await _repositoriosTiposCuentas.Existe(tipoCuenta))
            {
                await _repositoriosTiposCuentas.Crear(tipoCuenta);
            }
            else
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), "Ya existe un tipo de cuenta con este valor");
                return View(tipoCuenta);
            }
           

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View("Editar",tipoCuenta);
            }

            tipoCuenta.UsuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            try
            { 
                int resultado = await _repositoriosTiposCuentas.Actualizar(tipoCuenta);
                if (resultado == 0)
                    throw new Exception("No hay datos a modificar");
            }
            catch (Exception ex)
            {
                //Se añade error al modelo
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Editar", tipoCuenta);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(TipoCuenta tipoCuenta)
        {
            tipoCuenta.UsuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuentaConsulta = await _repositoriosTiposCuentas.ConsultarPorId(tipoCuenta.Id, tipoCuenta.UsuarioId);
            if (tipoCuentaConsulta == null)
                return RedirectToAction("NoEncontrado", "Home");

            return View(tipoCuentaConsulta);
        }


        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(TipoCuenta tipoCuenta)
        {
            tipoCuenta.UsuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            try
            {
                int resultado = await _repositoriosTiposCuentas.Borrar(tipoCuenta);
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

        [HttpGet]
        public async Task<IActionResult> ExisteTipoCuenta(string nombre)
        {
            var yaExiste = await _repositoriosTiposCuentas.Existe(new TipoCuenta { UsuarioId = _serviciosUsuarios.ObtenerUsuarioId(), Nombre = nombre });

            return (yaExiste ? Json($"El nombre ya existe") : Json(true));
        }

        [HttpPost]
        public async Task<IActionResult> Sort([FromBody] int[] ids)
        {
            var UsuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            List<TipoCuenta> tiposCuentasOrdenados;

            //Selecciona los ids del listado
            var tiposCuentas = await _repositoriosTiposCuentas.ListadoTiposCuentas(UsuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            //Toma los ids que llega por parámetro y los compara con los que devuelve el listado
            var idsNoSonUsuario = ids.Except(idsTiposCuentas).ToList();
            if (idsNoSonUsuario.Count > 0)
                return Forbid();
            else
            {
                tiposCuentasOrdenados = ids.Select((valor, indice) => new TipoCuenta() {
                    Id = valor,
                    UsuarioId = UsuarioId,
                    Orden = indice + 1
                }).ToList();
            }

            await _repositoriosTiposCuentas.Sort(tiposCuentasOrdenados);

            return Ok();
        }
    }

}
