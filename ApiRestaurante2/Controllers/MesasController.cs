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
    public class MesasController : ControllerBase
    {
        private readonly MesaNegocio _mesaNegocio;

        public MesasController(MesaNegocio mesaNegocio)
        {
            _mesaNegocio = mesaNegocio;
        }

        // ==========================================
        // LÓGICA DE PERMISOS PERSONALIZADA
        // ==========================================

        // Verifica si es Admin (Rol 1) -> Acceso Total (Crear, Borrar, Restaurar)
        private bool EsAdmin(out int idRol)
        {
            idRol = 0;
            var claimRole = User.FindFirst("role")?.Value;
            if (string.IsNullOrEmpty(claimRole)) claimRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(claimRole) && int.TryParse(claimRole, out idRol) && idRol == 1)
                return true;
            return false;
        }

        // Verifica si es Operativo (Mesero 2 o Cajero 3) -> Pueden Cambiar Estado (PUT)
        private bool EsOperativo(out int idRol)
        {
            idRol = 0;
            var claimRole = User.FindFirst("role")?.Value;
            if (string.IsNullOrEmpty(claimRole)) claimRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(claimRole) && int.TryParse(claimRole, out idRol))
            {
                if (idRol == 2 || idRol == 3) return true;
            }
            return false;
        }

        //  FUNCIÓN UNIFICADA: Permite Lectura/Búsqueda a Admin, Mesero, Cajero. BLOQUEA Cocina (4).
        private bool TieneAcceso(out int idRol, out string mensajeError)
        {
            idRol = 0;
            mensajeError = "";

            var claimRole = User.FindFirst("role")?.Value;
            if (string.IsNullOrEmpty(claimRole)) claimRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(claimRole) && int.TryParse(claimRole, out idRol))
            {
                // Permitir: Admin (1), Mesero (2), Cajero (3)
                if (idRol == 1 || idRol == 2 || idRol == 3)
                    return true;

                // Bloquear explícitamente Cocina (4)
                if (idRol == 4)
                {
                    mensajeError = "Acceso Denegado: El personal de Cocina no tiene permisos en el módulo de Mesas.";
                    return false;
                }
            }
            mensajeError = "Acceso Denegado: Rol no reconocido o sin permisos.";
            return false;
        }

        // ==========================================
        // ENDPOINTS DE LECTURA Y BÚSQUEDA
        // ==========================================

        [HttpGet("activos")]
        public IActionResult GetActivos()
        {
            if (!TieneAcceso(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var lista = _mesaNegocio.ListarActivos();
                if (lista.Count == 0) return Ok(new { exito = true, mensaje = "No hay mesas activas.", datos = lista });
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("inactivos")]
        public IActionResult GetInactivos()
        {
            if (!TieneAcceso(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var lista = _mesaNegocio.ListarInactivos();
                if (lista.Count == 0) return Ok(new { exito = true, mensaje = "No hay mesas inactivas.", datos = lista });
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (!TieneAcceso(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var mesa = _mesaNegocio.ObtenerPorId(id);
                return Ok(new { exito = true, datos = mesa });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("buscar")]
        public IActionResult Buscar([FromQuery] int? numero, [FromQuery] int? id)
        {
            if (!TieneAcceso(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var resultados = _mesaNegocio.Buscar(numero, id);
                string mensajeRespuesta = resultados.Count > 0 ? "Búsqueda exitosa." : "No se encontraron mesas.";
                return Ok(new { exito = true, mensaje = mensajeRespuesta, datos = resultados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = ex.Message });
            }
        }

        // ==========================================
        // ENDPOINTS DE ESCRITURA
        // ==========================================

        [HttpPost]
        public IActionResult Post([FromBody] Mesa mesa)
        {
            // Solo Admin puede CREAR mesas nuevas
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden crear mesas." });

            if (mesa == null) return BadRequest(new { exito = false, mensaje = "Datos inválidos." });
            if (string.IsNullOrWhiteSpace(mesa.Estado)) mesa.Estado = "Disponible";

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _mesaNegocio.Agregar(mesa, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Mesa mesa)
        {
            // Admin y Operativos (Mesero/Cajero) pueden ACTUALIZAR (ej. cambiar estado a Ocupada)
            if (!EsAdmin(out int idRolAdmin) && !EsOperativo(out int idRolOp))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Admin, Meseros y Cajeros pueden modificar mesas." });

            if (mesa == null || id != mesa.Id_mesa)
                return BadRequest(new { exito = false, mensaje = "IDs no coinciden." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _mesaNegocio.Actualizar(mesa, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // Solo Admin puede ELIMINAR
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden eliminar mesas." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _mesaNegocio.EliminarLogico(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPatch("restaurar/{id}")]
        public IActionResult Restaurar(int id)
        {
            // Solo Admin puede RESTAURAR
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden restaurar mesas." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _mesaNegocio.Restaurar(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }
    }
}