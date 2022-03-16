using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Modelos;
using Core.Servicios.Impl;
using Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LineaNegociosBackend.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private ConexionConfig conf;
        private IConfiguration _config;
        public AuthController(ConexionConfig conf, IConfiguration config)
        {
            this.conf = conf;
            _config = config;
        }
        [Authorize]
        [HttpGet("GetRoles/{idCliente}")]
        public async Task<ActionResult> ObtenerRoles(string idCliente)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.ObtenerRoles(idCliente);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpGet("GetRolInfo/{idRol}")]
        public async Task<ActionResult> ObtenerRolInfo(string idRol)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.ObtenerRolInfo(idRol);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("GetRolesBusqueda")]
        public async Task<ActionResult> BuscaRol([FromBody] RequestRolBusqueda req)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.BuscaRol(req);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("OperacionRol/{idCliente}/{operacion}")]
        public async Task<ActionResult> OperacionRol([FromBody] RolInfo rolInfo, string idCliente, string operacion)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.OperacionRol(rolInfo,idCliente, operacion);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("GetUsuariosBusqueda")]
        public async Task<ActionResult> BuscaUsuario([FromBody] RequestUsuarioBusqueda req)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.BuscaUsuario(req);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("OperacionUsuario/{idCliente}/{operacion}")]
        public async Task<ActionResult> OperacionUsuario([FromBody] UsuarioInfo userInfo, string idCliente, string operacion)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.OperacionUsuario(userInfo, idCliente, operacion);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [HttpPost("ValidaCodigo")]
        public async Task<ActionResult> ValidaCodigo([FromBody] ValidaCodigo info)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.ValidaCodigo(info);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [HttpPost("NewUser")]
        public async Task<ActionResult> NuevoUsuarioSistema([FromBody] UsuarioClienteNuevo user)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.NuevoUsuarioSistema(user);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("GetUserInfo/{idUsuario}")]
        public async Task<ActionResult> GetUserInfo(string idUsuario)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.GetUserInfo(idUsuario);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("AuthUser")]
        public async Task<IActionResult> AutenticaUsuario([FromBody] Autenticacion auth)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                IActionResult response = Unauthorized();
                var responseAuth = await servicio.AutenticaUsuario(auth);
                if (responseAuth.status == "OK") {
                    responseAuth.token = GenerateJSONWebToken();
                    // response = Ok(responseAuth);
                }
                return Ok(responseAuth);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        private string GenerateJSONWebToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddDays(1),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("GetHorarios")]
        public async Task<ActionResult> ObtenerHorarios([FromBody] PeticionHorario info)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.ObtenerHorarios(info);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [HttpPost("Schedule")]
        public async Task<ActionResult> AgendarLlamada([FromBody] PeticionLlamada info)
        {
            IAuth servicio = new Auth(this.conf);
            try
            {
                var response = await servicio.AgendarLlamada(info);
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
