using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using System.Linq;

namespace Consultas
{
    public abstract class queryAbstract
    {
        protected const int _CommandTimeOut = 1800;
    }
    public class queryAsync<TReturn> : queryAbstract
    {
        const string _confConexionOracle = "OracleDBPool";
        private System.Data.IDbTransaction _Transaccion;
        private string _confConexion;

        public queryAsync(string ConfigConexion = _confConexionOracle)
        {
            _Transaccion = null;
            _confConexion = ConfigConexion;
        }

        public queryAsync(System.Data.IDbTransaction Transaccion, string ConfigConexion = _confConexionOracle)
        {
            _Transaccion = Transaccion;
            _confConexion = ConfigConexion;
        }

        public async Task<IEnumerable<TReturn>> QuerySelectAsync<T, T1, T2, T3>(string query, System.Func<T, T1, T2, T3, TReturn> map, string splitOn, object parametros)
        {
            using (System.Data.IDbConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=VentasWeb;Integrated Security=True"))
            {
                try
                {
                    var result = await conn.QueryAsync<T, T1, T2, T3, TReturn>(query, map, parametros, _Transaccion, true, splitOn, _CommandTimeOut, System.Data.CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<IEnumerable<TReturn>> QuerySelectAsync<T, T1, T2, T3, T4>(string query, System.Func<T, T1, T2, T3, T4, TReturn> map, string splitOn, object parametros)
        {
            using (System.Data.IDbConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=VentasWeb;Integrated Security=True"))
            {
                try
                {
                    var result = await conn.QueryAsync<T, T1, T2, T3, T4, TReturn>(query, map, parametros, _Transaccion, true, splitOn, _CommandTimeOut, System.Data.CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<IEnumerable<TReturn>> QuerySelectAsync(string query)
        {
            using (System.Data.IDbConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=VentasWeb;Integrated Security=True"))
            {
                var result = await conn.QueryAsync<TReturn>(query, null, _Transaccion, _CommandTimeOut, System.Data.CommandType.Text);
                return result;
            }
        }

        public async Task<IEnumerable<TReturn>> QuerySelectAsync(string query, object parametros)
        {
            using (System.Data.IDbConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=VentasWeb;Integrated Security=True"))
            {
                var result = await conn.QueryAsync<TReturn>(query, parametros, _Transaccion, _CommandTimeOut, System.Data.CommandType.Text);
                return result;
            }
        }
    }

    public abstract class query : queryAbstract
    {
        private System.Data.IDbConnection _Conexion;


        public query(System.Data.IDbConnection myconn)
        {
            _Conexion = myconn;
        }

        public System.Data.IDbConnection Conn
        {
            get { return _Conexion; }
        }

        protected List<T> QuerySelect<T>(string query)
        {
            return _Conexion.Query<T>(query, null, null, true, _CommandTimeOut, System.Data.CommandType.Text).ToList();
        }

        protected List<T> QuerySelect<T>(string query, object parametros)
        {
            return _Conexion.Query<T>(query, parametros, null, true, _CommandTimeOut, System.Data.CommandType.Text).ToList();
        }
        protected IEnumerable<object> QuerySelect(string query)
        {
            return _Conexion.Query(query, null, null, true, _CommandTimeOut, System.Data.CommandType.Text).ToList();
        }

        protected List<TReturn> QuerySelect<T, T1, TReturn>(string query, System.Func<T, T1, TReturn> map, string splitOn, object parametros)
        {
            return _Conexion.Query<T, T1, TReturn>(query, map, parametros, _Conexion.BeginTransaction(), true, splitOn, _CommandTimeOut, System.Data.CommandType.Text).ToList();
        }
        protected int InsertMany<T>(string query, List<T> parametros)
        {
            return _Conexion.Execute(query, parametros);
        }
        protected int InsertMany<T>(string query, object parametros)
        {
            return _Conexion.Execute(query, parametros);
        }
    }

    public class queryAsyncConn<TReturn> : queryAbstract
    {
        private System.Data.IDbConnection _Conexion;

        private System.Data.IDbTransaction _Transaccion;
        public queryAsyncConn(System.Data.IDbConnection myconn)
        {
            _Transaccion = null;
            _Conexion = myconn;
        }
        public queryAsyncConn(System.Data.IDbConnection myconn, System.Data.IDbTransaction transaction)
        {
            _Transaccion = transaction;
            _Conexion = myconn;
        }

        public System.Data.IDbConnection Conn
        {
            get { return _Conexion; }
        }

        public async Task<IEnumerable<TReturn>> QuerySelectAsync<T, T1, T2, T3>(string query, System.Func<T, T1, T2, T3, TReturn> map, string splitOn, object parametros)
        {
            using (_Conexion)
            {
                try
                {
                    var result = await _Conexion.QueryAsync<T, T1, T2, T3, TReturn>(query, map, parametros, _Transaccion, true, splitOn, _CommandTimeOut, System.Data.CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<IEnumerable<TReturn>> QuerySelectAsync<T, T1, T2, T3, T4>(string query, System.Func<T, T1, T2, T3, T4, TReturn> map, string splitOn, object parametros)
        {
            using (_Conexion)
            {
                try
                {
                    var result = await _Conexion.QueryAsync<T, T1, T2, T3, T4, TReturn>(query, map, parametros, _Transaccion, true, splitOn, _CommandTimeOut, System.Data.CommandType.Text);
                    return result;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<IEnumerable<TReturn>> QuerySelectAsync(string query)
        {

            var result = await _Conexion.QueryAsync<TReturn>(query, null, _Transaccion, _CommandTimeOut, System.Data.CommandType.Text);
            return result;
        }

        public async Task<IEnumerable<TReturn>> QuerySelectAsync(string query, object parametros)
        {

            var result = await _Conexion.QueryAsync<TReturn>(query, parametros, _Transaccion, _CommandTimeOut, System.Data.CommandType.Text);
            return result;
        }
        public async Task<int> ExecuteAsync(string query, object parametros)
        {
            return await _Conexion.ExecuteAsync(query, parametros, _Transaccion);
        }
        public async Task<int> ExecuteQueryAsync(string query, object parametros)
        {

            return await _Conexion.ExecuteAsync(query, parametros, _Transaccion);

        }
    }
}
