using Dapper;
using ManejoPresupuestoApp.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestoApp.Services
{
    public interface IRepositoriosCategorias
    {
        Task<int> Actualizar(Categoria categoria);
        Task<int> Borrar(int id);
        Task<Categoria> ConsultarPorId(int Id, int UsuarioId);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> ListadoCategorias(int UsuarioId);
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

        public async Task<IEnumerable<Categoria>> ListadoCategorias(int UsuarioId)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryAsync<Categoria>($@"SELECT * FROM Categorias
                                                            WHERE UsuarioId=@UsuarioId", new { UsuarioId });
        }

        public async Task<Categoria> ConsultarPorId(int Id, int UsuarioId)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryFirstOrDefaultAsync<Categoria>($@"SELECT * from Categorias                                                            
                                                            WHERE UsuarioId=@UsuarioId AND Id=@Id", new { Id, UsuarioId });
        }

        public async Task<int> Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var numFilas = await connection.ExecuteAsync($@"UPDATE Categorias SET Nombre=@Nombre,TipoOperacionId=@TipoOperacionId
                                                    WHERE Id=@Id AND UsuarioId=@UsuarioId", categoria);
            return numFilas;
        }

        public async Task<int> Borrar(int id)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var modificado = await connection.ExecuteAsync(@"DELETE CATEGORIAS WHERE Id=@Id", new { id });
            return modificado;
        }
    }
}
