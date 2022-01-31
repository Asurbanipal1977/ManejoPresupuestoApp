using System.Security.Claims;

namespace ManejoPresupuestoApp.Services
{
    public interface IServiciosUsuarios
    {
        int ObtenerUsuarioId();
    }
    public class ServiciosUsuarios : IServiciosUsuarios
    {
        private readonly HttpContext _httpContext;

        public ServiciosUsuarios(IHttpContextAccessor httpContextAccesor)
        {
            _httpContext = httpContextAccesor.HttpContext;
        }

        public IHttpContextAccessor HttpContextAccessor { get; }

        public int ObtenerUsuarioId()
        {
            int id = 0;
            var usuario = _httpContext.User;
            if (usuario.Identity.IsAuthenticated)
            //if (_signInManager.IsSignedIn(User))
            {
                var claims = usuario.Claims.ToList();
                var idClaim = claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                //id del usuario
                id = int.Parse(idClaim.Value);
            }
            else
            {
                throw new ApplicationException("El usuario no está autenticado");
            }
               
            return id;
        }
    }
}
