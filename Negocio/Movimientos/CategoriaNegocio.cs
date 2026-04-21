using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class CategoriaNegocio
    {
        private readonly CategoriaDatos _categoriaDatos;

        public CategoriaNegocio(CategoriaDatos categoriaDatos)
        {
            _categoriaDatos = categoriaDatos;
        }

        private string ValidarDatos(Categoria categoria, bool esActualizacion = false)
        {
            if (categoria == null) return "El objeto Categoría no puede ser nulo.";
            if (string.IsNullOrWhiteSpace(categoria.Nombre)) return "El nombre de la categoría es obligatorio.";
            return string.Empty;
        }

        public List<Categoria> ListarActivos()
        {
            try { return _categoriaDatos.ListarActivos() ?? new List<Categoria>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public List<Categoria> ListarInactivos()
        {
            try { return _categoriaDatos.ListarInactivos() ?? new List<Categoria>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public Categoria ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("ID inválido.");
            var cat = _categoriaDatos.ObtenerPorId(id);
            if (cat == null) throw new Exception("Categoría no encontrada.");
            return cat;
        }

        public List<Categoria> Buscar(string nombre, int? id)
        {
            try { return _categoriaDatos.Buscar(nombre, id) ?? new List<Categoria>(); }
            catch (Exception ex) { throw new Exception("Error al buscar: " + ex.Message); }
        }

        public string Agregar(Categoria categoria, string usuarioEjecutor)
        {
            string error = ValidarDatos(categoria);
            if (!string.IsNullOrEmpty(error)) return $"Error validación: {error}";
            try { return _categoriaDatos.Agregar(categoria, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al crear: " + ex.Message); }
        }

        public string Actualizar(Categoria categoria, string usuarioEjecutor)
        {
            string error = ValidarDatos(categoria, true);
            if (!string.IsNullOrEmpty(error)) return $"Error validación: {error}";
            if (categoria.Id_categoria <= 0) return "ID de categoría inválido.";
            try { return _categoriaDatos.Actualizar(categoria, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al actualizar: " + ex.Message); }
        }

        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            try { return _categoriaDatos.EliminarLogico(id, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al eliminar: " + ex.Message); }
        }

        public string Restaurar(int id, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            try { return _categoriaDatos.Restaurar(id, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al restaurar: " + ex.Message); }
        }
    }
}