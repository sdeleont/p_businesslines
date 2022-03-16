using Core.Modelos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Servicios.Interfaces
{
    public interface ILineaNegocio
    {
        Task<ResponseAnalisisFile> Analiza(InformacionLineaNegocio infoLineaNegocio);
        Task<List<TipoNegocio>> ObtenerTiposNegocio();
        Task<List<TiposDatoD>> ObtenerTiposDato();
        Task<List<LineaNegocioGeneral>> ObtenerLineasNegocio(string idCliente);
        Task<ConfiguracionLinea> ObtenerLineaNegocioDatos(BusquedaData busqueda, string idLinea, string inicioPuntero, string finPuntero);
        Task<List<object>> GetDataRegistroID(string idLinea, string idRegistro);
        Task<ResponseLineaNegocio> NuevaLinea(LineaNegocioModel lineaNegocio);
        Task<ResponseOperacionLineaNegocio> OperacionLineaNegocio(OperacionLineaNegocio operacion);
        Task<ResponseOperacionLineaNegocio> OperacionSobreRegistro(OperacionData operacion);
        Task<ResponseOperacionLineaNegocio> SetImage(RequestConImagen req);
        Task<ResponseOperacionLineaNegocio> DelImage(RequestOpImagen req);
        Task<ResponseImagenesProducto> GetImagesID(string idLinea, string valorLLave);
        Task<List<ResponseDatosPorColumna>> GetDataColumnaG1(string idLinea, string columna, int numDatos);
        Task<List<object>> GetDataGrafica1(RequestDataGrafica1 request);
        Task<ResponseOperacionLineaNegocio> SetImgCliente(RequestConImagen req, string idCliente, string tipo);
        Task<ResponseConfiguracionesCLiente> GetImagesConfCliente(string idCliente);
    }
}
