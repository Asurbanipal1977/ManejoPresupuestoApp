using Dapper;
using ManejoPresupuestoApp.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestoApp.Services
{
    public interface IRepositoriosCategorias
    {
        Task Crear(Categoria categoria);
    }
    public class RepositoriosCategorias : IRepositoriosCategorias
    {
        private readonly IConfiguration _configuration;
        private readonly string _cadenaConexion;
        public RepositoriosCategorias(IConfiguration configuration)
        {
            _configuration = configuration;
            _cadenaConexion = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Categorias 
                                        (Nombre,TipoOperacionId,UsuarioId) VALUES
                                        (@Nombre,@TipoOperacionId,@UsuarioId);
                                        select scope_identity();", categoria);
            categoria.Id = id;
        }
    }
}
