using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class OperacionData
    {
        public string idLineaNegocio { get; set; }
        public string valor { get; set; }
        public string operacion { get; set; }
        public string idLlavePrimaria { get; set; }
        public string valorLlavePrimaria { get; set; }
        public List<Valores> valores { get; set; }
    }
    public class Valores { 
        public string campotabla { get; set; }
        public string valor { get; set; }
    }

    public class BusquedaData { 
        public string texto { get; set; }
    }
    public class RequestConImagen {
        public string idLinea { get; set; }
        public string valorLlave { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile Archivo { get; set; }
    }
    public class RequestOpImagen
    {
        public string idLinea { get; set; }
        public string valorLlave { get; set; }
        public string guid { get; set; }
    }
    public class RequestDataGrafica1 {
        public string idLinea { get; set; }
        public string campoPivot { get; set; }
        public List<string> valoresPivot { get; set; }
        public List<string> valoresCamposNumericos { get; set; }
    }
}
