using Dapper;
using ManejoPresupuestoApp.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestoApp.Services
{
    public interface IRepositoriosTiposCuentas
    {
        Task<int> Actualizar(TipoCuenta tipoCuenta);
        Task<int> Borrar(TipoCuenta tipoCuenta);
        Task<TipoCuenta> ConsultarPorId(int Id, int UsuarioId);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(TipoCuenta tipoCuenta);
        Task<IEnumerable<TipoCuenta>> ListadoTiposCuentas(int UsuarioId);
        Task Sort(IEnumerable<TipoCuenta> tiposCuentasOrdenados);
    }

    public class RepositoriosTiposCuentas : IRepositoriosTiposCuentas
    {
        private readonly IConfiguration _configuration;
        private readonly string _cadenaConexion;
        public RepositoriosTiposCuentas (IConfiguration configuration)
        {
            _configuration = configuration;
            _cadenaConexion = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear (TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var id = await connection.QuerySingleAsync<int>("TiposCuentaInsertar",
                new { Nombre = tipoCuenta.Nombre,
                    UsuarioId = tipoCuenta.UsuarioId
                },commandType: System.Data.CommandType.StoredProcedure);
            tipoCuenta.Id = id;
        }

        public async Task<int> Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var id = await connection.ExecuteAsync($@"UPDATE TiposCuentas SET Nombre=@Nombre
                                                    WHERE Id=@Id AND UsuarioId=@UsuarioId", tipoCuenta);
            return id;
        }

        public async Task<int> Borrar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var id = await connection.ExecuteAsync($@"DELETE FROM TiposCuentas
                                                    WHERE Id=@Id AND UsuarioId=@UsuarioId", tipoCuenta);
            return id;
        }

        public async Task<bool> Existe(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var id = await connection.QueryFirstOrDefaultAsync<int>($@"SELECT 1 from TiposCuentas 
                                                    WHERE UPPER(Nombre) = UPPER(@Nombre) AND UsuarioId = @UsuarioId", tipoCuenta);
            return (id==1 ? true : false);
        }

        public async Task<IEnumerable<TipoCuenta>> ListadoTiposCuentas(int UsuarioId)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryAsync<TipoCuenta>($@"select * FROM TiposCuentas
                                                    WHERE UsuarioId =  @UsuarioId ORDER BY Orden", new { UsuarioId = UsuarioId });
        }

        public async Task<TipoCuenta> ConsultarPorId(int Id, int UsuarioId)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>($@"select * FROM TiposCuentas
                                    WHERE Id = @Id AND UsuarioId = @UsuarioId", new { Id, UsuarioId });
        }

        public async Task Sort(IEnumerable<TipoCuenta> tiposCuentasOrdenados)
        {
            var query = "UPDATE TiposCuentas Set Orden=@Orden Where Id=@Id";
            using var connection = new SqlConnection(_cadenaConexion);

            //Dapper actualiza cada uno de los elementos que le pasemos en tiposCuentasOrdenados
            await connection.ExecuteAsync(query, tiposCuentasOrdenados);
        }
    }
}
