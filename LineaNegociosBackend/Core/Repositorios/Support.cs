using Core.Modelos;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositorios
{
    public class Support : Consultas.query
    {
        public IDbConnection _conn;
        public IDbTransaction transaction;
        public Support(IDbConnection conn) : base(conn)
        {
            this._conn = conn;
        }
        public Support(IDbConnection conn, IDbTransaction transaction) : base(conn)
        {
            this._conn = conn; this.transaction = transaction;
        }
        public async Task<int> AgregaTicket(PeticionSoporte peticion)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDCLIENTESISTEMA", peticion.idClienteSistema);
                dynamicParameters.Add(":IDUSUARIO", peticion.idUsuario);
                dynamicParameters.Add(":MENSAJE", peticion.mensaje);
                dynamicParameters.Add(":TIPO", peticion.tipo);
                dynamicParameters.Add(":ESTADO", "Abierto");

                string Query = @"insert into LNComentariosOProblemas (IDCLIENTESISTEMA, IDUSUARIO, MENSAJE, TIPO, ESTADO, FECHA) values
                                  (@IDCLIENTESISTEMA, @IDUSUARIO, @MENSAJE, @TIPO, @ESTADO, SYSDATETIME()) SELECT SCOPE_IDENTITY()";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                var lista = await objQuery.QuerySelectAsync(Query, dynamicParameters);
                return lista.AsList().Count > 0 ? lista.AsList()[0] : -1;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> SetLeido(string idMensaje)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDMENSAJE", idMensaje);

                string Query = @"update LNMensajesRespuestaSoporte set leido='S' where idMensaje= @IDMENSAJE";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> AgregaImg(string idCaso, string pathImg)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDCASO", idCaso);
                dynamicParameters.Add(":PATHIMG", pathImg);

                string Query = @"insert into LNImagenesSoporte (IDCASO, PATHIMG) values
                                  (@IDCASO, @PATHIMG)";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<List<ResponseMensajes>> ObtenerMensajes(string idUsuario)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDUSUARIO", idUsuario);

            string sql = @"select LNMensajesRespuestaSoporte.* from LNMensajesRespuestaSoporte inner join LNComentariosOProblemas
                            on LNMensajesRespuestaSoporte.idCaso = LNComentariosOProblemas.idCaso where LNComentariosOProblemas.idUsuario= @IDUSUARIO";

            Consultas.queryAsyncConn<ResponseMensajes> objQuery = new Consultas.queryAsyncConn<ResponseMensajes>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<ResponseInfoMensaje> ObtenerInfoMensaje(string idMensaje)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDMENSAJE", idMensaje);

            string sql = @"select LNComentariosOProblemas.idCaso, LNComentariosOProblemas.mensaje, LNComentariosOProblemas.tipo,
                            LNComentariosOProblemas.estado, LNComentariosOProblemas.fecha
                            from LNMensajesRespuestaSoporte inner join LNComentariosOProblemas
                            on LNMensajesRespuestaSoporte.idCaso = LNComentariosOProblemas.idCaso where LNMensajesRespuestaSoporte.idMensaje = @IDMENSAJE";

            Consultas.queryAsyncConn<ResponseInfoMensaje> objQuery = new Consultas.queryAsyncConn<ResponseInfoMensaje>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList().Count > 0 ? existe.AsList()[0] : null;
        }
        public async Task<List<ImagenesRespuesta>> ObtenerImagenesResponse(string idMensaje)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDMSJ", idMensaje);

            string sql = @"select pathImg from LNImagenesRespuesta where idMensaje= @IDMSJ";

            Consultas.queryAsyncConn<ImagenesRespuesta> objQuery = new Consultas.queryAsyncConn<ImagenesRespuesta>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
    }
}
