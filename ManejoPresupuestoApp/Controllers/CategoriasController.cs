using AutoMapper;
using ManejoPresupuestoApp.Models;
using ManejoPresupuestoApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuestoApp.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly IRepositoriosCategorias _repositoriosCategorias;
        private readonly IServiciosUsuarios _serviciosUsuarios;
        private readonly IMapper _mapper;

        public CategoriasController(IRepositoriosCategorias repositoriosCategorias,
                                    IServiciosUsuarios serviciosUsuarios,
                                    IMapper mapper)
        {
            _repositoriosCategorias = repositoriosCategorias;
            _serviciosUsuarios = serviciosUsuarios;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var lstCategorias = await _repositoriosCategorias.ListadoCategorias(usuarioId);

            return View(lstCategorias);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            categoria.UsuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            await _repositoriosCategorias.Crear(categoria);   

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            Categoria categoria = await _repositoriosCategorias.ConsultarPorId(id, usuarioId);
            if (categoria == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Modificar(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            categoria.UsuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            Categoria categoriaCons = await _repositoriosCategorias.ConsultarPorId(categoria.Id, categoria.UsuarioId);
            if (categoriaCons == null)
                RedirectToAction("NoEncontrado", "Home");

            await _repositoriosCategorias.Actualizar(categoria);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = _serviciosUsuarios.ObtenerUsuarioId();
            var cuenta = await _repositoriosCategorias.ConsultarPorId(id, usuarioId);
            if (cuenta == null)
                return RedirectToAction("NoEncontrado", "Home");

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            try
            {
                int resultado = await _repositoriosCategorias.Borrar(id);
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
    }
}
