using Core.Modelos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Servicios.Interfaces
{
    public interface IIA
    {
        Task<ResponseIA> AplicarInteligenciaArtificial(RequestImageIA request);
    }
}
