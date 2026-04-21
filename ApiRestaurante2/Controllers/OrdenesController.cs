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
    public class OrdenesController : ControllerBase
    {
        private readonly OrdenNegocio _ordenNegocio;

        public OrdenesController(OrdenNegocio ordenNegocio)
        {
            _ordenNegocio = ordenNegocio;
        }

        // ==========================================
        // LÓGICA DE PERMISOS ESPECÍFICA PARA ÓRDENES
        // ==========================================

        private bool EsAdmin(out int idRol)
        {
            idRol = 0;
            var claimRole = User.FindFirst("role")?.Value;
            if (string.IsNullOrEmpty(claimRole)) claimRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(claimRole) && int.TryParse(claimRole, out idRol) && idRol == 1)
                return true;
            return false;
        }

        // Mesero y Cajero pueden gestionar órdenes (Crear, Ver, Cerrar)
        private bool EsGestorOrdenes(out int idRol)
        {
            idRol = 0;
            var claimRole = User.FindFirst("role")?.Value;
            if (string.IsNullOrEmpty(claimRole)) claimRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(claimRole) && int.TryParse(claimRole, out idRol))
            {
                if (idRol == 2 || idRol == 3) return true; // Mesero o Cajero
            }
            return false;
        }

        // Cocina solo puede VER y CAMBIAR ESTADO A LISTA
        private bool EsCocina(out int idRol)
        {
            idRol = 0;
            var claimRole = User.FindFirst("role")?.Value;
            if (string.IsNullOrEmpty(claimRole)) claimRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(claimRole) && int.TryParse(claimRole, out idRol) && idRol == 4)
                return true;
            return false;
        }

        // ==========================================
        // ENDPOINTS DE LECTURA
        // ==========================================

        [HttpGet("activas")]
        public IActionResult GetActivas()
        {
            // Admin, Gestores (Mesero/Cajero) y Cocina pueden ver órdenes activas
            if (!EsAdmin(out int r1) && !EsGestorOrdenes(out int r2) && !EsCocina(out int r3))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            try
            {
                var lista = _ordenNegocio.ListarActivas();
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("historial")]
        public IActionResult GetHistorial()
        {
            // Solo Admin y Gestores ven el historial (Cocina no necesita ver órdenes cerradas antiguas)
            if (!EsAdmin(out int r1) && !EsGestorOrdenes(out int r2))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Admin y personal de servicio ven el historial." });

            try
            {
                var lista = _ordenNegocio.ListarHistorial();
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (!EsAdmin(out int r1) && !EsGestorOrdenes(out int r2) && !EsCocina(out int r3))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            try
            {
                var orden = _ordenNegocio.ObtenerPorId(id);
                return Ok(new { exito = true, datos = orden });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("buscar")]
        public IActionResult Buscar([FromQuery] int? id_mesa, [FromQuery] string estado)
        {
            if (!EsAdmin(out int r1) && !EsGestorOrdenes(out int r2) && !EsCocina(out int r3))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            try
            {
                var resultados = _ordenNegocio.Buscar(id_mesa, estado);
                return Ok(new { exito = true, datos = resultados });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        // ==========================================
        // ENDPOINTS DE ESCRITURA
        // ==========================================

        [HttpPost]
        public IActionResult Post([FromBody] Orden orden)
        {
            // Solo Admin, Mesero y Cajero pueden CREAR órdenes. Cocina NO.
            if (!EsAdmin(out int r1) && !EsGestorOrdenes(out int r2))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores, Meseros y Cajeros pueden abrir órdenes." });

            if (orden == null) return BadRequest(new { exito = false, mensaje = "Datos inválidos." });

            // Obtener ID del mesero desde el Token 
            if (orden.Id_mesero == 0)
            {
                var claimId = User.FindFirst("IdUsuario")?.Value;
                if (!string.IsNullOrEmpty(claimId)) orden.Id_mesero = int.Parse(claimId);
            }

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _ordenNegocio.Agregar(orden, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPatch("{id}/estado")]
        public IActionResult CambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            // Lógica Especial:
            // - Admin/Gestores: Pueden poner cualquier estado (Abierta, Cerrada, Cancelada).
            // - Cocina: Solo puede poner "Lista" (o "EnProceso").

            if (!EsAdmin(out int r1) && !EsGestorOrdenes(out int r2) && !EsCocina(out int r3))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado." });

            // Restricción para Cocina
            if (EsCocina(out _))
            {
                if (nuevoEstado != "Lista" && nuevoEstado != "EnProceso")
                    return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: La Cocina solo puede marcar la orden como 'Lista' o 'EnProceso'." });
            }

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _ordenNegocio.ActualizarEstado(id, nuevoEstado, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // Solo Admin puede eliminar físicamente (lógico) una orden
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden eliminar órdenes." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _ordenNegocio.EliminarLogico(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }
    }
}