using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class InformacionLineaNegocio
    {
        public Microsoft.AspNetCore.Http.IFormFile Archivo { get; set; }
        public string idCliente { get; set; }
        public string nombreLineaNegocio { get; set; }
        public string descripcion { get; set; }
        public string idTipoNegocio { get; set; }
        public List<ColumnaLineaNegocio> columnas { get; set; }
    }
    public class ColumnaLineaNegocio {
        public string nombreCampo { get; set; }
        public string descripcionCampo { get; set; }
        public string obligatorio { get; set; }
        public string campoAgrupador { get; set; }
    }
}
