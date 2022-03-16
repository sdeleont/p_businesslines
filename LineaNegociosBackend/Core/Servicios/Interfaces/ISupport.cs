using Core.Modelos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Servicios.Interfaces
{
    public interface ISupport
    {
        Task<ResponseSupport> NuevoTicket(PeticionSoporte req);
        Task<List<ResponseMensajes>> GetMensajes(string idUsuario);
        Task<ResponseSupport> SetMensajeLiedo(string idMensaje);
        Task<ResponseInfoMensaje> GetInfoMensaje(string idMensaje);
    }
}
