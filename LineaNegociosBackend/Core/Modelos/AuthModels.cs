using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class AuthModels
    {
    }
    public class Rol {
        public string idRol { get; set; }
        public string idClienteSistema { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
    }

    public class Usuario {
        public string idUsuario { get; set; }
        public string idClienteSistema { get; set; }
        public string nombre { get; set; }
        public string fechaNac { get; set; }
        public string correo { get; set; }
        public string numero { get; set; }
        public string usuario { get; set; }
        public string idRol { get; set; }
        public object imagenFile { get; set; }
        public byte[] bytes { get; set; }
    }
    public class UsuarioInfo
    {
        public string idUsuario { get; set; }
        public string idClienteSistema { get; set; }
        public string nombre { get; set; }
        public string fechaNac { get; set; }
        public string correo { get; set; }
        public string numero { get; set; }
        public string usuario { get; set; }
        public string passwordUser { get; set; }
        public string idRol { get; set; }
        public string nombreRol { get; set; }
    }
    public class RolInfo
    {
        public string idRol { get; set; }
        public string idClienteSistema { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public List<PermisoRol> permisos { get; set; }
    }
    public class PermisoRol {
        public string idRol { get; set; }
        public string idLineaNegocio { get; set; }
        public string permisos { get; set; }
    }
    public class ResponseOperacionRol {
        public string status { get; set; }
        public string mensaje { get; set; }
        public string operacion { get; set; }
    }
    public class RequestRolBusqueda {
        public string busqueda { get; set; }
        public string idCliente { get; set; }
    }
    public class RequestUsuarioBusqueda {
        public string busqueda { get; set; }
        public string idCliente { get; set; }
    }
    public class Autenticacion {
        public string usuario { get; set; }
        public string password { get; set; }
    }
    public class ResponseAutenticacion {
        public string status { get; set; }
        public string mensaje { get; set; }
        public string token { get; set; }
        public InformacionUsuario usuario { get; set; }
    }
    public class InformacionUsuario {
        public string nombre { get; set; }
        public string idUsuario { get; set; }
        public string idCliente { get; set; }
        public string correo { get; set; }
        public string usuario { get; set; }
        public string idRol { get; set; }
        public string nombreRol { get; set; }
        public List<PermisoRol> permisos { get; set; }
    }
    public class UsuarioClienteNuevo {
        public string nombre { get; set; }
        public string correo { get; set; }
        public string numeroTelefonico { get; set; }
        public string password { get; set; }
    }
    public class ValidaCodigo {
        public string idCliente { get; set; }
        public string codigo { get; set; }
        public string operacion { get; set; }
    }
    public class ClSistema {
        public string idClienteSistema { get; set; }
        public string correo { get; set; }
        public string numeroTelefono { get; set; }
        public string codigoverificacion { get; set; }
        public string codigoverificado { get; set; }
    }
}
