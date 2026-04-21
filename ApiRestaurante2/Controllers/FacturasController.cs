using ApiRestaurante2.Servicios;
using Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio;
using System.Security.Claims;
using ApiRestaurante2.Servicios; 

namespace ApiRestaurante2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FacturasController : ControllerBase
    {
        private readonly FacturaNegocio _facturaNegocio;

        public FacturasController(FacturaNegocio facturaNegocio)
        {
            _facturaNegocio = facturaNegocio;
        }

        // ==========================================
        // PERMISOS
        // ==========================================

        private bool EsAdmin(out int idRol)
        {
            idRol = 0; var c = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            return int.TryParse(c, out idRol) && idRol == 1;
        }

        private bool EsCajero(out int idRol)
        {
            idRol = 0; var c = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            return int.TryParse(c, out idRol) && idRol == 3;
        }

        private bool TieneAccesoLectura(out int idRol)
        {
            idRol = 0; var c = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            return int.TryParse(c, out idRol) && (idRol == 1 || idRol == 3 || idRol == 2); // Admin, Cajero, Mesero
        }

        // ==========================================
        // ENDPOINTS
        // ==========================================

        [HttpGet("activas")]
        public IActionResult GetActivas()
        {
            if (!TieneAccesoLectura(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            try
            {
                var lista = _facturaNegocio.ListarActivas();
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (!TieneAccesoLectura(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            try
            {
                var factura = _facturaNegocio.ObtenerPorId(id);
                return Ok(new { exito = true, datos = factura });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        // GENERAR FACTURA 
        [HttpPost("generar")]
        public IActionResult Generar([FromBody] Factura factura)
        {
            // Solo Admin y Cajero pueden cobrar/generar factura
            if (!EsAdmin(out int r1) && !EsCajero(out int r2))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores y Cajeros pueden generar facturas." });

            if (factura == null) return BadRequest(new { exito = false, mensaje = "Datos inválidos." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";
            // Asegurar que el cajero sea quien firma la factura si no viene en el objeto
            if (factura.Id_cajero_cobrador == 0)
            {
                var claimId = User.FindFirst("IdUsuario")?.Value;
                if (!string.IsNullOrEmpty(claimId)) factura.Id_cajero_cobrador = int.Parse(claimId);
            }

            try
            {
                string resultado = _facturaNegocio.Generar(factura, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPatch("{id}/anular")]
        public IActionResult Anular(int id, [FromBody] string motivo)
        {
            if (!EsAdmin(out int r1)) // Solo Admin puede anular (política estricta)
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden anular facturas." });

            if (string.IsNullOrWhiteSpace(motivo)) return BadRequest(new { exito = false, mensaje = "Motivo obligatorio." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _facturaNegocio.Anular(id, motivo, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }
        // NUEVO ENDPOINT PARA DESCARGAR PDF
        [HttpGet("{id}/pdf")]
        public IActionResult DescargarPdf(int id)
        {
            if (!TieneAccesoLectura(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            try
            {
                // 1. Obtener los datos completos de la factura (con detalles)
                var factura = _facturaNegocio.ObtenerPorId(id);
                if (factura == null) return NotFound(new { exito = false, mensaje = "Factura no encontrada." });

                // 2. Generar el PDF
                var pdfService = new FacturaPdfService(); // O inyectarlo por constructor si prefieres
                byte[] pdfBytes = pdfService.GenerarFactura(factura);

                // 3. Retornar el archivo
                return File(pdfBytes, "application/pdf", $"Factura_{factura.Numero_factura_fiscal}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = "Error al generar PDF: " + ex.Message });
            }
        }
    }
}