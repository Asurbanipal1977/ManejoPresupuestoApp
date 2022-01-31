using Dapper;
using ManejoPresupuestoApp.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestoApp.Services
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }

    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly IConfiguration _configuration;
        private readonly string _cadenaConexion;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            _configuration = configuration;
            _cadenaConexion = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Usuarios 
                                        (Email,EmailNormalizado,PasswordHash) VALUES
                                        (@Email,@EmailNormalizado,@PasswordHash);
                                        select scope_identity();", usuario);
            await connection.ExecuteAsync("CrearDatosPorDefecto", new { UsuarioId = id }, 
                commandType: System.Data.CommandType.StoredProcedure);

            return id;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var usuario = await connection.QuerySingleOrDefaultAsync<Usuario>(@"SELECT * FROM Usuarios where 
                                    EmailNormalizado=@emailNormalizado;", new { emailNormalizado });
            return usuario;
        }
    }
}
