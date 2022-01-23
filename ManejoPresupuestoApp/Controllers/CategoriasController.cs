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
    }
}
