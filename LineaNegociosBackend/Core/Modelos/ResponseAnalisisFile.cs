using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class ResponseAnalisisFile
    {
        public ResponseAnalisisFile(string status, string mensaje) {
            this.status = status;
            this.mensaje = mensaje;
        }
        public string status { get; set; }
        public string mensaje { get; set; }
        public List<ColumnaAnalizada> columnas { get; set; }
        public int columnasEncontradas { get; set; }
        public int registrosEncontrados { get; set; }
        public int lineasVacias { get; set; }
    }
    public class ColumnaAnalizada {
        public ColumnaAnalizada(string nombre) {
            this.nombre = nombre;
        }
        public string nombre { get; set; }
        public string numVariantes { get; set; }
        public string tipoDatoSugerido { get; set; }
    }

    public class TipoNegocio {
        public string idTipoNegocio { get; set; }
        public string nombre { get; set; }
    }
    public class TiposDatoD {
        public string idTipoDato { get; set; }
        public string descripcion { get; set; }
    }
    public class TipoDatoBD
    {
        public string idTipoDato { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string TipoEnBD { get; set; }
        public string ER { get; set; }
    }
    public class ResponseLineaNegocio {
        public ResponseLineaNegocio(string status, string mensaje) {
            this.status = status;
            this.mensaje = mensaje;
            this.logs = new List<Log>();
        }
        public string status { get; set; }
        public string mensaje { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string registrosInsertados { get; set; }
        public string registrosNoInsertados { get; set; }
        public List<CampoLineaNegocio> campos { get; set; }
        public List<Log> logs { get; set; }
        public byte[]  archivo { get; set; }
        public object FileF { get; set; }
        public int lineasSuccess { get; set; }
        public int lineasFallidas { get; set; }
        public double porcentajeSucceed { get; set; }
        public double porcentajeFailed { get; set; }

    }
    public class Log
    {
        public Log(string tipoError, string mensaje, string sugerencia, string columna, string linea)
        {
            this.tipoError = tipoError;
            this.mensaje = mensaje;
            this.sugerencia = sugerencia;
            this.columna = columna;
            this.linea = linea;
        }
        public string tipoError { get; set; }
        public string mensaje { get; set; }
        public string sugerencia { get; set; }
        public string columna { get; set; }
        public string linea { get; set; }
    }
    public class RegistroExcel {
        public RegistroExcel() {
            this.valores = new List<string>();
        }
        public List<string> valores { get; set; }
    }
}
