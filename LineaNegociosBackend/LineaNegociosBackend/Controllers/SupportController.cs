using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Modelos;
using Core.Servicios.Impl;
using Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LineaNegociosBackend.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class SupportController : Controller
    {
        private ConexionConfig conf;
        private IConfiguration _config;
        public SupportController(ConexionConfig conf, IConfiguration config)
        {
            this.conf = conf;
            _config = config;
        }
        [Authorize]
        [HttpPost("NewTicket"), DisableRequestSizeLimit]
        public async Task<ActionResult> CrearTicket()
        {
            ISupport servicio = new Support(this.conf);
            try
            {
                InformacionLineaNegocio archivoL = new InformacionLineaNegocio();
                archivoL.Archivo = Request.Form.Files[0];
                string extension = System.IO.Path.GetExtension(archivoL.Archivo.FileName);
                //string json = Request.Form.Keys.ElementAt(0).value;
                var dict = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                string json = dict["objeto"];
                dynamic jsonResponse = JsonConvert.DeserializeObject(json);
                dynamic api = JObject.Parse(json);
                PeticionSoporte peticion = new PeticionSoporte(api.idClienteSistema.Value, api.idUsuario.Value, api.mensaje.Value, api.tipo.Value);
                for (int i = 0; i < Request.Form.Files.Count; i++) {
                    peticion.Archivos.Add(Request.Form.Files[i]);
                }
                ResponseSupport response = await servicio.NuevoTicket(peticion);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [HttpPost("GetMensajes/{idUsuario}")]
        public async Task<ActionResult> GetMensajes(string idUsuario)
        {
            ISupport servicio = new Support(this.conf);
            try
            {
                var response = await servicio.GetMensajes(idUsuario);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("MarcarLeido/{idMensaje}")]
        public async Task<ActionResult> SetMensajeLiedo(string idMensaje)
        {
            ISupport servicio = new Support(this.conf);
            try
            {
                var response = await servicio.SetMensajeLiedo(idMensaje);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("GetInfoMensaje/{idMensaje}")]
        public async Task<ActionResult> GetInfoMensaje(string idMensaje)
        {
            ISupport servicio = new Support(this.conf);
            try
            {
                var response = await servicio.GetInfoMensaje(idMensaje);
                List<ObjetoImagenDatos> lista = new List<ObjetoImagenDatos>();
                foreach (ObjetoImagenDatos objeto in response.imagenes)
                {
                    ObjetoImagenDatos nuevo = new ObjetoImagenDatos();
                    nuevo.tipo = objeto.tipo;
                    nuevo.guid = objeto.guid;
                    nuevo.imagenFile = File(objeto.bytes, objeto.tipo, "Imagen.jpg");
                    lista.Add(nuevo);
                }
                response.imagenes = lista;
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
