using AutoMapper;
namespace ManejoPresupuestoApp.Models
{
    public class Mapeador : Profile
    {
        public Mapeador()
        {
            //Nos permite mapear aunque el nombre de los campos cambie
            CreateMap<Cuenta, CuentaViewModel>();
        }
    }
}
