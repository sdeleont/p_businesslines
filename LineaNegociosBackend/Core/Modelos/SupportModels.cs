using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class SupportModels
    {
    }
    public class PeticionSoporte {
        public PeticionSoporte(string idCLiente, string idUsuario, string mensaje, string tipo) {
            this.idClienteSistema = idCLiente;
            this.idUsuario = idUsuario;
            this.mensaje = mensaje;
            this.tipo = tipo;
            this.Archivos = new List<Microsoft.AspNetCore.Http.IFormFile>();
        }
        public string idClienteSistema { get; set; }
        public string idUsuario { get; set; }
        public string mensaje { get; set; }
        public string tipo { get; set; }
        public List<Microsoft.AspNetCore.Http.IFormFile>  Archivos { get; set; }
    }
    public class ResponseSupport {
        public string status { get; set; }
        public string mensaje { get; set; }
        public string ticket { get; set; }
    }

    public class ResponseMensajes {
        public string idMensaje { get; set; }
        public string idCaso { get; set; }
        public string numOrden { get; set; }
        public string mensaje { get; set; }
        public string fecha { get; set; }
        public string leido { get; set; }
    }
    public class ResponseInfoMensaje {
        public string idCaso { get; set; }
        public string mensaje { get; set; }
        public string tipo { get; set; }
        public string estado { get; set; }
        public string fecha { get; set; }
        public List<ObjetoImagenDatos> imagenes { get; set; }
    }
    public class ImagenesRespuesta {
        public string pathImg { get; set; }
    }
}
