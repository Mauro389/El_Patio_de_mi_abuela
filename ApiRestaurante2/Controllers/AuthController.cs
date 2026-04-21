using Microsoft.AspNetCore.Mvc;
using Negocio;
using ApiRestaurante2.Models; 
using Entidades;

namespace ApiRestaurante2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtHelper _jwtHelper;
        private readonly UsuarioNegocio _usuarioNegocio;

        public AuthController(JwtHelper jwtHelper, UsuarioNegocio usuarioNegocio)
        {
            _jwtHelper = jwtHelper;
            _usuarioNegocio = usuarioNegocio;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel modelo)
        {
            // 1. Validación básica de entrada
            if (modelo == null || string.IsNullOrWhiteSpace(modelo.Username) || string.IsNullOrWhiteSpace(modelo.Password))
                return BadRequest(new { exito = false, mensaje = "Usuario y contraseña son obligatorios." });

            try
            {
                
                Usuario usuarioValido = _usuarioNegocio.ObtenerYValidarLogin(modelo.Username, modelo.Password);

                if (usuarioValido == null)
                {
                    return Unauthorized(new { exito = false, mensaje = "Credenciales incorrectas o usuario inactivo." });
                }

                // 3. Generar Token usando JwtHelper con los datos del usuario válido
                string token = _jwtHelper.GenerarToken(
                    idUsuario: usuarioValido.Id_usuario,
                    username: usuarioValido.Username,
                    idRol: usuarioValido.Id_rol,
                    nombreCompleto: usuarioValido.Nombre_completo
                );

                // 4. Retornar éxito con el token
                return Ok(new
                {
                    exito = true,
                    mensaje = "Login exitoso.",
                    token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = "Error interno del servidor.", detalle = ex.Message });
            }
        }
    }
}