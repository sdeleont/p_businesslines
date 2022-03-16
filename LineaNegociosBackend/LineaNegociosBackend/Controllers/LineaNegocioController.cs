using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Modelos;
using Core.Servicios.Impl;
using Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LineaNegociosBackend.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LineaNegocioController : Controller
    {
        private ConexionConfig conf;
        public LineaNegocioController(ConexionConfig conf)
        {
            this.conf = conf;
        }
        [Authorize]
        [HttpPost("Analiza"), DisableRequestSizeLimit]
        public async Task<ActionResult> Analiza()
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                InformacionLineaNegocio archivoL = new InformacionLineaNegocio();
                archivoL.Archivo = Request.Form.Files[0];
                string extension = System.IO.Path.GetExtension(archivoL.Archivo.FileName);
                ResponseAnalisisFile response = null;
                if (extension.Trim().ToLower().Equals(".xlsx"))
                {
                    response = await servicio.Analiza(archivoL);
                }
                else
                {
                    response = new ResponseAnalisisFile("Error","Por favor, provea un archivo de Excel con extensión .xlsx");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpGet("GetTiposNegocio")]
        public async Task<ActionResult> ObtenerTiposNegocio()
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.ObtenerTiposNegocio();
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpGet("GetTiposDato")]
        public async Task<ActionResult> ObtenerTiposDatos()
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.ObtenerTiposDato();
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpGet("GetAllBusinessline/{idCliente}")]
        public async Task<ActionResult> ObtenerLineasNegocio(string idCliente)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.ObtenerLineasNegocio(idCliente);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("GetDataBusinessline/{idLinea}/{inicioPuntero}/{finPuntero}")]
        public async Task<ActionResult> ObtenerLineaNegocioDatos([FromBody] BusquedaData busqueda, string idLinea, string inicioPuntero, string finPuntero)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.ObtenerLineaNegocioDatos(busqueda, idLinea, inicioPuntero, finPuntero);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("GetDataRegistroID/{idLinea}/{idRegistro}")]
        public async Task<ActionResult> GetDataRegistroID(string idLinea, string idRegistro)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.GetDataRegistroID(idLinea, idRegistro);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("NuevaLineaNegocio"), DisableRequestSizeLimit]
        public async Task<ActionResult> CrearLinea()
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
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
                LineaNegocioModel lineaNegocio = new LineaNegocioModel(api.idClienteSistema.Value, api.nombre.Value, api.descripcion.Value, api.formaMuestra.Value, api.idTipoNegocio.Value);
                lineaNegocio.Archivo = Request.Form.Files[0];
                var campos = api.campos;
                for (int i = 0; i < campos.Count; i++) {
                    string a = campos[0].nombreCampo;
                    bool s = (campos[i].obligatorio == "true" ? true : false);
                    CampoLineaNegocio campo = new CampoLineaNegocio(campos[i].idTipodato.Value, campos[i].nombreCampo.Value, campos[i].descripcionCampo.Value, (campos[i].obligatorio=="true"? true: false), (campos[i].esCampoAgrupador == "true" ? true : false),"false");
                    lineaNegocio.campos.Add(campo);
                }
                

                ResponseLineaNegocio response = null;
                if (extension.Trim().ToLower().Equals(".xlsx"))
                {
                    response = await servicio.NuevaLinea(lineaNegocio);
                    /*
                    using (var ms = new MemoryStream(response.archivo))
                    {
                        //archivoL.Archivo.CopyTo(ms);
                        //response.archivo = File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Data.xlsx");
                        //response.archivo = ms.ToArray();
                        
                        response.FileF = File(response.archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Data.xlsx");
                        //return File(response.archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Data.xlsx");
                    }
                    */
                    if (response.status.Equals("OK")) {
                        response.FileF = File(response.archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Data.xlsx");
                    }
                    return Ok(response);
                }
                else
                {
                    response = new ResponseLineaNegocio("Error", "Por favor, provea un archivo de Excel con extensión .xlsx");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("OperacionLineaNegocio")]
        public async Task<ActionResult> OperacionLineaNegocio([FromBody] OperacionLineaNegocio operacion)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.OperacionLineaNegocio(operacion);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("OperacionSobreRegistro")]
        public async Task<ActionResult> OperacionSobreRegistro([FromBody] OperacionData operacion)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.OperacionSobreRegistro(operacion);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("SetImage/{idLinea}/{valorIdLlavePrimaria}")]
        public async Task<ActionResult> SetImage(string idLinea, string valorIdLlavePrimaria)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                RequestConImagen req = new RequestConImagen();
                req.idLinea = idLinea;
                req.valorLlave = valorIdLlavePrimaria;
                req.Archivo = Request.Form.Files[0];
                var response = await servicio.SetImage(req);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("DeleteImage")]
        public async Task<ActionResult> DelImage([FromBody]RequestOpImagen req)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.DelImage(req);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [HttpPost("GetImagesID/{idLinea}/{valorLLave}")]
        public async Task<ActionResult> GetImagesID(string idLinea, string valorLLave)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.GetImagesID(idLinea, valorLLave);
                List<ObjetoImagenDatos> lista = new List<ObjetoImagenDatos>();
                foreach (ObjetoImagenDatos objeto in response.imagenes) {
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
        [Authorize]
        [HttpPost("GetDataColumnaG1/{idLinea}/{columna}/{numDatos}")]
        public async Task<ActionResult> GetDataColumnaG1(string idLinea, string columna, int numDatos)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.GetDataColumnaG1(idLinea, columna, numDatos);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("GetDataGrafica1")]
        public async Task<ActionResult> GetDataGrafica1([FromBody] RequestDataGrafica1 request)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.GetDataGrafica1(request);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        // metodos para la parte de configuraciones del cliente
        [Authorize]
        [HttpPost("SetImgCliente/{idCliente}/{tipo}")]
        public async Task<ActionResult> SetImgCliente(string idCliente, string tipo)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                RequestConImagen req = new RequestConImagen();
                req.Archivo = Request.Form.Files[0];
                var response = await servicio.SetImgCliente(req, idCliente, tipo);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        
        [HttpPost("GetImagesConfCliente/{idCliente}")]
        public async Task<ActionResult> GetImagesConfCliente(string idCliente)
        {
            ILineaNegocio servicio = new LineaNegocio(this.conf);
            try
            {
                var response = await servicio.GetImagesConfCliente(idCliente);
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
