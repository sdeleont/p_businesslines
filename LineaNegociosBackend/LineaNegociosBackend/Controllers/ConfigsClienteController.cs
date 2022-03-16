using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LineaNegociosBackend.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ConfigsClienteController : Controller
    {
        private ConexionConfig conf;
        public ConfigsClienteController(ConexionConfig conf)
        {
            this.conf = conf;
        }

    }
}
