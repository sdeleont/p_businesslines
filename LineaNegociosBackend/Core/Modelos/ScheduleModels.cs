using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class ScheduleModels
    {
    }
    public class RespuestaDisponibilidad { 
        public List<Dia> dias { get; set;  }
        public string mes { get; set; }
    }
    public class Dia
    {
        public string nombre { get; set; }
        public string dia { get; set; }
        public string esHoy { get; set; }
        public string diaDisponible { get; set; }
        public string mes { get; set; }
        public string año { get; set; }
        public List<HorarioDia> horarios { get; set; }
    }
    public class HorarioDia {
        public string hora { get; set; }
        public string disponibilidad { get; set; }
    }
    public class PeticionHorario {
        public string diaPos { get; set; }
        public string mesPos { get; set; }
        public string añoPos { get; set; }
    }
    public class Fecha { 
        public string UTCmenosSeis { get; set; }
    }
    public class DiaTexto
    {
        public string diasemana { get; set; }
        public string dia { get; set; }
        public string mes { get; set; }
        public string año { get; set; }
    }
    public class Disponibilidad {
        public string dia { get; set; }
        public string hora { get; set; }
    }
    public class RestriccionHorario {
        public string fecha { get; set; }
        public string hora { get; set; }
        public string dia { get; set; }
        public string sistema { get; set; }
        public string tipo { get; set; }
    }
    public class RestriccionReuniones {
        public string fecha { get; set; }
        public string hora { get; set; }
    }
    public class PeticionLlamada {
        public string fecha { get; set; }
        public string hora { get; set; }
        public string sistema { get; set; }
        public string nombrecompleto { get; set; }
        public string email { get; set; }
        public string empresa { get; set; }
        public string pais { get; set; }
        public string numerowhatsapp { get; set; }
        public string descripcion { get; set; }
    }
    public class ResponseLlamada {
        public string status { get; set; }
        public string mensaje { get; set; }
    }
}
