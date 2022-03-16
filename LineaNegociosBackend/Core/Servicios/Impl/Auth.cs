using Core.Modelos;
using Core.Servicios.Interfaces;
using Core.Utiles;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Core.Servicios.Impl
{
    public class Auth: IAuth
    {
        private IDbConnection conexionMD;
        private IDbTransaction transaction;
        public ConexionConfig conf;
        public Auth(ConexionConfig conf)
        {
            this.conf = conf;
        }
        public Auth(IDbConnection conn, IDbTransaction transaction)
        {
            this.conexionMD = conn;
            this.transaction = transaction;
        }
        public async Task<List<Rol>> ObtenerRoles(string idCliente)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        List<Rol> roles = new List<Rol>();
                        Repositorios.Auth repo = new Repositorios.Auth(_conn);
                        roles = await repo.ObtenerRoles(idCliente);
                        _conn.Close();
                        return roles;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<RolInfo> ObtenerRolInfo(string idRol)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        RolInfo rol = new RolInfo();
                        Repositorios.Auth repo = new Repositorios.Auth(_conn);
                        rol = await repo.ObtenerRolInfo(idRol);
                        if (rol != null) {
                            rol.permisos = await repo.Obtenerpermisos(idRol);
                        }
                        _conn.Close();
                        return rol;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<List<Rol>> BuscaRol(RequestRolBusqueda req)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        List<Rol> roles = new List<Rol>();
                        Repositorios.Auth repo = new Repositorios.Auth(_conn);
                        roles = await repo.ObtenerRolesBusqueda(req.idCliente, req.busqueda);
                        _conn.Close();
                        return roles;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResponseOperacionRol> OperacionRol(RolInfo rolInfo, string idCliente, string operacion)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            ResponseOperacionRol response = new ResponseOperacionRol();
                            response.status = "Error";
                            Repositorios.Auth repo = new Repositorios.Auth(_conn, transaction);
                            if (operacion.Equals("agregar"))
                            {
                                RolInfo existente = await repo.validaExisteRol(rolInfo.nombre, idCliente);
                                if (existente != null)
                                {
                                    response.mensaje = "Ya existe un rol con este nombre";
                                } else if (rolInfo.nombre.Trim().ToLower().Equals("administrador")) {
                                    response.mensaje = "No puede crear otro rol llamado Administrador";
                                } else {
                                    int idRol = await repo.AgregaRol(rolInfo);
                                    foreach (PermisoRol permiso in rolInfo.permisos)
                                    {
                                        await repo.AgregaRolPermiso(idRol.ToString(), permiso.idLineaNegocio, permiso.permisos);
                                    }
                                    response.status = "OK";
                                    response.mensaje = "Agregado con exito";
                                    response.operacion = "Agrego";
                                    transaction.Commit();
                                }
                            }
                            else if (operacion.Equals("modificar"))
                            {
                                await repo.UpdateRol(rolInfo);
                                await repo.BorrarTodosPermisos(rolInfo);
                                foreach (PermisoRol permiso in rolInfo.permisos)
                                {
                                    await repo.AgregaRolPermiso(rolInfo.idRol, permiso.idLineaNegocio, permiso.permisos);
                                }
                                response.status = "OK";
                                response.mensaje = "Actualizado con exito";
                                response.operacion = "Actualizacion";
                                transaction.Commit();
                            }
                            else if (operacion.Equals("eliminar"))
                            {
                                List<Usuario> existente = await repo.ValidarUsuarioConRol(rolInfo.idRol);
                                if (existente.Count > 0)
                                {
                                    response.mensaje = "Este rol tiene usuarios asociados, elimine primero los usuarios";
                                }
                                else {
                                    await repo.BorrarRol(rolInfo);
                                    await repo.BorrarTodosPermisos(rolInfo);
                                    response.status = "OK";
                                    response.mensaje = "Eliminado con exito";
                                    response.operacion = "Eliminacion";
                                    transaction.Commit();
                                }
                                
                            }
                            
                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResponseOperacionRol> OperacionUsuario(UsuarioInfo userInfo, string idCliente, string operacion)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            ResponseOperacionRol response = new ResponseOperacionRol();
                            response.status = "Error";
                            Repositorios.Auth repo = new Repositorios.Auth(_conn, transaction);
                            if (operacion.Equals("agregar"))
                            {
                                List<Usuario> existente = await repo.ValidarUsuarioExistente(userInfo.usuario);
                                if (existente.Count > 0)
                                {
                                    response.mensaje = "Ya existe este nombre de usuario";
                                }
                                else {
                                    userInfo.passwordUser = Encriptador.Encriptar(userInfo.passwordUser);
                                    int idRol = await repo.AgregaUsuario(userInfo, idCliente);
                                    response.status = "OK";
                                    response.mensaje = "Agregado con exito";
                                    response.operacion = "Agrego";
                                    transaction.Commit();
                                }
                            }
                            else if (operacion.Equals("modificar"))
                            {
                                if (userInfo.passwordUser.Trim().Equals(""))
                                {
                                    await repo.ModificaUsuarioSinPass(userInfo);
                                }
                                else
                                {
                                    userInfo.passwordUser = Encriptador.Encriptar(userInfo.passwordUser);
                                    await repo.ModificaUsuario(userInfo);
                                }

                                response.status = "OK";
                                response.mensaje = "Actualizado con exito";
                                response.operacion = "Actualizacion";
                                transaction.Commit();
                            }
                            else if (operacion.Equals("eliminar"))
                            {
                                await repo.EliminaUsuario(userInfo);
                                response.status = "OK";
                                response.mensaje = "Eliminado con exito";
                                response.operacion = "Eliminacion";
                                transaction.Commit();
                            }

                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResponseOperacionRol> ValidaCodigo(ValidaCodigo info)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            ResponseOperacionRol response = new ResponseOperacionRol();
                            response.status = "Error";
                            Repositorios.Auth repo = new Repositorios.Auth(_conn, transaction);
                            List<ClSistema> clSistema = await repo.GetInfoClienteSistemaCodigo(info.idCliente);
                            if (clSistema.Count > 0) {
                                if (info.operacion.Equals("verifica")) {
                                    if (info.codigo.Trim().Equals(clSistema[0].codigoverificacion)) {
                                        await repo.SetVerificado(info.idCliente);
                                        response.status = "OK";
                                        response.mensaje = "Verificado exitosamente";
                                        transaction.Commit();
                                    }
                                }
                                else if (info.operacion.Equals("reenviar")) {
                                    var ran = new Random();
                                    int cod = ran.Next(1000, 9999);
                                    await repo.ActualizaCodVerificacion(info.idCliente, cod.ToString());
                                    await SendEmail(cod.ToString(), clSistema[0].correo);
                                    response.status = "OK";
                                    response.mensaje = "Reenviado con exito";
                                    transaction.Commit();
                                }
                                else if (info.operacion.Equals("valida"))
                                {
                                    if (clSistema[0].codigoverificado.Equals("S")) {
                                        response.status = "OK";
                                        response.mensaje = "Verificado exitosamente";
                                    } else if (clSistema[0].codigoverificado.Equals("N")) {
                                        response.status = "NOVERIFICADO";
                                        response.mensaje = "";
                                    }
                                }
                            }

                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResponseOperacionRol> NuevoUsuarioSistema(UsuarioClienteNuevo user)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            ResponseOperacionRol response = new ResponseOperacionRol();
                            response.status = "Error";
                            Repositorios.Auth repo = new Repositorios.Auth(_conn, transaction);
                            var ran = new Random();
                            int cod = ran.Next(1000, 9999);
                            List<Usuario> existente = await repo.ValidarUsuarioExistente(user.correo);
                            if (existente.Count > 0)
                            {
                                response.mensaje = "Ya existe un usuario con este correo, porfavor cambie el usuario";
                            }
                            else
                            {
                                int ID = await repo.InsertaClienteSistema(user, cod.ToString());
                                if (ID > 0)
                                {
                                    await repo.InsertaConfCliente(ID.ToString(), "50", "150", "300", "1000", "50", "12", "6dd1f54e-180f-4877-8c01-85f805d9816a.png", "85d4a7ba-6b78-4350-8055-dfd74b63225a.png", "72e44150-3256-4527-92bb-3d5d5ad53eb7.png", "0eed572b-d6eb-430a-95eb-cb8ababb51d4.png");
                                    await repo.InsertaPeriodoPago(ID.ToString());
                                    RolInfo rolInfo = new RolInfo();
                                    rolInfo.idClienteSistema = ID.ToString();
                                    rolInfo.nombre = "Administrador";
                                    rolInfo.descripcion = "Administra el sistema completo";
                                    int idROL = await repo.AgregaRol(rolInfo);
                                    UsuarioInfo usuarioInfo = new UsuarioInfo();
                                    usuarioInfo.idClienteSistema = ID.ToString();
                                    usuarioInfo.nombre = user.nombre;
                                    usuarioInfo.fechaNac = "01/01/2021";
                                    usuarioInfo.correo = user.correo;
                                    usuarioInfo.numero = user.numeroTelefonico;
                                    usuarioInfo.usuario = user.correo;
                                    usuarioInfo.passwordUser = Encriptador.Encriptar(user.password);
                                    usuarioInfo.idRol = idROL.ToString();
                                    await repo.AgregaUsuario(usuarioInfo, ID.ToString());
                                    // await SendEmail(cod.ToString());
                                    transaction.Commit();
                                    response.status = "OK";
                                    response.mensaje = "Registrado con exito";
                                }
                            }
                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<int> SendEmail(string mensaje, string email)
        {
            try
            {
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("BlueIACloud",
                "Secret key");
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress("User",
                email);
                message.To.Add(to);

                message.Subject = "Codigo de Verificación para cuenta nueva";

                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = "<h3>Tu codigo de verificación es: " + mensaje + "</h3>";
                // bodyBuilder.TextBody = "Hello World!";
                message.Body = bodyBuilder.ToMessageBody();

                SmtpClient client = new SmtpClient();
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("Secret key", "Secret key");
                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
                return 1;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<List<Usuario>> BuscaUsuario(RequestUsuarioBusqueda req)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        List<Usuario> roles = new List<Usuario>();
                        Repositorios.Auth repo = new Repositorios.Auth(_conn);
                        roles = await repo.ObtenerUsuariosBusqueda(req.idCliente, req.busqueda);
                        _conn.Close();
                        return roles;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<Usuario> GetUserInfo(string idUsuario)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    try
                    {
                        Usuario usuario = new Usuario();
                        Repositorios.Auth repo = new Repositorios.Auth(_conn);
                        usuario = await repo.ObtenerUsuario(idUsuario);
                        _conn.Close();
                        return usuario;
                    }
                    catch (Exception ex)
                    {
                        _conn.Close();
                        throw new Exception(ex.Message);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResponseAutenticacion> AutenticaUsuario(Autenticacion usuario)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            ResponseAutenticacion response = new ResponseAutenticacion();
                            response.status = "Error";
                            Repositorios.Auth repo = new Repositorios.Auth(_conn, transaction);
                            List<UsuarioInfo> user = await repo.ObtenerUsuarioAuth(usuario.usuario);
                            if (user != null && user.Count == 1)
                            {
                                string contra = Encriptador.Encriptar(usuario.password);
                                // string contra = usuario.password;
                                if (contra.Equals(user[0].passwordUser))
                                { //autentico con exito
                                    response.status = "OK";
                                    response.mensaje = "Autenticación Correcta";
                                    InformacionUsuario infoUsr = new InformacionUsuario();
                                    infoUsr.nombre = user[0].nombre;
                                    infoUsr.idCliente = user[0].idClienteSistema;
                                    infoUsr.idRol = user[0].idRol;
                                    infoUsr.idUsuario = user[0].idUsuario;
                                    infoUsr.usuario = user[0].usuario;
                                    infoUsr.correo = user[0].correo;
                                    infoUsr.nombreRol = user[0].nombreRol;
                                    infoUsr.permisos = await repo.Obtenerpermisos(infoUsr.idRol);
                                    response.usuario = infoUsr;
                                    await repo.InsertaBitacoraAuth(infoUsr.idUsuario, "OK");
                                }
                                else
                                {
                                    response.mensaje = "Contraseña invalida";
                                    await repo.InsertaBitacoraAuth(user[0].idUsuario, "ERRCONTRA");
                                }
                            }
                            else {
                                response.mensaje = "Usuario no Existe";
                                await repo.InsertaBitacoraAuth("-1", "ERRENCONT");
                            }
                            transaction.Commit();

                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<RespuestaDisponibilidad> ObtenerHorarios(PeticionHorario info)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            RespuestaDisponibilidad response = new RespuestaDisponibilidad();
                            Repositorios.Auth repo = new Repositorios.Auth(_conn, transaction);
                            response.dias = new List<Dia>();
                            List<Disponibilidad> disponibilidadList = await repo.ObtenerDisponibilidad();
                            List<RestriccionHorario> restricciones = await repo.ObtenerRestricciones();
                            List<RestriccionReuniones> reunionesProgramadas;
                            if (info.diaPos.Equals("") && info.mesPos.Equals("") && info.añoPos.Equals(""))
                            {
                                reunionesProgramadas = await repo.ObtenerRestriccionesReuniones();
                            }
                            else {
                                reunionesProgramadas = await repo.ObtenerRestriccionesReuniones(info.mesPos + "/" + info.diaPos + "/" + info.añoPos);
                            }

                            
                            for (int i = 0; i < 7; i++) {
                                Dia dia = new Dia();

                                List<DiaTexto> diaBusqueda;
                                if (info.diaPos.Equals("") && info.mesPos.Equals("") && info.añoPos.Equals(""))
                                {
                                    diaBusqueda = await repo.ObtenerDiaPosterior(i.ToString());
                                }
                                else {
                                    diaBusqueda = await repo.ObtenerDiaPosteriorFecha(i.ToString(), info.mesPos + "/" + info.diaPos + "/" + info.añoPos);
                                }
                                
                                if (diaBusqueda.Count > 0) {
                                    dia.nombre = diaBusqueda[0].diasemana;
                                    dia.dia = diaBusqueda[0].dia;
                                    dia.mes = diaBusqueda[0].mes;
                                    dia.año = diaBusqueda[0].año;
                                    dia.esHoy = (i == 0) ? "S" : "N";
                                    dia.diaDisponible = "S";
                                    dia.horarios = new List<HorarioDia>();
                                    foreach (Disponibilidad disp in disponibilidadList) {
                                        if (disp.dia.ToUpper().Equals(dia.nombre)) {
                                            HorarioDia horarioDia = new HorarioDia();
                                            horarioDia.disponibilidad = "S";
                                            horarioDia.hora = disp.hora;
                                            dia.horarios.Add(horarioDia);
                                        }
                                    }
                                    response.dias.Add(dia);
                                    // recorro todas las restricciones
                                    for (int j = 0; j < restricciones.Count; j++) {
                                        if (restricciones[j].tipo.Equals("dia"))
                                        {
                                            string dateInput = restricciones[j].fecha;
                                            var parsedDate = DateTime.ParseExact(dateInput, "dd-MM-yyyy",CultureInfo.InvariantCulture);
                                            //var parsedDate = Convert.ToDateTime(dateInput);
                                            if ((Convert.ToInt32(parsedDate.Day) == Convert.ToInt32(diaBusqueda[0].dia)) && 
                                                (Convert.ToInt32(parsedDate.Month) == Convert.ToInt32(diaBusqueda[0].mes)) &&
                                                (Convert.ToInt32(parsedDate.Year) == Convert.ToInt32(diaBusqueda[0].año))) {
                                                dia.diaDisponible = "N";
                                            }
                                        } else if (restricciones[j].tipo.Equals("hora")) {
                                            string dateInput = restricciones[j].fecha;
                                            var parsedDate = DateTime.ParseExact(dateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                            //var parsedDate = Convert.ToDateTime(dateInput);
                                            if ((Convert.ToInt32(parsedDate.Day) == Convert.ToInt32(diaBusqueda[0].dia)) &&
                                                (Convert.ToInt32(parsedDate.Month) == Convert.ToInt32(diaBusqueda[0].mes)) &&
                                                (Convert.ToInt32(parsedDate.Year) == Convert.ToInt32(diaBusqueda[0].año)))
                                            {
                                                foreach (HorarioDia horario in dia.horarios) {
                                                    if (horario.hora.Equals(restricciones[j].hora)) {
                                                        horario.disponibilidad = "N";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    for (int a = 0; a < reunionesProgramadas.Count; a++) {
                                        string dateInput = reunionesProgramadas[a].fecha;
                                        //var parsedDate = Convert.ToDateTime(dateInput);
                                        var parsedDate = DateTime.ParseExact(dateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                        if ((Convert.ToInt32(parsedDate.Day) == Convert.ToInt32(diaBusqueda[0].dia)) &&
                                            (Convert.ToInt32(parsedDate.Month) == Convert.ToInt32(diaBusqueda[0].mes)) &&
                                            (Convert.ToInt32(parsedDate.Year) == Convert.ToInt32(diaBusqueda[0].año)))
                                        {
                                            foreach (HorarioDia horario in dia.horarios)
                                            {
                                                if (horario.hora.Equals(reunionesProgramadas[a].hora))
                                                {
                                                    horario.disponibilidad = "N";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            

                            transaction.Commit();
                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<ResponseLlamada> AgendarLlamada(PeticionLlamada info)
        {
            try
            {
                using (IDbConnection _conn = new SqlConnection(conf.SQLServerPool))
                {
                    _conn.Open();
                    using (var transaction = _conn.BeginTransaction())
                    {
                        try
                        {
                            ResponseLlamada response = new ResponseLlamada();
                            response.status = "Error";
                            Repositorios.Auth repo = new Repositorios.Auth(_conn, transaction);
                            await repo.InsertaReunionNueva(info);
                            transaction.Commit();
                            response.status = "OK";
                            response.mensaje = "Reunion Agendada Satisfactoriamente";
                            try {
                                await SendEmailReunion("Se agendo una nueva reunion con " + info.empresa + ", Fecha: " + info.fecha + ", Hora:" + info.hora + ", Whatsapp: " + info.numerowhatsapp, "Secret key");
                            }
                            catch (Exception ee) { 
                            
                            }
                            _conn.Close();
                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _conn.Close();
                            throw new Exception(ex.Message);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<int> SendEmailReunion(string mensaje, string email)
        {
            try
            {
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("BlueIACloud",
                "Secret key");
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress("User",
                email);
                message.To.Add(to);

                message.Subject = "Reunión con Cliente Programada";

                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = "<h3>" + mensaje + "</h3>";
                // bodyBuilder.TextBody = "Hello World!";
                message.Body = bodyBuilder.ToMessageBody();

                SmtpClient client = new SmtpClient();
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("Secret key", "Secret key");
                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
                return 1;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
