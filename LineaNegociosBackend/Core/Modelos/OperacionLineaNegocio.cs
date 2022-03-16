using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class OperacionLineaNegocio
    {
        public string idLineaNegocio { get; set; }
        public string idClienteSistema { get; set; }
        public string operacion { get; set; }
        public string valor { get; set; }
    }
    public class ResponseOperacionLineaNegocio
    {
        public string status { get; set; }
        public string mensaje { get; set; }
        public string operacion { get; set; }
    }
    public class ResponseImagenesProducto {
        public ResponseImagenesProducto() {
            this.imagenes = new List<ObjetoImagenDatos>();
        }
        public List<ObjetoImagenDatos>  imagenes { get; set; }
        public string status { get; set; }
        public string mensaje { get; set; }
    }
    public class ObjetoImagenDatos {
        public object imagenFile { get; set; }
        public  byte [] bytes { get; set; }
        public string guid { get; set; }
        public string tipo { get; set; }
    }
    public class LineaNegocioTablaInformacion {
        public string idLineaNegocio { get; set; }
        public string valorLlavePrimaria { get; set; }
        public string pathImg { get; set; }
    }
    public class ResponseDatosPorColumna {
        public string dato { get; set; }
    }
    public class ResponseConfiguracionesCLiente {
        public string status { get; set; }
        public string mensaje { get; set; }
        public string idClienteSistema { get; set; }
        public string numMaxUsuarios { get; set; }
        public string numMaxLineasNegocio { get; set; }
        public string tipoPeriodo { get; set; }
        public List<ObjetoImagenDatos> imagenes { get; set; } //icono, slide1, slide2, slide 3
    }
    public class ConfClientCompleta {
        public string idClienteSistema { get; set; }
        public string pathLogoEsquina { get; set; }
        public string pathSlide1 { get; set; }
        public string pathSlide2 { get; set; }
        public string pathSlide3 { get; set; }
        public string numMaxUsuarios { get; set; }
        public string numMaxLineasNegocio { get; set; }
        public string tipoPeriodo { get; set; }
    }

}
