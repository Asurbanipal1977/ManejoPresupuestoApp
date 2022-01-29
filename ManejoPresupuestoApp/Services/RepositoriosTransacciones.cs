using Dapper;
using ManejoPresupuestoApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuestoApp.Services
{
    public interface IRepositoriosTransacciones
    {
        Task<int> Borrar(int Id);
        Task<Transaccion> ConsultarPorId(int Id, int UsuarioId);
        Task Crear(Transaccion transaccion);
        Task<int> Modificar(Transaccion transaccion, decimal montoAnterior, int cuentaIdAnterior);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(TransaccionesPorCuentaModel modelo);
        Task<IEnumerable<TransaccionesPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<TransaccionesPorSemana>> ObtenerPorSemana(TransaccionesPorUsuarioModel modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(TransaccionesPorUsuarioModel modelo);
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

        public async Task<int> Modificar(Transaccion transaccion, 
            decimal montoAnterior, int cuentaIdAnterior)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var cantidad = await connection.QuerySingleAsync<int>(@"TransaccionesModificar",
                new
                {
                    Id = transaccion.Id,
                    UsuarioId = transaccion.UsuarioId,
                    FechaTransaccion = transaccion.FechaTransaccion,
                    Monto = transaccion.Monto,
                    MontoAnterior= montoAnterior,
                    CuentaId = transaccion.CuentaId,
                    CuentaIdAnterior = cuentaIdAnterior,
                    CategoriaId = transaccion.CategoriaId,
                    Nota = transaccion.Nota
                },
                commandType: CommandType.StoredProcedure);
            return cantidad;
        }


        public async Task<int> Borrar(int Id)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            var cantidad = await connection.QuerySingleAsync<int>(@"TransaccionesBorrar",
                new
                {
                    Id
                },
                commandType: CommandType.StoredProcedure);
            return cantidad;
        }

        public async Task<Transaccion> ConsultarPorId(int Id, int UsuarioId)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>($@"SELECT t.*, c.TipoOperacionId  from Transacciones t
                                                            INNER JOIN Categorias c ON t.CategoriaId = c.Id
                                                            WHERE t.UsuarioId=@UsuarioId AND t.Id=@Id", new { Id, UsuarioId });
        }


        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId
            (TransaccionesPorCuentaModel modelo)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryAsync<Transaccion>($@"SELECT t.Id, t.FechaTransaccion, t.Monto, ca.Nombre as Categoria,
                                                            cu.Nombre Cuenta, ca.TipoOperacionId from Transacciones t
                                                            INNER JOIN Categorias ca ON t.CategoriaId = ca.Id
                                                            INNER JOIN Cuentas cu ON t.CuentaId = cu.Id
                                                            WHERE t.UsuarioId=@UsuarioId AND t.CuentaId=@CuentaId
                                                            AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin", 
                                                            modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId
           (TransaccionesPorUsuarioModel modelo)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryAsync<Transaccion>($@"SELECT t.Id, t.FechaTransaccion, t.Monto, t.Nota, ca.Nombre as Categoria,
                                                            cu.Nombre Cuenta, ca.TipoOperacionId from Transacciones t
                                                            INNER JOIN Categorias ca ON t.CategoriaId = ca.Id
                                                            INNER JOIN Cuentas cu ON t.CuentaId = cu.Id
                                                            WHERE t.UsuarioId=@UsuarioId 
                                                            AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                                                            ORDER BY t.FechaTransaccion DESC",
                                                            modelo);
        }

        public async Task<IEnumerable<TransaccionesPorSemana>> ObtenerPorSemana
           (TransaccionesPorUsuarioModel modelo)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryAsync<TransaccionesPorSemana>($@"SELECT datediff (d,@FechaInicio,FechaTransaccion) / 7 +1
                                                            as Semana,
                                                            Sum(Monto) as Monto, cat.TipoOperacionId
                                                            FROM Transacciones t
                                                            inner join Categorias cat
                                                            on t.CategoriaId = cat.Id
                                                            WHERE FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                                                            and t.UsuarioId = 1
                                                            GROUP BY datediff (d,@FechaInicio,FechaTransaccion) / 7, cat.TipoOperacionId",
                                                            modelo);
        }

        public async Task<IEnumerable<TransaccionesPorMes>> ObtenerPorMes
           (int usuarioId, int año)
        {
            using var connection = new SqlConnection(_cadenaConexion);
            return await connection.QueryAsync<TransaccionesPorMes>($@"select month(t.FechaTransaccion) as Mes,
                    Sum(Monto) as Monto, cat.TipoOperacionId
                    FROM Transacciones t
                    inner join Categorias cat
                    on t.CategoriaId = cat.Id
                    WHERE year(FechaTransaccion)=@Año
                    and t.UsuarioId = @UsuarioId
                    GROUP BY month(t.FechaTransaccion), cat.TipoOperacionId", new
                    {
                        UsuarioId = usuarioId,
                        Año = año
                    });
        }

    }
}
