using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class RolNegocio
    {
        private readonly RolDatos _rolDatos;

        public RolNegocio(RolDatos rolDatos)
        {
            _rolDatos = rolDatos;
        }

        // Validaciones básicas
        private string ValidarRol(Rol rol, bool esActualizacion = false)
        {
            if (string.IsNullOrWhiteSpace(rol.Nombre_rol))
                return "El nombre del rol es obligatorio.";

            if (rol.Nombre_rol.Length < 3 || rol.Nombre_rol.Length > 50)
                return "El nombre del rol debe tener entre 3 y 50 caracteres.";

            return string.Empty;
        }

        public List<Rol> ListarActivos()
        {
            try { return _rolDatos.ListarActivos(); }
            catch (Exception ex) { throw new Exception("Error al listar roles activos: " + ex.Message); }
        }

        public List<Rol> ListarInactivos()
        {
            try { return _rolDatos.ListarInactivos(); }
            catch (Exception ex) { throw new Exception("Error al listar roles inactivos: " + ex.Message); }
        }

        public Rol ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("ID inválido.");
            try
            {
                var rol = _rolDatos.ObtenerPorId(id);
                if (rol == null) throw new Exception("Rol no encontrado.");
                return rol;
            }
            catch (Exception ex) { throw new Exception("Error al obtener rol: " + ex.Message); }
        }

        public List<Rol> BuscarPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return new List<Rol>();
            try { return _rolDatos.BuscarPorNombre(nombre); }
            catch (Exception ex) { throw new Exception("Error al buscar rol: " + ex.Message); }
        }

        public string Agregar(Rol rol, string usuarioEjecutor)
        {
            string error = ValidarRol(rol);
            if (!string.IsNullOrEmpty(error)) return $"Error de validación: {error}";

            try
            {
              
                return _rolDatos.Agregar(rol, usuarioEjecutor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar rol: " + ex.Message);
            }
        }

        public string Actualizar(Rol rol, string usuarioEjecutor)
        {
            string error = ValidarRol(rol, true);
            if (!string.IsNullOrEmpty(error)) return $"Error de validación: {error}";
            if (rol.Id_rol <= 0) return "El ID del rol es obligatorio.";

            try
            {
                return _rolDatos.Actualizar(rol, usuarioEjecutor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar rol: " + ex.Message);
            }
        }

        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            try
            {
                return _rolDatos.EliminarLogico(id, usuarioEjecutor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar rol: " + ex.Message);
            }
        }

        public string Restaurar(int id, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            try
            {
                return _rolDatos.Restaurar(id, usuarioEjecutor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al restaurar rol: " + ex.Message);
            }
        }
    }
}