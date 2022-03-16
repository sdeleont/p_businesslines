using Core.Modelos;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositorios
{
    public class LineaNegocio : Consultas.query
    {
        public IDbConnection _conn;
        public IDbTransaction transaction;
        public LineaNegocio(IDbConnection conn) : base(conn)
        {
            this._conn = conn;
        }
        public LineaNegocio(IDbConnection conn, IDbTransaction transaction) : base(conn)
        {
            this._conn = conn; this.transaction = transaction;
        }
        public async Task<ConfiguracionLinea> ObtenerLineaNegocioPorID(string idLinea)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDLINEA", idLinea);

            string sql = @"select LNLineaNegocio.idLineaNegocio, LNLineaNegocio.idClienteSistema, LNTiposDeNegocios.nombre nombreNegocio,
                            LNLineaNegocio.nombre, LNLineaNegocio.descripcion, LNLineaNegocio.formaMuestra, LNTiposDeNegocios.icono, LNLineaNegocio.tablaInformacion tabla
                            from LNLineaNegocio 
                            inner join LNTiposDeNegocios on LNLineaNegocio.idTipoNegocio = LNTiposDeNegocios.idTipoNegocio
                            where LNLineaNegocio.estado='A' and idLineaNegocio = @IDLINEA";

            Consultas.queryAsyncConn<ConfiguracionLinea> objQuery = new Consultas.queryAsyncConn<ConfiguracionLinea>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList().Count > 0 ? existe.AsList()[0] : null ;
        }
        public async Task<List<TiposDato>> ObtenerTiposDato()
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @" select * from  LNMDTiposDato";
            Consultas.queryAsyncConn<TiposDato> objQuery = new Consultas.queryAsyncConn<TiposDato>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<TipoNegocio>> ObtenerTiposNegocio()
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select idTipoNegocio, nombre from LNTiposDeNegocios";
            Consultas.queryAsyncConn<TipoNegocio> objQuery = new Consultas.queryAsyncConn<TipoNegocio>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<TiposDatoD>> ObtenerTiposDatoD()
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select idTipoDato, descripcion from LNMDTiposDato";
            Consultas.queryAsyncConn<TiposDatoD> objQuery = new Consultas.queryAsyncConn<TiposDatoD>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<object>> ObtenerData(string queryWhere,string tabla, string punteroInicio, string punteroFinal, string llavePrimaria)
        {
            var dynamicParameters = new DynamicParameters();
            // dynamicParameters.Add(":IDCLIENTESISTEMA", idCliente);

            string sql = @"select * from 
                            (select Row_Number() over 
                             (order by "+llavePrimaria+") as RowIndex, * from " + tabla + @") as Sub
                            Where Sub.RowIndex >= " + punteroInicio + " and Sub.RowIndex <= " + punteroFinal;
            if (!queryWhere.Equals("")) {
                sql = @"select * from 
                            (select Row_Number() over 
                             (order by " + llavePrimaria + ") as RowIndex, * from " + tabla + " where "+queryWhere + @") as Sub
                            Where Sub.RowIndex >= " + punteroInicio + " and Sub.RowIndex <= " + punteroFinal;
            }
            Consultas.queryAsyncConn<object> objQuery = new Consultas.queryAsyncConn<object>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<object>> ObtenerDataDeUnRegistro(string tabla, string llavePrimaria, string valorPrimaria)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":LLAVE", valorPrimaria);

            string sql = @"select * from " + tabla + " where " + llavePrimaria + "= @LLAVE";

            Consultas.queryAsyncConn<object> objQuery = new Consultas.queryAsyncConn<object>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<object>> ObtenerTotal(string queryWhere, string tabla, string llavePrimaria)
        {
            var dynamicParameters = new DynamicParameters();
            // dynamicParameters.Add(":IDCLIENTESISTEMA", idCliente);

            string sql = @"select count("+ llavePrimaria + @") total from 
                            (select Row_Number() over 
                             (order by " + llavePrimaria + ") as RowIndex, * from " + tabla + @") as Sub ";
            if (!queryWhere.Equals(""))
            {
                sql = @"select count(" + llavePrimaria + @") total from 
                            (select Row_Number() over 
                             (order by " + llavePrimaria + ") as RowIndex, * from " + tabla + " where " + queryWhere + @") as Sub ";
            }
            Consultas.queryAsyncConn<object> objQuery = new Consultas.queryAsyncConn<object>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<CampoLineaNegocioCompleto>> ObtenerCamposLineaCompleto(string idlinea)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDLINEANEGOCIO", idlinea);

            string sql = @"select LNMDTiposDato.nombre nombreTipoDato, LNMDTiposDato.descripcion descTipoDato, LNMDTiposDato.idTipodato, 
                            LNMDTiposDato.descripcion nombreCampo, LNLineasNegocioCampos.descripcionCampo, LNLineasNegocioCampos.obligatorio,
                            LNLineasNegocioCampos.esCampoAgrupador, LNLineasNegocioCampos.orden, LNLineasNegocioCampos.esLlavePrimaria, LNLineasNegocioCampos.nombreCampo campoTabla,
                            LNMDTiposDato.er, LNMDTiposDato.tipoenbd, LNLineasNegocioCampos.campoEnIA 
                            from LNLineasNegocioCampos 
                            inner join LNMDTiposDato on LNLineasNegocioCampos.idTipodato = LNMDTiposDato.idTipodato
                            where LNLineasNegocioCampos.idLineaNegocio = @IDLINEANEGOCIO
                            order by LNLineasNegocioCampos.orden asc";
            Consultas.queryAsyncConn<CampoLineaNegocioCompleto> objQuery = new Consultas.queryAsyncConn<CampoLineaNegocioCompleto>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<LineaNegocioGeneral>> ObtenerLineasNegocio(string idCliente)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDCLIENTESISTEMA", idCliente);

            string sql = @"select LNLineaNegocio.idLineaNegocio, LNLineaNegocio.idClienteSistema, LNTiposDeNegocios.nombre nombreNegocio,
                            LNLineaNegocio.nombre, LNLineaNegocio.descripcion, LNLineaNegocio.formaMuestra, LNTiposDeNegocios.icono
                            from LNLineaNegocio 
                            inner join LNTiposDeNegocios on LNLineaNegocio.idTipoNegocio = LNTiposDeNegocios.idTipoNegocio
                            where LNLineaNegocio.estado='A' and LNLineaNegocio.idClienteSistema=" + idCliente;
            Consultas.queryAsyncConn<LineaNegocioGeneral> objQuery = new Consultas.queryAsyncConn<LineaNegocioGeneral>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<CampoLineaGeneral>> ObtenerCamposLinea(string idlinea)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDLINEANEGOCIO", idlinea);

            string sql = @"select LNMDTiposDato.descripcion nombreCampo, LNLineasNegocioCampos.descripcionCampo, LNLineasNegocioCampos.obligatorio,
                            LNLineasNegocioCampos.esCampoAgrupador
                            from LNLineasNegocioCampos 
                            inner join LNMDTiposDato on LNLineasNegocioCampos.idTipodato = LNMDTiposDato.idTipodato
                            where LNLineasNegocioCampos.idLineaNegocio = @IDLINEANEGOCIO
                            order by LNLineasNegocioCampos.orden asc";
            Consultas.queryAsyncConn<CampoLineaGeneral> objQuery = new Consultas.queryAsyncConn<CampoLineaGeneral>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }

        public async Task<List<TipoDatoBD>> ObtenerTiposDatosCompleto()
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select * from LNMDTiposDato";
            Consultas.queryAsyncConn<TipoDatoBD> objQuery = new Consultas.queryAsyncConn<TipoDatoBD>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<LineaNegocioNombreTabla>> ObtenerNegociosNombre(string nombreTabla)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":TABLA", nombreTabla);
            string sql = @"select tablaInformacion from LNLineaNegocio where tablaInformacion = @TABLA";
            Consultas.queryAsyncConn<LineaNegocioNombreTabla> objQuery = new Consultas.queryAsyncConn<LineaNegocioNombreTabla>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<int> InsertaLineaNegocio(LineaNegocioModel lineaNegocio, string tablaInformacion)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDCLIENTESISTEMA",lineaNegocio.idClienteSistema);
                dynamicParameters.Add(":IDTIPONEGOCIO", lineaNegocio.idTipoNegocio);
                dynamicParameters.Add(":NOMBRE", lineaNegocio.nombre);
                dynamicParameters.Add(":DESCRIPCION", lineaNegocio.descripcion);
                dynamicParameters.Add(":FORMAMUESTRA", lineaNegocio.formaMuestra);
                dynamicParameters.Add(":TABLAINFORMACION", tablaInformacion);
                dynamicParameters.Add(":DIRECCIONSQLSERVER", this._conn.ConnectionString);
                dynamicParameters.Add(":ESTADO", "A");

                string Query = @"insert into LNLineaNegocio (idClienteSistema,idTipoNegocio, nombre, descripcion, formaMuestra, tablaInformacion, direccionSQLSERVER, estado) values
                                  (@IDCLIENTESISTEMA,@IDTIPONEGOCIO, @NOMBRE, @DESCRIPCION, @FORMAMUESTRA, @TABLAINFORMACION, @DIRECCIONSQLSERVER, @ESTADO)  SELECT SCOPE_IDENTITY()";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                var lista = await objQuery.QuerySelectAsync(Query, dynamicParameters);
                return lista.AsList().Count > 0 ? lista.AsList()[0] : -1;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> InsertaLineaNegocioCampo(string idLineaNegocio, CampoLineaNegocio campo, int orden)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDLINEANEGOCIO", idLineaNegocio);
                dynamicParameters.Add(":IDTIPODATO", campo.idTipodato);
                dynamicParameters.Add(":NOMBRECAMPO", campo.nombreCampo);
                dynamicParameters.Add(":DESCRIPCIONCAMPO", campo.descripcionCampo);
                dynamicParameters.Add(":OBLIGATORIO", campo.obligatorio);
                dynamicParameters.Add(":ESCAMPOAGRUPADOR", campo.esCampoAgrupador);
                dynamicParameters.Add(":ORDEN", orden);
                dynamicParameters.Add(":ESLLAVEPRIMARIA", campo.esLlavePrimaria);

                string Query = @"insert into LNLineasNegocioCampos (IDLINEANEGOCIO, IDTIPODATO, NOMBRECAMPO, DESCRIPCIONCAMPO, OBLIGATORIO, ESCAMPOAGRUPADOR, ORDEN, ESLLAVEPRIMARIA) values
                                  (@IDLINEANEGOCIO, @IDTIPODATO, @NOMBRECAMPO, @DESCRIPCIONCAMPO, @OBLIGATORIO, @ESCAMPOAGRUPADOR, @ORDEN, @ESLLAVEPRIMARIA) ";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> ActualizaFormaMuestra(string operacion, string idLineaNegocio)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":FORMAMUESTRA", operacion);
                dynamicParameters.Add(":IDLINEANEGOCIO", idLineaNegocio);

                string Query = @"UPDATE LNLineaNegocio SET FORMAMUESTRA = @FORMAMUESTRA WHERE IDLINEANEGOCIO=@IDLINEANEGOCIO";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> EliminaLineaNegocio(string idLineaNegocio)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDLINEANEGOCIO", idLineaNegocio);

                string Query = @"UPDATE LNLineaNegocio SET ESTADO = 'E' WHERE IDLINEANEGOCIO=@IDLINEANEGOCIO";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }

        public async Task<List<InformacionMDParaInsertar>> ObtenerInformacionParaParser(string idLineaNegocio)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDLINEANEGOCIO", idLineaNegocio);
            string sql = @"select LNLineaNegocio.idLineaNegocio, LNLineaNegocio.idTipoNegocio, LNLineaNegocio.tablaInformacion,
                            LNLineasNegocioCampos.nombreCampo, LNLineasNegocioCampos.obligatorio,LNLineasNegocioCampos.descripcionCampo, LNMDTiposDato.ER,
                            LNMDTiposDato.nombre, LNMDTiposDato.descripcion, LNMDTiposDato.TipoEnBD, LNLineasNegocioCampos.orden
                            from LNLineaNegocio 
                            inner join LNLineasNegocioCampos on LNLineaNegocio.idLineaNegocio=LNLineasNegocioCampos.idLineaNegocio
                            inner join LNMDTiposDato on LNLineasNegocioCampos.idTipodato = LNMDTiposDato.idTipodato
                            where LNLineaNegocio.idLineaNegocio = @IDLINEANEGOCIO AND LNLineaNegocio.estado='A' and LNLineasNegocioCampos.esLlavePrimaria = 'N'
                            order by orden asc";
            Consultas.queryAsyncConn<InformacionMDParaInsertar> objQuery = new Consultas.queryAsyncConn<InformacionMDParaInsertar>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<int> InsertaRegistro(string QueryArmado, List<ParametroDatoBD> listaParametrosBD)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                foreach (ParametroDatoBD parametroDatoBD in listaParametrosBD)
                {
                    dynamicParameters.Add(parametroDatoBD.parametro, parametroDatoBD.dato);
                }
                string Query = QueryArmado;
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> CreaTabla(string createDeTabla)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();

                string Query = createDeTabla;
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        ////////////////////////////////////////////////////////////////////Operaciones para la data////////////////////////////////
        public async Task<int> EliminaRegistro(string tabla, string idTabla, string valorIdTabla)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":VALOR", valorIdTabla);

                string Query = @"DELETE FROM  "+tabla+" where "+idTabla+"= @VALOR";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> insertaRegistro(string tabla, List<Valores> valores, List<CampoLineaNegocioCompleto> camposLista)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                string query = "insert into " + tabla;
                string campos = "";
                string vals = "";
                for (int i = 0; i < valores.Count; i++) {
                    string campo = valores[i].campotabla.ToUpper();
                    if (i == 0) { 
                        campos += campo;
                        vals += "@"+ campo;
                    }
                    else {
                        campos += "," + campo;
                        vals += ", @" + campo;
                    }
                    string TIPO = camposLista[i + 1].tipoenbd;
                    if (TIPO.Equals("INTEGER"))
                    {
                        dynamicParameters.Add(":" + campo, (valores[i].valor.Trim().Equals("")) ? 0 : Convert.ToInt32(valores[i].valor));
                    }
                    else if (TIPO.Equals("VARCHAR(MAX)") || TIPO.Equals("VARCHAR(250)") || TIPO.Equals("VARCHAR(50)")) {
                        dynamicParameters.Add(":" + campo, valores[i].valor);
                    }
                    else if (TIPO.Equals("REAL"))
                    {
                        dynamicParameters.Add(":" + campo, (valores[i].valor.Trim().Equals("")) ? 0 : Convert.ToDouble(valores[i].valor));
                    }
                    
                }
                query = query + " (" + campos + ") values (" + vals + ")";
                // string Query = @"DELETE FROM  " + tabla + " where " + idTabla + "= @VALOR";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> modificaRegistro(string tabla, List<Valores> valores, List<CampoLineaNegocioCompleto> camposLista, string idLlavePrimaria, string valorLlavePrimaria)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                string query = "update " + tabla;
                string campos = "";
                for (int i = 0; i < valores.Count; i++)
                {
                    string campo = valores[i].campotabla.ToUpper() + "= @" + valores[i].campotabla.ToUpper();
                    string parametroNombre = valores[i].campotabla.ToUpper();
                    if (i == 0)
                    {
                        campos += campo;
                    }
                    else
                    {
                        campos += "," + campo;
                    }
                    string TIPO = camposLista[i + 1].tipoenbd;
                    if (TIPO.Equals("INTEGER"))
                    {
                        dynamicParameters.Add(":" + parametroNombre, (valores[i].valor.Trim().Equals("")) ? 0 : Convert.ToInt32(valores[i].valor));
                    }
                    else if (TIPO.Equals("VARCHAR(MAX)") || TIPO.Equals("VARCHAR(250)") || TIPO.Equals("VARCHAR(50)"))
                    {
                        dynamicParameters.Add(":" + parametroNombre, valores[i].valor);
                    }
                    else if (TIPO.Equals("REAL"))
                    {
                        dynamicParameters.Add(":" + parametroNombre, (valores[i].valor.Trim().Equals("")) ? 0: Convert.ToDouble(valores[i].valor));
                    }

                }
                query = query + " set " + campos + " where " + idLlavePrimaria + "=" +valorLlavePrimaria;
                // string Query = @"DELETE FROM  " + tabla + " where " + idTabla + "= @VALOR";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> EliminaOtrasIMG(string idLinea, string valorRegistro, string guid)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDLINEA", idLinea);
                dynamicParameters.Add(":VALREGISTRO", valorRegistro);
                dynamicParameters.Add(":GUID", guid);

                string Query = @"DELETE FROM LNLineasNegocioTablasInformacion where idLineaNegocio=@IDLINEA and valorLlavePrimaria=@VALREGISTRO and pathImg=@GUID";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> InsertaRegistroIMG(string idLinea, string valorRegistro, string nomImg)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDLINEA", idLinea);
                dynamicParameters.Add(":VALREGISTRO", valorRegistro);
                dynamicParameters.Add(":NOMIMG", nomImg);

                string Query = @"INSERT INTO LNLineasNegocioTablasInformacion(idLineaNegocio, valorLlavePrimaria, pathImg) values (@IDLINEA, @VALREGISTRO, @NOMIMG)";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<List<LineaNegocioTablaInformacion>> ObtenerImagenesInformacion(string idLineaNegocio, string valorLLave)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDLINEANEGOCIO", idLineaNegocio);
            dynamicParameters.Add(":VALORLLAVE", valorLLave);

            string sql = @"select * from LNLineasNegocioTablasInformacion where idLineaNegocio = @IDLINEANEGOCIO and valorLlavePrimaria=@VALORLLAVE";
            Consultas.queryAsyncConn<LineaNegocioTablaInformacion> objQuery = new Consultas.queryAsyncConn<LineaNegocioTablaInformacion>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<ResponseDatosPorColumna>> ObtenerDataPorColumna(string tabla, string columna, int top)
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select top " + top +" " + columna + " as dato from " + tabla +" group by " + columna;
            Consultas.queryAsyncConn<ResponseDatosPorColumna> objQuery = new Consultas.queryAsyncConn<ResponseDatosPorColumna>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<object>> ObtenerDataGrafica1(string tabla, string strSumatorias, string  strValores, string campoPivot)
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select " + campoPivot + " " + strSumatorias + " from " + tabla + " where " + campoPivot + " in (" + strValores + ") group by " + campoPivot;
            Consultas.queryAsyncConn<object> objQuery = new Consultas.queryAsyncConn<object>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<int> ActualizarImgConfiguracionCliente(string idClienteSistema, string valorRegistro, string nomImg)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDCLIENTE", idClienteSistema);
                dynamicParameters.Add(":NOMIMG", nomImg);

                string Query = @"UPDATE LNConfiguracionCliente set " + valorRegistro + "=@NOMIMG where idClienteSistema=@IDCLIENTE";
                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<List<ConfClientCompleta>> ObtenerInfoCliente(string idCliente)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDCLIENTE", idCliente);

            string sql = @"select LNConfiguracionCliente.idClienteSistema, LNConfiguracionCliente.pathLogoEsquina, LNConfiguracionCliente.pathSlide1, 
                            LNConfiguracionCliente.pathSlide2, LNConfiguracionCliente.pathSlide3, LNConfiguracionCliente.numMaxUsuarios, 
                            LNConfiguracionCliente.numMaxLineasNegocio, 'Prueba' as tipoPeriodo
                            from LNConfiguracionCliente
                            where LNConfiguracionCliente.idClienteSistema = @IDCLIENTE";

            Consultas.queryAsyncConn<ConfClientCompleta> objQuery = new Consultas.queryAsyncConn<ConfClientCompleta>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
    }
}
