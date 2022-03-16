using Core.Modelos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Servicios.Interfaces
{
    public interface IAuth
    {
        Task<List<Rol>> ObtenerRoles(string idCliente);
        Task<RolInfo> ObtenerRolInfo(string idRol);
        Task<List<Rol>> BuscaRol(RequestRolBusqueda req);
        Task<ResponseOperacionRol> OperacionRol(RolInfo rolInfo, string idCliente, string operacion);
        Task<ResponseOperacionRol> OperacionUsuario(UsuarioInfo userInfo, string idCliente, string operacion);
        Task<ResponseOperacionRol> ValidaCodigo(ValidaCodigo info);
        Task<ResponseOperacionRol> NuevoUsuarioSistema(UsuarioClienteNuevo user);
        Task<List<Usuario>> BuscaUsuario(RequestUsuarioBusqueda req);
        Task<Usuario> GetUserInfo(string idUsuario);
        Task<ResponseAutenticacion> AutenticaUsuario(Autenticacion usuario);
        Task<RespuestaDisponibilidad> ObtenerHorarios(PeticionHorario info);
        Task<ResponseLlamada> AgendarLlamada(PeticionLlamada info);
    }
}
