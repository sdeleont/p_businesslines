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

namespace LineaNegociosBackend.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class IAController : Controller
    {
        private ConexionConfig conf;
        private IConfiguration _config;
        public IAController(ConexionConfig conf, IConfiguration config)
        {
            this.conf = conf;
            _config = config;
        }
        [Authorize]
        [HttpPost("AplicarIA/{idLinea}"), DisableRequestSizeLimit]
        public async Task<ActionResult> AplicarInteligenciaArtificialBuscaRol(string idLinea)
        {
            IIA servicio = new IA(this.conf);
            try
            {
                RequestImageIA requestIA = new RequestImageIA();
                requestIA.Archivo = Request.Form.Files[0];
                requestIA.idLineaNegocio = idLinea;
                var response = await servicio.AplicarInteligenciaArtificial(requestIA);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
