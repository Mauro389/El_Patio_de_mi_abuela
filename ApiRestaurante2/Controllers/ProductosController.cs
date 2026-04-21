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
    public class ProductosController : ControllerBase
    {
        private readonly ProductoNegocio _productoNegocio;

        public ProductosController(ProductoNegocio productoNegocio)
        {
            _productoNegocio = productoNegocio;
        }

        // ==========================================
        // LÓGICA DE PERMISOS 
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

        // ✅  Permite Lectura a Admin, Mesero y Cajero. BLOQUEA Cocina (4).
        private bool TienePermisoLectura(out int idRol, out string mensajeError)
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
                    mensajeError = "Acceso Denegado: El personal de Cocina no tiene acceso al módulo de Productos.";
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
            if (!TienePermisoLectura(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var lista = _productoNegocio.ListarActivos();
                if (lista.Count == 0) return Ok(new { exito = true, mensaje = "No hay productos activos.", datos = lista });
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("inactivos")]
        public IActionResult GetInactivos()
        {
            if (!TienePermisoLectura(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var lista = _productoNegocio.ListarInactivos();
                if (lista.Count == 0) return Ok(new { exito = true, mensaje = "No hay productos inactivos.", datos = lista });
                return Ok(new { exito = true, datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (!TienePermisoLectura(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var producto = _productoNegocio.ObtenerPorId(id);
                return Ok(new { exito = true, datos = producto });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        //  BÚSQUEDA AVANZADA
        [HttpGet("buscar")]
        public IActionResult Buscar([FromQuery] string nombre, [FromQuery] int? id, [FromQuery] int? id_categoria)
        {
            if (!TienePermisoLectura(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var resultados = _productoNegocio.Buscar(nombre, id, id_categoria);
                string mensajeRespuesta = resultados.Count > 0 ? "Búsqueda exitosa." : "No se encontraron productos.";
                return Ok(new { exito = true, mensaje = mensajeRespuesta, datos = resultados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = ex.Message });
            }
        }

        // ==========================================
        // ENDPOINTS DE ESCRITURA (SOLO ADMIN)
        // ==========================================

        [HttpPost]
        public IActionResult Post([FromBody] Producto producto)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden crear productos." });

            if (producto == null) return BadRequest(new { exito = false, mensaje = "Datos inválidos." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _productoNegocio.Agregar(producto, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Producto producto)
        {
            // Nota: Aquí solo Admin puede modificar productos. 
            
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden modificar." });

            if (producto == null || id != producto.Id_producto)
                return BadRequest(new { exito = false, mensaje = "IDs no coinciden." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _productoNegocio.Actualizar(producto, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden eliminar." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _productoNegocio.EliminarLogico(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPatch("restaurar/{id}")]
        public IActionResult Restaurar(int id)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden restaurar." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _productoNegocio.Restaurar(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }
    }
}