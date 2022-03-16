using Core.Modelos;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositorios
{
    public class Auth : Consultas.query
    {
        public IDbConnection _conn;
        public IDbTransaction transaction;
        public Auth(IDbConnection conn) : base(conn)
        {
            this._conn = conn;
        }
        public Auth(IDbConnection conn, IDbTransaction transaction) : base(conn)
        {
            this._conn = conn; this.transaction = transaction;
        }
        public async Task<List<Rol>> ObtenerRoles(string idCliente)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDCLIENTE", idCliente);

            string sql = @"select * from LNRol where idclienteSistema = @IDCLIENTE";

            Consultas.queryAsyncConn<Rol> objQuery = new Consultas.queryAsyncConn<Rol>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<Rol>> ObtenerRolesBusqueda(string idCliente, string busqueda)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDCLIENTE", idCliente);

            string sql = @"select * from LNRol where idclienteSistema = @IDCLIENTE and (nombre like '%" + busqueda + "%' or descripcion like '%" + busqueda + "%')";

            Consultas.queryAsyncConn<Rol> objQuery = new Consultas.queryAsyncConn<Rol>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<Usuario>> ObtenerUsuariosBusqueda(string idCliente, string busqueda)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDCLIENTE", idCliente);

            string sql = @"select * from LNUsuario where idclienteSistema = @IDCLIENTE and estado='A' and 
                            (nombre like '%" + busqueda + "%' or correo like '%" + busqueda + @"%' 
                             or numero like '%" + busqueda + "%' or usuario like '% " + busqueda + "%')";

            Consultas.queryAsyncConn<Usuario> objQuery = new Consultas.queryAsyncConn<Usuario>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<Usuario>> ValidarUsuarioExistente(string usuario)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":USUARIO", usuario);

            string sql = @"select * from LNUsuario where usuario = @USUARIO and estado='A' ";

            Consultas.queryAsyncConn<Usuario> objQuery = new Consultas.queryAsyncConn<Usuario>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<ClSistema>> GetInfoClienteSistemaCodigo(string clSistema)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":ID", clSistema);

            string sql = @"select idClienteSistema, correo, numeroTelefono, codigoverificacion, codigoverificado from LNClienteSistema where idClienteSistema = @ID";

            Consultas.queryAsyncConn<ClSistema> objQuery = new Consultas.queryAsyncConn<ClSistema>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<int> ActualizaCodVerificacion(string idCliente, string codigo)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDCLIENTE", idCliente);
                dynamicParameters.Add(":CODIGO", codigo);

                string Query = @"update LNClienteSistema set  CODIGOVERIFICACION= @CODIGO WHERE IDCLIENTESISTEMA=@IDCLIENTE";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> SetVerificado(string idCliente)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDCLIENTE", idCliente);

                string Query = @"update LNClienteSistema set  CODIGOVERIFICADO= 'S' WHERE IDCLIENTESISTEMA=@IDCLIENTE";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<List<Usuario>> ValidarUsuarioConRol(string idRol)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDROL", idRol);

            string sql = @"select * from LNUsuario where idRol = @IDROL and estado='A' ";

            Consultas.queryAsyncConn<Usuario> objQuery = new Consultas.queryAsyncConn<Usuario>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<Usuario> ObtenerUsuario(string idUsuario)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDUSUARIO", idUsuario);

            string sql = @"select * from LNUsuario where idUsuario = @IDUSUARIO";

            Consultas.queryAsyncConn<Usuario> objQuery = new Consultas.queryAsyncConn<Usuario>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList().Count > 0 ? existe.AsList()[0] : null;
        }
        public async Task<List<UsuarioInfo>> ObtenerUsuarioAuth(string usuario)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":USUARIO", usuario);

            string sql = @"select LNUsuario.*,LNRol.nombre nombreRol from LNUsuario inner join LNRol on LNUsuario.idRol = LNRol.idRol where LNUsuario.usuario = @USUARIO and estado = 'A'";

            Consultas.queryAsyncConn<UsuarioInfo> objQuery = new Consultas.queryAsyncConn<UsuarioInfo>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<RolInfo> ObtenerRolInfo(string idRol)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDROL", idRol);

            string sql = @"select * from LNRol where idRol = @IDROL";

            Consultas.queryAsyncConn<RolInfo> objQuery = new Consultas.queryAsyncConn<RolInfo>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList().Count > 0 ? existe.AsList()[0] : null;
        }
        public async Task<RolInfo> validaExisteRol(string rol, string idCliente)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":ROL", rol);
            dynamicParameters.Add(":IDCLIENTE", idCliente);

            string sql = @"select * from LNRol where nombre = @ROL and idClienteSistema= @IDCLIENTE";

            Consultas.queryAsyncConn<RolInfo> objQuery = new Consultas.queryAsyncConn<RolInfo>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList().Count > 0 ? existe.AsList()[0] : null;
        }
        public async Task<List<PermisoRol>> Obtenerpermisos(string idRol)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(":IDROL", idRol);

            string sql = @"select * from LNRolPermiso where idRol = @IDROL";

            Consultas.queryAsyncConn<PermisoRol> objQuery = new Consultas.queryAsyncConn<PermisoRol>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<int> AgregaUsuario(UsuarioInfo usuarioInfo, string idClienteSistema)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDCLIENTESISTEMA", idClienteSistema);
                dynamicParameters.Add(":NOMBRE", usuarioInfo.nombre);
                dynamicParameters.Add(":FECHANAC", usuarioInfo.fechaNac);
                dynamicParameters.Add(":CORREO", usuarioInfo.correo);
                dynamicParameters.Add(":NUMERO", usuarioInfo.numero);
                dynamicParameters.Add(":USUARIO", usuarioInfo.usuario);
                dynamicParameters.Add(":PASSWORDUSER", usuarioInfo.passwordUser);
                dynamicParameters.Add(":ESTADO", "A");
                dynamicParameters.Add(":IDROL", usuarioInfo.idRol);

                string Query = @"insert into LNUsuario (IDCLIENTESISTEMA, NOMBRE, FECHANAC, CORREO, NUMERO, USUARIO, PASSWORDUSER, ESTADO, IDROL) values
                                  (@IDCLIENTESISTEMA, @NOMBRE, @FECHANAC, @CORREO, @NUMERO, @USUARIO, @PASSWORDUSER, @ESTADO, @IDROL) SELECT SCOPE_IDENTITY()";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                var lista = await objQuery.QuerySelectAsync(Query, dynamicParameters);
                return lista.AsList().Count > 0 ? lista.AsList()[0] : -1;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> ModificaUsuario(UsuarioInfo usuarioInfo)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDUSUARIO", usuarioInfo.idUsuario);
                dynamicParameters.Add(":NOMBRE", usuarioInfo.nombre);
                dynamicParameters.Add(":FECHANAC", usuarioInfo.fechaNac);
                dynamicParameters.Add(":CORREO", usuarioInfo.correo);
                dynamicParameters.Add(":NUMERO", usuarioInfo.numero);
                dynamicParameters.Add(":USUARIO", usuarioInfo.usuario);
                dynamicParameters.Add(":PASSWORDUSER", usuarioInfo.passwordUser);
                dynamicParameters.Add(":ESTADO", "A");
                dynamicParameters.Add(":IDROL", usuarioInfo.idRol);

                string Query = @"update LNUsuario set  NOMBRE= @NOMBRE, FECHANAC= @FECHANAC, CORREO= @CORREO,
                                  NUMERO= @NUMERO, USUARIO= @USUARIO, PASSWORDUSER= @PASSWORDUSER, IDROL=@IDROL WHERE IDUSUARIO=@IDUSUARIO";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> ModificaUsuarioSinPass(UsuarioInfo usuarioInfo)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDUSUARIO", usuarioInfo.idUsuario);
                dynamicParameters.Add(":NOMBRE", usuarioInfo.nombre);
                dynamicParameters.Add(":FECHANAC", usuarioInfo.fechaNac);
                dynamicParameters.Add(":CORREO", usuarioInfo.correo);
                dynamicParameters.Add(":NUMERO", usuarioInfo.numero);
                dynamicParameters.Add(":USUARIO", usuarioInfo.usuario);
                dynamicParameters.Add(":IDROL", usuarioInfo.idRol);

                string Query = @"update LNUsuario set  NOMBRE= @NOMBRE, FECHANAC= @FECHANAC, CORREO= @CORREO,
                                  NUMERO= @NUMERO, USUARIO= @USUARIO, IDROL=@IDROL WHERE IDUSUARIO=@IDUSUARIO";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> EliminaUsuario(UsuarioInfo usuarioInfo)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDUSUARIO", usuarioInfo.idUsuario);
                

                string Query = @"update LNUsuario set  ESTADO='E' WHERE IDUSUARIO=@IDUSUARIO";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> AgregaRol(RolInfo rolInfo)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDCLIENTESISTEMA", rolInfo.idClienteSistema);
                dynamicParameters.Add(":NOMBRE", rolInfo.nombre);
                dynamicParameters.Add(":DESCRIPCION", rolInfo.descripcion);

                string Query = @"insert into LNRol (IDCLIENTESISTEMA, NOMBRE, DESCRIPCION) values
                                  (@IDCLIENTESISTEMA, @NOMBRE, @DESCRIPCION) SELECT SCOPE_IDENTITY()";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                var lista = await objQuery.QuerySelectAsync(Query, dynamicParameters);
                return lista.AsList().Count > 0 ? lista.AsList()[0] : -1;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> UpdateRol(RolInfo rolInfo)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDCLIENTESISTEMA", rolInfo.idClienteSistema);
                dynamicParameters.Add(":NOMBRE", rolInfo.nombre);
                dynamicParameters.Add(":DESCRIPCION", rolInfo.descripcion);

                string Query = @"update LNRol  set NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION where
                                  idRol= @IDROL ";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> BorrarRol(RolInfo rolInfo)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDROL", rolInfo.idRol);


                string Query = @"delete from LNRol where
                                  idRol= @IDROL ";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> BorrarTodosPermisos(RolInfo rolInfo)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDROL", rolInfo.idRol);
                

                string Query = @"delete from LNRolPermiso where
                                  idRol= @IDROL ";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> AgregaRolPermiso(string idRol, string idLineaNEgocio, string permisos)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDROL", idRol);
                dynamicParameters.Add(":IDLINEANEGOCIO", idLineaNEgocio);
                dynamicParameters.Add(":PERMISOS", permisos);

                string Query = @"insert into LNRolPermiso (IDROL, IDLINEANEGOCIO, PERMISOS) values
                                  (@IDROL, @IDLINEANEGOCIO, @PERMISOS) ";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                return await objQuery.ExecuteAsync(Query, dynamicParameters);
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> InsertaBitacoraAuth(string idUsuario, string ingreso)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":IDUSUARIO", idUsuario);
                dynamicParameters.Add(":INGRESO", ingreso);

                string Query = @"insert into LNBitacoraLogIn (idUsuario, hora, ingreso) values
                                  (@IDUSUARIO, SYSDATETIME(), @INGRESO)";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                var lista = await objQuery.QuerySelectAsync(Query, dynamicParameters);
                return lista.AsList().Count > 0 ? lista.AsList()[0] : -1;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> InsertaClienteSistema(UsuarioClienteNuevo user, string cod)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                // dynamicParameters.Add(":IDROL", rolInfo.idRol);
                dynamicParameters.Add(":NOMBREEMPRESA", user.nombre);
                dynamicParameters.Add(":CORREO", user.correo);
                dynamicParameters.Add(":NUMEROTELEFONO", user.numeroTelefonico);
                dynamicParameters.Add(":CODIGOVERIFICACION", cod);

                string Query = @"insert into LNClienteSistema (NOMBREEMPRESA, CORREO, NUMEROTELEFONO, FECHAREGISTRO, CODIGOVERIFICACION, CODIGOVERIFICADO) values
                                  (@NOMBREEMPRESA, @CORREO, @NUMEROTELEFONO, SYSDATETIME(), @CODIGOVERIFICACION, 'N') SELECT SCOPE_IDENTITY()";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                var lista = await objQuery.QuerySelectAsync(Query, dynamicParameters);
                return lista.AsList().Count > 0 ? lista.AsList()[0] : -1;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> InsertaConfCliente(string idCliente, string precioMensual, string precio3Meses, string precio6MEses, string precioanual, string maxusuarios, string maxlineas, string path1, string path2, string path3, string pathLogo)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDCLIENTESISTEMA", idCliente);
                dynamicParameters.Add(":PRECIOMENSUAL", precioMensual);
                dynamicParameters.Add(":PRECIO3MESES", precio3Meses);
                dynamicParameters.Add(":PRECIO6MESES", precio6MEses);
                dynamicParameters.Add(":PRECIO1AÑO", precioanual);
                dynamicParameters.Add(":NUMMAXUSUARIOS", maxusuarios);
                dynamicParameters.Add(":NUMMAXLINEASNEGOCIO", maxlineas);
                dynamicParameters.Add(":PATHSLIDE1", path1);
                dynamicParameters.Add(":PATHSLIDE2", path2);
                dynamicParameters.Add(":PATHSLIDE3", path3);
                dynamicParameters.Add(":PATHLOGOESQUINA", pathLogo);

                string Query = @"insert into LNConfiguracionCliente (IDCLIENTESISTEMA, PRECIOMENSUAL, PRECIO3MESES, PRECIO6MESES, PRECIO1AÑO, PATHLOGOESQUINA, PATHSLIDE1, PATHSLIDE2, PATHSLIDE3, NUMMAXUSUARIOS, NUMMAXLINEASNEGOCIO) values
                                  (@IDCLIENTESISTEMA, @PRECIOMENSUAL, @PRECIO3MESES, @PRECIO6MESES, @PRECIO1AÑO, @PATHLOGOESQUINA, @PATHSLIDE1, @PATHSLIDE2, @PATHSLIDE3, @NUMMAXUSUARIOS, @NUMMAXLINEASNEGOCIO)";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                var lista = await objQuery.QuerySelectAsync(Query, dynamicParameters);
                return lista.AsList().Count > 0 ? lista.AsList()[0] : -1;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<int> InsertaPeriodoPago(string idCLIENTE)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":IDCLIENTESISTEMA", idCLIENTE);
                

                string Query = @"insert into LNPeriodoPago (IDCLIENTESISTEMA, FECHAINICIO, FECHAFINAL, PRECIOPAGADO, NUMMESES, NOTA, TIPOPERIODO) values
                                  (@IDCLIENTESISTEMA,SYSDATETIME(),dateadd(DD,+36,CAST(CAST(getdate() as date) as datetime)),1000,1,'Periodo de Prueba', 'Prueba')";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                var lista = await objQuery.QuerySelectAsync(Query, dynamicParameters);
                return lista.AsList().Count > 0 ? lista.AsList()[0] : -1;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        public async Task<List<Fecha>> ObtenerFechasDeZonas()
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select FORMAT (DATEADD(hh, -6, getUTCDate()), 'yyyy-MM-dd') as UTCmenosSeis";

            Consultas.queryAsyncConn<Fecha> objQuery = new Consultas.queryAsyncConn<Fecha>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<DiaTexto>> ObtenerDiaPosterior(string cantidadDias)
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"SELECT (CASE DATENAME(dw,FORMAT (DATEADD(hh, -6, getUTCDate()+" + cantidadDias + @"), 'yyyy-MM-dd'))
                             when 'Monday' then 'LUNES'
                             when 'Tuesday' then 'MARTES'
                             when 'Wednesday' then 'MIERCOLES'
                             when 'Thursday' then 'JUEVES'
                             when 'Friday' then 'VIERNES'
                             when 'Saturday' then 'SABADO'
                             when 'Sunday' then 'DOMINGO'
                        END) diasemana, Day(FORMAT (DATEADD(hh, -6, getUTCDate()+" + cantidadDias + @"), 'yyyy-MM-dd')) dia,
                        Month(FORMAT (DATEADD(hh, -6, getUTCDate()+" + cantidadDias + @"), 'yyyy-MM-dd')) mes
                        , Year(FORMAT(DATEADD(hh, -6, getUTCDate() + " + cantidadDias + @"), 'yyyy-MM-dd')) año";

            Consultas.queryAsyncConn<DiaTexto> objQuery = new Consultas.queryAsyncConn<DiaTexto>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<DiaTexto>> ObtenerDiaPosteriorFecha(string cantidadDias, string fecha)
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"SELECT (CASE DATENAME(dw,FORMAT (DATEADD(hh, -0, DATEADD(DD, "+cantidadDias +", '" + fecha + @" 00:00:00.000')), 'yyyy-MM-dd'))
                             when 'Monday' then 'LUNES'
                             when 'Tuesday' then 'MARTES'
                             when 'Wednesday' then 'MIERCOLES'
                             when 'Thursday' then 'JUEVES'
                             when 'Friday' then 'VIERNES'
                             when 'Saturday' then 'SABADO'
                             when 'Sunday' then 'DOMINGO'
                        END) diasemana, Day(FORMAT (DATEADD(hh, -0, DATEADD(DD, " + cantidadDias + ", '" + fecha + @" 00:00:00.000')), 'yyyy-MM-dd')) dia,
                        Month(FORMAT (DATEADD(hh, -0,  DATEADD(DD, " + cantidadDias + ", '" + fecha + @" 00:00:00.000')), 'yyyy-MM-dd')) mes
                        , Year(FORMAT(DATEADD(hh, -0,  DATEADD(DD, " + cantidadDias + ", '" + fecha + @" 00:00:00.000')), 'yyyy-MM-dd')) año";

            Consultas.queryAsyncConn<DiaTexto> objQuery = new Consultas.queryAsyncConn<DiaTexto>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<Disponibilidad>> ObtenerDisponibilidad()
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select * from LNDisponibilidadHorario";

            Consultas.queryAsyncConn<Disponibilidad> objQuery = new Consultas.queryAsyncConn<Disponibilidad>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<RestriccionHorario>> ObtenerRestricciones()
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select FORMAT(fecha, 'dd-MM-yyyy')fecha,hora, dia, sistema, tipo from LNRestriccionHorario";

            Consultas.queryAsyncConn<RestriccionHorario> objQuery = new Consultas.queryAsyncConn<RestriccionHorario>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<RestriccionReuniones>> ObtenerRestriccionesReuniones(string fecha)
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select FORMAT(fecha, 'dd-MM-yyyy')fecha, hora from LNReunion where sigueEnPie='S' and fecha >= '" + fecha + "'";

            Consultas.queryAsyncConn<RestriccionReuniones> objQuery = new Consultas.queryAsyncConn<RestriccionReuniones>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<List<RestriccionReuniones>> ObtenerRestriccionesReuniones()
        {
            var dynamicParameters = new DynamicParameters();

            string sql = @"select FORMAT(fecha, 'dd-MM-yyyy')fecha, hora from LNReunion where sigueEnPie='S' and fecha >= DATEADD(hh, -6, getUTCDate())";

            Consultas.queryAsyncConn<RestriccionReuniones> objQuery = new Consultas.queryAsyncConn<RestriccionReuniones>(_conn, transaction);
            var existe = await objQuery.QuerySelectAsync(sql, dynamicParameters);
            return existe.AsList();
        }
        public async Task<int> InsertaReunionNueva(PeticionLlamada peticion)
        {
            try
            {
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add(":fecha", peticion.fecha);
                dynamicParameters.Add(":hora", peticion.hora);
                dynamicParameters.Add(":sistema", peticion.sistema);
                dynamicParameters.Add(":nombrecompleto", peticion.nombrecompleto);
                dynamicParameters.Add(":email", peticion.email);
                dynamicParameters.Add(":empresa", peticion.empresa);
                dynamicParameters.Add(":pais", peticion.pais);
                dynamicParameters.Add(":numerowhatsapp", peticion.numerowhatsapp);
                dynamicParameters.Add(":descripcion", peticion.descripcion);
                dynamicParameters.Add(":llevoACabo", 'N');
                dynamicParameters.Add(":sigueEnPie", 'S');


                string Query = @"insert into LNReunion (fecha, hora, sistema, nombrecompleto, email, empresa, pais, numerowhatsapp, descripcion,fechaRegistro, llevoACabo, sigueEnPie) values
                                  (@fecha, @hora, @sistema, @nombrecompleto, @email, @empresa, @pais, @numerowhatsapp, @descripcion,SYSDATETIME(), @llevoACabo, @sigueEnPie)";

                Consultas.queryAsyncConn<int> objQuery = new Consultas.queryAsyncConn<int>(_conn, transaction);
                var lista = await objQuery.QuerySelectAsync(Query, dynamicParameters);
                return lista.AsList().Count > 0 ? lista.AsList()[0] : -1;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
    }
}
