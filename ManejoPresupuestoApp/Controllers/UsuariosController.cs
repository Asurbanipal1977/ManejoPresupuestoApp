using ManejoPresupuestoApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace ManejoPresupuestoApp.Controllers
{
    //[Authorize]
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IStringLocalizer<Usuario> _localizer;
        private readonly SignInManager<Usuario> _signInManager;

        public UsuariosController(UserManager<Usuario> userManager, 
                                 IStringLocalizer<Usuario> localizer,
                                 SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _localizer = localizer;
            _signInManager = signInManager;
        } 

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new Usuario()
            {
                Email = modelo.Email,
            };
             var resultado = await _userManager.CreateAsync(usuario, password: modelo.Password);

            if (resultado.Succeeded)
            {
                await _signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(String.Empty, _localizer[error.Code].Value);
                }

                return View(modelo);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index","Transacciones");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var resultado = await _signInManager.PasswordSignInAsync
                (model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Transacciones");
            else
            {
                ModelState.AddModelError(String.Empty, "El usuario o password son incorrectos");
                return View(model);
            }
        }
    }
}
