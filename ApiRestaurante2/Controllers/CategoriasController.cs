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
    public class CategoriasController : ControllerBase
    {
        private readonly CategoriaNegocio _categoriaNegocio;

        public CategoriasController(CategoriaNegocio categoriaNegocio)
        {
            _categoriaNegocio = categoriaNegocio;
        }

        // ==========================================
        // LÓGICA DE PERMISOS 
        // ==========================================

        // Verifica si es Admin (Rol 1) -> Tiene acceso TOTAL
        private bool EsAdmin(out int idRol)
        {
            idRol = 0;
            var claimRole = User.FindFirst("role")?.Value;
            if (string.IsNullOrEmpty(claimRole)) claimRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(claimRole) && int.TryParse(claimRole, out idRol) && idRol == 1)
                return true;
            return false;
        }

        //  NUEVA FUNCIÓN: Verifica permiso de LECTURA (Admin, Mesero, Cajero) - EXCLUYE COCINA
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
                    mensajeError = "Acceso Denegado: El personal de Cocina no tiene acceso al módulo de Categorías.";
                    return false;
                }
            }
            mensajeError = "Acceso Denegado: Rol no reconocido o sin permisos.";
            return false;
        }

        // ==========================================
        // ENDPOINTS DE LECTURA Y BÚSQUEDA (Admin + Mesero + Cajero)
        // ==========================================

        [HttpGet("activos")]
        public IActionResult GetActivos()
        {
            if (!TienePermisoLectura(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var lista = _categoriaNegocio.ListarActivos();
                if (lista.Count == 0) return Ok(new { exito = true, mensaje = "No hay categorías activas.", datos = lista });
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
                var lista = _categoriaNegocio.ListarInactivos();
                if (lista.Count == 0) return Ok(new { exito = true, mensaje = "No hay categorías inactivas.", datos = lista });
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
                var categoria = _categoriaNegocio.ObtenerPorId(id);
                return Ok(new { exito = true, datos = categoria });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        //  ENDPOINT DE BÚSQUEDA
        [HttpGet("buscar")]
        public IActionResult Buscar([FromQuery] string nombre, [FromQuery] int? id)
        {
            if (!TienePermisoLectura(out int idRol, out string msg))
                return StatusCode(403, new { exito = false, mensaje = msg });

            try
            {
                var resultados = _categoriaNegocio.Buscar(nombre, id);
                string mensajeRespuesta = resultados.Count > 0 ? "Búsqueda exitosa." : "No se encontraron categorías con esos criterios.";
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
        public IActionResult Post([FromBody] Categoria categoria)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden crear categorías." });

            if (categoria == null || string.IsNullOrWhiteSpace(categoria.Nombre))
                return BadRequest(new { exito = false, mensaje = "Datos inválidos." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

            try
            {
                string resultado = _categoriaNegocio.Agregar(categoria, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Categoria categoria)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden modificar categorías." });

            if (categoria == null || id != categoria.Id_categoria)
                return BadRequest(new { exito = false, mensaje = "IDs no coinciden." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _categoriaNegocio.Actualizar(categoria, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden eliminar categorías." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _categoriaNegocio.EliminarLogico(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPatch("restaurar/{id}")]
        public IActionResult Restaurar(int id)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = "Acceso Denegado: Solo Administradores pueden restaurar categorías." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? "Sistema";

            try
            {
                string resultado = _categoriaNegocio.Restaurar(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }
    }
}