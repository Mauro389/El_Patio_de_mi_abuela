using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Entidades;
using Negocio;
using Microsoft.AspNetCore.Authorization;

namespace ApiRestaurante2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BitacoraController : ControllerBase
    {
        private readonly BitacoraNegocio _bitacoraNegocio;

        public BitacoraController(BitacoraNegocio bitacoraNegocio)
        {
            _bitacoraNegocio = bitacoraNegocio;
        }

        private bool EsAdmin(out int idRol)
        {
            idRol = 0;
            var claimRole = User.FindFirst("role")?.Value;
            if (string.IsNullOrEmpty(claimRole)) claimRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(claimRole) && int.TryParse(claimRole, out idRol) && idRol == 1)
                return true;
            return false;
        }

        // Ver TODO el historial (SOLO ADMIN)
        [HttpGet("todo")]
        public IActionResult GetTodo()
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden ver la bitácora." });

            try
            {
                var lista = _bitacoraNegocio.ListarTodo();
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        // Buscar por Tabla afectada (SOLO ADMIN)
        [HttpGet("tabla/{tabla}")]
        public IActionResult GetByTabla(string tabla)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            try
            {
                var lista = _bitacoraNegocio.BuscarPorTabla(tabla);
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

       
        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] Bitacora bitacora)
        {
            

            if (bitacora == null) return BadRequest(new { exito = false, mensaje = "Datos inválidos." });

            
            if (string.IsNullOrWhiteSpace(bitacora.Ip_origen))
            {
                bitacora.Ip_origen = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            }

          
            if (bitacora.Fecha_evento == default)
                bitacora.Fecha_evento = DateTime.Now;

            try
            {
                string resultado = _bitacoraNegocio.Registrar(bitacora);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }
    }
}