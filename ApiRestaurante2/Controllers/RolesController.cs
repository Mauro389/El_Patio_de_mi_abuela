using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Entidades;
using Negocio;
using Microsoft.AspNetCore.Authorization;

namespace ApiRestaurante2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere token válido para CUALQUIER operación
    public class RolesController : ControllerBase
    {
        private readonly RolNegocio _rolNegocio;

        public RolesController(RolNegocio rolNegocio)
        {
            _rolNegocio = rolNegocio;
        }

       
        private bool EsAdmin(out int idRol)
        {
            idRol = 0;

           
            var claimRole = User.FindFirst("role")?.Value;

            
            if (string.IsNullOrEmpty(claimRole))
            {
                claimRole = User.FindFirst(ClaimTypes.Role)?.Value;
            }

            if (!string.IsNullOrEmpty(claimRole) && int.TryParse(claimRole, out idRol) && idRol == 1)
                return true;

            return false;
        }

      
        [HttpGet("activos")]
        public IActionResult GetActivos()
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: El rol {idRol} no tiene permisos para ver la lista de roles." });

            try { return Ok(new { exito = true, datos = _rolNegocio.ListarActivos() }); }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("inactivos")]
        public IActionResult GetInactivos()
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: El rol {idRol} no tiene permisos para ver roles inactivos." });

            try
            {
                var lista = _rolNegocio.ListarInactivos();

                
                if (lista == null || lista.Count == 0)
                {
                    return Ok(new
                    {
                        exito = true,
                        mensaje = "No se encontraron roles inactivos.",
                        datos = new List<Rol>() 
                    });
                }

                return Ok(new { exito = true, mensaje = "Roles inactivos encontrados.", datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        
        [HttpPost]
        public IActionResult Post([FromBody] Rol rol)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: El rol {idRol} no tiene permisos. Solo Administradores." });

            if (rol == null) return BadRequest(new { exito = false, mensaje = "Datos inválidos." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

            try
            {
                string resultado = _rolNegocio.Agregar(rol, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Rol rol)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: Solo Administradores pueden modificar roles." });

            if (rol == null || id != rol.Id_rol) return BadRequest(new { exito = false, mensaje = "IDs no coinciden." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

            try
            {
                string resultado = _rolNegocio.Actualizar(rol, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: Solo Administradores pueden eliminar roles." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

            try
            {
                string resultado = _rolNegocio.EliminarLogico(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPatch("restaurar/{id}")]
        public IActionResult Restaurar(int id)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: Solo Administradores pueden restaurar roles." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

            try
            {
                string resultado = _rolNegocio.Restaurar(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }
    }
}