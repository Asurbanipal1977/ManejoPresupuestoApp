using Dapper;
using ManejoPresupuestoApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuestoApp.Services
{
    public interface IRepositoriosTransacciones
    {
        Task Crear(Transaccion transaccion);
    }
    public class RepositoriosTransacciones : IRepositoriosTransacciones
    {
        private readonly IConfiguration _configuration;
        private readonly string _cadenaConexion;
        public RepositoriosTransacciones(IConfiguration configuration)
        {
            _configuration = configuration;
            _cadenaConexion = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var id = await connection.QuerySingleAsync<int>(@"TransaccionesInsertar", 
                new {
                    UsuarioId = transaccion.UsuarioId,
                    FechaTransaccion = transaccion.FechaTransaccion,
                    Monto = transaccion.Monto,
                    Nota = transaccion.Nota,
                    CuentaId = transaccion.CuentaId,
                    CategoriaId = transaccion.CategoriaId
                }, 
                commandType: CommandType.StoredProcedure);
            transaccion.Id = id;
        }
    }
}
