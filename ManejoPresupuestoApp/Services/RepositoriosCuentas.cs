using ManejoPresupuestoApp.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace ManejoPresupuestoApp.Services
{
    public interface IRepositoriosCuentas
    {
        Task<int> Borrar(int id);
        Task<Cuenta> ConsultarPorId(int Id, int UsuarioId);
        Task Crear(Cuenta cuenta);
        Task<IEnumerable<Cuenta>> ListadoCuentas(int UsuarioId);
        Task<int> Modificar(Cuenta cuenta);
    }
    public class RepositoriosCuentas : IRepositoriosCuentas
    {
        private readonly IConfiguration _configuration;
        private readonly string _cadenaConexion;
        public RepositoriosCuentas(IConfiguration configuration)
        {
            _configuration = configuration;
            _cadenaConexion = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Cuentas 
                                        (Nombre,TipoCuentaId,Balance,Descripcion) VALUES
                                        (@Nombre,@TipoCuentaId,@Balance,@Descripcion);
                                        select scope_identity();", cuenta);
            cuenta.Id = id;
        }

        public async Task<int> Modificar(Cuenta cuenta)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var modificado = await connection.ExecuteAsync(@"UPDATE Cuentas 
                                        SET Nombre=@Nombre, TipoCuentaId=@TipoCuentaId, 
                                         Balance=@Balance, Descripcion=@Descripcion WHERE Id=@Id", cuenta);
            return modificado;
        }

        public async Task<int> Borrar(int id)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var modificado = await connection.ExecuteAsync(@"DELETE CUENTAS WHERE Id=@Id", new { id });
            return modificado;
        }

        public async Task<IEnumerable<Cuenta>> ListadoCuentas(int UsuarioId)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryAsync<Cuenta>($@"SELECT c.Id, c.Nombre, tc.Nombre As TipoCuenta, Balance, Descripcion  from Cuentas c
                                                            INNER JOIN TiposCuentas tc ON c.TipoCuentaId = tc.Id
                                                            WHERE UsuarioId=@UsuarioId
                                                            ORDER BY tc.Orden", new { UsuarioId });
        }

        public async Task<Cuenta> ConsultarPorId(int Id, int UsuarioId)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>($@"SELECT c.Id, c.Nombre, tc.Nombre As TipoCuenta, c.TipoCuentaId, Balance, Descripcion  from Cuentas c
                                                            INNER JOIN TiposCuentas tc ON c.TipoCuentaId = tc.Id
                                                            WHERE tc.UsuarioId=@UsuarioId AND c.Id=@Id", new { Id, UsuarioId });
        }
    }
}
