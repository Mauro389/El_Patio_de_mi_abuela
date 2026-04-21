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
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioNegocio _usuarioNegocio;

        public UsuariosController(UsuarioNegocio usuarioNegocio)
        {
            _usuarioNegocio = usuarioNegocio;
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
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: El rol {idRol} no tiene permisos." });

            try { return Ok(new { exito = true, datos = _usuarioNegocio.ListarActivos() }); }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpGet("inactivos")]
        public IActionResult GetInactivos()
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: El rol {idRol} no tiene permisos." });

            try
            {
                var lista = _usuarioNegocio.ListarInactivos();

                
                if (lista == null || lista.Count == 0)
                {
                    return Ok(new
                    {
                        exito = true,
                        mensaje = "No se encontraron usuarios inactivos.",
                        datos = new List<Usuario>()
                    });
                }

                return Ok(new { exito = true, mensaje = "Usuarios inactivos encontrados.", datos = lista });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Post([FromBody] DatosNuevoUsuario datos)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: Solo Administradores." });

            if (datos == null || string.IsNullOrWhiteSpace(datos.Password))
                return BadRequest(new { exito = false, mensaje = "Datos inválidos o falta contraseña." });

            var nuevoUsuario = new Usuario
            {
                Id_rol = datos.Id_rol,
                Nombre_completo = datos.Nombre_completo,
                Username = datos.Username,
                Password = datos.Password,
                Activo = datos.Activo
            };

            
            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

            try
            {
                string resultado = _usuarioNegocio.Agregar(nuevoUsuario, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Usuario usuario)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: Solo Administradores." });

            if (usuario == null || id != usuario.Id_usuario)
                return BadRequest(new { exito = false, mensaje = "IDs no coinciden." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

            try
            {
                string resultado = _usuarioNegocio.Actualizar(usuario, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: Solo Administradores." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

            try
            {
                string resultado = _usuarioNegocio.EliminarLogico(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }

        [HttpPatch("restaurar/{id}")]
        public IActionResult Restaurar(int id)
        {
            if (!EsAdmin(out int idRol))
                return StatusCode(403, new { exito = false, mensaje = $"Acceso Denegado: Solo Administradores." });

            string usuarioEjecutor = User.FindFirst("Username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

            try
            {
                string resultado = _usuarioNegocio.Restaurar(id, usuarioEjecutor);
                return Ok(new { exito = true, mensaje = resultado });
            }
            catch (Exception ex) { return StatusCode(500, new { exito = false, mensaje = ex.Message }); }
        }
    }

    // Clase para la creación de usuarios
    public class DatosNuevoUsuario
    {
        public int Id_rol { get; set; }
        public string Nombre_completo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Activo { get; set; } = true;
    }
}