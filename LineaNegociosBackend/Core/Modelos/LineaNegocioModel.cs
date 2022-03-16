using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class LineaNegocioModel
    {
        public LineaNegocioModel(string idClienteSistema, string nombre, string descripcion, string formaMuestra, string tipoNegocio) {
            this.idClienteSistema = idClienteSistema;
            this.idTipoNegocio = tipoNegocio;
            this.nombre = nombre;
            this.descripcion = descripcion;
            this.formaMuestra = formaMuestra;
            this.campos = new List<CampoLineaNegocio>();
        }
        public string idLineaNegocio { get; set; }
        public string idClienteSistema { get; set; }
        public string idTipoNegocio { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string formaMuestra { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile Archivo { get; set; }
        public List<CampoLineaNegocio> campos { get; set; }
    }
    public class CampoLineaNegocio {
        public CampoLineaNegocio(string idTipodato, string nombreCampo, string descripcionCampo, bool obligatorio, bool esCampoAgrupador, string llavePrimaria) {
            this.idTipodato = idTipodato;
            this.nombreCampo = nombreCampo;
            this.descripcionCampo = descripcionCampo;
            this.obligatorio = obligatorio;
            this.esCampoAgrupador = esCampoAgrupador;
            this.esLlavePrimaria = llavePrimaria;
        }
        public string idTipodato { get; set; }
        public string nombreCampo { get; set; }
        public string descripcionCampo { get; set; }
        public bool obligatorio { get; set; }
        public bool esCampoAgrupador { get; set; }
        public string esLlavePrimaria { get; set; }
    }
    public class LineaNegocioNombreTabla {
        public string tablaInformacion { get; set; }
    }
    public class InformacionMDParaInsertar
    {
        public string idLineaNegocio { get; set; }
        public string idTipoNegocio { get; set; }
        public string tablaInformacion { get; set; }
        public string nombreCampo { get; set; }
        public string obligatorio { get; set; }
        public string descripcionCampo { get; set; }
        public string ER { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string TipoEnBD { get; set; }
        public string orden { get; set; }
    }
    public class ParametroDatoBD
    {
        public ParametroDatoBD(string parametro, string dato)
        {
            this.parametro = parametro;
            this.dato = dato;
        }
        public string parametro { get; set; }
        public string dato { get; set; }
    }
    public class LineaNegocioGeneral
    {
        public string idLineaNegocio { get; set; }
        public string idClienteSistema { get; set; }
        public string nombreNegocio { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string formaMuestra { get; set; }
        public string icono { get; set; }
        public List<CampoLineaGeneral> campos { get; set; }
    }
    public class CampoLineaGeneral
    {
     
        public string nombreCampo { get; set; }
        public string descripcionCampo { get; set; }
        public string obligatorio { get; set; }
        public string esCampoAgrupador { get; set; }
    }
    public class DataLineaNegocio {
        public List<object> data { get; set; } 
    }

    public class ConfiguracionLinea {
        public string idLineaNegocio { get; set; }
        public string idClienteSistema { get; set; }
        public string nombreNegocio { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string formaMuestra { get; set; }
        public string icono { get; set; }
        public string tabla { get; set; }
        public List<CampoLineaNegocioCompleto> campos { get; set; }
        public List<object> data { get; set; }
        public string totalItems { get; set; }
    }
    public class CampoLineaNegocioCompleto
    {
        public string nombreTipoDato { get; set; }
        public string descTipoDato { get; set; }
        public string idTipoDato { get; set; }
        public string nombreCampo { get; set; }
        public string descripcionCampo { get; set; }
        public string obligatorio { get; set; }
        public string esCampoAgrupador { get; set; }
        public string orden { get; set; }
        public string esLlavePrimaria { get; set; }
        public string campoTabla { get; set; }
        public string er { get; set; }
        public string tipoenbd { get; set; }
        public string campoEnIA { get; set; }
    }
}
