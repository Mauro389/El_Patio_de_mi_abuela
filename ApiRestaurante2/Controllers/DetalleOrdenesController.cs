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
    public class DetalleOrdenesController : ControllerBase
    {
        private readonly DetalleOrdenNegocio _detalleNegocio;

        public DetalleOrdenesController(DetalleOrdenNegocio detalleNegocio)
        {
            _detalleNegocio = detalleNegocio;
        }

        // ==========================================
        // PERMISOS
        // ==========================================

        private bool EsAdmin(out int idRol)
        { 
            idRol = 0; var c = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            return int.TryParse(c, out idRol) && idRol == 1;
        }

        private bool EsGestor(out int idRol)
        { /* Mesero/Cajero */
            idRol = 0; var c = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            return int.TryParse(c, out idRol) && (idRol == 2 || idRol == 3);
        }

        private bool EsCocina(out int idRol)
        { /* Solo Cocina */
            idRol = 0; var c = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            return int.TryParse(c, out idRol) && idRol == 4;
        }

        // ==========================================
        // ENDPOINTS
        // ==========================================

        // Ver todos los items de una orden específica
        [HttpGet("orden/{id_orden}")]
        public IActionResult GetByOrden(int id_orden)
        {
            // Todos los roles involucrados pueden ver el detalle de UNA orden específica
            if (!EsAdmin(out int r1) && !EsGestor(out int r2) && !EsCocina(out int r3))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            try
            {
                var lista = _detalleNegocio.ObtenerPorOrden(id_orden);
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        // Agregar producto a la orden (Solo Admin, Mesero, Cajero)
        [HttpPost]
        public IActionResult Post([FromBody] DetalleOrden detalle)
        {
            if (!EsAdmin(out int r1) && !EsGestor(out int r2))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo personal de servicio puede agregar items." });

            if (detalle == null) return BadRequest(new { exito = false, mensaje = "Datos inválidos." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _detalleNegocio.Agregar(detalle, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        // Cambiar estado del item (Pendiente -> Listo)
        // Cocina: Solo puede poner "Listo".
        // Mesero/Admin: Pueden poner cualquier estado (Cancelado, EnProceso, etc.)
        [HttpPatch("{id_detalle}/estado")]
        public IActionResult CambiarEstado(int id_detalle, [FromBody] string nuevoEstado)
        {
            if (!EsAdmin(out int r1) && !EsGestor(out int r2) && !EsCocina(out int r3))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            // Restricción fuerte para Cocina
            if (EsCocina(out _))
            {
                if (nuevoEstado != "Listo" && nuevoEstado != "EnProceso")
                    return StatusCode(403, new { exito = false, mensaje = "La Cocina solo puede cambiar el estado a 'Listo' o 'EnProceso'." });
            }

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _detalleNegocio.ActualizarEstadoItem(id_detalle, nuevoEstado, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        // Eliminar/Cancelar un item de la orden (Solo Admin, Mesero, Cajero)
        [HttpDelete("{id_detalle}")]
        public IActionResult Delete(int id_detalle)
        {
            if (!EsAdmin(out int r1) && !EsGestor(out int r2))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: La Cocina no puede eliminar items." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _detalleNegocio.EliminarLogico(id_detalle, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }
    }
}