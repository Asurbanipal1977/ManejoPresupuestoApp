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

        public TransaccionesController(IRepositoriosCategorias repositoriosCategorias,
                                    IRepositoriosCuentas repositoriosCuentas,
                                    IRepositoriosTransacciones repositoriosTransacciones,
                                    IServiciosUsuarios serviciosUsuarios,
                                    IMapper mapper)
        {
            _repositoriosCategorias = repositoriosCategorias;
            _repositoriosCuentas = repositoriosCuentas;
            _repositoriosTransacciones = repositoriosTransacciones;
            _serviciosUsuarios = serviciosUsuarios;
            _mapper = mapper;
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
        public async Task<IActionResult> Crear(TransaccionCreacionModel transaccion)
        {
            if (!ModelState.IsValid)
            {
                return View(transaccion);
            }

            transaccion.UsuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            await _repositoriosTransacciones.Crear(transaccion);

            return RedirectToAction("Index");
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
